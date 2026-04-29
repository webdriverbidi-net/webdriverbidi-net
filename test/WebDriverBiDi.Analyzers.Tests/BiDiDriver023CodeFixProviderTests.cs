// <copyright file="BiDiDriver023CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver023 code fix provider.
/// </summary>
[TestFixture]
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
    [Test]
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

        CSharpCodeFixTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix replaces an existing options argument with RunHandlerAsynchronously.
    /// </summary>
    [Test]
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

        CSharpCodeFixTest<BiDiDriver023_ModuleCommandInEventHandlerAnalyzer, BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests FixableDiagnosticIds contains BIDI023.
    /// </summary>
    [Test]
    public void FixableDiagnosticIds_ContainsBIDI023()
    {
        BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider provider = new();
        System.Collections.Immutable.ImmutableArray<string> ids = provider.FixableDiagnosticIds;

        Assert.That(ids, Has.Length.EqualTo(1));
        Assert.That(ids[0], Is.EqualTo(BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId));
    }

    /// <summary>
    /// Tests GetFixAllProvider returns the batch fixer.
    /// </summary>
    [Test]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider provider = new();
        Microsoft.CodeAnalysis.CodeFixes.FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.That(fixAllProvider, Is.Not.Null);
        Assert.That(fixAllProvider, Is.EqualTo(Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders.BatchFixer));
    }
}
