// <copyright file="BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects string literals in Session.SubscribeAsync() calls instead of using ObservableEvent.EventName.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI015";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Use ObservableEvent.EventName instead of string literal";

    private static readonly LocalizableString MessageFormat = "Use '{0}' instead of string literal \"{1}\" to avoid typos and ensure event name consistency";

    private static readonly LocalizableString Description = "String literals for event names in Session.SubscribeAsync() are error-prone. Use the EventName property from the corresponding ObservableEvent to ensure type safety and consistency.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi015");

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
        // Find all Session.SubscribeAsync calls
        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            System.Collections.Generic.IEnumerable<InvocationExpressionSyntax> invocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (InvocationExpressionSyntax invocation in invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                {
                    continue;
                }

                // Rule out every other call in the body on its name alone, before paying for the
                // semantic model. The name test against the resolved symbol below remains the
                // authoritative one; this only avoids binding calls that cannot possibly match.
                if (!AnalyzerSymbolHelpers.CouldInvokeAnyOf(invocation, SubscribeMethodName))
                {
                    continue;
                }

                IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (methodSymbol == null)
                {
                    continue;
                }

                // Check if this is Session.SubscribeAsync
                if (methodSymbol.Name != "SubscribeAsync" || !IsSessionModule(methodSymbol.ContainingType))
                {
                    continue;
                }

                // The driver comes from the call's own receiver rather than from a search for a local
                // declaration, so a driver held in a parameter, a field or a property is found just as
                // a local is — and the suggested replacement names whatever the call site actually used.
                if (GetDriverFromReceiver(context, memberAccess.Expression) is not (string, ITypeSymbol) driverVariable)
                {
                    continue;
                }

                AnalyzeSubscribeCall(context, invocation, driverVariable);
            }
        }
    }

    /// <summary>
    /// Gets the name and type of the driver a Session.SubscribeAsync call was made through.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="receiver">The receiver of the SubscribeAsync call, for example <c>driver.Session</c>.</param>
    /// <returns>The driver's name and type, or <see langword="null"/> if the receiver does not root in one.</returns>
    private static (string Name, ITypeSymbol Type)? GetDriverFromReceiver(SyntaxNodeAnalysisContext context, ExpressionSyntax receiver)
    {
        ExpressionSyntax current = receiver;
        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            current = memberAccess.Expression;
        }

        if (current is not IdentifierNameSyntax identifier)
        {
            return null;
        }

        // IsCommandExecutorType accepts a null type and answers false, so no separate null test is needed.
        ITypeSymbol? type = context.SemanticModel.GetTypeInfo(identifier).Type;
        return AnalyzerSymbolHelpers.IsCommandExecutorType(type)
            ? (identifier.Identifier.Text, type!)
            : null;
    }

    private static void AnalyzeSubscribeCall(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        (string Name, ITypeSymbol Type) driverVariable)
    {
        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        // Get the first argument (SubscribeCommandParameters)
        ExpressionSyntax firstArg = invocation.ArgumentList.Arguments[0].Expression;
        // BaseObjectCreationExpressionSyntax so the target-typed form (`new(...)`) is recognized,
        // matching what BIDI005 already accepts for the same argument.
        if (firstArg is not BaseObjectCreationExpressionSyntax objectCreation || objectCreation.ArgumentList == null)
        {
            return;
        }

        if (objectCreation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        // Get the events array argument
        ExpressionSyntax eventsArg = objectCreation.ArgumentList.Arguments[0].Expression;
        AnalyzeEventsArray(context, eventsArg, driverVariable);
    }

    private static void AnalyzeEventsArray(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        (string Name, ITypeSymbol Type) driverVariable)
    {
        // Handle array creation: new[] { "event1", "event2" } or new string[] { "event1", "event2" }
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            foreach (ExpressionSyntax item in implicitArray.Initializer.Expressions)
            {
                AnalyzeStringLiteral(context, item, driverVariable);
            }
        }
        else if (expression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer != null)
        {
            foreach (ExpressionSyntax item in arrayCreation.Initializer.Expressions)
            {
                AnalyzeStringLiteral(context, item, driverVariable);
            }
        }
        else if (expression is CollectionExpressionSyntax collectionExpression)
        {
            // Handle C# 12 collection expressions: ["event1", "event2"]
            foreach (CollectionElementSyntax element in collectionExpression.Elements)
            {
                if (element is ExpressionElementSyntax expressionElement)
                {
                    AnalyzeStringLiteral(context, expressionElement.Expression, driverVariable);
                }
            }
        }
        else
        {
            // Not an array: the single-event constructor, SubscribeCommandParameters(string eventName, ...).
            AnalyzeStringLiteral(context, expression, driverVariable);
        }
    }

    private static void AnalyzeStringLiteral(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        (string Name, ITypeSymbol Type) driverVariable)
    {
        // Check if this is a string literal
        if (expression is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        string eventName = (string)context.SemanticModel.GetConstantValue(literal).Value!;

        // Try to find the corresponding ObservableEvent property
        string? eventPath = FindObservableEventPath(context, driverVariable, eventName);
        if (eventPath != null)
        {
            ImmutableDictionary<string, string?>.Builder propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
            propertiesBuilder.Add("EventPath", eventPath);
            propertiesBuilder.Add("DriverVariable", driverVariable.Name);
            ImmutableDictionary<string, string?> properties = propertiesBuilder.ToImmutable();

            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                literal.GetLocation(),
                properties,
                eventPath,
                eventName);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string? FindObservableEventPath(
        SyntaxNodeAnalysisContext context,
        (string Name, ITypeSymbol Type) driverVariable,
        string eventName)
    {
        // Search the driver's module properties, including those it inherits: a user's type deriving
        // from BiDiDriver declares none of them itself, and GetMembers returns declared members only.
        foreach (ISymbol member in GetAllMembers(driverVariable.Type))
        {
            if (member is IPropertySymbol propertySymbol && IsModuleType(propertySymbol.Type))
            {
                // Search through module's ObservableEvent properties
                foreach (ISymbol moduleMember in GetAllMembers(propertySymbol.Type))
                {
                    if (moduleMember is IPropertySymbol eventProperty && IsObservableEventType(eventProperty.Type))
                    {
                        // Get the event name from the ObservableEvent
                        string? observableEventName = GetEventNameFromObservableEvent(context, eventProperty);
                        if (observableEventName == eventName)
                        {
                            return $"{driverVariable.Name}.{propertySymbol.Name}.{eventProperty.Name}.EventName";
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a type's members, including those declared on its base types.
    /// </summary>
    /// <param name="type">The type to enumerate.</param>
    /// <returns>The declared and inherited members.</returns>
    private static System.Collections.Generic.IEnumerable<ISymbol> GetAllMembers(ITypeSymbol type)
    {
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                yield return member;
            }
        }
    }

    private static string? GetEventNameFromObservableEvent(
        SyntaxNodeAnalysisContext context,
        IPropertySymbol propertySymbol)
    {
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

    private static bool IsModuleType(ITypeSymbol type)
    {
        return AnalyzerSymbolHelpers.IsLibraryModuleType(type);
    }

    // The only method name this analyzer inspects. Hoisted to a static field to avoid allocating on
    // every invocation examined.
    private static readonly string[] SubscribeMethodName = ["SubscribeAsync"];

    private static bool IsSessionModule(INamedTypeSymbol type)
    {
        // Require the type to be declared in the WebDriverBiDi namespace so a user's own type named
        // SessionModule in another namespace is not treated as the library's session module.
        return type.Name == "SessionModule" && AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(type);
    }

    private static bool IsObservableEventType(ITypeSymbol type)
    {
        return AnalyzerSymbolHelpers.IsLibraryTypeNamed(type, "ObservableEvent");
    }
}
