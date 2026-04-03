// <copyright file="BiDiDriver019AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver019 analyzer.
/// </summary>
public class BiDiDriver019AnalyzerTests
{
    [Test]
    public async Task UnsetCheckpoint_WithoutGetCheckpointTasks_ReportsWarning()
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

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(14, 13, 14, 39)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_WithGetCheckpointTasksBeforeIt_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
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
                        observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_WithGetCheckpointTasksAfterIt_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
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
                        observer.UnsetCheckpoint();
                        Task[] tasks = observer.GetCheckpointTasks();
                        await Task.WhenAll(tasks);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(14, 13, 14, 39)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_InFinallyWithGetCheckpointTasksInTry_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.SetCheckpoint();
                        try
                        {
                            await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(5));
                            Task[] tasks = observer.GetCheckpointTasks();
                            await Task.WhenAll(tasks);
                        }
                        finally
                        {
                            observer.UnsetCheckpoint();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_InFinallyWithoutGetCheckpointTasks_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.SetCheckpoint();
                        try
                        {
                            await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(5));
                        }
                        finally
                        {
                            observer.UnsetCheckpoint();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(21, 17, 21, 43)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task MultipleObservers_UnsetCheckpointOnOneWithoutGetTasks_ReportsWarning()
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
                        Task[] tasks1 = observer1.GetCheckpointTasks();
                        await Task.WhenAll(tasks1);
                        observer1.UnsetCheckpoint();

                        observer2.SetCheckpoint(3);
                        observer2.UnsetCheckpoint();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(22, 13, 22, 40)
            .WithArguments("observer2");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_WithGetCheckpointTasksOnDifferentVariable_ReportsWarning()
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
                        Task[] tasks = observer2.GetCheckpointTasks();
                        await Task.WhenAll(tasks);
                        observer1.UnsetCheckpoint();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(19, 13, 19, 40)
            .WithArguments("observer1");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task NoUnsetCheckpoint_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
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

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_InNestedBlock_WithGetCheckpointTasksInParentScope_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
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

                        if (true)
                        {
                            observer.UnsetCheckpoint();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_WithGetCheckpointTasksInNestedScope_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
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

                        if (true)
                        {
                            Task[] tasks = observer.GetCheckpointTasks();
                            await Task.WhenAll(tasks);
                        }

                        observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        // GetCheckpointTasks in nested scope doesn't guarantee tasks were retrieved before UnsetCheckpoint
        // because the if block might not execute
        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(22, 13, 22, 39)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_MultipleCallsOnSameObserver_OnlyFirstReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
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
                        observer.UnsetCheckpoint();

                        observer.SetCheckpoint(3);
                        Task[] tasks = observer.GetCheckpointTasks();
                        await Task.WhenAll(tasks);
                        observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(16, 13, 16, 39)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_InExpressionBodiedMethod_WithoutGetCheckpointTasks_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private EventObserver<NavigationEventArgs> observer;

                    public TestClass(BiDiDriver driver)
                    {
                        observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.SetCheckpoint(5);
                    }

                    public void TestMethod() => observer.UnsetCheckpoint();
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(17, 37, 17, 63)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_NoMethodBody_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public abstract class TestClass
                {
                    public abstract void TestMethod(EventObserver<NavigationEventArgs> observer);
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_OnFieldAccess_WithoutGetCheckpointTasks_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private EventObserver<NavigationEventArgs> observer;

                    public TestClass(BiDiDriver driver)
                    {
                        observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                    }

                    public void TestMethod()
                    {
                        observer.SetCheckpoint(5);
                        observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(19, 13, 19, 39)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_OnThisQualifiedMember_WithGetCheckpointTasks_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private EventObserver<NavigationEventArgs> observer;

                    public TestClass(BiDiDriver driver)
                    {
                        this.observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                    }

                    public async Task TestMethod()
                    {
                        this.observer.SetCheckpoint(5);
                        Task[] tasks = this.observer.GetCheckpointTasks();
                        await Task.WhenAll(tasks);
                        this.observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_MultipleGetCheckpointTasksCallsBefore_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
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

                        // First retrieval
                        Task[] tasks1 = observer.GetCheckpointTasks();
                        await Task.WhenAll(tasks1);

                        observer.SetCheckpoint(3);

                        // Second retrieval before unset
                        Task[] tasks2 = observer.GetCheckpointTasks();
                        await Task.WhenAll(tasks2);

                        observer.UnsetCheckpoint();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_InCatchBlock_WithGetCheckpointTasksInTry_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                        observer.SetCheckpoint();
                        try
                        {
                            Task[] tasks = observer.GetCheckpointTasks();
                            await Task.WhenAll(tasks);
                        }
                        catch (Exception)
                        {
                            observer.UnsetCheckpoint();
                        }
                    }
                }
            }
            """;

        // GetCheckpointTasks in try doesn't guarantee it ran before catch executes
        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(22, 17, 22, 43)
            .WithArguments("observer");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task UnsetCheckpoint_ComplexNesting_WithGetCheckpointTasksInGrandparentScope_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;
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

                        if (true)
                        {
                            if (true)
                            {
                                observer.UnsetCheckpoint();
                            }
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }

    [Test]
    public async Task UnsetCheckpoint_WithNoInvocationExpression_NoDiagnostic()
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
                        // No UnsetCheckpoint call at all
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer>(testCode);
    }
}
