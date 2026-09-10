// <copyright file="AnalyzerConventionTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Conventions the analyzer and code fix sources must follow, checked against the sources themselves
/// rather than against behaviour, because a violation costs a silently missing diagnostic that no
/// individual analyzer test would think to look for.
/// </summary>
public class AnalyzerConventionTests
{
    /// <summary>
    /// Sources every analyzer is run over. Two kinds are represented deliberately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first kind is code that does not compile. An analyzer spends most of its life running over
    /// exactly that — a file mid-keystroke — where a symbol may be missing, a type may not bind, and a
    /// constant may be of a type the property could never hold. An analyzer that throws there is
    /// reported as AD0001 and is silently switched off for the whole file, which is worse than any
    /// wrong diagnostic it might have produced.
    /// </para>
    /// <para>
    /// The second kind compiles but is written in a shape the analyzers historically mishandled:
    /// rebinding a tracked driver inside a branch, a blocking call spelled as a conditional access, a
    /// handler passed as a delegate's Invoke, a this-qualified receiver. Each of these once produced an
    /// AD0001, so each stays in the corpus to keep that from recurring.
    /// </para>
    /// </remarks>
    private static readonly (string Name, string Source)[] AnalyzerCorpus =
    [
        ("unresolved types and members", """
            using WebDriverBiDi;

            class C
            {
                void M(BiDiDriver driver)
                {
                    NotAType thing = driver.NoSuchMember;
                    driver.Log.OnEntryAdded.AddObserver(e => thing.AlsoMissing());
                }
            }
            """),
        ("half-written member access", """
            using WebDriverBiDi;

            class C
            {
                void M(BiDiDriver driver)
                {
                    driver.
                }
            }
            """),
        ("constant of a type the property cannot hold", """
            using WebDriverBiDi.BrowsingContext;
            using WebDriverBiDi.Script;

            class C
            {
                void M()
                {
                    ImageFormat format = new ImageFormat { Quality = "high" };
                    ImageFormat other = new ImageFormat { Quality = true };
                    SerializationOptions options = new SerializationOptions { MaxDomDepth = "deep" };
                }
            }
            """),
        ("driver rebound from a factory inside a branch", """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            class C
            {
                static BiDiDriver Create() => new BiDiDriver();

                static async Task M(bool flag, int which, string url)
                {
                    BiDiDriver driver = new BiDiDriver();
                    if (flag)
                    {
                        driver = Create();
                    }

                    try
                    {
                        driver = Create();
                    }
                    catch (Exception)
                    {
                        await driver.Session.StatusAsync();
                    }

                    switch (which)
                    {
                        case 1:
                            driver = Create();
                            break;
                        default:
                            break;
                    }

                    await driver.StartAsync(url);
                    await driver.Session.StatusAsync();
                }
            }
            """),
        ("blocking calls in handlers, in several spellings", """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;
            using static System.Threading.Thread;

            delegate void LogHandler(EntryAddedEventArgs args);

            class C
            {
                void M(BiDiDriver driver, Task pending, LogHandler handler)
                {
                    driver.Log.OnEntryAdded.AddObserver(e => { pending?.Wait(); });
                    driver.Log.OnEntryAdded.AddObserver(e => Sleep(100));
                    driver.Log.OnEntryAdded.AddObserver(handler.Invoke);
                    driver.Log.OnEntryAdded.AddObserver(e => Task.Run(() => Sleep(10)));
                }
            }
            """),
        ("receivers that do not root in a plain name", """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            class C
            {
                private readonly BiDiDriver driver = new BiDiDriver();

                static BiDiDriver Create() => new BiDiDriver();

                async Task M()
                {
                    await this.driver.Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                    await Create().Session.SubscribeAsync(new SubscribeCommandParameters(new[] { "log.entryAdded" }));
                }
            }
            """),
        ("generic type parameter standing in for the driver", """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            class C<TDriver>
                where TDriver : IBiDiCommandExecutor
            {
                async Task M(TDriver driver)
                {
                    await driver.ExecuteCommandAsync(new StatusCommandParameters());
                }
            }
            """),
        ("top-level statements", """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
            driver.Log.OnEntryAdded.AddObserver(async (e) => { });
            await driver.StartAsync("ws://localhost:9222");
            await driver.Session.SubscribeAsync(new SubscribeCommandParameters("log.entryAdded"));
            await driver.DisposeAsync();
            """),
        ("nullable list add without an initializer", """
            #nullable enable
            using WebDriverBiDi.Network;

            class C
            {
                void M(ContinueRequestCommandParameters parameters)
                {
                    parameters.Headers.Add(new Header("name", BytesValue.FromString("value")));
                }
            }
            """),
    ];

    /// <summary>
    /// A correlation of a syntax token's text with a name: a comparison in either operand order
    /// (against a literal or against another value), the same comparison expressed as a membership
    /// test over a set of names, and the use of the text as a collection key.
    /// </summary>
    /// <remarks>
    /// The comparison alternatives deliberately do not require a string literal on the other side.
    /// The mismatches this rule exists to catch have been between a token's text and a name held in a
    /// variable — one analyzer stored a dictionary key with <c>Text</c> and looked it up with
    /// <c>ValueText</c>, another recorded a variable name with <c>Text</c> and compared it with
    /// <c>ValueText</c> — and a literal-only pattern sees neither.
    /// </remarks>
    private static readonly Regex NameComparisonUsingText = new(
        """Identifier\.Text\s*(==|!=)|(==|!=)\s*[A-Za-z_.]*Identifier\.Text|\.Contains\([^)]*Identifier\.Text\)|\[[^\]]*Identifier\.Text\]""",
        RegexOptions.Compiled);

    /// <summary>
    /// Asserts that no analyzer or code fix source compares an identifier's <c>Text</c> against a
    /// member name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>SyntaxToken.Text</c> is the identifier exactly as written, so for the verbatim identifier
    /// <c>@AddObserver</c> — legal C# meaning precisely <c>AddObserver</c> — it is <c>"@AddObserver"</c>
    /// and no name comparison matches. Every such comparison gates an analyzer's real work, so a
    /// mismatch is a silently lost diagnostic. <c>ValueText</c> strips the escape and is the value to
    /// compare.
    /// </para>
    /// <para>
    /// This deliberately does not ban <c>Identifier.Text</c> outright. Where an identifier is emitted
    /// into generated code, <c>Text</c> is the correct choice and <c>ValueText</c> would be a defect:
    /// dropping the <c>@</c> from a variable named <c>@event</c> produces source that does not compile.
    /// Only the comparisons are constrained.
    /// </para>
    /// </remarks>
    [Fact]
    public void NameComparisonsUseValueTextNotText()
    {
        List<string> violations = [];
        foreach (string file in EnumerateAnalyzerSources())
        {
            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (NameComparisonUsingText.IsMatch(lines[i]))
                {
                    violations.Add($"{Path.GetFileName(file)}:{i + 1}: {lines[i].Trim()}");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Confirms the convention check is actually reading the analyzer sources, so that a broken path
    /// cannot make <see cref="NameComparisonsUseValueTextNotText"/> pass by finding nothing to check.
    /// </summary>
    [Fact]
    public void AnalyzerSourcesAreFound()
    {
        List<string> sources = EnumerateAnalyzerSources().ToList();
        Assert.Contains(sources, file => Path.GetFileName(file) == "AnalyzerSymbolHelpers.cs");
        Assert.True(sources.Count > 25, $"Expected the analyzer and code fix sources, found {sources.Count}.");
    }

    /// <summary>
    /// Runs every analyzer in the package over the corpus and asserts that none of them throws.
    /// </summary>
    /// <remarks>
    /// A thrown analyzer surfaces as AD0001 and suppresses that rule for the whole file it was
    /// analyzing, so the cost of the bug is every diagnostic the rule would otherwise have produced —
    /// a loss no test for an individual rule would notice, because each of those feeds its analyzer
    /// well-formed code. Analyzers are discovered by reflection so a newly added one is covered the
    /// day it is written. Exceptions are collected from both routes they can take: the callback the
    /// host is given, and the AD0001 diagnostic a build would see.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NoAnalyzerThrowsOnErroneousOrUnusualCode()
    {
        ImmutableArray<DiagnosticAnalyzer> analyzers = CreateAllAnalyzers();
        ImmutableArray<MetadataReference> references = await GetCorpusReferencesAsync();

        List<string> failures = [];
        int totalDiagnostics = 0;
        foreach ((string name, string source) in AnalyzerCorpus)
        {
            List<string> thrown = [];
            CSharpCompilation compilation = CSharpCompilation.Create(
                "AnalyzerCorpus",
                [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest), cancellationToken: TestContext.Current.CancellationToken)],
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication, nullableContextOptions: NullableContextOptions.Enable));

            // Analysis runs single-threaded so that the collected list needs no synchronization and a
            // failure names the same analyzer every time.
            CompilationWithAnalyzers withAnalyzers = compilation.WithAnalyzers(
                analyzers,
                new CompilationWithAnalyzersOptions(
                    options: null!,
                    onAnalyzerException: (exception, analyzer, _) => thrown.Add($"{analyzer.GetType().Name} threw {exception.GetType().Name}: {exception.Message}"),
                    concurrentAnalysis: false,
                    logAnalyzerExecutionTime: false));

            ImmutableArray<Diagnostic> diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
            thrown.AddRange(diagnostics.Where(diagnostic => diagnostic.Id == "AD0001").Select(diagnostic => diagnostic.GetMessage()));
            totalDiagnostics += diagnostics.Count(diagnostic => diagnostic.Id != "AD0001");

            if (thrown.Count > 0)
            {
                failures.Add($"[{name}] {string.Join("; ", thrown)}");
            }
        }

        Assert.True(failures.Count == 0, $"An analyzer threw while analyzing the corpus, which suppresses that rule for the whole file:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");

        // A corpus that failed to reference the library would bind none of the types the analyzers key
        // on, report nothing, and pass this test having exercised nothing at all.
        Assert.True(totalDiagnostics > 0, "The corpus produced no analyzer diagnostics at all, so it is not exercising the analyzers.");
    }

    /// <summary>
    /// Confirms the reflection discovery finds the whole analyzer package, so that a discovery that
    /// silently returned nothing could not make <see cref="NoAnalyzerThrowsOnErroneousOrUnusualCode"/>
    /// pass with no analyzer having run.
    /// </summary>
    [Fact]
    public void EveryAnalyzerIsDiscovered()
    {
        ImmutableArray<DiagnosticAnalyzer> analyzers = CreateAllAnalyzers();
        Assert.True(analyzers.Length >= 28, $"Expected the full analyzer package, found {analyzers.Length}.");

        IEnumerable<string> ids = analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics).Select(descriptor => descriptor.Id).Distinct();
        Assert.Contains("BIDI001", ids);
        Assert.Contains("BIDI031", ids);
    }

    private static ImmutableArray<DiagnosticAnalyzer> CreateAllAnalyzers()
    {
        return
        [
            .. typeof(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer).Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type))
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)
                .OrderBy(analyzer => analyzer.GetType().Name, StringComparer.Ordinal),
        ];
    }

    private static async Task<ImmutableArray<MetadataReference>> GetCorpusReferencesAsync()
    {
        ImmutableArray<MetadataReference> references = await ReferenceAssemblies.Net.Net80.ResolveAsync(LanguageNames.CSharp, TestContext.Current.CancellationToken);
        return references.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
    }

    private static IEnumerable<string> EnumerateAnalyzerSources()
    {
        string repositoryRoot = AnalyzerTestHelpers.FindRepositoryRoot();
        foreach (string projectDirectory in new[] { "WebDriverBiDi.Analyzers", "WebDriverBiDi.Analyzers.CodeFixProviders" })
        {
            string directory = Path.Combine(repositoryRoot, "src", projectDirectory);
            foreach (string file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                // Skip build intermediates, which contain generated copies of these sources.
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                    file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                yield return file;
            }
        }
    }
}
