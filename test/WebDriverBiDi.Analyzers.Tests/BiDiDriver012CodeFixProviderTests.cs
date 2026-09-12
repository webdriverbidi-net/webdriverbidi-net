// <copyright file="BiDiDriver012CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver012 code fix provider.
/// </summary>
public class BiDiDriver012CodeFixProviderTests
{
    [Fact]
    public async Task DisposeAsync_CodeFixInsertsStopAsync()
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
                        await {|#0:driver.DisposeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
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
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix also applies to the Warning-severity Collect-mode variant of the
    /// diagnostic, which shares the diagnostic ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_WithCollectBehavior_CodeFixInsertsStopAsync()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
                        await driver.StartAsync("ws://localhost:9222");
                        await {|#0:driver.DisposeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithMessage("Call StopAsync on 'driver' before calling DisposeAsync; a TransportErrorBehavior is set to Collect, and DisposeAsync discards collected errors without throwing them");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DisposeAsync_InConditional_CodeFixInsertsStopAsync()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldDispose)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        if (shouldDispose)
                        {
                            await {|#0:driver.DisposeAsync()|};
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldDispose)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        if (shouldDispose)
                        {
                            await driver.StopAsync();
                            await driver.DisposeAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DisposeAsync_InSwitchSection_CodeFixInsertsStopAsyncInSection()
    {
        // The statements of a switch section form a statement list of their own, without a block, so
        // the stop is inserted directly before the disposal rather than wrapped in braces.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        switch (mode)
                        {
                            case 1:
                                await {|#0:driver.DisposeAsync()|};
                                break;
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(int mode)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        switch (mode)
                        {
                            case 1:
                                await driver.StopAsync();
                                await driver.DisposeAsync();
                                break;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DisposeAsync_InEmbeddedConditionalStatement_CodeFixWrapsInBlock()
    {
        // The disposal is the unbraced embedded statement of an if, so it belongs to no statement
        // list and nothing can be inserted before it. Braces are added around both statements, which
        // keeps the stop inside the branch instead of hoisting it out where it would run
        // unconditionally.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldDispose)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        if (shouldDispose)
                            await {|#0:driver.DisposeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod(bool shouldDispose)
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        if (shouldDispose)
                        {
                            await driver.StopAsync();
                            await driver.DisposeAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MultipleDrivers_CodeFixInsertsStopAsyncForFlaggedDriver()
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
                        BiDiDriver driver1 = new();
                        BiDiDriver driver2 = new();
                        await driver1.StartAsync("ws://localhost:9222");
                        await driver2.StartAsync("ws://localhost:9223");
                        await driver1.StopAsync();
                        await driver1.DisposeAsync();
                        await {|#0:driver2.DisposeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver1 = new();
                        BiDiDriver driver2 = new();
                        await driver1.StartAsync("ws://localhost:9222");
                        await driver2.StartAsync("ws://localhost:9223");
                        await driver1.StopAsync();
                        await driver1.DisposeAsync();
                        await driver2.StopAsync();
                        await driver2.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver2");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingDeclaration_CodeFixAppendsStopAsyncToBlock()
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
                        await using BiDiDriver {|#0:driver|} = new();
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        await using BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingDeclaration_WhenTheFinalReturnObservesTheDriver_OffersNoFix()
    {
        // StopAsync has to go before the return, or it never runs; in front of this return it would
        // change the answer from true to false. There is no correct position, so no fix is offered and
        // the diagnostic stands for the author to resolve.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<bool> TestMethod()
                    {
                        await using BiDiDriver {|#0:driver|} = new();
                        await driver.StartAsync("ws://localhost:9222");
                        return driver.IsStarted;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingDeclaration_WhenTheFinalReturnIgnoresTheDriver_InsertsStopAsyncBeforeIt()
    {
        // The return does not observe the driver, so the inserted statement both runs and leaves the
        // returned value alone.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<int> TestMethod()
                    {
                        await using BiDiDriver {|#0:driver|} = new();
                        await driver.StartAsync("ws://localhost:9222");
                        return 42;
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<int> TestMethod()
                    {
                        await using BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        return 42;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingStatement_WithBlockEndingInReturn_InsertsStopAsyncBeforeTheReturn()
    {
        // Appending after the return would leave the inserted statement unreachable, so it never ran.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<int> TestMethod()
                    {
                        BiDiDriver driver = new();
                        await using ({|#0:driver|})
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            return 42;
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<int> TestMethod()
                    {
                        BiDiDriver driver = new();
                        await using (driver)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            await driver.StopAsync();
                            return 42;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingStatement_WithSingleReturnStatementBody_InsertsStopAsyncBeforeIt()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<int> TestMethod()
                    {
                        BiDiDriver driver = new();
                        await using ({|#0:driver|})
                            return 42;
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<int> TestMethod()
                    {
                        BiDiDriver driver = new();
                        await using (driver)
                        {
                            await driver.StopAsync();
                            return 42;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingStatement_WhenTheClosingThrowObservesTheDriver_OffersNoFix()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await using ({|#0:driver|})
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            throw new InvalidOperationException(driver.IsStarted.ToString());
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingStatement_WithBlock_CodeFixAppendsStopAsyncToBlock()
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
                        await using (BiDiDriver {|#0:driver|} = new())
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

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        await using (BiDiDriver driver = new())
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingStatement_WithEmbeddedStatement_CodeFixWrapsInBlock()
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
                        await using ({|#0:driver|})
                            await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await using (driver)
                        {
                            await driver.StartAsync("ws://localhost:9222");
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the inserted StopAsync uses the receiver as written, so a field disposed through <c>this.</c> is stopped through <c>this.</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_OnThisField_CodeFixInsertsStopAsyncOnThisField()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private readonly BiDiDriver driver = new();

                    public async Task TestMethod()
                    {
                        await this.driver.StartAsync("ws://localhost:9222");
                        await {|#0:this.driver.DisposeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private readonly BiDiDriver driver = new();

                    public async Task TestMethod()
                    {
                        await this.driver.StartAsync("ws://localhost:9222");
                        await this.driver.StopAsync();
                        await this.driver.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no fix is offered inside a synchronous member, where the inserted <c>await</c> would not compile (CS4033).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_InSynchronousMethod_NoFixOffered()
    {
        string testCode = """
            using WebDriverBiDi;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private readonly BiDiDriver driver = new();

                    public void Dispose()
                    {
                        {|#0:this.driver.DisposeAsync()|}.AsTask().GetAwaiter().GetResult();
                    }
                }
            }
            """;

        
        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the fix inserts a global statement when the DisposeAsync is a top-level statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_InTopLevelProgram_CodeFixInsertsStopAsync()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            BiDiDriver driver = new();
            await driver.StartAsync("ws://localhost:9222");
            await {|#0:driver.DisposeAsync()|};
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            BiDiDriver driver = new();
            await driver.StartAsync("ws://localhost:9222");
            await driver.StopAsync();
            await driver.DisposeAsync();
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            TestState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
            FixedState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that for an <c>await using var</c> declaration in a top-level program the fix appends StopAsync after the last global statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AwaitUsingDeclaration_InTopLevelProgram_CodeFixAppendsStopAsync()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            await using BiDiDriver {|#0:driver|} = new();
            await driver.StartAsync("ws://localhost:9222");
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            await using BiDiDriver driver = new();
            await driver.StartAsync("ws://localhost:9222");
            await driver.StopAsync();
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            TestState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
            FixedState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that for an <c>await using var</c> declaration in a top-level program ending in a return the fix inserts StopAsync before the return, where it is reachable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AwaitUsingDeclaration_InTopLevelProgramEndingWithReturn_CodeFixInsertsBeforeReturn()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            await using BiDiDriver {|#0:driver|} = new();
            await driver.StartAsync("ws://localhost:9222");
            return 0;
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            await using BiDiDriver driver = new();
            await driver.StartAsync("ws://localhost:9222");
            await driver.StopAsync();
            return 0;
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            TestState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
            FixedState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that for <c>await using (this.driver) { ... }</c> the fix appends StopAsync on <c>this.driver</c> to the body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AwaitUsingStatement_WithThisFieldExpression_CodeFixAppendsStopAsyncOnThisField()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private readonly BiDiDriver driver = new();

                    public async Task TestMethod()
                    {
                        await using ({|#0:this.driver|})
                        {
                            await this.driver.StartAsync("ws://localhost:9222");
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private readonly BiDiDriver driver = new();

                    public async Task TestMethod()
                    {
                        await using (this.driver)
                        {
                            await this.driver.StartAsync("ws://localhost:9222");
                            await this.driver.StopAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an <c>await using var</c> declaration inside the block of an unrelated using statement is not mistaken for that statement's declaration: StopAsync is appended to the declaration's own block.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AwaitUsingDeclaration_NestedInsideAnotherUsingBlock_CodeFixAppendsToInnerBlock()
    {
        string testCode = """
            using System.IO;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        using (MemoryStream stream = new())
                        {
                            await using BiDiDriver {|#0:driver|} = new();
                            await driver.StartAsync("ws://localhost:9222");
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using System.IO;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        using (MemoryStream stream = new())
                        {
                            await using BiDiDriver driver = new();
                            await driver.StartAsync("ws://localhost:9222");
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that for an <c>await using var</c> declaration in a block ending with a throw the fix
    /// inserts StopAsync before the throw, where it is reachable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AwaitUsingDeclaration_InBlockEndingWithThrow_CodeFixInsertsBeforeThrow()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        await using BiDiDriver {|#0:driver|} = new();
                        await driver.StartAsync("ws://localhost:9222");
                        throw new InvalidOperationException();
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        await using BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        throw new InvalidOperationException();
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AwaitUsingStatement_WithEmptyBlock_AppendsStopAsync()
    {
        // An empty body has no closing statement to insert in front of, so the stop is appended.
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
                        await using ({|#0:driver|})
                        {
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await using (driver)
                        {
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no fix is offered when the scope's last statement leaves no reachable end point.
    /// Appending StopAsync after an if/else that returns from both arms would be unreachable code that
    /// never runs, while the analyzer's textual check would see it and stop reporting.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AwaitUsingDeclaration_WhenEveryPathAlreadyReturned_OffersNoFix()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task<bool> TestMethod(bool flag)
                    {
                        await using BiDiDriver {|#0:driver|} = new();
                        await driver.StartAsync("ws://localhost:9222");
                        if (flag)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an await using statement with an empty block is handled: the scope has no last
    /// statement, so its end point is reachable and the fix appends StopAsync to the empty block.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AwaitUsingStatement_WithEmptyBlock_CodeFixAppendsStopAsync()
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
                        driver.TransportConfiguration.ProtocolErrorBehavior = WebDriverBiDi.Protocol.TransportErrorBehavior.Collect;
                        await driver.StartAsync("ws://localhost:9222");
                        await using ({|#0:driver|})
                        {
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        driver.TransportConfiguration.ProtocolErrorBehavior = WebDriverBiDi.Protocol.TransportErrorBehavior.Collect;
                        await driver.StartAsync("ws://localhost:9222");
                        await using (driver)
                        {
                            await driver.StopAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("driver");

        RealAssemblyCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
