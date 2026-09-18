// <copyright file="BiDiDriver002AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver002 analyzer that detects event registration after StartAsync.
/// </summary>
public class BiDiDriver002AnalyzerTests
{
    /// <summary>
    /// Tests that RegisterEvent called after StartAsync reports an error diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterStartAsync_ReportsError()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterEvent called before StartAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_BeforeStartAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterEvent<string>("test.event", async (e) => { });
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver called after StartAsync does not report a diagnostic. Observers may be
    /// added to an observable event at any time; only RegisterEvent is locked once the driver has started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_AfterStartAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        driver.OnLogMessage.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddObserver called before StartAsync does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserver_BeforeStartAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.OnLogMessage.AddObserver(async (e) => { });
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple AddObserver calls after StartAsync, on both driver-level and module-level
    /// observable events, do not report diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleAddObserver_AfterStartAsync_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        driver.Log.OnEntryAdded.AddObserver(async (e) => { });
                        driver.Network.OnBeforeRequestSent.AddObserver(async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that methods without body are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MethodWithoutBody_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public abstract class TestClass
                {
                    public abstract Task TestMethod();
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-BiDiDriver types are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonBiDiDriverType_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class CustomDriver
                {
                    public CustomDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterEvent<T>(string eventName, Func<T, Task> eventInvoker) { }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomDriver driver = new CustomDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        driver.RegisterEvent<string>("test.event", async (e) => { });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that assignment expressions whose right-hand side is an invocation are analyzed,
    /// so that a StartAsync call assigned to a variable still marks the driver as started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AssignmentExpression_AfterStartAsync_ReportsError()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        Task startTask;
                        startTask = driver.StartAsync("ws://localhost:9222");
                        await startTask;
                        {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocations without member access are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Invocation_WithoutMemberAccess_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        Func<Task> action = async () => { };
                        await action();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that unresolved method symbols are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UnresolvedMethodSymbol_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        driver.{|CS1061:NonExistentMethod|}();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests GetFixAllProvider returns the correct provider.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver002_EventRegistrationAfterStartCodeFixProvider provider = new BiDiDriver002_EventRegistrationAfterStartCodeFixProvider();
        FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.Equal(WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI002()
    {
        BiDiDriver002_EventRegistrationAfterStartCodeFixProvider provider = new BiDiDriver002_EventRegistrationAfterStartCodeFixProvider();

        Assert.Contains(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, provider.FixableDiagnosticIds);
        Assert.Single(provider.FixableDiagnosticIds);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property of the analyzer.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI002()
    {
        BiDiDriver002_EventRegistrationAfterStartAnalyzer analyzer = new BiDiDriver002_EventRegistrationAfterStartAnalyzer();

        Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, analyzer.SupportedDiagnostics[0].Id);
    }

    /// <summary>
    /// Tests that multiple drivers are tracked independently.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleDrivers_IndependentTracking()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver1 = new BiDiDriver(TimeSpan.FromSeconds(30));
                        BiDiDriver driver2 = new BiDiDriver(TimeSpan.FromSeconds(30));

                        // driver1: correct order
                        driver1.RegisterEvent<string>("test.event", async (e) => { });
                        await driver1.StartAsync("ws://localhost:9222");

                        // driver2: incorrect order
                        await driver2.StartAsync("ws://localhost:9222");
                        {|#0:driver2.RegisterEvent<string>("test.event", async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that variable without initializer is handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task VariableWithoutInitializer_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver;
                        driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterEvent on non-tracked driver doesn't report diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEventOnNonTrackedDriver_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    private BiDiDriver fieldDriver = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod()
                    {
                        await fieldDriver.StartAsync("ws://localhost:9222");
                        fieldDriver.RegisterEvent<string>("test.event", async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that complex expression statements are handled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ComplexExpressionStatement_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        int x = 5;
                        x++;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a RegisterEvent call whose member-access chain cannot be traced back to a known
    /// driver variable does not report a diagnostic (exercises the GetDriverVariableName null path
    /// when the base of the chain is not a simple identifier).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_OnChainedMethodCallResult_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public static class DriverFactory
                {
                    public static BiDiDriver Create() => new BiDiDriver(TimeSpan.FromSeconds(30));
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        // RegisterEvent called on a method-call result, not a simple variable —
                        // GetDriverVariableName cannot extract a name, so no diagnostic.
                        DriverFactory.Create().RegisterEvent<string>("test.event", async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a non-declaration, non-expression statement (like an if-block) in
    /// a method body does not crash — exercises the neither-branch path on line 76.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MethodWithIfStatement_DoesNotCrash()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        // An if-statement is neither LocalDeclaration nor ExpressionStatement.
                        if (true) { }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that awaiting a non-invocation task does not crash — exercises the
    /// awaitExpression.Expression is not InvocationExpressionSyntax false branch (line 121).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MethodWithAwaitedVariable_DoesNotCrash()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        // Await a variable (not an invocation expression).
                        Task t = Task.CompletedTask;
                        await t;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterEvent() called after StopAsync() does not report a diagnostic,
    /// because the driver is no longer started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterStopAsync_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        driver.RegisterEvent<string>("test.event", async (e) => { });
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RegisterEvent_WithNonConstantEventName_ReportsArgumentText()
    {
        // When the event name is not a compile-time constant, the message falls back to the argument's
        // source text rather than a resolved value.
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(string eventName)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterEvent<string>(eventName, async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("eventName");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a RegisterEvent nested inside an if block, after a top-level StartAsync, is now
    /// flagged. The previous top-level-only walk never saw calls inside nested blocks.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_InsideIfAfterStartAsync_ReportsError()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(bool condition)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        if (condition)
                        {
                            {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that starting the driver in one branch of an if/else does not mark it started for the
    /// other branch, so a RegisterEvent in the branch that did not start is not flagged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_InBranchThatDidNotStart_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(bool condition)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        if (condition)
                        {
                            driver.RegisterEvent<string>("test.event", async (e) => { });
                        }
                        else
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterEvent after a StartAsync call written with a <c>.ConfigureAwait(false)</c> continuation is still reported.
    /// The wrapper is the outermost invocation, so without unwrapping the chain the analyzer sees a
    /// call it does not recognize and never marks the driver as started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterStartAsyncWithConfigureAwait_ReportsError()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222").ConfigureAwait(false);
                        {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0).WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterEvent after a StartAsync call written with a blocking <c>.Wait()</c> is still reported.
    /// The wrapper is the outermost invocation, so without unwrapping the chain the analyzer sees a
    /// call it does not recognize and never marks the driver as started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterStartAsyncWithBlockingWait_ReportsError()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.StartAsync("ws://localhost:9222").Wait();
                        {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0).WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that RegisterEvent after a StartAsync call written with a blocking <c>.ConfigureAwait(false).GetAwaiter().GetResult()</c> chain is still reported.
    /// The wrapper is the outermost invocation, so without unwrapping the chain the analyzer sees a
    /// call it does not recognize and never marks the driver as started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterStartAsyncWithGetAwaiterGetResult_ReportsError()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.StartAsync("ws://localhost:9222").ConfigureAwait(false).GetAwaiter().GetResult();
                        {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0).WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that starting a driver instance returned by another call does not mark a differently
    /// obtained local as started. The StartAsync receiver is itself an invocation, so this also pins
    /// that only the known task-chaining wrappers are unwrapped: unwrapping every
    /// invocation-receivered member access would discard the StartAsync call entirely.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterStartAsyncOnDifferentReturnedDriver_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = GetDriver();
                        await GetDriver().StartAsync("ws://localhost:9222");
                        driver.RegisterEvent<string>("test.event", async (e) => { });
                    }

                    private static BiDiDriver GetDriver() => new BiDiDriver(TimeSpan.FromSeconds(30));
                }
            }
            """;


        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that declaring the start task, rather than assigning it, is still recognized as starting the
    /// driver. The connect attempt begins at the call in both spellings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventRegistration_AfterStartAsyncDeclaredAsTask_ReportsError()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        Task startTask = driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                        await startTask;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("test.event");

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the same driver name declared in two sibling loop bodies does not crash the analyzer (the second declaration used to be added to an immutable dictionary that already held the key) and is tracked afresh in each body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_DriverRedeclaredInSiblingLoopBodies_DoesNotCrashAndReportsOnlyStartedOne()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(string[] urls)
                    {
                        foreach (string url in urls)
                        {
                            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                            await driver.StartAsync(url);
                            {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                        }

                        foreach (string url in urls)
                        {
                            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                            driver.RegisterEvent<string>("test.event", async (e) => { });
                            await driver.StartAsync(url);
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("test.event");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver002_EventRegistrationAfterStartAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that a registration in one switch section is not judged against a StartAsync in a different, mutually exclusive section.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_InSwitchSectionAfterStartInAnotherSection_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(string browser)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        switch (browser)
                        {
                            case "chrome":
                                await driver.StartAsync("ws://localhost:9222");
                                break;
                            case "firefox":
                                driver.RegisterEvent<string>("test.event", async (e) => { });
                                await driver.StartAsync("ws://localhost:9223");
                                break;
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver002_EventRegistrationAfterStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a registration in a catch clause after a StartAsync in the try block is not reported: the start may have failed, leaving the driver not started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_InCatchAfterFailedStartInTry_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        catch (WebDriverBiDiException)
                        {
                            driver.RegisterEvent<string>("test.event", async (e) => { });
                            await driver.StartAsync("ws://localhost:9223");
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver002_EventRegistrationAfterStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a driver declared by a classic <c>await using (T x = ...)</c> statement is tracked like one declared by a local declaration statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_InClassicUsingStatementDeclaration_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        await using (BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30)))
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            {|#0:driver.RegisterEvent<string>("test.event", async (e) => { })|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("test.event");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver002_EventRegistrationAfterStartAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that a start inside a while loop does not count as having certainly run for the code after
    /// the loop, which may never have entered the body, so a registration there is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterStartInsideWhileLoop_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(bool flag)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        while (flag)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            break;
                        }

                        driver.RegisterEvent<string>("custom.event", e => Task.CompletedTask);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver002_EventRegistrationAfterStartAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a RegisterEvent that takes no arguments — which the real driver has no overload for, so a
    /// stub driver type declares one — is reported with a generic name rather than failing on the missing
    /// event-name argument.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_WithoutArguments_ReportsGenericEventName()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public interface IBiDiDriverLifecycleManager
                {
                    Task StartAsync(string url);
                }

                public interface IBiDiModuleHost
                {
                    void RegisterEvent();
                }

                public class BiDiDriver : IBiDiDriverLifecycleManager, IBiDiModuleHost
                {
                    public BiDiDriver(TimeSpan timeout) { }
                    public Task StartAsync(string url) => Task.CompletedTask;
                    public void RegisterEvent() { }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterEvent()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("event");

        CSharpAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an event registration after the driver has been handed to a helper is not reported: the
    /// helper may have stopped the driver, which this rule cannot see.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterEvent_AfterDriverPassedToHelperThatStopsIt_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        await StopHelperAsync(driver);
                        driver.RegisterEvent<string>("test.event", async (e) => { });
                    }

                    private static async Task StopHelperAsync(BiDiDriver driver)
                    {
                        await driver.StopAsync();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests the walker paths the reference describes but this rule's own tests never reached: a start in
    /// the right operand of <c>??</c> or <c>??=</c>, which may not run, and a hand-off through a cast,
    /// which stops tracking altogether.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandOffAndShortCircuitForms_NoDiagnostic()
    {
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
            // The right operand of ?? may not run, so the start it holds is not certain on every path.
            BiDiDriver coalesced = new();
            Task? pendingCoalesce = null;
            await (pendingCoalesce ?? coalesced.StartAsync(url));
            coalesced.RegisterEvent<string>("test.event", async (e) => { });

            // ??= carries the same uncertainty in its compound form.
            BiDiDriver assigned = new();
            Task? pendingAssign = null;
            pendingAssign ??= assigned.StartAsync(url);
            await pendingAssign;
            assigned.RegisterEvent<string>("test.event", async (e) => { });

            // A cast hand-off stops tracking entirely, so even a certain start leaves nothing reported.
            BiDiDriver handedOff = new();
            await handedOff.StartAsync(url);
            await StartHelperAsync((IBiDiDriverLifecycleManager)handedOff);
            handedOff.RegisterEvent<string>("test.event", async (e) => { });
                    }

                    private static Task StartHelperAsync(IBiDiDriverLifecycleManager manager) => Task.CompletedTask;
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver002_EventRegistrationAfterStartAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
