// <copyright file="BiDiDriver031AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver031 analyzer.
/// </summary>
public class BiDiDriver031AnalyzerTests
{
    [Fact]
    public async Task AddObserver_ResultDiscarded_ReportsInfo()
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
                        {|#0:driver.BrowsingContext.OnLoad.AddObserver(args => { })|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver031_DiscardedObserverResultAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EventObserver", "AddObserver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task AddObserver_WithOptionsArgumentDiscarded_ReportsInfo()
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
                        {|#0:driver.BrowsingContext.OnLoad.AddObserver(async args => await Task.Yield(), ObservableEventHandlerOptions.RunHandlerAsynchronously)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver031_DiscardedObserverResultAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EventObserver", "AddObserver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task AddObserver_ResultAssignedToVariable_ReportsNothing()
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
                        observer.Unobserve();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    [Fact]
    public async Task AddObserver_ResultExplicitlyDiscarded_ReportsNothing()
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
                        _ = driver.BrowsingContext.OnLoad.AddObserver(args => { });
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    [Fact]
    public async Task OtherInvocationStatement_ReportsNothing()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(string url)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync(url);
                        driver.ToString();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    [Fact]
    public async Task DelegateInvocationWithoutInvokedName_ReportsNothing()
    {
        // The invoked name is not syntactically evident here, so the pre-filter admits the invocation and
        // the resolved symbol (Action.Invoke) is what rules it out.
        string testCode = """
            using System;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        new Action(() => { })();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    [Fact]
    public async Task UnrelatedAddObserverReturningOtherType_ReportsNothing()
    {
        string testCode = """
            using System;

            namespace TestNamespace
            {
                public class UnrelatedEvent
                {
                    public string AddObserver(Action handler) => "token";
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        UnrelatedEvent unrelated = new();
                        unrelated.AddObserver(() => { });
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    [Fact]
    public async Task LateBoundAddObserver_ReportsNothing()
    {
        // A dynamic invocation resolves to no symbol, so the return type cannot be examined and nothing
        // is reported.
        string testCode = """
            using System;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(dynamic observableEvent)
                    {
                        observableEvent.AddObserver((Action)(() => { }));
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    [Fact]
    public async Task AddDataCollectorDiscarded_ReportsInfo()
    {
        // A discarded collector keeps receiving and queueing events with no handle to stop it.
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
                        {|#0:driver.BrowsingContext.OnLoad.AddDataCollector()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver031_DiscardedObserverResultAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EventDataCollector", "AddDataCollector");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task SubscribeResultDiscarded_ReportsInfo()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(IObserver<NavigationEventArgs> handler)
                    {
                        BiDiDriver driver = new();
                        {|#0:driver.BrowsingContext.OnLoad.ToObservable().Subscribe(handler)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver031_DiscardedObserverResultAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("ObservableEventSubscription", "Subscribe");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task SubscribeOnAnUnrelatedObservableDiscarded_ReportsNothing()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(IObservable<int> numbers, IObserver<int> handler)
                    {
                        numbers.Subscribe(handler);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    [Fact]
    public async Task AddDataCollectorResultKept_ReportsNothing()
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
                        using EventDataCollector<NavigationEventArgs> collector = driver.BrowsingContext.OnLoad.AddDataCollector();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a handle is discarded when the subscription is made through null-conditional receivers: the value
    /// the statement drops is that of the whole conditional access, which is the handle or null.
    /// </summary>
    /// <param name="receiver">The receiver chain, with its null-conditional accesses, up to the subscription.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("driver?{|#0:.BrowsingContext.OnLoad.AddObserver(args => { })|}")]
    [InlineData("driver?.BrowsingContext?{|#0:.OnLoad.AddObserver(args => { })|}")]
    public async Task AddObserverThroughNullConditionalReceiver_ResultDiscarded_ReportsInfo(string receiver)
    {
        string testCode = $$"""
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver? driver)
                    {
                        {{receiver}};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver031_DiscardedObserverResultAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EventObserver", "AddObserver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a handle used through a null-conditional access of its own is not discarded: the statement drops
    /// the value of the member called on the handle, not the handle.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddObserverResultUsedThroughNullConditionalAccess_ReportsNothing()
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
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.BrowsingContext.OnLoad.AddObserver(args => { })?.Dispose();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver031_DiscardedObserverResultAnalyzer>(testCode);
    }
}
