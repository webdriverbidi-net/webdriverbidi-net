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
public class BiDiDriver004AnalyzerTests
{
    /// <summary>
    /// Tests that NavigateAsync without CancellationToken is not reported by BIDI004: that call
    /// is reported by BIDI013 (long-running operation) instead, so it must not produce two diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NavigateAsync_WithoutCancellationToken_NoDiagnostic_ReportedByBIDI013()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        NavigateCommandParameters navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await driver.BrowsingContext.NavigateAsync(navParams);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that methods with CancellationToken do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NavigateAsync_WithCancellationToken_NoDiagnostic()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId, CancellationToken cancellationToken)
                    {
                        NavigateCommandParameters navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await driver.BrowsingContext.NavigateAsync(navParams, cancellationToken: cancellationToken);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-BiDiDriver/Module methods are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that methods not in the suggestion list do not report a diagnostic. ActivateAsync is a
    /// real BrowsingContext module method with a CancellationToken overload, but it is absent from the
    /// BIDI004 suggestion list, so the analyzer must not flag it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonSuggestedMethod_WithoutCancellationToken_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        ActivateCommandParameters activateParams = new ActivateCommandParameters(contextId);
                        await driver.BrowsingContext.ActivateAsync(activateParams);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that StartAsync without CancellationToken does not report BIDI004, because BIDI013
    /// already covers this call site with a higher-severity Warning diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_WithoutCancellationToken_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that EvaluateAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EvaluateAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        EvaluateCommandParameters evalParams = new EvaluateCommandParameters("document.title", new ContextTarget(contextId), true);
                        await {|#0:driver.Script.EvaluateAsync(evalParams)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EvaluateAsync");

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a method in the suggestion list without an overload that takes CancellationToken
    /// does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because every real suggested
    /// method (here <c>GetTreeAsync</c>) exposes a trailing <see cref="System.Threading.CancellationToken"/>
    /// overload. Declaring a suggested method with no token overload is the only way to exercise the
    /// analyzer's <c>hasTokenOverload == false</c> branch, so it cannot be reproduced against the real API.
    /// </remarks>
    [Fact]
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
                public class BrowsingContextModule
                {
                    // GetTreeAsync is in the BIDI004 suggestion list, but this stub declares no
                    // CancellationToken overload, so no suggestion is possible.
                    public Task GetTreeAsync() => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that unresolved method symbols are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UnresolvedMethodSymbol_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.{|CS1061:NonExistentMethod|}();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that null containing type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests GetFixAllProvider returns the correct provider.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver004_CancellationTokenSuggestionCodeFixProvider provider = new BiDiDriver004_CancellationTokenSuggestionCodeFixProvider();
        FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.Equal(WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI004()
    {
        BiDiDriver004_CancellationTokenSuggestionCodeFixProvider provider = new BiDiDriver004_CancellationTokenSuggestionCodeFixProvider();

        Assert.Contains(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, provider.FixableDiagnosticIds);
        Assert.Single(provider.FixableDiagnosticIds);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property of the analyzer.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI004()
    {
        BiDiDriver004_CancellationTokenSuggestionAnalyzer analyzer = new BiDiDriver004_CancellationTokenSuggestionAnalyzer();

        Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, analyzer.SupportedDiagnostics[0].Id);
    }

    /// <summary>
    /// Tests that CallFunctionAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CallFunctionAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        CallFunctionCommandParameters callParams = new CallFunctionCommandParameters("() => document.title", new ContextTarget(contextId), false);
                        await {|#0:driver.Script.CallFunctionAsync(callParams)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("CallFunctionAsync");

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that ExecuteCommandAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that GetTreeAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTreeAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that LocateNodesAsync without CancellationToken reports info.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LocateNodesAsync_WithoutCancellationToken_ReportsInfo()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        LocateNodesCommandParameters locateParams = new LocateNodesCommandParameters(contextId, new CssLocator("button"));
                        await {|#0:driver.BrowsingContext.LocateNodesAsync(locateParams)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("LocateNodesAsync");

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that calling a long-running method through a plain method invocation whose
    /// containing type cannot be resolved does not produce a diagnostic (exercises the
    /// IsBiDiDriverOrModuleType null-guard path).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NavigateAsync_OnUnresolvableReceiver_DoesNotReportDiagnostic()
    {
        // A free function call (not on any type) — the containing type will be null.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        await NavigateAsync("https://example.com");
                    }

                    private static Task NavigateAsync(string url) => Task.CompletedTask;
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UserTypeNamedModule_OutsideWebDriverBiDiNamespace_NoDiagnostic()
    {
        // A user's own type whose name merely ends in "Module" is not a library module: it neither
        // derives from the Module base class nor lives in the WebDriverBiDi namespace. The analyzer
        // must not treat it as a module and suggest a CancellationToken.
        string test = """
            using System.Threading;
            using System.Threading.Tasks;

            namespace UserApp
            {
                public class MyHelperModule
                {
                    public Task EvaluateAsync(string script) => Task.CompletedTask;
                    public Task EvaluateAsync(string script, CancellationToken cancellationToken) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(MyHelperModule helper)
                    {
                        await helper.EvaluateAsync("document.title");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
