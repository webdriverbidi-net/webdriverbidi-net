// <copyright file="BiDiDriver010CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver010 code fix provider, which awaits a fire-and-forget module command.
/// </summary>
public class BiDiDriver010CodeFixProviderTests
{
    /// <summary>
    /// Tests that the fix awaits the discarded command in an async method.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetInAsyncMethod_IsAwaited()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        RealAssemblyCodeFixTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a command reached through a null-conditional receiver is awaited as a whole,
    /// rather than the `await` being spliced into the middle of the conditional access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetThroughConditionalAccess_IsAwaitedAsAWhole()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        driver?{|#0:.Session.StatusAsync()|};
                    }
                }
            }
            """;

        // Coalescing is what keeps the null receiver meaning "do nothing", as `?.` asked: awaiting the
        // conditional access alone would throw a NullReferenceException the discarded call never had.
        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await (driver?.Session.StatusAsync() ?? Task.CompletedTask);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        RealAssemblyCodeFixTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a chain of null-conditional accesses is awaited as a whole, so that the fix climbs
    /// past every conditional access the call is the right-hand side of, not just the innermost.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetThroughChainedConditionalAccess_IsAwaitedAsAWhole()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        driver?.Session?{|#0:.StatusAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await (driver?.Session?.StatusAsync() ?? Task.CompletedTask);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("StatusAsync");

        RealAssemblyCodeFixTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a <see cref="ValueTask"/>-returning operation reached through a null-conditional
    /// receiver coalesces to <c>default</c>, since its conditional access yields a nullable value type
    /// that <c>Task.CompletedTask</c> could not stand in for.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetValueTaskThroughConditionalAccess_CoalescesToDefault()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi.Protocol;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(Transport transport)
                    {
                        transport?{|#0:.DisposeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi.Protocol;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(Transport transport)
                    {
                        await (transport?.DisposeAsync() ?? default);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("DisposeAsync");

        RealAssemblyCodeFixTest<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no fix is offered in a synchronous method, where <c>await</c> would not compile.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetInSynchronousMethod_OffersNoCodeAction()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Empty(actions);
    }

    /// <summary>
    /// Tests that no fix is offered inside members that cannot be made async. A constructor and a
    /// property accessor carry executable code but take no async modifier, so awaiting inside one
    /// would replace the reported problem with a compiler error.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetInMemberThatCannotBeAsync_OffersNoCodeAction()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly BiDiDriver driver;

                    public TestClass(BiDiDriver driver)
                    {
                        this.driver = driver;
                        driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                    }

                    public int Count
                    {
                        get
                        {
                            this.driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                            return 0;
                        }
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Empty(actions);
    }

    /// <summary>
    /// Tests that the fix is offered inside an async lambda, and that the enclosing method's own
    /// modifier does not decide the question.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetInAsyncLambdaInSynchronousMethod_OffersTheAction()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        Func<Task> run = async () =>
                        {
                            driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        };
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        CodeAction action = Assert.Single(actions);
        Assert.Equal("Await the command", action.Title);
    }

    /// <summary>
    /// Tests that no fix is offered inside a synchronous lambda, even when the enclosing method is
    /// async: the nearest enclosing function decides.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetInSynchronousLambdaInAsyncMethod_OffersNoCodeAction()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        Action run = () =>
                        {
                            driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        };
                        await Task.CompletedTask;
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Empty(actions);
    }

    /// <summary>
    /// Tests that the fix is offered inside an async local function.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetInAsyncLocalFunction_OffersTheAction()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        async Task RunAsync()
                        {
                            driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        }
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        CodeAction action = Assert.Single(actions);
        Assert.Equal("Await the command", action.Title);
    }

    /// <summary>
    /// Tests that the fix is offered in a top-level program, where the compiler generates an
    /// asynchronous entry point.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FireAndForgetInTopLevelProgram_OffersTheAction()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
            driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer, BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        CodeAction action = Assert.Single(actions);
        Assert.Equal("Await the command", action.Title);
    }
}
