// <copyright file="BiDiDriver008CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver008 code fix provider.
/// </summary>
public class BiDiDriver008CodeFixProviderTests
{
    /// <summary>
    /// Tests that the fix leaves the document's line endings alone. The fix used to format the whole
    /// root and reparse its text with every CRLF rewritten to LF, which changed every line of a CRLF
    /// document, not the few the fix touched.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_OnCarriageReturnLineFeedDocument_PreservesLineEndings()
    {
        string testCode = string.Join(
            "\r\n",
            "using WebDriverBiDi.Script;",
            string.Empty,
            "namespace TestApp",
            "{",
            "    public class TestClass",
            "    {",
            "        public void TestMethod(EvaluateResult result)",
            "        {",
            "            var success = (EvaluateResultSuccess)result;",
            "            var value = success.RealmId;",
            "        }",
            "    }",
            "}",
            string.Empty);

        (IReadOnlyList<CodeAction> actions, Document document) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        CodeAction action = Assert.Single(actions);

        string fixedText = await AnalyzerTestHelpers.ApplyCodeActionAsync(action, document);

        // The lines the fix never touched keep the endings they came in with. Reformatting and
        // reparsing the whole root used to rewrite all of them to LF.
        Assert.Contains("using WebDriverBiDi.Script;\r\n\r\nnamespace TestApp\r\n{\r\n", fixedText);
        Assert.Contains("    }\r\n}", fixedText);
    }

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
    public async Task AsExpression_InMethodArgument_OffersNoCodeAction()
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
    public async Task CodeFix_InlineCastInReturn_OffersNoCodeAction()
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
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that no code action is offered for a cast in a return statement. Wrapping that statement
    /// in an <c>if</c> would leave a path that no longer returns, so the conversion declined to change
    /// anything — while still putting an item on the light-bulb menu that did nothing when chosen.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InlineCastInReturn_RegistersNoAction()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public EvaluateResultSuccess GetSuccess(EvaluateResult result)
                    {
                        return (EvaluateResultSuccess)result;
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Empty(actions);
    }

    /// <summary>
    /// Tests that no code action is offered for an <c>as</c> expression that does not initialize a
    /// local, which is the only shape the conversion knows how to rewrite.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AsExpressionInMethodArgument_RegistersNoAction()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        Consume(result as EvaluateResultSuccess);
                    }

                    private static void Consume(EvaluateResultSuccess? s) { }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Empty(actions);
    }

    /// <summary>
    /// Tests that the action is still offered for the shapes the conversion can rewrite, so the
    /// applicability gate did not turn the fix off.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastInitializingALocal_RegistersTheAction()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = (EvaluateResultSuccess)result;
                        var value = success.Result;
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        CodeAction action = Assert.Single(actions);
        Assert.Equal("Use pattern matching with 'is' expression", action.Title);
    }


    /// <summary>
    /// Tests that the action is offered for an inline cast in an expression statement, which the
    /// conversion rewrites by wrapping that statement in an <c>if</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InlineCastInExpressionStatement_RegistersTheAction()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        Consume((EvaluateResultSuccess)result);
                    }

                    private static void Consume(EvaluateResultSuccess s) { }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Single(actions);
    }

    /// <summary>
    /// Tests that the action is offered for a cast nested inside a local declaration without being
    /// that local's initializer, which the conversion also rewrites by wrapping the statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InlineCastWithinLocalDeclaration_RegistersTheAction()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        int length = Describe((EvaluateResultSuccess)result).Length;
                    }

                    private static string Describe(EvaluateResultSuccess s) => string.Empty;
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Single(actions);
    }


    /// <summary>
    /// Tests that no action is offered for a cast in a field initializer. It has the same
    /// declaration shape as a local, but is not a statement the conversion can wrap.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastInFieldInitializer_OffersNoCodeAction()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    private static readonly EvaluateResult Shared = null!;

                    private readonly EvaluateResultSuccess success = (EvaluateResultSuccess)Shared;
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Empty(actions);
    }


    /// <summary>
    /// Tests that no action is offered for a cast in a property initializer, whose enclosing syntax is
    /// not a variable declarator at all.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CastInPropertyInitializer_OffersNoCodeAction()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    private static readonly EvaluateResult Shared = null!;

                    private EvaluateResultSuccess Success { get; } = (EvaluateResultSuccess)Shared;
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider>(testCode, referenceWebDriverBiDi: true);
        Assert.Empty(actions);
    }

    /// <summary>
    /// Tests that a cast nested in a declaration's initializer is rewritten by moving the declaration, and the statements that use its variable, into the pattern block — rather than wrapping the declaration alone, which would leave the later use out of scope (CS0103). A local declared by a moved statement is tracked in turn, so a later use of it moves as well; a statement using none of them stays.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_CastNestedInDeclarationInitializer_KeepsDeclaredVariableInScope()
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
                        dynamic sink = 1;
                        string realm = ({|#0:(EvaluateResultSuccess)result|}).RealmId;
                        int alongside = 1;
                        Console.WriteLine(realm);
                        sink.Consume(alongside);
                        Console.WriteLine("done");
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
                    public void TestMethod(EvaluateResult result)
                    {
                        dynamic sink = 1;
                        if (result is EvaluateResultSuccess success)
                        {
                            string realm = (success).RealmId;
                            int alongside = 1;
                            Console.WriteLine(realm);
                            sink.Consume(alongside);
                        }
                        Console.WriteLine("done");
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a declaration of several variables is rewritten as a nested conversion, so that no declarator is lost.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_CastInitializingOneOfSeveralDeclarators_KeepsEveryDeclarator()
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
                        EvaluateResultSuccess first = {|#0:(EvaluateResultSuccess)result|}, second = null;
                        Console.WriteLine(second);
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
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            EvaluateResultSuccess first = success, second = null;
                            Console.WriteLine(second);
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast or <c>as</c> initializing a declaration placed directly in a switch section, which has no enclosing block whose statements could move, offers no action.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_DeclarationsDirectlyInSwitchSection_OfferNoCodeAction()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result, int mode)
                    {
                        switch (mode)
                        {
                            case 1:
                                var success = {|#0:(EvaluateResultSuccess)result|};
                                break;
                            case 2:
                                var maybe = {|#1:result as EvaluateResultSuccess|};
                                break;
                        }
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

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);
        testState.ExpectedDiagnostics.Add(expected1);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast inside an expression statement is rewritten by wrapping just that statement,
    /// which declares nothing that later statements could use.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_CastInExpressionStatement_WrapsThatStatement()
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
                        Console.WriteLine(({|#0:(EvaluateResultSuccess)result|}).RealmId);
                        Console.WriteLine("done");
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
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            Console.WriteLine((success).RealmId);
                        }
                        Console.WriteLine("done");
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a cast initializing a for-loop variable, whose declaration is not a statement of a
    /// block, offers no action.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_CastInForInitializer_OffersNoCodeAction()
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
                        for (var success = {|#0:(EvaluateResultSuccess)result|}; success != null; success = null)
                        {
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an <c>as</c> expression that does not directly initialize a single block-local
    /// variable — a property initializer, a for-loop initializer, one of several declarators —
    /// offers no action.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_AsExpressionsNotInitializingASingleLocal_OfferNoCodeAction()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    private static EvaluateResult result = null!;

                    public EvaluateResultSuccess? Initialized { get; } = {|#0:result as EvaluateResultSuccess|};

                    public void TestMethod()
                    {
                        for (var success = {|#1:result as EvaluateResultSuccess|}; success != null; success = null)
                        {
                        }

                        EvaluateResultSuccess? first = {|#2:result as EvaluateResultSuccess|}, second = first;
                        Console.WriteLine(second);
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

        RealAssemblyCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = testCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);
        testState.ExpectedDiagnostics.Add(expected1);
        testState.ExpectedDiagnostics.Add(expected2);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CastFollowedByAReturnThatUsesIt_OffersNoCodeAction()
    {
        // Moving the return into the if block would leave the method with no return on the path where
        // the pattern does not match, which does not compile (CS0161).
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public RemoteValue TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        return success.Result;
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
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CastFollowedByAThrowThatUsesIt_OffersNoCodeAction()
    {
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public RemoteValue TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        throw new InvalidOperationException(success.RealmId);
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
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CastFollowedByAConditionalReturn_StillAppliesPatternMatching()
    {
        // The moved statement can return, but the method still returns on every path afterwards, so
        // the rewrite compiles and the fix is offered.
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public string TestMethod(EvaluateResult result, bool wanted)
                    {
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        if (wanted)
                        {
                            return success.RealmId;
                        }

                        return null;
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
                    public string TestMethod(EvaluateResult result, bool wanted)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            if (wanted)
                            {
                                return success.RealmId;
                            }
                        }

                        return null;
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
    public async Task AsCastWithAnExitingNullGuard_OffersNoCodeAction()
    {
        // The pattern variable is never null, so the guard would become dead code and the failing case
        // would stop returning early and fall out of the if instead.
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result)
                    {
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success == null)
                        {
                            return;
                        }

                        System.Console.WriteLine(success.RealmId);
                        System.Console.WriteLine("done");
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
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AsCastWithAnIsNullGuardThatThrows_OffersNoCodeAction()
    {
        // The `is null` spelling of the same guard.
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
                        if (success is null)
                        {
                            throw new InvalidOperationException();
                        }

                        Console.WriteLine(success.RealmId);
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
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AsCastWithANullGuardThatFallsThrough_StillAppliesPatternMatching()
    {
        // A guard that merely wraps the work leaves the failing case doing nothing either way, which
        // is what the rewrite produces, so the fix is still offered.
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
                            Console.WriteLine(success.RealmId);
                        }
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
                    public void TestMethod(EvaluateResult result)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            if (success != null)
                            {
                                Console.WriteLine(success.RealmId);
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
    public async Task AsCastWithAReversedNullGuard_OffersNoCodeAction()
    {
        // The same guard with the operands the other way round.
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
                        if (null == success)
                        {
                            return;
                        }

                        Console.WriteLine(success.RealmId);
                        Console.WriteLine("done");
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
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AsCastWithAnExitingGuardThatIsNotANullTest_StillAppliesPatternMatching()
    {
        // An equality test against something other than null says nothing about the conversion, so it
        // does not withhold the fix.
        string testCode = """
            using System;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(EvaluateResult result, EvaluateResultSuccess other)
                    {
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success == other)
                        {
                            return;
                        }

                        Console.WriteLine(success.RealmId);
                        Console.WriteLine("done");
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
                    public void TestMethod(EvaluateResult result, EvaluateResultSuccess other)
                    {
                        if (result is EvaluateResultSuccess success)
                        {
                            if (success == other)
                            {
                                return;
                            }
                            Console.WriteLine(success.RealmId);
                        }
                        Console.WriteLine("done");
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
}
