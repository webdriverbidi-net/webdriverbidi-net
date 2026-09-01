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

}
