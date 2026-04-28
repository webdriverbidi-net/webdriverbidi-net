// <copyright file="BiDiDriver016AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver016 analyzer that detects deadlock-prone synchronization patterns in event handlers.
/// </summary>
[TestFixture]
public class BiDiDriver016AnalyzerTests
{
    /// <summary>
    /// Tests that lock statement in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task LockStatement_InAsyncEventHandler_ReportsWarning()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new object();
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Monitor.Enter in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MonitorEnter_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new object();
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SemaphoreSlim.Wait in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SemaphoreSlimWait_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SemaphoreSlim.WaitAsync in async event handler does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SemaphoreSlimWaitAsync_InAsyncEventHandler_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that lock statement in non-async event handler does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task LockStatement_InNonAsyncEventHandler_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new object();
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver((e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that deadlock patterns with RunHandlerAsynchronously do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task LockStatement_WithRunHandlerAsynchronously_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                [Flags]
                public enum ObservableEventHandlerOptions
                {
                    None = 0,
                    RunHandlerAsynchronously = 1
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions options = ObservableEventHandlerOptions.None) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new object();
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that ManualResetEvent.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ManualResetEventWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Task.WaitAll in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TaskWaitAll_InAsyncEventHandler_ReportsWarning()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that multiple deadlock patterns are all detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MultipleDeadlockPatterns_InAsyncEventHandler_ReportsMultipleWarnings()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new object();
                    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Monitor.TryEnter in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MonitorTryEnter_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly object lockObj = new();
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Task.WaitAny in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task TaskWaitAny_InAsyncEventHandler_ReportsWarning()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Mutex.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MutexWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly Mutex mutex = new();
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AutoResetEvent.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AutoResetEventWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly AutoResetEvent resetEvent = new(false);
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SynchronizationContext.Send in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SynchronizationContextSend_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Semaphore.WaitOne in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SemaphoreWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly Semaphore semaphore = new(1, 1);
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-async event handler with sync lambda does not report diagnostic even with lock.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task LockStatement_InSyncLambdaHandler_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly object lockObj = new();
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver((e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that invocations that are not AddObserver are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that invocations without member access are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver that doesn't return EventObserver is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with non-EventObserver return type is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with no arguments is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that handler without body (null body) is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task HandlerWithoutBody_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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

                    public async Task HandleEventAsync(EntryAddedEventArgs e) => await Task.Delay(1);

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(HandleEventAsync);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that method reference (not lambda) with async modifier is not detected (analyzer limitation - IsAsyncHandler only checks lambdas).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MethodReference_WithLockStatement_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly object lockObj = new();
                    private readonly LogModule log = new();

                    public async Task HandleEventAsync(EntryAddedEventArgs e)
                    {
                        lock (lockObj)
                        {
                            // Method references are not detected as async by IsAsyncHandler
                        }
                        await Task.Delay(1);
                    }

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(HandleEventAsync);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that local function reference (not lambda) with async modifier is not detected (analyzer limitation - IsAsyncHandler only checks lambdas).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task LocalFunctionReference_WithLockStatement_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly object lockObj = new();
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        async Task HandleEventAsync(EntryAddedEventArgs e)
                        {
                            lock (lockObj)
                            {
                                // Method references are not detected as async by IsAsyncHandler
                            }
                            await Task.Delay(1);
                        }

                        log.OnEntryAdded.AddObserver(HandleEventAsync);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that WaitHandle.WaitOne (base class) in async event handler reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task WaitHandleWaitOne_InAsyncEventHandler_ReportsWarning()
    {
        string test = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                        log.OnEntryAdded.AddObserver(async (e) =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that simple lambda (non-parenthesized) with async works correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SimpleLambdaAsync_WithLockStatement_ReportsWarning()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                    private readonly object lockObj = new();
                    private readonly LogModule log = new();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async e =>
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

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with RunHandlerAsynchronously option does not report diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddObserver_WithRunHandlerAsynchronously_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions? options = null) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs { }

                public class ObservableEventHandlerOptions
                {
                    public bool RunHandlerAsynchronously { get; set; }
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
                        }, new ObservableEventHandlerOptions { RunHandlerAsynchronously = true });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that method reference to a property does not cause errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyReference_AsHandler_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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

                    public Func<EntryAddedEventArgs, Task> Handler { get; } = async (e) => await Task.Delay(1);

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(Handler);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that unresolved invocations inside handler body do not cause errors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UnresolvedInvocationInHandlerBody_NoDiagnostic()
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
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => null!;
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
                        log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|CS0103:UndefinedMethod|}();
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

        await testState.RunAsync();
    }

    [Test]
    public async Task LockStatement_WithObservableEventHandlerOptionsNone_ReportsWarning()
    {
        // Exercises the HasRunHandlerAsynchronouslyOption fallthrough when
        // ObservableEventHandlerOptions is present but does NOT contain "RunHandlerAsynchronously".
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

                [Flags]
                public enum ObservableEventHandlerOptions
                {
                    None = 0,
                    RunHandlerAsynchronously = 1
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions options = ObservableEventHandlerOptions.None) => null!;
                }

                public class EventObserver<T> where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class LogModule
                {
                    public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<EntryAddedEventArgs>();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly object lockObj = new object();
                    private readonly LogModule log = new LogModule();

                    public void TestMethod()
                    {
                        log.OnEntryAdded.AddObserver(async (e) =>
                        {
                            {|#0:lock (lockObj)
                            {
                                var x = 1 + 1;
                            }|}
                            await Task.Delay(100);
                        }, ObservableEventHandlerOptions.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("lock statement");

        CSharpAnalyzerTest<BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

}
