// <copyright file="BiDiDriver003CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver003 code fix provider.
/// </summary>
public class BiDiDriver003CodeFixProviderTests
{
    /// <summary>
    /// Tests that no fix is offered in a top-level program: the fix rearranges statements of a
    /// method declaration, which does not exist in that context.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_InTopLevelProgram_NoFixOffered()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using WebDriverBiDi;

            IJsonTypeInfoResolver resolver = JsonTypeInfoResolver.Combine();
            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
            await driver.StartAsync("ws://localhost:9222");
            {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyCodeFixTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
            TestState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
            FixedState = { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication },
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a StartAsync whose receiver is not a simple identifier (a driver held in a field
    /// accessed through <c>this</c>) yields no variable name and is not treated as the StartAsync
    /// of the local driver; the fix moves the registration before the matching one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_StartAsyncOnFieldReceiverIgnored_CodeFixMovesBeforeMatchingStartAsync()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    private BiDiDriver other = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await this.other.StartAsync("ws://otherhost:9222");
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    private BiDiDriver other = new BiDiDriver(TimeSpan.FromSeconds(30));

                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await this.other.StartAsync("ws://otherhost:9222");
                        driver.RegisterTypeInfoResolverAsync(resolver);
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyCodeFixTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix moves RegisterTypeInfoResolverAsync before StartAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RegisterTypeInfoResolverAsync_CodeFixMovesBeforeStartAsync()
    {
        string testCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        await driver.StartAsync("ws://localhost:9222");
                        {|#0:driver.RegisterTypeInfoResolverAsync(resolver)|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Text.Json.Serialization.Metadata;
            using System.Threading.Tasks;
            using WebDriverBiDi;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(IJsonTypeInfoResolver resolver)
                    {
                        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                        driver.RegisterTypeInfoResolverAsync(resolver);
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyCodeFixTest<BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer, BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
