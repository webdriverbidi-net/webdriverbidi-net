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
public class BiDiDriver028SpecRangeValueOutOfRangeAnalyzerTests
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
            .WithArguments("1.5", "Quality", "0", "1");

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
            .WithArguments("-0.1", "Quality", "0", "1");

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
            .WithArguments("2", "Grid", "0", "1");

        DiagnosticResult belowExpected = new DiagnosticResult(
            BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.DiagnosticId,
            DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithArguments("-2", "Grid", "0", "1");

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
            .WithArguments("-5", "MaxDomDepth", "0", "∞");

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
            .WithArguments("-1", "Left", "0", "∞");

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
            .WithArguments("'A'", "Quality", "0", "1");

        await VerifyDiagnosticsAsync(testCode, expected);
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

    private static async Task VerifyDiagnosticsAsync(string testCode, params DiagnosticResult[] expected)
    {
        RealAssemblyAnalyzerTest<BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer> test = new()
        {
            TestCode = testCode,
        };
        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
