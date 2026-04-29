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
[TestFixture]
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
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that when RunHandlerAsynchronously is set no diagnostic is reported.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a non-AddObserver invocation is not analyzed.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a method returning non-EventObserver is not analyzed.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that multiple module commands in one handler report multiple warnings.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a named method reference containing a module command reports a warning.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a member-access method reference containing a module command reports a warning.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a local function containing a module command reports a warning.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a parenthesized lambda handler with a module command reports a warning.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a handler with no module commands does not report a diagnostic.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a non-Task-returning module method does not report a diagnostic.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a custom class ending in "Module" without the Module base class is not flagged.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with no arguments is not analyzed.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a module command in a deep inheritance hierarchy is reported.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddObserver with an unresolved method symbol is handled gracefully.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a module non-generic Task method (e.g. Task DoAsync()) is not flagged.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that an invocation expression used as the handler argument is not analyzed
    /// because GetHandlerBody reaches its default null arm (the expression is not a lambda
    /// or identifier/member-access method group).
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a local Func variable passed as the handler is not analyzed because the
    /// identifier resolves to an ILocalSymbol, not an IMethodSymbol, so GetMethodBodyFromSymbol
    /// returns null and no diagnostic is reported.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a named method with an expression body containing a module command reports a warning.
    /// Covers the expression-body (non-block) branch of GetMethodBodyFromSymbol.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that a local function with an expression body containing a module command reports a warning.
    /// Covers the expression-body (non-block) branch of GetMethodBodyFromSymbol for local functions.
    /// </summary>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that SupportedDiagnostics contains BIDI023.
    /// </summary>
    [Test]
    public void SupportedDiagnostics_ContainsBIDI023()
    {
        BiDiDriver023_ModuleCommandInEventHandlerAnalyzer analyzer = new();
        System.Collections.Immutable.ImmutableArray<DiagnosticDescriptor> diagnostics = analyzer.SupportedDiagnostics;

        Assert.That(diagnostics, Has.Length.EqualTo(1));
        Assert.That(diagnostics[0].Id, Is.EqualTo(BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId));
    }
}
