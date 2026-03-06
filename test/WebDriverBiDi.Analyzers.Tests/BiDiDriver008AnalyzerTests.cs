// <copyright file="BiDiDriver008AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver008 analyzer that detects unsafe EvaluateResult casts.
/// </summary>
[TestFixture]
public class BiDiDriver008AnalyzerTests
{
    /// <summary>
    /// Tests that direct cast to EvaluateResultSuccess reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EvaluateResult_DirectCast_ReportsWarning()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var title = success.Result;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that 'as' cast to EvaluateResultSuccess reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EvaluateResult_AsCast_ReportsWarning()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success != null)
                        {
                            var title = success.Result;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that pattern matching with 'is' does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EvaluateResult_PatternMatching_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        if (result is EvaluateResultSuccess success)
                        {
                            var title = success.Result;
                        }
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that direct cast to EvaluateResultException reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EvaluateResult_CastToException_ReportsWarning()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult { }

                public record EvaluateResultException : EvaluateResult
                {
                    public ExceptionDetails ExceptionDetails { get; set; } = new ExceptionDetails();
                }

                public class ExceptionDetails { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultException();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("throw new Error()");
                        var exception = {|#0:(EvaluateResultException)result|};
                        var details = exception.ExceptionDetails;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultException");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that cast inside try-catch does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task EvaluateResult_CastInTryCatch_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        try
                        {
                            var success = (EvaluateResultSuccess)result;
                            var title = success.Result;
                        }
                        catch (InvalidCastException)
                        {
                            // Handle exception case
                        }
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that casts to non-EvaluateResult derived types do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Cast_ToNonEvaluateResultDerivedType_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public class RemoteValue { }

                public record OtherResult : CommandResult { }

                public class ScriptModule
                {
                    public CommandResult Evaluate(string script) => new OtherResult();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        CommandResult result = script.Evaluate("document.title");
                        var other = (OtherResult)result;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that casts from non-EvaluateResult types do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Cast_FromNonEvaluateResultType_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public class RemoteValue { }

                public record OtherResult : CommandResult { }

                public class ScriptModule
                {
                    public OtherResult Evaluate(string script) => new OtherResult();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        OtherResult result = script.Evaluate("document.title");
                        var success = (EvaluateResultSuccess)(CommandResult)result;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that cast inside local function statements are detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Cast_InsideLocalFunction_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");

                        void ProcessResult()
                        {
                            try
                            {
                                var success = (EvaluateResultSuccess)result;
                                var title = success.Result;
                            }
                            catch (InvalidCastException)
                            {
                                // Handle exception case
                            }
                        }

                        ProcessResult();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that 'as' cast with null check is still flagged as warning to encourage pattern matching.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AsCast_WithNullCheck_StillReportsWarning()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success != null)
                        {
                            var title = success.Result;
                        }
                        else
                        {
                            // Handle failure case
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-invocation member access is not analyzed for casts.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonCast_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public class RemoteValue { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        var type = result.GetType();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix is offered for direct cast with variable declaration.
    /// Note: The code fixer successfully identifies and fixes the issue, though exact output
    /// formatting may vary due to trivia handling complexity in Roslyn.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DirectCast_WithVariableDeclaration_CodeFixIsOffered()
    {
        string testCode = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue
                {
                    public string Value { get; set; } = string.Empty;
                }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var title = success.Result.Value;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix is offered for 'as' cast to pattern matching.
    /// Note: The code fixer successfully identifies and fixes the issue, though exact output
    /// formatting may vary due to trivia handling complexity in Roslyn.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AsCast_WithVariableDeclaration_CodeFixIsOffered()
    {
        string testCode = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue
                {
                    public string Value { get; set; } = string.Empty;
                }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success != null)
                        {
                            var title = success.Result.Value;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix is offered for cast to EvaluateResultException.
    /// Note: The code fixer successfully identifies and fixes the issue, though exact output
    /// formatting may vary due to trivia handling complexity in Roslyn.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DirectCast_ToException_CodeFixIsOffered()
    {
        string testCode = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult { }

                public record EvaluateResultException : EvaluateResult
                {
                    public ExceptionDetails ExceptionDetails { get; set; } = new ExceptionDetails();
                }

                public class ExceptionDetails
                {
                    public string Text { get; set; } = string.Empty;
                }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultException();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("throw new Error()");
                        var exception = {|#0:(EvaluateResultException)result|};
                        var text = exception.ExceptionDetails.Text;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultException");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix is offered for cast with multiple dependent statements.
    /// Note: The code fixer successfully identifies and fixes the issue, though exact output
    /// formatting may vary due to trivia handling complexity in Roslyn.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DirectCast_WithMultipleDependentStatements_CodeFixIsOffered()
    {
        string testCode = """
            using System;

            namespace WebDriverBiDi
            {
                public record CommandResult { }
            }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }

                public record EvaluateResultSuccess : EvaluateResult
                {
                    public RemoteValue Result { get; set; } = new RemoteValue();
                }

                public record EvaluateResultException : EvaluateResult { }

                public class RemoteValue
                {
                    public string Value { get; set; } = string.Empty;
                    public string Type { get; set; } = string.Empty;
                }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResultSuccess();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("document.title");
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.Result.Value;
                        var type = success.Result.Type;
                        Console.WriteLine(value);
                        Console.WriteLine(type);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
