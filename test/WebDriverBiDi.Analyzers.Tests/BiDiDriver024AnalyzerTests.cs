// <copyright file="BiDiDriver024AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver024 analyzer, which flags a second StartAsync call on a driver with no
/// intervening StopAsync.
/// </summary>
public class BiDiDriver024AnalyzerTests
{
    [Fact]
    public async Task SecondStartAsync_WithoutStopAsync_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await {|#0:driver.StartAsync("ws://localhost:9222")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver024_DuplicateStartAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver024_DuplicateStartAsyncAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SingleStartAsync_ThenCommand_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartStopStart_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncInsideNestedFunction_NotCountedAsDuplicate()
    {
        string testCode = """
            using WebDriverBiDi;
            using System;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        // A StartAsync inside a nested function runs when the delegate is invoked, not
                        // here, so it must not be treated as a second start of the driver.
                        Func<Task> reconnect = async () => await driver.StartAsync("ws://localhost:9222");
                        await reconnect();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncInMutuallyExclusiveIfElseBranches_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool useLocal)
                    {
                        BiDiDriver driver = new();
                        if (useLocal)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        else
                        {
                            await driver.StartAsync("ws://remotehost:9222");
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncInMutuallyExclusiveSwitchSections_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int endpoint)
                    {
                        BiDiDriver driver = new();
                        switch (endpoint)
                        {
                            case 1:
                                await driver.StartAsync("ws://localhost:9222");
                                break;
                            default:
                                await driver.StartAsync("ws://remotehost:9222");
                                break;
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncAfterBothIfElseBranchesStarted_ReportsError()
    {
        // The driver is started on every path through the if/else, so a subsequent
        // StartAsync is a definite duplicate.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool useLocal)
                    {
                        BiDiDriver driver = new();
                        if (useLocal)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        else
                        {
                            await driver.StartAsync("ws://remotehost:9222");
                        }

                        await {|#0:driver.StartAsync("ws://localhost:9222")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver024_DuplicateStartAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver024_DuplicateStartAsyncAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DuplicateStartAsyncWithinSameBranch_ReportsError()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool useLocal)
                    {
                        BiDiDriver driver = new();
                        if (useLocal)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            await {|#0:driver.StartAsync("ws://localhost:9222")|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver024_DuplicateStartAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver024_DuplicateStartAsyncAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task StartAsyncAfterConditionalStart_NoDiagnostic()
    {
        // The driver is started on only one of the paths through the if, so the later
        // StartAsync is not a definite duplicate; the analyzer deliberately reports only
        // duplicates that occur on every path, because flagging "started on some path"
        // would break correct conditional start patterns at Error severity.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool alreadyConnected)
                    {
                        BiDiDriver driver = new();
                        if (alreadyConnected)
                        {
                            await driver.StopAsync();
                        }

                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncAfterSwitchWithoutDefault_NoDiagnostic()
    {
        // A switch with no default section may match nothing, so a StartAsync inside one of
        // its sections does not definitely start the driver, and a StartAsync after the
        // switch is not a definite duplicate.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int endpoint)
                    {
                        BiDiDriver driver = new();
                        switch (endpoint)
                        {
                            case 1:
                                await driver.StartAsync("ws://localhost:9222");
                                break;
                        }

                        await driver.StartAsync("ws://remotehost:9222");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncAfterElseIfChainCoveringAllPaths_ReportsError()
    {
        // An else-if chain arrives at the branch walker as an else clause whose statement is
        // itself an if statement. Every path through the chain starts the driver, so a
        // subsequent StartAsync is a definite duplicate.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int endpoint)
                    {
                        BiDiDriver driver = new();
                        if (endpoint == 1)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        else if (endpoint == 2)
                        {
                            await driver.StartAsync("ws://remotehost:9222");
                        }
                        else
                        {
                            await driver.StartAsync("ws://fallbackhost:9222");
                        }

                        await {|#0:driver.StartAsync("ws://localhost:9222")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver024_DuplicateStartAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver024_DuplicateStartAsyncAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task StartAsyncInsideEveryNestedFunctionKind_NotCountedAsDuplicate()
    {
        // Each nested-function kind — simple lambda, anonymous method, and local function —
        // runs when its delegate is invoked, not at its textual position, so a StartAsync in
        // any of them must not be treated as a second start of the driver.
        string testCode = """
            using WebDriverBiDi;
            using System;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        Func<object, Task> viaSimpleLambda = x => driver.StartAsync("ws://localhost:9222");
                        Func<Task> viaAnonymousMethod = delegate { return driver.StartAsync("ws://localhost:9222"); };
                        Task ViaLocalFunction() => driver.StartAsync("ws://localhost:9222");

                        await viaSimpleLambda(driver);
                        await viaAnonymousMethod();
                        await ViaLocalFunction();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task NonDriverInvocationsAndDeclarations_NoDiagnostic()
    {
        string testCode = """
            using System;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        int counter = 0;          // non-driver declaration
                        int uninitialized;        // declaration without initializer
                        uninitialized = counter;
                        NoOp();                   // invocation that is not a member access
                        _ = counter.ToString();   // member access on an untracked identifier
                        Console.WriteLine(uninitialized);
                    }

                    private void NoOp()
                    {
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }
}
