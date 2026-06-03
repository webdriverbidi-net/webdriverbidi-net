// <copyright file="BiDiDriver008AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver008 analyzer that detects unsafe EvaluateResult casts.
/// </summary>
public class BiDiDriver008AnalyzerTests
{
    /// <summary>
    /// Tests that direct cast to EvaluateResultSuccess reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that 'as' cast to EvaluateResultSuccess reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that pattern matching with 'is' does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that direct cast to EvaluateResultException reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that cast inside try-catch does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that casts to non-EvaluateResult derived types do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that casts from non-EvaluateResult types do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that cast inside local function statements are detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that 'as' cast with null check is still flagged as warning to encourage pattern matching.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-invocation member access is not analyzed for casts.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that direct cast with variable declaration triggers the analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DirectCast_WithVariableDeclaration_ReportsWarning()
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that 'as' cast with variable declaration triggers the analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsCast_WithVariableDeclaration_ReportsWarning()
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that cast to EvaluateResultException triggers the analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DirectCast_ToException_ReportsWarning()
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that cast with multiple dependent statements triggers the analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DirectCast_WithMultipleDependentStatements_ReportsWarning()
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests SupportedDiagnostics property.
    /// </summary>
    [Fact]
    public void SupportedDiagnostics_ContainsBIDI008()
    {
        BiDiDriver008_UnsafeEvaluateResultCastAnalyzer analyzer = new();
        System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.DiagnosticDescriptor> diagnostics = analyzer.SupportedDiagnostics;

        Assert.Single(diagnostics);
        Assert.Equal(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, diagnostics[0].Id);
    }

    /// <summary>
    /// Tests that cast with unresolved target type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DirectCast_UnresolvedTargetType_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResult();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("test");
                        var success = ({|CS0246:UnknownType|})result;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that cast from unresolved expression type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DirectCast_UnresolvedExpressionType_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult { }
                public record EvaluateResultSuccess : EvaluateResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var success = (EvaluateResultSuccess){|CS0103:unknownVariable|};
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that 'as' cast with unresolved target type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsCast_UnresolvedTargetType_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult { }

                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string script) => new EvaluateResult();
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("test");
                        var success = result as {|CS0246:UnknownType|};
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that 'as' cast from unresolved expression type is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsCast_UnresolvedExpressionType_NoDiagnostic()
    {
        string test = """
            using System;

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult { }
                public record EvaluateResultSuccess : EvaluateResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var success = {|CS0103:unknownVariable|} as EvaluateResultSuccess;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests GetFixAllProvider property.
    /// </summary>
    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider provider = new();
        Microsoft.CodeAnalysis.CodeFixes.FixAllProvider fixAllProvider = provider.GetFixAllProvider();

        Assert.NotNull(fixAllProvider);
        Assert.Equal(Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders.BatchFixer, fixAllProvider);
    }

    /// <summary>
    /// Tests FixableDiagnosticIds property.
    /// </summary>
    [Fact]
    public void FixableDiagnosticIds_ContainsBIDI008()
    {
        BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider provider = new();
        System.Collections.Immutable.ImmutableArray<string> ids = provider.FixableDiagnosticIds;

        Assert.Single(ids);
        Assert.Equal(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, ids[0]);
    }

    /// <summary>
    /// Tests that a cast to an unresolvable type does not report a diagnostic —
    /// exercises targetType == null guard in AnalyzeCastExpression (line 62).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastExpression_WithUnresolvableTargetType_DoesNotReportDiagnostic()
    {
        string test = """
            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(object value)
                    {
                        var x = ({|CS0246:NonExistentType|})value;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an 'as' expression targeting an unresolvable type does not report a
    /// diagnostic — exercises targetType == null guard in AnalyzeAsExpression (line 74).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsExpression_WithUnresolvableTargetType_DoesNotReportDiagnostic()
    {
        string test = """
            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(object value)
                    {
                        var x = value as {|CS0246:NonExistentType|};
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast where the source expression type is unresolvable does not report a
    /// diagnostic — exercises expressionType == null guard in AnalyzeCastExpression (line 98).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastExpression_WithUnresolvableSourceType_DoesNotReportDiagnostic()
    {
        string test = """
            namespace WebDriverBiDi
            {
                public abstract record EvaluateResult { }
                public record EvaluateResultSuccess : EvaluateResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var x = (EvaluateResultSuccess)({|CS0103:unknownVariable|});
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an 'as' expression where the left-hand side is unresolvable does not report
    /// a diagnostic — exercises expressionType == null guard in AnalyzeAsExpression (line 110).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsExpression_WithUnresolvableSourceType_DoesNotReportDiagnostic()
    {
        string test = """
            namespace WebDriverBiDi
            {
                public abstract record EvaluateResult { }
                public record EvaluateResultSuccess : EvaluateResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var x = ({|CS0103:unknownVariable|}) as EvaluateResultSuccess;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that casting to a type that is not EvaluateResultSuccess/Exception does not
    /// fire — exercises the IsEvaluateResultDerivedType false branch (line 61 `||` and
    /// line 114 false arm).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastToUnrelatedType_DoesNotReportDiagnostic()
    {
        string test = """
            namespace WebDriverBiDi.Script
            {
                public abstract record EvaluateResult { }
                public record EvaluateResultSuccess : EvaluateResult { }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(object value)
                    {
                        // Cast to a type that is NOT EvaluateResultSuccess/Exception.
                        var x = (string)value;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an EvaluateResult type in a different namespace does not fire —
    /// exercises the ContainingNamespace check in IsEvaluateResultBaseType (line 105).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastFromEvaluateResultInWrongNamespace_DoesNotReportDiagnostic()
    {
        // EvaluateResult in a non-WebDriverBiDi.Script namespace — namespace check fails.
        string test = """
            namespace NotScript
            {
                public abstract record EvaluateResult { }
                public record EvaluateResultSuccess : EvaluateResult { }
            }

            namespace TestApp
            {
                using NotScript;

                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = (EvaluateResultSuccess)result;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that casting a non-EvaluateResult source to EvaluateResultSuccess does not
    /// fire — exercises the IsEvaluateResultBaseType false branch (line 109 &&amp; short-circuit).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastFromNonEvaluateResultType_DoesNotReportDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(object value)
                    {
                        // Source is 'object', not EvaluateResult — IsEvaluateResultBaseType returns false.
                        var success = (EvaluateResultSuccess)value;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an 'as' expression to an unrelated type does not fire — exercises
    /// the IsEvaluateResultDerivedType false branch (line 89 `||` and line 114 false).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsExpressionToUnrelatedType_DoesNotReportDiagnostic()
    {
        string test = """
            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(object value)
                    {
                        var x = value as string;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast inside an expression-bodied local function stops at the
    /// LocalFunctionStatementSyntax boundary — exercises block=33 branch=1 (line 130),
    /// where is-MethodDeclarationSyntax is false and is-LocalFunctionStatementSyntax is true.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastInsideExpressionBodiedLocalFunction_ReportsDiagnostic()
    {
        // Expression-bodied local function has no BlockSyntax, so the walk reaches
        // LocalFunctionStatementSyntax before finding a safe context.
        string test = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        // Expression-bodied: cast.Parent is ArrowExpressionClause → LocalFunctionStatement.
                        EvaluateResultSuccess? Cast(EvaluateResult r) => {|#0:(EvaluateResultSuccess)r|};
                        var s = Cast(result);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast inside a local-function body is in a safe context — exercises
    /// the LocalFunctionStatementSyntax branch of IsInSafeContext (line 134).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastInsideLocalFunction_InSafeContext_DoesNotReportDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        // The cast is inside a local function — IsInSafeContext should detect
                        // the LocalFunctionStatementSyntax boundary (line 134).
                        EvaluateResultSuccess? Process(EvaluateResult r)
                        {
                            try
                            {
                                return (EvaluateResultSuccess)r;
                            }
                            catch
                            {
                                return null;
                            }
                        }

                        var s = Process(result);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(AnalyzerTestHelpers.GetWebDriverBiDiAssemblyPath()));

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
