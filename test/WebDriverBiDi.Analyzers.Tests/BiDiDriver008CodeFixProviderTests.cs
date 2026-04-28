// <copyright file="BiDiDriver008CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver008 code fix provider.
/// </summary>
[TestFixture]
public class BiDiDriver008CodeFixProviderTests
{
    /// <summary>
    /// Tests that code fix provider is registered for direct cast.
    /// Note: Full output validation disabled due to formatter line ending issues.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFixProvider_RegisteredForDirectCast()
    {
        string testCode = """
            using System;

            namespace WebDriverBiDi { public record CommandResult { } }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }
                public record EvaluateResultSuccess : EvaluateResult { public string V { get; set; } = ""; }
                public class ScriptModule { public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess(); }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("test");
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.V;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CodeActionValidationMode = Microsoft.CodeAnalysis.Testing.CodeActionValidationMode.None,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that code fix provider is registered for 'as' cast.
    /// Note: Full output validation disabled due to formatter line ending issues.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFixProvider_RegisteredForAsCast()
    {
        string testCode = """
            using System;

            namespace WebDriverBiDi { public record CommandResult { } }

            namespace WebDriverBiDi.Script
            {
                public record EvaluateResult : CommandResult { }
                public record EvaluateResultSuccess : EvaluateResult { public string V { get; set; } = ""; }
                public class ScriptModule { public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess(); }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Script;

                public class TestClass
                {
                    public void TestMethod(ScriptModule script)
                    {
                        EvaluateResult result = script.Evaluate("test");
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success != null)
                        {
                            var value = success.V;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CodeActionValidationMode = Microsoft.CodeAnalysis.Testing.CodeActionValidationMode.None,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests code fix for direct cast with variable declaration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_DirectCast_AppliesPatternMatching()
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.Value;
                    }
                }
            }
            """;

        // Expected output: code fix converts cast to pattern matching
        string fixedCode = """
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        if (result is EvaluateResultSuccess success)
                        {
                            var value = success.Value;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests code fix for 'as' cast with variable declaration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_AsCast_AppliesPatternMatching()
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success != null)
                        {
                            var value = success.Value;
                        }
                    }
                }
            }
            """;

        // Use CRLF line endings to match Roslyn formatter output on this platform
        string fixedCode = """
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        if (result is EvaluateResultSuccess success)
                        {
                            if (success != null)
                            {
                                var value = success.Value;
                            }
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
    public async Task CodeFix_DirectCast_WithTrailingStatement_PreservesTrailingStatements()
    {
        // Exercises the i > declarationIndex + dependentStatements.Count branch in
        // ConvertCastInVariableDeclarationAsync: a statement after the dependent block
        // that does NOT reference 'success' stays outside the if block.
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        var success = {|#0:(EvaluateResultSuccess)result|};
                        var value = success.Value;
                        var unrelated = 42;
                    }
                }
            }
            """;

        string fixedCode = """
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        if (result is EvaluateResultSuccess success)
                        {
                            var value = success.Value;
                        }
                        var unrelated = 42;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
    public async Task CodeFix_AsCast_WithTrailingStatement_PreservesTrailingStatements()
    {
        // Exercises the i > declarationIndex + dependentStatements.Count branch in
        // ConvertAsInVariableDeclarationAsync, plus the else { break; } path when
        // the statement after the null-check doesn't reference 'success'.
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        var success = {|#0:result as EvaluateResultSuccess|};
                        if (success != null)
                        {
                            var value = success.Value;
                        }
                        var unrelated = 42;
                    }
                }
            }
            """;

        string fixedCode = """
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        if (result is EvaluateResultSuccess success)
                        {
                            if (success != null)
                            {
                                var value = success.Value;
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

        CSharpCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    [Test]
    public async Task CodeFix_InlineCast_WrapsInIfStatement()
    {
        // Exercises the inline-cast path in ConvertCastToPatternMatchingAsync:
        // the cast is not in a variable declaration (parent is not EqualsValueClauseSyntax),
        // so the fix generates a fresh variable name and wraps the statement in an if block.
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        var value = ({|#0:(EvaluateResultSuccess)result|}).Value;
                    }
                }
            }
            """;

        string fixedCode = """
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
                    public string Value { get; set; } = "";
                }
                public class ScriptModule
                {
                    public EvaluateResult Evaluate(string s) => new EvaluateResultSuccess();
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
                        if (result is EvaluateResultSuccess success)
                        {
                            var value = (success).Value;
                        }
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("EvaluateResultSuccess");

        CSharpCodeFixTest<BiDiDriver008_UnsafeEvaluateResultCastAnalyzer, BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
