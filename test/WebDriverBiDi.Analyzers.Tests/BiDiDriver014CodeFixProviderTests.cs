// <copyright file="BiDiDriver014CodeFixProviderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver014 code fix provider.
/// </summary>
public class BiDiDriver014CodeFixProviderTests
{
    /// <summary>
    /// Runs a code-fix verification against the real geolocation command-parameters types, where the
    /// Reset property (<c>ResetGeolocationOverride</c>) is declared on the base
    /// <c>SetGeolocationOverrideCommandParameters</c> but <em>returns</em> the derived
    /// <c>SetGeolocationOverrideCoordinatesCommandParameters</c>, whose public parameterless
    /// constructor is the one being replaced. Because the property's type is the derived type, a local
    /// declared with that type still holds the replacement and must be left as written; the fix
    /// retypes only when the property's return type cannot be assigned to the declared type.
    /// </summary>
    /// <param name="testCode">The source containing the marked diagnostic.</param>
    /// <param name="fixedCode">The source after the fix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous verification.</returns>
    private static async Task VerifyInheritedResetFixAsync(string testCode, string fixedCode)
    {
        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetGeolocationOverrideCoordinatesCommandParameters", "ResetGeolocationOverride");

        RealAssemblyCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a local declared with the derived type keeps that type: the inherited Reset
    /// property returns the derived type, so the declaration still compiles unchanged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_LeavesExplicitlyTypedLocalUnchanged()
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
                        SetGeolocationOverrideCoordinatesCommandParameters parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
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
                using System.Threading.Tasks;
                using WebDriverBiDi;
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.Emulation.SetGeolocationOverrideAsync({|#0:new SetGeolocationOverrideCoordinatesCommandParameters()|});
                    }
                }
            }
            """,
            """
            namespace TestApp
            {
                using System.Threading.Tasks;
                using WebDriverBiDi;
                using WebDriverBiDi.Emulation;

                public class TestClass
                {
                    public async Task TestMethod(BiDiDriver driver)
                    {
                        await driver.Emulation.SetGeolocationOverrideAsync(SetGeolocationOverrideCommandParameters.ResetGeolocationOverride);
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that a namespace-qualified declared type is likewise left as written.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_LeavesQualifiedLocalDeclarationUnchanged()
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
                        WebDriverBiDi.Emulation.SetGeolocationOverrideCoordinatesCommandParameters parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
                    }
                }
            }
            """);
    }

    /// <summary>
    /// Tests that a nullable declared type is likewise left as written.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_InheritedReset_LeavesNullableLocalDeclarationUnchanged()
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
                        SetGeolocationOverrideCoordinatesCommandParameters? parameters = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
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

        string fixedCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
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

        string fixedCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
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

        RealAssemblyCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
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

        RealAssemblyCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = testCode,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that the fix rewrites a target-typed <c>new()</c>. There is no written type to reuse, so
    /// the receiver comes from the analyzer-supplied type name instead of the construction's syntax.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TargetTypedParameterlessConstructor_IsReplacedWithResetProperty()
    {
        string test = """
            using WebDriverBiDi.Browser;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetDownloadBehaviorCommandParameters parameters = {|#0:new()|};
                    }
                }
            }
            """;

        string fixedCode = """
            using WebDriverBiDi.Browser;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetDownloadBehaviorCommandParameters parameters = SetDownloadBehaviorCommandParameters.ResetDownloadBehavior;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("SetDownloadBehaviorCommandParameters", "ResetDownloadBehavior");

        RealAssemblyCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = test,
            FixedCode = fixedCode,
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Runs a code-fix verification over a consumer-defined command-parameters hierarchy whose base
    /// declares a reset property returning the base type. This is the one shape in which the fix must
    /// retype the local, because the replacement cannot be assigned to a local declared with the
    /// constructed type. No type in the library has it — the library's one inherited reset property
    /// returns the derived type — but BIDI014 applies to any type deriving from the library's
    /// <c>CommandParameters</c>, so a consumer's own hierarchy can.
    /// </summary>
    /// <param name="declaration">The declared type and variable name, as written before the fix.</param>
    /// <param name="fixedDeclaration">The declared type and variable name expected after the fix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous verification.</returns>
    private static async Task VerifyResetReturningBaseTypeFixAsync(string declaration, string fixedDeclaration)
    {
        const string Template = """
            #nullable enable
            namespace TestApp
            {
                using WebDriverBiDi;

                public class BaseParameters : CommandParameters<EmptyResult>
                {
                    public override string MethodName => "custom.command";

                    public static BaseParameters ResetCustom => new BaseParameters();
                }

                public class DerivedParameters : BaseParameters
                {
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        DECLARATION = INITIALIZER;
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("DerivedParameters", "ResetCustom");

        RealAssemblyCodeFixTest<BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer, BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider> testState = new()
        {
            TestCode = Template
                .Replace("DECLARATION", declaration, StringComparison.Ordinal)
                .Replace("INITIALIZER", "{|#0:new DerivedParameters()|}", StringComparison.Ordinal),
            FixedCode = Template
                .Replace("DECLARATION", fixedDeclaration, StringComparison.Ordinal)
                .Replace("INITIALIZER", "BaseParameters.ResetCustom", StringComparison.Ordinal),
        };
        testState.ExpectedDiagnostics.Add(expected);

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a local declared with the constructed type is retyped to the reset property's own
    /// return type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_ResetReturningBaseType_RetypesExplicitlyTypedLocal()
    {
        await VerifyResetReturningBaseTypeFixAsync("DerivedParameters parameters", "BaseParameters parameters");
    }

    /// <summary>
    /// Tests that a namespace-qualified declared type has only its rightmost identifier retyped.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_ResetReturningBaseType_RetypesQualifiedLocalDeclaration()
    {
        await VerifyResetReturningBaseTypeFixAsync("TestApp.DerivedParameters parameters", "TestApp.BaseParameters parameters");
    }

    /// <summary>
    /// Tests that a nullable declared type is retyped inside the nullable annotation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_ResetReturningBaseType_RetypesNullableLocalDeclaration()
    {
        await VerifyResetReturningBaseTypeFixAsync("DerivedParameters? parameters", "BaseParameters? parameters");
    }

    /// <summary>
    /// Tests that a declared type that names no identifier to swap — a predefined type such as
    /// <c>object</c>, which already holds the replacement — has only its initializer replaced.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CodeFix_ResetReturningBaseType_LeavesPredefinedDeclaredTypeUnchanged()
    {
        await VerifyResetReturningBaseTypeFixAsync("object parameters", "object parameters");
    }
}
