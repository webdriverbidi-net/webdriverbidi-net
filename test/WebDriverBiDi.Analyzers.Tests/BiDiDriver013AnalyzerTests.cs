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
    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
    public async Task WaitForCheckpointAsync_WithoutCancellationToken_ReportsWarning()
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
                        observer.SetCheckpoint(5);
                        await {|#0:observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitForCheckpointAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task WaitForCheckpointAndTasksAsync_WithoutCancellationToken_ReportsWarning()
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
                        observer.SetCheckpoint(5);
                        await {|#0:observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitForCheckpointAndTasksAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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
                        observer.SetCheckpoint();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }
}
