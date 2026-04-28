// <copyright file="BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects blocking operations in event handlers passed to AddObserver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI007";

    private const string Category = "Performance";

    private static readonly LocalizableString Title = "Avoid blocking operations in event handlers";

    private static readonly LocalizableString MessageFormat = "Blocking operation '{0}' detected in event handler. Consider using 'ObservableEventHandlerOptions.RunHandlerAsynchronously' or making the handler fully asynchronous.";

    private static readonly LocalizableString Description = "Blocking operations like Thread.Sleep(), Task.Wait(), or .Result in event handlers can cause deadlocks and performance issues. Use RunHandlerAsynchronously option for handlers with blocking operations, or refactor to be fully asynchronous.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Check if this is an AddObserver call
        if (memberAccess.Name.Identifier.Text != "AddObserver")
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        // Verify it's returning EventObserver<T>
        if (methodSymbol.ReturnType is not INamedTypeSymbol returnType || returnType.Name != "EventObserver")
        {
            return;
        }

        // Check if the handler options parameter includes RunHandlerAsynchronously
        if (HasRunHandlerAsynchronouslyOption(context, invocation))
        {
            // Handler is configured to run asynchronously, no need to check for blocking operations
            return;
        }

        // Find the handler lambda/method passed to AddObserver
        ArgumentSyntax? handlerArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (handlerArgument == null)
        {
            return;
        }

        // Analyze the handler for blocking operations
        SyntaxNode? handlerBody = GetHandlerBody(context, handlerArgument.Expression);
        if (handlerBody == null)
        {
            return;
        }

        // Look for blocking operations
        IEnumerable<SyntaxNode> blockingOperations = FindBlockingOperations(context, handlerBody);
        foreach (SyntaxNode blockingOp in blockingOperations)
        {
            string operationName = GetBlockingOperationName(blockingOp);
            Diagnostic diagnostic = Diagnostic.Create(Rule, blockingOp.GetLocation(), operationName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasRunHandlerAsynchronouslyOption(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation)
    {
        // Check if there's a second or third argument that's the options parameter
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            ITypeSymbol? argType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
            if (argType?.Name == "ObservableEventHandlerOptions")
            {
                // Check if the argument contains RunHandlerAsynchronously
                string argumentText = argument.Expression.ToString();
                if (argumentText.Contains("RunHandlerAsynchronously"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static SyntaxNode? GetHandlerBody(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        return expression switch
        {
            // Lambda expression: args => { ... }
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Body,
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.Body,

            // Method reference: resolve the method and get its body
            IdentifierNameSyntax identifierName => GetMethodBodyFromSymbol(context, identifierName),
            MemberAccessExpressionSyntax memberAccess => GetMethodBodyFromSymbol(context, memberAccess),

            _ => null,
        };
    }

    private static SyntaxNode? GetMethodBodyFromSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
        if (symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        // Get the method declaration from the symbol
        SyntaxReference? syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return null;
        }

        SyntaxNode methodDeclaration = syntaxReference.GetSyntax();

        // Extract the body based on the method declaration type
        return methodDeclaration switch
        {
            MethodDeclarationSyntax methodDecl => methodDecl.Body ?? (SyntaxNode?)methodDecl.ExpressionBody?.Expression,
            LocalFunctionStatementSyntax localFunc => localFunc.Body ?? (SyntaxNode?)localFunc.ExpressionBody?.Expression,
            _ => null,
        };
    }

    private static IEnumerable<SyntaxNode> FindBlockingOperations(
        SyntaxNodeAnalysisContext context,
        SyntaxNode handlerBody)
    {
        List<SyntaxNode> blockingOps = [];

        // Find all invocations and member accesses in the handler
        IEnumerable<InvocationExpressionSyntax> invocations = handlerBody.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            // Check for Thread.Sleep
            if (methodSymbol.ContainingType?.Name == "Thread" && methodSymbol.Name == "Sleep")
            {
                blockingOps.Add(invocation);
                continue;
            }

            // Check for Task.Wait()
            if (methodSymbol.ContainingType?.Name == "Task" && methodSymbol.Name == "Wait")
            {
                blockingOps.Add(invocation);
                continue;
            }

            // Check for GetAwaiter().GetResult()
            if (methodSymbol.Name == "GetResult" &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InvocationExpressionSyntax getAwaiterCall)
            {
                IMethodSymbol? getAwaiterSymbol = context.SemanticModel.GetSymbolInfo(getAwaiterCall).Symbol as IMethodSymbol;
                if (getAwaiterSymbol?.Name == "GetAwaiter")
                {
                    blockingOps.Add(invocation);
                    continue;
                }
            }
        }

        // Check for .Result property access on Task
        IEnumerable<MemberAccessExpressionSyntax> memberAccesses = handlerBody.DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>();

        foreach (MemberAccessExpressionSyntax memberAccess in memberAccesses)
        {
            if (memberAccess.Name.Identifier.Text == "Result")
            {
                ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (expressionType?.Name == "Task")
                {
                    blockingOps.Add(memberAccess);
                }
            }
        }

        return blockingOps;
    }

    private static string GetBlockingOperationName(SyntaxNode blockingOperation)
    {
        return blockingOperation switch
        {
            InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text + "()",
            MemberAccessExpressionSyntax memberAccessExpr => memberAccessExpr.Name.Identifier.Text,
            _ => blockingOperation.ToString(),
        };
    }
}
