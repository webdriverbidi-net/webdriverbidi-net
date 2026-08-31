// <copyright file="BiDiDriver024AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver024 analyzer, which flags a second StartAsync call on a driver with no
/// intervening StopAsync.
/// </summary>
public class BiDiDriver024AnalyzerTests
{
    [Fact]
    public async Task SecondStartAsync_WithoutStopAsync_ReportsError()
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
                        await {|#0:driver.StartAsync("ws://localhost:9222")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver024_DuplicateStartAsyncAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0);

        RealAssemblyAnalyzerTest<BiDiDriver024_DuplicateStartAsyncAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SingleStartAsync_ThenCommand_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi;
            using System.Threading.Tasks;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartStopStart_NoDiagnostic()
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
                        await driver.StartAsync("ws://localhost:9222");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task StartAsyncInsideNestedFunction_NotCountedAsDuplicate()
    {
        string testCode = """
            using WebDriverBiDi;
            using System;
            using System.Threading.Tasks;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        BiDiDriver driver = new();
                        await driver.StartAsync("ws://localhost:9222");

                        // A StartAsync inside a nested function runs when the delegate is invoked, not
                        // here, so it must not be treated as a second start of the driver.
                        Func<Task> reconnect = async () => await driver.StartAsync("ws://localhost:9222");
                        await reconnect();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }

    [Fact]
    public async Task NonDriverInvocationsAndDeclarations_NoDiagnostic()
    {
        string testCode = """
            using System;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        int counter = 0;          // non-driver declaration
                        int uninitialized;        // declaration without initializer
                        uninitialized = counter;
                        NoOp();                   // invocation that is not a member access
                        _ = counter.ToString();   // member access on an untracked identifier
                        Console.WriteLine(uninitialized);
                    }

                    private void NoOp()
                    {
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver024_DuplicateStartAsyncAnalyzer>(testCode);
    }
}
