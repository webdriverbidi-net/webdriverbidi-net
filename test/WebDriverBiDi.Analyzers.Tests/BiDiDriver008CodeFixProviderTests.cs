// <copyright file="BiDiDriver008CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver008 code fix provider.
/// </summary>
public class BiDiDriver008CodeFixProviderTests
{
    /// <summary>
    /// Tests that code fix provider is registered for direct cast.
    /// Note: Full output validation disabled due to formatter line ending issues.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFixProvider_RegisteredForDirectCast()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.RealmId;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            CodeActionValidationMode = Microsoft.CodeAnalysis.Testing.CodeActionValidationMode.None,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that code fix provider is registered for 'as' cast.
    /// Note: Full output validation disabled due to formatter line ending issues.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFixProvider_RegisteredForAsCast()
    {
        string testCode = """
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
                            var value = success.RealmId;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            CodeActionValidationMode = Microsoft.CodeAnalysis.Testing.CodeActionValidationMode.None,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests code fix for direct cast with variable declaration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_DirectCast_AppliesPatternMatching()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.RealmId;
                    }
                }
            }
            """;

        // Expected output: code fix converts cast to pattern matching
        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            var value = success.RealmId;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests code fix for 'as' cast with variable declaration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_AsCast_AppliesPatternMatching()
    {
        string testCode = """
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
                            var value = success.RealmId;
                        }
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            if (success != null)
                            {
                                var value = success.RealmId;
                            }
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeFix_DirectCast_WithTrailingStatement_PreservesTrailingStatements()
    {
        // Exercises the i > declarationIndex + dependentStatements.Count branch in
        // ConvertCastInVariableDeclarationAsync: a statement after the dependent block
        // that does NOT reference 'success' stays outside the if block.
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.RealmId;
                        var unrelated = 42;
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            var value = success.RealmId;
                        }
                        var unrelated = 42;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeFix_AsCast_WithTrailingStatement_PreservesTrailingStatements()
    {
        // Exercises the i > declarationIndex + dependentStatements.Count branch in
        // ConvertAsInVariableDeclarationAsync, plus the else { break; } path when
        // the statement after the null-check doesn't reference 'success'.
        string testCode = """
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
                            var value = success.RealmId;
                        }
                        var unrelated = 42;
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            if (success != null)
                            {
                                var value = success.RealmId;
                            }
                        }
                        var unrelated = 42;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeFix_InlineCast_WrapsInIfStatement()
    {
        // Exercises the inline-cast path in ConvertCastToPatternMatchingAsync:
        // the cast is not in a variable declaration (parent is not EqualsValueClauseSyntax),
        // so the fix generates a fresh variable name and wraps the statement in an if block.
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var value = ({|#0:(EvaluateResultSuccess)result|}).RealmId;
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            var value = (success).RealmId;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that when an 'as' expression appears in a non-variable-declaration context
    /// (e.g. directly as a method argument) the code fix returns the document unchanged —
    /// exercises the "other cases" return at line 280 of the code fix provider.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsExpression_InMethodArgument_CodeFixIsNoOp()
    {
        string testCode = """
            using WebDriverBiDi.Script;
            using System;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        // 'as' expression passed directly as an argument, not a declaration.
                        Consume({|#0:result as EvaluateResultSuccess|});
                    }

                    private static void Consume(EvaluateResultSuccess? s) { }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
            NumberOfIncrementalIterations = 1,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix converts a cast in a variable declaration when there are
    /// no dependent statements after the declaration — exercises the
    /// dependentStatements.LastOrDefault()?.GetTrailingTrivia() null branch.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastInVariableDeclaration_NoDependentStatements_AppliesFixCorrectly()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        int before = 1;
                        var success = {|#0:(EvaluateResultSuccess)result|};
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        int before = 1;
                        if (result is EvaluateResultSuccess success)
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix converts an 'as' expression in a variable declaration when
    /// there are no dependent statements after the declaration — exercises the
    /// dependentStatements.LastOrDefault()?.GetTrailingTrivia() null branch for 'as'.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsInVariableDeclaration_NoDependentStatements_AppliesFixCorrectly()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        int before = 1;
                        var success = {|#0:result as EvaluateResultSuccess|};
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        int before = 1;
                        if (result is EvaluateResultSuccess success)
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a following statement containing an identifier whose text matches the declared
    /// variable but whose symbol is a different member — here a field of the same name reached
    /// through <c>this</c> — is not treated as depending on the cast result.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_IdentifierWithMatchingTextButDifferentSymbol_IsNotADependentStatement()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    private string success = string.Empty;

                    public void TestMethod(EvaluateResult result)
                    {
                        EvaluateResultSuccess success = {|#0:(EvaluateResultSuccess)result|};

                        // Contains an identifier whose text is "success" but whose symbol is the
                        // field, not the local declared above.
                        Console.WriteLine(this.success);
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    private string success = string.Empty;

                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                        }

                        // Contains an identifier whose text is "success" but whose symbol is the
                        // field, not the local declared above.
                        Console.WriteLine(this.success);
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0);

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            NumberOfIncrementalIterations = 1,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeFix_DirectCast_WithInterveningStatement_KeepsVariableInScope()
    {
        // A statement that does not reference 'success' appears between the declaration and a later
        // statement that does. The fix must move every statement through the last reference into the
        // if block so 'success' remains in scope; stopping at the first non-referencing statement
        // would strand the later reference (CS0103/CS0165).
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var unrelated = 42;
                        var value = success.RealmId;
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            var unrelated = 42;
                            var value = success.RealmId;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeFix_InlineCastInReturn_IsNoOp()
    {
        // A cast inside a return statement cannot be wrapped in an if without leaving a code path
        // that does not return a value (CS0161), so the fix leaves such casts unchanged.
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public EvaluateResultSuccess GetSuccess(EvaluateResult result)
                    {
                        return {|#0:(EvaluateResultSuccess)result|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
            NumberOfIncrementalIterations = 1,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
