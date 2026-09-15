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
                        IBiDiModuleHost driver = new BiDiDriver();
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

                public interface IBiDiDriverLifecycleManager
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiDriverLifecycleManager
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

                public interface IBiDiDriverLifecycleManager
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiDriverLifecycleManager
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

                public interface IBiDiDriverLifecycleManager
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiDriverLifecycleManager
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

                public interface IBiDiDriverLifecycleManager
                {
                    Task StartAsync(string url);
                }

                public class BiDiDriver : IBiDiDriverLifecycleManager
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

    [Fact]
    public async Task DriverDeclaredInsideTryBlock_CommandBeforeStart_ReportsError()
    {
        // Driver declarations inside nested blocks (here, a try block) are tracked the
        // same as top-level declarations.
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        try
                        {
                            BiDiDriver driver = new();
                            await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                        }
                        finally
                        {
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

    /// <summary>
    /// Tests that a StopAsync confined to a catch clause does not poison a command that follows the
    /// try statement. Without forking state across try/catch, the flat walk saw the stop and reported
    /// the later command at Error severity — on code where the catch rethrows, so the command is only
    /// ever reached when the catch did not run.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandAfterTryWithStopAsyncInCatch_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System;
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
                        try
                        {
                            await driver.Session.StatusAsync();
                        }
                        catch (Exception)
                        {
                            await driver.StopAsync();
                            throw;
                        }

                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a command inside a catch clause is not reported when the try block started the
    /// driver. The catch may begin after any prefix of the try, so the start may already have run.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandInCatchAfterStartAsyncInTry_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        catch (Exception)
                        {
                            await driver.Session.StatusAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a command in a try block on a driver that was never started is still reported.
    /// Forking state across try/catch must not turn the rule off inside a try statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandInTryWithoutStartAsync_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await {|#0:driver.Session.StatusAsync()|};
                        }
                        catch (Exception)
                        {
                        }
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

    /// <summary>
    /// Tests that a command in a finally block on a driver that was never started is still reported,
    /// and that the finally walk participates in the state merged after the statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandInFinallyWithoutStartAsync_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        try
                        {
                        }
                        finally
                        {
                            await {|#0:driver.Session.StatusAsync()|};
                        }
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

    /// <summary>
    /// Tests that a catch filter is walked, so a command in the filter expression on a never-started
    /// driver is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandInCatchFilterWithoutStartAsync_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        try
                        {
                        }
                        catch (Exception) when ({|#0:driver.Session.StatusAsync()|} is not null)
                        {
                        }
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
    public async Task Command_AfterDriverPassedToHelper_NoDiagnostic()
    {
        // The helper may start the driver, which this rule cannot see. Reporting an Error here would be
        // reporting on correct code, so the driver's state is treated as unknown once it escapes.
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
                        await StartHelperAsync(driver);
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    private static async Task StartHelperAsync(BiDiDriver driver)
                    {
                        await driver.StartAsync("ws://localhost:1234");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_AfterDriverStoredInField_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private BiDiDriver? stored;

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        this.stored = driver;
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_WhenALambdaStartsTheDriver_NoDiagnostic()
    {
        // A nested function runs when its delegate is invoked, not where it is declared, so a StartAsync
        // inside one puts the driver's state beyond what a textual walk can determine.
        string testCode = """
            using System;
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
                        Func<Task> starter = async () => await driver.StartAsync("ws://localhost:1234");
                        await starter();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_WhenALambdaOnlyIssuesCommands_StillReportsError()
    {
        // Treating every capture as an escape would lose this genuine error. Only a StartAsync or
        // StopAsync inside a nested function makes the state unknown.
        string testCode = """
            using System;
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
                        Func<Task> later = async () => await driver.ExecuteCommandAsync(new StatusCommandParameters());
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithSpan(14, 19, 14, 76)
            .WithArguments("ExecuteCommandAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task Command_AfterDriverReturnedToCaller_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<BiDiDriver> TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                        return driver;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_AfterDriverAliasedToAnotherLocal_NoDiagnostic()
    {
        // The alias may be started instead, and this rule tracks names rather than the object behind them.
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
                        BiDiDriver alias = driver;
                        await alias.StartAsync("ws://localhost:1234");
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_AfterDriverPlacedInObjectInitializer_NoDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;
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
                        List<BiDiDriver> tracked = new List<BiDiDriver> { driver };
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_AfterDriverPlacedInCollectionExpression_NoDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;
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
                        List<BiDiDriver> tracked = [driver];
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a driver declared by a classic <c>await using (T x = ...)</c> statement is tracked like one declared by a local declaration statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandInClassicUsingStatementDeclaration_BeforeStart_ReportsError()
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
                        await using (BiDiDriver driver = new())
                        {
                            await {|#0:driver.Session.StatusAsync()|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that handing the driver to a module's constructor, the documented way to register a custom module, is not treated as an escape: a module holds the driver and cannot start it, so a command before the start is still reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Command_AfterDriverPassedToCustomModuleConstructor_StillReportsError()
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
                        driver.RegisterModule(new CustomModule(driver));
                        await {|#0:driver.Session.StatusAsync()|};
                    }
                }

                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that handing the driver to the constructor of a type that is not a module remains an escape: that type may start the driver.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Command_AfterDriverPassedToNonModuleConstructor_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class Holder
                {
                    public Holder(BiDiDriver driver)
                    {
                        driver.StartAsync("ws://localhost:9222").Wait();
                    }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        Holder holder = new Holder(driver);
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that rebinding a tracked variable to a driver the walk cannot see stops the tracking. The
    /// new driver may already have been started by whatever produced it, and this Error-severity rule
    /// reports only what is certain.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_AfterReassignmentFromFactory_NoDiagnostic()
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
                        BiDiDriver driver = new BiDiDriver();
                        driver = CreateDriver();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    private static BiDiDriver CreateDriver()
                    {
                        return new BiDiDriver();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that rebinding a tracked variable to a freshly constructed driver resets its started
    /// state: the start that preceded the reassignment applied to a driver the variable no longer
    /// names, so the command that follows runs against one that has never been started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_AfterReassignmentToNewDriver_ReportsError()
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
                        BiDiDriver driver = new BiDiDriver();
                        await driver.StartAsync("ws://localhost:1234");
                        driver = new BiDiDriver();
                        await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
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

    /// <summary>
    /// Tests that assignments that do not rebind a tracked variable — one whose target is a member
    /// access rather than a bare name, and one naming a variable the walk never tracked — leave the
    /// tracked state alone.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithUnrelatedAssignments_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private BiDiDriver stored;

                    private BiDiDriver untracked;

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        this.stored = new BiDiDriver();
                        untracked = new BiDiDriver();
                        await driver.StartAsync("ws://localhost:1234");
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a rebinding on one arm of an if statement stops the tracking after the merge. The
    /// variable's started state is unknown on that path, so nothing after the branch is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_AfterReassignmentInIfBranch_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool flag)
                    {
                        BiDiDriver driver = new BiDiDriver();
                        if (flag)
                        {
                            driver = CreateDriver();
                        }

                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    private static BiDiDriver CreateDriver()
                    {
                        return new BiDiDriver();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a rebinding inside a try block stops the tracking, both for the catch clause — which
    /// may begin executing after the rebinding has run — and for the code that follows the statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_AfterReassignmentInTryBlock_NoDiagnostic()
    {
        string testCode = """
            using System;
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
                        try
                        {
                            driver = CreateDriver();
                        }
                        catch (Exception)
                        {
                            await driver.ExecuteCommandAsync(new StatusCommandParameters());
                        }

                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    private static BiDiDriver CreateDriver()
                    {
                        return new BiDiDriver();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a rebinding inside a switch section stops the tracking after the merge, the same way
    /// one inside an if branch does.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_AfterReassignmentInSwitchSection_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int value)
                    {
                        BiDiDriver driver = new BiDiDriver();
                        switch (value)
                        {
                            case 1:
                                driver = CreateDriver();
                                break;

                            default:
                                break;
                        }

                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    private static BiDiDriver CreateDriver()
                    {
                        return new BiDiDriver();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_WhenALocalFunctionRebindsTheDriver_NoDiagnostic()
    {
        // A nested function runs when its delegate is invoked, so an assignment inside one leaves the
        // variable naming a driver this walk cannot see, which may already have been started.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(DriverPool pool)
                    {
                        BiDiDriver driver = new BiDiDriver();
                        async Task ConnectAsync()
                        {
                            driver = await pool.RentStartedAsync();
                        }

                        await ConnectAsync();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }

                public class DriverPool
                {
                    public Task<BiDiDriver> RentStartedAsync() => Task.FromResult(new BiDiDriver());
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_WhenALambdaRebindsTheDriver_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(DriverPool pool)
                    {
                        BiDiDriver driver = new BiDiDriver();
                        Func<Task> connect = async () => driver = await pool.RentStartedAsync();
                        await connect();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }

                public class DriverPool
                {
                    public Task<BiDiDriver> RentStartedAsync() => Task.FromResult(new BiDiDriver());
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_WhenALambdaAssignsTheDriverToAField_NoDiagnostic()
    {
        // The driver on the right of an assignment is handed to something else, and that is an escape
        // wherever it is written, including inside a nested function.
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private BiDiDriver? shared;

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        Action publish = () => this.shared = driver;
                        publish();
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a driver handed to other code through a wrapped mention — a cast, parentheses, the
    /// null-forgiving operator, or a conditional expression — escapes exactly as a bare mention does,
    /// so the commands that follow are not reported: the helper may have started it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Command_AfterDriverEscapesThroughWrappedArgument_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task CastAsync()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        await StartHelperAsync((IBiDiDriverLifecycleManager)driver);
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    public async Task ParenthesizedAsync()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        await StartHelperAsync((driver));
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    public async Task NullForgivenAsync()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        await StartHelperAsync(driver!);
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    public async Task ConditionalAsync(bool flag)
                    {
                        BiDiDriver driver = new BiDiDriver();
                        BiDiDriver other = new BiDiDriver();
                        await StartHelperAsync(flag ? driver : other);
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                        await other.ExecuteCommandAsync(new StatusCommandParameters());
                    }

                    private static async Task StartHelperAsync(IBiDiDriverLifecycleManager driver)
                    {
                        await driver.StartAsync("ws://localhost:1234");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a driver handed to an extension method is no longer tracked, because that method
    /// may start it and the walk cannot see into it -- the same reasoning as for a driver passed as
    /// an ordinary argument, which is what an extension method receiver is.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandAfterExtensionMethodStart_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public static class DriverExtensions
                {
                    public static Task StartWithRetryAsync(this BiDiDriver driver, string url)
                    {
                        return driver.StartAsync(url);
                    }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartWithRetryAsync("ws://localhost:9222");
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a command called through a local holding one of the driver's modules is judged
    /// against that driver, both before and after StartAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandThroughModuleAlias_BeforeStart_ReportsError()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    private int counter;

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        BrowsingContextModule context = driver.BrowsingContext;

                        // An assignment whose target is not a plain name: the scan for a rebinding of
                        // the alias has to look past it rather than mistake it for one.
                        this.counter = 1;

                        // An assignment to a different plain name, which the same scan must also
                        // distinguish from a rebinding of the alias.
                        int unrelated = 0;
                        unrelated = 2;
                        await {|#0:context.GetTreeAsync()|};
                        await driver.StartAsync("ws://localhost:9222");
                        await context.GetTreeAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        testState.ExpectedDiagnostics.Add(new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync"));

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests the cases an alias must not be resolved from: one rebound after its declaration, one
    /// declared without an initializer, one initialized from something other than a member of a
    /// driver, and a module held in a field rather than a local. Each must leave the call unreported,
    /// because the walk cannot say which driver it belongs to.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandThroughUnresolvableModuleReference_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class Holder
                {
                    public BrowsingContextModule Module { get; set; }
                }

                public class TestClass
                {
                    private BrowsingContextModule field;

                    private Holder holder = new();

                    public async Task Rebound()
                    {
                        BiDiDriver driver = new();
                        BiDiDriver other = new();
                        BrowsingContextModule context = driver.BrowsingContext;
                        context = other.BrowsingContext;
                        await context.GetTreeAsync();
                    }

                    public async Task NoInitializer()
                    {
                        BiDiDriver driver = new();
                        BrowsingContextModule context;
                        context = driver.BrowsingContext;
                        await context.GetTreeAsync();
                    }

                    public async Task NotAModuleOfADriver(Holder holder)
                    {
                        BiDiDriver driver = new();
                        BrowsingContextModule context = holder.Module;
                        await context.GetTreeAsync();
                    }

                    public async Task FromANestedMemberAccess()
                    {
                        BiDiDriver driver = new();
                        BrowsingContextModule context = this.holder.Module;
                        await context.GetTreeAsync();
                    }

                    public async Task FromAField()
                    {
                        BiDiDriver driver = new();
                        await field.GetTreeAsync();
                    }

                    public async Task FromAForeachVariable(BrowsingContextModule[] modules)
                    {
                        BiDiDriver driver = new();
                        foreach (BrowsingContextModule module in modules)
                        {
                            await module.GetTreeAsync();
                        }
                    }

                    public async Task ThroughALongerChain()
                    {
                        BiDiDriver driver = new();
                        await this.holder.Module.GetTreeAsync();
                    }

                    public async Task ThroughAnElementAccess(BrowsingContextModule[] modules)
                    {
                        BiDiDriver driver = new();
                        await modules[0].GetTreeAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CommandAfterForeachLoopThatStops_NoDiagnostic()
    {
        // The loop body may never run, so a stop inside it does not leave the driver certainly stopped. Walking the body straight through reported this command at Error severity.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string[] urls)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        foreach (string url in urls)
                        {
                            await driver.StopAsync();
                        }

                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandAfterForLoopThatStarts_NoDiagnostic()
    {
        // A start inside a for loop may have run, so the command after the loop is not reported.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int count)
                    {
                        BiDiDriver driver = new();
                        for (int attempt = 0; attempt < count; attempt++)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }

                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandInsideWhileLoopBeforeStart_ReportsError()
    {
        // Inside the body, the state is the state at loop entry, so a command before the body's own start is reported.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
                    {
                        BiDiDriver driver = new();
                        while (retry)
                        {
                            await {|#0:driver.Session.StatusAsync()|};
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
            .WithArguments("StatusAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task CommandAfterDoWhileLoopThatStops_ReportsError()
    {
        // A do...while body runs at least once, so a stop inside it certainly leaves the driver stopped.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        do
                        {
                            await driver.StopAsync();
                        }
                        while (retry);

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
    public async Task CommandAfterTryWhoseFinallyStops_ReportsError()
    {
        // A finally runs however the try ends, so its StopAsync certainly leaves the driver stopped for the command that follows.
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
                        try
                        {
                            await driver.Session.StatusAsync();
                        }
                        finally
                        {
                            await driver.StopAsync();
                        }

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
    public async Task CommandInFinallyAfterStopInTry_NoDiagnostic()
    {
        // The finally may run after an exception thrown before the StopAsync in the try, while the driver is still started, so the command in it is not reported.
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
                        try
                        {
                            await driver.Session.StatusAsync();
                            await driver.StopAsync();
                        }
                        finally
                        {
                            await driver.Session.StatusAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandAfterConditionalExpressionsThatMayStop_NoDiagnostic()
    {
        // Only one arm of a conditional expression runs, so a stop in either arm may not have run. Walked straight through, the stop reported the command at Error severity.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await (retry ? Task.CompletedTask : driver.StopAsync());
                        await (retry ? driver.StopAsync() : Task.CompletedTask);
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandAfterSwitchExpressionThatMayStop_NoDiagnostic()
    {
        // The arms of a switch expression are mutually exclusive, including one guarded by a when clause.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await (mode switch
                        {
                            1 when retry => driver.StopAsync(),
                            _ => Task.CompletedTask,
                        });
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandAfterEmptySwitchExpression_ReportsError()
    {
        // A switch expression with no arms always throws, so it adds no path and leaves the driver not started.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        #pragma warning disable CS8509
                        Task pending = mode switch { };
                        #pragma warning restore CS8509
                        await pending;
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
    public async Task CommandAfterCoalesceThatMayStop_NoDiagnostic()
    {
        // The right operand of ?? runs only when the left is null, so the stop in it may not have run.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        Task pending = null;
                        await driver.StartAsync("ws://localhost:9222");
                        await (pending ?? driver.StopAsync());
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandAfterCoalesceAssignmentThatMayStop_NoDiagnostic()
    {
        // The right operand of ??= runs only when the variable is null, so the stop in it may not have run.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        Task pending = null;
                        await driver.StartAsync("ws://localhost:9222");
                        pending ??= driver.StopAsync();
                        await pending;
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandAfterConditionalAndThatMayStop_NoDiagnostic()
    {
        // The right operand of && runs only when the left is true, so the stop in it may not have run.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        bool stopped = retry && driver.StopAsync().Wait(1000);
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task CommandAfterConditionalOrThatMayStart_NoDiagnostic()
    {
        // The right operand of || runs only when the left is false, so the start in it may have run, and this rule reports only a driver that is not started on every path.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry, int mode)
                    {
                        BiDiDriver driver = new();
                        bool started = retry || driver.StartAsync("ws://localhost:9222").Wait(1000);
                        await driver.Session.StatusAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver009_CommandExecutionBeforeStartAnalyzer>(testCode);
    }

    [Fact]
    public async Task Command_BeforeStartAsync_InTopLevelProgramDeclaringAModule_ReportsError()
    {
        // The hand-off scan covers only the program's own statements. The module declared in the same file passes
        // its constructor parameter, also named driver, to its base class; that is not the program's local driver
        // escaping, so the command before the start is still reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using TestApp;

            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
            driver.RegisterModule(new CustomModule(driver));
            await {|#0:driver.Session.StatusAsync()|};

            namespace TestApp
            {
                public class CustomModule : Module
                {
                    public CustomModule(IBiDiModuleHost driver) : base(driver) { }
                    public override string ModuleName => "custom";
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        RealAssemblyAnalyzerTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer> testState = new()
        {
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
