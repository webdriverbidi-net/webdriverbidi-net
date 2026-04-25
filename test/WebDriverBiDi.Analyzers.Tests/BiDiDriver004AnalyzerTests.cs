// <copyright file="BiDiDriver004AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
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

    /// <summary>
    /// Tests that unresolved method symbols are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UnresolvedMethodSymbol_NoDiagnostic()
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
                public class BrowsingContextModule
                {
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.{|CS1061:NonExistentMethod|}();
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
    /// Tests that null containing type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NullContainingType_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        Func<Task> action = async () => { };
                        await action();
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
    /// Tests GetFixAllProvider returns the correct provider.
    /// </summary>
    [Test]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver004_CancellationTokenSuggestionCodeFixProvider provider = new BiDiDriver004_CancellationTokenSuggestionCodeFixProvider();
        FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.That(fixAllProvider, Is.EqualTo(WellKnownFixAllProviders.BatchFixer));
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Test]
    public void FixableDiagnosticIds_ContainsBIDI004()
    {
        BiDiDriver004_CancellationTokenSuggestionCodeFixProvider provider = new BiDiDriver004_CancellationTokenSuggestionCodeFixProvider();

        Assert.That(provider.FixableDiagnosticIds, Does.Contain(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId));
        Assert.That(provider.FixableDiagnosticIds.Length, Is.EqualTo(1));
    }

    /// <summary>
    /// Tests SupportedDiagnostics property of the analyzer.
    /// </summary>
    [Test]
    public void SupportedDiagnostics_ContainsBIDI004()
    {
        BiDiDriver004_CancellationTokenSuggestionAnalyzer analyzer = new BiDiDriver004_CancellationTokenSuggestionAnalyzer();

        Assert.That(analyzer.SupportedDiagnostics.Length, Is.EqualTo(1));
        Assert.That(analyzer.SupportedDiagnostics[0].Id, Is.EqualTo(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId));
    }

    /// <summary>
    /// Tests that CallFunctionAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CallFunctionAsync_WithoutCancellationToken_ReportsInfo()
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
                    public Task CallFunctionAsync(string function) => Task.CompletedTask;
                    public Task CallFunctionAsync(string function, CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.Script.CallFunctionAsync("myFunction")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("CallFunctionAsync");

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that ExecuteCommandAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ExecuteCommandAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public Task ExecuteCommandAsync(string command) => Task.CompletedTask;
                    public Task ExecuteCommandAsync(string command, CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.ExecuteCommandAsync("session.new")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that GetTreeAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetTreeAsync_WithoutCancellationToken_ReportsInfo()
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
                    public Task GetTreeAsync() => Task.CompletedTask;
                    public Task GetTreeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that LocateNodesAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task LocateNodesAsync_WithoutCancellationToken_ReportsInfo()
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
                    public Task LocateNodesAsync(string locator) => Task.CompletedTask;
                    public Task LocateNodesAsync(string locator, CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.BrowsingContext.LocateNodesAsync("css:button")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("LocateNodesAsync");

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
