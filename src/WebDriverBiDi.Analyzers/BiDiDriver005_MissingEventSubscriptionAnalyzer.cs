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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi005");

    /// <summary>
    /// The diagnostic property naming the span of the events argument the code fix can amend, written
    /// as "start,length". Absent when the analyzed body has no amendable subscription.
    /// </summary>
    public const string EventsArgumentSpanKey = "EventsArgumentSpan";

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
        // Find all AddObserver calls on module events
        System.Collections.Generic.List<(InvocationExpressionSyntax Invocation, string EventName)> addObserverCalls = [];

        // GetBodyDescendantNodes covers block bodies, expression bodies, and top-level programs alike.
        foreach (InvocationExpressionSyntax invocation in AnalyzerSymbolHelpers.GetBodyDescendantNodes(context.Node).OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            // Cheap syntactic pre-filter before the expensive semantic bind: skip any invocation
            // whose member name is not the one this pass cares about. The bound symbol's name is
            // therefore already known, so only the null (unresolved) case needs re-checking.
            if (memberAccess.Name.Identifier.ValueText != "AddObserver")
            {
                continue;
            }

            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            // Check if AddObserver is being called on a Module's ObservableEvent
            if (IsModuleObservableEvent(context, memberAccess.Expression, out string? eventName))
            {
                addObserverCalls.Add((invocation, eventName!));
            }
        }

        if (addObserverCalls.Count == 0)
        {
            return;
        }

        // Get all subscribed event names from Session.SubscribeAsync calls. When any such
        // call's parameters cannot be inspected at the call site, the subscription set is
        // unknowable and no missing-subscription warning may be reported.
        if (!TryGetSubscribedEventNames(context, context.Node, out System.Collections.Generic.HashSet<string> subscribedEvents, out ExpressionSyntax? amendableEventsArgument))
        {
            return;
        }

        // Hand the code fix the events argument this pass already located and validated, so the fix
        // does not repeat the search — and cannot disagree with it about which calls count or which
        // shapes can be amended.
        ImmutableDictionary<string, string?> properties = CreateDiagnosticProperties(amendableEventsArgument);

        // Report diagnostics for AddObserver calls without matching Subscribe
        foreach ((InvocationExpressionSyntax invocation, string eventName) in addObserverCalls)
        {
            if (!IsEventSubscribed(eventName, subscribedEvents))
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), properties, eventName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    /// <summary>
    /// Records the span of the events argument the code fix can amend, when this pass found one.
    /// </summary>
    /// <param name="amendableEventsArgument">The events argument, or <see langword="null"/> if there is none.</param>
    /// <returns>The diagnostic properties.</returns>
    private static ImmutableDictionary<string, string?> CreateDiagnosticProperties(ExpressionSyntax? amendableEventsArgument)
    {
        if (amendableEventsArgument is null)
        {
            return ImmutableDictionary<string, string?>.Empty;
        }

        Microsoft.CodeAnalysis.Text.TextSpan span = amendableEventsArgument.Span;
        return ImmutableDictionary<string, string?>.Empty.Add(
            EventsArgumentSpanKey,
            $"{span.Start},{span.Length}");
    }

    private static bool IsEventSubscribed(string eventName, System.Collections.Generic.HashSet<string> subscribedEvents)
    {
        if (subscribedEvents.Contains(eventName))
        {
            return true;
        }

        // A subscription to a bare module name (for example "log") subscribes to every event in that
        // module, so it covers a fully-qualified event such as "log.entryAdded".
        int dotIndex = eventName.IndexOf('.');
        return dotIndex > 0 && subscribedEvents.Contains(eventName.Substring(0, dotIndex));
    }

    private static bool IsModuleObservableEvent(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        out string? eventName)
    {
        eventName = null;

        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(expression).Type;

        // Check if the type is ObservableEvent<T>
        if (typeSymbol is not INamedTypeSymbol namedType || namedType.Name != "ObservableEvent")
        {
            return false;
        }

        // The event must be reached through a module property of a driver: driver.Log.OnEntryAdded,
        // where Log is a module-typed property and its receiver has the driver type. The
        // driver may be spelled any way that has that type — a local, a parameter, a field reached
        // through `this`, a property of another object, or the result of a call.
        if (expression is not MemberAccessExpressionSyntax eventAccess
            || eventAccess.Expression is not MemberAccessExpressionSyntax moduleAccess)
        {
            return false;
        }

        if (context.SemanticModel.GetSymbolInfo(moduleAccess).Symbol is not IPropertySymbol moduleProperty || !IsModuleType(moduleProperty.Type))
        {
            return false;
        }

        if (!AnalyzerSymbolHelpers.IsCommandExecutorType(context.SemanticModel.GetTypeInfo(moduleAccess.Expression).Type))
        {
            return false;
        }

        // Extract the EventName from the ObservableEvent property
        eventName = GetEventNameFromProperty(context, expression);
        return eventName != null;
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
            if (attr.AttributeClass is { Name: "ObservableEventNameAttribute" } &&
                attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is string eventName)
            {
                return eventName;
            }
        }

        return null;
    }

    private static bool TryGetSubscribedEventNames(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node,
        out System.Collections.Generic.HashSet<string> subscribedEvents,
        out ExpressionSyntax? amendableEventsArgument)
    {
        subscribedEvents = [];
        amendableEventsArgument = null;

        foreach (InvocationExpressionSyntax invocation in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            // Cheap syntactic pre-filter before the expensive semantic bind: skip any invocation
            // whose member name is not the one this pass cares about. The bound symbol's name is
            // therefore already known, so only its containing type needs checking below.
            if (memberAccess.Name.Identifier.ValueText != "SubscribeAsync")
            {
                continue;
            }

            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            // Check if this is Session.SubscribeAsync
            if (IsSessionModule(methodSymbol.ContainingType))
            {
                // Extract event names from the SubscribeCommandParameters argument
                if (invocation.ArgumentList.Arguments.Count > 0)
                {
                    ExpressionSyntax firstArg = invocation.ArgumentList.Arguments[0].Expression;
                    if (firstArg is not BaseObjectCreationExpressionSyntax objectCreation)
                    {
                        // The subscription parameters are not created inline; they are held
                        // in a variable (and possibly built up before the call), so the set
                        // of subscribed event names cannot be determined from this call
                        // site. Treat the whole set as unknowable so the caller suppresses
                        // its warnings: a warning about missing code must prefer a false
                        // negative over a false positive.
                        return false;
                    }

                    if (!ExtractEventNamesFromSubscribeParameters(context, objectCreation, subscribedEvents))
                    {
                        // At least one subscribed event name could not be determined, so the
                        // subscription set is incomplete. Reporting from an incomplete set would
                        // warn about an event that is in fact subscribed.
                        return false;
                    }

                    amendableEventsArgument ??= GetAmendableEventsArgument(objectCreation);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the events argument of a validated parameters construction, or <see langword="null"/> when
    /// the construction names its events somewhere the code fix cannot amend in place.
    /// </summary>
    /// <param name="objectCreation">The parameters construction, already read successfully.</param>
    /// <returns>The events argument, or <see langword="null"/>.</returns>
    private static ExpressionSyntax? GetAmendableEventsArgument(BaseObjectCreationExpressionSyntax objectCreation)
    {
        return objectCreation.ArgumentList is { Arguments.Count: > 0 } argumentList
            ? argumentList.Arguments[0].Expression
            : null;
    }

    private static bool ExtractEventNamesFromSubscribeParameters(
        SyntaxNodeAnalysisContext context,
        BaseObjectCreationExpressionSyntax objectCreation,
        System.Collections.Generic.HashSet<string> eventNames)
    {
        // Handle: new SubscribeCommandParameters(new[] { "log.entryAdded", "network.beforeRequest" }),
        // the single-event constructor new SubscribeCommandParameters("log.entryAdded"), and their
        // target-typed new(...) equivalents.
        // Events is exposed as a mutable list, so an object initializer can add to the set the
        // constructor arguments established. An initializer leaves the event set intact only when
        // every one of its elements assigns some other member (Contexts or UserContexts); anything
        // else — an assignment to Events, or a collection-initializer element, which adds an event
        // directly — puts events into the set that are not written in the constructor arguments.
        if (objectCreation.Initializer != null && MayExtendEventList(objectCreation.Initializer))
        {
            return false;
        }

        if (objectCreation.ArgumentList == null || objectCreation.ArgumentList.Arguments.Count == 0)
        {
            // A construction that passes no events names none: nothing to add, and nothing unread.
            // The library's own parameters type has no such constructor, so this arises only for a
            // type of the same shape declared elsewhere.
            return true;
        }

        ExpressionSyntax eventsArg = objectCreation.ArgumentList.Arguments[0].Expression;
        return ExtractEventNamesFromArrayExpression(context, eventsArg, eventNames);
    }

    private static bool MayExtendEventList(InitializerExpressionSyntax initializer)
    {
        return !initializer.Expressions.All(expression =>
            expression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax identifier }
            && identifier.Identifier.ValueText != "Events");
    }

    private static bool ExtractEventNamesFromArrayExpression(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        System.Collections.Generic.HashSet<string> eventNames)
    {
        // Handle array creation: new[] { "event1", "event2" } or new string[] { "event1", "event2" }
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            return ExtractStringLiterals(context, implicitArray.Initializer.Expressions, eventNames);
        }

        if (expression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer != null)
        {
            return ExtractStringLiterals(context, arrayCreation.Initializer.Expressions, eventNames);
        }

        if (expression is CollectionExpressionSyntax collectionExpression)
        {
            // Handle C# 12 collection expressions: ["event1", "event2"]
            foreach (CollectionElementSyntax element in collectionExpression.Elements)
            {
                // A spread element ([..events]) contributes a set of names that is not written out
                // here, so the subscription set cannot be determined.
                if (element is not ExpressionElementSyntax expressionElement)
                {
                    return false;
                }

                if (!ExtractStringLiteral(context, expressionElement.Expression, eventNames))
                {
                    return false;
                }
            }

            return true;
        }

        // Not an array: either the single-event constructor, SubscribeCommandParameters(string eventName, ...),
        // whose argument is a string literal, a constant, or an ObservableEvent.EventName access; or an
        // events list held in a variable, which ExtractStringLiteral reports as unknowable.
        return ExtractStringLiteral(context, expression, eventNames);
    }

    private static bool ExtractStringLiterals(
        SyntaxNodeAnalysisContext context,
        SeparatedSyntaxList<ExpressionSyntax> expressions,
        System.Collections.Generic.HashSet<string> eventNames)
    {
        foreach (ExpressionSyntax item in expressions)
        {
            if (!ExtractStringLiteral(context, item, eventNames))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ExtractStringLiteral(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        System.Collections.Generic.HashSet<string> eventNames)
    {
        Optional<object?> constantValue = context.SemanticModel.GetConstantValue(expression);
        if (constantValue.HasValue && constantValue.Value is string eventName)
        {
            eventNames.Add(eventName);
            return true;
        }

        // Handle .EventName property access: driver.Module.Event.EventName (as recommended by BIDI015).
        // GetConstantValue cannot resolve property accesses, so we extract the event name from
        // the [ObservableEventName] attribute on the ObservableEvent property instead.
        if (expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.ValueText == "EventName")
        {
            string? resolvedName = GetEventNameFromProperty(context, memberAccess.Expression);
            if (resolvedName != null)
            {
                eventNames.Add(resolvedName);
                return true;
            }
        }

        // The name is not determinable at this call site — an events list held in a variable, a
        // non-constant element, or an unresolvable EventName access. The caller must treat the whole
        // subscription set as unknowable: a warning about a missing subscription has to prefer a
        // false negative over reporting an event that is in fact subscribed.
        return false;
    }

    private static bool IsModuleType(ITypeSymbol type)
    {
        return AnalyzerSymbolHelpers.IsLibraryModuleType(type);
    }

    private static bool IsSessionModule(INamedTypeSymbol type)
    {
        // Require the type to be declared in the WebDriverBiDi namespace so a user's own type named
        // SessionModule in another namespace is not treated as the library's session module.
        return type.Name == "SessionModule" && AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(type);
    }
}
