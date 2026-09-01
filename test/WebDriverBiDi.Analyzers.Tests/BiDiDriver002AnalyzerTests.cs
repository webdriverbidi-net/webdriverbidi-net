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
}
