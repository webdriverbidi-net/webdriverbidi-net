// <copyright file="BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that flags a compile-time constant assigned to a command-parameter property whose value
/// is outside the WebDriver BiDi specification range declared by
/// <c>WebDriverBiDi.SpecRangeAttribute</c>. A value the specification places outside the range is
/// representable on the wire, so the library does not validate it at run time and a conforming remote
/// end rejects it when the command is executed. Only compile-time constants are examined; runtime and
/// dynamic values are never reported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver028_SpecRangeValueOutOfRangeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI028";

    private const string Category = "Usage";

    private const string SpecRangeAttributeFullName = "WebDriverBiDi.SpecRangeAttribute";

    private static readonly LocalizableString Title = "Constant value outside the specification range";

    private static readonly LocalizableString MessageFormat = "The constant value {0} assigned to '{1}' is outside the specification range {2}. A conforming remote end will reject it.";

    private static readonly LocalizableString Description = "Flags a compile-time constant assigned to a command-parameter property whose WebDriver BiDi specification range is declared by SpecRangeAttribute. The library deliberately does not validate these ranges at run time, so this provides compile-time feedback for an obviously out-of-range constant. A range's upper bound may be declared exclusive, in which case a constant equal to it is flagged. A property's declared reset sentinel value is treated as valid, and runtime or dynamic values are never flagged.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi028");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationStart =>
        {
            // The range metadata lives on WebDriverBiDi.SpecRangeAttribute. If the library is not
            // referenced, that attribute type is not found, no property can carry the attribute, and
            // there is nothing to analyze.
            INamedTypeSymbol? specRangeAttributeSymbol = compilationStart.Compilation.GetTypeByMetadataName(SpecRangeAttributeFullName);
            if (specRangeAttributeSymbol is null)
            {
                return;
            }

            // A simple assignment covers both `x.Prop = <constant>` and the member assignments in an
            // object initializer (`new T { Prop = <constant> }`).
            compilationStart.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeAssignment(nodeContext, specRangeAttributeSymbol),
                SyntaxKind.SimpleAssignmentExpression);
        });
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context, INamedTypeSymbol specRangeAttributeSymbol)
    {
        AssignmentExpressionSyntax assignment = (AssignmentExpressionSyntax)context.Node;

        // The left side must bind to a property; constructor-parameter arguments and fields are out of
        // scope.
        if (context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol is not IPropertySymbol property)
        {
            return;
        }

        if (!TryGetSpecRange(property, specRangeAttributeSymbol, out double minimum, out double maximum, out bool maximumExclusive, out bool hasSentinel, out double sentinelValue))
        {
            return;
        }

        // Only compile-time constants are ever flagged. A missing value (runtime/dynamic expression) or
        // a null value must never be reported.
        Optional<object?> constant = context.SemanticModel.GetConstantValue(assignment.Right);
        if (!constant.HasValue || constant.Value is null)
        {
            return;
        }

        double value = ConvertToDouble(constant.Value);

        // The reset sentinel deliberately falls outside the range and is valid.
        if (hasSentinel && value == sentinelValue)
        {
            return;
        }

        if (value < minimum || value > maximum || (maximumExclusive && value == maximum))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                assignment.Right.GetLocation(),
                assignment.Right.ToString(),
                property.Name,
                FormatRange(minimum, maximum, maximumExclusive));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool TryGetSpecRange(IPropertySymbol property, INamedTypeSymbol specRangeAttributeSymbol, out double minimum, out double maximum, out bool maximumExclusive, out bool hasSentinel, out double sentinelValue)
    {
        minimum = double.NegativeInfinity;
        maximum = double.PositiveInfinity;
        maximumExclusive = false;
        hasSentinel = false;
        sentinelValue = 0.0;

        foreach (AttributeData attribute in property.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, specRangeAttributeSymbol))
            {
                continue;
            }

            // SpecRangeAttribute's only constructor is (double minimum, double maximum), so the two
            // positional arguments are always present and always doubles.
            minimum = (double)attribute.ConstructorArguments[0].Value!;
            maximum = (double)attribute.ConstructorArguments[1].Value!;

            // MaximumExclusive, HasSentinel, and SentinelValue are independent optional named
            // arguments; a property may set any combination of them, so each is read with its own
            // separate check.
            foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key == "MaximumExclusive")
                {
                    maximumExclusive = (bool)namedArgument.Value.Value!;
                }

                if (namedArgument.Key == "HasSentinel")
                {
                    hasSentinel = (bool)namedArgument.Value.Value!;
                }

                if (namedArgument.Key == "SentinelValue")
                {
                    sentinelValue = (double)namedArgument.Value.Value!;
                }
            }

            return true;
        }

        return false;
    }

    private static double ConvertToDouble(object value)
    {
        // The left side is always a numeric-typed property, so the assigned constant is either a
        // numeric type or a char (which is implicitly convertible to the numeric property type).
        // Every numeric boxed value implements IConvertible.ToDouble, but IConvertible.ToDouble throws
        // for char, so char is converted directly.
        if (value is char charValue)
        {
            return charValue;
        }

        return ((IConvertible)value).ToDouble(CultureInfo.InvariantCulture);
    }

    private static string FormatRange(double minimum, double maximum, bool maximumExclusive)
    {
        // An exclusive upper bound renders in interval notation with a closing parenthesis,
        // for example [0, 360) for the specification's CDDL range 0.0...360.0.
        string closingDelimiter = maximumExclusive ? ")" : "]";
        return $"[{FormatBound(minimum)}, {FormatBound(maximum)}{closingDelimiter}";
    }

    private static string FormatBound(double bound)
    {
        // Every ranged property has a finite lower bound; only some upper bounds are positive infinity.
        if (double.IsPositiveInfinity(bound))
        {
            return "∞";
        }

        return bound.ToString(CultureInfo.InvariantCulture);
    }
}
