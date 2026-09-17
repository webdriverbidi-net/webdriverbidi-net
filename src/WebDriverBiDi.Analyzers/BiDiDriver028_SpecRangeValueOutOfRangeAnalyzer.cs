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

        if (!TryGetSpecRange(property, specRangeAttributeSymbol, out double minimum, out double maximum, out bool minimumExclusive, out bool maximumExclusive, out bool hasSentinel, out double sentinelValue))
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

        if (!TryConvertToDouble(constant.Value, out double value))
        {
            return;
        }

        // The reset sentinel deliberately falls outside the range and is valid.
        if (hasSentinel && value == sentinelValue)
        {
            return;
        }

        if (value < minimum || value > maximum || (minimumExclusive && value == minimum) || (maximumExclusive && value == maximum))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                assignment.Right.GetLocation(),
                assignment.Right.ToString(),
                property.Name,
                FormatRange(minimum, maximum, minimumExclusive, maximumExclusive));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool TryGetSpecRange(IPropertySymbol property, INamedTypeSymbol specRangeAttributeSymbol, out double minimum, out double maximum, out bool minimumExclusive, out bool maximumExclusive, out bool hasSentinel, out double sentinelValue)
    {
        minimum = double.NegativeInfinity;
        maximum = double.PositiveInfinity;
        minimumExclusive = false;
        maximumExclusive = false;
        hasSentinel = false;
        sentinelValue = 0.0;

        foreach (AttributeData attribute in property.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, specRangeAttributeSymbol))
            {
                continue;
            }

            // SpecRangeAttribute's only constructor is (double minimum, double maximum), but an analyzer also runs
            // over code that does not compile. An application with missing or mistyped arguments ([SpecRange], or
            // [SpecRange("low", "high")]) binds no constructor and carries no positional values, so there is no
            // range to judge against. Reading the values it lacks would throw, which is reported as AD0001 and
            // suppresses this rule for the whole file.
            if (attribute.ConstructorArguments.Length != 2
                || attribute.ConstructorArguments[0].Value is not double declaredMinimum
                || attribute.ConstructorArguments[1].Value is not double declaredMaximum)
            {
                return false;
            }

            minimum = declaredMinimum;
            maximum = declaredMaximum;

            // MinimumExclusive, MaximumExclusive, HasSentinel, and SentinelValue are independent
            // optional named arguments; a property may set any combination of them, so each is read
            // with its own separate check. A named argument of the wrong type, again possible only in code
            // that does not compile, carries no value and leaves that setting at its default.
            foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
            {
                object? namedValue = namedArgument.Value.Value;
                if (namedArgument.Key == "MinimumExclusive" && namedValue is bool declaredMinimumExclusive)
                {
                    minimumExclusive = declaredMinimumExclusive;
                }

                if (namedArgument.Key == "MaximumExclusive" && namedValue is bool declaredMaximumExclusive)
                {
                    maximumExclusive = declaredMaximumExclusive;
                }

                if (namedArgument.Key == "HasSentinel" && namedValue is bool declaredHasSentinel)
                {
                    hasSentinel = declaredHasSentinel;
                }

                if (namedArgument.Key == "SentinelValue" && namedValue is double declaredSentinelValue)
                {
                    sentinelValue = declaredSentinelValue;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts a compile-time constant to a double, reporting whether it is numeric at all.
    /// </summary>
    /// <param name="value">The constant value assigned to the property.</param>
    /// <param name="numericValue">When this method returns <see langword="true"/>, the value as a double.</param>
    /// <returns><see langword="true"/> if the constant is numeric; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// In code that compiles, the left side is always a numeric-typed property, so the constant is a
    /// numeric type or a char, which converts implicitly to it. An analyzer also runs over code that
    /// does not compile, which is most of what it sees while a developer is typing, and a constant of
    /// any other type has to be declined rather than converted: the only remaining possibilities for a
    /// C# constant are a bool and a string, and <see cref="IConvertible.ToDouble"/> throws for both.
    /// An exception raised here is reported as AD0001 and suppresses this rule for the whole file.
    /// </remarks>
    private static bool TryConvertToDouble(object value, out double numericValue)
    {
        // Every numeric boxed value implements IConvertible.ToDouble, but IConvertible.ToDouble throws
        // for char, so char is converted directly.
        if (value is char charValue)
        {
            numericValue = charValue;
            return true;
        }

        // The numeric type codes are contiguous, running from SByte to Decimal, with Boolean and Char
        // below them and DateTime and String above. Testing the range rather than listing the eleven
        // numeric types keeps this to a single decision.
        TypeCode typeCode = Convert.GetTypeCode(value);
        if (typeCode is < TypeCode.SByte or > TypeCode.Decimal)
        {
            numericValue = 0.0;
            return false;
        }

        numericValue = ((IConvertible)value).ToDouble(CultureInfo.InvariantCulture);
        return true;
    }

    private static string FormatRange(double minimum, double maximum, bool minimumExclusive, bool maximumExclusive)
    {
        // Exclusive bounds render in interval notation with a parenthesis on that side: [0, 360) for
        // the specification's CDDL range 0.0...360.0, and (1, ∞] for a member declared js-uint .gt 1.
        string openingDelimiter = minimumExclusive ? "(" : "[";
        string closingDelimiter = maximumExclusive ? ")" : "]";
        return $"{openingDelimiter}{FormatBound(minimum)}, {FormatBound(maximum)}{closingDelimiter}";
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
