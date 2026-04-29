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

        // When RunHandlerAsynchronously is present the handler already runs on a thread-pool
        // thread, so module commands are safe to call.
        if (AnalyzerSymbolHelpers.HasRunHandlerAsynchronouslyOption(context, invocation))
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

        IEnumerable<(InvocationExpressionSyntax Node, string MethodName)> moduleCommands = FindModuleCommandInvocations(context, handlerBody);
        foreach ((InvocationExpressionSyntax node, string methodName) in moduleCommands)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, node.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<(InvocationExpressionSyntax, string)> FindModuleCommandInvocations(
        SyntaxNodeAnalysisContext context,
        SyntaxNode handlerBody)
    {
        List<(InvocationExpressionSyntax, string)> results = [];

        foreach (InvocationExpressionSyntax innerInvocation in handlerBody.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
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
        if (method.ContainingType == null)
        {
            return false;
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
