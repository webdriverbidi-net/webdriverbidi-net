// <copyright file="BiDiDriver004CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver004 code fix provider.
/// </summary>
[TestFixture]
public class BiDiDriver004CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix adds CancellationToken.None.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NavigateAsync_CodeFixAddsCancellationTokenNone()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class NavigateCommandParameters
                {
                    public NavigateCommandParameters(string contextId, string url) { }
                }

                public class NavigateCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters) => Task.FromResult(new NavigateCommandResult());
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new NavigateCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        var navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await {|#0:driver.BrowsingContext.NavigateAsync(navParams)|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class NavigateCommandParameters
                {
                    public NavigateCommandParameters(string contextId, string url) { }
                }

                public class NavigateCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters) => Task.FromResult(new NavigateCommandResult());
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new NavigateCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        var navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await driver.BrowsingContext.NavigateAsync(navParams, CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix adds cancellationToken parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NavigateAsync_CodeFixAddsCancellationTokenParameter()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class NavigateCommandParameters
                {
                    public NavigateCommandParameters(string contextId, string url) { }
                }

                public class NavigateCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters) => Task.FromResult(new NavigateCommandResult());
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new NavigateCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId, CancellationToken cancellationToken)
                    {
                        var navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await {|#0:driver.BrowsingContext.NavigateAsync(navParams)|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class NavigateCommandParameters
                {
                    public NavigateCommandParameters(string contextId, string url) { }
                }

                public class NavigateCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters) => Task.FromResult(new NavigateCommandResult());
                    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new NavigateCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId, CancellationToken cancellationToken)
                    {
                        var navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await driver.BrowsingContext.NavigateAsync(navParams, cancellationToken);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CodeActionEquivalenceKey = "AddCancellationTokenParameter",
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
