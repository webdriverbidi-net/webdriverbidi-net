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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var title = success.Result;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            var title = success.Result;
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var exception = {|#0:(EvaluateResultException)result|};
                        var details = exception.ExceptionDetails;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultException");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
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
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(CommandResult result)
                    {
                        // Target GetRealmsCommandResult is a CommandResult, not an EvaluateResult subtype.
                        var other = (GetRealmsCommandResult)result;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(GetRealmsCommandResult result)
                    {
                        // The immediate operand of the cast is typed CommandResult, not EvaluateResult.
                        var success = (EvaluateResultSuccess)(CommandResult)result;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var type = result.GetType();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var title = success.Result.Type;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = testCode,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success != null)
                        {
                            var title = success.Result.Type;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = testCode,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var exception = {|#0:(EvaluateResultException)result|};
                        var text = exception.ExceptionDetails.Text;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultException");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = testCode,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.Result.Type;
                        var realm = success.RealmId;
                        Console.WriteLine(value);
                        Console.WriteLine(realm);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = testCode,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = ({|CS0246:UnknownType|})result;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var success = (EvaluateResultSuccess){|CS0103:unknownVariable|};
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = result as {|CS0246:UnknownType|};
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var success = {|CS0103:unknownVariable|} as EvaluateResultSuccess;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var x = (EvaluateResultSuccess)({|CS0103:unknownVariable|});
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var x = ({|CS0103:unknownVariable|}) as EvaluateResultSuccess;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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
            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an EvaluateResult type in a different namespace does not fire —
    /// exercises the ContainingNamespace check in IsEvaluateResultBaseType (line 105).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because it deliberately
    /// declares <c>EvaluateResult</c>/<c>EvaluateResultSuccess</c> in a namespace other than
    /// <c>WebDriverBiDi.Script</c>. The real library types always live in <c>WebDriverBiDi.Script</c>,
    /// so the namespace-mismatch branch of <c>IsEvaluateResultBaseType</c> cannot be reproduced
    /// against the real API.
    /// </remarks>
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
    /// fire — exercises the IsEvaluateResultBaseType false branch (line 109 &amp;&amp; short-circuit).
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };

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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };
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

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectCast_FromNullLiteral_DoesNotCrashAnalyzer()
    {
        // (EvaluateResultSuccess)null! has a resolvable target type in WebDriverBiDi.Script but a
        // null operand type; the analyzer must not dereference a null ITypeSymbol (which would throw
        // and surface as AD0001, disabling the rule for the whole compilation).
        string test = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        EvaluateResultSuccess success = (EvaluateResultSuccess)null!;
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast in a try block whose catch is the ordinary catch (Exception) is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EvaluateResult_CastInTryCatchException_NoDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        try
                        {
                            var success = (EvaluateResultSuccess)result;
                            var title = success.Result;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast in a try block whose catch is a base type of InvalidCastException is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EvaluateResult_CastInTryCatchSystemException_NoDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        try
                        {
                            var success = (EvaluateResultSuccess)result;
                            var title = success.Result;
                        }
                        catch (SystemException)
                        {
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast in a try block whose catch carries a when filter, a deliberate handling idiom is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EvaluateResult_CastInTryCatchWithFilter_NoDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        try
                        {
                            var success = (EvaluateResultSuccess)result;
                            var title = success.Result;
                        }
                        catch (Exception ex) when (ex is InvalidCastException)
                        {
                        }
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast in a try statement that has no catch clause at all, so nothing handles the failure is reported. Treating any enclosing try statement as protection
    /// suppressed this.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EvaluateResult_CastInTryFinally_ReportsDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        try
                        {
                            var success = {|#0:(EvaluateResultSuccess)result|};
                            var title = success.Result;
                        }
                        finally
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast whose only catch clause catches an unrelated exception type is reported. Treating any enclosing try statement as protection
    /// suppressed this.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EvaluateResult_CastInTryCatchUnrelatedType_ReportsDiagnostic()
    {
        string test = """
            using System;
            using System.IO;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        try
                        {
                            var success = {|#0:(EvaluateResultSuccess)result|};
                            var title = success.Result;
                        }
                        catch (IOException)
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast in the catch block of a try statement, which that try does not cover is reported. Treating any enclosing try statement as protection
    /// suppressed this.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EvaluateResult_CastInCatchBlock_ReportsDiagnostic()
    {
        string test = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        try
                        {
                        }
                        catch (Exception)
                        {
                            var success = {|#0:(EvaluateResultSuccess)result|};
                            var title = success.Result;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyAnalyzerTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast in the then-branch of an <c>if</c> on the library's discriminator (<c>result.ResultType == EvaluateResultType.Success</c>) is not reported, in either operand order and through parentheses.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedByDiscriminatorIf_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result.ResultType == EvaluateResultType.Success)
                        {
                            var success = (EvaluateResultSuccess)result;
                        }

                        if ((EvaluateResultType.Exception == (result).ResultType))
                        {
                            var exception = (EvaluateResultException)(result);
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a cast in the then-branch of an <c>if</c> on a type test of the operand is not reported, for the <c>is T</c>, <c>is T x</c> and <c>is T { }</c> spellings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedByTypeTestIf_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess)
                        {
                            var success = (EvaluateResultSuccess)result;
                        }

                        if (result is EvaluateResultSuccess named)
                        {
                            var success = (EvaluateResultSuccess)result;
                        }

                        if (result is EvaluateResultSuccess { })
                        {
                            var success = (EvaluateResultSuccess)result;
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a guard reaches a cast on the right of an <c>&amp;&amp;</c>, in the true arm of a conditional expression, and through a conjunction in an <c>if</c> condition.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedByConjunctionOrConditionalOrAndOperand_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public string TestMethod(EvaluateResult result, bool flag)
                    {
                        string first = result.ResultType == EvaluateResultType.Success && ((EvaluateResultSuccess)result).RealmId.Length > 0 ? "a" : "b";
                        string second = result is EvaluateResultSuccess ? ((EvaluateResultSuccess)result).RealmId : "none";
                        if (flag && result.ResultType == EvaluateResultType.Success)
                        {
                            return ((EvaluateResultSuccess)result).RealmId;
                        }

                        if (result is EvaluateResultSuccess && flag)
                        {
                            return ((EvaluateResultSuccess)result).RealmId;
                        }

                        return first + second;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a cast in a switch section selected by the discriminator value, by a constant pattern of it, or by a type pattern on the operand is not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedBySwitchSection_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        switch (result.ResultType)
                        {
                            case EvaluateResultType.Success:
                                var success = (EvaluateResultSuccess)result;
                                break;
                            case EvaluateResultType.Exception when result is not null:
                                var exception = (EvaluateResultException)result;
                                break;
                        }

                        switch (result)
                        {
                            case EvaluateResultSuccess when result.RealmId.Length > 0:
                                var guarded = (EvaluateResultSuccess)result;
                                break;
                            case EvaluateResultSuccess:
                                var typed = (EvaluateResultSuccess)result;
                                break;
                            case EvaluateResultException { } exception:
                                var declared = (EvaluateResultException)result;
                                break;
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a cast after an <c>if</c> that leaves the block whenever the operand is not of the target type is not reported, for the <c>!=</c>, <c>is not</c>, <c>!(is)</c> and <c>||</c> spellings and for a block body ending in a throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_AfterEarlyExitGuard_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void NotEqualsGuard(EvaluateResult result)
                    {
                        if (result.ResultType != EvaluateResultType.Success)
                        {
                            return;
                        }

                        var first = (EvaluateResultSuccess)result;
                    }

                    public void IsNotGuard(EvaluateResult result)
                    {
                        if (result is not EvaluateResultSuccess) return;
                        var second = (EvaluateResultSuccess)result;
                    }

                    public void LogicalNotGuard(EvaluateResult result)
                    {
                        if (!(result is EvaluateResultSuccess))
                        {
                            Console.WriteLine("no");
                            throw new InvalidOperationException();
                        }

                        var third = (EvaluateResultSuccess)result;
                    }

                    public void DisjunctionGuard(EvaluateResult result, bool flag)
                    {
                        foreach (int i in new[] { 1 })
                        {
                            if (flag || result.ResultType != EvaluateResultType.Success)
                            {
                                continue;
                            }

                            var fourth = (EvaluateResultSuccess)result;
                        }

                        while (flag)
                        {
                            if (EvaluateResultType.Success != result.ResultType)
                            {
                                break;
                            }

                            var fifth = (EvaluateResultSuccess)result;
                        }

                        do
                        {
                            if (result.ResultType != EvaluateResultType.Success || flag)
                            {
                                break;
                            }

                            var sixth = (EvaluateResultSuccess)result;
                        }
                        while (flag);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a guard for the other result type, for a different operand, in the else-branch, in a switch section with an unrelated label, or an early-exit guard whose body does not exit, does not suppress the report.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_WithGuardThatDoesNotEstablishTheType_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result, EvaluateResult other, bool flag)
                    {
                        if (result.ResultType == EvaluateResultType.Exception)
                        {
                            var first = {|#0:(EvaluateResultSuccess)result|};
                        }

                        if (other is EvaluateResultSuccess)
                        {
                            var second = {|#1:(EvaluateResultSuccess)result|};
                        }

                        if (result is EvaluateResultSuccess)
                        {
                        }
                        else
                        {
                            var third = {|#2:(EvaluateResultSuccess)result|};
                        }

                        switch (result.ResultType)
                        {
                            case EvaluateResultType.Success:
                            default:
                                var fourth = {|#3:(EvaluateResultSuccess)result|};
                                break;
                        }

                        if (result.ResultType != EvaluateResultType.Success)
                        {
                            Console.WriteLine("not exiting");
                        }

                        var fifth = {|#4:(EvaluateResultSuccess)result|};

                        if (result.ResultType != EvaluateResultType.Success && flag)
                        {
                            return;
                        }

                        var sixth = {|#5:(EvaluateResultSuccess)result|};
                        var seventh = flag ? "x" : ({|#6:(EvaluateResultSuccess)result|}).RealmId;
                        var eighth = flag || ({|#7:(EvaluateResultSuccess)result|}).RealmId.Length > 0;
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(2)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected3 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(3)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected4 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(4)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected5 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(5)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected6 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(6)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected7 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(7)
            .WithArguments("EvaluateResultSuccess");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode, expected0, expected1, expected2, expected3, expected4, expected5, expected6, expected7);
    }

    /// <summary>
    /// Tests that a guard outside a lambda does not protect a cast inside it: the lambda runs when invoked, not where it is written.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_InsideLambdaUnderGuard_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess)
                        {
                            Func<string> read = () => ({|#0:(EvaluateResultSuccess)result|}).RealmId;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that comparisons that are not the discriminator test — a different member, a non-enum value, a not-equals in a positive guard, a pattern that is not a type pattern — do not suppress the report.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedByUnrelatedComparisons_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result, EvaluateResult other, EvaluateResultType type, bool flag)
                    {
                        if (result.RealmId == "x")
                        {
                            var first = {|#0:(EvaluateResultSuccess)result|};
                        }

                        if (result.ResultType == type)
                        {
                            var second = {|#1:(EvaluateResultSuccess)result|};
                        }

                        if (result.ResultType != EvaluateResultType.Success)
                        {
                            var third = {|#2:(EvaluateResultSuccess)result|};
                        }

                        if (result is { RealmId: "x" })
                        {
                            var fourth = {|#3:(EvaluateResultSuccess)result|};
                        }

                        if (result is not null)
                        {
                            var fifth = {|#4:(EvaluateResultSuccess)result|};
                        }

                        if (result is var anything)
                        {
                            var sixth = {|#5:(EvaluateResultSuccess)result|};
                        }

                        if (other is EvaluateResultSuccess named)
                        {
                            var otherNamed = {|#7:(EvaluateResultSuccess)result|};
                        }

                        if (result is not { RealmId: "x" })
                        {
                            return;
                        }

                        if (other is not EvaluateResultSuccess)
                        {
                            return;
                        }

                        if (!flag)
                        {
                            return;
                        }

                        var afterGuards = {|#8:(EvaluateResultSuccess)result|};

                        if (!(result.ResultType == EvaluateResultType.Exception))
                        {
                        }

                        if (result is not EvaluateResultException)
                        {
                        }

                        var seventh = {|#6:(EvaluateResultSuccess)result|};
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected2 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(2)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected3 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(3)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected4 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(4)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected5 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(5)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected6 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(6)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected7 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(7)
            .WithArguments("EvaluateResultSuccess");

        DiagnosticResult expected8 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(8)
            .WithArguments("EvaluateResultSuccess");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode, expected0, expected1, expected2, expected3, expected4, expected5, expected6, expected7, expected8);
    }

    /// <summary>
    /// Tests that comparing the discriminator with a same-named member of an unrelated enum is not
    /// the library's type test, so the cast is still reported (alongside the compiler's own complaint).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedByComparisonWithUnrelatedEnum_ReportsWarning()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public enum Outcome
                {
                    Success,
                }

                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if ({|CS0019:result.ResultType == Outcome.Success|})
                        {
                            var success = {|#0:(EvaluateResultSuccess)result|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that the pattern spelling of the discriminator test suppresses the report, both as a
    /// positive guard and as an early-exit guard.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_AfterDiscriminatorConstantPatternGuard_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void PositiveGuard(EvaluateResult result)
                    {
                        if (result.ResultType is EvaluateResultType.Success)
                        {
                            var first = (EvaluateResultSuccess)result;
                        }
                    }

                    public void EarlyExitGuard(EvaluateResult result)
                    {
                        if (result.ResultType is not EvaluateResultType.Success) return;
                        var second = (EvaluateResultSuccess)result;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that an `is` test naming the other derived type does not suppress the report, since that
    /// guard establishes the operand is not the type being cast to.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedByIsTestForOtherType_ReportsDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void OtherTypeGuard(EvaluateResult result)
                    {
                        if (result is EvaluateResultException)
                        {
                            var value = {|#0:(EvaluateResultSuccess)result|};
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a cast in a switch expression arm selected by the discriminator value or by a type pattern on the operand, or guarded by the arm's <c>when</c> clause, is not reported, and that a section label's <c>when</c> clause guards the section's statements.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_GuardedBySwitchExpressionArmOrWhenClause_NoDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        object byDiscriminator = result.ResultType switch
                        {
                            EvaluateResultType.Success => ((EvaluateResultSuccess)result).Result,
                            EvaluateResultType.Exception => ((EvaluateResultException)result).ExceptionDetails,
                            _ => null,
                        };

                        object byType = result switch
                        {
                            EvaluateResultSuccess => ((EvaluateResultSuccess)result).Result,
                            EvaluateResultException { } => ((EvaluateResultException)result).ExceptionDetails,
                            EvaluateResult other when other.RealmId.Length > 0 && result is EvaluateResultSuccess => ((EvaluateResultSuccess)result).Result,
                            _ => null,
                        };

                        object byWhenClause = result switch
                        {
                            _ when result.ResultType == EvaluateResultType.Success => ((EvaluateResultSuccess)result).Result,
                            _ => null,
                        };

                        switch (result.ResultType)
                        {
                            case EvaluateResultType resultType when result is EvaluateResultException:
                                EvaluateResultException guardedByWhenClause = (EvaluateResultException)result;
                                break;
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a cast is still reported in a switch expression arm that selects the other type or nothing, under a <c>when</c> clause that does not establish the type, and inside the very <c>when</c> clause that would establish it, for arms and for section labels alike.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Cast_InSwitchExpressionArmOrWhenClauseThatDoesNotEstablishItsType_ReportsDiagnostic()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        object wrongArm = result.ResultType switch
                        {
                            EvaluateResultType.Exception => {|#0:(EvaluateResultSuccess)result|},
                            _ => {|#1:(EvaluateResultSuccess)result|},
                        };

                        object unrelatedWhenClause = result switch
                        {
                            _ when result.RealmId.Length > 0 => {|#2:(EvaluateResultSuccess)result|},
                            _ => null,
                        };

                        object castInsideWhenClause = result switch
                        {
                            _ when ({|#3:(EvaluateResultSuccess)result|}).Result != null && result.ResultType == EvaluateResultType.Success => 1,
                            _ => null,
                        };

                        switch (result.ResultType)
                        {
                            case EvaluateResultType resultType when ({|#4:(EvaluateResultSuccess)result|}).Result != null && result is EvaluateResultSuccess:
                                break;
                            case EvaluateResultType otherType:
                                EvaluateResultSuccess unguarded = {|#5:(EvaluateResultSuccess)result|};
                                break;
                        }
                    }
                }
            }
            """;

        DiagnosticResult[] expected = new DiagnosticResult[6];
        for (int i = 0; i < expected.Length; i++)
        {
            expected[i] = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
                .WithLocation(i)
                .WithArguments("EvaluateResultSuccess");
        }

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer>(testCode, expected);
    }
}
