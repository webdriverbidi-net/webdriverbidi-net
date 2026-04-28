// <copyright file="BiDiDriver006CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver006 code fix provider.
/// </summary>
[TestFixture]
public class BiDiDriver006CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix adds using declaration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EventObserver_CodeFixAddsUsingDeclaration()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Unobserve() { }
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
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var {|#0:observer|} = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Unobserve() { }
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
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        CSharpCodeFixTest<BiDiDriver006_ObserverDisposalAnalyzer, BiDiDriver006_ObserverDisposalCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
    public async Task ObserverDeclaredAsField_NoDiagnostic()
    {
        // BIDI006 only watches local variable declarations; field-level declarations
        // must not trigger the diagnostic.
        string testCode = """
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
                    public ObservableEvent<LogEntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<LogEntryAddedEventArgs>();
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new LogModule();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly EventObserver<LogEntryAddedEventArgs> _observer;

                    public TestClass(BiDiDriver driver)
                    {
                        _observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver006_ObserverDisposalAnalyzer>(testCode);
    }
}
