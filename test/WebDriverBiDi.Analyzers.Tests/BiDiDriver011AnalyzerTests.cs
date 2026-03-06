// <copyright file="BiDiDriver011AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver011 analyzer.
/// </summary>
public class BiDiDriver011AnalyzerTests
{
    [Test]
    public async Task SetCheckpoint_WithoutWaitOrUnset_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.SetCheckpoint(5);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver011_EventObserverCheckpointMisuseAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(13, 13, 13, 38)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task SetCheckpoint_WithWaitForCheckpointAsync_NoDiagnostic()
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
                        observer.SetCheckpoint(5);
                        await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }

    [Test]
    public async Task SetCheckpoint_WithWaitForCheckpointAndTasksAsync_NoDiagnostic()
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
                        observer.SetCheckpoint(5);
                        await observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }

    [Test]
    public async Task SetCheckpoint_WithGetCheckpointTasks_NoDiagnostic()
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
                        observer.SetCheckpoint(5);
                        Task[] tasks = observer.GetCheckpointTasks();
                        await Task.WhenAll(tasks);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }

    [Test]
    public async Task SetCheckpoint_WithUnsetCheckpoint_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.SetCheckpoint(5);
                        observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }

    [Test]
    public async Task NoSetCheckpoint_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }

    [Test]
    public async Task MultipleObservers_SetCheckpointOnOne_WithoutWait_ReportsWarning()
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
                        EventObserver<NavigationEventArgs> observer1 = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> observer2 = driver.BrowsingContext.OnLoad.AddObserver(args => { });

                        observer1.SetCheckpoint(5);
                        await observer1.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

                        observer2.SetCheckpoint(3);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver011_EventObserverCheckpointMisuseAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(19, 13, 19, 39)
            .WithArguments("observer2");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task SetCheckpoint_InConditionalBranch_WithoutWait_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(bool condition)
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });

                        if (condition)
                        {
                            observer.SetCheckpoint(5);
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver011_EventObserverCheckpointMisuseAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(16, 17, 16, 42)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task SetCheckpoint_WithWaitInConditionalBranch_NoDiagnostic()
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
                            observer.SetCheckpoint(5);
                            await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }

    [Test]
    public async Task MethodWithoutBody_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public interface ITestInterface
                {
                    void TestMethod();
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }

    [Test]
    public async Task SetCheckpoint_OnDifferentVariable_WithWaitOnOriginal_ReportsWarning()
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
                        EventObserver<NavigationEventArgs> observer1 = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        EventObserver<NavigationEventArgs> observer2 = driver.BrowsingContext.OnLoad.AddObserver(args => { });

                        observer1.SetCheckpoint(5);
                        await observer2.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver011_EventObserverCheckpointMisuseAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(16, 13, 16, 39)
            .WithArguments("observer1");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task SetCheckpoint_InTryCatchFinally_WithWaitInFinally_NoDiagnostic()
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

                        try
                        {
                            observer.SetCheckpoint(5);
                        }
                        finally
                        {
                            await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver011_EventObserverCheckpointMisuseAnalyzer>(testCode);
    }
}
