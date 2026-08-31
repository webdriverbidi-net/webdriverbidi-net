// <copyright file="BiDiDriver016AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
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
/// Tests for the BiDiDriver016 analyzer that detects deadlock-prone synchronization patterns in event handlers.
/// </summary>
public class BiDiDriver016AnalyzerTests
{
    /// <summary>
    /// Tests that lock statement in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LockStatement_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new object();
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:lock (lockObj)
                            {
                                // Deadlock-prone: holding lock in async context
                                var x = 1 + 1;
                            }|}
                            await Task.Delay(100);
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("lock statement");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Monitor.Enter in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MonitorEnter_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new object();
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:Monitor.Enter(lockObj)|};
                            try
                            {
                                await Task.Delay(100);
                            }
                            finally
                            {
                                Monitor.Exit(lockObj);
                            }
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Monitor.Enter");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SemaphoreSlim.Wait in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SemaphoreSlimWait_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(1);
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:semaphore.Wait()|};
                            try
                            {
                                await Task.Delay(100);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SemaphoreSlim.Wait");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SemaphoreSlim.WaitAsync in async event handler does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SemaphoreSlimWaitAsync_InAsyncEventHandler_NoDiagnostic()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(1);
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                await Task.Delay(100);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that lock statement in non-async event handler does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LockStatement_InNonAsyncEventHandler_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new object();
                        driver.Log.OnEntryAdded.AddObserver((e) =>
                        {
                            lock (lockObj)
                            {
                                // Non-async handler, so lock is less problematic
                            }
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that deadlock patterns with RunHandlerAsynchronously do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LockStatement_WithRunHandlerAsynchronously_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new object();
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            lock (lockObj)
                            {
                                // Safe with RunHandlerAsynchronously
                                var x = 1 + 1;
                            }
                            await Task.Delay(100);
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that ManualResetEvent.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ManualResetEventWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        ManualResetEvent resetEvent = new ManualResetEvent(false);
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:resetEvent.WaitOne()|};
                            await Task.Delay(100);
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitHandle.WaitOne");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Task.WaitAll in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TaskWaitAll_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            var task1 = Task.Delay(100);
                            var task2 = Task.Delay(200);
                            {|#0:Task.WaitAll(task1, task2)|};
                            await Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Task.WaitAll");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple deadlock patterns are all detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleDeadlockPatterns_InAsyncEventHandler_ReportsMultipleWarnings()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new object();
                        SemaphoreSlim semaphore = new SemaphoreSlim(1);
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:lock (lockObj)
                            {
                                // Deadlock-prone: holding lock in async context
                                var x = 1 + 1;
                            }|}

                            {|#1:semaphore.Wait()|};
                            try
                            {
                                await Task.Delay(100);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("lock statement");

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("SemaphoreSlim.Wait");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Monitor.TryEnter in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MonitorTryEnter_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new();
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            if ({|#0:Monitor.TryEnter(lockObj)|})
                            {
                                try { await Task.Delay(1); }
                                finally { Monitor.Exit(lockObj); }
                            }
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Monitor.TryEnter");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Task.WaitAny in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TaskWaitAny_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            var t1 = Task.Delay(100);
                            var t2 = Task.Delay(200);
                            {|#0:Task.WaitAny(t1, t2)|};
                            await Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Task.WaitAny");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Mutex.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MutexWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Mutex mutex = new();
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:mutex.WaitOne()|};
                            await Task.Delay(1);
                            mutex.ReleaseMutex();
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitHandle.WaitOne");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AutoResetEvent.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AutoResetEventWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        AutoResetEvent resetEvent = new(false);
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:resetEvent.WaitOne()|};
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitHandle.WaitOne");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SynchronizationContext.Send in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SynchronizationContextSend_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            var ctx = SynchronizationContext.Current;
                            if (ctx != null)
                            {
                                {|#0:ctx.Send(_ => { }, null)|};
                            }
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SynchronizationContext.Send");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Semaphore.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SemaphoreWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Semaphore semaphore = new(1, 1);
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:semaphore.WaitOne()|};
                            await Task.Delay(1);
                            semaphore.Release();
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitHandle.WaitOne");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-async event handler with sync lambda does not report diagnostic even with lock.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LockStatement_InSyncLambdaHandler_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new();
                        driver.Log.OnEntryAdded.AddObserver((e) =>
                        {
                            lock (lockObj)
                            {
                                // Non-async handler, lock is acceptable
                            }
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocations that are not AddObserver are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly. The real observable-event
    /// type exposes no <c>Func&lt;T, Task&gt;</c>-taking method other than <c>AddObserver</c>, so a
    /// differently-named subscribe method (here <c>Subscribe</c>) is the only way to exercise the
    /// analyzer's <c>Name != "AddObserver"</c> early-return branch and cannot be reproduced against
    /// the real API.
    /// </remarks>
    [Fact]
    public async Task NonAddObserverInvocation_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public void Subscribe(Func<T, Task> handler) { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new();
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.Subscribe(async (e) =>
                        {
                            lock (lockObj) { }
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocations without member access are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Uses a local function named <c>AddObserver</c> (no member-access receiver) to exercise the
    /// analyzer's <c>invocation.Expression is not MemberAccessExpressionSyntax</c> early-return
    /// branch; the real API is always invoked through a member access, so this pure-syntax scenario
    /// cannot be reproduced against it.
    /// </remarks>
    [Fact]
    public async Task InvocationWithoutMemberAccess_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        void AddObserver(Func<int, Task> handler) { }
                        AddObserver(async (e) =>
                        {
                            var lockObj = new object();
                            lock (lockObj) { }
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver that doesn't return EventObserver is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly. The real <c>AddObserver</c>
    /// always returns <c>EventObserver&lt;T&gt;</c>, so an <c>AddObserver</c> whose return type is
    /// something else (here <c>string</c>) is the only way to exercise the analyzer's
    /// <c>ReturnType is not EventObserver</c> early-return branch and cannot be reproduced against the
    /// real API.
    /// </remarks>
    [Fact]
    public async Task AddObserverWithDifferentReturnType_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public string AddObserver(Func<T, Task> handler) => "subscribed";
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new();
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            lock (lockObj) { }
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with non-EventObserver return type is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// Uses a user type whose <c>AddObserver</c> returns <c>void</c> to exercise the analyzer's
    /// <c>ReturnType is not EventObserver</c> early-return branch; the real <c>AddObserver</c> always
    /// returns <c>EventObserver&lt;T&gt;</c>, so this cannot be reproduced against the real API.
    /// </remarks>
    [Fact]
    public async Task AddObserverWithWrongReturnType_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class CustomClass
                {
                    public void AddObserver(Func<int, Task> handler) { }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var obj = new CustomClass();
                        obj.AddObserver(async (e) =>
                        {
                            var lockObj = new object();
                            lock (lockObj) { }
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with no arguments is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly. The real <c>AddObserver</c>
    /// always takes a handler argument, so a no-argument overload is the only way to exercise the
    /// analyzer's empty-argument-list early-return branch and cannot be reproduced against the real
    /// API.
    /// </remarks>
    [Fact]
    public async Task AddObserverWithNoArguments_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver() => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs { }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that handler without body (method group) is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandlerWithoutBody_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task HandleEventAsync(EntryAddedEventArgs e) => await Task.Delay(1);

                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(HandleEventAsync);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that method reference (not lambda) with async modifier is not detected (analyzer limitation - IsAsyncHandler only checks lambdas).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MethodReference_WithLockStatement_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly object lockObj = new();

                    public async Task HandleEventAsync(EntryAddedEventArgs e)
                    {
                        lock (lockObj)
                        {
                            // Method references are not detected as async by IsAsyncHandler
                        }
                        await Task.Delay(1);
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(HandleEventAsync);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that local function reference (not lambda) with async modifier is not detected (analyzer limitation - IsAsyncHandler only checks lambdas).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LocalFunctionReference_WithLockStatement_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly object lockObj = new();

                    public void TestMethod(BiDiDriver driver)
                    {
                        async Task HandleEventAsync(EntryAddedEventArgs e)
                        {
                            lock (lockObj)
                            {
                                // Method references are not detected as async by IsAsyncHandler
                            }
                            await Task.Delay(1);
                        }

                        driver.Log.OnEntryAdded.AddObserver(HandleEventAsync);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that WaitHandle.WaitOne (base class) in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WaitHandleWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            WaitHandle handle = new ManualResetEvent(false);
                            {|#0:handle.WaitOne()|};
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitHandle.WaitOne");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that simple lambda (non-parenthesized) with async works correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SimpleLambdaAsync_WithLockStatement_ReportsWarning()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new();
                        driver.Log.OnEntryAdded.AddObserver(async e =>
                        {
                            {|#0:lock (lockObj) { }|}
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("lock statement");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with a non-constant RunHandlerAsynchronously option does not report
    /// diagnostics — exercises the non-constant option path of HasRunHandlerAsynchronouslyOption,
    /// where the argument cannot be resolved to a compile-time constant and is treated as present.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithRunHandlerAsynchronously_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new();
                        ObservableEventHandlerOptions options = ObservableEventHandlerOptions.RunHandlerAsynchronously;
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            lock (lockObj) { }
                            await Task.Delay(1);
                        }, options);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that method reference to a property does not cause errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task PropertyReference_AsHandler_NoDiagnostic()
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
                    public Func<EntryAddedEventArgs, Task> Handler { get; } = async (e) => await Task.Delay(1);

                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(Handler);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that unresolved invocations inside handler body do not cause errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UnresolvedInvocationInHandlerBody_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|CS0103:UndefinedMethod|}();
                            await Task.Delay(1);
                        });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a lock in an async handler still reports even when a non-RunHandlerAsynchronously
    /// option (RunHandlerSynchronously) is present — exercises the HasRunHandlerAsynchronouslyOption
    /// fallthrough when the option argument is present but is not RunHandlerAsynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LockStatement_WithObservableEventHandlerOptionsSynchronous_ReportsWarning()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        object lockObj = new object();
                        driver.Log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:lock (lockObj)
                            {
                                var x = 1 + 1;
                            }|}
                            await Task.Delay(100);
                        }, ObservableEventHandlerOptions.RunHandlerSynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("lock statement");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that passing an async method reference from a compiled assembly as the handler
    /// does not report a diagnostic — exercises GetHandlerBody returning null when the
    /// method has no syntax references (AnalyzerSymbolHelpers line 114 / BIDI016 line 101).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test compiles a separate fake assembly so the handler method has no
    /// <c>DeclaringSyntaxReferences</c>; that "method symbol with no source syntax" scenario cannot be
    /// reproduced against the real WebDriverBiDi assembly through an in-source stub, so it keeps its
    /// own compiled stand-in.
    /// </remarks>
    [Fact]
    public async Task AddObserver_WithCompiledAssemblyAsyncMethodRef_DoesNotReportDiagnostic()
    {
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

                public class AsyncHandlerHelper
                {
                    public static async Task Handle(LogEntryAddedEventArgs e)
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(librarySource, cancellationToken: TestContext.Current.CancellationToken);
        ImmutableArray<MetadataReference> netRefs = await ReferenceAssemblies.Net.Net80.ResolveAsync(LanguageNames.CSharp, TestContext.Current.CancellationToken);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "FakeHandlerLib16",
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
                        // Async method from compiled assembly — GetHandlerBody returns null (line 101)
                        using EventObserver<LogEntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(AsyncHandlerHelper.Handle);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(libRef);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Mutex.WaitOne inside an event handler reports a diagnostic —
    /// exercises the WaitHandle.WaitOne detection branch for a Mutex receiver.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_WithMutexWaitOne_ReportsWarning()
    {
        string test = """
            using System.Threading;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private static Mutex mutex = new Mutex();

                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                {|#0:mutex.WaitOne()|};
                                mutex.ReleaseMutex();
                            });
                    }
                }
            }
            """;

        // Mutex inherits from WaitHandle, so the diagnostic reports "WaitHandle.WaitOne"
        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("WaitHandle.WaitOne");

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a late-bound (<c>dynamic</c>) call inside an async handler is skipped, because the
    /// invocation binds to no method symbol.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DynamicInvocationInHandler_DoesNotReportDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void Setup(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async e =>
                            {
                                // Late-bound call: the invocation binds to no method symbol.
                                dynamic value = new object();
                                value.Anything();
                                await Task.CompletedTask;
                            });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a method named AddObserver whose return type is not a named type (here an array)
    /// does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// A method named AddObserver whose return type is not a named type (here an array) must not
    /// crash the analyzer (an unchecked cast to INamedTypeSymbol would surface as AD0001); it is
    /// simply not the library's EventObserver-returning AddObserver, so nothing is reported. The real
    /// <c>AddObserver</c> always returns the named <c>EventObserver&lt;T&gt;</c>, so this array-return
    /// lookalike cannot be reproduced against the real API.
    /// </remarks>
    [Fact]
    public async Task AddObserver_WithNonNamedReturnType_NoDiagnostic()
    {
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
