// <copyright file="BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects the use of parameterless constructor for CommandParameters classes
/// that have a public static Reset property, when no properties are set after construction.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI014";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Use Reset property instead of parameterless constructor";

    private static readonly LocalizableString MessageFormat = "Use '{0}.{1}' instead of 'new {0}()' to make the intent of resetting more explicit. The parameterless constructor should only be used when setting properties afterward.";

    private static readonly LocalizableString Description = "CommandParameters classes with Reset properties should use the Reset property instead of the parameterless constructor to make the intent clear. The parameterless constructor should only be used when properties will be set after construction.";

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

        // Register for method body analysis
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        // Track variables created with parameterless constructor and whether properties are set
        Dictionary<string, VariableState> trackedVariables = [];

        // Walk through all statements in the method
        foreach (StatementSyntax statement in methodDeclaration.DescendantNodes().OfType<StatementSyntax>())
        {
            // Check for variable declaration: var params = new CommandParameters()
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                AnalyzeLocalDeclaration(localDecl, context, semanticModel, trackedVariables);
            }

            // Check for property assignments: params.Property = value
            if (statement is ExpressionStatementSyntax expressionStmt)
            {
                AnalyzeExpressionStatement(expressionStmt, semanticModel, trackedVariables);
            }
        }

        // Report diagnostics for variables that were never assigned properties
        foreach (var kvp in trackedVariables)
        {
            if (!kvp.Value.HasPropertyAssignment && kvp.Value.ResetPropertyName != null)
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("TypeName", kvp.Value.TypeName);
                properties.Add("ResetPropertyName", kvp.Value.ResetPropertyName);

                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    kvp.Value.ConstructorLocation,
                    properties.ToImmutable(),
                    kvp.Value.TypeName,
                    kvp.Value.ResetPropertyName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeLocalDeclaration(
        LocalDeclarationStatementSyntax localDecl,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, VariableState> trackedVariables)
    {
        foreach (var variable in localDecl.Declaration.Variables)
        {
            if (variable.Initializer?.Value is not ObjectCreationExpressionSyntax objectCreation)
            {
                continue;
            }

            // Check if it's a parameterless constructor call
            if (objectCreation.ArgumentList != null && objectCreation.ArgumentList.Arguments.Count > 0)
            {
                continue;
            }

            ITypeSymbol? type = semanticModel.GetTypeInfo(objectCreation).Type;
            if (type == null)
            {
                continue;
            }

            // Check if this is a CommandParameters type
            if (!IsCommandParametersType(type))
            {
                continue;
            }

            // Check if the type has a public static Reset property
            string? resetPropertyName = GetResetPropertyName(type);
            if (resetPropertyName == null)
            {
                continue;
            }

            // Check if object initializer is present: new Type() { Property = value }
            bool hasObjectInitializer = objectCreation.Initializer != null && objectCreation.Initializer.Expressions.Count > 0;

            // Track this variable
            trackedVariables[variable.Identifier.Text] = new VariableState
            {
                TypeName = type.Name,
                ResetPropertyName = resetPropertyName,
                ConstructorLocation = objectCreation.GetLocation(),
                HasPropertyAssignment = hasObjectInitializer,
            };
        }
    }

    private static void AnalyzeExpressionStatement(
        ExpressionStatementSyntax expressionStmt,
        SemanticModel semanticModel,
        Dictionary<string, VariableState> trackedVariables)
    {
        // Check for assignment expressions: variable.Property = value
        if (expressionStmt.Expression is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (assignment.Left is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Get the variable name
        string? variableName = GetVariableName(memberAccess.Expression);
        if (variableName == null || !trackedVariables.ContainsKey(variableName))
        {
            return;
        }

        // Check if the member being assigned is a property
        ISymbol? symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
        if (symbol is IPropertySymbol)
        {
            // Mark that this variable has property assignments
            trackedVariables[variableName].HasPropertyAssignment = true;
        }
    }

    private static string? GetVariableName(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax member => GetVariableName(member.Expression),
            _ => null,
        };
    }

    private static bool IsCommandParametersType(ITypeSymbol type)
    {
        // Check if the type inherits from CommandParameters or CommandParameters<T>
        INamedTypeSymbol? baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "CommandParameters" && baseType.ContainingNamespace?.ToString() == "WebDriverBiDi")
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static string? GetResetPropertyName(ITypeSymbol type)
    {
        // Look for public static properties that start with "Reset" and return the same type
        var properties = type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.IsStatic && p.DeclaredAccessibility == Accessibility.Public);

        foreach (var property in properties)
        {
            // Check if property name starts with "Reset" and returns the same type
            if (property.Name.StartsWith("Reset") && SymbolEqualityComparer.Default.Equals(property.Type, type))
            {
                return property.Name;
            }
        }

        return null;
    }

    private class VariableState
    {
        public string TypeName { get; set; } = string.Empty;

        public string? ResetPropertyName { get; set; }

        public Location ConstructorLocation { get; set; } = Location.None;

        public bool HasPropertyAssignment { get; set; }
    }
}
