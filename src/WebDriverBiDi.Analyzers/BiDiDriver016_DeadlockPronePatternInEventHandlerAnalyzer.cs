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

        if (memberAccess.Name.Identifier.Text != "AddObserver")
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        if (methodSymbol.ReturnType is not INamedTypeSymbol returnType || returnType.Name != "EventObserver")
        {
            return;
        }

        if (AnalyzerSymbolHelpers.HasRunHandlerAsynchronouslyOption(context, invocation))
        {
            return;
        }

        ArgumentSyntax? handlerArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (handlerArgument == null)
        {
            return;
        }

        bool isAsyncHandler = IsAsyncHandler(handlerArgument.Expression);
        if (!isAsyncHandler)
        {
            return;
        }

        SyntaxNode? handlerBody = AnalyzerSymbolHelpers.GetHandlerBody(context, handlerArgument.Expression);
        if (handlerBody == null)
        {
            return;
        }

        IEnumerable<(SyntaxNode Node, string Pattern)> deadlockPatterns = FindDeadlockPronePatterns(context, handlerBody);
        foreach ((SyntaxNode node, string pattern) in deadlockPatterns)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, node.GetLocation(), pattern);
            context.ReportDiagnostic(diagnostic);
        }
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

    private static IEnumerable<(SyntaxNode Node, string Pattern)> FindDeadlockPronePatterns(
        SyntaxNodeAnalysisContext context,
        SyntaxNode handlerBody)
    {
        List<(SyntaxNode, string)> patterns = [];

        IEnumerable<LockStatementSyntax> lockStatements = handlerBody.DescendantNodes()
            .OfType<LockStatementSyntax>();

        foreach (LockStatementSyntax lockStmt in lockStatements)
        {
            patterns.Add((lockStmt, "lock statement"));
        }

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

            if (containingTypeName == "Monitor" && (methodName == "Enter" || methodName == "TryEnter"))
            {
                patterns.Add((invocation, $"Monitor.{methodName}"));
                continue;
            }

            if ((containingTypeName == "Semaphore" || containingTypeName == "SemaphoreSlim") && (methodName == "Wait" || methodName == "WaitOne"))
            {
                patterns.Add((invocation, $"{containingTypeName}.{methodName}"));
                continue;
            }

            if ((containingTypeName == "ManualResetEvent" ||
                 containingTypeName == "ManualResetEventSlim" ||
                 containingTypeName == "AutoResetEvent" ||
                 containingTypeName == "WaitHandle") &&
                methodName == "WaitOne")
            {
                patterns.Add((invocation, $"{containingTypeName}.{methodName}"));
                continue;
            }

            if (containingTypeName == "SynchronizationContext" && methodName == "Send")
            {
                patterns.Add((invocation, "SynchronizationContext.Send"));
                continue;
            }

            if (containingTypeName == "Task" && (methodName == "WaitAll" || methodName == "WaitAny"))
            {
                patterns.Add((invocation, $"Task.{methodName}"));
                continue;
            }

            if (containingTypeName == "Mutex" && methodName == "WaitOne")
            {
                patterns.Add((invocation, "Mutex.WaitOne"));
                continue;
            }
        }

        return patterns;
    }
}
