// <copyright file="BiDiDriver025AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver025 analyzer, which flags an <c>async void</c> handler passed to
/// <c>AddObserver</c>.
/// </summary>
public class BiDiDriver025AnalyzerTests
{
    [Fact]
    public async Task AsyncVoidMethodGroup_ReportsWarning()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private async void HandleEntry(EntryAddedEventArgs args)
                    {
                        await Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver({|#0:HandleEntry|});
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver025_AsyncVoidEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("HandleEntry");

        RealAssemblyAnalyzerTest<BiDiDriver025_AsyncVoidEventHandlerAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AsyncVoidMemberAccessMethodGroup_ReportsWarning()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private async void HandleEntry(EntryAddedEventArgs args)
                    {
                        await Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver({|#0:this.HandleEntry|});
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver025_AsyncVoidEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("HandleEntry");

        RealAssemblyAnalyzerTest<BiDiDriver025_AsyncVoidEventHandlerAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AsyncTaskMethodGroup_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private async Task HandleEntry(EntryAddedEventArgs args)
                    {
                        await Task.CompletedTask;
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleEntry);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver025_AsyncVoidEventHandlerAnalyzer>(testCode);
    }

    [Fact]
    public async Task AsyncLambda_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args => await Task.CompletedTask);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver025_AsyncVoidEventHandlerAnalyzer>(testCode);
    }

    [Fact]
    public async Task SynchronousMethodGroup_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private void HandleEntry(EntryAddedEventArgs args)
                    {
                    }

                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleEntry);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver025_AsyncVoidEventHandlerAnalyzer>(testCode);
    }

    [Fact]
    public async Task FuncVariableHandler_NoDiagnostic()
    {
        string testCode = """
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
                        Func<EntryAddedEventArgs, Task> handler = async args => await Task.CompletedTask;
                        var observer = driver.Log.OnEntryAdded.AddObserver(handler);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver025_AsyncVoidEventHandlerAnalyzer>(testCode);
    }

    [Fact]
    public async Task NonAddObserverInvocations_NoDiagnostic()
    {
        string testCode = """
            using System;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        NoOp();                        // invocation that is not a member access
                        Console.WriteLine("no-op");    // member access whose name is not AddObserver
                    }

                    private void NoOp()
                    {
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver025_AsyncVoidEventHandlerAnalyzer>(testCode);
    }

    [Fact]
    public async Task UnresolvedAddObserver_NoDiagnostic()
    {
        // A method named AddObserver that does not exist yields a null symbol; the analyzer must not
        // report (and must not crash) on it. Cannot be reproduced against the real API.
        string testCode = """
            namespace TestApp
            {
                public class BrokenObservable
                {
                    // No AddObserver defined.
                }

                public class TestClass
                {
                    public async void HandleThing(int value)
                    {
                        await System.Threading.Tasks.Task.CompletedTask;
                    }

                    public void TestMethod()
                    {
                        var obs = new BrokenObservable();
                        obs.{|CS1061:AddObserver|}(HandleThing);
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver025_AsyncVoidEventHandlerAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddObserverReturningNonEventObserver_NoDiagnostic()
    {
        // A same-named AddObserver on an unrelated type whose return type is a named type other than
        // EventObserver is not the library's method, so an async void handler is not reported.
        string testCode = """
            using System;

            namespace TestApp
            {
                public class CustomObservable
                {
                    public void AddObserver(Action<int> handler)
                    {
                    }
                }

                public class TestClass
                {
                    public async void HandleThing(int value)
                    {
                        await System.Threading.Tasks.Task.CompletedTask;
                    }

                    public void TestMethod()
                    {
                        var obs = new CustomObservable();
                        obs.AddObserver(HandleThing);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver025_AsyncVoidEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddObserverReturningNonNamedType_NoDiagnostic()
    {
        // A method named AddObserver whose return type is not a named type (here an array) must not
        // crash the analyzer and is not the library's EventObserver-returning method.
        string testCode = """
            using System;

            namespace TestApp
            {
                public class Widget
                {
                    public int[] AddObserver(Action<int> handler) => new int[0];
                }

                public class TestClass
                {
                    public async void HandleThing(int value)
                    {
                        await System.Threading.Tasks.Task.CompletedTask;
                    }

                    public void TestMethod()
                    {
                        var widget = new Widget();
                        var result = widget.AddObserver(HandleThing);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver025_AsyncVoidEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddObserverNoArguments_NoDiagnostic()
    {
        // A same-named AddObserver returning EventObserver but taking no arguments has no handler
        // argument to inspect. Cannot be reproduced against the real API.
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class NoArgObservable
                {
                    public EventObserver<EntryAddedEventArgs> AddObserver() => null!;
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var obs = new NoArgObservable();
                        var observer = obs.AddObserver();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver025_AsyncVoidEventHandlerAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
