// <copyright file="BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi023");

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
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi023");

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

        // When RunHandlerAsynchronously is present AND the handler actually runs off the
        // dispatching thread (an Action<T> handler, an async lambda, or an async method group),
        // module commands are safe to call. A non-async Task-returning handler still issues the
        // command inline, so it is reported with a message that says the option cannot help.
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
        IEnumerable<(InvocationExpressionSyntax Node, string MethodName)> moduleCommands = FindModuleCommandInvocations(context, handlerBody);
        foreach ((InvocationExpressionSyntax node, string methodName) in moduleCommands)
        {
            Diagnostic diagnostic = Diagnostic.Create(rule, node.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<(InvocationExpressionSyntax, string)> FindModuleCommandInvocations(
        SyntaxNodeAnalysisContext context,
        SyntaxNode handlerBody)
    {
        List<(InvocationExpressionSyntax, string)> results = [];

        // Do not descend into a nested lambda, anonymous method or local function: its body runs only
        // when that delegate is invoked, not at this point in the handler. Offloading work with
        // Task.Run(() => ...) is a remedy these rules recommend, so reporting inside it would flag the fix.
        foreach (InvocationExpressionSyntax innerInvocation in handlerBody.DescendantNodesAndSelf(AnalyzerSymbolHelpers.DoesNotBeginNestedFunction).OfType<InvocationExpressionSyntax>())
        {
            IMethodSymbol? innerMethod = context.SemanticModel.GetSymbolInfo(innerInvocation).Symbol as IMethodSymbol;
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

        if (!method.ContainingType.Name.EndsWith("Module", System.StringComparison.Ordinal))
        {
            return false;
        }

        if (!AnalyzerSymbolHelpers.IsModuleSubclass(method.ContainingType))
        {
            return false;
        }

        // Only flag Task<T>-returning methods (actual commands), not Task/void utility methods.
        return method.ReturnType is INamedTypeSymbol namedReturn && namedReturn.Name == "Task" && namedReturn.IsGenericType;
    }
}
