// <copyright file="BiDiDriver004CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver004 code fix provider.
/// </summary>
public class BiDiDriver004CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix adds CancellationToken.None.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LocateNodesAsync_CodeFixAddsCancellationTokenNone()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class LocateNodesCommandParameters
                {
                    public LocateNodesCommandParameters(string contextId, string url) { }
                }

                public class LocateNodesCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters) => Task.FromResult(new LocateNodesCommandResult());
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new LocateNodesCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        var locateParams = new LocateNodesCommandParameters(contextId, "https://example.com");
                        await {|#0:driver.BrowsingContext.LocateNodesAsync(locateParams)|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class LocateNodesCommandParameters
                {
                    public LocateNodesCommandParameters(string contextId, string url) { }
                }

                public class LocateNodesCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters) => Task.FromResult(new LocateNodesCommandResult());
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new LocateNodesCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId)
                    {
                        var locateParams = new LocateNodesCommandParameters(contextId, "https://example.com");
                        await driver.BrowsingContext.LocateNodesAsync(locateParams, CancellationToken.None);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("LocateNodesAsync");

        LfCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix adds cancellationToken parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LocateNodesAsync_CodeFixAddsCancellationTokenParameter()
    {
        string testCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class LocateNodesCommandParameters
                {
                    public LocateNodesCommandParameters(string contextId, string url) { }
                }

                public class LocateNodesCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters) => Task.FromResult(new LocateNodesCommandResult());
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new LocateNodesCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId, CancellationToken cancellationToken)
                    {
                        var locateParams = new LocateNodesCommandParameters(contextId, "https://example.com");
                        await {|#0:driver.BrowsingContext.LocateNodesAsync(locateParams)|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public class BiDiDriver
                {
                    public BrowsingContext.BrowsingContextModule BrowsingContext { get; }
                }
            }

            namespace WebDriverBiDi.BrowsingContext
            {
                public class LocateNodesCommandParameters
                {
                    public LocateNodesCommandParameters(string contextId, string url) { }
                }

                public class LocateNodesCommandResult { }

                public class BrowsingContextModule
                {
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters) => Task.FromResult(new LocateNodesCommandResult());
                    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters parameters, CancellationToken cancellationToken) => Task.FromResult(new LocateNodesCommandResult());
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver, string contextId, CancellationToken cancellationToken)
                    {
                        var locateParams = new LocateNodesCommandParameters(contextId, "https://example.com");
                        await driver.BrowsingContext.LocateNodesAsync(locateParams, cancellationToken);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithLocation(0)
            .WithArguments("LocateNodesAsync");

        LfCodeFixTest<BiDiDriver004_CancellationTokenSuggestionAnalyzer, BiDiDriver004_CancellationTokenSuggestionCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CodeActionEquivalenceKey = "AddCancellationTokenParameter",
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
