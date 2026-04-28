// <copyright file="BiDiDriver005_MissingEventSubscriptionAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects AddObserver() calls on module events without corresponding Session.SubscribeAsync() calls.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver005_MissingEventSubscriptionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI005";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Missing Session.SubscribeAsync for event observer";

    private static readonly LocalizableString MessageFormat = "Event observer registered for event '{0}' but '{0}' is not included in Session.SubscribeAsync() call. Protocol events require both AddObserver() and Session.SubscribeAsync() with matching event names.";

    private static readonly LocalizableString Description = "WebDriver BiDi uses a two-part event model: AddObserver() registers a local handler, and Session.SubscribeAsync() tells the remote end to send those events. Both calls are required with matching event names.";

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

        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;

        if (method.Body == null)
        {
            return;
        }

        // Find all AddObserver calls on module events
        System.Collections.Generic.List<(InvocationExpressionSyntax Invocation, string EventName)> addObserverCalls = [];

        foreach (StatementSyntax statement in method.Body.Statements)
        {
            System.Collections.Generic.IEnumerable<InvocationExpressionSyntax> invocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (InvocationExpressionSyntax invocation in invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                {
                    continue;
                }

                IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (methodSymbol == null || methodSymbol.Name != "AddObserver")
                {
                    continue;
                }

                // Check if AddObserver is being called on a Module's ObservableEvent
                if (IsModuleObservableEvent(context, memberAccess.Expression, out string? eventName))
                {
                    addObserverCalls.Add((invocation, eventName!));
                }
            }
        }

        if (addObserverCalls.Count == 0)
        {
            return;
        }

        // Get all subscribed event names from Session.SubscribeAsync calls
        System.Collections.Generic.HashSet<string> subscribedEvents = GetSubscribedEventNames(context, method);

        // Report diagnostics for AddObserver calls without matching Subscribe
        foreach ((InvocationExpressionSyntax invocation, string eventName) in addObserverCalls)
        {
            if (!subscribedEvents.Contains(eventName))
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), eventName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsModuleObservableEvent(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        out string? eventName)
    {
        eventName = null;

        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(expression).Type;
        if (typeSymbol == null)
        {
            return false;
        }

        // Check if the type is ObservableEvent<T>
        if (typeSymbol is not INamedTypeSymbol namedType || namedType.Name != "ObservableEvent")
        {
            return false;
        }

        // Check if the root is a driver variable accessing a module
        ExpressionSyntax current = expression;
        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            current = memberAccess.Expression;
        }

        if (current is IdentifierNameSyntax identifier)
        {
            ITypeSymbol? identifierType = context.SemanticModel.GetTypeInfo(identifier).Type;
            if (identifierType != null && AnalyzerSymbolHelpers.IsCommandExecutorType(identifierType))
            {
                // Verify the expression goes through a Module property
                if (expression is MemberAccessExpressionSyntax ma)
                {
                    ISymbol? firstSymbol = context.SemanticModel.GetSymbolInfo(ma.Expression).Symbol;
                    if (firstSymbol is IPropertySymbol propertySymbol && IsModuleType(propertySymbol.Type))
                    {
                        // Extract the EventName from the ObservableEvent property
                        eventName = GetEventNameFromProperty(context, expression);
                        return eventName != null;
                    }
                }
            }
        }

        return false;
    }

    private static string? GetEventNameFromProperty(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
        if (symbol is not IPropertySymbol propertySymbol)
        {
            return null;
        }

        // Read the event name from [ObservableEventName("...")] — works for both source-backed
        // and metadata-backed symbols, so this analyzer functions when WebDriverBiDi is
        // referenced as a compiled assembly rather than compiled alongside user code.
        foreach (AttributeData attr in propertySymbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "ObservableEventNameAttribute" &&
                attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is string eventName)
            {
                return eventName;
            }
        }

        return null;
    }

    private static System.Collections.Generic.HashSet<string> GetSubscribedEventNames(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method)
    {
        System.Collections.Generic.HashSet<string> subscribedEvents = [];

        if (method.Body == null)
        {
            return subscribedEvents;
        }

        foreach (StatementSyntax statement in method.Body.Statements)
        {
            System.Collections.Generic.IEnumerable<InvocationExpressionSyntax> invocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (InvocationExpressionSyntax invocation in invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                {
                    continue;
                }

                IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (methodSymbol == null)
                {
                    continue;
                }

                // Check if this is Session.SubscribeAsync
                if (methodSymbol.Name == "SubscribeAsync" && IsSessionModule(methodSymbol.ContainingType))
                {
                    // Extract event names from the SubscribeCommandParameters argument
                    if (invocation.ArgumentList.Arguments.Count > 0)
                    {
                        ExpressionSyntax firstArg = invocation.ArgumentList.Arguments[0].Expression;
                        ExtractEventNamesFromSubscribeParameters(context, firstArg, subscribedEvents);
                    }
                }
            }
        }

        return subscribedEvents;
    }

    private static void ExtractEventNamesFromSubscribeParameters(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        System.Collections.Generic.HashSet<string> eventNames)
    {
        // Handle: new SubscribeCommandParameters(new[] { "log.entryAdded", "network.beforeRequest" })
        if (expression is ObjectCreationExpressionSyntax objectCreation && objectCreation.ArgumentList != null)
        {
            if (objectCreation.ArgumentList.Arguments.Count > 0)
            {
                ExpressionSyntax eventsArg = objectCreation.ArgumentList.Arguments[0].Expression;
                ExtractEventNamesFromArrayExpression(context, eventsArg, eventNames);
            }
        }
    }

    private static void ExtractEventNamesFromArrayExpression(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        System.Collections.Generic.HashSet<string> eventNames)
    {
        // Handle array creation: new[] { "event1", "event2" } or new string[] { "event1", "event2" }
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            foreach (ExpressionSyntax item in implicitArray.Initializer.Expressions)
            {
                ExtractStringLiteral(context, item, eventNames);
            }
        }
        else if (expression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer != null)
        {
            foreach (ExpressionSyntax item in arrayCreation.Initializer.Expressions)
            {
                ExtractStringLiteral(context, item, eventNames);
            }
        }
        else if (expression is CollectionExpressionSyntax collectionExpression)
        {
            // Handle C# 12 collection expressions: ["event1", "event2"]
            foreach (CollectionElementSyntax element in collectionExpression.Elements)
            {
                if (element is ExpressionElementSyntax expressionElement)
                {
                    ExtractStringLiteral(context, expressionElement.Expression, eventNames);
                }
            }
        }
    }

    private static void ExtractStringLiteral(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        System.Collections.Generic.HashSet<string> eventNames)
    {
        Optional<object?> constantValue = context.SemanticModel.GetConstantValue(expression);
        if (constantValue.HasValue && constantValue.Value is string eventName)
        {
            eventNames.Add(eventName);
            return;
        }

        // Handle .EventName property access: driver.Module.Event.EventName (as recommended by BIDI015).
        // GetConstantValue cannot resolve property accesses, so we extract the event name from
        // the [ObservableEventName] attribute on the ObservableEvent property instead.
        if (expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == "EventName")
        {
            string? resolvedName = GetEventNameFromProperty(context, memberAccess.Expression);
            if (resolvedName != null)
            {
                eventNames.Add(resolvedName);
            }
        }
    }

    private static bool IsModuleType(ITypeSymbol type)
    {
        return type.Name.EndsWith("Module");
    }

    private static bool IsSessionModule(ITypeSymbol type)
    {
        return type.Name == "SessionModule";
    }
}
