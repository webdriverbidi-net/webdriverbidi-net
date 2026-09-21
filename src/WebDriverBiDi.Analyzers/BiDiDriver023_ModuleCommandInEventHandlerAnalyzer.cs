// <copyright file="BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects module command calls inside AddObserver event handlers.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver023_ModuleCommandInEventHandlerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI023";

    private const string Category = "Reliability";

    private const string HelpLinkUri = "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi023";

    private static readonly LocalizableString Title = "Module command called inside event handler";

    private static readonly LocalizableString MessageFormat = "Module command '{0}' is called inside an event handler. Module commands are not safe to call directly inside event handlers because the driver's command pipeline may already be executing on the same thread context. Use ObservableEventHandlerOptions.RunHandlerAsynchronously to run the handler on a separate thread.";

    private static readonly LocalizableString Description = "Calling module commands (e.g. NavigateAsync, EvaluateAsync) inside an AddObserver event handler can deadlock or produce unexpected behavior because the WebDriver BiDi command pipeline dispatches events synchronously by default. Configure the handler to run asynchronously with ObservableEventHandlerOptions.RunHandlerAsynchronously.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: HelpLinkUri);

    private static readonly LocalizableString SynchronousBodyMessageFormat = "Module command '{0}' is called inside an event handler. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' does not offload the synchronous body of a Task-returning handler; make the handler 'async' so the command is issued from a continuation rather than on the dispatching thread.";

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
        helpLinkUri: HelpLinkUri);

    private static readonly LocalizableString BeforeFirstAwaitMessageFormat = "Module command '{0}' is issued before the handler's first 'await', on the thread dispatching the event. 'ObservableEventHandlerOptions.RunHandlerAsynchronously' offloads only what follows that 'await'; await first (for example 'await Task.Yield()') so the command is issued from a continuation.";

    // Same ID, category, and severity as Rule; used when the RunHandlerAsynchronously option is present and
    // the handler is async, for a module command issued before the handler's first await and so not from a
    // continuation. The code fix inserts an await of Task.Yield() at the top of the handler.
    private static readonly DiagnosticDescriptor BeforeFirstAwaitRule = new(
        DiagnosticId,
        Title,
        BeforeFirstAwaitMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: HelpLinkUri);

    private static readonly LocalizableString CollectorFilterMessageFormat = "Module command '{0}' is called inside a data collector filter. The filter decides whether to keep an event and runs on the thread dispatching it; it takes no handler options, so the command cannot be offloaded. Collect the event and issue the command where the collected data is read.";

    // Same ID, category and severity as Rule; used for the filter of AddDataCollector, which the library
    // invokes synchronously on the dispatching thread and which has no RunHandlerAsynchronously to offer.
    private static readonly DiagnosticDescriptor CollectorFilterRule = new(
        DiagnosticId,
        Title,
        CollectorFilterMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: HelpLinkUri);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, SynchronousBodyRule, BeforeFirstAwaitRule, CollectorFilterRule);

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

        if (memberAccess.Name.Identifier.ValueText is not ("AddObserver" or "AddDataCollector"))
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        // A collector's filter runs on the dispatching thread exactly as a synchronous handler does: the
        // collector registers it through an observer of its own with RunHandlerSynchronously.
        bool collectorFilter = AnalyzerSymbolHelpers.IsLibraryTypeNamed(methodSymbol.ReturnType, "EventDataCollector");
        if (!collectorFilter && !AnalyzerSymbolHelpers.IsLibraryTypeNamed(methodSymbol.ReturnType, "EventObserver"))
        {
            return;
        }

        // With RunHandlerAsynchronously, a module command is safe where it is issued off the dispatching
        // thread. The library queues an Action<T> handler to the thread pool whole, so nothing in it is
        // reported. A non-async Task-returning handler issues every command inline, so each is reported with a
        // message saying the option cannot help; an async handler (an async lambda or an async method group)
        // is reported for the commands it issues before its first await.
        bool optionPresent = !collectorFilter && AnalyzerSymbolHelpers.HasRunHandlerAsynchronouslyOption(context, invocation);
        if (optionPresent && AnalyzerSymbolHelpers.IsBoundToActionOverload(methodSymbol))
        {
            return;
        }

        ArgumentSyntax? handlerArgument = AnalyzerSymbolHelpers.GetArgumentForParameter(invocation, methodSymbol, collectorFilter ? "filter" : "handler");
        if (handlerArgument == null)
        {
            return;
        }

        bool asyncHandler = AnalyzerSymbolHelpers.IsAsyncHandler(context, handlerArgument.Expression);

        SyntaxNode? handlerBody = AnalyzerSymbolHelpers.GetHandlerBody(context, handlerArgument.Expression);
        if (handlerBody == null)
        {
            return;
        }

        // A method-group handler may be declared in another file. Its body is then queried through the
        // model for that file's tree, and the diagnostic is reported at the AddObserver argument in this
        // file rather than inside the other one, so that it appears where the handler was registered.
        SemanticModel semanticModel = AnalyzerSymbolHelpers.GetSemanticModelFor(context, handlerBody);
        bool reportAtHandlerArgument = !ReferenceEquals(semanticModel, context.SemanticModel);

        // An async handler issues a command on the dispatching thread only until its first await. Without the
        // option every command in the handler is reported, but the ones before that await are still marked,
        // because the code fix has to await first to move them.
        Func<SyntaxNode, bool> runsBeforeFirstYield = asyncHandler ? AnalyzerSymbolHelpers.GetRunsBeforeFirstYield(handlerBody) : static _ => true;
        DiagnosticDescriptor rule = collectorFilter ? CollectorFilterRule : !optionPresent ? Rule : asyncHandler ? BeforeFirstAwaitRule : SynchronousBodyRule;
        IEnumerable<(InvocationExpressionSyntax Node, string MethodName)> moduleCommands = FindModuleCommandInvocations(semanticModel, handlerBody);
        foreach ((InvocationExpressionSyntax node, string methodName) in moduleCommands)
        {
            bool beforeFirstYield = runsBeforeFirstYield(node);
            if (optionPresent && !beforeFirstYield)
            {
                continue;
            }

            Location location = reportAtHandlerArgument ? handlerArgument.GetLocation() : node.GetLocation();
            ImmutableDictionary<string, string?>? properties = asyncHandler && beforeFirstYield ? AnalyzerSymbolHelpers.RunsBeforeFirstAwaitProperties : null;
            Diagnostic diagnostic = Diagnostic.Create(rule, location, properties, methodName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<(InvocationExpressionSyntax, string)> FindModuleCommandInvocations(
        SemanticModel semanticModel,
        SyntaxNode handlerBody)
    {
        List<(InvocationExpressionSyntax, string)> results = [];

        // Do not descend into a nested lambda, anonymous method or local function: its body runs only
        // when that delegate is invoked, not at this point in the handler. Offloading work with
        // Task.Run(() => ...) is a remedy these rules recommend, so reporting inside it would flag the fix.
        foreach (InvocationExpressionSyntax innerInvocation in handlerBody.DescendantNodesAndSelf(AnalyzerSymbolHelpers.DoesNotBeginNestedFunction).OfType<InvocationExpressionSyntax>())
        {
            IMethodSymbol? innerMethod = semanticModel.GetSymbolInfo(innerInvocation).Symbol as IMethodSymbol;
            if (innerMethod == null)
            {
                continue;
            }

            if (!IsModuleCommandMethod(innerMethod))
            {
                continue;
            }

            results.Add((innerInvocation, innerMethod.Name));
        }

        return results;
    }

    private static bool IsModuleCommandMethod(IMethodSymbol method)
    {
        // ExecuteCommandAsync sends a command over the same connection a module command does, so awaiting
        // it from a synchronous handler deadlocks in exactly the same way. Reaching a command through the
        // executor rather than through a module is a spelling difference, not a different hazard. Both
        // of its overloads return Task<T>, so it needs no separate return-type test.
        if (method.Name == "ExecuteCommandAsync" && AnalyzerSymbolHelpers.IsCommandExecutorType(method.ContainingType))
        {
            return true;
        }

        // Any type deriving from the library's Module base class is a module, whatever it is named.
        if (!AnalyzerSymbolHelpers.IsModuleSubclass(method.ContainingType))
        {
            return false;
        }

        // Only flag Task<T>-returning methods (actual commands), not Task/void utility methods.
        return method.ReturnType is INamedTypeSymbol namedReturn && namedReturn.Name == "Task" && namedReturn.IsGenericType;
    }
}
