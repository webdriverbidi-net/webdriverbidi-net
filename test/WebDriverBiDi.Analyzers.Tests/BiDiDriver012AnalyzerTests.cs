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

    /// <summary>
    /// Tests that when a Collect behavior is assigned on the driver in the same method and
    /// DisposeAsync is called without StopAsync, the diagnostic escalates to a Warning with
    /// the Collect-specific message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_WithCollectBehaviorOnDriver_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(14, 19, 14, 40)
            .WithMessage("Call StopAsync on 'driver' before calling DisposeAsync; a TransportErrorBehavior is set to Collect, and DisposeAsync discards collected errors without throwing them");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a Collect behavior set through a Transport object initializer (the form used in
    /// the documentation samples) is detected — exercises the identifier-name branch of the
    /// assignment-target check.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_WithCollectBehaviorInTransportInitializer_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        Transport transport = new Transport(new WebSocketConnection())
                        {
                            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
                        };
                        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(18, 19, 18, 40)
            .WithMessage("Call StopAsync on 'driver' before calling DisposeAsync; a TransportErrorBehavior is set to Collect, and DisposeAsync discards collected errors without throwing them");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a Collect behavior assignment does not produce a diagnostic when StopAsync
    /// precedes DisposeAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_WithCollectBehaviorAndStopAsync_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        driver.UnknownMessageBehavior = TransportErrorBehavior.Collect;
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.StopAsync();
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that assigning a behavior other than Collect (Terminate) keeps the diagnostic at
    /// Info with the original message — exercises the field-name check failing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_WithTerminateBehavior_ReportsInfo()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        driver.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Info)
            .WithSpan(14, 19, 14, 40)
            .WithArguments("driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a field named Collect on an unrelated enum, assigned to a same-named property on
    /// an unrelated type, does not escalate — exercises the containing-type check failing — and that
    /// assignments whose target is neither a member access nor an identifier (element access) and
    /// assignments to unrelated properties are skipped.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_WithUnrelatedCollectFieldAndOtherAssignments_ReportsInfo()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public enum OtherBehavior
                {
                    Collect,
                }

                public class OtherSettings
                {
                    public OtherBehavior ProtocolErrorBehavior { get; set; }

                    public string Name { get; set; } = "";
                }

                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        int[] values = new int[1];
                        values[0] = 1;
                        OtherSettings settings = new OtherSettings();
                        settings.Name = "unrelated";
                        settings.ProtocolErrorBehavior = OtherBehavior.Collect;
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
            .WithSpan(29, 19, 29, 40)
            .WithArguments("driver");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that two DisposeAsync calls without StopAsync in a Collect-configured method both
    /// escalate, and that the Collect lookup is performed once and reused for the second call.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleDrivers_WithCollectBehavior_ReportsWarningForEach()
    {
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Protocol;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver first = new();
                        first.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
                        BiDiDriver second = new();
                        await first.DisposeAsync();
                        await second.DisposeAsync();
                    }
                }
            }
            """;

        DiagnosticResult expectedFirst = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(14, 19, 14, 39)
            .WithMessage("Call StopAsync on 'first' before calling DisposeAsync; a TransportErrorBehavior is set to Collect, and DisposeAsync discards collected errors without throwing them");

        DiagnosticResult expectedSecond = new DiagnosticResult(
            BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithSpan(15, 19, 15, 40)
            .WithMessage("Call StopAsync on 'second' before calling DisposeAsync; a TransportErrorBehavior is set to Collect, and DisposeAsync discards collected errors without throwing them");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer>(testCode, expectedFirst, expectedSecond);
    }
}
