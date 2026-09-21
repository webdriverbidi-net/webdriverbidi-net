// <copyright file="BiDiDriver017CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver017 code fix provider.
/// </summary>
public class BiDiDriver017CodeFixProviderTests
{
    /// <summary>
    /// Tests that the code fix correctly applies ??= to the receiver.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task BelowCSharp8_ReportsNothingSoTheFixIsNeverAskedFor()
    {
        // The fix emits ??=, which is C# 8, and offers no fallback for older language versions, so the
        // analyzer does not report below it. The rule reads the property's declared annotation, which
        // metadata carries whatever the consumer's language version or nullable context, so the guard
        // is explicit rather than a side effect of the annotation being invisible.
        string testCode = """
            using System.Collections.Generic;
            using WebDriverBiDi.UserAgentClientHints;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ClientHintsMetadata parameters = new ClientHintsMetadata();
                        parameters.FormFactors.Add("proxy1");
                    }
                }
            }
            """;

        (IReadOnlyList<CodeAction> actions, Document _) = await AnalyzerTestHelpers.GetCodeActionsAsync<BiDiDriver017_NullableListAddAnalyzer, BiDiDriver017_NullableListAddCodeFixProvider>(
            testCode,
            referenceWebDriverBiDi: true,
            languageVersion: LanguageVersion.CSharp7_3);

        Assert.Empty(actions);
    }

    [Fact]
    public async Task CodeFix_WrapsReceiverWithNullCoalescing()
    {
        string testCode = """
            #nullable enable
            using System.Collections.Generic;
            using WebDriverBiDi.UserAgentClientHints;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ClientHintsMetadata parameters = new ClientHintsMetadata();
                        {|#0:parameters.FormFactors|}.Add("proxy1");
                    }
                }
            }
            """;

        string fixedCode = """
            #nullable enable
            using System.Collections.Generic;
            using WebDriverBiDi.UserAgentClientHints;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ClientHintsMetadata parameters = new ClientHintsMetadata();
                        (parameters.FormFactors ??= new List<string>()).Add("proxy1");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "FormFactors");

        RealAssemblyCodeFixTest<BiDiDriver017_NullableListAddAnalyzer, BiDiDriver017_NullableListAddCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NullableListOnUnrelatedType_NoDiagnostic()
    {
        // A nullable List<?>.Add() call on a class that does not inherit from
        // CommandParameters should not trigger BIDI017.
        string testCode = """
            #nullable enable
            using System.Collections.Generic;

            namespace TestApp
            {
                public class MyClass
                {
                    public List<string>? Items { get; set; }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var obj = new MyClass();
                        obj.Items?.Add("item");
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver017_NullableListAddAnalyzer>(testCode);
    }

    /// <summary>
    /// Tests that the element type of the new list is qualified when the file does not import its namespace, so the fixed code compiles; the message still names the type minimally.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_QualifiesElementTypeWhenItsNamespaceIsNotImported()
    {
        string testCode = """
            #nullable enable
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        WebDriverBiDi.Network.ContinueRequestCommandParameters parameters = new("request");
                        {|#0:parameters.Headers|}.Add(new WebDriverBiDi.Network.Header("name", "value"));
                    }
                }
            }
            """;

        string fixedCode = """
            #nullable enable
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        WebDriverBiDi.Network.ContinueRequestCommandParameters parameters = new("request");
                        (parameters.Headers ??= new List<WebDriverBiDi.Network.Header>()).Add(new WebDriverBiDi.Network.Header("name", "value"));
                    }
                }
            }
            """;

        DiagnosticResult expected0 = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Header", "Headers");

        RealAssemblyCodeFixTest<BiDiDriver017_NullableListAddAnalyzer, BiDiDriver017_NullableListAddCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected0);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
