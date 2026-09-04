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
