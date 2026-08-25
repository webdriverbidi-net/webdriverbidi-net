// <copyright file="BiDiDriver023CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver023 code fix provider.
/// </summary>
public class BiDiDriver023CodeFixProviderTests
{
    private const string CommonStubs = """
        namespace WebDriverBiDi
        {
            using System;
            using System.Threading.Tasks;

            public class WebDriverBiDiEventArgs { }
            public class LogEntryAddedEventArgs : WebDriverBiDiEventArgs { }

            public enum ObservableEventHandlerOptions { None = 0, RunHandlerAsynchronously = 1 }

            public class EventObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
            {
                public void Dispose() { }
            }

            public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
            {
                public EventObserver<T> AddObserver(Func<T, Task> handler) => new EventObserver<T>();
                public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions options) => new EventObserver<T>();
            }

            public abstract class Module
            {
                protected Module(IBiDiCommandExecutor executor) { }
                public abstract string ModuleName { get; }
            }

            public interface IBiDiCommandExecutor { }

            public class NavigateCommandResult { }
            public class NavigateCommandParameters
            {
                public NavigateCommandParameters(string contextId, string url) { }
            }

            public class BrowsingContextModule : Module
            {
                public BrowsingContextModule(IBiDiCommandExecutor executor) : base(executor) { }
                public override string ModuleName => "browsingContext";
                public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters parameters) => Task.FromResult(new NavigateCommandResult());
            }

            public class LogModule
            {
                public ObservableEvent<LogEntryAddedEventArgs> OnEntryAdded { get; } = new ObservableEvent<LogEntryAddedEventArgs>();
            }

            public class BiDiDriver : IBiDiCommandExecutor
            {
                public BrowsingContextModule BrowsingContext { get; } = new BrowsingContextModule(null!);
                public LogModule Log { get; } = new LogModule();
            }
        }
        """;

    /// <summary>
    /// Tests that the code fix adds RunHandlerAsynchronously when it is not yet present.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithModuleCommand_CodeFixAddsRunHandlerAsynchronously()
    {
        string testCode = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                        });
                    }
                }
            }
            """;

        string fixedCode = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        LfCodeFixTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix replaces an existing options argument with RunHandlerAsynchronously.
    /// </summary>
    [Fact]
    public async Task EventHandler_WithExistingNoneOption_CodeFixReplacesWithRunHandlerAsynchronously()
    {
        string testCode = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|};
                        }, ObservableEventHandlerOptions.None);
                    }
                }
            }
            """;

        string fixedCode = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(async args =>
                        {
                            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("NavigateAsync");

        LfCodeFixTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds contains BIDI023.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI023()
    {
        BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider provider = new();
        System.Collections.Immutable.ImmutableArray<string> ids = provider.FixableDiagnosticIds;

        Assert.Single(ids);
        Assert.Equal(BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId, ids[0]);
    }

    /// <summary>
    /// Tests GetFixAllProvider returns the batch fixer.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider provider = new();
        Microsoft.CodeAnalysis.CodeFixes.FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.NotNull(fixAllProvider);
        Assert.Equal(Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests that when RunHandlerAsynchronously is already present on a non-async, expression-bodied
    /// Task-returning lambda, the code fix converts it to an async block lambda that awaits
    /// Task.Yield before issuing the command, and keeps the option.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_NonAsyncExpressionLambda_WithRunHandlerAsynchronously_CodeFixMakesHandlerAsync()
    {
        string testCode = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(
                            args => {|#0:driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"))|},
                            ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        string fixedCode = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod(BiDiDriver driver)
                    {
                        var observer = driver.Log.OnEntryAdded.AddObserver(
                            async args =>
                            {
                                await Task.Yield();
                                await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                            },
                            ObservableEventHandlerOptions.RunHandlerAsynchronously);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithMessage("Module command 'NavigateAsync' is called inside an event handler. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' does not offload the synchronous body of a Task-returning handler; make the handler 'async' so the command is issued from a continuation rather than on the dispatching thread.");

        LfCodeFixTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no code fix is offered (and no exception is thrown) when the module command is
    /// reported inside a method passed as a method group, because the diagnostic is not enclosed by
    /// the AddObserver invocation; the provider is invoked directly because the diagnostic is non-local.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EventHandler_MethodGroup_OffersNoCodeFix()
    {
        string source = $$"""
            {{CommonStubs}}

            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;

                public class TestClass
                {
                    private readonly BiDiDriver driver = new BiDiDriver();

                    public void TestMethod()
                    {
                        var observer = this.driver.Log.OnEntryAdded.AddObserver(this.HandleAsync);
                    }

                    private async Task HandleAsync(LogEntryAddedEventArgs args)
                    {
                        await this.driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("ctx", "https://example.com"));
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, _) = await AnalyzerTestHelpers
            .GetCodeActionsAsync<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider>(source);

        Assert.Empty(actions);
    }
}
