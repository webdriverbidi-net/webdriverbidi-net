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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi007");

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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi007");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, SynchronousBodyRule);

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

        if (memberAccess.Name.Identifier.ValueText != "AddObserver")
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        if (!AnalyzerSymbolHelpers.IsLibraryTypeNamed(methodSymbol.ReturnType, "EventObserver"))
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

        // Synchronization primitives (lock, SemaphoreSlim.Wait, WaitHandle.WaitOne, ...) block the
        // dispatching thread of a non-async handler just as Thread.Sleep does. In an async lambda
        // they are BIDI016's deadlock-prone patterns instead, so they are left to that rule there
        // rather than reported twice on the same line.
        bool includeSynchronizationPrimitives = handlerArgument.Expression is not AnonymousFunctionExpressionSyntax anonymousFunction
            || !anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

        DiagnosticDescriptor rule = optionPresent ? SynchronousBodyRule : Rule;
        IEnumerable<(SyntaxNode Node, string Name)> blockingOperations = FindBlockingOperations(context, handlerBody, includeSynchronizationPrimitives);
        foreach ((SyntaxNode node, string operationName) in blockingOperations)
        {
            Diagnostic diagnostic = Diagnostic.Create(rule, node.GetLocation(), operationName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Finds the blocking operations in a handler body, each paired with the name to report it under.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="handlerBody">The body of the handler to search.</param>
    /// <param name="includeSynchronizationPrimitives">Whether synchronization primitives count as blocking here.</param>
    /// <returns>Each blocking operation and the name the diagnostic reports for it.</returns>
    /// <remarks>
    /// The name is taken from the bound symbol at the point the operation is recognized, rather than
    /// recovered from syntax afterwards. How a call is written varies more than its meaning does:
    /// <c>task.Wait()</c> is a member access, <c>task?.Wait()</c> a member binding, and <c>Sleep(100)</c>
    /// under a <c>using static</c> a bare name. Reading the name back off the syntax has to enumerate
    /// those shapes and fails on the ones it does not know, which for an analyzer means an exception
    /// that suppresses the whole rule for the file.
    /// </remarks>
    private static IEnumerable<(SyntaxNode Node, string Name)> FindBlockingOperations(
        SyntaxNodeAnalysisContext context,
        SyntaxNode handlerBody,
        bool includeSynchronizationPrimitives)
    {
        List<(SyntaxNode Node, string Name)> blockingOps = [];

        if (includeSynchronizationPrimitives)
        {
            blockingOps.AddRange(handlerBody.DescendantNodesAndSelf(AnalyzerSymbolHelpers.DoesNotBeginNestedFunction)
                .OfType<LockStatementSyntax>()
                .Select(lockStatement => ((SyntaxNode)lockStatement, "lock")));
        }

        // Do not descend into a nested lambda, anonymous method or local function: its body runs only
        // when that delegate is invoked, not on the dispatching thread. Task.Run(() => Thread.Sleep(...))
        // is the very remedy this rule recommends, so reporting inside it would flag the fix.
        IEnumerable<InvocationExpressionSyntax> invocations = handlerBody.DescendantNodesAndSelf(AnalyzerSymbolHelpers.DoesNotBeginNestedFunction)
            .OfType<InvocationExpressionSyntax>();

        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            if (IsBlockingMethod(methodSymbol, includeSynchronizationPrimitives))
            {
                blockingOps.Add((invocation, methodSymbol.Name + "()"));
                continue;
            }

            if (methodSymbol.Name == "GetResult" &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InvocationExpressionSyntax getAwaiterCall)
            {
                IMethodSymbol? getAwaiterSymbol = context.SemanticModel.GetSymbolInfo(getAwaiterCall).Symbol as IMethodSymbol;
                if (getAwaiterSymbol is { Name: "GetAwaiter" })
                {
                    blockingOps.Add((invocation, methodSymbol.Name + "()"));
                    continue;
                }
            }
        }

        IEnumerable<MemberAccessExpressionSyntax> memberAccesses = handlerBody.DescendantNodesAndSelf(AnalyzerSymbolHelpers.DoesNotBeginNestedFunction)
            .OfType<MemberAccessExpressionSyntax>();

        foreach (MemberAccessExpressionSyntax memberAccess in memberAccesses)
        {
            if (memberAccess.Name.Identifier.ValueText == "Result")
            {
                // Task<T>.Result and ValueTask<T>.Result both block until the operation completes.
                ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (expressionType is { Name: "Task" or "ValueTask" })
                {
                    blockingOps.Add((memberAccess, memberAccess.Name.Identifier.Text));
                }
            }
        }

        return blockingOps;
    }

    private static bool IsBlockingMethod(IMethodSymbol method, bool includeSynchronizationPrimitives)
    {
        // WaitOne is declared on WaitHandle, so a call through any of its derived types (Mutex,
        // Semaphore, ManualResetEvent, AutoResetEvent, ...) binds to a method whose containing type
        // is WaitHandle. ManualResetEventSlim and CountdownEvent are not WaitHandles and declare
        // their own blocking Wait methods.
        return (method.ContainingType.Name, method.Name) switch
        {
            ("Thread", "Sleep") or ("Thread", "Join") or ("Task", "Wait") => true,
            ("Task", "WaitAll") or ("Task", "WaitAny") => includeSynchronizationPrimitives,
            ("Monitor", "Enter") or ("Monitor", "TryEnter") => includeSynchronizationPrimitives,
            ("SemaphoreSlim", "Wait") or ("ManualResetEventSlim", "Wait") or ("CountdownEvent", "Wait") => includeSynchronizationPrimitives,
            ("WaitHandle", "WaitOne") => includeSynchronizationPrimitives,
            _ => false,
        };
    }

}
