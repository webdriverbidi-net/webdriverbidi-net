// <copyright file="BiDiDriver019CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver019 code fix provider.
/// </summary>
[TestFixture]
public class BiDiDriver019CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix inserts GetCheckpointTasks before UnsetCheckpoint.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_InsertsGetCheckpointTasksBeforeUnsetCheckpoint()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void SetCheckpoint() { }
                    public void UnsetCheckpoint() { }
                    public IReadOnlyList<Task> GetCheckpointTasks() => new List<Task>();
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
                using System.Linq;
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        observer.SetCheckpoint();
                        // Wait for checkpoint
                        {|#0:observer.UnsetCheckpoint()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void SetCheckpoint() { }
                    public void UnsetCheckpoint() { }
                    public IReadOnlyList<Task> GetCheckpointTasks() => new List<Task>();
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
                using System.Linq;
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver();
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        observer.SetCheckpoint();
                        var tasks = observer.GetCheckpointTasks();
                        // Wait for checkpoint
                        observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        CSharpCodeFixTest<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer, BiDiDriver019_UnsetCheckpointWithoutGetTasksCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix works with this-qualified member access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_WorksWithThisQualifiedMemberAccess()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void SetCheckpoint() { }
                    public void UnsetCheckpoint() { }
                    public IReadOnlyList<Task> GetCheckpointTasks() => new List<Task>();
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
                using System.Linq;
                using WebDriverBiDi;

                public class TestClass
                {
                    private EventObserver<LogEntryAddedEventArgs> observer;

                    public TestClass(BiDiDriver driver)
                    {
                        this.observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }

                    public async Task TestMethod()
                    {
                        this.observer.SetCheckpoint();
                        // Wait for checkpoint
                        {|#0:this.observer.UnsetCheckpoint()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void SetCheckpoint() { }
                    public void UnsetCheckpoint() { }
                    public IReadOnlyList<Task> GetCheckpointTasks() => new List<Task>();
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
                using System.Linq;
                using WebDriverBiDi;

                public class TestClass
                {
                    private EventObserver<LogEntryAddedEventArgs> observer;

                    public TestClass(BiDiDriver driver)
                    {
                        this.observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }

                    public async Task TestMethod()
                    {
                        this.observer.SetCheckpoint();
                        var tasks = observer.GetCheckpointTasks();
                        // Wait for checkpoint
                        this.observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        CSharpCodeFixTest<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer, BiDiDriver019_UnsetCheckpointWithoutGetTasksCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
