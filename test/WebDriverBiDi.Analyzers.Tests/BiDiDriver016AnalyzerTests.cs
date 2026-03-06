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
}
