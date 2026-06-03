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
    [Fact]
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
                        IBiDiCommandExecutor driver = new BiDiDriver();
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    /// <summary>
    /// Tests that an invocation whose method symbol cannot be resolved does not produce a
    /// diagnostic — exercises the methodSymbol == null guard (line 124).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UnresolvableMethodCall_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        // Call to a non-existent method — symbol resolution returns null
                        driver.{|CS1061:NonExistentMethod|}();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module method that does NOT end in "Async" is not flagged as a command
    /// before StartAsync — exercises the !method.Name.EndsWith("Async") early return in
    /// IsModuleCommandMethod (line 206).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleMethod_NonAsync_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }

                public abstract class Module { }

                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public BrowserModule Browser { get; } = new BrowserModule();
                }

                public class BrowserModule : Module
                {
                    // Non-async method on a module — should not trigger BIDI009
                    public string GetInfo() => "info";
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        string info = driver.Browser.GetInfo();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a non-Async method on a module called before StartAsync is not flagged —
    /// exercises the !method.Name.EndsWith("Async") early return in IsModuleCommandMethod
    /// (line 206).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleMethod_NonAsync_BeforeStart_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public abstract class Module { }

                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public BrowserModule Browser { get; } = new BrowserModule();
                }

                public class BrowserModule : Module
                {
                    // Synchronous method — does not end in "Async", so IsModuleCommandMethod
                    // returns false at line 206.
                    public string GetInfo() => "info";
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        string info = driver.Browser.GetInfo();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module Async method returning plain Task (non-generic) is not flagged
    /// as a command even before StartAsync — exercises IsModuleCommandMethod returning false
    /// at line 219 (non-generic Task has no TypeArguments).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleMethod_AsyncReturningPlainTask_BeforeStart_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public abstract class Module { }

                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public BrowserModule Browser { get; } = new BrowserModule();
                }

                public class BrowserModule : Module
                {
                    // Non-generic Task → IsModuleCommandMethod returns false (line 219)
                    public Task DoWorkAsync() => Task.CompletedTask;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        // Not started, but DoWorkAsync is not a command → no diagnostic
                        await driver.Browser.DoWorkAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a module Async method returning Task{string} (not a CommandResult) is not
    /// flagged even before StartAsync — exercises InheritsFromCommandResult returning false
    /// (line 235) when T is string.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleMethod_AsyncReturningTaskOfNonCommandResult_BeforeStart_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public abstract class Module { }

                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public BrowserModule Browser { get; } = new BrowserModule();
                }

                public class BrowserModule : Module
                {
                    // Task<string>: InheritsFromCommandResult(string) = false (line 235)
                    public Task<string> GetInfoAsync() => Task.FromResult("info");
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        // GetInfoAsync returns Task<string>, not Task<CommandResult> → no diagnostic
                        string info = await driver.Browser.GetInfoAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a driver variable declared without an initializer does not crash —
    /// exercises the variable.Initializer == null continue branch (line 87).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DriverVariable_DeclaredWithoutInitializer_DoesNotCrash()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandResult { }
                public class EmptyResult : CommandResult { }
                public abstract class Module { }

                public interface IBiDiCommandExecutor
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiCommandExecutor
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public BrowserModule Browser { get; } = new BrowserModule();
                }

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
                    public async Task TestMethod()
                    {
                        // Variable declared without initializer — exercises line 87.
                        BiDiDriver driver;
                        driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.Browser.CloseAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }
}
