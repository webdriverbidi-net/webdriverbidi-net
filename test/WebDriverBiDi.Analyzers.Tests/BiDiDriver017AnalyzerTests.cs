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
}
