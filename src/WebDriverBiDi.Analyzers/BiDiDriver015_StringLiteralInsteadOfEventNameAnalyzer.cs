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

    private static readonly LocalizableString Title =
        "Use ObservableEvent.EventName instead of string literal";

    private static readonly LocalizableString MessageFormat = "Use '{0}' instead of string literal \"{1}\" to avoid typos and ensure event name consistency";

    private static readonly LocalizableString Description = "String literals for event names in Session.SubscribeAsync() are error-prone. Use the EventName property from the corresponding ObservableEvent to ensure type safety and consistency.";

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

        // Find the driver variable (if any)
        string? driverVariableName = FindDriverVariable(context, method);
        if (driverVariableName == null)
        {
            return;
        }

        // Find all Session.SubscribeAsync calls
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
                    AnalyzeSubscribeCall(context, invocation, driverVariableName);
                }
            }
        }
    }

    private static void AnalyzeSubscribeCall(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        string driverVariableName)
    {
        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        // Get the first argument (SubscribeCommandParameters)
        ExpressionSyntax firstArg = invocation.ArgumentList.Arguments[0].Expression;
        if (firstArg is not ObjectCreationExpressionSyntax objectCreation || objectCreation.ArgumentList == null)
        {
            return;
        }

        if (objectCreation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        // Get the events array argument
        ExpressionSyntax eventsArg = objectCreation.ArgumentList.Arguments[0].Expression;
        AnalyzeEventsArray(context, eventsArg, driverVariableName);
    }

    private static void AnalyzeEventsArray(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        string driverVariableName)
    {
        // Handle array creation: new[] { "event1", "event2" } or new string[] { "event1", "event2" }
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            foreach (ExpressionSyntax item in implicitArray.Initializer.Expressions)
            {
                AnalyzeStringLiteral(context, item, driverVariableName);
            }
        }
        else if (expression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer != null)
        {
            foreach (ExpressionSyntax item in arrayCreation.Initializer.Expressions)
            {
                AnalyzeStringLiteral(context, item, driverVariableName);
            }
        }
        else if (expression is CollectionExpressionSyntax collectionExpression)
        {
            // Handle C# 12 collection expressions: ["event1", "event2"]
            foreach (CollectionElementSyntax element in collectionExpression.Elements)
            {
                if (element is ExpressionElementSyntax expressionElement)
                {
                    AnalyzeStringLiteral(context, expressionElement.Expression, driverVariableName);
                }
            }
        }
    }

    private static void AnalyzeStringLiteral(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        string driverVariableName)
    {
        // Check if this is a string literal
        if (expression is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        Optional<object?> constantValue = context.SemanticModel.GetConstantValue(literal);
        if (!constantValue.HasValue || constantValue.Value is not string eventName)
        {
            return;
        }

        // Try to find the corresponding ObservableEvent property
        string? eventPath = FindObservableEventPath(context, driverVariableName, eventName);
        if (eventPath != null)
        {
            ImmutableDictionary<string, string?>.Builder propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
            propertiesBuilder.Add("EventPath", eventPath);
            propertiesBuilder.Add("DriverVariable", driverVariableName);
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
        string driverVariableName,
        string eventName)
    {
        // Look for the driver variable in the method
        MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;
        if (method.Body == null)
        {
            return null;
        }

        // Find driver variable declaration
        ITypeSymbol? driverType = null;
        foreach (StatementSyntax statement in method.Body.Statements)
        {
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
                {
                    if (variable.Identifier.Text == driverVariableName)
                    {
                        if (variable.Initializer?.Value != null)
                        {
                            driverType = context.SemanticModel.GetTypeInfo(variable.Initializer.Value).Type;
                            break;
                        }
                    }
                }
            }
        }

        if (driverType == null || !AnalyzerSymbolHelpers.IsCommandExecutorType(driverType))
        {
            return null;
        }

        // Search through driver's module properties
        foreach (ISymbol member in driverType.GetMembers())
        {
            if (member is IPropertySymbol propertySymbol && IsModuleType(propertySymbol.Type))
            {
                // Search through module's ObservableEvent properties
                foreach (ISymbol moduleMember in propertySymbol.Type.GetMembers())
                {
                    if (moduleMember is IPropertySymbol eventProperty && IsObservableEventType(eventProperty.Type))
                    {
                        // Get the event name from the ObservableEvent
                        string? observableEventName = GetEventNameFromObservableEvent(context, eventProperty);
                        if (observableEventName == eventName)
                        {
                            return $"{driverVariableName}.{propertySymbol.Name}.{eventProperty.Name}.EventName";
                        }
                    }
                }
            }
        }

        return null;
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
            if (attr.AttributeClass?.Name == "ObservableEventNameAttribute" &&
                attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is string eventName)
            {
                return eventName;
            }
        }

        return null;
    }

    private static string? FindDriverVariable(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method)
    {
        if (method.Body == null)
        {
            return null;
        }

        // Look for BiDiDriver variable declarations
        foreach (StatementSyntax statement in method.Body.Statements)
        {
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
                {
                    if (variable.Initializer?.Value != null)
                    {
                        ITypeSymbol? type = context.SemanticModel.GetTypeInfo(variable.Initializer.Value).Type;
                        if (type != null && AnalyzerSymbolHelpers.IsCommandExecutorType(type))
                        {
                            return variable.Identifier.Text;
                        }
                    }
                }
            }
        }

        return null;
    }

    private static bool IsModuleType(ITypeSymbol type)
    {
        return type.Name.EndsWith("Module");
    }

    private static bool IsSessionModule(ITypeSymbol type)
    {
        return type.Name == "SessionModule";
    }

    private static bool IsObservableEventType(ITypeSymbol type)
    {
        return type is INamedTypeSymbol namedType && namedType.Name == "ObservableEvent";
    }
}
