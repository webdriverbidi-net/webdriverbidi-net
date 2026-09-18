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
            .WithArguments("GetTreeCommandParameters", "AdditionalData");

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
            .WithArguments("GetTreeCommandParameters", "AdditionalData");

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
            .WithArguments("GetTreeCommandParameters", "AdditionalData");

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
            .WithArguments("PartialCookie", "AdditionalData");

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
            .WithArguments("GetTreeCommandParameters", "AdditionalData");

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
        // A WebDriverBiDi type whose "AdditionalData" property returns Dictionary<string, string>
        // (not object?) should NOT fire — the value type does not match. Declaring the type in the
        // WebDriverBiDi namespace ensures the declaring-type check passes so this exercises
        // IsDictionaryStringObject returning false.
        string testCode = """
            using System.Collections.Generic;

            namespace WebDriverBiDi.Fakes
            {
                public class MyClass
                {
                    public Dictionary<string, string> AdditionalData { get; } = new Dictionary<string, string>();
                }
            }

            namespace TestNamespace
            {
                using WebDriverBiDi.Fakes;

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
            .WithArguments("CommandParameters", "AdditionalData");

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
        // A WebDriverBiDi type whose "AdditionalData" property carries [JsonExtensionData] but returns a
        // plain array rather than Dictionary<string, object?>. It passes the name filter, the namespace
        // test and the attribute test, and is rejected only by the dictionary-shape guard, which is what
        // keeps the rule from reporting a dictionary shape it cannot reason about.
        string testCode = """
            using System.Collections.Generic;
            using System.Text.Json.Serialization;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    // AdditionalData returning an array — not an INamedTypeSymbol path
                    [JsonExtensionData]
                    public string[] AdditionalData { get; } = new string[4];
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
                        cmd.AdditionalData[0] = "value";
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

    /// <summary>
    /// Tests that a property named <c>AdditionalData</c> whose type is an array rather than a
    /// dictionary is not flagged — exercises the non-INamedTypeSymbol property-type path.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ArrayTypedAdditionalDataProperty_DoesNotReportDiagnostic()
    {
        string testCode = """
            #nullable enable
            namespace TestApp
            {
                public class NotCommandParameters
                {
                    // Named AdditionalData, but the type is an array, not Dictionary<string, object?>.
                    public object?[] AdditionalData { get; } = new object?[4];
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        NotCommandParameters parameters = new NotCommandParameters();
                        parameters.AdditionalData[0] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task IndexerAssignment_OnUserTypeAdditionalData_NoDiagnostic()
    {
        // A user's own type that merely has an AdditionalData property of the same shape is not a
        // WebDriverBiDi received-data type, so mutating it must not be flagged.
        string testCode = """
            #nullable enable
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class MyType
                {
                    public Dictionary<string, object?> AdditionalData { get; } = new();
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var thing = new MyType();
                        thing.AdditionalData["ext"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    [Fact]
    public async Task IndexerAssignment_OnUserTypeInNamespaceWithLibraryPrefix_NoDiagnostic()
    {
        // A user's own AdditionalData dictionary on a type in a namespace that merely
        // begins with the library's root namespace name (here, WebDriverBiDiExtensions)
        // is not the library's Extensible plumbing and must not be branded with this
        // library's diagnostic.
        string testCode = """
            using System.Collections.Generic;

            namespace WebDriverBiDiExtensions
            {
                public class UserExtensibleType
                {
                    public Dictionary<string, object?> AdditionalData { get; } = new Dictionary<string, object?>();
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        UserExtensibleType userValue = new UserExtensibleType();
                        userValue.AdditionalData["ext"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that the nested object-initializer spelling <c>AdditionalData = { ["key"] = value }</c> is reported once per element.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IndexerElementInitializer_OnAdditionalData_ReportsWarningPerElement()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters
                        {
                            AdditionalData =
                            {
                                {|#0:["first"] = "value"|},
                                {|#1:["second"] = 2|},
                            },
                        };
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("GetTreeCommandParameters", "AdditionalData");

        DiagnosticResult expected1 = new DiagnosticResult(BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("GetTreeCommandParameters", "AdditionalData");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected0, expected1);
    }

    /// <summary>
    /// Tests that the nested collection-initializer spelling <c>AdditionalData = { { "key", value } }</c> is reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CollectionElementInitializer_OnAdditionalData_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters
                        {
                            AdditionalData = { {|#0:{ "ext", "value" }|} },
                        };
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("GetTreeCommandParameters", "AdditionalData");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that an AdditionalData initializer nested inside a member initializer (<c>Cookie = { AdditionalData = { ... } }</c>) is reported, naming the member's type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NestedMemberInitializer_OnAdditionalData_ReportsWarningNamingMemberType()
    {
        string testCode = """
            using WebDriverBiDi.Network;
            using WebDriverBiDi.Storage;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new SetCookieCommandParameters(new PartialCookie("name", BytesValue.FromString("value"), "example.com"))
                        {
                            Cookie =
                            {
                                AdditionalData = { {|#0:["ext"] = "value"|} },
                            },
                        };
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("PartialCookie", "AdditionalData");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected0);
    }

    /// <summary>
    /// Tests that an ordinary member initializer, and an initializer assigning some other property, are not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task PropertyAssignmentInInitializer_OtherThanAdditionalData_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var cmd = new GetTreeCommandParameters
                        {
                            MaxDepth = 2,
                        };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a null-forgiving receiver is still recognized, so peeling that wrapper matches what
    /// binding the receiver would have resolved to.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IndexerAssignment_ViaNullForgivingReceiver_ReportsWarning()
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
                        {|#0:cmd.AdditionalData!["ext"] = "value"|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("CommandParameters", "AdditionalData");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a local variable that merely shares the name is not reported: the name test is a
    /// cheap filter, and what the receiver actually binds to still decides.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IndexerAssignment_OnLocalNamedAdditionalData_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        Dictionary<string, object?> AdditionalData = new Dictionary<string, object?>();
                        AdditionalData["ext"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that a null-conditional receiver, whose member is written as a member binding, is
    /// recognized.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IndexerAssignment_ViaConditionalAccessReceiver_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        GetTreeCommandParameters? cmd = new GetTreeCommandParameters();
                        cmd?{|#0:.AdditionalData["ext"] = "value"|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("CommandParameters", "AdditionalData");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that a receiver that names no member at all, such as the result of a call, is rejected
    /// without binding it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IndexerAssignment_OnCallResult_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class TestClass
                {
                    private Dictionary<string, object?> GetData() => new Dictionary<string, object?>();

                    public void TestMethod()
                    {
                        this.GetData()["ext"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that <c>CapabilityRequest.AdditionalCapabilities</c> is reported. It carries
    /// <c>[JsonExtensionData]</c> and so shares the reflection-serialization hazard, but the rule
    /// previously keyed on the name <c>AdditionalData</c> and left it alone.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IndexerAssignment_OnAdditionalCapabilities_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        CapabilityRequest capabilities = new CapabilityRequest();
                        {|#0:capabilities.AdditionalCapabilities["vendor:option"] = "value"|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("CapabilityRequest", "AdditionalCapabilities");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that <c>Command.AdditionalCommandProperties</c> is reported, through the <c>Add</c> form.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddMethod_OnAdditionalCommandProperties_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.Protocol;
            using WebDriverBiDi.Session;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        Command command = new Command(1, new StatusCommandParameters());
                        {|#0:command.AdditionalCommandProperties.Add("vendor:option", "value")|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver022_AdditionalDataMutationAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Command", "AdditionalCommandProperties");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode, expected);
    }

    /// <summary>
    /// Tests that an extension-data property whose type is a named generic other than
    /// <c>Dictionary&lt;string, object?&gt;</c> is not reported. The dictionary-shape guard is what
    /// decides this, and only a named type reaches it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExtensionData_PropertyWithOtherDictionaryValueType_DoesNotReportDiagnostic()
    {
        string testCode = """
            using System.Collections.Generic;
            using System.Text.Json.Serialization;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    [JsonExtensionData]
                    public Dictionary<string, string> AdditionalData { get; } = new Dictionary<string, string>();
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
                        cmd.AdditionalData["ext"] = "value";
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver022_AdditionalDataMutationAnalyzer>(testCode);
    }
}
