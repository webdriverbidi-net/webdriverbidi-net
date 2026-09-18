// <copyright file="BiDiDriver021AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver021 analyzer.
/// </summary>
public class BiDiDriver021AnalyzerTests
{
    [Fact]
    public async Task StartCapturing_NeverRead_ReportsWarning()
    {
        string testCode = """
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
                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartCapturing_FollowedByWaitForAsync_NoDiagnostic()
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturing_FollowedByWaitForCapturedTasksAsync_NoDiagnostic()
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturing_FollowedByGetCapturedTasks_NoDiagnostic()
    {
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
                        observer.StartCapturingTasks();
                        Task[] tasks = observer.GetCapturedTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturing_ThenStopCapturing_NeverRead_ReportsWarning()
    {
        string testCode = """
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
                        {|#0:observer.StartCapturingTasks()|};
                        observer.StopCapturingTasks();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartCapturing_Twice_SecondNeverRead_ReportsOnlySecond()
    {
        // First session is satisfied; second is not.
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
                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task NoStartCapturing_NoDiagnostic()
    {
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
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task MultipleObservers_OnlyOneUnread_ReportsOneWarning()
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
                        Task[] tasks = await obs1.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        {|#0:obs2.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("obs2");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task ObserverPassedAsParameter_NoDiagnostic()
    {
        // Parameter-passed observers are not tracked.
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(EventObserver<NavigationEventArgs> observer)
                    {
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturing_ReadInNestedBlock_NoDiagnostic()
    {
        // The read is inside an if-block but the analyzer sees it within the same method body.
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
                        observer.StartCapturingTasks();
                        if (condition)
                        {
                            Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task TwoSessionsBothRead_NoDiagnostic()
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task ObserverDeclaredInsideTryBlock_StartWithoutRead_ReportsWarning()
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
                            {|#0:observer.StartCapturingTasks()|};
                            await Task.Delay(100);
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that an observer declared by a classic <c>using (T x = ...)</c> statement is tracked like one declared by a local declaration statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ObserverDeclaredInClassicUsingStatement_StartWithoutRead_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        using (EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { }))
                        {
                            {|#0:observer.StartCapturingTasks()|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that a read inside a lambda (for example one handed to <c>Task.Run</c>) counts as reading the session, wherever the lambda is written: it runs when invoked, which the textual walk cannot place.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartCapturing_ReadInsideLambda_NoDiagnostic()
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
                        EventObserver<NavigationEventArgs> before = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        Func<Task> readBefore = () => before.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        before.StartCapturingTasks();
                        await readBefore();

                        EventObserver<NavigationEventArgs> after = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        after.StartCapturingTasks();
                        await Task.Run(() => after.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10)));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a StartCapturingTasks inside a lambda is not tracked: it runs when the delegate is invoked, so the reads that follow it textually cannot be judged against it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartCapturingInsideLambda_IsNotTracked()
    {
        string testCode = """
            using System;
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
                        Action start = () => observer.StartCapturingTasks();
                        start();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturing_ObserverPassedToHelper_NoDiagnostic()
    {
        // The helper may read the session, which this rule cannot see, so the observer is not tracked.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private static void Drain(EventObserver<NavigationEventArgs> target)
                    {
                        target.GetCapturedTasks();
                    }

                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Drain(observer);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturing_ObserverReturned_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public EventObserver<NavigationEventArgs> TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        return observer;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartCapturing_OnAKeptObserver_StillReportsWhenAnotherEscapes()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private static void Drain(EventObserver<NavigationEventArgs> target)
                    {
                        target.GetCapturedTasks();
                    }

                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> handedOut = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> kept = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        handedOut.StartCapturingTasks();
                        Drain(handedOut);
                        {|#0:kept.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("kept");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a read inside a local function counts as reading the session, as a read inside a lambda
    /// does. The reference names both, but only the lambda form was exercised.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartCapturing_ReadInsideLocalFunction_NoDiagnostic()
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
                        await DrainAsync();

                        async Task DrainAsync()
                        {
                            await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }
}
