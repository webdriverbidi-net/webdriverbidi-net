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
    /// Tests that blocking operations in event handlers report a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithThreadSleep_ReportsWarning()
    {
        string test = """
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
                        });
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
    /// Tests that Task.Wait in event handlers reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithTaskWait_ReportsWarning()
    {
        string test = """
            using System;
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that handlers with RunHandlerAsynchronously option do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithRunHandlerAsynchronouslyOption_NoDiagnostic()
    {
        string test = """
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
                            Thread.Sleep(1000);
                            return Task.CompletedTask;
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
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
    /// Tests that GetAwaiter().GetResult() pattern reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_WithGetAwaiterGetResult_ReportsWarning()
    {
        string test = """
            using System;
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                    public EventObserver<T> CreateObserver(Func<T, Task> handler) => new EventObserver<T>();
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
                        var observer = driver.Log.OnEntryAdded.CreateObserver(args =>
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
    /// Tests that method returning non-EventObserver is not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private Task HandleLogEntry(LogEntryAddedEventArgs args)
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private Task HandleLogEntry(LogEntryAddedEventArgs args)
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private Task HandleLogEntry(LogEntryAddedEventArgs args)
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs
                {
                    public string Message { get; set; } = string.Empty;
                }

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
                using System;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private async Task HandleLogEntry(LogEntryAddedEventArgs args)
                    {
                        Console.WriteLine(args.Message);
                        await Task.Delay(100);
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleLogEntry);
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private Task HandleLogEntry(LogEntryAddedEventArgs args)
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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

        Assert.Single(diagnostics);
        Assert.Equal(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId, diagnostics[0].Id);
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private Task HandleLogEntry(LogEntryAddedEventArgs args)
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Task HandleLogEntry(LogEntryAddedEventArgs args)
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
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
    /// Tests that expression with Task.Result in simple expression body is detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SimpleLambda_WithExpressionBodyTaskResult_ReportsWarning()
    {
        string test = """
            using System;
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
                using System.Threading.Tasks;
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                        // Expression-bodied local function (null Body, non-null ExpressionBody).
                        Task Handle(LogEntryAddedEventArgs e) => Task.CompletedTask;

                        using EventObserver<LogEntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(Handle);
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
                        using EventObserver<LogEntryAddedEventArgs> observer =
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                        using EventObserver<LogEntryAddedEventArgs> observer =
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                        using EventObserver<LogEntryAddedEventArgs> observer =
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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

                public class Container
                {
                    public int Result { get; set; }
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<LogEntryAddedEventArgs> observer =
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                    public void TestMethod(BiDiDriver driver, Func<LogEntryAddedEventArgs, Task> handler)
                    {
                        // handler is a parameter — GetHandlerBody returns null for it.
                        using EventObserver<LogEntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(handler);
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
                        using EventObserver<LogEntryAddedEventArgs> observer =
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
                        using EventObserver<LogEntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async (e) =>
                            {
                                // Task.Delay (not Task.Wait) — exercises line 130 false branch.
                                await Task.Delay(1);
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
    /// Tests that calling AddObserver on a type that returns something other than
    /// EventObserver does not report a diagnostic — exercises the
    /// methodSymbol.ReturnType.Name != "EventObserver" branch (line 76).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
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
                    // Expression-bodied method reference — MethodDeclarationSyntax with null Body.
                    private static Task HandleEntry(LogEntryAddedEventArgs e) => Task.CompletedTask;

                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<LogEntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(HandleEntry);
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
                        // Local function as handler — exercises LocalFunctionStatementSyntax path.
                        async Task Handle(LogEntryAddedEventArgs e)
                        {
                            {|#0:Thread.Sleep(100)|};
                        }

                        using EventObserver<LogEntryAddedEventArgs> observer =
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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
        string test = HandlerFakeSource + """

            namespace TestApp
            {
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
        string test = HandlerFakeSource + """

            namespace TestApp
            {
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// In-source stand-ins for the driver, log module, and observable-event types used by the
    /// handler-resolution tests above.
    /// </summary>
    private const string HandlerFakeSource = """
        using System;
        using System.Threading.Tasks;

        namespace WebDriverBiDi
        {
            public class WebDriverBiDiEventArgs { }

            public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

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
                public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
            }

            public abstract class Module { }

            public class BrowserModule : Module
            {
                public Task<string> CloseAsync() => Task.FromResult("closed");
            }

            public class BiDiDriver
            {
                public LogModule Log { get; } = new();

                public BrowserModule Browser { get; } = new();
            }
        }
        """;

    /// <summary>
    /// Tests that a member access named <c>Result</c> whose qualifier is a namespace is skipped.
    /// The name check is purely syntactic, so the rule then asks for the qualifier's type, and a
    /// namespace has none.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ResultMemberOnNamespaceQualifier_DoesNotReportDiagnostic()
    {
        string test = BlockingOperationFakeSource + """

            namespace TestApp
            {
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
        string test = BlockingOperationFakeSource + """

            namespace TestApp
            {
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
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
    /// In-source stand-ins for the driver, log module, and observable-event types used by the
    /// unresolvable-receiver tests above.
    /// </summary>
    private const string BlockingOperationFakeSource = """
        using System;
        using System.Threading.Tasks;

        namespace WebDriverBiDi
        {
            public class WebDriverBiDiEventArgs { }

            public class EntryAddedEventArgs : WebDriverBiDiEventArgs { }

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
                public ObservableEvent<EntryAddedEventArgs> OnEntryAdded { get; } = new();
            }

            public class BiDiDriver
            {
                public LogModule Log { get; } = new();
            }
        }
        """;
}
