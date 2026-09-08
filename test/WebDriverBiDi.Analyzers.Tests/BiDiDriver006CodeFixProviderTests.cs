// <copyright file="BiDiDriver006CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver006 code fix provider.
/// </summary>
public class BiDiDriver006CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix adds using declaration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_CodeFixAddsUsingDeclaration()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var {|#0:observer|} = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        RealAssemblyCodeFixTest<BiDiDriver006_ObserverDisposalAnalyzer, BiDiDriver006_ObserverDisposalCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ObserverDeclaredAsField_NoDiagnostic()
    {
        // BIDI006 only watches local variable declarations; field-level declarations
        // must not trigger the diagnostic.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly EventObserver<EntryAddedEventArgs> _observer;

                    public TestClass(BiDiDriver driver)
                    {
                        _observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver006_ObserverDisposalAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that no fix is offered when the project's language version predates C# 8, which is when
    /// the using declaration this fix emits was introduced. The diagnostic still reports; only the
    /// automated edit is withheld, because there is no equivalent one-line spelling to fall back to.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task BelowCSharp8_OffersNoFix()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> belowCSharp8Actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver006_ObserverDisposalAnalyzer, BiDiDriver006_ObserverDisposalCodeFixProvider>(
            testCode,
            referenceWebDriverBiDi: true,
            languageVersion: LanguageVersion.CSharp7_3);

        Assert.Empty(belowCSharp8Actions);

        // The same source at C# 8 does get a fix, which pins that the empty result above comes from the
        // language version and not from the diagnostic failing to appear.
        (IReadOnlyList<CodeAction> cSharp8Actions, Document __) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver006_ObserverDisposalAnalyzer, BiDiDriver006_ObserverDisposalCodeFixProvider>(
            testCode,
            referenceWebDriverBiDi: true,
            languageVersion: LanguageVersion.CSharp8);

        Assert.Single(cSharp8Actions);
    }
}
