// <copyright file="BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects deadlock-prone synchronization patterns in event handlers passed to AddObserver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI016";

    private const string Category = "Reliability";

    private static readonly LocalizableString Title = "Avoid deadlock-prone synchronization patterns in event handlers";

    private static readonly LocalizableString MessageFormat = "Deadlock-prone pattern '{0}' detected in event handler. This can cause deadlocks in async contexts. Consider using async alternatives or ObservableEventHandlerOptions.RunHandlerAsynchronously.";

    private static readonly LocalizableString Description = "Synchronization primitives like lock statements, Monitor.Enter, Semaphore.Wait(), WaitHandle.WaitOne(), or SynchronizationContext.Send() in async event handlers can cause deadlocks. Use async alternatives (SemaphoreSlim.WaitAsync, async/await patterns) or configure the handler to run asynchronously with RunHandlerAsynchronously option.";

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
            // Handler is configured to run asynchronously, deadlock patterns are less of a concern
            return;
        }

        // Find the handler lambda/method passed to AddObserver
        ArgumentSyntax? handlerArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (handlerArgument == null)
        {
            return;
        }

        // Check if handler is async
        bool isAsyncHandler = IsAsyncHandler(handlerArgument.Expression);
        if (!isAsyncHandler)
        {
            // Non-async handlers are less prone to these specific deadlock patterns
            return;
        }

        // Analyze the handler for deadlock-prone patterns
        SyntaxNode? handlerBody = GetHandlerBody(context, handlerArgument.Expression);
        if (handlerBody == null)
        {
            return;
        }

        // Look for deadlock-prone patterns
        IEnumerable<(SyntaxNode Node, string Pattern)> deadlockPatterns = FindDeadlockPronePatterns(context, handlerBody);
        foreach ((SyntaxNode node, string pattern) in deadlockPatterns)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, node.GetLocation(), pattern);
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

    private static bool IsAsyncHandler(ExpressionSyntax expression)
    {
        return expression switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword),
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword),
            _ => false,
        };
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

    private static IEnumerable<(SyntaxNode Node, string Pattern)> FindDeadlockPronePatterns(
        SyntaxNodeAnalysisContext context,
        SyntaxNode handlerBody)
    {
        List<(SyntaxNode, string)> patterns = [];

        // Check for lock statements
        IEnumerable<LockStatementSyntax> lockStatements = handlerBody.DescendantNodes()
            .OfType<LockStatementSyntax>();

        foreach (LockStatementSyntax lockStmt in lockStatements)
        {
            patterns.Add((lockStmt, "lock statement"));
        }

        // Find all invocations in the handler
        IEnumerable<InvocationExpressionSyntax> invocations = handlerBody.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            string? containingTypeName = methodSymbol.ContainingType?.Name;
            string methodName = methodSymbol.Name;

            // Check for Monitor.Enter/TryEnter
            if (containingTypeName == "Monitor" && (methodName == "Enter" || methodName == "TryEnter"))
            {
                patterns.Add((invocation, $"Monitor.{methodName}"));
                continue;
            }

            // Check for Semaphore.WaitOne or Semaphore.Wait
            if ((containingTypeName == "Semaphore" || containingTypeName == "SemaphoreSlim") && (methodName == "Wait" || methodName == "WaitOne"))
            {
                patterns.Add((invocation, $"{containingTypeName}.{methodName}"));
                continue;
            }

            // Check for ManualResetEvent/AutoResetEvent/WaitHandle.WaitOne
            if ((containingTypeName == "ManualResetEvent" ||
                 containingTypeName == "ManualResetEventSlim" ||
                 containingTypeName == "AutoResetEvent" ||
                 containingTypeName == "WaitHandle") &&
                methodName == "WaitOne")
            {
                patterns.Add((invocation, $"{containingTypeName}.{methodName}"));
                continue;
            }

            // Check for SynchronizationContext.Send
            if (containingTypeName == "SynchronizationContext" && methodName == "Send")
            {
                patterns.Add((invocation, "SynchronizationContext.Send"));
                continue;
            }

            // Check for Task.WaitAll/WaitAny
            if (containingTypeName == "Task" && (methodName == "WaitAll" || methodName == "WaitAny"))
            {
                patterns.Add((invocation, $"Task.{methodName}"));
                continue;
            }

            // Check for Mutex.WaitOne
            if (containingTypeName == "Mutex" && methodName == "WaitOne")
            {
                patterns.Add((invocation, "Mutex.WaitOne"));
                continue;
            }
        }

        return patterns;
    }
}
