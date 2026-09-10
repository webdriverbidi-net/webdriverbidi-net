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

    /// <summary>
    /// The driver's own lifecycle operations return a bare <c>Task</c>, or a <c>ValueTask</c> for
    /// <c>DisposeAsync</c>, so the generic-<c>Task</c> test that identifies a module command does not
    /// admit them. Discarding one is at least as damaging: an un-awaited <c>StartAsync</c> leaves the
    /// connect racing the next command, and an un-awaited <c>StopAsync</c> discards the
    /// <c>AggregateException</c> carrying every error collected under
    /// <c>TransportErrorBehavior.Collect</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DriverLifecycleCalls_FireAndForget_ReportsError()
    {
        // The real assembly is referenced against .NET 10 here because RegisterTypeInfoResolverAsync's
        // parameter type comes from System.Text.Json, whose version in the .NET 8 reference set is
        // older than the one the library binds to.
        string testCode = """
            using System.Text.Json.Serialization.Metadata;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new();
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                        {|#1:driver.StartAsync("ws://localhost:9222")|};
                        {|#2:driver.StopAsync()|};
                        {|#3:driver.DisposeAsync()|};
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.AddRange(
        [
            Expect(0, "RegisterTypeInfoResolverAsync"),
            Expect(1, "StartAsync"),
            Expect(2, "StopAsync"),
            Expect(3, "DisposeAsync"),
        ]);

        await testState.RunAsync(TestContext.Current.CancellationToken);

        static DiagnosticResult Expect(int location, string methodName) => new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(location)
            .WithArguments(methodName);
    }

    /// <summary>
    /// A lifecycle call whose task is consumed — awaited, or captured for later — is not
    /// fire-and-forget, exactly as for a module command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DriverLifecycleCalls_Consumed_NoDiagnostic()
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
                        Task stopping = driver.StopAsync();
                        await stopping;
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// A lifecycle call whose ValueTask is wrapped and then discarded is still fire-and-forget: the
    /// chain is followed through the wrapper — to a Task from <c>AsTask</c>, to another ValueTask from
    /// <c>Preserve</c> — exactly as it is for a Task-returning command. A wrapper that is awaited is
    /// not discarded and is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DriverDisposeAsync_ChainedThenDiscarded_ReportsError()
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
                        {|#0:driver.DisposeAsync()|}.AsTask();
                        {|#1:driver.DisposeAsync()|}.Preserve();
                        await driver.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            """;

        DiagnosticResult[] expected =
        [
            Expect(0),
            Expect(1),
        ];

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected);

        static DiagnosticResult Expect(int location) => new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(location)
            .WithArguments("DisposeAsync");
    }

    /// <summary>
    /// A method carrying a lifecycle name on the driver but returning neither a Task nor a ValueTask
    /// has no completion to observe, so discarding its result is not this rule's subject.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DriverLifecycleNameOnNonAwaitableOverload_NoDiagnostic()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class CustomExecutor : IBiDiCommandExecutor
                {
                    public TimeSpanHolder Holder { get; } = new TimeSpanHolder();

                    public System.TimeSpan DefaultCommandTimeout => System.TimeSpan.Zero;

                    public bool IsStarted => false;

                    public Task StartAsync(string connectionString, CancellationToken cancellationToken = default) => Task.CompletedTask;

                    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

                    // An overload that answers immediately, so there is nothing to await.
                    public void StopAsync(int code) { }

                    public Task<T> ExecuteCommandAsync<T>(CommandParameters<T> commandParameters, System.TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
                        where T : CommandResult => Task.FromResult<T>(default!);

                    public Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters, System.TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
                        where T : CommandResult => Task.FromResult<T>(default!);

                    public void RegisterEvent<T>(string eventName, System.Func<EventInfo<T>, Task> eventInvoker) { }

                    public ValueTask DisposeAsync() => default;
                }

                public class TimeSpanHolder { }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        CustomExecutor executor = new();
                        executor.StopAsync(0);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// The lifecycle names are only the driver's own. A method of the same name on an unrelated type
    /// is not this rule's subject, which is what the containing-type test in the analyzer enforces.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonDriverMethodNamedLikeLifecycleOperation_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class CustomService
                {
                    public Task StartAsync() => Task.CompletedTask;

                    public Task StopAsync() => Task.CompletedTask;
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        CustomService service = new();
                        service.StartAsync();
                        service.StopAsync();
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
    public async Task ModuleCommandWithBlockingWait_NoDiagnostic()
    {
        // A chained .Wait() blocks until the command completes and propagates its
        // exceptions; the command is synchronously waited on, not fire-and-forget.
        // (Blocking waits are BIDI007/BIDI016 territory, not BIDI010's.)
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
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("contextId", "https://example.com")).Wait();
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

    /// <summary>
    /// Tests that a module command whose <c>Task&lt;T&gt;</c> result is implicitly converted to
    /// <c>Task</c> is treated as used in every position the conversion can appear in.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ConvertedResultInEveryUsedPosition_DoesNotReportDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    private static void Consume(Task task) { }

                    public Task AsExpressionBodiedReturn(BiDiDriver driver) => driver.Browser.CloseAsync();

                    public Task AsReturnStatement(BiDiDriver driver)
                    {
                        return driver.Browser.CloseAsync();
                    }

                    public void AsArgument(BiDiDriver driver)
                    {
                        Consume(driver.Browser.CloseAsync());
                    }

                    public void AsVariableInitializer(BiDiDriver driver)
                    {
                        Task task = driver.Browser.CloseAsync();
                        Consume(task);
                    }

                    public void AsSimpleAssignment(BiDiDriver driver)
                    {
                        Task task;
                        task = driver.Browser.CloseAsync();
                        Consume(task);
                    }

                    public async Task AsAwaitedConversionAsync(BiDiDriver driver)
                    {
                        await (Task)driver.Browser.CloseAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the result of <c>ConfigureAwait</c> on a module command counts as used when it is
    /// stored in a local rather than awaited directly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ConfigureAwaitResultStoredInLocal_DoesNotReportDiagnostic()
    {
        string test = """
            using System.Runtime.CompilerServices;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Browser;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task VariableInitializerAsync(BiDiDriver driver)
                    {
                        ConfiguredTaskAwaitable<CloseCommandResult> awaitable =
                            driver.Browser.CloseAsync().ConfigureAwait(false);
                        CloseCommandResult result = await awaitable;
                        System.Console.WriteLine(result);
                    }

                    public async Task SimpleAssignmentAsync(BiDiDriver driver)
                    {
                        ConfiguredTaskAwaitable<CloseCommandResult> awaitable;
                        awaitable = driver.Browser.CloseAsync().ConfigureAwait(false);
                        CloseCommandResult result = await awaitable;
                        System.Console.WriteLine(result);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ModuleCommandsPassedToTaskWhenAll_NoDiagnostic()
    {
        // Passing module command Tasks to Task.WhenAll consumes them (through the params array), so
        // they are not fire-and-forget.
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
                        await driver.StartAsync("ws://localhost:9222");
                        await Task.WhenAll(driver.BrowsingContext.GetTreeAsync(), driver.Script.GetRealmsAsync());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandsInCollectionExpression_NoDiagnostic()
    {
        // Elements of a collection expression are consumed by the collection, not fire-and-forget.
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
                        await driver.StartAsync("ws://localhost:9222");
                        Task[] tasks = [driver.BrowsingContext.GetTreeAsync(), driver.Script.GetRealmsAsync()];
                        await Task.WhenAll(tasks);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    [Fact]
    public async Task ModuleCommandsInArrayInitializer_NoDiagnostic()
    {
        // Elements of an array initializer are consumed by the array, not fire-and-forget.
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
                        await driver.StartAsync("ws://localhost:9222");
                        Task[] tasks = new Task[] { driver.BrowsingContext.GetTreeAsync(), driver.Script.GetRealmsAsync() };
                        await Task.WhenAll(tasks);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a user type deriving from an unrelated base class named <c>Module</c> is not treated
    /// as a WebDriver BiDi module. Matching the base type on its simple name alone claimed types this
    /// library has nothing to do with — a class deriving from <c>Autofac.Module</c>, for example — and
    /// reported an ordinary fire-and-forget call at Error severity.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetOnForeignModuleBaseClass_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace ThirdParty
            {
                public abstract class Module { }
            }

            namespace TestNamespace
            {
                using ThirdParty;

                public class RegistrationModule : Module
                {
                    public Task<int> LoadAsync() => Task.FromResult(0);
                }

                public class TestClass
                {
                    public void TestMethod(RegistrationModule registration)
                    {
                        registration.LoadAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a discarded <c>ExecuteCommandAsync</c> call on the driver is reported: it sends a command over the same connection a module command does.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_FireAndForget_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that a discarded call to a user method that merely shares the name <c>ExecuteCommandAsync</c> is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_OnUnrelatedType_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class Sender
                {
                    public Task<int> ExecuteCommandAsync() => Task.FromResult(1);
                }

                public class TestClass
                {
                    public void TestMethod(Sender sender)
                    {
                        sender.ExecuteCommandAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a discarded module command made through a null-conditional receiver (<c>driver?.Session.StatusAsync()</c>) is reported: the conditional access as a whole is what is discarded.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_ThroughNullConditionalReceiver_FireAndForget_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver? driver)
                    {
                        driver?{|#0:.Session.StatusAsync()|};
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that a module command made through a null-conditional receiver whose result is consumed is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommand_ThroughNullConditionalReceiver_Awaited_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver? driver)
                    {
                        Task<StatusCommandResult>? pending = driver?.Session.StatusAsync();
                        if (pending is not null)
                        {
                            await pending;
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a command on a custom module whose name does not end in "Module" is reported: any type deriving from the library's Module base class is a module.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleSubclassWithoutModuleSuffix_FireAndForget_ReportsError()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class GoogleCdp : Module
                {
                    public GoogleCdp(IBiDiCommandExecutor driver) : base(driver) { }
                    public override string ModuleName => "goog:cdp";
                    public Task<int> SendAsync() => Task.FromResult(1);
                }

                public class TestClass
                {
                    public void TestMethod(GoogleCdp cdp)
                    {
                        {|#0:cdp.SendAsync()|};
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("SendAsync");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer>(testCode, expected0);
    }
}
