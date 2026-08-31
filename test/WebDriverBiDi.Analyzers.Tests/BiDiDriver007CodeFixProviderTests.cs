// <copyright file="BiDiDriver007CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver007 code fix provider.
/// </summary>
public class BiDiDriver007CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix adds RunHandlerAsynchronously option.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithThreadSleep_CodeFixAddsRunHandlerAsynchronously()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            {|#0:Thread.Sleep(1000)|};
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await Task.Yield();
                            Thread.Sleep(1000);
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyCodeFixTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix replaces existing options parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithExistingOptions_CodeFixReplacesWithRunHandlerAsynchronously()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            {|#0:Thread.Sleep(1000)|};
                            return Task.CompletedTask;
                        }, ObservableEventHandlerOptions.RunHandlerSynchronously);
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await Task.Yield();
                            Thread.Sleep(1000);
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyCodeFixTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix works with Task.Wait.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithTaskWait_CodeFixAddsRunHandlerAsynchronously()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            var task = Task.Delay(100);
                            {|#0:task.Wait()|};
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await Task.Yield();
                            var task = Task.Delay(100);
                            task.Wait();
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Wait()");

        RealAssemblyCodeFixTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that when RunHandlerAsynchronously is already present on a non-async Task-returning lambda,
    /// the code fix converts the lambda to async (awaiting Task.Yield first) and keeps the option.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_NonAsyncTaskLambda_WithRunHandlerAsynchronously_CodeFixMakesHandlerAsync()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            {|#0:Thread.Sleep(1000)|};
                            return Task.CompletedTask;
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await Task.Yield();
                            Thread.Sleep(1000);
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithMessage("Blocking operation 'Sleep()' detected in event handler. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' does not offload the synchronous body of a Task-returning handler; make the handler 'async' and await before the blocking work, or move the work into Task.Run.");

        RealAssemblyCodeFixTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a handler bound to the Action&lt;T&gt; overload only needs the option added, because the
    /// library queues the whole action to the thread pool.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_ActionLambda_CodeFixAddsRunHandlerAsynchronously()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            {|#0:Thread.Sleep(1000)|};
                        });
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            Thread.Sleep(1000);
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyCodeFixTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the async conversion rewrites nested 'return Task.CompletedTask;' to 'return;', nested
    /// 'return &lt;task&gt;;' to an awaited block, a trailing 'return &lt;task&gt;;' to 'await &lt;task&gt;;',
    /// and leaves nested lambdas and local functions untouched.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_NonAsyncTaskLambdaWithNestedReturns_CodeFixRewritesReturns()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            Func<Task> nested = () => { return Task.CompletedTask; };
                            Task Local() { return Task.CompletedTask; }
                            if (args == null)
                            {
                                return Task.CompletedTask;
                            }

                            if (args.ToString() == "skip")
                            {
                                return Task.Delay(1);
                            }

                            {|#0:Thread.Sleep(1000)|};
                            return nested();
                        });
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await Task.Yield();
                            Func<Task> nested = () => { return Task.CompletedTask; };
                            Task Local() { return Task.CompletedTask; }
                            if (args == null)
                            {
                                return;
                            }

                            if (args.ToString() == "skip")
                            {
                                {
                                    await Task.Delay(1);
                                    return;
                                }
                            }

                            Thread.Sleep(1000);
                            await nested();
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyCodeFixTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no code fix is offered (and no exception is thrown) when the diagnostic is reported
    /// inside a method passed as a method group, because the diagnostic is not enclosed by the
    /// AddObserver invocation; the provider is invoked directly because the diagnostic is non-local.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: this drives the code fix through <see cref="AnalyzerTestHelpers.GetCodeActionsAsync{TAnalyzer, TCodeFix}"/>,
    /// which builds an ad-hoc workspace that references only the base framework assemblies (not the real
    /// <c>WebDriverBiDi</c> assembly), so the analyzed source must supply its own stub types.
    /// </remarks>
    [Fact]
    public async Task EventHandler_AsyncMethodGroup_OffersNoCodeFix()
    {
        string source = BothOverloadStubs + """
            namespace TestApp
            {
                using System.Threading;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(this.HandleAsync);
                    }

                    private async Task HandleAsync(LogEntryAddedEventArgs args)
                    {
                        await Task.Yield();
                        Thread.Sleep(1000);
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, _) = await AnalyzerTestHelpers
            .GetCodeActionsAsync<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider>(source);

        Assert.Empty(actions);
    }

    /// <summary>
    /// Tests that no code fix is offered for a method group resolving to a non-async Task-returning
    /// method (reported with the synchronous-body message), because the method declaration itself
    /// would have to change.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: this drives the code fix through <see cref="AnalyzerTestHelpers.GetCodeActionsAsync{TAnalyzer, TCodeFix}"/>,
    /// which builds an ad-hoc workspace that references only the base framework assemblies (not the real
    /// <c>WebDriverBiDi</c> assembly), so the analyzed source must supply its own stub types.
    /// </remarks>
    [Fact]
    public async Task EventHandler_NonAsyncTaskMethodGroup_OffersNoCodeFix()
    {
        string source = BothOverloadStubs + """
            namespace TestApp
            {
                using System.Threading;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(this.Handle, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }

                    private Task Handle(LogEntryAddedEventArgs args)
                    {
                        Thread.Sleep(1000);
                        return Task.CompletedTask;
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, _) = await AnalyzerTestHelpers
            .GetCodeActionsAsync<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider>(source);

        Assert.Empty(actions);
    }

    /// <summary>
    /// In-source stand-ins for the driver, log module, and observable-event types used by the code-fix
    /// tests that run through <see cref="AnalyzerTestHelpers.GetCodeActionsAsync{TAnalyzer, TCodeFix}"/>.
    /// That helper builds an ad-hoc workspace referencing only the base framework assemblies, so those
    /// tests cannot use the real <c>WebDriverBiDi</c> assembly and rely on these stubs instead.
    /// </summary>
    private const string BothOverloadStubs = """
        using System;
        using System.Threading;
        using System.Threading.Tasks;

        namespace WebDriverBiDi
        {
            public class WebDriverBiDiEventArgs { }

            public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

            public enum ObservableEventHandlerOptions
            {
                None = 0,
                RunHandlerAsynchronously = 1
            }

            public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
            {
                public void Dispose() { }
            }

            public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
            {
                public EventObserver<T> AddObserver(Action<T> handler, ObservableEventHandlerOptions options = ObservableEventHandlerOptions.None) => new EventObserver<T>();
                public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions options = ObservableEventHandlerOptions.None) => new EventObserver<T>();
            }

            public class LogModule
            {
                public ObservableEvent<LogEntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<LogEntryAddedEventArgs>();
            }

            public class BiDiDriver
            {
                public LogModule Log { get; } = new LogModule();
            }
        }

        """;
}
