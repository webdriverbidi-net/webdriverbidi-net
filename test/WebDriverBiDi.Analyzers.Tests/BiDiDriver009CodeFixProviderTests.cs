// <copyright file="BiDiDriver009CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver009 code fix provider.
/// </summary>
public class BiDiDriver009CodeFixProviderTests
{
    [Fact]
    public async Task ExecuteCommandAsync_NoStartAsyncInMethod_NoFixOffered()
    {
        // The fix relocates the command after an existing StartAsync call. When no StartAsync
        // exists anywhere in the method, the diagnostic still fires but no fix can be built,
        // so none may be offered (previously the provider threw while building the fix).
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
                        await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_InTopLevelProgram_NoFixOffered()
    {
        // The fix rearranges statements of a method declaration, which does not exist in a
        // top-level program; the diagnostic still fires there, but no fix may be offered.
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            BiDiDriver driver = new();
            await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
            await driver.StartAsync("ws://localhost:9222");
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
            TestState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
            FixedState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_StartAsyncOnFieldReceiverIgnored_CodeFixMovesAfterMatchingStartAsync()
    {
        // A StartAsync whose receiver chain does not end in a simple identifier (a driver held
        // in a field accessed through `this`) yields no variable name and must not be treated
        // as the start of the local driver; the fix moves the command after the matching
        // StartAsync on the local variable.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private BiDiDriver other = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                        await this.other.StartAsync("ws://otherhost:9222");
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private BiDiDriver other = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await this.other.StartAsync("ws://otherhost:9222");
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_CodeFixMovesAfterStartAsync()
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
                        await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        string fixedCode = """
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

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ModuleCommand_CodeFixMovesAfterStartAsync()
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
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        string fixedCode = """
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
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CommandBeforeStart_WithStatementAfterStart_CodeFixInsertsCorrectly()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;
            using WebDriverBiDi.Script;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.Script.GetRealmsAsync();
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;
            using WebDriverBiDi.Script;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.BrowsingContext.GetTreeAsync();
                        await driver.Script.GetRealmsAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CommandBeforeStart_WithStartAsyncInTryBlock_InsertsInsideTryAfterStartAsync()
    {
        // When StartAsync is nested inside a try block, the moved command must be placed immediately
        // after StartAsync within that same block, not after the whole try/finally statement.
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
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        finally
                        {
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        string fixedCode = """
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
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            await driver.BrowsingContext.GetTreeAsync();
                        }
                        finally
                        {
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithStartAsyncAsEmbeddedStatement_NoFixOffered()
    {
        // An unbraced embedded StartAsync has a statement, not a block, for its parent, so there is no statement list to insert the command into. The fix must decline rather than fail while it is being built.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool startNow)
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        if (startNow)
                            await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithStartAsyncInsideIfBlock_NoFixOffered()
    {
        // Moving the command into the branch would make it conditional, which is a different program from the one the diagnostic asked to correct.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool startNow)
                    {
                        BiDiDriver driver = new();
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        if (startNow)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CommandDeclaringLocalUsedBeforeStart_NoFixOffered()
    {
        // The command declares a local that a statement between it and the start uses. Moving the declaration below that use would not compile.
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
                        GetTreeCommandResult tree = await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        System.Console.WriteLine(tree.ContextTree.Count);
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CommandDeclaringLocalUsedAfterTry_NoFixOffered()
    {
        // The command declares a local used after the try statement. Moving the declaration into the try block would put that use out of scope.
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
                        GetTreeCommandResult tree = await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        finally
                        {
                            await driver.StopAsync();
                        }

                        System.Console.WriteLine(tree.ContextTree.Count);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CommandDeclaringLocalUsedAfterStart_CodeFixMovesAfterStartAsync()
    {
        // A local declared by the command and used only after the start moves with the command, and every use still follows the declaration in the same block.
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
                        GetTreeCommandResult tree = await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        await driver.StartAsync("ws://localhost:9222");
                        System.Console.WriteLine(tree.ContextTree.Count);
                    }
                }
            }
            """;

        string fixedCode = """
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
                        await driver.StartAsync("ws://localhost:9222");
                        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync();
                        System.Console.WriteLine(tree.ContextTree.Count);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CommandBeforeStart_WithStartAsyncInUsingBlock_InsertsInsideUsingAfterStartAsync()
    {
        // A using block is entered unconditionally and exactly once, so the command may move into it just as it may move into a try block.
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
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                        using (System.IO.StringReader reader = new(""))
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                    }
                }
            }
            """;

        string fixedCode = """
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
                        using (System.IO.StringReader reader = new(""))
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            await driver.BrowsingContext.GetTreeAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the moved command takes its own comments with it (the one above it and the one on the same line) and that the comment above StartAsync stays there alone rather than being copied onto the command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommand_WithCommentsOnBothStatements_CodeFixKeepsEachCommentWithItsStatement()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        // Check the status first.
                        await {|#0:driver.Session.StatusAsync()|}; // before start
                        // Connect to the browser.
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        // Connect to the browser.
                        await driver.StartAsync("ws://localhost:9222");
                        // Check the status first.
                        await driver.Session.StatusAsync(); // before start
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId, DiagnosticSeverity.Error).WithLocation(0).WithArguments("StatusAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a comment above the command is re-indented along with the command when the command moves into the deeper block holding StartAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExecuteCommand_WithCommentMovedIntoDeeperBlock_CodeFixReindentsComment()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        // Check the status first.
                        await {|#0:driver.Session.StatusAsync()|};
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            // Check the status first.
                            await driver.Session.StatusAsync();
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId, DiagnosticSeverity.Error).WithLocation(0).WithArguments("StatusAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the fix finds the driver and its StartAsync through wrapped receivers, and moves the command
    /// after that StartAsync.
    /// </summary>
    /// <param name="command">The command, through a wrapped receiver.</param>
    /// <param name="start">The statement starting the driver, through a wrapped receiver.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("driver!.Session.StatusAsync()", "await driver!.StartAsync(\"ws://localhost:9222\");")]
    [InlineData("(driver).Session.StatusAsync()", "await (driver).StartAsync(\"ws://localhost:9222\");")]
    [InlineData("((BiDiDriver)driver).Session.StatusAsync()", "await ((IBiDiDriverLifecycleManager)driver).StartAsync(\"ws://localhost:9222\");")]
    public async Task CommandThroughWrappedReceiver_CodeFixMovesAfterWrappedStartAsync(string command, string start)
    {
        string testCode = $$"""
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver? driver = new();
                        await {|#0:{{command}}|};
                        {{start}}
                    }
                }
            }
            """;

        string fixedCode = $$"""
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver? driver = new();
                        {{start}}
                        await {{command}};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the fix finds the driver and its StartAsync through null-conditional receivers.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandThroughNullConditionalReceiver_CodeFixMovesAfterNullConditionalStartAsync()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver? driver = new();
                        await (driver?{|#0:.Session.StatusAsync()|} ?? Task.FromResult<StatusCommandResult>(null!));
                        await (driver?.StartAsync("ws://localhost:9222") ?? Task.CompletedTask);
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver? driver = new();
                        await (driver?.StartAsync("ws://localhost:9222") ?? Task.CompletedTask);
                        await (driver?.Session.StatusAsync() ?? Task.FromResult<StatusCommandResult>(null!));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsync_ModuleStartAsyncBeforeDriverStartAsync_CodeFixMovesAfterDriverStart()
    {
        // A member reached through the driver may expose a StartAsync of its own, which is a different
        // lifecycle and does not start the driver. The fix must move the command after the driver's own
        // StartAsync, not after the first call that merely shares the name and roots in the driver.
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class Tracer
                {
                    public Task StartAsync(string name) => Task.CompletedTask;
                }

                public class TracingDriver : BiDiDriver
                {
                    public Tracer Tracing { get; } = new Tracer();
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        TracingDriver driver = new TracingDriver();
                        await driver.Tracing.StartAsync("trace");
                        await {|#0:driver.ExecuteCommandAsync(new StatusCommandParameters())|};
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class Tracer
                {
                    public Task StartAsync(string name) => Task.CompletedTask;
                }

                public class TracingDriver : BiDiDriver
                {
                    public Tracer Tracing { get; } = new Tracer();
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        TracingDriver driver = new TracingDriver();
                        await driver.Tracing.StartAsync("trace");
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ExecuteCommandAsync");

        RealAssemblyCodeFixTest<BiDiDriver009_CommandExecutionBeforeStartAnalyzer, BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
