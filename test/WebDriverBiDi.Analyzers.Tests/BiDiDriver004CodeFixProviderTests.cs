// <copyright file="BiDiDriver004CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver004 code fix provider. These compile against the real
/// <c>WebDriverBiDi.dll</c> so the fixed code is verified against the actual method signatures,
/// which place an optional <see cref="System.TimeSpan"/> parameter before the trailing
/// <see cref="System.Threading.CancellationToken"/>. A positionally appended token would bind to
/// that parameter and fail to compile (CS1503); the fix must therefore use a named argument.
/// </summary>
public class BiDiDriver004CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix adds CancellationToken.None as a named argument.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTreeAsync_CodeFixAddsCancellationTokenNone()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.GetTreeAsync(cancellationToken: CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix adds a cancellationToken named argument referencing an in-scope token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTreeAsync_CodeFixAddsCancellationTokenParameter()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken cancellationToken)
                    {
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken cancellationToken)
                    {
                        await driver.BrowsingContext.GetTreeAsync(cancellationToken: cancellationToken);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionEquivalenceKey = "AddCancellationTokenParameter",
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
