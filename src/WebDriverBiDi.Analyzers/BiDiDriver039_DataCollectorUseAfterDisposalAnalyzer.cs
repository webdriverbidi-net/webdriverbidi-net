// <copyright file="BiDiDriver039_DataCollectorUseAfterDisposalAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects an <c>EventDataCollector&lt;T&gt;</c> whose collected data is read after it
/// has been disposed.
/// </summary>
/// <remarks>
/// Disposing a collector unsubscribes its observer from the event and completes its channel, and
/// reading the collected data throws <see cref="System.ObjectDisposedException"/> from then on. A
/// collector cannot be resubscribed, so the remedy is a new collector from the event. This is the
/// companion of BIDI038, which says the same of an <c>EventObserver&lt;T&gt;</c>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver039_DataCollectorUseAfterDisposalAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI039";

    private const string Category = "Usage";

    /// <summary>
    /// The members of <c>EventDataCollector&lt;T&gt;</c> that throw once the collector is disposed. The
    /// members deliberately absent do not: <c>Dispose</c> and <c>DisposeAsync</c> are idempotent, and
    /// <c>ToString</c> reads the observer, which outlives disposal. <c>Events</c> is a property rather
    /// than a call, and does not throw either: its channel is completed, so the sequence simply ends.
    /// </summary>
    private static readonly HashSet<string> DisposalGuardedCollectorMethods =
    [
        "GetCollectedEventData",
    ];

    private static readonly LocalizableString Title = "Data collector used after disposal";

    private static readonly LocalizableString MessageFormat = "'{0}' is called on '{1}' after it has been disposed. A disposed EventDataCollector cannot be reused; add a new data collector to the event instead.";

    private static readonly LocalizableString Description = "Disposing an EventDataCollector<T> unsubscribes its observer from the event and completes the channel it collects into, and GetCollectedEventData throws ObjectDisposedException from that point on. A collector cannot be resubscribed, so code that needs to read collected events after disposing must add a new data collector to the event.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi039");

    private static readonly DisposalRules CollectorRules = new(
        IsCollectorType,
        GetCollectorVariableName,
        ["Dispose", "DisposeAsync"],
        ThrowsAfterDisposal);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        DisposalStateWalker.Walk(
            context,
            CollectorRules,
            (invocation, method, collectorVariableName) =>
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), method.Name, collectorVariableName)));
    }

    private static bool IsCollectorType(ITypeSymbol? type)
    {
        return AnalyzerSymbolHelpers.IsLibraryTypeNamed(type, "EventDataCollector");
    }

    /// <summary>
    /// Names the collector an invocation is made on.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="body">The member body being walked.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <param name="isDirectCall">Whether the call is made on the collector variable itself.</param>
    /// <returns>The collector variable's name, or <see langword="null"/>.</returns>
    /// <remarks>
    /// As for an observer, a collector guards only its own members, so nothing but a call on the
    /// variable itself is judged. The receiver may still be wrapped -- <c>collector!.GetCollectedEventData()</c>.
    /// </remarks>
    private static string? GetCollectorVariableName(InvocationExpressionSyntax invocation, SyntaxNode body, SemanticModel semanticModel, out bool isDirectCall)
    {
        isDirectCall = false;

        IdentifierNameSyntax? receiver = AnalyzerSymbolHelpers.GetMemberChainRoot(invocation.Expression, out int memberDepth);
        if (receiver is null || memberDepth != 1)
        {
            return null;
        }

        if (!IsCollectorType(semanticModel.GetTypeInfo(receiver).Type))
        {
            return null;
        }

        isDirectCall = true;
        return receiver.Identifier.ValueText;
    }

    private static bool ThrowsAfterDisposal(IMethodSymbol method)
    {
        return IsCollectorType(method.ContainingType) && DisposalGuardedCollectorMethods.Contains(method.Name);
    }
}
