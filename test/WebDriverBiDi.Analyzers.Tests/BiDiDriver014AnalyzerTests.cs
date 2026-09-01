// <copyright file="BiDiDriver014AnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver014 analyzer that detects parameterless constructor usage when a Reset property is available.
/// </summary>
public class BiDiDriver014AnalyzerTests
{
    /// <summary>
    /// Tests that an expression-bodied method produces no diagnostic: the statement walk sees no
    /// statements in a member without a block body, so the object creation is not examined.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParameterlessConstructor_InExpressionBodiedMethod_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public SetGeolocationOverrideCoordinatesCommandParameters Create() => new SetGeolocationOverrideCoordinatesCommandParameters();
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer>(test);
    }

    /// <summary>
    /// Tests that a parameterless constructor of a derived class is reported when the Reset
    /// property is declared on its base class and returns the base type. Uses the real
    /// <c>SetGeolocationOverrideCommandParameters</c> (protected constructor, declares
    /// <c>ResetGeolocationOverride</c> returning the base type) and its derived
    /// <c>SetGeolocationOverrideCoordinatesCommandParameters</c> (public parameterless constructor).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DerivedParameterlessConstructor_WithInheritedResetProperty_ReportsDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetGeolocationOverrideCoordinatesCommandParameters parameters = {|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetGeolocationOverrideCoordinatesCommandParameters", "ResetGeolocationOverride");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer>(test, expected);
    }

    /// <summary>
    /// Tests that an inline parameterless constructor of the derived class passed as an argument
    /// is reported when the Reset property is inherited.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DerivedInlineParameterlessConstructor_WithInheritedResetProperty_ReportsDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void Use(SetGeolocationOverrideCommandParameters parameters) { }

                    public void TestMethod()
                    {
                        this.Use({|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|});
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetGeolocationOverrideCoordinatesCommandParameters", "ResetGeolocationOverride");

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer>(test, expected);
    }

    /// <summary>
    /// Tests that setting a property on the derived instance suppresses the diagnostic, as it does
    /// for classes that declare their own Reset property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DerivedParameterlessConstructor_WithPropertyAssignment_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetGeolocationOverrideCoordinatesCommandParameters parameters = new SetGeolocationOverrideCoordinatesCommandParameters();
                        parameters.Coordinates = new GeolocationCoordinates(0.0, 0.0);
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer>(test);
    }

    /// <summary>
    /// Tests that Reset* properties returning a type unrelated to the constructed type (the
    /// property-level sentinel pattern used by SetViewportCommandParameters) are not treated as
    /// command-level reset properties — exercises the base-type walk exhausting without a match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ResetPropertyReturningUnrelatedType_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void Use(SetViewportCommandParameters parameters) { }

                    public void TestMethod()
                    {
                        SetViewportCommandParameters parameters = new SetViewportCommandParameters();
                        this.Use(new SetViewportCommandParameters());
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer>(test);
    }

    /// <summary>
    /// Tests that parameterless constructor without property assignment reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParameterlessConstructor_WithoutPropertyAssignment_ReportsDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that parameterless constructor with property assignment does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParameterlessConstructor_WithPropertyAssignment_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that using the Reset property directly does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task UsingResetProperty_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that non-CommandParameters types are not analyzed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that CommandParameters without a Reset property do not report a diagnostic. Uses the
    /// real <c>NavigateCommandParameters</c>, which takes constructor arguments and has no Reset
    /// property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CommandParametersWithoutResetProperty_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // This should not trigger BIDI014 (no Reset property)
                        var parameters = new NavigateCommandParameters("myContext", "https://example.com");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that multiple variables are tracked independently.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultipleVariables_IndependentTracking()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that populating a property suppresses the diagnostic. The real
    /// <c>SetTimeZoneOverrideCommandParameters.Contexts</c> is a get-only list, so it is configured
    /// with <c>Add</c>; the analyzer treats a member call on the tracked variable as configuration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParameterlessConstructor_WithContextsPopulated_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // This is correct - populating the Contexts list
                        var parameters = new SetTimeZoneOverrideCommandParameters();
                        parameters.Contexts.Add("context1");
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that object initializer syntax is handled correctly (should not report diagnostic).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParameterlessConstructor_WithObjectInitializer_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that variable without initializer is handled gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task VariableWithoutInitializer_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an inline parameterless constructor used directly as a method argument reports a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InlineParameterlessConstructor_AsMethodArgument_ReportsDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that an inline constructor with an object initializer does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InlineConstructor_WithObjectInitializer_AsMethodArgument_NoDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a parameterless constructor without property assignment reports a diagnostic
    /// for a CommandParameters class that has a list property and a command-level Reset property.
    /// Uses the real <c>SetExtraHeadersCommandParameters</c> (get-only <c>Headers</c> list and
    /// <c>ResetExtraHeaders</c>).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParameterlessConstructor_WithListPropertyAndResetProperty_ReportsDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Network;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // Should trigger BIDI014 — no property assignment, and ResetExtraHeaders exists
                        var parameters = {|#0:new SetExtraHeadersCommandParameters()|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetExtraHeadersCommandParameters", "ResetExtraHeaders");

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a parameterless constructor with a settable list property assigned via object
    /// initializer does not report a diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly. It exercises assigning a
    /// list property (with a public setter) through an object initializer, but every real
    /// command-parameters list property that has a Reset sibling (for example
    /// <c>SetExtraHeadersCommandParameters.Headers</c>) is get-only and can only be populated with
    /// <c>Add</c>, so this specific shape cannot be reproduced against the real API. The get-only
    /// population path is covered by <see cref="ParameterlessConstructor_WithCollectionAddCall_NoDiagnostic"/>.
    /// </remarks>
    [Fact]
    public async Task ParameterlessConstructor_WithListPropertyAssignedViaObjectInitializer_NoDiagnostic()
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

            namespace WebDriverBiDi.Network
            {
                using System.Text.Json.Serialization;
                using WebDriverBiDi;

                public class SetExtraHeadersCommandResult : CommandResult { }

                public class SetExtraHeadersCommandParameters : CommandParameters<SetExtraHeadersCommandResult>
                {
                    public static SetExtraHeadersCommandParameters ResetExtraHeaders => new();

                    [JsonIgnore]
                    public override string MethodName => "network.setExtraHeaders";

                    [JsonPropertyName("headers")]
                    [JsonInclude]
                    public List<string> Headers { get; set; } = new List<string>();

                    [JsonPropertyName("contexts")]
                    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                    public List<string>? Contexts { get; set; }
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Network;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        // No diagnostic — Headers assigned via object initializer
                        var parameters = new SetExtraHeadersCommandParameters
                        {
                            Headers = new List<string> { "X-Custom-Header: value" }
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

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that property assignment whose left-hand side is not a tracked variable name
    /// does not crash — exercises GetVariableName returning null via the _ => null arm
    /// (line 248) and the variableName == null guard (line 230).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task PropertyAssignment_OnLiteralExpression_DoesNotReportDiagnostic()
    {
        // Assignment to an array element — not a simple identifier or member access,
        // so GetVariableName hits the _ => null arm.
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetTimeZoneOverrideCommandParameters[] arr = [new SetTimeZoneOverrideCommandParameters()];
                        // Assignment to array element — GetVariableName hits _ => null
                        arr[0].TimeZone = "America/New_York";
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that property assignment accessed through a chained member access (e.g.
    /// obj.Sub.Prop = x) exercises the MemberAccessExpressionSyntax recursive arm of
    /// GetVariableName (line 247).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task PropertyAssignment_OnChainedMemberAccess_DoesNotReportDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class Holder
                {
                    public SetTimeZoneOverrideCommandParameters Params { get; set; } = new SetTimeZoneOverrideCommandParameters();
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        Holder holder = new Holder();
                        // Chained member access: GetVariableName recurses through MemberAccessExpressionSyntax
                        holder.Params.TimeZone = "America/New_York";
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a CommandParameters type with a parameterless constructor but NO Reset*
    /// static property does not report a diagnostic — exercises GetResetPropertyName
    /// returning null (line 285). Uses the real <c>GetCookiesCommandParameters</c>, which has a
    /// parameterless constructor and no Reset property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ParameterlessConstructor_WithNoResetProperty_DoesNotReportDiagnostic()
    {
        string test = """
            using WebDriverBiDi.Storage;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        GetCookiesCommandParameters p = new GetCookiesCommandParameters();
                    }
                }
            }
            """;

        RealAssemblyAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer> testState = new()
        {
            TestCode = test,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that assigning to a field (not a property) on a tracked variable does not
    /// mark the variable as having a property assignment — exercises the
    /// symbol is not IPropertySymbol path (line 231).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because the analyzer branch
    /// under test — an assignment to a public <em>field</em> (not a property) on a resettable
    /// command-parameters type — cannot be reproduced against the real API: every real
    /// command-parameters type exposes its data through properties, never public fields.
    /// </remarks>
    [Fact]
    public async Task FieldAssignment_DoesNotMarkVariableAsPropertyAssigned()
    {
        string test = """
            using System;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters { }

                public class TestParams : CommandParameters
                {
                    public TestParams() { }
                    public static TestParams Reset => new TestParams();
                    public string Field = string.Empty;
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        TestParams p = {|#0:new TestParams()|};
                        // Assignment to a field (not a property) — exercises line 231.
                        p.Field = "value";
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("TestParams", "Reset");

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests the three ways an inline constructor argument is rejected: it takes constructor
    /// arguments, it is not a command-parameters type, or its type exposes no usable static
    /// <c>Reset</c> property. The last case also walks past a public static property whose name does
    /// not begin with "Reset" and one that does but returns a different type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    /// <remarks>
    /// This test keeps a hand-written stub rather than the real assembly because it depends on
    /// specially-shaped types to hit specific rejection branches: a command-parameters type that
    /// exposes a public static property whose name does not begin with "Reset" does not exist in the
    /// real API, so that branch of <c>GetResetProperty</c> cannot be reproduced against it.
    /// </remarks>
    [Fact]
    public async Task InlineConstructors_ThatAreNotResettableParameters_DoNotReportDiagnostic()
    {
        string test = """
            using System;
            using System.Threading.Tasks;

            namespace WebDriverBiDi
            {
                public abstract class CommandParameters
                {
                    public abstract string MethodName { get; }
                }

                public abstract class CommandParameters<T> : CommandParameters
                    where T : CommandResult
                {
                }

                public class CommandResult { }
            }

            namespace WebDriverBiDi.Emulation
            {
                using WebDriverBiDi;

                public class SampleCommandResult : CommandResult { }

                // Requires constructor arguments.
                public class WithArgsCommandParameters : CommandParameters<SampleCommandResult>
                {
                    public WithArgsCommandParameters(string value) { }

                    public static WithArgsCommandParameters ResetValue => new WithArgsCommandParameters("x");

                    public override string MethodName => "emulation.withArgs";
                }

                // A command-parameters type with no usable Reset property.
                public class NoResetCommandParameters : CommandParameters<SampleCommandResult>
                {
                    // Public static, but the name does not begin with "Reset".
                    public static string Label => "label";

                    // Begins with "Reset", but the property type is not the containing type.
                    public static string ResetLabel => "reset";

                    public override string MethodName => "emulation.noReset";
                }
            }

            namespace TestApp
            {
                using WebDriverBiDi.Emulation;

                // Not a command-parameters type at all.
                public class PlainOptions { }

                public class TestClass
                {
                    private static void Consume(object value) { }

                    public void TestMethod()
                    {
                        // Constructor with arguments — skipped.
                        Consume(new WithArgsCommandParameters("x"));

                        // Parameterless, but not a command-parameters type — skipped.
                        Consume(new PlainOptions());

                        // A command-parameters type with no Reset property — skipped.
                        Consume(new NoResetCommandParameters());
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ParameterlessConstructor_WithCollectionAddCall_NoDiagnostic()
    {
        // SetExtraHeadersCommandParameters.Headers is a get-only list; the only way to populate it is
        // parameters.Headers.Add(...). That is genuine configuration, so the parameterless constructor
        // must not be flagged even though there is no 'parameters.X = value' assignment.
        string test = """
            using System.Threading.Tasks;
            using WebDriverBiDi.Network;

            namespace TestNamespace
            {
                public class TestClass
                {
                    public async Task TestMethod()
                    {
                        var parameters = new SetExtraHeadersCommandParameters();
                        parameters.Headers.Add(new Header("name", "value"));

                        // Unrelated statements: a member call on a non-tracked receiver, a bare call,
                        // and a non-invocation expression statement must not affect the tracked
                        // variable's configuration state.
                        System.Console.WriteLine("unrelated");
                        LocalHelper();
                        await Task.CompletedTask;

                        void LocalHelper()
                        {
                        }
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer>(test);
    }
}
