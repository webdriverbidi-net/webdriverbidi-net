// <copyright file="BiDiDriver023AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver023 analyzer that detects module command calls inside event handlers.
/// </summary>
public class BiDiDriver023AnalyzerTests
{
    // Common stub definitions reused across tests.
    private const string CommonStubs = """
        namespace WebDriverBiDi
        {
            using System;
            using System.Threading.Tasks;

            public class WebDriverBiDiEventArgs { }
            public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

            public enum ObservableEventHandlerOptions { None = 0, RunHandlerAsynchronously = 1 }

            public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
            {
                public void Dispose() { }
            }

            public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
            {
                public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions options) => new EventObserver<T>();
            }

            public abstract class Module
            {
                protected Module(IBiDiCommandExecutor executor) { }
                public abstract string ModuleName { get; }
            }

            public interface IBiDiCommandExecutor { }

            public class NavigateCommandResult { }
            public class NavigateCommandParameters
            {
                public NavigateCommandParameters(string contextId, string url) { }
            }

            public class BrowsingContextModule : Module
            {
                public BrowsingContextModule(IBiDiCommandExecutor executor) : base(executor) { }
                public override string ModuleName => "browsingContext";
                public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters) => Task.FromResult(new NavigateCommandResult());
            }

            public class LogModule
            {
                public ObservableEvent<LogEntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<LogEntryAddedEventArgs>();
            }

            public class BiDiDriver : IBiDiCommandExecutor
            {
                public BrowsingContextModule BrowsingContext { get; } = new BrowsingContextModule(null!);
                public LogModule Log { get; } = new LogModule();
            }
        }
        """;

    /// <summary>
    /// Tests that a module command call inside an event handler reports a warning.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a non-async Task-returning lambda with RunHandlerAsynchronously still reports the
    /// module command, using the message that explains the option does not offload a synchronous body.
    /// </summary>
    [Fact]
    public async Task EventHandler_NonAsyncTaskLambda_WithRunHandlerAsynchronously_ReportsSynchronousBodyWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(
                            args => {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|},
                            ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithMessage("Module command 'NavigateAsync' is called inside an event handler. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' does not offload the synchronous body of a Task-returning handler; make the handler 'async' so the command is issued from a continuation rather than on the dispatching thread.");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that when RunHandlerAsynchronously is set no diagnostic is reported.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithRunHandlerAsynchronously_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a non-AddObserver invocation is not analyzed.
    /// </summary>
    [Fact]
    public async Task NonAddObserverInvocation_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Func<LogEntryAddedEventArgs, Task> handler = async args =>
                        {
                            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        };
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a method returning non-EventObserver is not analyzed.
    /// </summary>
    [Fact]
    public async Task MethodReturningNonEventObserver_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class CustomObservable
                {
                    public void AddObserver(Func<LogEntryAddedEventArgs, Task> handler) { }
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var obs = new CustomObservable();
                        obs.AddObserver(async args =>
                        {
                            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple module commands in one handler report multiple warnings.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithMultipleModuleCommands_ReportsMultipleWarnings()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://a.com"))|};
                            await {|#1:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://b.com"))|};
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        DiagnosticResult expected2 = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a named method reference containing a module command reports a warning.
    /// </summary>
    [Fact]
    public async Task NamedMethodHandler_WithModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private BiDiDriver driver = null!;

                    private async Task HandleEntry(LogEntryAddedEventArgs args)
                    {
                        await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                    }

                    public void TestMethod(BiDiDriver d)
                    {
                        driver = d;
                        var observer = d.Log.OnEntryAdded.AddObserver(HandleEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a member-access method reference containing a module command reports a warning.
    /// </summary>
    [Fact]
    public async Task MemberAccessMethodReference_WithModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private BiDiDriver driver = null!;

                    private async Task HandleEntry(LogEntryAddedEventArgs args)
                    {
                        await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                    }

                    public void TestMethod(BiDiDriver d)
                    {
                        driver = d;
                        var observer = d.Log.OnEntryAdded.AddObserver(this.HandleEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a local function containing a module command reports a warning.
    /// </summary>
    [Fact]
    public async Task LocalFunctionHandler_WithModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        async Task HandleEntry(LogEntryAddedEventArgs args)
                        {
                            await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                        }

                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a parenthesized lambda handler with a module command reports a warning.
    /// </summary>
    [Fact]
    public async Task ParenthesizedLambdaHandler_WithModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async (args) =>
                        {
                            await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a handler with no module commands does not report a diagnostic.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithNoModuleCommands_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            Console.WriteLine("event received");
                            await Task.Delay(10);
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a non-Task-returning module method does not report a diagnostic.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithNonTaskReturningModuleMethod_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

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
                            string name = driver.BrowsingContext.ModuleName;
                            return Task.CompletedTask;
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a custom class ending in "Module" without the Module base class is not flagged.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithCustomModuleNotInheritingModule_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class FakeModule
                {
                    public Task<string> DoStuffAsync() => Task.FromResult("ok");
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var fake = new FakeModule();
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await fake.DoStuffAsync();
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with no arguments is not analyzed.
    /// </summary>
    [Fact]
    public async Task AddObserver_NoArguments_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class NoArgObservable
                {
                    public EventObserver<LogEntryAddedEventArgs> AddObserver() => new EventObserver<LogEntryAddedEventArgs>();
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

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a module command in a deep inheritance hierarchy is reported.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithDeepInheritanceModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public abstract class BaseModule : Module
                {
                    protected BaseModule(IBiDiCommandExecutor executor) : base(executor) { }
                }

                public class ConcreteModule : BaseModule
                {
                    public ConcreteModule(IBiDiCommandExecutor executor) : base(executor) { }
                    public override string ModuleName => "concrete";
                    public Task<string> DoWorkAsync() => Task.FromResult("done");
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        IBiDiCommandExecutor executor = driver;
                        var module = new ConcreteModule(executor);
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await {|#0:module.DoWorkAsync()|};
                        });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("DoWorkAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver with an unresolved method symbol is handled gracefully.
    /// </summary>
    [Fact]
    public async Task AddObserver_UnresolvedMethodSymbol_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class BrokenObservable
                {
                    // No AddObserver defined
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var obs = new BrokenObservable();
                        obs.{|CS1061:AddObserver|}(async args =>
                        {
                            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a module non-generic Task method (e.g. Task DoAsync()) is not flagged.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithNonGenericTaskModuleMethod_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class UtilModule : Module
                {
                    public UtilModule(IBiDiCommandExecutor executor) : base(executor) { }
                    public override string ModuleName => "util";
                    public Task RunAsync() => Task.CompletedTask;
                }

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var module = new UtilModule(driver);
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await module.RunAsync();
                        });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an invocation expression used as the handler argument is not analyzed
    /// because GetHandlerBody reaches its default null arm (the expression is not a lambda
    /// or identifier/member-access method group).
    /// </summary>
    [Fact]
    public async Task AddObserver_HandlerReturnedByInvocation_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private static Func<LogEntryAddedEventArgs, Task> GetHandler(BiDiDriver driver) =>
                        async args => await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));

                    public void TestMethod(BiDiDriver driver)
                    {
                        // The argument is an InvocationExpressionSyntax — not a lambda or method group —
                        // so GetHandlerBody returns null and no diagnostic is reported.
                        var observer = driver.Log.OnEntryAdded.AddObserver(GetHandler(driver));
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a local Func variable passed as the handler is not analyzed because the
    /// identifier resolves to an ILocalSymbol, not an IMethodSymbol, so GetMethodBodyFromSymbol
    /// returns null and no diagnostic is reported.
    /// </summary>
    [Fact]
    public async Task AddObserver_LocalFuncVariableAsIdentifier_NoDiagnostic()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System;
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        // A local variable of delegate type passed by identifier resolves to
                        // ILocalSymbol, not IMethodSymbol, so GetMethodBodyFromSymbol returns null.
                        Func<LogEntryAddedEventArgs, Task> fn = async e =>
                        {
                            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        };
                        var observer = driver.Log.OnEntryAdded.AddObserver(fn);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a named method with an expression body containing a module command reports a warning.
    /// Covers the expression-body (non-block) branch of GetMethodBodyFromSymbol.
    /// </summary>
    [Fact]
    public async Task NamedMethod_ExpressionBody_WithModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private BiDiDriver driver = null!;

                    private Task HandleEntry(LogEntryAddedEventArgs args) =>
                        {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};

                    public void TestMethod(BiDiDriver d)
                    {
                        driver = d;
                        var observer = d.Log.OnEntryAdded.AddObserver(HandleEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a local function with an expression body containing a module command reports a warning.
    /// Covers the expression-body (non-block) branch of GetMethodBodyFromSymbol for local functions.
    /// </summary>
    [Fact]
    public async Task LocalFunction_ExpressionBody_WithModuleCommand_ReportsWarning()
    {
        string test = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Task HandleEntry(LogEntryAddedEventArgs args) =>
                            {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};

                        var observer = driver.Log.OnEntryAdded.AddObserver(HandleEntry);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that SupportedDiagnostics contains BIDI023.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI023()
    {
        BiDiDriver023_ModuleCommandInEventHandlerAnalyzer analyzer = new();
        System.Collections.Immutable.ImmutableArray<DiagnosticDescriptor> diagnostics = analyzer.SupportedDiagnostics;

        // Two descriptors share the ID: the default message and the message used when
        // RunHandlerAsynchronously is present but the handler body is synchronous.
        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, descriptor => Assert.Equal(BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId, descriptor.Id));
    }

    /// <summary>
    /// Tests that calling a module method returning plain Task (not generic) in an event
    /// handler does not report — exercises namedReturn.IsGenericType false (line 146).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommandInEventHandler_PlainTaskMethod_DoesNotReport()
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

                public abstract class Module { }

                public class BrowserModule : Module
                {
                    // Returns plain non-generic Task — IsGenericType is false.
                    public Task DoWorkAsync() => Task.CompletedTask;
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new();
                    public BrowserModule Browser { get; } = new();
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
                                await driver.Browser.DoWorkAsync();
                            });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an expression-bodied regular method reference as handler is analysed —
    /// exercises the MethodDeclarationSyntax Body==null path in GetMethodBodyFromSymbol
    /// (AnalyzerSymbolHelpers line 114 block 0).
    /// BIDI023 does not check IsAsyncHandler, so it calls GetHandlerBody for all expressions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommandInEventHandler_ExpressionBodiedMethodRef_ReportsDiagnostic()
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

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    private BiDiDriver _driver;

                    public TestClass(BiDiDriver driver) { _driver = driver; }

                    // Expression-bodied method — Body is null, ExpressionBody is non-null.
                    // When passed as a method reference, BIDI023 calls GetHandlerBody on it.
                    private Task Handle(LogEntryAddedEventArgs e) =>
                        {|#0:_driver.Browser.CloseAsync()|};

                    public void Setup()
                    {
                        using EventObserver<LogEntryAddedEventArgs> observer =
                            _driver.Log.OnEntryAdded.AddObserver(Handle);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("CloseAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an expression-bodied local function as handler is analysed —
    /// exercises the LocalFunctionStatementSyntax Body==null path in GetMethodBodyFromSymbol
    /// (AnalyzerSymbolHelpers line 115/142 block 0).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommandInEventHandler_ExpressionBodiedLocalFunctionRef_ReportsDiagnostic()
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

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void Setup(BiDiDriver driver)
                    {
                        // Expression-bodied local function — Body is null, ExpressionBody non-null.
                        Task Handle(LogEntryAddedEventArgs e) =>
                            {|#0:driver.Browser.CloseAsync()|};

                        using EventObserver<LogEntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(Handle);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("CloseAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Minimal test that exercises IsModuleCommandMethod's Task generic check (line 146
    /// branches 54,1 and 72,1) using simplest possible inline stub.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IsModuleCommandMethod_GenericTaskReturn_ExercisesLine146TrueBranches()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class WebDriverBiDiEventArgs { }
                public class MyEventArgs : WebDriverBiDiEventArgs { }
                public class MyResult { }

                public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
                {
                    public void Dispose() { }
                }

                public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
                {
                    public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                }

                public abstract class Module { }

                public class MyModule : Module
                {
                    public ObservableEvent<MyEventArgs> OnEvent { get; } = new();
                    // Task<T> return — IsModuleCommandMethod should return true.
                    public Task<MyResult> DoCommandAsync() => Task.FromResult(new MyResult());
                }

                public class BiDiDriver
                {
                    public MyModule My { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<MyEventArgs> obs =
                            driver.My.OnEvent.AddObserver(async (e) =>
                            {
                                await {|#0:driver.My.DoCommandAsync()|};
                            });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("DoCommandAsync");

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
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
    public async Task ModuleCommandInEventHandler_InterfaceMethodRef_DoesNotReportDiagnostic()
    {
        string test = ModuleCommandFakeSource + """

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

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a handler resolved to an <c>extern</c> local function, which has neither a block
    /// body nor an expression body, is skipped rather than crashing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleCommandInEventHandler_ExternLocalFunctionRef_DoesNotReportDiagnostic()
    {
        string test = ModuleCommandFakeSource + """

            namespace TestApp
            {
                using System.Runtime.InteropServices;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void Setup(BiDiDriver driver)
                    {
                        // An extern local function: Body and ExpressionBody are both null.
                        [DllImport("does-not-exist")]
                        static extern Task HandleAsync(EntryAddedEventArgs e);

                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(HandleAsync);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that module methods which do not return <c>Task&lt;T&gt;</c> — a synchronous method
    /// and a non-generic <c>Task</c> method — are not treated as module commands.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModuleMethodsNotReturningGenericTask_DoNotReportDiagnostic()
    {
        string test = """
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
                    public string Describe() => "browser";

                    public Task PingAsync() => Task.CompletedTask;
                }

                public class BiDiDriver
                {
                    public LogModule Log { get; } = new();

                    public BrowserModule Browser { get; } = new();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void Setup(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer =
                            driver.Log.OnEntryAdded.AddObserver(async e =>
                            {
                                string description = driver.Browser.Describe();
                                System.Console.WriteLine(description);
                                await driver.Browser.PingAsync();
                            });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// In-source stand-ins for the driver, log module, and module-command types used by the
    /// handler-resolution tests above.
    /// </summary>
    private const string ModuleCommandFakeSource = """
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
    /// Tests that a late-bound (<c>dynamic</c>) call inside a handler is skipped, because the
    /// invocation binds to no method symbol.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DynamicInvocationInHandler_DoesNotReportDiagnostic()
    {
        string test = EventHandlerFakeSource + """

            namespace TestApp
            {
                using WebDriverBiDi;

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

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// In-source stand-ins for the driver, log module, and observable-event types used by the
    /// handler-body tests above.
    /// </summary>
    private const string EventHandlerFakeSource = """
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

        CSharpAnalyzerTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
