// <copyright file="BiDiDriver012AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver012 analyzer.
/// </summary>
public class BiDiDriver012AnalyzerTests
{
    [Test]
    public async Task DisposeAsync_WithoutStopAsync_ReportsInfo()
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
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(12, 19, 12, 40)
            .WithArguments("driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task DisposeAsync_WithStopAsync_NoDiagnostic()
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
                        await driver.StopAsync();
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }

    [Test]
    public async Task DisposeAsync_WithStopAsyncInTryFinally_NoDiagnostic()
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
                        try
                        {
                            await driver.StartAsync("ws://localhost:9222");
                        }
                        finally
                        {
                            await driver.StopAsync();
                            await driver.DisposeAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }

    [Test]
    public async Task DisposeAsync_WithStopAsyncAfter_ReportsInfo()
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
                        await driver.DisposeAsync();
                        await driver.StopAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(12, 19, 12, 40)
            .WithArguments("driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task MultipleDrivers_DisposeAsyncWithoutStopAsync_ReportsMultipleInfo()
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
                        await driver1.DisposeAsync();
                        await driver2.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected1 = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(14, 19, 14, 41)
            .WithArguments("driver1");

        DiagnosticResult expected2 = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(15, 19, 15, 41)
            .WithArguments("driver2");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected1, expected2);
    }

    [Test]
    public async Task MultipleDrivers_OneWithStopAsync_ReportsInfoForOtherDriver()
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
                        await driver2.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(16, 19, 16, 41)
            .WithArguments("driver2");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task NonBiDiDriverDisposeAsync_NoDiagnostic()
    {
        string testCode = """
            using System;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class CustomDisposable : IAsyncDisposable
                {
                    public ValueTask DisposeAsync()
                    {
                        return ValueTask.CompletedTask;
                    }
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomDisposable custom = new();
                        await custom.DisposeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }

    [Test]
    public async Task MethodWithoutBody_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public interface ITestInterface
                {
                    Task TestMethod();
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }

    [Test]
    public async Task DisposeAsync_InConditional_WithoutStopAsync_ReportsInfo()
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
                            await driver.DisposeAsync();
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(15, 23, 15, 44)
            .WithArguments("driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    [Test]
    public async Task DisposeAsync_WithStopAsyncInSameConditional_NoDiagnostic()
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
                            await driver.StopAsync();
                            await driver.DisposeAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }

    [Test]
    public async Task DisposeAsync_StopAsyncOnDifferentDriver_ReportsInfo()
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
                        await driver2.StopAsync();
                        await driver1.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(15, 19, 15, 41)
            .WithArguments("driver1");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }
}
