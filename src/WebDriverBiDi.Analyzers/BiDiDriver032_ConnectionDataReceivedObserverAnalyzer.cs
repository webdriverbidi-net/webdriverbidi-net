// <copyright file="BiDiDriver032_ConnectionDataReceivedObserverAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects an observer added to <c>Connection.OnDataReceived</c> on a connection that the same body wraps
/// in a <c>Transport</c>. The event hands over ownership of a pooled buffer rather than broadcasting a copy, so it admits
/// one observer, which the transport claims when it is constructed: adding another throws, and constructing a transport
/// over a connection that already has one throws an <c>ArgumentException</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver032_ConnectionDataReceivedObserverAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI032";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "OnDataReceived observed on a connection a Transport owns";

    private static readonly LocalizableString MessageFormat = "'{0}' observes OnDataReceived on '{1}', which this code also wraps in a Transport. The event admits one observer, which the transport claims, so this throws at runtime. Observe OnLogMessage at Trace level to see protocol traffic, or the transport's own events to see messages.";

    private static readonly LocalizableString Description = "Connection.OnDataReceived hands the observer ownership of a pooled buffer instead of broadcasting a copy, so it admits exactly one observer. A Transport claims it when it is constructed over the connection: adding a second observer throws, and constructing a Transport over a connection that already has one throws an ArgumentException. To inspect traffic, observe the connection's OnLogMessage at Trace level; to inspect messages, observe the transport's events. Observing OnDataReceived is only valid on a connection that no Transport wraps.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi032");

    private static readonly string[] ObserverAddingMethodNames = ["AddObserver", "AddDataCollector", "ToObservable"];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeBody, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeBody(SyntaxNodeAnalysisContext context)
    {
        SemanticModel semanticModel = context.SemanticModel;

        // The names are matched from syntax alone first: finding the wrapped connections binds every object creation in
        // the body, which is wasted on the great majority of bodies that never mention the event.
        List<SimpleNameSyntax> eventNames = [.. AnalyzerSymbolHelpers.GetBodyDescendantNodes(context.Node).OfType<SimpleNameSyntax>().Where(name => name.Identifier.ValueText == "OnDataReceived")];
        if (eventNames.Count == 0)
        {
            return;
        }

        HashSet<ISymbol> wrappedConnections = FindConnectionsWrappedInTransport(context.Node, semanticModel);
        if (wrappedConnections.Count == 0)
        {
            return;
        }

        foreach (SimpleNameSyntax eventName in eventNames)
        {
            // The event read is `connection.OnDataReceived`, or `.OnDataReceived` bound through a null-conditional access.
            if (eventName.Parent is not (MemberAccessExpressionSyntax or MemberBindingExpressionSyntax))
            {
                continue;
            }

            // A bare `OnDataReceived` that is itself the receiver of a member access (inside a derived connection) reads
            // the event of `this`, which no transport in this body was given.
            ExpressionSyntax eventAccess = (ExpressionSyntax)eventName.Parent;
            if (eventAccess is MemberAccessExpressionSyntax memberAccess && memberAccess.Name != eventName)
            {
                continue;
            }

            if (AnalyzerSymbolHelpers.GetMemberChainRoot(eventAccess, out int memberDepth) is not { } connectionIdentifier
                || memberDepth != 1
                || semanticModel.GetSymbolInfo(connectionIdentifier).Symbol is not { } connection
                || !wrappedConnections.Contains(connection)
                || semanticModel.GetSymbolInfo(eventAccess).Symbol is not IPropertySymbol eventProperty
                || !AnalyzerSymbolHelpers.HasTypeOrBaseOrInterface(eventProperty.ContainingType, "Connection"))
            {
                continue;
            }

            if (GetObserverAddingInvocation(eventAccess) is { } invocation)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), GetInvokedName(invocation), connectionIdentifier.Identifier.ValueText));
            }
        }
    }

    /// <summary>
    /// Collects the locals and parameters this body passes to the constructor of a <c>Transport</c>.
    /// </summary>
    /// <param name="body">The body to search.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns>The symbols of the connections wrapped in a transport.</returns>
    private static HashSet<ISymbol> FindConnectionsWrappedInTransport(SyntaxNode body, SemanticModel semanticModel)
    {
        HashSet<ISymbol> wrapped = new(SymbolEqualityComparer.Default);
        foreach (BaseObjectCreationExpressionSyntax creation in AnalyzerSymbolHelpers.GetBodyDescendantNodes(body).OfType<BaseObjectCreationExpressionSyntax>())
        {
            if (creation.ArgumentList is not { Arguments.Count: > 0 } arguments
                || !AnalyzerSymbolHelpers.HasTypeOrBaseOrInterface(semanticModel.GetTypeInfo(creation).Type, "Transport"))
            {
                continue;
            }

            foreach (ArgumentSyntax argument in arguments.Arguments)
            {
                ExpressionSyntax value = argument.Expression;
                while (value is ParenthesizedExpressionSyntax or PostfixUnaryExpressionSyntax or CastExpressionSyntax)
                {
                    value = value switch
                    {
                        ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
                        PostfixUnaryExpressionSyntax postfix => postfix.Operand,
                        _ => ((CastExpressionSyntax)value).Expression,
                    };
                }

                ISymbol? symbol = value is IdentifierNameSyntax identifier ? semanticModel.GetSymbolInfo(identifier).Symbol : null;
                if (symbol is ILocalSymbol or IParameterSymbol)
                {
                    wrapped.Add(symbol);
                }
            }
        }

        return wrapped;
    }

    /// <summary>
    /// Gets the call that adds an observer to an event read: <c>AddObserver</c> or <c>AddDataCollector</c> on the event,
    /// or <c>Subscribe</c> on the event's <c>ToObservable()</c>.
    /// </summary>
    /// <param name="eventAccess">The read of the event.</param>
    /// <returns>The call, or <see langword="null"/> when the event is read for some other purpose.</returns>
    private static InvocationExpressionSyntax? GetObserverAddingInvocation(ExpressionSyntax eventAccess)
    {
        // The event read is always the receiver of a member access that is its parent, never that access's name.
        if (eventAccess.Parent is not MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax invocation } call
            || !ObserverAddingMethodNames.Contains(call.Name.Identifier.ValueText))
        {
            return null;
        }

        if (call.Name.Identifier.ValueText != "ToObservable")
        {
            return invocation;
        }

        // An observable adds an observer only when something subscribes to it.
        return invocation.Parent is MemberAccessExpressionSyntax subscribeAccess
            && subscribeAccess.Name.Identifier.ValueText == "Subscribe"
            && subscribeAccess.Parent is InvocationExpressionSyntax subscription
            ? subscription
            : null;
    }

    private static string GetInvokedName(InvocationExpressionSyntax invocation)
    {
        return ((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier.ValueText;
    }
}
