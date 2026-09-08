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
    public async Task StartInsideConditionOfIf_ReportsWarning()
    {
        // A call in the condition executes unconditionally, so a start there precedes both branches.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public bool Capture(EventObserver<NavigationEventArgs> observer)
                    {
                        observer.StartCapturingTasks();
                        return true;
                    }

                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.StartCapturingTasks();
                        if (this.Capture(observer))
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
}
