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
    public async Task NavigateAsync_WithoutCancellationToken_NoDiagnostic()
    {
        // Module commands don't have CancellationToken overloads yet
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
                        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }


    [Test]
    public async Task ReloadAsync_WithoutCancellationToken_NoDiagnostic()
    {
        // Module commands don't have CancellationToken overloads yet
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
                        await driver.BrowsingContext.ReloadAsync(new ReloadCommandParameters("contextId"));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    [Test]
    public async Task PrintAsync_WithoutCancellationToken_NoDiagnostic()
    {
        // Module commands don't have CancellationToken overloads yet
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
                        await driver.BrowsingContext.PrintAsync(new PrintCommandParameters("contextId"));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
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
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(11, 19, 11, 59)
            .WithArguments("StartAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task WaitForCheckpointAsync_WithoutCancellationToken_NoDiagnostic()
    {
        // EventObserver methods don't have CancellationToken overloads yet
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
                        await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
    }

    [Test]
    public async Task WaitForCheckpointAndTasksAsync_WithoutCancellationToken_NoDiagnostic()
    {
        // EventObserver methods don't have CancellationToken overloads yet
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
                        await observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode);
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
    public async Task MultipleLongRunningOperations_WithoutCancellationToken_ReportsWarningForStartAsync()
    {
        // Only StartAsync has a CancellationToken overload currently
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
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        await driver.BrowsingContext.PrintAsync(new PrintCommandParameters("contextId"));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(13, 19, 13, 59)
            .WithArguments("StartAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer>(testCode, expected);
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
