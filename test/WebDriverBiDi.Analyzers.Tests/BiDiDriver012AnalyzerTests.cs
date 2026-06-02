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
    [Fact]
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
                        IBiDiCommandExecutor driver = new BiDiDriver();
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    /// <summary>
    /// Tests that DisposeAsync called through a method invocation chain (not a simple
    /// identifier) does not report a diagnostic — exercises GetDriverVariableName returning
    /// null (line 122) because the expression is not a MemberAccessExpressionSyntax with an
    /// identifier base.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_OnMethodCallResult_DoesNotReportDiagnostic()
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
                        await GetDriver().DisposeAsync();
                    }

                    private static BiDiDriver GetDriver() => new BiDiDriver();
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that DisposeAsync inside a deeply nested block (where GetContainingBlock
    /// would return null if the driver call is at the compilation-unit level) does not
    /// crash — exercises the null return from GetContainingBlock (line 158) by placing
    /// DisposeAsync in an expression-bodied member with no enclosing block.
    /// Also exercises GetStatements returning empty (line 227) for the expression-bodied path.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_InExpressionBodiedMethod_ReportsInfo()
    {
        // Expression-bodied method: HasStopAsyncBefore will find no containing BlockSyntax
        // or MethodDeclarationSyntax with a body → exercises GetStatements returning empty.
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    // Expression-bodied async method — method.Body is null
                    public Task TestMethod(BiDiDriver driver) =>
                        driver.DisposeAsync().AsTask();
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(10, 13, 10, 34)
            .WithArguments("driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that DisposeAsync nested inside an if-block is handled — exercises
    /// GetContainingBlock walking more than one parent level (line 145 while loop body)
    /// and HasStopAsyncBeforeInStatements iterating statements (line 182 foreach body).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_InsideIfBlock_WithStopAsync_DoesNotReport()
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
                        BiDiDriver driver = new BiDiDriver();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        if (true)
                        {
                            await driver.DisposeAsync();
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }
}
