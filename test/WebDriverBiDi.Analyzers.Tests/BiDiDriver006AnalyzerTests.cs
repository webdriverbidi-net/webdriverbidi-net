// <copyright file="BiDiDriver006AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver006 analyzer that detects undisposed EventObservers.
/// </summary>
public class BiDiDriver006AnalyzerTests
{
    /// <summary>
    /// Tests that EventObserver without disposal reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithoutDisposal_ReportsWarning()
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
                        var {|#0:observer|} = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        // observer is never disposed
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that EventObserver with using statement does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithUsingStatement_NoDiagnostic()
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
                        using var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        // using statement handles disposal
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that EventObserver with explicit Unobserve call does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithUnobserveCall_NoDiagnostic()
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
                        // Do something with observer
                        observer.Unobserve();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that EventObserver with Dispose call does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithDisposeCall_NoDiagnostic()
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
                        // Do something with observer
                        observer.Dispose();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that EventObserver with DisposeAsync call does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithDisposeAsyncCall_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        // Do something with observer
                        await observer.DisposeAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that EventObserver with traditional using statement does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithTraditionalUsingStatement_NoDiagnostic()
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
                        using (var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask))
                        {
                            // using statement handles disposal
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that methods without body or expression body are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MethodWithoutBodyOrExpressionBody_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public abstract class TestClass
                {
                    public abstract void TestMethod(BiDiDriver driver);
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-AddObserver invocations are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonAddObserverInvocation_NoDiagnostic()
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
                        // AddDataCollector is not AddObserver, so the analyzer ignores it.
                        var collector = driver.Log.OnEntryAdded.AddDataCollector();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI006()
    {
        BiDiDriver006_ObserverDisposalAnalyzer analyzer = new();
        System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.DiagnosticDescriptor> diagnostics = analyzer.SupportedDiagnostics;

        Assert.Single(diagnostics);
        Assert.Equal(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, diagnostics[0].Id);
    }

    /// <summary>
    /// Tests that AddObserver call without member access expression is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_ViaDelegate_NoDiagnostic()
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
                        Func<Func<EntryAddedEventArgs, Task>, ObservableEventHandlerOptions, string, EventObserver<EntryAddedEventArgs>> addObserverFunc = driver.Log.OnEntryAdded.AddObserver;
                        var observer = addObserverFunc(args => Task.CompletedTask, ObservableEventHandlerOptions.RunHandlerSynchronously, "");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with unresolved method symbol is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_UnresolvedMethod_NoDiagnostic()
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
                        // BiDiDriver has no AddObserver method, so the symbol does not resolve.
                        var observer = driver.{|CS1061:AddObserver|}(args => Task.CompletedTask);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver returning non-generic type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because the library's
    /// <c>AddObserver</c> always returns the generic <c>EventObserver&lt;T&gt;</c>. A method named
    /// <c>AddObserver</c> that returns a non-generic type is the only way to exercise the analyzer's
    /// "return type is not EventObserver" branch, so it cannot be reproduced against the real API.
    /// </remarks>
    [Fact]
    public async Task AddObserver_ReturningNonGenericType_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class NonGenericObserver : IDisposable
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public NonGenericObserver AddObserver(Func<T, Task> handler) => new NonGenericObserver();
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
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that disposal call on complex member access is recognized.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_DisposedViaComplexMemberAccess_NoDiagnostic()
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
                    public EventObserver<EntryAddedEventArgs> Observer { get; set; }
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var container = new Container();
                        container.Observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        // This is testing that non-IdentifierNameSyntax in member access is handled
                        container.Observer.Dispose();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple undisposed observers report multiple warnings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleUndisposedObservers_ReportsMultipleWarnings()
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
                        var {|#0:observer1|} = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        var {|#1:observer2|} = driver.Network.OnBeforeRequestSent.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer1");

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("observer2");

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests GetFixAllProvider property.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver006_ObserverDisposalCodeFixProvider provider = new();
        Microsoft.CodeAnalysis.CodeFixes.FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.NotNull(fixAllProvider);
        Assert.Equal(Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI006()
    {
        BiDiDriver006_ObserverDisposalCodeFixProvider provider = new();
        System.Collections.Immutable.ImmutableArray<string> ids = provider.FixableDiagnosticIds;

        Assert.Single(ids);
        Assert.Equal(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, ids[0]);
    }

    /// <summary>
    /// Tests that invocation without member access in disposal detection is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_DisposalViaDelegate_StillReportsWarning()
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
                        var {|#0:observer|} = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        Action disposeAction = observer.Dispose;
                        // Dispose via delegate - analyzer doesn't detect this pattern
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that await DisposeAsync with variable intermediate step IS detected (not a limitation).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithIntermediateAwaitDisposeAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        // Await via intermediate variable - analyzer DOES detect this via await expression analysis
                        ValueTask disposeTask = observer.DisposeAsync();
                        await disposeTask;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that traditional using statement with explicit variable declaration is recognized.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithTraditionalUsingExplicitVariable_NoDiagnostic()
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
                        EventObserver<EntryAddedEventArgs> observer;
                        using (observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask))
                        {
                            // using statement handles disposal
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver returning wrong generic type name is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because the library's
    /// <c>AddObserver</c> always returns <c>EventObserver&lt;T&gt;</c>. A method named
    /// <c>AddObserver</c> returning a differently named generic type is the only way to exercise the
    /// analyzer's <c>Name != "EventObserver"</c> branch, so it cannot be reproduced against the real API.
    /// </remarks>
    [Fact]
    public async Task AddObserver_ReturningWrongGenericTypeName_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }

                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class CustomObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public CustomObserver<T> AddObserver(Func<T, Task> handler) => new CustomObserver<T>();
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
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventObserver_WithNonDisposalMethodCall_ReportsWarning()
    {
        // Exercises the path in HasDisposalCall where expressionName == variableName
        // but the method name is NOT a disposal method (the inner-if's closing brace
        // at line 197 is reached when the if-body is not taken).
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
                        var {|#0:observer|} = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        // Calls a non-disposal method — no Dispose/Unobserve/DisposeAsync call.
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that calling AddObserver on a method that returns void (not EventObserver) does not
    /// report a diagnostic (exercises the INamedTypeSymbol? returnType == null guard, line 138).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because the library's
    /// <c>AddObserver</c> always returns <c>EventObserver&lt;T&gt;</c>. A <c>void</c>-returning
    /// <c>AddObserver</c> is the only way to exercise the null-return-type guard, so it cannot be
    /// reproduced against the real API.
    /// </remarks>
    [Fact]
    public async Task AddObserver_ReturningVoid_DoesNotReportDiagnostic()
    {
        // AddObserver here returns void, so the cast to INamedTypeSymbol will fail.
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    // Returns void instead of EventObserver<T>
                    public void AddObserver(Func<T, Task> handler) { }
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
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an EventObserver declared as the resource of a legacy using-statement block
    /// does not report a diagnostic (exercises the UsingStatementSyntax parent-walk path, lines 159-161).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_InUsingStatementBlock_DoesNotReportDiagnostic()
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
                        // Variable declared as the resource of the using-statement —
                        // IsInUsingStatement walks parent to find UsingStatementSyntax.
                        using (EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(async (e) => { }))
                        {
                            // observer is disposed when the block exits
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that awaiting DisposeAsync on an EventObserver is recognized as disposal and
    /// does not report a diagnostic (exercises the await DisposeAsync detection, lines 204-208).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventObserver_WithAwaitedDisposeAsync_DoesNotReportDiagnostic()
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
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        await observer.DisposeAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventObserver_RemoveObserverWithNonMatchingArgumentsThenMatch_NoDiagnostic()
    {
        // A series of RemoveObserver calls whose arguments do not match the observer's Id (a plain string,
        // a member access with a different name, an Id read off a non-identifier receiver, and a different
        // variable's Id) are skipped; the final matching call releases the observer, so no leak is reported.
        // This exercises every partial-match branch and loop back-edge in the RemoveObserver recognition.
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class Other
                {
                    public string Id { get; } = "x";
                    public string Name { get; } = "y";
                }

                public class TestClass
                {
                    private Other Get() => new Other();

                    public void TestMethod(BiDiDriver driver)
                    {
                        string s = "id";
                        Other other = new Other();
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        driver.Log.OnEntryAdded.RemoveObserver(s);
                        driver.Log.OnEntryAdded.RemoveObserver(other.Name);
                        driver.Log.OnEntryAdded.RemoveObserver(Get().Id);
                        driver.Log.OnEntryAdded.RemoveObserver(other.Id);
                        driver.Log.OnEntryAdded.RemoveObserver(observer.Id);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventObserver_WithNonMatchingReturns_ReportsWarning()
    {
        // The observer is neither disposed nor returned/stored: the preceding return of a non-identifier
        // (null) and of a different observer variable do not count as handling it, so a leak is reported.
        // This exercises the non-matching branches and loop back-edge of the returned-or-stored recognition.
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public EventObserver<EntryAddedEventArgs> TestMethod(BiDiDriver driver, bool flag)
                    {
                        var {|#0:observer|} = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        var otherObserver = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);

                        // Non-matching assignments: the right-hand side is a literal (not an identifier),
                        // then an identifier other than 'observer'. Neither stores the observer, so the
                        // assignment-scan loop iterates without matching.
                        int counter = 0;
                        counter = 5;
                        BiDiDriver localDriver = driver;
                        localDriver = driver;

                        if (flag)
                        {
                            return null;
                        }

                        return otherObserver;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventObserver_ReleasedViaRemoveObserver_NoDiagnostic()
    {
        // Releasing the observer through ObservableEvent.RemoveObserver(observer.Id) unregisters it, so it is not leaked.
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
                        driver.Log.OnEntryAdded.RemoveObserver(observer.Id);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventObserver_Returned_NoDiagnostic()
    {
        // Returning the observer transfers ownership to the caller, which is responsible for disposing it.
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public EventObserver<EntryAddedEventArgs> TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        return observer;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventObserver_StoredInField_NoDiagnostic()
    {
        // Storing the observer in a field transfers ownership to the field's owner, which disposes it later.
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private EventObserver<EntryAddedEventArgs> observerField;

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        this.observerField = observer;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddObserver_WithNonNamedReturnType_NoDiagnostic()
    {
        // A method named AddObserver whose return type is not a named type (here an array) must not
        // crash the analyzer (an unchecked cast to INamedTypeSymbol would surface as AD0001); it is
        // simply not the library's EventObserver-returning AddObserver, so nothing is reported. This
        // scenario needs a custom type because the real AddObserver always returns EventObserver<T>.
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

        CSharpAnalyzerTest<BiDiDriver006_ObserverDisposalAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
