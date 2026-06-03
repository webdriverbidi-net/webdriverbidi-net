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
    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task ModuleCommandWithConfigureAwaitNotAwaited_ReportsError()
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
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ConfigureAwait(false);
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

    [Fact]
    public async Task ModuleCommandWithConversionAssigned_NoDiagnostic()
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
                        Task task = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        await task;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandWithConversionPassedAsArgument_NoDiagnostic()
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
                        await ProcessTask(driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")));
                    }

                    private async Task ProcessTask(Task task)
                    {
                        await task;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandWithConversionAwaited_NoDiagnostic()
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
                        Task task = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        await task;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandWithConversionReturned_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        Task result = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        return result;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandContinueWithAwaited_NoDiagnostic()
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
                        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ContinueWith(t => t.Result);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandContinueWithNotAwaited_ReportsError()
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
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ContinueWith(t => t.Result);
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

    [Fact]
    public async Task ModuleCommandContinueWithAssigned_NoDiagnostic()
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
                        Task continuation = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ContinueWith(t => t.Result);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandContinueWithReturned_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        return driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ContinueWith(t => t.Result);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandContinueWithPassedAsArgument_NoDiagnostic()
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
                        await ProcessTask(driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ContinueWith(t => t.Result));
                    }

                    private async Task ProcessTask(Task task)
                    {
                        await task;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandContinueWithChainedNotAwaited_ReportsError()
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
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).ContinueWith(t => t.Result).ContinueWith(t => t.Result);
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

    [Fact]
    public async Task ClassNotEndingWithModule_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class CustomHandler
                {
                    public Task<string> ProcessAsync()
                    {
                        return Task.FromResult("data");
                    }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        CustomHandler handler = new();
                        handler.ProcessAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ClassEndingWithModuleButNoModuleBaseClass_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class CustomModule
                {
                    public Task<string> ProcessAsync()
                    {
                        return Task.FromResult("data");
                    }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        CustomModule module = new();
                        module.ProcessAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandInVariableDeclarator_NoDiagnostic()
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
                        var task = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com"));
                        await task;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task CustomModuleWithNonGenericTask_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestModule : Module
                {
                    public TestModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "test";
                    public Task DoSomethingAsync() => Task.CompletedTask;
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        IBiDiCommandExecutor executor = null!;
                        TestModule module = new(executor);
                        module.DoSomethingAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task CustomModuleWithVoidMethod_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestModule : Module
                {
                    public TestModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "test";
                    public void DoSomething() { }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        IBiDiCommandExecutor executor = null!;
                        TestModule module = new(executor);
                        module.DoSomething();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task CustomModuleWithNonAsyncMethod_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestModule : Module
                {
                    public TestModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "test";
                    public string GetData() => "data";
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        IBiDiCommandExecutor executor = null!;
                        TestModule module = new(executor);
                        module.GetData();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleWithInterfaceInheritance_FireAndForget_ReportsError()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public interface ITestInterface
                {
                    Task<string> GetDataAsync();
                }

                public class TestModule : Module, ITestInterface
                {
                    public TestModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "test";
                    public Task<string> GetDataAsync() => Task.FromResult("data");
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        IBiDiCommandExecutor executor = null!;
                        TestModule module = new(executor);
                        module.GetDataAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(24, 13, 24, 34)
            .WithArguments("GetDataAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task DeepModuleInheritanceHierarchy_FireAndForget_ReportsError()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public abstract class BaseModule : Module
                {
                    protected BaseModule(IBiDiCommandExecutor driver) : base(driver) { }
                }

                public class ConcreteModule : BaseModule
                {
                    public ConcreteModule(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "concrete";
                    public Task<string> ProcessAsync() => Task.FromResult("result");
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        IBiDiCommandExecutor executor = null!;
                        ConcreteModule module = new(executor);
                        module.ProcessAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(24, 13, 24, 34)
            .WithArguments("ProcessAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a module method returning plain Task (not generic Task{T}) unawaited
    /// does NOT report BIDI010 — exercises IsTaskReturningMethod returning false (line 113).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleMethod_FireAndForget_PlainTask_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    // Plain non-generic Task — IsTaskReturningMethod returns false (line 113)
                    public Task DoSomethingAsync() => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BrowserModule module)
                    {
                        // Fire and forget of a plain Task — not flagged (not Task<T>)
                        module.DoSomethingAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module method returning Task{string} (not CommandResult) when unawaited
    /// still reports BIDI010 — BIDI010 checks Task{T} generically via IsTaskReturningMethod,
    /// not whether T inherits CommandResult. Exercises InheritsFromCommandResult path in
    /// IsModuleCommandMethod indirectly (all lines of IsModuleCommandMethod are exercised).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleMethod_FireAndForget_TaskOfNonCommandResult_ReportsDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    // Task<string> — IsTaskReturningMethod = true; BIDI010 fires on fire-and-forget
                    public Task<string> GetNameAsync() => Task.FromResult("browser");
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BrowserModule module)
                    {
                        module.GetNameAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(24, 13, 24, 34)
            .WithArguments("GetNameAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that assigning a module command result to a variable is not flagged —
    /// exercises the ISimpleAssignmentOperation branch (line 118).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_AssignedToExistingVariable_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BrowserModule module)
                    {
                        // Assignment to existing variable — ISimpleAssignmentOperation parent.
                        Task<EmptyResult> t;
                        t = module.CloseAsync();
                        await t;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that passing a module command result as a method argument is not flagged —
    /// exercises the IArgumentOperation branch (line 118/166 pattern) via a conversion.
    /// Also exercises the IConversionOperation parent path (line 138) when the result
    /// is used in an awaited conversion context.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_PassedAsArgument_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BrowserModule module)
                    {
                        // Passed as argument — IArgumentOperation parent.
                        await Consume(module.CloseAsync());
                    }

                    private static async Task Consume(Task<EmptyResult> t) => await t;
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that awaiting a module command through an explicit cast does not report —
    /// exercises the IConversionOperation parent IAwaitOperation branch (line 138).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_AwaitedViaExplicitCast_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BrowserModule module)
                    {
                        // Explicit cast to base Task type, then awaited.
                        await (Task)module.CloseAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that returning a module command result from a method does not report —
    /// exercises the IReturnOperation branch in IsReturnValueUsed (line 130 in the
    /// invocation overload) and the IsOperationUsed ISimpleAssignmentOperation false
    /// path (line 166).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_ReturnedFromMethod_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public Task<EmptyResult> GetCloseTask(BrowserModule module)
                    {
                        return module.CloseAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module command stored via a variable declaration cast does not report —
    /// exercises IVariableInitializerOperation branch in IsReturnValueUsed(IConversionOperation)
    /// (line 138 block 107 branch 1).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_StoredInObjectVariableViaCast_DoesNotReportDiagnostic()
    {
        // Cast to object forces a genuine IConversionOperation (widening reference conversion).
        // IConversionOperation.Parent is IVariableInitializerOperation — exercises block 107 branch 1.
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BrowserModule module)
                    {
                        // Cast to object: IConversionOperation parent = IVariableInitializerOperation.
                        object t = (object)module.CloseAsync();
                        await (Task<EmptyResult>)t;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module command passed as argument via cast does not report —
    /// exercises IArgumentOperation branch in IsReturnValueUsed(IConversionOperation)
    /// (line 138 block 125).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_PassedAsArgumentViaCast_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BrowserModule module)
                    {
                        // Cast result passed as argument — IConversionOperation parent is IArgumentOperation.
                        await Consume((Task)module.CloseAsync());
                    }

                    private static async Task Consume(Task t) => await t;
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module command used as a return value via cast does not report —
    /// exercises IReturnOperation branch in IsReturnValueUsed(IConversionOperation)
    /// (line 138 block 125 branch 1).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_ReturnedViaCast_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    // Cast to object, then return — IConversionOperation parent is IReturnOperation.
                    public object GetTask(BrowserModule module) => (object)module.CloseAsync();
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module command result used via chained method call does not report —
    /// exercises the IsOperationUsed false path (line 166 block 31 branch 0) where parent
    /// is neither IVariableInitializerOperation nor ISimpleAssignmentOperation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_ChainedCallPassedAsArgument_DoesNotReportDiagnostic()
    {
        // module.CloseAsync() passed as argument via chained .ToString() call —
        // parent of CloseAsync is IInvocationOperation (ToString parent), whose parent is
        // IArgumentOperation → IsOperationUsed: block 23 branch 0 (not IAwait), block 31 branch 0
        // (not Initializer/Assignment), block 40 branch 1 (IS IArgument → return true).
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BrowserModule module)
                    {
                        // ChainedOnInvocation: CloseAsync().GetHashCode() — parent chain goes to argument.
                        await Consume(module.CloseAsync().GetHashCode());
                    }

                    private static Task Consume(int v) => Task.CompletedTask;
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>Tests chained call result (existing).</summary>
    [Fact]
    public async Task ModuleCommand_UsedInChainedCall_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public class BrowserModule : Module
                {
                    public Task<EmptyResult> CloseAsync() => Task.FromResult(new EmptyResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod(BrowserModule module)
                    {
                        // task.ConfigureAwait(false) — parent of CloseAsync() invocation is
                        // IInvocationOperation (ConfigureAwait), NOT IVariableInitializerOperation.
                        await module.CloseAsync().ConfigureAwait(false);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }
}
