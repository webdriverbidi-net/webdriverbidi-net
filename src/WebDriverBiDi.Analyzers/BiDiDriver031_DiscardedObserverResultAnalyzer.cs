// <copyright file="BiDiDriver031_DiscardedObserverResultAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects an <c>AddObserver</c> call whose returned <c>EventObserver&lt;T&gt;</c> is
/// discarded, leaving no handle with which to remove the observer.
/// </summary>
/// <remarks>
/// Removing an observer requires the handle <c>AddObserver</c> returns: <c>Unobserve()</c> and
/// <c>Dispose()</c> are members of it, and <c>RemoveObserver</c> needs its <c>Id</c>. An observer added
/// without keeping the handle therefore lives as long as the observable event does. That is a
/// legitimate choice for an observer meant to last the life of the driver, which is why this is
/// reported at <c>Info</c> severity rather than as a warning.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver031_DiscardedObserverResultAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI031";

    private const string Category = "Design";

    private static readonly string[] AddObserverMethodNames = ["AddObserver"];

    private static readonly LocalizableString Title = "EventObserver result discarded";

    private static readonly LocalizableString MessageFormat = "The EventObserver returned by 'AddObserver' is discarded, so this observer can never be removed. Keep the returned observer if it should not last for the lifetime of the event.";

    private static readonly LocalizableString Description = "AddObserver returns the EventObserver that represents the subscription, and that handle is the only way to remove it: Unobserve and Dispose are its members, and RemoveObserver needs its Id. Discarding the result registers a handler that stays for the life of the observable event. Assign the result to a variable (or a using declaration) when the observer should be removed before then.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi031");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        // The result is discarded exactly when the call is the whole of an expression statement. An
        // explicit discard (`_ = ...AddObserver(h);`) is deliberate and is left alone, as is any use
        // that keeps the handle: a declaration, an assignment, an argument, a return.
        if (invocation.Parent is not ExpressionStatementSyntax)
        {
            return;
        }

        if (!AnalyzerSymbolHelpers.CouldInvokeAnyOf(invocation, AddObserverMethodNames))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.Name != "AddObserver" || !AnalyzerSymbolHelpers.IsLibraryTypeNamed(methodSymbol.ReturnType, "EventObserver"))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
    }
}
