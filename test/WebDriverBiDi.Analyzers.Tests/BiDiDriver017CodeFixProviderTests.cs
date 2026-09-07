// <copyright file="BiDiDriver017CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
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
    public async Task CodeFix_WrapsReceiverWithNullCoalescing()
    {
        string testCode = """
            #nullable enable
            using System.Collections.Generic;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ManualProxyConfiguration parameters = new ManualProxyConfiguration();
                        {|#0:parameters.NoProxyAddresses|}.Add("proxy1");
                    }
                }
            }
            """;

        string fixedCode = """
            #nullable enable
            using System.Collections.Generic;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ManualProxyConfiguration parameters = new ManualProxyConfiguration();
                        (parameters.NoProxyAddresses ??= new List<string>()).Add("proxy1");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "NoProxyAddresses");

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
