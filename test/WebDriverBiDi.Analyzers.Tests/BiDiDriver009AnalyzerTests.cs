// <copyright file="BiDiDriver009AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
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
            using WebDriverBiDi;

            namespace TestApp
            {
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

        // Kept as a hand-written stub: every real module method ends in "Async", so a non-async
        // module method (needed to exercise the !EndsWith("Async") branch) cannot be reproduced
        // against the real API.
        CSharpAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
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

        // Kept as a hand-written stub: every real module method ends in "Async", so a non-async
        // module method called before StartAsync (needed to exercise the !EndsWith("Async") early
        // return) cannot be reproduced against the real API.
        CSharpAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
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

        // Kept as a hand-written stub: every real module command method returns a generic
        // Task<TCommandResult>, so a module Async method returning a non-generic Task (needed to
        // exercise the non-generic-Task early return) cannot be reproduced against the real API.
        CSharpAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
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

        // Kept as a hand-written stub: every real module command method returns Task<TCommandResult>,
        // so a module Async method returning Task<string> (needed to exercise the
        // InheritsFromCommandResult false branch) cannot be reproduced against the real API.
        CSharpAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
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
            using WebDriverBiDi;

            namespace TestApp
            {
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

    /// <summary>
    /// Tests that invocations which cannot name a driver variable are ignored: one whose expression
    /// is a bare identifier rather than a member access, and one whose nested member-access root is
    /// not a driver.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvocationsWithoutADriverReceiver_DoNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class Inner
                {
                    public void DoWork() { }
                }

                public class Holder
                {
                    public Inner Inner { get; } = new Inner();
                }

                public class TestClass
                {
                    private static void Helper() { }

                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        Holder holder = new Holder();

                        // Invocation whose expression is a bare identifier, not a member access.
                        Helper();

                        // Nested member access whose root identifier is not a driver.
                        holder.Inner.DoWork();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommand_AfterStopAsync_ReportsError()
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
                        await driver.Session.StatusAsync();
                        await driver.StopAsync();
                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task NonCommandDriverMethod_BeforeStartAsync_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        SessionModule session = driver.GetModule<SessionModule>("session");
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandInEventHandlerLambdaBeforeStartAsync_NoDiagnostic()
    {
        // A command issued inside an event-handler lambda runs when the event fires (after the
        // connection is started), not at the point the handler is registered. The analyzer must not
        // descend into the lambda body and flag it as executing before StartAsync.
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
                        BiDiDriver driver = new BiDiDriver();
                        driver.BrowsingContext.OnLoad.AddObserver(async args =>
                        {
                            await driver.ExecuteCommandAsync(new StatusCommandParameters());
                        });
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandInLocalFunctionBeforeStartAsync_NoDiagnostic()
    {
        // A command issued inside a local function runs only when the function is called, not where it
        // is declared, so a declaration before StartAsync must not be flagged.
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
                        BiDiDriver driver = new BiDiDriver();

                        async Task RunCommandAsync()
                        {
                            await driver.ExecuteCommandAsync(new StatusCommandParameters());
                        }

                        await driver.StartAsync("ws://localhost:9222");
                        await RunCommandAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task DynamicInvocationWithDriverTracked_NoDiagnostic()
    {
        // With a driver tracked, a dynamic invocation does not bind to a method symbol; the analyzer
        // must ignore it (and report nothing) rather than misclassify it as a command call.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        dynamic value = "text";
                        value.ToLowerInvariant();
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task InvocationsWithNoDriverDeclared_NoDiagnostic()
    {
        // A method containing invocations but no BiDiDriver variable has nothing to track; the analyzer
        // must produce no diagnostics (and skips the semantic bind for these invocations).
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        Console.WriteLine("no driver here");
                        await Task.Delay(1);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task StopAsyncInThenBranch_CommandInElseBranch_NoDiagnostic()
    {
        // The branches are mutually exclusive: the else branch runs only when the driver
        // was not stopped, so the command there is valid and must not be reported.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldStop)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        if (shouldStop)
                        {
                            await driver.StopAsync();
                        }
                        else
                        {
                            await driver.ExecuteCommandAsync(new StatusCommandParameters());
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task DriverFromAwaitedFactoryMethod_CommandWithoutObservedStart_NoDiagnostic()
    {
        // A driver obtained from a factory method may already have been started by the
        // factory; only variables initialized directly with an object creation are known
        // to be unstarted, so no Error-severity diagnostic may be reported here.
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
                        BiDiDriver driver = await CreateAndStartDriverAsync();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    private static async Task<BiDiDriver> CreateAndStartDriverAsync()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        return driver;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task ConditionalStartAsync_CommandAfterBranch_NoDiagnostic()
    {
        // The driver is started on at least one path through the branch, so the command
        // after it is not certain to fail and must not be reported at Error severity.
        // The non-driver object creation declaration also confirms such variables are
        // not tracked as drivers.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldStart)
                    {
                        BiDiDriver driver = new();
                        StatusCommandParameters parameters = new();
                        if (shouldStart)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }

                        await driver.ExecuteCommandAsync(parameters);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandInBothBranchesBeforeStart_ReportsErrorInEachBranch()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool condition)
                    {
                        BiDiDriver driver = new();
                        if (condition)
                        {
                            await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                        }
                        else
                        {
                            await {|#1:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expectedInThenBranch = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");
        DiagnosticResult expectedInElseBranch = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(1)
            .WithArguments("ExecuteCommandAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expectedInThenBranch, expectedInElseBranch);
    }

    [Fact]
    public async Task StartAsyncInSwitchSection_CommandAfterSwitch_NoDiagnostic()
    {
        // The driver is started on at least one path through the switch, so the command
        // after it must not be reported at Error severity.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        switch (mode)
                        {
                            case 0:
                                await driver.StartAsync("ws://localhost:9222");
                                break;
                            default:
                                break;
                        }

                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncBeforeSwitch_ConditionalStopInSection_CommandAfterSwitch_NoDiagnostic()
    {
        // The driver was started before the switch and is stopped on only one path
        // through it, so the command after the switch is not certain to fail and must
        // not be reported at Error severity.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        switch (mode)
                        {
                            case 0:
                                await driver.StopAsync();
                                break;
                        }

                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandInIfCondition_BeforeStart_ReportsError()
    {
        // Invocations in the branch condition execute unconditionally, before either
        // branch, so a command there is judged against the state at the branch point.
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
                        if (await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|} is not null)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected);
    }
}
