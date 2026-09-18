// <copyright file="BiDiDriver028SpecRangeValueOutOfRangeAnalyzerTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

/// <summary>
/// Tests for the BiDiDriver028 analyzer, which flags a compile-time constant assigned to a
/// command-parameter property whose value is outside the WebDriver BiDi specification range declared
/// by <c>SpecRangeAttribute</c>.
/// </summary>
public class BiDiDriver028AnalyzerTests
{
    [Fact]
    public async Task ObjectInitializer_QualityAboveRange_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ImageFormat format = new ImageFormat { Quality = {|#0:1.5|} };
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("1.5", "Quality", "[0, 1]");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task DirectAssignment_QualityBelowRange_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(ImageFormat format)
                    {
                        format.Quality = {|#0:-0.1|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("-0.1", "Quality", "[0, 1]");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task Quality_WithinRangeNullOrNonConstant_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(ImageFormat format, double someVariable)
                    {
                        ImageFormat mid = new ImageFormat { Quality = 0.5 };
                        ImageFormat low = new ImageFormat { Quality = 0.0 };
                        ImageFormat high = new ImageFormat { Quality = 1.0 };
                        ImageFormat none = new ImageFormat { Quality = null };
                        format.Quality = someVariable;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task Grid_SentinelAndBoundaryValues_NoDiagnostic()
    {
        // Grid has range [0.0, 1.0] with a reset sentinel of -1.
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        MediaFeatures sentinel = new MediaFeatures { Grid = -1 };
                        MediaFeatures off = new MediaFeatures { Grid = 0 };
                        MediaFeatures on = new MediaFeatures { Grid = 1 };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task Grid_OutOfRange_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(MediaFeatures features)
                    {
                        MediaFeatures above = new MediaFeatures { Grid = {|#0:2|} };
                        features.Grid = {|#1:-2|};
                    }
                }
            }
            """;

        DiagnosticResult aboveExpected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("2", "Grid", "[0, 1]");

        DiagnosticResult belowExpected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("-2", "Grid", "[0, 1]");

        await VerifyDiagnosticsAsync(testCode, aboveExpected, belowExpected);
    }

    [Fact]
    public async Task MaxDomDepth_SentinelAndLargeValue_NoDiagnostic()
    {
        // MaxDomDepth has range [0, +inf) with a reset sentinel of -1.
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SerializationOptions sentinel = new SerializationOptions { MaxDomDepth = -1 };
                        SerializationOptions large = new SerializationOptions { MaxDomDepth = 100000 };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task MaxDomDepth_BelowRange_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SerializationOptions options = new SerializationOptions { MaxDomDepth = {|#0:-5|} };
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("-5", "MaxDomDepth", "[0, ∞)");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task PrintMargin_BelowRange_ReportsWarning()
    {
        // Left has range [0, +inf) with no sentinel.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        PrintMarginParameters margins = new PrintMarginParameters { Left = {|#0:-1|} };
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("-1", "Left", "[0, ∞)");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task PrintMargin_InRange_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        PrintMarginParameters margins = new PrintMarginParameters { Left = 10 };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task DevicePixelRatio_SentinelAndValuesAboveZero_NoDiagnostic()
    {
        // devicePixelRatio is (float .gt 0.0) with a reset sentinel of -1, so the sentinel and any
        // value above zero are both acceptable.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        SetViewportCommandParameters sentinel = new SetViewportCommandParameters { DevicePixelRatio = -1 };
                        SetViewportCommandParameters fractional = new SetViewportCommandParameters { DevicePixelRatio = 0.5 };
                        SetViewportCommandParameters whole = new SetViewportCommandParameters { DevicePixelRatio = 2 };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task DevicePixelRatio_ZeroAndUndeclaredNegative_ReportsWarning()
    {
        // Zero equals the exclusive minimum, so it is out of range. A negative value other than the
        // declared sentinel is reported too: the remote end resets on any negative, but the named
        // sentinel is the supported way to ask for that.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(SetViewportCommandParameters parameters)
                    {
                        SetViewportCommandParameters zero = new SetViewportCommandParameters { DevicePixelRatio = {|#0:0|} };
                        parameters.DevicePixelRatio = {|#1:-2|};
                    }
                }
            }
            """;

        DiagnosticResult zeroExpected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("0", "DevicePixelRatio", "(0, ∞)");

        DiagnosticResult negativeExpected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("-2", "DevicePixelRatio", "(0, ∞)");

        await VerifyDiagnosticsAsync(testCode, zeroExpected, negativeExpected);
    }

    [Fact]
    public async Task JsUintMediaFeatures_SentinelAndNonNegativeValues_NoDiagnostic()
    {
        // color, color-index, monochrome and the two viewport-segment counts are js-uint, so zero and
        // above are acceptable, as is each feature's own reset sentinel.
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        MediaFeatures sentinels = new MediaFeatures
                        {
                            Color = MediaFeatures.ResetColorValue,
                            ColorIndex = MediaFeatures.ResetColorIndexValue,
                            Monochrome = MediaFeatures.ResetMonochromeValue,
                            HorizontalViewportSegments = MediaFeatures.ResetHorizontalViewportSegmentsValue,
                            VerticalViewportSegments = MediaFeatures.ResetVerticalViewportSegmentsValue,
                        };
                        MediaFeatures values = new MediaFeatures
                        {
                            Color = 0,
                            ColorIndex = 256,
                            Monochrome = 8,
                            HorizontalViewportSegments = 2,
                            VerticalViewportSegments = 1,
                        };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task JsUintMediaFeatures_UndeclaredNegative_ReportsWarning()
    {
        // js-uint admits no negative value, and only the declared sentinel is exempt.
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(MediaFeatures features)
                    {
                        MediaFeatures below = new MediaFeatures { Color = {|#0:-2|} };
                        features.VerticalViewportSegments = {|#1:-3|};
                    }
                }
            }
            """;

        DiagnosticResult colorExpected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("-2", "Color", "[0, ∞)");

        DiagnosticResult segmentsExpected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("-3", "VerticalViewportSegments", "[0, ∞)");

        await VerifyDiagnosticsAsync(testCode, colorExpected, segmentsExpected);
    }

    [Fact]
    public async Task InclusiveMinimum_ValueBelowMinimum_ReportsWarning()
    {
        // ImageSize.MaxWidth is (js-uint .ge 1): the bound is inclusive, so 1 itself is in range and
        // only a value below it is reported. The exclusive-bound case is covered by the
        // DevicePixelRatio tests above, whose CDDL is (float .gt 0.0).
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ImageSize size = new ImageSize { MaxWidth = {|#0:0|} };
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("0", "MaxWidth", "[1, \u221E)");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task InclusiveMinimum_ValueAtOrAboveMinimum_NoDiagnostic()
    {
        // 1 is the smallest value the inclusive bound admits; larger values are equally fine.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ImageSize atMinimum = new ImageSize { MaxWidth = 1 };
                        ImageSize size = new ImageSize { MaxWidth = 2 };
                        ImageSize other = new ImageSize();
                        other.MaxHeight = 1024;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task PropertyWithoutSpecRange_NoDiagnostic()
    {
        // GeolocationCoordinates.Altitude is a double? property with no [SpecRange] attribute.
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        GeolocationCoordinates coordinates = new GeolocationCoordinates(0.0, 0.0) { Altitude = -100000.0 };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task NonWebDriverBiDiType_SameNamedProperty_NoDiagnostic()
    {
        // A user type with a same-named 'Quality' property carries no [SpecRange] attribute, so an
        // out-of-range constant is not reported.
        string testCode = """
            namespace TestApp
            {
                public class NotImageFormat
                {
                    public double Quality { get; set; }
                }

                public class TestClass
                {
                    public void TestMethod()
                    {
                        NotImageFormat format = new NotImageFormat { Quality = 1.5 };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task ConstantAssignedToNonProperty_NoDiagnostic()
    {
        // The analyzer examines only property assignments; a constant assigned to a field (even one
        // named like a ranged property) binds to a field symbol, not a property, and is never analyzed.
        string testCode = """
            namespace TestApp
            {
                public class TestClass
                {
                    private double quality;

                    public void TestMethod()
                    {
                        this.quality = 1.5;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task DirectAssignment_CharConstantAboveRange_ReportsWarning()
    {
        // A char constant is implicitly convertible to the double-typed Quality property. 'A' is 65,
        // well above the [0, 1] range, so it is reported. IConvertible.ToDouble throws for char, so the
        // analyzer converts a char constant directly.
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod(ImageFormat format)
                    {
                        format.Quality = {|#0:'A'|};
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("'A'", "Quality", "[0, 1]");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task Heading_EqualToExclusiveMaximum_ReportsWarning()
    {
        // Heading has range [0.0, 360.0) — the specification's CDDL range 0.0...360.0
        // excludes its upper bound, so 360.0 itself is invalid.
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        GeolocationCoordinates coordinates = new GeolocationCoordinates(0.0, 0.0) { Heading = {|#0:360.0|} };
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("360.0", "Heading", "[0, 360)");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task Heading_WithinExclusiveMaximum_NoDiagnostic()
    {
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        GeolocationCoordinates north = new GeolocationCoordinates(0.0, 0.0) { Heading = 0.0 };
                        GeolocationCoordinates almostNorth = new GeolocationCoordinates(0.0, 0.0) { Heading = 359.9 };
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }

    [Fact]
    public async Task LibraryNotReferenced_NoDiagnostic()
    {
        // Without the WebDriverBiDi assembly, the SpecRangeAttribute type is not found, so the analyzer
        // registers no per-assignment action and reports nothing.
        string testCode = """
            namespace TestApp
            {
                public class TestClass
                {
                    public double Quality { get; set; }

                    public void TestMethod()
                    {
                        this.Quality = 1.5;
                    }
                }
            }
            """;

        CSharpAnalyzerTest<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer, DefaultVerifier> testState = new()
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        await testState.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Tests that a non-numeric constant assigned to a ranged property is declined rather than
    /// converted. An analyzer also runs over code that does not compile, so the constant it is handed
    /// is not always of the property's type; converting a string would throw, which is reported as
    /// AD0001 and suppresses this rule for the whole file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StringConstant_AssignedToRangedProperty_ReportsNothing()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ImageFormat format = new ImageFormat { Quality = {|#0:"high"|} };
                    }
                }
            }
            """;

        await VerifyDiagnosticsAsync(
            testCode,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0).WithArguments("string", "double?"));
    }

    /// <summary>
    /// Tests that a boolean constant assigned to a ranged property is declined for the same reason a
    /// string is: it is the only other type a C# constant can have, and it does not convert either.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task BooleanConstant_AssignedToRangedProperty_ReportsNothing()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ImageFormat format = new ImageFormat { Quality = {|#0:true|} };
                    }
                }
            }
            """;

        await VerifyDiagnosticsAsync(
            testCode,
            DiagnosticResult.CompilerError("CS0029").WithLocation(0).WithArguments("bool", "double?"));
    }

    /// <summary>
    /// Tests that a char constant assigned to a ranged property is converted through its numeric value
    /// rather than through IConvertible, which throws for char. A char is the one constant type that
    /// converts implicitly to a numeric property, so this shape does compile.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CharConstant_OutsideRange_ReportsWarning()
    {
        string testCode = """
            using WebDriverBiDi.BrowsingContext;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        ImageFormat format = new ImageFormat { Quality = {|#0:'A'|} };
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("'A'", "Quality", "[0, 1]");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    private static async Task VerifyDiagnosticsAsync(string testCode, params DiagnosticResult[] expected)
    {
        RealAssemblyAnalyzerTest<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer> test = new()
        {
            TestCode = testCode,
        };
        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ConstructorArgument_OutOfRange_NotCheckedWhilePropertyAssignmentIs()
    {
        // The rule checks property assignments only. GeolocationCoordinates takes its ranged latitude and
        // longitude as constructor arguments, which are out of scope; the same value assigned to the property is
        // reported.
        string testCode = """
            using WebDriverBiDi.Emulation;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        GeolocationCoordinates fromConstructor = new GeolocationCoordinates(90.5, 180.5);
                        GeolocationCoordinates fromInitializer = new GeolocationCoordinates(0.0, 0.0) { Latitude = {|#0:90.5|} };
                    }
                }
            }
            """;

        DiagnosticResult expected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("90.5", "Latitude", "[-90, 90]");

        await VerifyDiagnosticsAsync(testCode, expected);
    }

    [Fact]
    public async Task IndexerAssignment_IsNotJudged_NoDiagnostic()
    {
        // The rule judges assignments to named properties. An element access on the left is rejected on
        // shape, before any symbol is bound, because a range is only ever declared on a named property.
        string testCode = """
            using System.Collections.Generic;
            using WebDriverBiDi.Script;

            namespace TestApp
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        Dictionary<string, int> values = new Dictionary<string, int>();
                        values["key"] = -1;
                    }
                }
            }
            """;

        await AnalyzerTestHelpers.VerifyAnalyzerAsync<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer>(testCode);
    }
}
