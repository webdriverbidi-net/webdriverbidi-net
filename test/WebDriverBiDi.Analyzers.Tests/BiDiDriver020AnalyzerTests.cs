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
}
