// <copyright file="BiDiDriver010AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver010 analyzer.
/// </summary>
public class BiDiDriver010AnalyzerTests
{
    [Test]
    public async Task FireAndForgetModuleCommand_ReportsError()
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
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(12, 13, 12, 116)
            .WithArguments("NavigateAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task AwaitedModuleCommand_NoDiagnostic()
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Test]
    public async Task ModuleCommandAssignedToVariable_NoDiagnostic()
    {
        string testCode = """
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
                        Task<NavigateCommandResult> task = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        await task;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Test]
    public async Task ModuleCommandPassedAsArgument_NoDiagnostic()
    {
        string testCode = """
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
                        await ProcessTask(driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")));
                    }

                    private async Task ProcessTask(Task<NavigateCommandResult> task)
                    {
                        await task;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Test]
    public async Task ModuleCommandReturned_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public Task<NavigateCommandResult> TestMethod()
                    {
                        BiDiDriver driver = new();
                        return driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Test]
    public async Task MultipleFireAndForgetCommands_ReportsMultipleErrors()
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
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        driver.Script.EvaluateAsync(new EvaluateCommandParameters("expression", new ContextTarget("realmId"), true));
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(13, 13, 13, 116)
            .WithArguments("NavigateAsync");

        DiagnosticResult expected2 = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(14, 13, 14, 121)
            .WithArguments("EvaluateAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected1, expected2);
    }

    [Test]
    public async Task FireAndForgetInExpressionStatement_ReportsError()
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
                        driver.BrowsingContext.ReloadAsync(new ReloadCommandParameters("contextId"));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(12, 13, 12, 89)
            .WithArguments("ReloadAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task NonModuleMethod_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class CustomClass
                {
                    public Task<string> GetDataAsync()
                    {
                        return Task.FromResult("data");
                    }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        CustomClass custom = new();
                        custom.GetDataAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Test]
    public async Task NonGenericTaskReturningMethod_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        driver.StopAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Test]
    public async Task ModuleCommandInConditional_FireAndForget_ReportsError()
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
                        if (condition)
                        {
                            driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(14, 17, 14, 120)
            .WithArguments("NavigateAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task ModuleCommandInLoop_FireAndForget_ReportsError()
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
                        for (int i = 0; i < 5; i++)
                        {
                            driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", $"https://example{i}.com"));
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(14, 17, 14, 124)
            .WithArguments("NavigateAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task ModuleCommandWithConfigureAwait_NoDiagnostic()
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
                        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ConfigureAwait(false);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }
}
