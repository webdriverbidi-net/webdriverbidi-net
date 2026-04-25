// <copyright file="BiDiDriver014CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver014 code fix provider.
/// </summary>
[TestFixture]
public class BiDiDriver014CodeFixProviderTests
{
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
}
