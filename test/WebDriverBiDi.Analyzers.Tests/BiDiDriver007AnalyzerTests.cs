// <copyright file="BiDiDriver007AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver007 analyzer that detects blocking operations in event handlers.
/// </summary>
public class BiDiDriver007AnalyzerTests
{
    /// <summary>
    /// Tests that an options argument whose type is named ObservableEventHandlerOptions but is not
    /// backed by <see cref="int"/> (a same-named type from another assembly) is not mistaken for the
    /// real option, so the blocking call is still reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: this keeps a hand-written stub because the real
    /// <c>ObservableEventHandlerOptions</c> is <see cref="int"/>-backed. A <c>long</c>-backed enum with
    /// the same name is the only way to drive the option resolution's <c>constantValue.Value is int and 1</c>
    /// defensive branch (a value of <c>long</c> 1 is not <c>int</c> 1), which cannot be reproduced against
    /// the real assembly.
    /// </remarks>
    [Fact]
    public async Task EventHandler_WithNonIntBackedOptionsType_ReportsWarning()
    {
        // The option resolution matches the options type by name and reads its constant value; a value
        // that is not an int (here a long-backed enum) must not be treated as RunHandlerAsynchronously.
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public enum ObservableEventHandlerOptions : long
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions options) => new EventObserver<T>();
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

            namespace TestApp
            {
                using System.Threading;
                using WebDriverBiDi;

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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a blocking operation is still reported when the handler is registered with an explicit
    /// options value other than RunHandlerAsynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithExplicitNonAsyncOption_ReportsWarning()
    {
        // A handler registered with an options value that is a constant other than RunHandlerAsynchronously
        // (here RunHandlerSynchronously = 0) does not opt into asynchronous dispatch, so the blocking call
        // is still reported. This exercises the constant-but-not-1 path of the option resolution.
        string test = """
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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Thread.Sleep in event handlers reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithThreadSleep_ReportsWarning()
    {
        string test = """
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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Task.Wait in event handlers reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithTaskWait_ReportsWarning()
    {
        string test = """
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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Wait()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that .Result property access in event handlers reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithTaskResult_ReportsWarning()
    {
        string test = """
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
                            var task = Task.FromResult(42);
                            var value = {|#0:task.Result|};
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Result");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a non-async Task-returning lambda with RunHandlerAsynchronously still reports blocking
    /// operations, using the message that explains the option does not offload a synchronous body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_NonAsyncTaskLambda_WithRunHandlerAsynchronouslyOption_ReportsSynchronousBodyWarning()
    {
        string test = """
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

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithMessage("Blocking operation 'Sleep()' detected in event handler. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' does not offload the synchronous body of a Task-returning handler; make the handler 'async' and await before the blocking work, or move the work into Task.Run.");
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an async lambda with RunHandlerAsynchronously does not report a diagnostic, because
    /// the blocking work runs in a continuation off the dispatching thread.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_AsyncLambda_WithRunHandlerAsynchronouslyOption_NoDiagnostic()
    {
        string test = """
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

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a handler bound to the Action&lt;T&gt; overload with RunHandlerAsynchronously does not
    /// report a diagnostic, because the library queues the whole action to the thread pool.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_ActionLambda_WithRunHandlerAsynchronouslyOption_NoDiagnostic()
    {
        string test = """
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

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a method group resolving to an async method with RunHandlerAsynchronously does not
    /// report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_AsyncMethodGroup_WithRunHandlerAsynchronouslyOption_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(this.HandleAsync, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }

                    private async Task HandleAsync(EntryAddedEventArgs args)
                    {
                        await Task.Yield();
                        Thread.Sleep(1000);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a method group resolving to a non-async Task-returning method with RunHandlerAsynchronously
    /// still reports blocking operations in the method body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_NonAsyncTaskMethodGroup_WithRunHandlerAsynchronouslyOption_ReportsSynchronousBodyWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(this.Handle, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }

                    private Task Handle(EntryAddedEventArgs args)
                    {
                        {|#0:Thread.Sleep(1000)|};
                        return Task.CompletedTask;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithMessage("Blocking operation 'Sleep()' detected in event handler. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' does not offload the synchronous body of a Task-returning handler; make the handler 'async' and await before the blocking work, or move the work into Task.Run.");
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a delegate-typed property passed as the handler with RunHandlerAsynchronously does not
    /// report a diagnostic, because neither the handler kind nor its body can be resolved.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_DelegateProperty_WithRunHandlerAsynchronouslyOption_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private Func<EntryAddedEventArgs, Task> Handler { get; } = args => Task.CompletedTask;

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(this.Handler, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a handler wrapped in a cast expression with RunHandlerAsynchronously does not report a
    /// diagnostic, because the handler body cannot be resolved through the cast.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_CastLambda_WithRunHandlerAsynchronouslyOption_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver((Func<EntryAddedEventArgs, Task>)(args =>
                        {
                            Thread.Sleep(1000);
                            return Task.CompletedTask;
                        }), ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that GetAwaiter().GetResult() pattern reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithGetAwaiterGetResult_ReportsWarning()
    {
        string test = """
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
                            {|#0:task.GetAwaiter().GetResult()|};
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("GetResult()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-AddObserver invocations are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonAddObserverInvocation_NoDiagnostic()
    {
        // AddDataCollector is a real ObservableEvent method that is not AddObserver, so the analyzer
        // short-circuits on the member name and never inspects the handler body.
        string test = """
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
                        var collector = driver.Log.OnEntryAdded.AddDataCollector(args =>
                        {
                            Thread.Sleep(1000);
                            return true;
                        });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that method returning non-EventObserver is not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: the real <c>AddObserver</c> overloads always return
    /// <c>EventObserver&lt;T&gt;</c>. A stub whose <c>AddObserver</c> returns <c>void</c> is the only way to
    /// exercise the analyzer's <c>ReturnType is not INamedTypeSymbol { Name: "EventObserver" }</c> early-out,
    /// so it cannot be reproduced against the real assembly.
    /// </remarks>
    [Fact]
    public async Task MethodReturningNonEventObserver_NoDiagnostic()
    {
        string test = """
                using System;
                using System.Threading;
                using System.Threading.Tasks;

                namespace WebDriverBiDi
                {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public void AddObserver(Func<T, Task> handler) { }
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

            namespace TestApp
            {
                using System.Threading;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(args =>
                        {
                            Thread.Sleep(1000);
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that parenthesized lambda expressions are analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParenthesizedLambda_WithBlockingOperation_ReportsWarning()
    {
        string test = """
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
                        var observer = driver.Log.OnEntryAdded.AddObserver((args) =>
                        {
                            {|#0:Thread.Sleep(1000)|};
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple blocking operations report multiple warnings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithMultipleBlockingOperations_ReportsMultipleWarnings()
    {
        string test = """
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
                            {|#0:Thread.Sleep(100)|};
                            var task = Task.FromResult(42);
                            var result = {|#1:task.Result|};
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("Result");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that named methods with blocking operations report a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NamedMethod_WithThreadSleep_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private Task HandleLogEntry(EntryAddedEventArgs args)
                    {
                        {|#0:Thread.Sleep(1000)|};
                        return Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleLogEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that named methods with Task.Wait report a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NamedMethod_WithTaskWait_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private Task HandleLogEntry(EntryAddedEventArgs args)
                    {
                        var task = Task.Delay(100);
                        {|#0:task.Wait()|};
                        return Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleLogEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Wait()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that named methods with Task.Result report a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NamedMethod_WithTaskResult_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private Task HandleLogEntry(EntryAddedEventArgs args)
                    {
                        var task = Task.FromResult(42);
                        var value = {|#0:task.Result|};
                        return Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleLogEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Result");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that named methods with no blocking operations do not report a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NamedMethod_WithNoBlockingOperations_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private async Task HandleLogEntry(EntryAddedEventArgs args)
                    {
                        Console.WriteLine(args.Text);
                        await Task.Delay(100);
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleLogEntry);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that member access method references with blocking operations report a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MemberAccessMethodReference_WithBlockingOperation_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private Task HandleLogEntry(EntryAddedEventArgs args)
                    {
                        {|#0:Thread.Sleep(500)|};
                        return Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(this.HandleLogEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI007()
    {
        BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer analyzer = new();
        System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.DiagnosticDescriptor> diagnostics = analyzer.SupportedDiagnostics;

        // Two descriptors share the ID: the default message and the message used when
        // RunHandlerAsynchronously is present but the handler body is synchronous.
        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, descriptor => Assert.Equal(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, descriptor.Id));
    }

    /// <summary>
    /// Tests GetFixAllProvider property.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider provider = new();
        Microsoft.CodeAnalysis.CodeFixes.FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.NotNull(fixAllProvider);
        Assert.Equal(Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI007()
    {
        BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider provider = new();
        System.Collections.Immutable.ImmutableArray<string> ids = provider.FixableDiagnosticIds;

        Assert.Single(ids);
        Assert.Equal(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, ids[0]);
    }

    /// <summary>
    /// Tests that AddObserver invoked via delegate is not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: the real <c>AddObserver</c> overloads carry optional parameters, so the
    /// method group cannot convert to a single-argument <c>Func&lt;Func&lt;T, Task&gt;, EventObserver&lt;T&gt;&gt;</c>.
    /// A single-parameter stub overload is the only way to bind <c>AddObserver</c> to such a delegate and
    /// exercise the "invocation is not a member access" early-out.
    /// </remarks>
    [Fact]
    public async Task AddObserver_ViaDelegate_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
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

            namespace TestApp
            {
                using System.Threading;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Func<Func<LogEntryAddedEventArgs, Task>, EventObserver<LogEntryAddedEventArgs>> addObserverFunc = driver.Log.OnEntryAdded.AddObserver;
                        var observer = addObserverFunc(args =>
                        {
                            Thread.Sleep(1000);
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with unresolved method symbol is not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: the real <c>ObservableEvent&lt;T&gt;</c> always exposes <c>AddObserver</c>, so
    /// the call always binds to a method symbol. A stub whose <c>ObservableEvent</c> declares no
    /// <c>AddObserver</c> is the only way to drive the "no method symbol" early-out (the call then reports
    /// CS1061), which cannot be reproduced against the real assembly.
    /// </remarks>
    [Fact]
    public async Task AddObserver_UnresolvedMethod_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    // No AddObserver method defined
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

            namespace TestApp
            {
                using System.Threading;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.{|CS1061:AddObserver|}(args =>
                        {
                            Thread.Sleep(1000);
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with no arguments is not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: every real <c>AddObserver</c> overload requires a handler argument, so a
    /// zero-argument call does not compile. A stub with a parameterless <c>AddObserver</c> is the only way
    /// to reach the analyzer's "no handler argument" early-out, which cannot be reproduced against the real
    /// assembly.
    /// </remarks>
    [Fact]
    public async Task AddObserver_NoArguments_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver() => new EventObserver<T>();
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

            namespace TestApp
            {
                using System.Threading;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that handler with expression body method is analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExpressionBodyMethod_WithBlockingOperation_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private Task HandleLogEntry(EntryAddedEventArgs args)
                    {
                        {|#0:Thread.Sleep(100)|};
                        return Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleLogEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that local function with blocking operation reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LocalFunction_WithBlockingOperation_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Task HandleLogEntry(EntryAddedEventArgs args)
                        {
                            {|#0:Thread.Sleep(100)|};
                            return Task.CompletedTask;
                        }

                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleLogEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocations with unresolved method symbols in handler body are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandlerBody_WithUnresolvedMethodInvocation_NoDiagnostic()
    {
        string test = """
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
                            {|CS0103:NonExistentMethod|}();
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that simple lambda expression without block body is handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SimpleLambdaExpression_WithoutBlockBody_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that expression with Task.Result in simple expression body is detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SimpleLambda_WithExpressionBodyTaskResult_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var task = Task.FromResult(42);
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.FromResult({|#0:task.Result|}));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Result");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that passing a method reference from a compiled assembly (no source, so no
    /// syntax references) does not report a diagnostic — exercises GetMethodBodyFromSymbol
    /// returning null when DeclaringSyntaxReferences is empty (AnalyzerSymbolHelpers lines
    /// 114 and 122).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by construction: this test compiles a purpose-built <c>FakeLib</c> assembly so the handler
    /// method symbol has no <c>DeclaringSyntaxReferences</c> when analyzed from the test project. It does not
    /// use hand-written <c>WebDriverBiDi</c> stub types in the analyzed source.
    /// </remarks>
    [Fact]
    public async Task AddObserver_WithCompiledAssemblyMethodReference_DoesNotReportDiagnostic()
    {
        // Build a compiled assembly containing the handler method, so the symbol has
        // no DeclaringSyntaxReferences when analyzed from the test project.
        string librarySource = """
            using System;
            using System.Threading.Tasks;

            namespace FakeLib
            {
                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                }

                public class LogModule
                {
                    public ObservableEvent<LogEntryAddedEventArgs> OnEntryAdded { get; } = new();
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new();
                }

                public class HandlerHelper
                {
                    public static Task Handle(LogEntryAddedEventArgs e) => Task.CompletedTask;
                }
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(librarySource, cancellationToken: TestContext.Current.CancellationToken);
        ImmutableArray<MetadataReference> netRefs = await ReferenceAssemblies.Net.Net80.ResolveAsync(LanguageNames.CSharp, TestContext.Current.CancellationToken);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "FakeHandlerLib",
            [tree],
            netRefs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream stream = new();
        compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        stream.Position = 0;
        MetadataReference libRef = MetadataReference.CreateFromStream(stream);

        string testCode = """
            using System;
            using System.Threading.Tasks;
            using FakeLib;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        // Handler is a method from a compiled assembly — no syntax references
                        using EventObserver<LogEntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(HandlerHelper.Handle);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(libRef);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an expression-bodied local function reference as handler is analysed —
    /// exercises the LocalFunctionStatementSyntax ExpressionBody path of
    /// GetMethodBodyFromSymbol (AnalyzerSymbolHelpers line 113 false arm, ExpressionBody).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithExpressionBodiedLocalFunctionReference_NoBlockingOp_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        // Expression-bodied local function (null Body, non-null ExpressionBody).
                        Task Handle(EntryAddedEventArgs e) => Task.CompletedTask;

                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(Handle);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a late-bound <c>GetAwaiter().GetResult()</c> chain is not reported. Because the
    /// receiver is <c>dynamic</c>, the whole chain is late-bound, so the outer <c>GetResult</c>
    /// call itself binds to no method symbol and the rule stops before it inspects the receiver.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithGetResultOnDynamicGetAwaiter_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                // Late-bound: neither GetAwaiter() nor GetResult() binds to a method.
                                dynamic d = Task.CompletedTask;
                                d.GetAwaiter().GetResult();
                                await Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Tests GetResult on non-GetAwaiter method (exercises false branch).</summary>
    [Fact]
    public async Task EventHandler_WithGetResultOnNonGetAwaiterMethod_DoesNotReportDiagnostic()
    {
        // GetResult called where the preceding call is NOT named "GetAwaiter" —
        // exercises getAwaiterSymbol?.Name != "GetAwaiter" (line 141 false branch).
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public struct CustomRunner
                {
                    public int GetResult() => 0;
                }

                public struct CustomHolder
                {
                    // Method is NOT named "GetAwaiter" — exercises line 141 false branch.
                    public CustomRunner PrepareResult() => new CustomRunner();
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                // PrepareResult().GetResult() — predecessor method is NOT "GetAwaiter".
                                CustomHolder h = new CustomHolder();
                                int result = h.PrepareResult().GetResult();
                                await Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that accessing <c>.Result</c> on a <c>dynamic</c> receiver does not report a
    /// diagnostic. A <c>dynamic</c> expression does have a type symbol, but its name is "dynamic"
    /// rather than "Task", so the receiver-type check rejects it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithResultOnDynamicType_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                // The receiver's type symbol is `dynamic`, which is not `Task`.
                                dynamic d = Task.FromResult(42);
                                var result = d.Result;
                                await Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Tests .Result on non-Task type (name not "Task" — false branch).</summary>
    [Fact]
    public async Task EventHandler_WithResultOnNonTaskType_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class Container
                {
                    public int Result { get; set; }
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                // .Result on a Container (not Task) — expressionType.Name != "Task".
                                Container c = new Container();
                                int val = c.Result;
                                await Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a delegate variable as handler does not report a diagnostic.
    /// </summary>
    [Fact]
    public async Task AddObserver_WithDelegateVariable_DoesNotReportDiagnostic()
    {
        // The handler is stored in a local variable of type Func<T, Task>.
        // GetHandlerBody cannot resolve it (it's not a lambda or a method ref) → returns null.
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver, Func<EntryAddedEventArgs, Task> handler)
                    {
                        // handler is a parameter — GetHandlerBody returns null for it.
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(handler);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Thread.Join (not Thread.Sleep) in a handler does not report a
    /// diagnostic — exercises the Thread.methodName != "Sleep" short-circuit (line 124).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithThreadJoin_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                Thread thread = new Thread(() => { });
                                thread.Join();
                                await Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Task.WaitAll (not Task.Wait) in a handler does not report a
    /// diagnostic — exercises the Task.methodName != "Wait" short-circuit (line 130).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithTaskDelay_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                // Task.Delay (not Task.Wait) — exercises line 130 false branch.
                                await Task.Delay(1);
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that calling AddObserver on a type that returns something other than
    /// EventObserver does not report a diagnostic — exercises the
    /// methodSymbol.ReturnType.Name != "EventObserver" branch (line 76).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: the real <c>AddObserver</c> overloads always return
    /// <c>EventObserver&lt;T&gt;</c>. A stub whose <c>AddObserver</c> returns a different named type
    /// (<c>SomeTracker</c>) is the only way to exercise the <c>ReturnType.Name != "EventObserver"</c> branch.
    /// </remarks>
    [Fact]
    public async Task AddObserver_ReturningNonObserverType_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class SomeTracker { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    // Returns SomeTracker instead of EventObserver<T>.
                    public SomeTracker AddObserver(Func<T, Task> handler) => new SomeTracker();
                }

                public class LogModule
                {
                    public ObservableEvent<LogEntryAddedEventArgs> OnEntryAdded { get; } = new();
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) => {
                            await Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an expression-bodied method reference handler is analysed — exercises
    /// the MethodDeclarationSyntax expression-body path (Body == null, ExpressionBody != null)
    /// of GetMethodBodyFromSymbol (AnalyzerSymbolHelpers line 113/114).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithExpressionBodiedMethodReference_NoBlockingOp_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    // Expression-bodied method reference — MethodDeclarationSyntax with null Body.
                    private static Task HandleEntry(EntryAddedEventArgs e) => Task.CompletedTask;

                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(HandleEntry);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a local-function method reference passed as handler is analysed for
    /// blocking operations — exercises the LocalFunctionStatementSyntax branch of
    /// GetMethodBodyFromSymbol (AnalyzerSymbolHelpers line 113).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithLocalFunctionReference_WithBlockingOp_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        // Local function as handler — exercises LocalFunctionStatementSyntax path.
                        async Task Handle(EntryAddedEventArgs e)
                        {
                            {|#0:Thread.Sleep(100)|};
                        }

                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(Handle);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Sleep()");

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a handler resolved to a declaration with neither a block body nor an expression
    /// body (an interface method declaration) is skipped rather than crashing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithInterfaceMethodReference_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public interface IHandler
                {
                    // A declaration only: Body and ExpressionBody are both null.
                    Task HandleAsync(EntryAddedEventArgs e);
                }

                public class TestClass
                {
                    public void Setup(BiDiDriver driver, IHandler handler)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(handler.HandleAsync);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a <c>GetResult()</c> call whose receiver is an invocation other than
    /// <c>GetAwaiter()</c>, and a <c>.Result</c> access on a type other than <c>Task</c>, are both
    /// left alone.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LookalikeGetResultAndResultMembers_DoNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class Holder
                {
                    public string Result { get; } = string.Empty;

                    public void GetResult() { }
                }

                public class TestClass
                {
                    private static Holder GetHolder() => new Holder();

                    public void Setup(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(e =>
                            {
                                // GetResult() whose receiver is an invocation that is not GetAwaiter().
                                GetHolder().GetResult();

                                // A .Result access on a type that is not Task.
                                Holder holder = new Holder();
                                string value = holder.Result;
                                System.Console.WriteLine(value);
                                return Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a member access named <c>Result</c> whose qualifier is a namespace is skipped.
    /// The name check is purely syntactic, so the rule then asks for the qualifier's type, and a
    /// namespace has none.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ResultMemberOnNamespaceQualifier_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public static class Result
                {
                    public static string Value { get; } = string.Empty;
                }

                public class TestClass
                {
                    public void Setup(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(e =>
                            {
                                // Within `TestApp.Result.Value`, the inner member access is named
                                // "Result" and its qualifier is the TestApp namespace.
                                string value = TestApp.Result.Value;
                                System.Console.WriteLine(value);
                                return Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that <c>GetResult()</c> called on a function-pointer invocation is skipped. A function
    /// pointer call has a result type, so the outer <c>GetResult</c> binds, but the call itself has
    /// no method symbol for the rule to inspect.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetResultOnFunctionPointerInvocation_DoesNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class Holder
                {
                    public void GetResult() { }
                }

                public class TestClass
                {
                    private static Holder MakeHolder() => new Holder();

                    public unsafe void Setup(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(e =>
                            {
                                // A function-pointer invocation has a result type but no method symbol.
                                delegate*<Holder> factory = &MakeHolder;
                                factory().GetResult();
                                return Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.SolutionTransforms.Add((solution, projectId) =>
        {
            CSharpCompilationOptions options =
                (CSharpCompilationOptions)solution.GetProject(projectId)!.CompilationOptions!;
            return solution.WithProjectCompilationOptions(projectId, options.WithAllowUnsafe(true));
        });

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a method named AddObserver whose return type is not a named type does not crash the
    /// analyzer and reports nothing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Synthetic by necessity: the real <c>AddObserver</c> overloads return the named type
    /// <c>EventObserver&lt;T&gt;</c>. A name-only lookalike whose <c>AddObserver</c> returns an array is the
    /// only way to reach the <c>ReturnType is not INamedTypeSymbol</c> guard, so it cannot be reproduced
    /// against the real assembly.
    /// </remarks>
    [Fact]
    public async Task AddObserver_WithNonNamedReturnType_NoDiagnostic()
    {
        // A method named AddObserver whose return type is not a named type (here an array) must not
        // crash the analyzer (an unchecked cast to INamedTypeSymbol would surface as AD0001); it is
        // simply not the library's EventObserver-returning AddObserver, so nothing is reported.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class Widget
                {
                    public int[] AddObserver(Func<int, Task> handler) => new int[0];
                }

                public class TestClass
                {
                    public void TestMethod(Widget widget)
                    {
                        var result = widget.AddObserver(async x => { await Task.CompletedTask; });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an async handler whose RunHandlerAsynchronously option is passed through a variable is
    /// not falsely reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_AsyncWithOptionViaVariable_NoDiagnostic()
    {
        // The RunHandlerAsynchronously option is passed through a local variable rather than a direct
        // member reference. The analyzer must still recognize the option (resolving it semantically,
        // not by source text) so an async handler is not falsely reported.
        string test = """
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
                        var options = ObservableEventHandlerOptions.RunHandlerAsynchronously;
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            Thread.Sleep(1000);
                            await Task.CompletedTask;
                        }, options);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
