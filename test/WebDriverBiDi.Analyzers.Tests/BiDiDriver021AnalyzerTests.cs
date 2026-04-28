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
[TestFixture]
public class BiDiDriver021AnalyzerTests
{
    [Test]
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
                        {|#0:observer.StartCapturing()|};
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

    [Test]
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
                        observer.StartCapturing();
                        Task[] tasks = await observer.WaitForAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Test]
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
                        observer.StartCapturing();
                        bool occurred = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Test]
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
                        observer.StartCapturing();
                        Task[] tasks = observer.GetCapturedTasks();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Test]
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
                        {|#0:observer.StartCapturing()|};
                        observer.StopCapturing();
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

    [Test]
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
                        observer.StartCapturing();
                        Task[] first = await observer.WaitForAsync(1, TimeSpan.FromSeconds(10));
                        {|#0:observer.StartCapturing()|};
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

    [Test]
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

    [Test]
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
                        obs1.StartCapturing();
                        Task[] tasks = await obs1.WaitForAsync(1, TimeSpan.FromSeconds(10));
                        {|#0:obs2.StartCapturing()|};
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

    [Test]
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
                        observer.StartCapturing();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Test]
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
                        observer.StartCapturing();
                        if (condition)
                        {
                            Task[] tasks = await observer.WaitForAsync(1, TimeSpan.FromSeconds(10));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }

    [Test]
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
                        observer.StartCapturing();
                        Task[] first = await observer.WaitForAsync(1, TimeSpan.FromSeconds(10));
                        observer.StartCapturing();
                        Task[] second = await observer.WaitForAsync(1, TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer>(testCode);
    }
}
