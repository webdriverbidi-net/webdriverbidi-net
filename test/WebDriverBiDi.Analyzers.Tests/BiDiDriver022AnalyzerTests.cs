// <copyright file="BiDiDriver022AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver022 analyzer.
/// </summary>
public class BiDiDriver022AnalyzerTests
{
    // -----------------------------------------------------------------------
    // Positive cases — should report BIDI022
    // -----------------------------------------------------------------------

    [Fact]
    public async Task IndexerAssignment_OnCommandParametersAdditionalData_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        {|#0:cmd.AdditionalData["ext"] = "value"|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("GetTreeCommandParameters");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task AddMethod_OnCommandParametersAdditionalData_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        {|#0:cmd.AdditionalData.Add("ext", 42)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("GetTreeCommandParameters");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task TryAddMethod_OnCommandParametersAdditionalData_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        {|#0:cmd.AdditionalData.TryAdd("ext", 42)|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("GetTreeCommandParameters");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task IndexerAssignment_OnStoragePartialCookieAdditionalData_ReportsWarning()
    {
        // Verifies that the analyzer also fires for non-CommandParameters types that expose
        // a Dictionary<string, object?> AdditionalData property.
        string testCode = """
            using WebDriverBiDi.Network;
            using WebDriverBiDi.Storage;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cookie = new PartialCookie("name", BytesValue.FromString("v"), "example.com");
                        {|#0:cookie.AdditionalData["ext"] = "value"|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("PartialCookie");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    // -----------------------------------------------------------------------
    // Negative cases — should produce no diagnostic
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadingAdditionalDataByKey_NoDiagnostic()
    {
        // Reading back a value that was already written should not fire again.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        {|#0:cmd.AdditionalData["ext"] = "value"|};
                        object? val = cmd.AdditionalData["ext"];
                    }
                }
            }
            """;

        // Only the write at #0 fires; the read on the next line does not.
        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("GetTreeCommandParameters");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    [Fact]
    public async Task RemoveMethod_OnCommandParametersAdditionalData_NoDiagnostic()
    {
        // Remove does not add a new value, so no AOT risk is introduced.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        cmd.AdditionalData.Remove("ext");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task ClearMethod_OnCommandParametersAdditionalData_NoDiagnostic()
    {
        // Clear removes values; it does not introduce new non-AOT-safe objects.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        cmd.AdditionalData.Clear();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task IteratingAdditionalData_NoDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        foreach (KeyValuePair<string, object?> pair in cmd.AdditionalData)
                        {
                            _ = pair.Value;
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task CountProperty_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        int count = cmd.AdditionalData.Count;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task ContainsKey_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        bool has = cmd.AdditionalData.ContainsKey("ext");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task IndexerAssignment_OnUnrelatedDictionary_NoDiagnostic()
    {
        // A locally-declared Dictionary<string, object?> that is not an AdditionalData property should not fire.
        string testCode = """
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var dict = new Dictionary<string, object?>();
                        dict["key"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task AddMethod_OnUnrelatedDictionary_NoDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var dict = new Dictionary<string, object?>();
                        dict.Add("key", "value");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task IndexerAssignment_OnPropertyNamed_OtherThanAdditionalData_NoDiagnostic()
    {
        // A property named "Data" (not "AdditionalData") with the same type should NOT fire.
        // This exercises the property.Name != "AdditionalData" guard branch.
        string testCode = """
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class MyClass
                {
                    public Dictionary<string, object?> Data { get; } = new Dictionary<string, object?>();
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var obj = new MyClass();
                        obj.Data["key"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task IndexerAssignment_OnAdditionalDataWithWrongValueType_NoDiagnostic()
    {
        // An "AdditionalData" property that returns Dictionary<string, string> (not object?)
        // should NOT fire — the value type does not match.
        // This exercises the IsDictionaryStringObject returning false.
        string testCode = """
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class MyClass
                {
                    public Dictionary<string, string> AdditionalData { get; } = new Dictionary<string, string>();
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var obj = new MyClass();
                        obj.AdditionalData["key"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task IndexerAssignment_ViaParenthesizedExpression_ReportsWarning()
    {
        // Accessing AdditionalData via a parenthesized expression exercises the
        // ReceiverTypeName fallback path (non-MemberAccessExpression receiver).
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters();
                        {|#0:(cmd.AdditionalData)["ext"] = "value"|};
                    }
                }
            }
            """;

        // The fallback path gets ContainingType.Name of the AdditionalData property symbol,
        // which is the declaring type (CommandParameters), not the concrete type.
        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("CommandParameters");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that accessing a property whose return type is not a named type (e.g. an
    /// array type like T[]) does not trigger BIDI022 — exercises the
    /// "property.Type is not INamedTypeSymbol" guard (line 83).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AdditionalData_PropertyWithArrayReturnType_DoesNotReportDiagnostic()
    {
        // Define a CommandParameters subclass with an "AdditionalData" property returning
        // a plain array type (not a named generic type). IsAdditionalDataProperty will
        // hit the "property.Type is not INamedTypeSymbol" guard and return false.
        string testCode = """
            using System.Collections.Generic;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    // AdditionalData returning an array — not an INamedTypeSymbol path
                    public string[]? AdditionalData { get; }
                }

                public class GetTreeCommandParameters : CommandParameters
                {
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        GetTreeCommandParameters cmd = new GetTreeCommandParameters();
                        string[]? data = cmd.AdditionalData;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that an invocation (e.g. Add) on an expression that is not a member access
    /// does not report a diagnostic — exercises the invocation.Expression is not
    /// MemberAccessExpressionSyntax guard in AnalyzeInvocation (line 157).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AdditionalDataAdd_NotMemberAccess_DoesNotReportDiagnostic()
    {
        // Invoke Add through a local variable, not a member access chain.
        string testCode = """
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        Dictionary<string, object?> dict = new Dictionary<string, object?>();
                        // Direct local variable call — not a member-access expression
                        dict.Add("key", "value");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that an AdditionalData property returning a non-Dictionary type does not
    /// fire — exercises the IsDictionaryStringObject false branch (line 80).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AdditionalData_WithNonDictionaryType_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    // AdditionalData returning List, not Dictionary — IsDictionaryStringObject returns false.
                    public List<string> AdditionalData { get; } = new();
                }

                public class GetTreeCommandParameters : CommandParameters { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        GetTreeCommandParameters cmd = new GetTreeCommandParameters();
                        cmd.AdditionalData.Add("value");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }


    /// <summary>
    /// Tests that calling a non-Add method (e.g. Clear) on AdditionalData does not fire —
    /// exercises the !ValueAddingMethodNames.Contains false branch (line 149).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AdditionalData_Clear_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    public Dictionary<string, object?> AdditionalData { get; } = new();
                }

                public class GetTreeCommandParameters : CommandParameters { }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        GetTreeCommandParameters cmd = new GetTreeCommandParameters();
                        // Clear is not in ValueAddingMethodNames — no diagnostic.
                        cmd.AdditionalData.Clear();
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that invoking a non-member-access expression (e.g. a delegate variable) named
    /// "Add" does not fire — exercises invocation.Expression is not MemberAccessExpressionSyntax
    /// true path (line 145 branch 1).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DelegateAdd_NotMemberAccess_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // Invocation where Expression is NOT MemberAccessExpressionSyntax.
                        Action<string> Add = s => { };
                        Add("value");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }
}
