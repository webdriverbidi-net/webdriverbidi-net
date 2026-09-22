// <copyright file="BiDiDriver038_ObserverUseAfterDisposalAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects an <c>EventObserver&lt;T&gt;</c> whose capture members are called after it has
/// been disposed.
/// </summary>
/// <remarks>
/// Disposing an observer unsubscribes it and closes any capture session it holds, and its capture
/// members throw <see cref="System.ObjectDisposedException"/> from then on. An observer cannot be
/// resubscribed, so the remedy is a new observer from the event.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver038_ObserverUseAfterDisposalAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI038";

    private const string Category = "Usage";

    /// <summary>
    /// The members of <c>EventObserver&lt;T&gt;</c> that throw once the observer is disposed. The
    /// members deliberately absent do not: <c>Unobserve</c> and <c>StopCapturingTasks</c> are no-ops on
    /// a disposed observer, <c>Dispose</c> and <c>DisposeAsync</c> are idempotent, and <c>ToString</c>
    /// reads state that outlives disposal.
    /// </summary>
    private static readonly HashSet<string> DisposalGuardedObserverMethods =
    [
        "StartCapturingTasks",
        "WaitForCapturedTasksAsync",
        "WaitForCapturedTasksCompleteAsync",
        "GetCapturedTasks",
    ];

    private static readonly LocalizableString Title = "Observer used after disposal";

    private static readonly LocalizableString MessageFormat = "'{0}' is called on '{1}' after it has been disposed. A disposed EventObserver cannot be reused; add a new observer to the event instead.";

    private static readonly LocalizableString Description = "Disposing an EventObserver<T> unsubscribes it from its event and closes any capture session it holds, and its capture members throw ObjectDisposedException from that point on. An observer cannot be resubscribed, so code that needs to capture handler tasks after disposing must add a new observer to the event.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi038");

    private static readonly DisposalRules ObserverRules = new(
        IsObserverType,
        GetObserverVariableName,
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
            ObserverRules,
            (invocation, method, observerVariableName) =>
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), method.Name, observerVariableName)));
    }

    private static bool IsObserverType(ITypeSymbol? type)
    {
        return AnalyzerSymbolHelpers.IsLibraryTypeNamed(type, "EventObserver");
    }

    /// <summary>
    /// Names the observer an invocation is made on.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="body">The member body being walked.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <param name="isDirectCall">Whether the call is made on the observer variable itself.</param>
    /// <returns>The observer variable's name, or <see langword="null"/>.</returns>
    /// <remarks>
    /// Unlike a driver, whose modules carry its disposal guard into calls made one member deeper, an
    /// observer guards only its own members, so nothing but a call on the variable itself is judged.
    /// The receiver may still be wrapped -- <c>observer!.GetCapturedTasks()</c>.
    /// </remarks>
    private static string? GetObserverVariableName(InvocationExpressionSyntax invocation, SyntaxNode body, SemanticModel semanticModel, out bool isDirectCall)
    {
        isDirectCall = false;

        IdentifierNameSyntax? receiver = AnalyzerSymbolHelpers.GetMemberChainRoot(invocation.Expression, out int memberDepth);
        if (receiver is null || memberDepth != 1)
        {
            return null;
        }

        if (!IsObserverType(semanticModel.GetTypeInfo(receiver).Type))
        {
            return null;
        }

        isDirectCall = true;
        return receiver.Identifier.ValueText;
    }

    private static bool ThrowsAfterDisposal(IMethodSymbol method)
    {
        return IsObserverType(method.ContainingType) && DisposalGuardedObserverMethods.Contains(method.Name);
    }
}
