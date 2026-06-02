// <copyright file="BiDiDriver013AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver013 analyzer.
/// </summary>
public class BiDiDriver013AnalyzerTests
{
    [Fact]
    public async Task NavigateAsync_WithoutCancellationToken_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task ReloadAsync_WithoutCancellationToken_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.BrowsingContext.ReloadAsync(new ReloadCommandParameters("contextId"))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("ReloadAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task PrintAsync_WithoutCancellationToken_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.BrowsingContext.PrintAsync(new PrintCommandParameters("contextId"))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("PrintAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartAsync_WithoutCancellationToken_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.StartAsync("ws://localhost:9222")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("StartAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitForAsync_WithoutCancellationToken_ReportsWarning()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        await {|#0:observer.WaitForCapturedTasksAsync(5, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitForCapturedTasksAsync_WithoutCancellationToken_ReportsWarning()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        await {|#0:observer.WaitForCapturedTasksCompleteAsync(5, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksCompleteAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task NonLongRunningMethod_WithoutCancellationToken_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    [Fact]
    public async Task NonBiDiDriverMethod_WithoutCancellationToken_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class CustomClass
                {
                    public async Task NavigateAsync(string url)
                    {
                        await Task.Delay(100);
                    }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomClass custom = new();
                        await custom.NavigateAsync("https://example.com");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    [Fact]
    public async Task MultipleLongRunningOperations_WithoutCancellationToken_ReportsAllWarnings()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.StartAsync("ws://localhost:9222")|};
                        await {|#1:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"))|};
                        await {|#2:driver.BrowsingContext.PrintAsync(new PrintCommandParameters("contextId"))|};
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("StartAsync");
        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("NavigateAsync");
        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(2)
            .WithArguments("PrintAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected0, expected1, expected2);
    }

    [Fact]
    public async Task MethodWithoutTokenOverload_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a call to an unresolvable method name does not report a diagnostic —
    /// exercises methodSymbol == null guard (line 62).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NavigateAsync_OnUnresolvableReceiver_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.{|CS1061:NavigateAsync|}("https://example.com");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that calling a long-running method with an explicit CancellationToken does not
    /// report a diagnostic — exercises the hasExplicitToken early return (line 87).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NavigateAsync_WithExplicitCancellationToken_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class Module { }

                public class BrowsingContextModule : Module
                {
                    public Task<string> NavigateAsync(string url, CancellationToken cancellationToken = default)
                        => Task.FromResult(url);
                }

                public class BiDiDriver
                {
                    public BrowsingContextModule BrowsingContext { get; } = new();
                }
            }

            namespace TestApp
            {
                using System.Threading;
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken token)
                    {
                        // Passing an explicit token — hasExplicitToken is true, returns early (line 87).
                        await driver.BrowsingContext.NavigateAsync("https://example.com", token);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a known long-running method name on a type that is NOT a BiDiDriver or
    /// module does not report a diagnostic — exercises IsTargetType returning false (line 87).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NavigateAsync_OnUnrelatedType_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class SomeHelper
                {
                    public Task NavigateAsync(string url) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        SomeHelper helper = new SomeHelper();
                        await helper.NavigateAsync("https://example.com");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a known long-running method on a target type that has NO overload
    /// accepting CancellationToken does not report a diagnostic — exercises the
    /// hasTokenOverload == false early exit (line 107 path not taken → no diagnostic).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NavigateAsync_WithNoTokenOverload_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class Module { }

                public class BrowsingContextModule : Module
                {
                    // NavigateAsync exists but has no CancellationToken overload
                    public Task<string> NavigateAsync(string url) => Task.FromResult(url);
                }

                public class BiDiDriver
                {
                    public BrowsingContextModule BrowsingContext { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.NavigateAsync("https://example.com");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }
}
