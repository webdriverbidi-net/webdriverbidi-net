// <copyright file="AnalyzerTestHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using WebDriverBiDi;

/// <summary>
/// A <see cref="CSharpCodeFixTest{TAnalyzer, TCodeFix, TVerifier}"/> that forces LF line endings
/// for the ad-hoc test project.
/// </summary>
/// <typeparam name="TAnalyzer">The type of analyzer under test.</typeparam>
/// <typeparam name="TCodeFix">The type of code fix provider under test.</typeparam>
/// <remarks>
/// Microsoft.CodeAnalysis.Testing's fix-verification pipeline reformats the span touched by a code
/// fix using the ambient newline setting, which falls back to <c>Environment.NewLine</c> when no
/// <c>end_of_line</c> EditorConfig override is in effect for the test project. That makes code fix
/// tests fail on Windows (actual output gets CRLF on the touched line) even though the checked-out
/// source and the code fix providers themselves are LF-only — this is independent of the code fix
/// provider's own logic and cannot be worked around from within a provider. Injecting an explicit
/// <c>end_of_line = lf</c> EditorConfig here makes the comparison consistent across platforms.
/// </remarks>
public class LfCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    private const string LfEditorConfig = "root = true\n\n[*]\nend_of_line = lf\n";

    /// <summary>
    /// Initializes a new instance of the <see cref="LfCodeFixTest{TAnalyzer, TCodeFix}"/> class.
    /// </summary>
    public LfCodeFixTest()
    {
        // Only TestState needs this: it is the project the code fix provider actually runs
        // against, which is where the newline-sensitive cleanup pipeline reads its EditorConfig
        // from. Adding it to FixedState too is unnecessary and, for the handful of tests that
        // omit FixedCode (relying on the framework's "no explicit fixed state" skip of full output
        // comparison), it defeats that skip by giving FixedState explicit content.
        this.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", LfEditorConfig));
    }
}

/// <summary>
/// A <see cref="CSharpAnalyzerTest{TAnalyzer, TVerifier}"/> that references the real
/// <c>WebDriverBiDi</c> assembly so tests exercise the analyzer against the library's actual public API
/// rather than hand-written stubs; this makes signature drift in the analyzed types visible.
/// </summary>
/// <typeparam name="TAnalyzer">The type of analyzer under test.</typeparam>
public sealed class RealAssemblyAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RealAssemblyAnalyzerTest{TAnalyzer}"/> class.
    /// </summary>
    public RealAssemblyAnalyzerTest()
    {
        this.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
        this.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
    }
}

/// <summary>
/// A <see cref="LfCodeFixTest{TAnalyzer, TCodeFix}"/> that additionally references the real
/// <c>WebDriverBiDi</c> assembly so code-fix tests run against the library's actual public API.
/// </summary>
/// <typeparam name="TAnalyzer">The type of analyzer under test.</typeparam>
/// <typeparam name="TCodeFix">The type of code fix provider under test.</typeparam>
public sealed class RealAssemblyCodeFixTest<TAnalyzer, TCodeFix> : LfCodeFixTest<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RealAssemblyCodeFixTest{TAnalyzer, TCodeFix}"/> class.
    /// </summary>
    public RealAssemblyCodeFixTest()
    {
        this.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
        this.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
    }
}

/// <summary>
/// Helper methods for analyzer tests.
/// </summary>
public static class AnalyzerTestHelpers
{
    /// <summary>
    /// Verifies that an analyzer produces no diagnostics for the given test code.
    /// </summary>
    /// <typeparam name="TAnalyzer">The type of analyzer to test.</typeparam>
    /// <param name="testCode">The test code to analyze.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task VerifyAnalyzerAsync<TAnalyzer>(string testCode)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> test = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(GetWebDriverBiDiAssemblyPath()));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that an analyzer produces the expected diagnostics for the given test code.
    /// </summary>
    /// <typeparam name="TAnalyzer">The type of analyzer to test.</typeparam>
    /// <param name="testCode">The test code to analyze.</param>
    /// <param name="expected">The expected diagnostic results.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task VerifyAnalyzerAsync<TAnalyzer>(string testCode, params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> test = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(GetWebDriverBiDiAssemblyPath()));
        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync();
    }

    /// <summary>
    /// Runs an analyzer over the given source and invokes a code fix provider directly for every
    /// diagnostic it reports, returning the registered code actions and the document they apply to.
    /// </summary>
    /// <typeparam name="TAnalyzer">The type of analyzer to run.</typeparam>
    /// <typeparam name="TCodeFix">The type of code fix provider to invoke.</typeparam>
    /// <param name="source">The source code to analyze.</param>
    /// <param name="referenceWebDriverBiDi">Whether the analyzed source needs the library's real types.</param>
    /// <param name="languageVersion">The C# version the ad-hoc project is parsed with, for fixes whose output depends on it.</param>
    /// <returns>The registered code actions and the analyzed document.</returns>
    /// <remarks>
    /// Use this instead of <see cref="LfCodeFixTest{TAnalyzer, TCodeFix}"/> when the diagnostic is
    /// reported outside the syntax node the analyzer registered for (for example inside the body of
    /// a method passed as a method group); the testing framework rejects such "non-local"
    /// diagnostics before the provider is ever invoked.
    /// </remarks>
    internal static async Task<(IReadOnlyList<CodeAction> Actions, Document Document)> GetCodeActionsAsync<TAnalyzer, TCodeFix>(string source, bool referenceWebDriverBiDi = false, LanguageVersion languageVersion = LanguageVersion.Default, string? additionalSource = null)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        ImmutableArray<MetadataReference> references = await ReferenceAssemblies.Net.Net80.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
        if (referenceWebDriverBiDi)
        {
            // Analyzed sources that use the library's real types rather than hand-written stubs need it
            // on the compilation, or nothing resolves and no diagnostic — hence no code action — appears.
            references = references.Add(MetadataReference.CreateFromFile(GetWebDriverBiDiAssemblyPath()));
        }

        using AdhocWorkspace workspace = new();
        Project project = workspace.AddProject("TestProject", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithParseOptions(new CSharpParseOptions(languageVersion))
            .AddMetadataReferences(references);
        Document document = project.AddDocument("Test0.cs", source);
        if (additionalSource is not null)
        {
            // A second document in the same compilation, for the case where the analyzer reports at
            // the registration site because the handler's body lives in another syntax tree.
            document = document.Project.AddDocument("Test1.cs", additionalSource).Project.GetDocument(document.Id)!;
        }

        Compilation compilation = (await document.Project.GetCompilationAsync(CancellationToken.None))!;
        CompilationWithAnalyzers withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer()));
        ImmutableArray<Diagnostic> diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None);

        List<CodeAction> actions = [];
        TCodeFix provider = new();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            CodeFixContext context = new(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);
            await provider.RegisterCodeFixesAsync(context);
        }

        return (actions, document);
    }

    /// <summary>
    /// Applies a code action and returns the resulting text of the given document.
    /// </summary>
    /// <param name="action">The code action to apply.</param>
    /// <param name="document">The document whose changed text is returned.</param>
    /// <returns>The document text after the action is applied.</returns>
    internal static async Task<string> ApplyCodeActionAsync(CodeAction action, Document document)
    {
        ImmutableArray<CodeActionOperation> operations = await action.GetOperationsAsync(CancellationToken.None);
        ApplyChangesOperation applyChanges = Assert.Single(operations.OfType<ApplyChangesOperation>());
        Document changedDocument = applyChanges.ChangedSolution.GetDocument(document.Id)!;
        return (await changedDocument.GetTextAsync(CancellationToken.None)).ToString();
    }

    /// <summary>
    /// Gets the path to the WebDriverBiDi assembly for use in code fix tests.
    /// </summary>
    /// <returns>The assembly path.</returns>
    /// <summary>
    /// Gets the path to the <c>WebDriverBiDi</c> assembly the analyzer tests compile their sources against.
    /// </summary>
    /// <returns>The path to the assembly.</returns>
    /// <remarks>
    /// <para>
    /// This deliberately resolves the library's <c>net8.0</c> artifact by path rather than using
    /// <c>typeof(BiDiDriver).Assembly.Location</c>, which the project reference already supplies. The
    /// harnesses in this project compile their analyzed sources against
    /// <see cref="ReferenceAssemblies.Net.Net80"/>, and handing those compilations the library's
    /// <c>net10.0</c> build fails with <c>CS1705</c> — the assembly references a framework newer than the
    /// reference set. Switching to the loaded assembly fails 203 tests for that reason.
    /// </para>
    /// <para>
    /// The cost of resolving by path is that this artifact is refreshed only by an explicit build of the
    /// library project: a <c>net10.0</c> test project's project reference builds only the library's
    /// <c>net10.0</c> output. A stale artifact used to bind silently, so a test referencing a
    /// newly-added library type failed with <c>CS0246</c> pointing at the test source rather than at the
    /// real cause. It is now detected and reported: the artifact is compared against the newest source
    /// file in the library project, which answers "was this rebuilt since the source last changed?"
    /// directly, rather than against the loaded assembly's timestamp, which a copy during the test
    /// project's own build would make newer even when everything is current.
    /// </para>
    /// </remarks>
    internal static string GetWebDriverBiDiAssemblyPath()
    {
        string testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        // Extract configuration (Debug/Release) from path: .../bin/{Configuration}/net10.0
        string? configDir = Path.GetDirectoryName(testDirectory);
        string configuration = configDir != null ? Path.GetFileName(configDir) : "Debug";

        string repositoryRoot = FindRepositoryRoot();
        string librarySourceDirectory = Path.Combine(repositoryRoot, "src", "WebDriverBiDi");
        string net80AssemblyPath = Path.Combine(librarySourceDirectory, "bin", configuration, "net8.0", "WebDriverBiDi.dll");
        if (!File.Exists(net80AssemblyPath))
        {
            // Nothing to go stale; the loaded assembly is the only thing available.
            return typeof(BiDiDriver).Assembly.Location;
        }

        DateTime artifactWriteTime = File.GetLastWriteTimeUtc(net80AssemblyPath);
        DateTime newestSourceWriteTime = NewestSourceWriteTime(librarySourceDirectory);
        if (artifactWriteTime < newestSourceWriteTime)
        {
            throw new InvalidOperationException(
                $"The WebDriverBiDi {configuration} net8.0 assembly is older than the library's sources, so these tests "
                + $"would compile against an out-of-date library and fail in ways that point at the test source rather "
                + $"than at the stale reference.{Environment.NewLine}"
                + $"  assembly: {net80AssemblyPath} ({artifactWriteTime:u}){Environment.NewLine}"
                + $"  newest source: {newestSourceWriteTime:u}{Environment.NewLine}"
                + $"Build the library project for this configuration and re-run:{Environment.NewLine}"
                + $"  dotnet build src/WebDriverBiDi/WebDriverBiDi.csproj -c {configuration}");
        }

        return net80AssemblyPath;
    }

    /// <summary>
    /// Gets the most recent write time of any C# source file in a project directory.
    /// </summary>
    /// <param name="projectDirectory">The directory to search.</param>
    /// <returns>The newest write time, or <see cref="DateTime.MinValue"/> if there are no sources.</returns>
    private static DateTime NewestSourceWriteTime(string projectDirectory)
    {
        DateTime newest = DateTime.MinValue;
        foreach (string file in Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories))
        {
            // Build intermediates contain generated copies whose timestamps track the build, not the source.
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            DateTime writeTime = File.GetLastWriteTimeUtc(file);
            if (writeTime > newest)
            {
                newest = writeTime;
            }
        }

        return newest;
    }

    /// <summary>
    /// Finds the repository root by walking up from the test assembly until the solution file is found.
    /// </summary>
    /// <returns>The repository root directory.</returns>
    internal static string FindRepositoryRoot()
    {
        string? current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        while (current != null && !File.Exists(Path.Combine(current, "WebDriverBiDi.NET.sln")))
        {
            current = Path.GetDirectoryName(current);
        }

        return current ?? throw new InvalidOperationException("Could not locate the repository root from the test assembly location.");
    }
}
