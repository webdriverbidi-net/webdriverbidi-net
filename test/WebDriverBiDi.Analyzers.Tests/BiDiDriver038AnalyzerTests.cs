// <copyright file="BiDiDriver038AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver038 analyzer.
/// </summary>
/// <remarks>
/// The control-flow walk these tests exercise is the one BIDI029 uses, so the cases here cover what
/// this rule adds to it: which observer members throw after disposal, that a synchronous
/// <c>Dispose</c> counts as one, and that only a call on the observer itself is judged.
/// </remarks>
public class BiDiDriver038AnalyzerTests
{
    [Fact]
    public async Task StartCapturingTasksAfterDispose_ReportsError()
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
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.Dispose();
                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("StartCapturingTasks", "observer"));
    }

    [Fact]
    public async Task GetCapturedTasksAfterDisposeAsync_ReportsError()
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
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        await observer.DisposeAsync();
                        Task[] tasks = {|#0:observer.GetCapturedTasks()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("GetCapturedTasks", "observer"));
    }

    [Fact]
    public async Task WaitForCapturedTasksAfterDispose_ReportsError()
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
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.Dispose();
                        Task[] tasks = await {|#0:observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(1))|};
                        bool complete = await {|#1:observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(1))|};
                    }
                }
            }
            """;

        await VerifyAsync(
            testCode,
            Expect("WaitForCapturedTasksAsync", "observer"),
            Expect("WaitForCapturedTasksCompleteAsync", "observer", location: 1));
    }

    [Fact]
    public async Task WrappedReceiverAfterDispose_ReportsError()
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
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.Dispose();
                        Task[] tasks = {|#0:observer!.GetCapturedTasks()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("GetCapturedTasks", "observer"));
    }

    [Fact]
    public async Task UsingStatementOverAnExistingObserver_ReportsErrorAfterIt()
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
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        using (observer)
                        {
                            observer.StartCapturingTasks();
                        }

                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("StartCapturingTasks", "observer"));
    }

    [Fact]
    public async Task DisposeInBothBranches_ReportsError()
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
                    public void TestMethod(BiDiDriver driver, bool flag)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        if (flag)
                        {
                            observer.Dispose();
                        }
                        else
                        {
                            observer.Dispose();
                        }

                        {|#0:observer.StartCapturingTasks()|};
                    }
                }
            }
            """;

        await VerifyAsync(testCode, Expect("StartCapturingTasks", "observer"));
    }

    [Fact]
    public async Task DisposeInOneBranchOnly_ReportsNothing()
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
                    public void TestMethod(BiDiDriver driver, bool flag)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        if (flag)
                        {
                            observer.Dispose();
                        }

                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task UnguardedMembersAfterDispose_ReportNothing()
    {
        // Unobserve and StopCapturingTasks are no-ops on a disposed observer, disposal is idempotent,
        // and the last two calls are not the observer's members at all.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.Dispose();
                        observer.Unobserve();
                        observer.StopCapturingTasks();
                        observer.Dispose();
                        string text = observer.ToString();
                        Type type = observer.GetType();
                        string trimmed = text.Trim();
                        string upper = "literal".ToUpperInvariant();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task UseBeforeDispose_ReportsNothing()
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
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.StartCapturingTasks();
                        observer.Dispose();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task UsingDeclaration_ReportsNothing()
    {
        // A using declaration disposes at the end of the block, after every statement the walk reaches.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        using EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task ObserverRebound_ReportsNothing()
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
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.Dispose();
                        observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task DisposedInsideNestedFunction_ReportsNothing()
    {
        // The lambda runs when it is invoked, not where it is written, so the walk cannot place the
        // disposal in this member's order.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        Action cleanup = () => observer.Dispose();
                        cleanup();
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task ObserverPassedByReference_ReportsNothing()
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
                    public void TestMethod(BiDiDriver driver)
                    {
                        EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        observer.Dispose();
                        Replace(ref observer);
                        observer.StartCapturingTasks();
                    }

                    private static void Replace(ref EventObserver<EntryAddedEventArgs> observer)
                    {
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task ObserverHeldInField_ReportsNothing()
    {
        // Only locals are tracked: a field may be disposed by any member of the type.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Log;

            namespace TestApp
            {
                public class TestClass
                {
                    private EventObserver<EntryAddedEventArgs>? observer;

                    public void TestMethod(BiDiDriver driver)
                    {
                        this.observer = driver.Log.OnEntryAdded.AddObserver(e => { });
                        this.observer.Dispose();
                        this.observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task SimilarlyNamedMemberOnAnotherType_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace TestApp
            {
                public class FakeObserver : IDisposable
                {
                    public void Dispose()
                    {
                    }

                    public void StartCapturingTasks()
                    {
                    }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        FakeObserver observer = new FakeObserver();
                        observer.Dispose();
                        observer.StartCapturingTasks();
                    }
                }
            }
            """;

        await VerifyAsync(testCode);
    }

    private static DiagnosticResult Expect(string methodName, string variableName, int location = 0)
    {
        return new DiagnosticResult(BiDiDriver038_ObserverUseAfterDisposalAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(location)
            .WithArguments(methodName, variableName);
    }

    private static async Task VerifyAsync(string testCode, params DiagnosticResult[] expected)
    {
        RealAssemblyAnalyzerTest<BiDiDriver038_ObserverUseAfterDisposalAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.AddRange(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
