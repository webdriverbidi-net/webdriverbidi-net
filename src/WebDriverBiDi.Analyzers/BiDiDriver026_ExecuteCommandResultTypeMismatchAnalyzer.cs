// <copyright file="BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects an explicit <c>ExecuteCommandAsync&lt;T&gt;</c> type argument that disagrees
/// with the command's declared result type. Such a call binds to the overload taking the
/// non-generic <c>CommandParameters</c> parameter type, compiles, and then throws
/// <c>WebDriverBiDiException</c> at runtime because the response cannot be converted to <c>T</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI026";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "ExecuteCommandAsync type argument does not match the command result type";

    private static readonly LocalizableString MessageFormat = "The type argument '{0}' does not match the command's result type '{1}'. This call binds to the ExecuteCommandAsync overload taking the non-generic CommandParameters parameter type and throws WebDriverBiDiException at runtime because the response deserializes as '{1}', which cannot be converted to '{0}'. Use ExecuteCommandAsync<{1}> or let the type argument be inferred.";

    private static readonly LocalizableString Description = "ExecuteCommandAsync has an overload taking CommandParameters<T> and an overload taking the non-generic CommandParameters; both are generic methods. When the explicit type argument does not match the result type of the supplied parameters object, the first overload does not apply and the call binds to the one taking the non-generic parameter type. It compiles, but at runtime the response is deserialized as the command's real result type and cannot be cast to the requested type, so ExecuteCommandAsync throws a WebDriverBiDiException. Match the type argument to the command's result type, or omit it and let it be inferred.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi026");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        // Only calls with an explicit type argument (a GenericNameSyntax member name) can mismatch;
        // without one the type is inferred from the parameters and always matches.
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName })
        {
            return;
        }

        if (genericName.Identifier.Text != "ExecuteCommandAsync")
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        // Only the library's ExecuteCommandAsync is of interest, not a same-named method on another type.
        if (!AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            return;
        }

        ITypeSymbol expectedResultType = methodSymbol.TypeArguments[0];

        ITypeSymbol? actualResultType = FindCommandResultType(context, invocation);
        if (actualResultType == null)
        {
            return;
        }

        // When either type is an open type parameter (for example inside a generic helper method), the
        // real types are not known at analysis time, so the outcome cannot be determined statically.
        if (expectedResultType is ITypeParameterSymbol || actualResultType is ITypeParameterSymbol)
        {
            return;
        }

        // Safe when an instance of the command's result type can be used as the requested type: the
        // requested type is the result type itself or one of its base types (result is T succeeds).
        if (IsAssignableTo(actualResultType, expectedResultType))
        {
            return;
        }

        TypeSyntax typeArgument = genericName.TypeArgumentList.Arguments[0];

        // Carry the correct result type (minimally qualified at the type-argument position, so it is
        // valid where the code fix substitutes it) to the code fix provider, which rewrites the
        // argument without having to recompute it.
        ImmutableDictionary<string, string?> properties = ImmutableDictionary<string, string?>.Empty
            .Add("ResultType", actualResultType.ToMinimalDisplayString(context.SemanticModel, typeArgument.SpanStart));

        Diagnostic diagnostic = Diagnostic.Create(
            Rule,
            typeArgument.GetLocation(),
            properties,
            expectedResultType.Name,
            actualResultType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static ITypeSymbol? FindCommandResultType(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        // Locate the argument bound to the CommandParameters parameter by shape rather than position,
        // so a named or reordered argument list is handled correctly.
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            ITypeSymbol? argumentType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
            if (GetCommandResultType(argumentType) is { } resultType)
            {
                return resultType;
            }
        }

        return null;
    }

    private static ITypeSymbol? GetCommandResultType(ITypeSymbol? type)
    {
        for (ITypeSymbol? current = type; current != null; current = current.BaseType)
        {
            if (current is INamedTypeSymbol { Name: "CommandParameters", IsGenericType: true } named)
            {
                return named.TypeArguments[0];
            }
        }

        return null;
    }

    private static bool IsAssignableTo(ITypeSymbol source, ITypeSymbol target)
    {
        for (ITypeSymbol? current = source; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, target))
            {
                return true;
            }
        }

        return false;
    }
}
