// <copyright file="BiDiDriver026CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver026 code fix provider.
/// </summary>
public class BiDiDriver026CodeFixProviderTests
{
    [Fact]
    public async Task CodeFix_ChangesTypeArgumentToCommandResultType()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public record WrongResult : CommandResult
                {
                }

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.ExecuteCommandAsync<{|#0:WrongResult|}>(new StatusCommandParameters());
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public record WrongResult : CommandResult
                {
                }

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.ExecuteCommandAsync<StatusCommandResult>(new StatusCommandParameters());
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WrongResult", "StatusCommandResult");

        RealAssemblyCodeFixTest<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer, BiDiDriver026_ExecuteCommandResultTypeMismatchCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionEquivalenceKey = "ChangeExecuteCommandTypeArgument",
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeFix_RemovesExplicitTypeArgument()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public record WrongResult : CommandResult
                {
                }

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.ExecuteCommandAsync<{|#0:WrongResult|}>(new StatusCommandParameters());
                    }
                }
            }
            """;

        string fixedCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public record WrongResult : CommandResult
                {
                }

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WrongResult", "StatusCommandResult");

        RealAssemblyCodeFixTest<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer, BiDiDriver026_ExecuteCommandResultTypeMismatchCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionEquivalenceKey = "RemoveExecuteCommandTypeArgument",
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
