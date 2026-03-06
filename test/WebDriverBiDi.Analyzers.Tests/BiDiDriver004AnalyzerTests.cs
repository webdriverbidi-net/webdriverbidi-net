// <copyright file="BiDiDriver004AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver004 analyzer that suggests using CancellationToken.
/// </summary>
[TestFixture]
public class BiDiDriver004AnalyzerTests
{
    /// <summary>
    /// Tests that methods without CancellationToken report an info diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NavigateAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that methods with CancellationToken do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NavigateAsync_WithCancellationToken_NoDiagnostic()
    {
        string test = """
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

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

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

    /// <summary>
    /// Tests that non-BiDiDriver/Module methods are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonBiDiDriverMethod_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class CustomService
                {
                    public Task NavigateAsync(string url) => Task.CompletedTask;
                    public Task NavigateAsync(string url, CancellationToken cancellationToken) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(CustomService service)
                    {
                        await service.NavigateAsync("https://example.com");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that methods not in the suggestion list do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonSuggestedMethod_WithoutCancellationToken_NoDiagnostic()
    {
        string test = """
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
                public class BrowsingContextModule
                {
                    public Task SomeOtherMethodAsync() => Task.CompletedTask;
                    public Task SomeOtherMethodAsync(CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.SomeOtherMethodAsync();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that StartAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task StartAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public Task StartAsync(string url, CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.StartAsync("ws://localhost:9222")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("StartAsync");

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that EvaluateAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EvaluateAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public Script.ScriptModule Script { get; }
                }
            }

            namespace WebDriverBiDi.Script
            {
                public class ScriptModule
                {
                    public Task EvaluateAsync(string script) => Task.CompletedTask;
                    public Task EvaluateAsync(string script, CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.Script.EvaluateAsync("document.title")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EvaluateAsync");

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that methods without overload that takes CancellationToken do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MethodWithoutTokenOverload_NoDiagnostic()
    {
        string test = """
            using System;
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
                        await driver.BrowsingContext.NavigateAsync(navParams);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }
}
