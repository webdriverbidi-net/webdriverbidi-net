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

    private static readonly LocalizableString SynchronousBodyMessageFormat = "Blocking operation '{0}' detected in event handler. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' does not offload the synchronous body of a Task-returning handler; make the handler 'async' and await before the blocking work, or move the work into Task.Run.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    // Same ID, category, and severity as Rule (release tracking is unchanged); used when the
    // RunHandlerAsynchronously option is present but the handler is a non-async Task-returning
    // delegate, so the option cannot help; the code fix converts the handler to async instead.
    private static readonly DiagnosticDescriptor SynchronousBodyRule = new(
        DiagnosticId,
        Title,
        SynchronousBodyMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, SynchronousBodyRule];

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

        if (((INamedTypeSymbol)methodSymbol.ReturnType).Name != "EventObserver")
        {
            return;
        }

        // The option only helps when the handler actually runs off the dispatching thread:
        // an Action<T> handler (queued to the thread pool by the library), an async lambda, or
        // an async method group. A non-async Task-returning handler still executes its body
        // inline, so blocking calls in it are reported with a message that says so.
        bool optionPresent = AnalyzerSymbolHelpers.HasRunHandlerAsynchronouslyOption(context, invocation);
        if (optionPresent && AnalyzerSymbolHelpers.IsHandlerAsynchronous(context, invocation, methodSymbol))
        {
            return;
        }

        ArgumentSyntax? handlerArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (handlerArgument == null)
        {
            return;
        }

        SyntaxNode? handlerBody = AnalyzerSymbolHelpers.GetHandlerBody(context, handlerArgument.Expression);
        if (handlerBody == null)
        {
            return;
        }

        DiagnosticDescriptor rule = optionPresent ? SynchronousBodyRule : Rule;
        IEnumerable<SyntaxNode> blockingOperations = FindBlockingOperations(context, handlerBody);
        foreach (SyntaxNode blockingOp in blockingOperations)
        {
            string operationName = GetBlockingOperationName(blockingOp);
            Diagnostic diagnostic = Diagnostic.Create(rule, blockingOp.GetLocation(), operationName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<SyntaxNode> FindBlockingOperations(
        SyntaxNodeAnalysisContext context,
        SyntaxNode handlerBody)
    {
        List<SyntaxNode> blockingOps = [];

        IEnumerable<InvocationExpressionSyntax> invocations = handlerBody.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            if (methodSymbol.ContainingType.Name == "Thread" && methodSymbol.Name == "Sleep")
            {
                blockingOps.Add(invocation);
                continue;
            }

            if (methodSymbol.ContainingType.Name == "Task" && methodSymbol.Name == "Wait")
            {
                blockingOps.Add(invocation);
                continue;
            }

            if (methodSymbol.Name == "GetResult" &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InvocationExpressionSyntax getAwaiterCall)
            {
                IMethodSymbol? getAwaiterSymbol = context.SemanticModel.GetSymbolInfo(getAwaiterCall).Symbol as IMethodSymbol;
                if (getAwaiterSymbol is { Name: "GetAwaiter" })
                {
                    blockingOps.Add(invocation);
                    continue;
                }
            }
        }

        IEnumerable<MemberAccessExpressionSyntax> memberAccesses = handlerBody.DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>();

        foreach (MemberAccessExpressionSyntax memberAccess in memberAccesses)
        {
            if (memberAccess.Name.Identifier.Text == "Result")
            {
                ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (expressionType is { Name: "Task" })
                {
                    blockingOps.Add(memberAccess);
                }
            }
        }

        return blockingOps;
    }

    private static string GetBlockingOperationName(SyntaxNode blockingOperation)
    {
        if (blockingOperation is InvocationExpressionSyntax invocation && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.Text + "()";
        }

        return ((MemberAccessExpressionSyntax)blockingOperation).Name.Identifier.Text;
    }
}
