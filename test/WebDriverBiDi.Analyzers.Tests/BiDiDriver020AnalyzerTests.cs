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
[TestFixture]
public class BiDiDriver020AnalyzerTests
{
    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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
}
