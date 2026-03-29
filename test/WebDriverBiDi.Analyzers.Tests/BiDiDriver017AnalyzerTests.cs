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
[TestFixture]
public class BiDiDriver017AnalyzerTests
{
    /// <summary>
    /// Tests that Add on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddToNullableListProperty_ReportsDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    [JsonIgnore]
                    public abstract string MethodName { get; }

                    [JsonIgnore]
                    public abstract Type ResponseType { get; }
                }

                public abstract class CommandParameters<T> : CommandParameters
                    where T : CommandResult
                {
                    [JsonIgnore]
                    public override Type ResponseType => typeof(T);
                }

                public class CommandResult { }
            }

            namespace WebDriverBiDi.Emulation
            {
                using System.Text.Json.Serialization;
                using WebDriverBiDi;

                public class SetTimeZoneOverrideCommandResult : CommandResult { }

                public class SetTimeZoneOverrideCommandParameters : CommandParameters<SetTimeZoneOverrideCommandResult>
                {
                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("contexts")]
                    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                    public List<string>? Contexts { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetTimeZoneOverrideCommandParameters parameters = new();
                        {|#0:parameters.Contexts|}.Add("context1");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Contexts");

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on nullable list property with ??= does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task AddToNullableListProperty_WithNullCoalescing_NoDiagnostic()
    {
        string test = """
            #nullable enable
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    [JsonIgnore]
                    public abstract string MethodName { get; }

                    [JsonIgnore]
                    public abstract Type ResponseType { get; }
                }

                public abstract class CommandParameters<T> : CommandParameters
                    where T : CommandResult
                {
                    [JsonIgnore]
                    public override Type ResponseType => typeof(T);
                }

                public class CommandResult { }
            }

            namespace WebDriverBiDi.Emulation
            {
                using System.Text.Json.Serialization;
                using WebDriverBiDi;

                public class SetTimeZoneOverrideCommandResult : CommandResult { }

                public class SetTimeZoneOverrideCommandParameters : CommandParameters<SetTimeZoneOverrideCommandResult>
                {
                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("contexts")]
                    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                    public List<string>? Contexts { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetTimeZoneOverrideCommandParameters parameters = new();
                        (parameters.Contexts ??= new List<string>()).Add("context1");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver017_NullableListAddAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on non-nullable list property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on local variable (not property) does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix correctly applies ??= to the receiver.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_WrapsReceiverWithNullCoalescing()
    {
        string testCode = """
            #nullable enable
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    [JsonIgnore]
                    public abstract string MethodName { get; }

                    [JsonIgnore]
                    public abstract Type ResponseType { get; }
                }

                public abstract class CommandParameters<T> : CommandParameters
                    where T : CommandResult
                {
                    [JsonIgnore]
                    public override Type ResponseType => typeof(T);
                }

                public class CommandResult { }
            }

            namespace WebDriverBiDi.Emulation
            {
                using System.Text.Json.Serialization;
                using WebDriverBiDi;

                public class SetTimeZoneOverrideCommandResult : CommandResult { }

                public class SetTimeZoneOverrideCommandParameters : CommandParameters<SetTimeZoneOverrideCommandResult>
                {
                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("contexts")]
                    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                    public List<string>? Contexts { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetTimeZoneOverrideCommandParameters parameters = new();
                        {|#0:parameters.Contexts|}.Add("context1");
                    }
                }
            }
            """;

        string fixedCode = """
            #nullable enable
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    [JsonIgnore]
                    public abstract string MethodName { get; }

                    [JsonIgnore]
                    public abstract Type ResponseType { get; }
                }

                public abstract class CommandParameters<T> : CommandParameters
                    where T : CommandResult
                {
                    [JsonIgnore]
                    public override Type ResponseType => typeof(T);
                }

                public class CommandResult { }
            }

            namespace WebDriverBiDi.Emulation
            {
                using System.Text.Json.Serialization;
                using WebDriverBiDi;

                public class SetTimeZoneOverrideCommandResult : CommandResult { }

                public class SetTimeZoneOverrideCommandParameters : CommandParameters<SetTimeZoneOverrideCommandResult>
                {
                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("contexts")]
                    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                    public List<string>? Contexts { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetTimeZoneOverrideCommandParameters parameters = new();
                        (parameters.Contexts ??= new List<string>()).Add("context1");
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("string", "Contexts");

        CSharpCodeFixTest<BiDiDriver017_NullableListAddAnalyzer, BiDiDriver017_NullableListAddCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddRange on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Insert on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that InsertRange on nullable list property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on nullable IList property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on nullable ICollection property reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on non-nullable list property with initializer does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddRange with null-conditional does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Insert with null-coalescing assignment does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-list method call does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that methods not in the add list (e.g., Remove) do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add with null-conditional operator does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that AddRange with null-coalescing inside a larger expression does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that InsertRange with null-coalescing inside a larger expression does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that invocation on expression with unresolved type does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on non-generic list does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on nullable value type property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on non-collection type does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that invocations without member access syntax are handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on field (not property) does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on IEnumerable property (not IList or ICollection) does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Clear method on nullable list property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that nested Add call (e.g., within another method) with null coalescing is detected correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on nested conditional access property does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that Add on property with unresolved type does not crash.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
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

        await testState.RunAsync();
    }
}
