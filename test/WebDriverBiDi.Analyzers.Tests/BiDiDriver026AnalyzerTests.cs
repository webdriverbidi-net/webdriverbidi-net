// <copyright file="BiDiDriver026AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver026 analyzer, which flags an explicit ExecuteCommandAsync&lt;T&gt; type
/// argument that disagrees with the command's result type.
/// </summary>
public class BiDiDriver026AnalyzerTests
{
    [Fact]
    public async Task MismatchedTypeArgument_ReportsError()
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

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer.DiagnosticId,
            DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("WrongResult", "StatusCommandResult");

        RealAssemblyAnalyzerTest<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer> testState = new()
        {
            TestCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MatchingTypeArgument_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.ExecuteCommandAsync<StatusCommandResult>(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer>(testCode);
    }

    [Fact]
    public async Task BaseTypeArgument_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        // CommandResult is a base of StatusCommandResult, so 'result is CommandResult'
                        // succeeds at runtime; the call is safe.
                        await driver.ExecuteCommandAsync<CommandResult>(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer>(testCode);
    }

    [Fact]
    public async Task NoExplicitTypeArgument_NoDiagnostic()
    {
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.ExecuteCommandAsync(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer>(testCode);
    }

    [Fact]
    public async Task NonExecuteCommandInvocations_NoDiagnostic()
    {
        string testCode = """
            using System;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        NoOp();                       // invocation that is not a member access
                        var empty = Array.Empty<int>();  // generic member call, different name
                    }

                    private void NoOp()
                    {
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer>(testCode);
    }

    [Fact]
    public async Task UnresolvedExecuteCommandAsync_NoDiagnostic()
    {
        // A method named ExecuteCommandAsync that does not exist yields a null symbol; the analyzer
        // must not report or crash. Cannot be reproduced against the real API.
        string testCode = """
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class Foo
                {
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var foo = new Foo();
                        var task = foo.{|CS1061:ExecuteCommandAsync<StatusCommandResult>|}(new StatusCommandParameters());
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteCommandAsyncOnNonExecutorType_NoDiagnostic()
    {
        // A same-named ExecuteCommandAsync on a type that is not a command executor is not the
        // library's method, so a mismatched type argument is not reported.
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class FakeExecutor
                {
                    public Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters) => null!;
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var fake = new FakeExecutor();
                        var task = fake.ExecuteCommandAsync<StatusCommandResult>(new StatusCommandParameters());
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonGenericCommandParametersArgument_NoDiagnostic()
    {
        // A CommandParameters that is not CommandParameters<T> has no statically known result type, so
        // the analyzer cannot compare and does not report.
        string testCode = """
            using System;
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class PlainParameters : CommandParameters
                {
                    public override string MethodName => "plain";

                    public override Type ResponseType => typeof(object);
                }

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.ExecuteCommandAsync<StatusCommandResult>(new PlainParameters());
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer> testState = new()
        {
            TestCode = testCode,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task OpenTypeParameterAsTypeArgument_NoDiagnostic()
    {
        // The requested type is an open type parameter, so the outcome is not statically determinable.
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task Wrap<TResult>(BiDiDriver driver)
                        where TResult : CommandResult
                    {
                        await driver.ExecuteCommandAsync<TResult>(new StatusCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer>(testCode);
    }

    [Fact]
    public async Task OpenTypeParameterAsCommandResult_NoDiagnostic()
    {
        // The command's result type is an open type parameter, so the outcome is not statically
        // determinable.
        string testCode = """
            using System.Threading.Tasks;
            using WebDriverBiDi;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public async Task Wrap<TResult>(BiDiDriver driver, CommandParameters<TResult> parameters)
                        where TResult : CommandResult
                    {
                        await driver.ExecuteCommandAsync<StatusCommandResult>(parameters);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer>(testCode);
    }
}
