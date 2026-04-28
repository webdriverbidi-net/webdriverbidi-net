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
[TestFixture]
public class BiDiDriver022AnalyzerTests
{
    // -----------------------------------------------------------------------
    // Positive cases — should report BIDI022
    // -----------------------------------------------------------------------

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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
}
