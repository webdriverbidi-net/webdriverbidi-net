// <copyright file="BiDiDriver009AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver009 analyzer.
/// </summary>
public class BiDiDriver009AnalyzerTests
{
    [Test]
    public async Task ExecuteCommandAsync_BeforeStartAsync_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(12, 19, 12, 76)
            .WithArguments("ExecuteCommandAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task ExecuteCommandAsync_AfterStartAsync_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task ModuleCommand_BeforeStartAsync_ReportsError()
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
                        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(12, 19, 12, 122)
            .WithArguments("NavigateAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task ModuleCommand_AfterStartAsync_NoDiagnostic()
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
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task MultipleDrivers_CommandBeforeStartOnOne_ReportsError()
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
                        BiDiDriver driver1 = new();
                        BiDiDriver driver2 = new();
                        await driver1.StartAsync("ws://localhost:9222");
                        await driver1.BrowsingContext.GetTreeAsync();
                        await driver2.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(15, 19, 15, 57)
            .WithArguments("GetTreeAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task MultipleDrivers_BothStarted_NoDiagnostic()
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
                        BiDiDriver driver1 = new();
                        BiDiDriver driver2 = new();
                        await driver1.StartAsync("ws://localhost:9222");
                        await driver2.StartAsync("ws://localhost:9223");
                        await driver1.BrowsingContext.GetTreeAsync();
                        await driver2.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task MultipleCommands_BeforeStartAsync_ReportsMultipleErrors()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;
            using WebDriverBiDi.Script;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.BrowsingContext.GetTreeAsync();
                        await driver.Script.GetRealmsAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(13, 19, 13, 56)
            .WithArguments("GetTreeAsync");

        DiagnosticResult expected2 = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(14, 19, 14, 49)
            .WithArguments("GetRealmsAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected1, expected2);
    }

    [Test]
    public async Task NonCommandMethod_BeforeStartAsync_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task NonBiDiDriverCommand_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class CustomClass
                {
                    public Task<string> ExecuteCommandAsync()
                    {
                        return Task.FromResult("result");
                    }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomClass custom = new();
                        await custom.ExecuteCommandAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task DriverWithoutInitializer_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task CommandInConditional_AfterStart_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool condition)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        if (condition)
                        {
                            await driver.BrowsingContext.GetTreeAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task CommandInLoop_AfterStart_NoDiagnostic()
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
                        await driver.StartAsync("ws://localhost:9222");

                        for (int i = 0; i < 5; i++)
                        {
                            await driver.BrowsingContext.GetTreeAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task StartAsync_IsNotFlaggedAsCommand_NoDiagnostic()
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task StopAsync_BeforeStartAsync_NoDiagnostic()
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
                        await driver.StopAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Test]
    public async Task MethodWithoutBody_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public interface ITestInterface
                {
                    Task TestMethod();
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }
}
