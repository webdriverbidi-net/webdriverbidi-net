// <copyright file="BiDiDriver014CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver014 code fix provider.
/// </summary>
public class BiDiDriver014CodeFixProviderTests
{
    /// <summary>
    /// Stub types mirroring the real geolocation command parameters: the Reset property is declared
    /// on the base class and returns the base type; the derived class has the public constructor.
    /// </summary>
    private const string InheritedResetStubTypes = """
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

            public class SetGeolocationOverrideCommandResult : CommandResult { }

            public class GeolocationCoordinates { }

            public class SetGeolocationOverrideCommandParameters : CommandParameters<SetGeolocationOverrideCommandResult>
            {
                protected SetGeolocationOverrideCommandParameters() { }

                public static SetGeolocationOverrideCommandParameters ResetGeolocationOverride => new SetGeolocationOverrideCoordinatesCommandParameters();

                [JsonIgnore]
                public override string MethodName => "emulation.setGeolocationOverride";
            }

            public class SetGeolocationOverrideCoordinatesCommandParameters : SetGeolocationOverrideCommandParameters
            {
                public SetGeolocationOverrideCoordinatesCommandParameters() : base() { }

                [JsonPropertyName("coordinates")]
                [JsonInclude]
                public GeolocationCoordinates? Coordinates { get; set; }
            }
        }

        """;

    /// <summary>
    /// Runs a code-fix verification for the inherited-Reset stub types with the given TestApp bodies.
    /// </summary>
    /// <param name="testBody">The TestApp namespace body containing the marked diagnostic.</param>
    /// <param name="fixedBody">The TestApp namespace body after the fix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous verification.</returns>
    private static async Task VerifyInheritedResetFixAsync(string testBody, string fixedBody)
    {
        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetGeolocationOverrideCoordinatesCommandParameters", "ResetGeolocationOverride");

        LfCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = InheritedResetStubTypes + testBody,
            FixedCode = InheritedResetStubTypes + fixedBody,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a local declared with the derived type is retyped to the declaring (base) type
    /// so that the base-typed Reset property can be assigned to it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_RetypesExplicitlyTypedLocal()
    {
        await VerifyInheritedResetFixAsync(
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetGeolocationOverrideCoordinatesCommandParameters parameters = {|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|};
                    }
                }
            }
            """,
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetGeolocationOverrideCommandParameters parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that a <c>var</c> local only has its initializer replaced.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_LeavesVarLocalDeclarationUnchanged()
    {
        await VerifyInheritedResetFixAsync(
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var parameters = {|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|};
                    }
                }
            }
            """,
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        var parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that an inline constructor argument is replaced with the base-qualified Reset property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_ReplacesInlineArgument()
    {
        await VerifyInheritedResetFixAsync(
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void Use(SetGeolocationOverrideCommandParameters parameters) { }

                    public void TestMethod()
                    {
                        this.Use({|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|});
                    }
                }
            }
            """,
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void Use(SetGeolocationOverrideCommandParameters parameters) { }

                    public void TestMethod()
                    {
                        this.Use(SetGeolocationOverrideCommandParameters.ResetGeolocationOverride);
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that a namespace-qualified declared type has only its rightmost identifier retyped.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_RetypesQualifiedLocalDeclaration()
    {
        await VerifyInheritedResetFixAsync(
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        WebDriverBiDi.Emulation.SetGeolocationOverrideCoordinatesCommandParameters parameters = {|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|};
                    }
                }
            }
            """,
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        WebDriverBiDi.Emulation.SetGeolocationOverrideCommandParameters parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that a nullable declared type is retyped inside the nullable annotation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_RetypesNullableLocalDeclaration()
    {
        await VerifyInheritedResetFixAsync(
            """
            #nullable enable
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetGeolocationOverrideCoordinatesCommandParameters? parameters = {|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|};
                    }
                }
            }
            """,
            """
            #nullable enable
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetGeolocationOverrideCommandParameters? parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that a declared type that is not an identifier (a predefined type such as
    /// <c>object</c>) is left unchanged, since the base-typed value is still assignable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_LeavesPredefinedTypeDeclarationUnchanged()
    {
        await VerifyInheritedResetFixAsync(
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        object parameters = {|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|};
                    }
                }
            }
            """,
            """
            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        object parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that the code fix replaces parameterless constructor with Reset property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        LfCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the code fix works with multiple variables and only fixes the flagged one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        LfCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeFix_WithFullyQualifiedConstruction_EmitsQualifiedResetAccess()
    {
        // The type is constructed with a fully-qualified name and no using directive, so the reset
        // property access must be qualified the same way or it would not resolve.
        string testCode = """
            namespace WebDriverBiDi
            {
                public abstract class CommandParameters { }
            }

            namespace WebDriverBiDi.Emulation
            {
                using WebDriverBiDi;

                public class SetTimeZoneOverrideCommandParameters : CommandParameters
                {
                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();
                }
            }

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var parameters = {|#0:new WebDriverBiDi.Emulation.SetTimeZoneOverrideCommandParameters()|};
                    }
                }
            }
            """;

        string fixedCode = """
            namespace WebDriverBiDi
            {
                public abstract class CommandParameters { }
            }

            namespace WebDriverBiDi.Emulation
            {
                using WebDriverBiDi;

                public class SetTimeZoneOverrideCommandParameters : CommandParameters
                {
                    public static SetTimeZoneOverrideCommandParameters ResetTimeZoneOverride => new();
                }
            }

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var parameters = WebDriverBiDi.Emulation.SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetTimeZoneOverrideCommandParameters", "ResetTimeZoneOverride");

        LfCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }
}
