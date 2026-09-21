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

    private static readonly LocalizableString OtherShapeMessageFormat = "A subscription to event '{0}' is registered with {1}() but '{0}' is not included in Session.SubscribeAsync() call. Protocol events require both a local subscription and Session.SubscribeAsync() with matching event names.";

    // Same ID, category and severity as Rule (release tracking is unchanged); used for the two
    // subscription shapes that are not AddObserver, whose remedy is the same but whose call is not.
    private static readonly DiagnosticDescriptor OtherShapeRule = new(
        DiagnosticId,
        Title,
        OtherShapeMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi005");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, OtherShapeRule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        // Find all AddObserver calls on module events of a driver this body creates
        System.Collections.Generic.List<(InvocationExpressionSyntax Invocation, string EventName, string MethodName)> subscriptionCalls = [];
        System.Collections.Generic.HashSet<string>? escapedNames = null;

        // GetBodyDescendantNodes covers block bodies, expression bodies, and top-level programs alike.
        foreach (InvocationExpressionSyntax invocation in AnalyzerSymbolHelpers.GetBodyDescendantNodes(context.Node).OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            // Cheap syntactic pre-filter before the expensive semantic bind: skip any invocation
            // whose member name is not one this pass cares about. The bound symbol's name is
            // therefore already known, so only the null (unresolved) case needs re-checking.
            string methodName = memberAccess.Name.Identifier.ValueText;
            if (methodName is not ("AddObserver" or "AddDataCollector" or "Subscribe"))
            {
                continue;
            }

            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            // Every one of the three shapes needs the same remote subscription. Subscribe is reached
            // through the IObservable<T> adapter, so the event is the receiver of that ToObservable call.
            ExpressionSyntax eventExpression = memberAccess.Expression;
            if (methodName == "Subscribe")
            {
                if (eventExpression is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "ToObservable" } adapterAccess })
                {
                    continue;
                }

                eventExpression = adapterAccess.Expression;
            }

            // Check that the subscription is made on a Module's ObservableEvent
            if (!IsModuleObservableEvent(context, eventExpression, out string? eventName, out ExpressionSyntax? driverExpression))
            {
                continue;
            }

            // Only a driver this body creates, and does not hand to other code, can be judged from this body. A
            // driver held in a field, received as a parameter, or returned by a call is typically subscribed where
            // it is set up (a test fixture's set-up method, say), which this body cannot see; so is a local driver
            // passed to a helper. Reporting for those would warn about a subscription that exists. A driver handed to
            // a module's constructor (driver.RegisterModule(new CustomModule(driver))) is not handed on in that sense:
            // the module keeps it in order to send commands through it, and subscribes nothing.
            escapedNames ??= FindDriversThatMayBeSubscribedElsewhere(context.Node, context.SemanticModel);
            if (IsDriverCreatedInBody(context.SemanticModel, driverExpression!, escapedNames))
            {
                subscriptionCalls.Add((invocation, eventName!, methodName));
            }
        }

        if (subscriptionCalls.Count == 0)
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

        // Report diagnostics for local subscriptions without a matching remote one
        foreach ((InvocationExpressionSyntax invocation, string eventName, string methodName) in subscriptionCalls)
        {
            if (!IsEventSubscribed(eventName, subscribedEvents))
            {
                Diagnostic diagnostic = methodName == "AddObserver"
                    ? Diagnostic.Create(Rule, invocation.GetLocation(), properties, eventName)
                    : Diagnostic.Create(OtherShapeRule, invocation.GetLocation(), properties, eventName, methodName);
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

    /// <summary>
    /// Collects the names of driver variables whose subscriptions this body cannot see: those handed to other code,
    /// and those whose session module is handed to other code, which can subscribe through it.
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the body.</param>
    /// <returns>The names of the drivers that may be subscribed elsewhere.</returns>
    /// <remarks>
    /// Only the session module is followed. It is the module that subscribes, and every other module is handed to
    /// other code without giving that code any way to subscribe. A session module held in a local
    /// (<c>SessionModule session = driver.Session;</c>) escapes when that local does.
    /// </remarks>
    private static System.Collections.Generic.HashSet<string> FindDriversThatMayBeSubscribedElsewhere(SyntaxNode body, SemanticModel semanticModel)
    {
        System.Collections.Generic.HashSet<string> escapedNames = DriverStartStateWalker.FindDriversWithUnknownStartedState(body, semanticModel);
        System.Collections.Generic.HashSet<string>? escapedLocals = null;
        foreach (SyntaxNode node in AnalyzerSymbolHelpers.GetBodyDescendantNodes(body))
        {
            // The read of the module (`driver.Session`, or `.Session` in `driver?.Session`) and the expression whose
            // value is the module: the member access itself, or the whole conditional access when the binding is its
            // entire non-null branch. A binding with more after it (`driver?.Session.StatusAsync()`) yields some other
            // value, and is not a read of the module that could be handed on.
            ExpressionSyntax sessionMember;
            ExpressionSyntax sessionRead;
            if (node is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.ValueText == "Session")
            {
                sessionMember = memberAccess;
                sessionRead = memberAccess;
            }
            else if (node is MemberBindingExpressionSyntax binding
                && binding.Name.Identifier.ValueText == "Session"
                && binding.Parent is ConditionalAccessExpressionSyntax conditionalAccess
                && conditionalAccess.WhenNotNull == binding)
            {
                sessionMember = binding;
                sessionRead = conditionalAccess;
            }
            else
            {
                continue;
            }

            if (AnalyzerSymbolHelpers.GetMemberChainRoot(sessionMember, out int memberDepth) is not { } driverIdentifier
                || memberDepth != 1
                || !AnalyzerSymbolHelpers.IsLibraryTypeNamed(semanticModel.GetTypeInfo(sessionMember).Type, "SessionModule"))
            {
                continue;
            }

            bool sessionEscapes;
            if (AnalyzerSymbolHelpers.PeelExpressionWrappers(sessionRead).Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax alias })
            {
                escapedLocals ??= AnalyzerSymbolHelpers.FindVariablesHandedToOtherCode(body);
                sessionEscapes = escapedLocals.Contains(alias.Identifier.ValueText);
            }
            else
            {
                sessionEscapes = AnalyzerSymbolHelpers.IsHandedToOtherCode(sessionRead);
            }

            if (sessionEscapes)
            {
                escapedNames.Add(driverIdentifier.Identifier.ValueText);
            }
        }

        return escapedNames;
    }

    /// <summary>
    /// Determines whether a driver expression names a driver this body creates and keeps to itself: a
    /// <c>new BiDiDriver(...)</c> in the event access itself, or a local initialized with one that is never handed
    /// to other code.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the body.</param>
    /// <param name="driverExpression">The expression the module is read from.</param>
    /// <param name="escapedNames">The names of the variables this body hands to other code.</param>
    /// <returns><see langword="true"/> if the body creates the driver and keeps it; otherwise <see langword="false"/>.</returns>
    private static bool IsDriverCreatedInBody(SemanticModel semanticModel, ExpressionSyntax driverExpression, System.Collections.Generic.HashSet<string> escapedNames)
    {
        return driverExpression switch
        {
            BaseObjectCreationExpressionSyntax => true,
            IdentifierNameSyntax identifier => semanticModel.GetSymbolInfo(identifier).Symbol is ILocalSymbol local
                && local.DeclaringSyntaxReferences[0].GetSyntax() is VariableDeclaratorSyntax { Initializer.Value: BaseObjectCreationExpressionSyntax }
                && !escapedNames.Contains(identifier.Identifier.ValueText),
            _ => false,
        };
    }

    private static bool IsModuleObservableEvent(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        out string? eventName,
        out ExpressionSyntax? driverExpression)
    {
        eventName = null;
        driverExpression = null;

        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(expression).Type;

        // Check if the type is ObservableEvent<T>
        if (typeSymbol is not INamedTypeSymbol namedType || namedType.Name != "ObservableEvent")
        {
            return false;
        }

        // The event must be reached through a module property of a driver: driver.Log.OnEntryAdded,
        // where Log is a module-typed property and its receiver has the driver type. The
        // driver may be spelled any way that has that type — a local, a parameter, a field reached
        // through `this`, a property of another object, or the result of a call — and the caller
        // decides which of those spellings this body can judge.
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
        driverExpression = moduleAccess.Expression;
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
            // whose member name is not one this pass cares about. A subscription is sent either as
            // Session.SubscribeAsync(parameters) or, at the driver level, as
            // ExecuteCommandAsync(parameters) with the same SubscribeCommandParameters; the remote end
            // sees the same command either way, so both spellings count.
            string memberName = memberAccess.Name.Identifier.ValueText;
            if (memberName != "SubscribeAsync" && memberName != "ExecuteCommandAsync")
            {
                continue;
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                continue;
            }

            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                continue;
            }

            ExpressionSyntax firstArg = invocation.ArgumentList.Arguments[0].Expression;
            if (!IsSubscription(context, memberName, methodSymbol, firstArg))
            {
                continue;
            }

            // Extract event names from the SubscribeCommandParameters argument
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

        return true;
    }

    /// <summary>
    /// Determines whether an invocation sends a subscription: <c>Session.SubscribeAsync</c>, whose
    /// argument can only be subscription parameters, or the driver's <c>ExecuteCommandAsync</c> given a
    /// <c>SubscribeCommandParameters</c> argument.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="memberName">The invoked member's name, already known to be one of the two.</param>
    /// <param name="methodSymbol">The bound method.</param>
    /// <param name="firstArg">The invocation's first argument.</param>
    /// <returns><see langword="true"/> if the invocation sends a subscription; otherwise <see langword="false"/>.</returns>
    private static bool IsSubscription(SyntaxNodeAnalysisContext context, string memberName, IMethodSymbol methodSymbol, ExpressionSyntax firstArg)
    {
        if (memberName == "SubscribeAsync")
        {
            return IsSessionModule(methodSymbol.ContainingType);
        }

        // ExecuteCommandAsync sends whatever parameters it is given, so only the argument's type says
        // whether this call is a subscription. The library's own type is required, as for the session
        // module, so a user's type of the same name is not mistaken for it.
        return AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType)
            && context.SemanticModel.GetTypeInfo(firstArg).Type is INamedTypeSymbol { Name: "SubscribeCommandParameters" } parametersType
            && AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(parametersType);
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
