// <copyright file="BiDiDriver004CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver004 code fix provider. These compile against the real
/// <c>WebDriverBiDi.dll</c> so the fixed code is verified against the actual method signatures,
/// which place an optional <see cref="System.TimeSpan"/> parameter before the trailing
/// <see cref="System.Threading.CancellationToken"/>. A positionally appended token would bind to
/// that parameter and fail to compile (CS1503); the fix must therefore use a named argument.
/// </summary>
public class BiDiDriver004CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix adds CancellationToken.None as a named argument.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTreeAsync_CodeFixAddsCancellationTokenNone()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.GetTreeAsync(cancellationToken: CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix adds a cancellationToken named argument referencing an in-scope token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTreeAsync_CodeFixAddsCancellationTokenParameter()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken cancellationToken)
                    {
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken cancellationToken)
                    {
                        await driver.BrowsingContext.GetTreeAsync(cancellationToken: cancellationToken);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionEquivalenceKey = "AddCancellationTokenParameter",
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that <c>CancellationToken.None</c> is written fully qualified when the file has no using
    /// for <c>System.Threading</c>. Emitting the short name there produced code that does not compile —
    /// and that is the common case, because the diagnostic fires precisely on calls that pass no token
    /// and so had no reason to import the namespace.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFixQualifiesCancellationTokenWhenNamespaceNotImported()
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
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
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
                        await driver.BrowsingContext.GetTreeAsync(cancellationToken: System.Threading.CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionIndex = 0,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the fix uses the name of the token actually in scope rather than assuming
    /// <c>cancellationToken</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFixUsesTheInScopeTokenName()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken stoppingToken)
                    {
                        await {|#0:driver.BrowsingContext.GetTreeAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken stoppingToken)
                    {
                        await driver.BrowsingContext.GetTreeAsync(cancellationToken: stoppingToken);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("GetTreeAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionIndex = 1,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that only the <c>CancellationToken.None</c> action is offered when no token is in scope.
    /// The second action inserted a bare <c>cancellationToken</c> identifier unconditionally, which
    /// produced code that does not compile wherever no such symbol existed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OffersOnlyTheNoneActionWhenNoTokenIsInScope()
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
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        CodeAction action = Assert.Single(actions);
        Assert.Equal("Add CancellationToken.None parameter", action.Title);
    }

    /// <summary>
    /// Tests that both actions are offered when a token is in scope.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OffersBothActionsWhenATokenIsInScope()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken cancellationToken)
                    {
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Equal(2, actions.Count);
        Assert.Equal("Add cancellationToken parameter", actions[1].Title);
    }


    /// <summary>
    /// Tests that a token held in a local variable is found and offered by name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OffersTokenHeldInLocal()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        CancellationToken localToken = CancellationToken.None;
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Equal(2, actions.Count);
        Assert.Equal("Add localToken parameter", actions[1].Title);
    }

    /// <summary>
    /// Tests that a token held in a field is found and offered by name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OffersTokenHeldInField()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    private readonly CancellationToken fieldToken = CancellationToken.None;

                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Equal(2, actions.Count);
        Assert.Equal("Add fieldToken parameter", actions[1].Title);
    }

    /// <summary>
    /// Tests that a token held in a property is found and offered by name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OffersTokenHeldInProperty()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    private CancellationToken PropertyToken { get; } = CancellationToken.None;

                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Equal(2, actions.Count);
        Assert.Equal("Add PropertyToken parameter", actions[1].Title);
    }

    /// <summary>
    /// Tests that the first token found is offered when several are in scope and none is named
    /// <c>cancellationToken</c>, rather than the search continuing past a match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OffersOneTokenWhenSeveralAreInScope()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, CancellationToken firstToken, CancellationToken secondToken)
                    {
                        await driver.BrowsingContext.GetTreeAsync();
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Equal(2, actions.Count);
        Assert.Contains(actions[1].Title, new[] { "Add firstToken parameter", "Add secondToken parameter" });
    }


    /// <summary>
    /// Tests that the same provider fixes BIDI013, which reports the identical shape — a call with a
    /// cancellation-token overload that passes no token — at Warning severity. Registering only
    /// BIDI004 left the more severe rule with no fix at all.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task BIDI013_IsFixedByTheSameProvider()
    {
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        NavigateCommandParameters navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await {|#0:driver.BrowsingContext.NavigateAsync(navParams)|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        NavigateCommandParameters navParams = new NavigateCommandParameters(contextId, "https://example.com");
                        await driver.BrowsingContext.NavigateAsync(navParams, cancellationToken: CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        RealAssemblyCodeFixTest<BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionIndex = 0,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CustomModuleWithADifferentlyNamedTokenParameter_NamesTheArgumentAfterIt()
    {
        // The rule reports calls on user-written Module subclasses, whose token parameter can be named
        // anything. Hard-coding "cancellationToken:" would produce CS1739 here.
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestNamespace
            {
                public class CustomModule : Module
                {
                    public CustomModule(BiDiDriver driver)
                        : base(driver)
                    {
                    }

                    public override string ModuleName => "custom";

                    public Task EvaluateAsync(CancellationToken token = default) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(CustomModule module)
                    {
                        await {|#0:module.EvaluateAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestNamespace
            {
                public class CustomModule : Module
                {
                    public CustomModule(BiDiDriver driver)
                        : base(driver)
                    {
                    }

                    public override string ModuleName => "custom";

                    public Task EvaluateAsync(CancellationToken token = default) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(CustomModule module)
                    {
                        await module.EvaluateAsync(token: CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EvaluateAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionIndex = 0,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CustomModuleWhoseTokenIsOnASiblingOverload_NamesTheArgumentAfterIt()
    {
        // The call binds to the overload without a token, so the name comes from the sibling that has
        // one, which is the overload the argument makes the call bind to.
        string testCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestNamespace
            {
                public class CustomModule : Module
                {
                    public CustomModule(BiDiDriver driver)
                        : base(driver)
                    {
                    }

                    public override string ModuleName => "custom";

                    public Task EvaluateAsync() => Task.CompletedTask;

                    public Task EvaluateAsync(CancellationToken abortToken) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(CustomModule module)
                    {
                        await {|#0:module.EvaluateAsync()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;

            namespace TestNamespace
            {
                public class CustomModule : Module
                {
                    public CustomModule(BiDiDriver driver)
                        : base(driver)
                    {
                    }

                    public override string ModuleName => "custom";

                    public Task EvaluateAsync() => Task.CompletedTask;

                    public Task EvaluateAsync(CancellationToken abortToken) => Task.CompletedTask;
                }

                public class TestClass
                {
                    public async Task TestMethod(CustomModule module)
                    {
                        await module.EvaluateAsync(abortToken: CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("EvaluateAsync");

        RealAssemblyCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionIndex = 0,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
