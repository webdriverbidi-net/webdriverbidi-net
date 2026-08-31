// <copyright file="BiDiDriver017AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver017 analyzer that suggests ??= when adding to nullable list properties.
/// </summary>
public class BiDiDriver017AnalyzerTests
{
    /// <summary>
    /// Tests that Add on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNullableListProperty_ReportsDiagnostic()
    {
        string test = """
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

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "NoProxyAddresses");

        RealAssemblyAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on nullable list property with ??= does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNullableListProperty_WithNullCoalescing_NoDiagnostic()
    {
        string test = """
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

        RealAssemblyAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on non-nullable list property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNonNullableListProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;

            namespace TestApp
            {
                public class CustomParameters
                {
                    public List<string> Items { get; } = new();
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        CustomParameters parameters = new();
                        parameters.Items.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on local variable (not property) does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToLocalVariable_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        List<string>? list = null;
                        list.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddRange on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddRangeToNullableListProperty_ReportsDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        {|#0:Items|}.AddRange(new[] { "a", "b" });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Items");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Insert on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InsertToNullableListProperty_ReportsDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        {|#0:Items|}.Insert(0, "item");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Items");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that InsertRange on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InsertRangeToNullableListProperty_ReportsDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        {|#0:Items|}.InsertRange(0, new[] { "a", "b" });
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Items");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on nullable IList property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNullableIListProperty_ReportsDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public IList<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        {|#0:Items|}.Add("item");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Items");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on nullable ICollection property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNullableICollectionProperty_ReportsDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public ICollection<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        {|#0:Items|}.Add("item");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Items");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on non-nullable list property with initializer does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNonNullableListPropertyWithInitializer_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string> Items { get; set; } = new();

                    public void TestMethod()
                    {
                        Items.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddRange with null-conditional does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddRangeWithNullConditional_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        Items?.AddRange(new[] { "a", "b" });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Insert with null-coalescing assignment does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InsertWithNullCoalescingAssignment_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        (Items ??= new List<string>()).Insert(0, "item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-list method call does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonListMethodCall_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;

            namespace TestApp
            {
                public class TestClass
                {
                    public string? Value { get; set; }

                    public void TestMethod()
                    {
                        var result = Value?.ToString();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that methods not in the add list (e.g., Remove) do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RemoveFromNullableListProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        Items?.Remove("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add with null-conditional operator does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddWithNullConditional_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        Items?.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that AddRange with null-coalescing inside a larger expression does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddRangeNestedInCoalescingAssignment_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        (Items ??= new List<string>()).AddRange(new[] { "a", "b" });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that InsertRange with null-coalescing inside a larger expression does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InsertRangeNestedInCoalescingAssignment_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        (Items ??= new List<string>()).InsertRange(0, new[] { "a", "b" });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocation on expression with unresolved type does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddOnUnresolvedType_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        dynamic items = new List<string>();
                        items.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on non-generic list does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNonGenericCollectionProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections;

            namespace TestApp
            {
                public class TestClass
                {
                    public ArrayList? Items { get; set; }

                    public void TestMethod()
                    {
                        Items?.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on nullable value type property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddOnNullableValueTypeProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;

            namespace TestApp
            {
                public class TestClass
                {
                    public int? Value { get; set; }

                    public void TestMethod()
                    {
                        // This is nonsensical but should not crash the analyzer
                        var result = Value?.ToString();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on non-collection type does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddOnNonCollectionProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;

            namespace TestApp
            {
                public class CustomType
                {
                    public void Add(string item) { }
                }

                public class TestClass
                {
                    public CustomType? Items { get; set; }

                    public void TestMethod()
                    {
                        Items?.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that invocations without member access syntax are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LocalFunctionAdd_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        void Add(string item) { }
                        Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on field (not property) does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToField_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    private List<string>? items;

                    public void TestMethod()
                    {
                        items?.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on IEnumerable property (not IList or ICollection) does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToIEnumerableProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public IEnumerable<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        // This won't compile but analyzer should handle gracefully
                        // Items?.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Clear method on nullable list property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ClearNullableListProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        Items.Clear();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that nested Add call (e.g., within another method) with null coalescing is detected correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NestedCoalescingWithAdd_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public List<string>? Items { get; set; }

                    public void TestMethod()
                    {
                        var count = (Items ??= new List<string>()).Count;
                        (Items ??= new List<string>()).Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on nested conditional access property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddOnNestedConditionalAccessProperty_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class Container
                {
                    public TestClass? Data { get; set; }
                }

                public class TestClass
                {
                    public List<string> Items { get; set; } = new();
                }

                public class Program
                {
                    public void TestMethod()
                    {
                        Container? container = new();
                        container?.Data.Items.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on property with unresolved type does not crash.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddOnPropertyWithUnresolvedType_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;

            namespace TestApp
            {
                public class TestClass
                {
                    public dynamic Items { get; set; }

                    public void TestMethod()
                    {
                        Items.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that using the null-conditional operator (?.Add) suppresses the diagnostic —
    /// exercises the ConditionalAccessExpressionSyntax guard (line 84).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NullableListAdd_WithNullConditionalOperator_DoesNotReportDiagnostic()
    {
        string test = """
            #nullable enable
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ManualProxyConfiguration p = new ManualProxyConfiguration();
                        // null-conditional ?.Add — should be suppressed (line 84)
                        p.NoProxyAddresses?.Add("item");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that using a ??= coalesce-assignment suppresses the diagnostic —
    /// exercises IsInsideNullCoalescingAssignment returning true (line 147) and the
    /// corresponding early return (line 90).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NullableListAdd_InsideNullCoalescingAssignment_DoesNotReportDiagnostic()
    {
        string test = """
            #nullable enable
            using System.Collections.Generic;
            using WebDriverBiDi.Session;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ManualProxyConfiguration p = new ManualProxyConfiguration();
                        // (p.NoProxyAddresses ??= new List<string>()).Add(...) — suppressed (line 90)
                        (p.NoProxyAddresses ??= new List<string>()).Add("item");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add called on a value whose type cannot be resolved does not crash —
    /// exercises the receiverType == null guard (line 97).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Add_OnUnresolvableReceiver_DoesNotReportDiagnostic()
    {
        string test = """
            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // Unresolvable expression — receiverType will be null
                        {|CS0103:unknownVar|}.Add("item");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that Add on a nullable value-type property does not report a diagnostic —
    /// exercises GetNullableListElementType returning (false, null) for value-type nullables
    /// (line 163) and IsNullableType returning true for Nullable{T} (line 204).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Add_OnNullableValueTypeProperty_DoesNotReportDiagnostic()
    {
        // int? is a Nullable<int> — GetNullableListElementType returns (false, null)
        // because Nullable<T>.OriginalDefinition.SpecialType == System_Nullable_T.
        string test = """
            namespace TestApp
            {
                public class TestClass
                {
                    public int? Value { get; set; }

                    public void TestMethod()
                    {
                        // This is actually a compile error, but exercises the value-type nullable path
                        Value.{|CS1061:Add|}(1);
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that adding to a nullable IList{T} property reports a warning —
    /// exercises the IList/ICollection branch of GetNullableListElementType (line 141).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NullableICollectionAdd_WithoutInitialization_ReportsWarning()
    {
        // Kept synthetic: no public WebDriverBiDi type exposes a nullable ICollection<T>? property,
        // so the metadata-backed shape this branch needs cannot be reproduced against the real
        // assembly. The interface-typed nullable collection path is also covered against ordinary
        // user code in AddToNullableICollectionProperty_ReportsDiagnostic.
        string test = """
            #nullable enable
            using System.Collections.Generic;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters { }

                public class TestCommandParameters : CommandParameters
                {
                    public ICollection<string>? Items { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        TestCommandParameters p = new TestCommandParameters();
                        {|#0:p.Items|}.Add("item");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver017_NullableListAddAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Items");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that adding to a non-nullable List property does not report a warning —
    /// exercises the IsNullableType false branch (line 156).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NonNullableListAdd_DoesNotReportWarning()
    {
        // The real SetTimeZoneOverrideCommandParameters.Contexts is a non-nullable List<string>,
        // so IsNullableType returns false and BIDI017 does not report.
        string test = """
            #nullable enable
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetTimeZoneOverrideCommandParameters p = new SetTimeZoneOverrideCommandParameters();
                        p.Contexts.Add("item");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that adding to a nullable IList{T} property reports a warning —
    /// exercises the IList/ICollection branch of GetNullableListElementType (line 141).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NullableIListAdd_WithoutInitialization_ReportsWarning()
    {
        // Kept synthetic: no public WebDriverBiDi type exposes a nullable IList<T>? property, so the
        // metadata-backed shape this branch needs cannot be reproduced against the real assembly.
        // The interface-typed nullable list path is also covered against ordinary user code in
        // AddToNullableIListProperty_ReportsDiagnostic.
        string test = """
            #nullable enable
            using System.Collections.Generic;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters { }

                public class TestCommandParameters : CommandParameters
                {
                    public IList<string>? Items { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        TestCommandParameters p = new TestCommandParameters();
                        {|#0:p.Items|}.Add("item");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver017_NullableListAddAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Items");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that nullable <c>ICollection&lt;T&gt;</c> and <c>IList&lt;T&gt;</c> properties are
    /// flagged alongside nullable <c>List&lt;T&gt;</c> properties.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddToNullableInterfaceCollectionProperties_ReportsDiagnostics()
    {
        string test = """
            #nullable enable
            using System.Collections.Generic;

            namespace TestApp
            {
                public class Parameters
                {
                    public ICollection<string>? Contexts { get; set; }

                    public IList<string>? Names { get; set; }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        Parameters parameters = new Parameters();
                        {|#0:parameters.Contexts|}.Add("context1");
                        {|#1:parameters.Names|}.Add("name1");
                    }
                }
            }
            """;

        DiagnosticResult contexts = new DiagnosticResult(
            BiDiDriver017_NullableListAddAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Contexts");
        DiagnosticResult names = new DiagnosticResult(
            BiDiDriver017_NullableListAddAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("string", "Names");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(contexts);
        testState.ExpectedDiagnostics.Add(names);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
