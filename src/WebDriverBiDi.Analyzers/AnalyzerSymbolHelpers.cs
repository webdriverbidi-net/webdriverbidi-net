// <copyright file="AnalyzerSymbolHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Helper methods for identifying WebDriver BiDi driver-related symbols.
/// </summary>
internal static class AnalyzerSymbolHelpers
{
    /// <summary>
    /// Determines whether the symbol represents a command executor capability.
    /// </summary>
    /// <param name="type">The symbol to inspect.</param>
    /// <returns><see langword="true"/> if the symbol represents a command executor capability; otherwise <see langword="false"/>.</returns>
    internal static bool IsCommandExecutorType(ITypeSymbol? type)
    {
        return HasTypeOrBaseOrInterface(type, "BiDiDriver", "IBiDiCommandExecutor");
    }

    /// <summary>
    /// Determines whether the symbol represents a driver configuration capability.
    /// </summary>
    /// <param name="type">The symbol to inspect.</param>
    /// <returns><see langword="true"/> if the symbol represents a driver configuration capability; otherwise <see langword="false"/>.</returns>
    internal static bool IsDriverConfigurationType(ITypeSymbol? type)
    {
        return HasTypeOrBaseOrInterface(type, "BiDiDriver", "IBiDiDriverConfiguration");
    }

    /// <summary>
    /// Determines whether the given AddObserver invocation has the RunHandlerAsynchronously option.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="invocation">The AddObserver invocation to inspect.</param>
    /// <returns><see langword="true"/> if the RunHandlerAsynchronously option is present; otherwise <see langword="false"/>.</returns>
    internal static bool HasRunHandlerAsynchronouslyOption(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            ITypeSymbol? argType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
            if (argType?.Name == "ObservableEventHandlerOptions" && argument.Expression.ToString().Contains("RunHandlerAsynchronously"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the body syntax node for a handler expression passed to AddObserver.
    /// Returns the lambda body, or resolves a method reference to its body.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="expression">The handler expression.</param>
    /// <returns>The body syntax node, or <see langword="null"/> if it cannot be resolved.</returns>
    internal static SyntaxNode? GetHandlerBody(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        return expression switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Body,
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.Body,
            IdentifierNameSyntax identifierName => GetMethodBodyFromSymbol(context, identifierName),
            MemberAccessExpressionSyntax memberAccess => GetMethodBodyFromSymbol(context, memberAccess),
            _ => null,
        };
    }

    /// <summary>
    /// Determines whether a Module type has <c>Module</c> anywhere in its base-type chain.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> if the type inherits from <c>Module</c>; otherwise <see langword="false"/>.</returns>
    internal static bool IsModuleSubclass(INamedTypeSymbol? type)
    {
        if (type == null)
        {
            return false;
        }

        INamedTypeSymbol? current = type.BaseType;
        while (current != null)
        {
            if (current.Name == "Module")
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static SyntaxNode? GetMethodBodyFromSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
        if (symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        SyntaxReference? syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return null;
        }

        SyntaxNode methodDeclaration = syntaxReference.GetSyntax();
        return methodDeclaration switch
        {
            MethodDeclarationSyntax methodDecl => methodDecl.Body ?? (SyntaxNode?)methodDecl.ExpressionBody?.Expression,
            LocalFunctionStatementSyntax localFunc => localFunc.Body ?? (SyntaxNode?)localFunc.ExpressionBody?.Expression,
            _ => null,
        };
    }

    private static bool HasTypeOrBaseOrInterface(ITypeSymbol? type, params string[] typeNames)
    {
        for (ITypeSymbol? current = type; current != null; current = current.BaseType)
        {
            if (typeNames.Contains(current.Name))
            {
                return true;
            }

            if (current is INamedTypeSymbol namedType && namedType.AllInterfaces.Any(interfaceType => typeNames.Contains(interfaceType.Name)))
            {
                return true;
            }
        }

        return false;
    }
}
