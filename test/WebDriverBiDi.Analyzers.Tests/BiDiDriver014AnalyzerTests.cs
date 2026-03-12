// <copyright file="BiDiDriver014AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver014 analyzer that detects parameterless constructor usage when a Reset property is available.
/// </summary>
[TestFixture]
public class BiDiDriver014AnalyzerTests
{
    /// <summary>
    /// Tests that parameterless constructor without property assignment reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ParameterlessConstructor_WithoutPropertyAssignment_ReportsDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }

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
                        // This should trigger BIDI014
                        var parameters = {|#0:new SetTimeZoneOverrideCommandParameters()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetTimeZoneOverrideCommandParameters", "ResetTimeZoneOverride");

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that parameterless constructor with property assignment does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ParameterlessConstructor_WithPropertyAssignment_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }

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
                        // This is correct - no diagnostic
                        var parameters = new SetTimeZoneOverrideCommandParameters();
                        parameters.TimeZone = "America/New_York";
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that using the Reset property directly does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task UsingResetProperty_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }

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
                        // This is correct - using the Reset property
                        var parameters = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that non-CommandParameters types are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NonCommandParametersType_NoDiagnostic()
    {
        string test = """
            using System;

            namespace TestApp
            {
                public class CustomParameters
                {
                    public CustomParameters() { }

                    public static CustomParameters Reset => new();

                    public string? Value { get; set; }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        // This should not trigger BIDI014 (not a CommandParameters type)
                        var parameters = new CustomParameters();
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that CommandParameters without a Reset property do not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CommandParametersWithoutResetProperty_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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

            namespace WebDriverBiDi.BrowsingContext
            {
                using System.Text.Json.Serialization;
                using WebDriverBiDi;

                public class NavigateCommandResult : CommandResult { }

                public class NavigateCommandParameters : CommandParameters<NavigateCommandResult>
                {
                    public NavigateCommandParameters(string url)
                    {
                        this.Url = url;
                    }

                    [JsonIgnore]
                    public override string MethodName => "browsingContext.navigate";

                    [JsonPropertyName("url")]
                    public string Url { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.BrowsingContext;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        // This should not trigger BIDI014 (no Reset property)
                        var parameters = new NavigateCommandParameters("https://example.com");
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that multiple variables are tracked independently.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MultipleVariables_IndependentTracking()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        // params1: incorrect usage (no properties set)
                        var params1 = {|#0:new SetTimeZoneOverrideCommandParameters()|};

                        // params2: correct usage (property set)
                        var params2 = new SetTimeZoneOverrideCommandParameters();
                        params2.TimeZone = "America/New_York";

                        // params3: correct usage (using Reset property)
                        var params3 = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetTimeZoneOverrideCommandParameters", "ResetTimeZoneOverride");

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that setting any property suppresses the diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ParameterlessConstructor_WithContextsPropertyAssignment_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }

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
                        // This is correct - setting Contexts property
                        var parameters = new SetTimeZoneOverrideCommandParameters();
                        parameters.Contexts = new List<string> { "context1" };
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that object initializer syntax is handled correctly (should not report diagnostic).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ParameterlessConstructor_WithObjectInitializer_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }

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
                        // This is correct - using object initializer
                        var parameters = new SetTimeZoneOverrideCommandParameters
                        {
                            TimeZone = "America/New_York"
                        };
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that variable without initializer is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task VariableWithoutInitializer_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetTimeZoneOverrideCommandParameters parameters;
                        parameters = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix replaces parameterless constructor with Reset property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_ReplacesParameterlessConstructorWithResetProperty()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }

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
                        // This should trigger BIDI014
                        var parameters = {|#0:new SetTimeZoneOverrideCommandParameters()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }

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
                        // This should trigger BIDI014
                        var parameters = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetTimeZoneOverrideCommandParameters", "ResetTimeZoneOverride");

        CSharpCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that the code fix works with multiple variables and only fixes the flagged one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CodeFix_WithMultipleVariables_FixesOnlyFlaggedOne()
    {
        string testCode = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        // params1: incorrect usage (no properties set)
                        var params1 = {|#0:new SetTimeZoneOverrideCommandParameters()|};

                        // params2: correct usage (property set)
                        var params2 = new SetTimeZoneOverrideCommandParameters();
                        params2.TimeZone = "America/New_York";

                        // params3: correct usage (using Reset property)
                        var params3 = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
                    }
                }
            }
            """;

        string fixedCode = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        // params1: incorrect usage (no properties set)
                        var params1 = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;

                        // params2: correct usage (property set)
                        var params2 = new SetTimeZoneOverrideCommandParameters();
                        params2.TimeZone = "America/New_York";

                        // params3: correct usage (using Reset property)
                        var params3 = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetTimeZoneOverrideCommandParameters", "ResetTimeZoneOverride");

        CSharpCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that an inline parameterless constructor used directly as a method argument reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InlineParameterlessConstructor_AsMethodArgument_ReportsDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void Execute(SetTimeZoneOverrideCommandParameters p) { }

                    public void TestMethod()
                    {
                        // Inline constructor with no properties — should fire BIDI014
                        Execute({|#0:new SetTimeZoneOverrideCommandParameters()|});
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetTimeZoneOverrideCommandParameters", "ResetTimeZoneOverride");

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync();
    }

    /// <summary>
    /// Tests that an inline constructor with an object initializer does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InlineConstructor_WithObjectInitializer_AsMethodArgument_NoDiagnostic()
    {
        string test = """
            using System;
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using System.Threading.Tasks;

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
                    public SetTimeZoneOverrideCommandParameters() { }

                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();

                    [JsonIgnore]
                    public override string MethodName => "emulation.setTimezoneOverride";

                    [JsonPropertyName("timezone")]
                    [JsonInclude]
                    public string? TimeZone { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void Execute(SetTimeZoneOverrideCommandParameters p) { }

                    public void TestMethod()
                    {
                        // Inline constructor with object initializer — intent is clear, no diagnostic
                        Execute(new SetTimeZoneOverrideCommandParameters { TimeZone = "America/New_York" });
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync();
    }
}
