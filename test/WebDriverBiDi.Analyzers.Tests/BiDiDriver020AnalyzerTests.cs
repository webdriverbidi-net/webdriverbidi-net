// <copyright file="BiDiDriver020AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver020 analyzer.
/// </summary>
public class BiDiDriver020AnalyzerTests
{
    [Fact]
    public async Task WaitForAsync_WithoutStartCapturing_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        Task[] tasks = await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitForCapturedTasksAsync_WithoutStartCapturing_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        await {|#0:observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksCompleteAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitForAsync_AfterStartCapturing_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitForCapturedTasksAsync_AfterStartCapturing_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        bool occurred = await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitForAsync_AfterStopCapturing_ReportsError()
    {
        // StartCapturing then StopCapturing leaves no active session.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        observer.StopCapturingTasks();
                        Task[] tasks = await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitForAsync_AfterStopAndRestart_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Task[] first = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        observer.StartCapturingTasks();
                        Task[] second = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task MultipleObservers_OnlyOneWithoutStartCapturing_ReportsOneError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> obs1 = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> obs2 = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        obs1.StartCapturingTasks();
                        Task[] tasks1 = await obs1.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        Task[] tasks2 = await {|#0:obs2.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "obs2");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task ObserverPassedAsParameter_NoDiagnostic()
    {
        // Parameter-passed observers are not tracked; we can't know their capture state.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(EventObserver<NavigationEventArgs> observer)
                    {
                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task MethodWithoutBody_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public interface ITestInterface
                {
                    Task TestMethod();
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task GetCapturedTasks_WithoutStartCapturing_NoDiagnostic()
    {
        // GetCapturedTasks is synchronous and returns empty when no session is active — not an error.
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        Task[] tasks = observer.GetCapturedTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task BothWaitMethods_WithoutStartCapturing_ReportsTwoErrors()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        Task[] tasks = await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                        bool ok = await {|#1:observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        DiagnosticResult expected1 = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(1)
            .WithArguments("WaitForCapturedTasksCompleteAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected0, expected1);
    }

    /// <summary>
    /// Tests that an invocation whose expression is a bare identifier rather than a member access is
    /// skipped while scanning a method for capture-session calls.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonMemberAccessInvocation_IsSkipped()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private static void Helper() { }

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();

                        // Invocation whose expression is a bare identifier, not a member access.
                        Helper();

                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitForCapturedTasksAsync_InsideLambdaDeclaredBeforeStartCapturing_NoDiagnostic()
    {
        // The WaitForCapturedTasksAsync call lives inside a lambda whose body runs only when the
        // delegate is invoked, after StartCapturingTasks has been called. Because the walk must not
        // descend into nested-function bodies, the call is not judged against the textual position of
        // the lambda declaration (which precedes StartCapturingTasks), so no diagnostic is reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        Func<Task> waiter = async () => await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        observer.StartCapturingTasks();
                        await waiter();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task StopCapturingInThenBranch_WaitInElseBranch_NoDiagnostic()
    {
        // The branches are mutually exclusive: the else branch runs only when the capture
        // session was not stopped, so the wait there is valid and must not be reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldStop)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        if (shouldStop)
                        {
                            observer.StopCapturingTasks();
                        }
                        else
                        {
                            await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task ConditionalStartCapturing_WaitAfterBranch_NoDiagnostic()
    {
        // A capture session is active on at least one path through the branch, so the
        // wait after it is not certain to fail and must not be reported at Error severity.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldCapture)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        if (shouldCapture)
                        {
                            observer.StartCapturingTasks();
                        }

                        await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterBranchWhereNoPathStartsCapturing_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool condition)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        if (condition)
                        {
                            Console.WriteLine("then branch");
                        }
                        else
                        {
                            Console.WriteLine("else branch");
                        }

                        await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartCapturingInSwitchSection_WaitAfterSwitch_NoDiagnostic()
    {
        // A capture session is active on at least one path through the switch, so the
        // wait after it must not be reported at Error severity.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        switch (mode)
                        {
                            case 0:
                                observer.StartCapturingTasks();
                                break;
                            default:
                                break;
                        }

                        await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturingBeforeSwitch_ConditionalStopInSection_WaitAfterSwitch_NoDiagnostic()
    {
        // The capture session was started before the switch and is stopped on only one
        // path through it, so the wait after the switch is not certain to fail and must
        // not be reported at Error severity.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        switch (mode)
                        {
                            case 0:
                                observer.StopCapturingTasks();
                                break;
                        }

                        await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitInsideIfCondition_WithoutStartCapturing_ReportsError()
    {
        // Invocations in the branch condition execute unconditionally, before either
        // branch, so a wait there is judged against the state at the branch point.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        if (await {|#0:observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(10))|})
                        {
                            Console.WriteLine("completed");
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksCompleteAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task ObserverDeclaredInsideTryBlock_WaitWithoutStart_ReportsError()
    {
        // Observer declarations inside nested blocks (here, a try block) are tracked the
        // same as top-level declarations.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                            await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that an observer declared by a classic <c>using (T x = ...)</c> statement is tracked like one declared by a local declaration statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WaitInsideClassicUsingStatementDeclaration_WithoutStartCapturing_ReportsError()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        using (EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { }))
                        {
                            await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected0);
    }

    [Fact]
    public async Task WaitAfterObserverPassedToHelper_NoDiagnostic()
    {
        // The helper may open the session, which this rule's textual walk cannot see, so the observer's
        // capturing state is unknown and an Error here would be wrong.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private static void BeginCapture(EventObserver<NavigationEventArgs> target)
                    {
                        target.StartCapturingTasks();
                    }

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        BeginCapture(observer);
                        await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterLambdaStartsCapturing_NoDiagnostic()
    {
        // A nested function runs when its delegate is invoked, not where it is written, so a
        // StartCapturingTasks inside one puts the session beyond what a textual walk can place.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        Action begin = () => observer.StartCapturingTasks();
                        begin();
                        await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterObserverStoredInAField_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private EventObserver<NavigationEventArgs>? shared;

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        this.shared = observer;
                        await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterObserverPlacedInACollection_NoDiagnostic()
    {
        // A collection expression element and a collection initializer both hand the observer to code
        // that may open a session on it.
        string testCode = """
            using System;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> first = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> second = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs>[] pooled = [first];
                        List<EventObserver<NavigationEventArgs>> listed = new List<EventObserver<NavigationEventArgs>> { second };
                        await first.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                        await second.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterObserverAliasedToAnotherLocal_NoDiagnostic()
    {
        // The alias may have a session opened on it under its own name, so neither name is tracked.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> alias = observer;
                        alias.StartCapturingTasks();
                        await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterObserverYielded_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public IEnumerable<EventObserver<NavigationEventArgs>> TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        yield return observer;
                        _ = observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterObserverReturned_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public EventObserver<NavigationEventArgs> TestMethod(bool early)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        if (early)
                        {
                            return observer;
                        }

                        _ = observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                        return observer;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitOnAnUnrelatedObserverStillReportsWhenAnotherEscapes_ReportsError()
    {
        // Treating an escape as method-wide would lose this genuine error. Only the escaping name stops
        // being tracked.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private static void BeginCapture(EventObserver<NavigationEventArgs> target)
                    {
                        target.StartCapturingTasks();
                    }

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> handedOut = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> kept = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        BeginCapture(handedOut);
                        await handedOut.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                        await {|#0:kept.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksCompleteAsync", "kept");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that an observer obtained from something the walk cannot see into is not tracked, so a
    /// capture session the helper already opened is not reported as missing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WaitForCapturedTasks_OnObserverFromHelper_NoDiagnostic()
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
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = CreateCapturingObserver(driver);
                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                    }

                    private EventObserver<EntryAddedEventArgs> CreateCapturingObserver(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        observer.StartCapturingTasks();
                        return observer;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver020_CaptureSessionNotStartedAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an observer declared without an initializer at all is likewise not tracked.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WaitForCapturedTasks_OnObserverDeclaredWithoutInitializer_NoDiagnostic()
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
                    public async Task TestMethod(BiDiDriver driver, bool flag)
                    {
                        EventObserver<EntryAddedEventArgs> observer;
                        observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        observer.StartCapturingTasks();
                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver020_CaptureSessionNotStartedAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that declarations this rule has no business tracking are left alone: one whose
    /// initializer is not a call at all, and one initialized from a subscription call that is not
    /// AddObserver and so yields no observer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WaitForCapturedTasks_WithAliasDeclaration_NoDiagnostic()
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
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(args => Task.CompletedTask);
                        observer.StartCapturingTasks();
                        EventObserver<EntryAddedEventArgs> alias = observer;
                        EventDataCollector<EntryAddedEventArgs> collector = driver.Log.OnEntryAdded.AddDataCollector();
                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                        collector.Dispose();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver020_CaptureSessionNotStartedAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WaitAfterTryWhoseRethrowingCatchStops_NoDiagnostic()
    {
        // A stop in a catch that rethrows is on a path that never reaches the wait, so the wait is not reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        try
                        {
                            await Task.Delay(1);
                        }
                        catch (Exception) when (abort)
                        {
                            observer.StopCapturingTasks();
                            throw;
                        }

                        await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterTryWhoseFinallyStops_ReportsError()
    {
        // A finally block runs on every way out of the try, so a stop there certainly ends the session before the wait.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        try
                        {
                            await Task.Delay(1);
                        }
                        finally
                        {
                            observer.StopCapturingTasks();
                        }

                        await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitInCatchAfterStartInTry_NoDiagnostic()
    {
        // A catch may begin after the start in the try has run, so a wait in the catch is not reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        try
                        {
                            observer.StartCapturingTasks();
                            await Task.Delay(1);
                        }
                        catch (Exception)
                        {
                            await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitInCatchWithoutStart_ReportsError()
    {
        // No path through the try opens a session, so a wait in the catch certainly has none.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        try
                        {
                            await Task.Delay(1);
                        }
                        catch (Exception)
                        {
                            await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5))|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitInFinallyAfterStopInTry_NoDiagnostic()
    {
        // A finally may run after an exception thrown before the stop in the try, when the session is still open, so a wait there is not reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        try
                        {
                            await Task.Delay(1);
                            observer.StopCapturingTasks();
                        }
                        finally
                        {
                            await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitInFinallyWithoutStart_ReportsError()
    {
        // No path into the finally has a session, so a wait there is reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        try
                        {
                            await Task.Delay(1);
                        }
                        finally
                        {
                            await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5))|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitAfterForeachLoopThatStops_NoDiagnostic()
    {
        // The loop body may never run, so a stop inside it does not certainly end the session.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string[] urls)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        foreach (string url in urls)
                        {
                            observer.StopCapturingTasks();
                        }

                        await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterForLoopThatStops_NoDiagnostic()
    {
        // A for loop body, like a foreach body, may never run.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int count)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        for (int i = 0; i < count; i++)
                        {
                            observer.StopCapturingTasks();
                        }

                        await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitAfterWhileLoopThatStops_NoDiagnostic()
    {
        // A while loop body, like a foreach body, may never run.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        while (abort)
                        {
                            observer.StopCapturingTasks();
                        }

                        await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode);
    }

    [Fact]
    public async Task WaitInsideLoopBodyWithoutStart_ReportsError()
    {
        // Inside the body, the state is the state at loop entry, so a wait with no session opened before it is reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string[] urls)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        foreach (string url in urls)
                        {
                            await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5))|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task WaitAfterDoWhileLoopThatStops_ReportsError()
    {
        // A do...while body runs at least once, so a stop inside it certainly ends the session.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool abort)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        do
                        {
                            observer.StopCapturingTasks();
                        }
                        while (abort);

                        await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5))|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver020_CaptureSessionNotStartedAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task TopLevelProgram_SameNameUsedInsideLaterClass_StillReportsError()
    {
        // A top-level program's compilation unit holds the types the file declares as well as the
        // program's own statements. The escape scan is scoped to the global statements, so a same-named
        // identifier handed on inside a class declared later in the file is not the program's local
        // observer escaping, and does not stop the local being tracked.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            BiDiDriver driver = new();
            EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
            Task[] tasks = await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10))|};

            public class Helper
            {
                public static void Store(object observer) => Keep(observer);

                private static void Keep(object value)
                {
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WaitForCapturedTasksAsync", "observer");

        RealAssemblyAnalyzerTest<BiDiDriver020_CaptureSessionNotStartedAnalyzer> testState = new()
        {
            TestCode = testCode,
            TestState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
