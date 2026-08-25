// <copyright file="AnalyzerSymbolHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// Determines whether the handler passed to an <c>AddObserver</c> invocation will actually
    /// execute off the dispatching thread when <c>RunHandlerAsynchronously</c> is specified.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="invocation">The AddObserver invocation to inspect.</param>
    /// <param name="addObserverMethod">The resolved AddObserver overload.</param>
    /// <returns><see langword="true"/> if the handler is asynchronous; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The option only affects what happens with the <c>Task</c> a handler returns; the code that
    /// runs before the handler returns still executes on the dispatching thread. A handler is
    /// therefore considered asynchronous when it is bound to the <c>Action&lt;T&gt;</c> overload
    /// (the library queues the whole action to the thread pool in that case), when it is an
    /// <c>async</c> lambda or anonymous method, or when it is a method group that resolves to an
    /// <c>async</c> method. A non-<c>async</c> <c>Task</c>-returning handler is not offloaded.
    /// Callers only invoke this when an options argument is present, so the invocation always has
    /// at least one argument and the resolved overload at least one parameter.
    /// </remarks>
    internal static bool IsHandlerAsynchronous(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol addObserverMethod)
    {
        if (addObserverMethod.Parameters[0].Type.Name == "Action")
        {
            return true;
        }

        ExpressionSyntax handler = invocation.ArgumentList.Arguments[0].Expression;
        return handler switch
        {
            AnonymousFunctionExpressionSyntax anonymousFunction => anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword),
            IdentifierNameSyntax or MemberAccessExpressionSyntax => context.SemanticModel.GetSymbolInfo(handler).Symbol is IMethodSymbol { IsAsync: true },
            _ => false,
        };
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
        INamedTypeSymbol? current = type!.BaseType;
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
        return methodDeclaration is MethodDeclarationSyntax methodDecl
            ? methodDecl.Body ?? (SyntaxNode?)methodDecl.ExpressionBody?.Expression
            : ((LocalFunctionStatementSyntax)methodDeclaration).Body ?? (SyntaxNode?)((LocalFunctionStatementSyntax)methodDeclaration).ExpressionBody?.Expression;
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
