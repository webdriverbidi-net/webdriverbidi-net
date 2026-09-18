// <copyright file="BiDiDriver030AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver030 analyzer.
/// </summary>
public class BiDiDriver030AnalyzerTests
{
    [Fact]
    public async Task SecondStartCapturingTasks_ReportsWarning()
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
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartAfterStopCapturingTasks_ReportsNothing()
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
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        observer.StopCapturingTasks();
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAfterWaitForCapturedTasks_ReportsNothing()
    {
        // A wait that collects its full batch ends the session itself, so a start after one is not a
        // duplicate.
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
                        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
                        observer.StartCapturingTasks();
                        bool complete = await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5));
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task SingleStartCapturingTasks_ReportsNothing()
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
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        observer.Unobserve();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartInOneBranchOnly_ReportsNothing()
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
                    public void TestMethod(bool capture)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        if (capture)
                        {
                            observer.StartCapturingTasks();
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StopInOneBranchOnly_ReportsNothing()
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
                    public void TestMethod(bool release)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        if (release)
                        {
                            observer.StopCapturingTasks();
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartInSwitchSectionWithNoPriorSession_ReportsNothing()
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
                    public void TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        switch (mode)
                        {
                            case 1:
                                observer.StartCapturingTasks();
                                break;
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartInBothBranches_ReportsWarning()
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
                    public void TestMethod(bool capture)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        if (capture)
                        {
                            observer.StartCapturingTasks();
                        }
                        else
                        {
                            observer.StartCapturingTasks();
                        }

                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task ReadInsideConditionOfIf_EndsTheSessionForBothBranches()
    {
        // A call in the condition executes unconditionally, before either branch and before anything
        // that follows the if. The read there ends the session, so neither later start is a duplicate.
        // Were the condition not walked, the start after the if would be reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool flag)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        if (await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(5)))
                        {
                            observer.StartCapturingTasks();
                            observer.StopCapturingTasks();
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartInEverySwitchSection_ReportsWarning()
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
                    public void TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        switch (mode)
                        {
                            case 1:
                                observer.StopCapturingTasks();
                                observer.StartCapturingTasks();
                                break;
                            default:
                                observer.StopCapturingTasks();
                                observer.StartCapturingTasks();
                                break;
                        }

                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StopInOneSwitchSection_ReportsNothing()
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
                    public void TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        switch (mode)
                        {
                            case 1:
                                observer.StopCapturingTasks();
                                break;
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartInsideNestedFunction_ReportsNothing()
    {
        // A lambda body runs when the delegate is invoked, not at its textual position, so a start there
        // is not judged against the state at that point.
        string testCode = """
            using System;
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
                        Action restart = () => observer.StartCapturingTasks();
                        restart();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartOnUntrackedReceivers_ReportsNothing()
    {
        // A parameter-supplied observer is not tracked, a non-observer local is not tracked, and a call
        // whose receiver is not a plain identifier is not matched.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class Holder
                {
                    public EventObserver<NavigationEventArgs> Observer { get; set; } = null!;
                }

                public class TestClass
                {
                    public void TestMethod(EventObserver<NavigationEventArgs> parameterObserver, Holder holder)
                    {
                        int counter = 0;
                        counter++;
                        parameterObserver.StartCapturingTasks();
                        parameterObserver.StartCapturingTasks();
                        holder.Observer.StartCapturingTasks();
                        holder.Observer.StartCapturingTasks();
                        Console.WriteLine(counter);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task DuplicateStartInTopLevelProgram_ReportsWarning()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            BiDiDriver driver = new();
            EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
            observer.StartCapturingTasks();
            {|#0:observer.StartCapturingTasks()|};
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        RealAssemblyAnalyzerTest<BiDiDriver030_DuplicateCaptureSessionAnalyzer> testState = new()
        {
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task StartAfterObserverPassedToHelper_NoDiagnostic()
    {
        // The helper may end the session, which this rule cannot see, so the second start is not known
        // to be a duplicate.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private static void Release(EventObserver<NavigationEventArgs> target)
                    {
                        target.StopCapturingTasks();
                    }

                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Release(observer);
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAfterLambdaStopsCapturing_NoDiagnostic()
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
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        Action release = () => observer.StopCapturingTasks();
                        release();
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartOnAKeptObserver_StillReportsWhenAnotherEscapes()
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
                    private static void Release(EventObserver<NavigationEventArgs> target)
                    {
                        target.StopCapturingTasks();
                    }

                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> handedOut = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> kept = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        handedOut.StartCapturingTasks();
                        Release(handedOut);
                        handedOut.StartCapturingTasks();
                        kept.StartCapturingTasks();
                        {|#0:kept.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("kept");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartAfterObserverEscapesThroughCast_ReportsNothing()
    {
        // The observer is handed to other code through a cast, which may open or close a session
        // this rule cannot see, so the observer is not tracked and neither start is reported.
        string testCode = """
            using System;
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
                        Keep((IDisposable)observer);
                        observer.StartCapturingTasks();
                        observer.StartCapturingTasks();
                    }

                    private static void Keep(IDisposable disposable) { }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAfterStartInsideForLoop_ReportsNothing()
    {
        // A for loop's body may run zero times, so a session opened inside it is not certainly active
        // for the start that follows the loop. The declaration, condition and incrementor are walked
        // too, so a start placed in any of them is judged in its own position.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(int count)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        for (int i = 0; i < count; i++)
                        {
                            observer.StartCapturingTasks();
                            break;
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAfterStartInsideWhileLoop_ReportsNothing()
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
                    public void TestMethod(bool flag)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        while (flag)
                        {
                            observer.StartCapturingTasks();
                            break;
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task SecondStartInsideLoopBody_ReportsWarning()
    {
        // Inside the body the session opened earlier in the same body is certainly active, so a second
        // start there is still reported.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(string[] urls)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        foreach (string url in urls)
                        {
                            observer.StartCapturingTasks();
                            {|#0:observer.StartCapturingTasks()|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartAfterLoopWithSessionOpenedBeforeIt_ReportsWarning()
    {
        // A session opened before the loop is certainly active afterwards whether or not the body runs.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(string[] urls)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        foreach (string url in urls)
                        {
                            Console.WriteLine(url);
                        }

                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartAfterTryThatStarts_ReportsWarning()
    {
        // The try block completing is the only way to reach the second start, and it leaves a session active.
        // The empty finally does not change that.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        try
                        {
                            observer.StartCapturingTasks();
                            await Task.Delay(1);
                        }
                        finally
                        {
                        }

                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartAfterTryWhoseCatchDoesNotStart_ReportsNothing()
    {
        // A catch that completes without a session is another way out of the try, so the second start is not certain to be a duplicate.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        try
                        {
                            observer.StartCapturingTasks();
                            await Task.Delay(1);
                        }
                        catch (Exception) when (retry)
                        {
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartInCatchWhenStartedBeforeTry_ReportsWarning()
    {
        // A session opened before the try, with nothing in the try ending it, is certainly active in the catch.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        try
                        {
                            await Task.Delay(1);
                        }
                        catch (Exception)
                        {
                            {|#0:observer.StartCapturingTasks()|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartInCatchAfterStartInTry_ReportsNothing()
    {
        // A catch may begin before the start in the try has run, so a start in the catch is not certain to be a duplicate.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
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
                            observer.StartCapturingTasks();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAfterTryWhoseFinallyStarts_ReportsWarning()
    {
        // A finally runs on every way out of the try, so a start there certainly leaves a session active.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        try
                        {
                            await Task.Delay(1);
                        }
                        finally
                        {
                            observer.StartCapturingTasks();
                        }

                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task StartAfterTryWhoseFinallyStops_ReportsNothing()
    {
        // A stop in a finally certainly ends the session opened before the try.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool retry)
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

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartInFinallyAfterStartInTry_ReportsNothing()
    {
        // The finally may run after an exception thrown before the start in the try, when no session is
        // active, so a start in the finally is not certain to be a duplicate.
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
                        try
                        {
                            await Task.Delay(1);
                            observer.StartCapturingTasks();
                        }
                        finally
                        {
                            observer.StartCapturingTasks();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAfterReassigningObserver_ReportsNothing()
    {
        // The assignment binds the name to a different observer, which has no session of its own, so the
        // start after it is not a duplicate; before this case was handled the rule reported it.
        string testCode = """
            using System;
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
                        observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode);
    }

    [Fact]
    public async Task AssignmentToOtherVariable_DoesNotResetSession()
    {
        // Only an assignment to the tracked observer variable itself replaces the observer.
        string testCode = """
            using System;
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
                        int count = 0;
                        observer.StartCapturingTasks();
                        count = 1;
                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver030_DuplicateCaptureSessionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver030_DuplicateCaptureSessionAnalyzer>(testCode, expected);
    }
}
