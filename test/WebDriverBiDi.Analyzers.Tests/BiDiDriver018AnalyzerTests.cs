// <copyright file="BiDiDriver018AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver018 analyzer that detects unsafe ValueAs&lt;Dictionary&lt;string, object&gt;&gt;
/// and ValueAs&lt;List&lt;object&gt;&gt; on RemoteValue instances.
/// </summary>
[TestFixture]
public class BiDiDriver018AnalyzerTests
{
    private const string TestFilePrefix = """
        using System.Collections.Generic;
        using WebDriverBiDi.Script;

        namespace WebDriverBiDi.Script
        {
            public class RemoteValue
            {
                public T ValueAs<T>() => default!;
            }

            public class RemoteValueDictionary : Dictionary<object, RemoteValue> { }

            public class RemoteValueList : List<RemoteValue> { }
        }

        namespace TestApp
        {
        """;

    private const string TestFileSuffix = """
        }
        """;

    /// <summary>
    /// Tests that ValueAs&lt;Dictionary&lt;string, object&gt;&gt; on RemoteValue reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsDictionary_OnRemoteValue_ReportsWarning()
    {
        string test = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    var dict = value.{|#0:ValueAs<Dictionary<string, object>>()|};
                }
            }
            """ + TestFileSuffix;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dictionary<string, object>");

        CSharpAnalyzerTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that ValueAs&lt;List&lt;object&gt;&gt; on RemoteValue reports a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsList_OnRemoteValue_ReportsWarning()
    {
        string test = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    var list = value.{|#0:ValueAs<List<object>>()|};
                }
            }
            """ + TestFileSuffix;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("List<object>");

        CSharpAnalyzerTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that ValueAs&lt;RemoteValueDictionary&gt; on RemoteValue does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsRemoteValueDictionary_OnRemoteValue_NoDiagnostic()
    {
        string test = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    RemoteValueDictionary dict = value.ValueAs<RemoteValueDictionary>();
                }
            }
           """ + TestFileSuffix;

        CSharpAnalyzerTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that ValueAs&lt;RemoteValueList&gt; on RemoteValue does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsRemoteValueList_OnRemoteValue_NoDiagnostic()
    {
        string test = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    RemoteValueList list = value.ValueAs<RemoteValueList>();
                }
            }
            """ + TestFileSuffix;

        CSharpAnalyzerTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that ValueAs&lt;string&gt; on RemoteValue does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsString_OnRemoteValue_NoDiagnostic()
    {
        string test = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    string text = value.ValueAs<string>();
                }
            }
            """ + TestFileSuffix;

        CSharpAnalyzerTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that ValueAs&lt;Dictionary&lt;string, object&gt;&gt; on non-RemoteValue does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsDictionary_OnNonRemoteValue_NoDiagnostic()
    {
        string test = """
            using System.Collections.Generic;

            namespace TestApp
            {
                public class CustomValue
                {
                    public T ValueAs<T>() => default!;
                }

                public class TestClass
                {
                    public void TestMethod(CustomValue value)
                    {
                        var dict = value.ValueAs<Dictionary<string, object>>();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix replaces ValueAs&lt;Dictionary&lt;string, object&gt;&gt; with ValueAs&lt;RemoteValueDictionary&gt;.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsDictionary_CodeFix_ReplacesWithRemoteValueDictionary()
    {
        string testCode = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    var dict = value.{|#0:ValueAs<Dictionary<string, object>>()|};
                    var name = dict["name"];
                }
            }
            """ + TestFileSuffix;

        string fixedCode = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    var dict = value.ValueAs<RemoteValueDictionary>();
                    var name = dict["name"];
                }
            }
            """ + TestFileSuffix;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("Dictionary<string, object>");

        CSharpCodeFixTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, BiDiDriver018_UnsafeRemoteValueValueAsCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix replaces ValueAs&lt;List&lt;object&gt;&gt; with ValueAs&lt;RemoteValueList&gt;.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ValueAsList_CodeFix_ReplacesWithRemoteValueList()
    {
        string testCode = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    var list = value.{|#0:ValueAs<List<object>>()|};
                    foreach (var item in list) { }
                }
            }
            """ + TestFileSuffix;

        string fixedCode = TestFilePrefix + """
            public class TestClass
            {
                public void TestMethod(RemoteValue value)
                {
                    var list = value.ValueAs<RemoteValueList>();
                    foreach (var item in list) { }
                }
            }
            """ + TestFileSuffix;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("List<object>");

        CSharpCodeFixTest<BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer, BiDiDriver018_UnsafeRemoteValueValueAsCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }
}
