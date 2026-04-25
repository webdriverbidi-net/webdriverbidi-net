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
[TestFixture]
public class BiDiDriver012CodeFixProviderTests
{
    [Test]
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

        CSharpCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
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

        CSharpCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
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

        CSharpCodeFixTest<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer, BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
