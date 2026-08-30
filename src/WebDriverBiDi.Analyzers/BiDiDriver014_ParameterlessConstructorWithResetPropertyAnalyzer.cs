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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for method body analysis
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        SemanticModel semanticModel = context.SemanticModel;

        // Track variables created with parameterless constructor and whether properties are set
        Dictionary<string, VariableState> trackedVariables = [];

        // Walk through all statements in the method, constructor, or top-level program.
        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetAllStatements(context.Node))
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
        foreach (KeyValuePair<string, VariableState> kvp in trackedVariables)
        {
            if (!kvp.Value.HasPropertyAssignment && kvp.Value.ResetPropertyName != null)
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    kvp.Value.ConstructorLocation,
                    CreateDiagnosticProperties(kvp.Value.TypeName, kvp.Value.ResetPropertyName, kvp.Value.DeclaringTypeName),
                    kvp.Value.TypeName,
                    kvp.Value.ResetPropertyName);

                context.ReportDiagnostic(diagnostic);
            }
        }

        // Also detect inline constructor usage in method call arguments, e.g.:
        //   await driver.Emulation.SetTimeZoneOverrideAsync(new SetTimeZoneOverrideCommandParameters())
        // An inline constructor has no variable to assign properties to afterward, so any
        // parameterless constructor with a Reset property used inline is always a diagnostic.
        AnalyzeInlineConstructors(context.Node, context, semanticModel);
    }

    private static void AnalyzeInlineConstructors(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel)
    {
        foreach (ArgumentSyntax argument in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<ArgumentSyntax>())
        {
            if (argument.Expression is not ObjectCreationExpressionSyntax objectCreation)
            {
                continue;
            }

            // Parameterless?
            if (objectCreation.ArgumentList != null && objectCreation.ArgumentList.Arguments.Count > 0)
            {
                continue;
            }

            // Has object initializer with properties set? Then intent is clear — suppress.
            if (objectCreation.Initializer != null && objectCreation.Initializer.Expressions.Count > 0)
            {
                continue;
            }

            ITypeSymbol type = semanticModel.GetTypeInfo(objectCreation).Type!;
            if (!IsCommandParametersType(type))
            {
                continue;
            }

            ResetPropertyInfo? resetProperty = GetResetProperty(type);
            if (resetProperty == null)
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                objectCreation.GetLocation(),
                CreateDiagnosticProperties(type.Name, resetProperty.PropertyName, resetProperty.DeclaringTypeName),
                type.Name,
                resetProperty.PropertyName));
        }
    }

    private static ImmutableDictionary<string, string?> CreateDiagnosticProperties(string typeName, string resetPropertyName, string declaringTypeName)
    {
        ImmutableDictionary<string, string?>.Builder properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("TypeName", typeName);
        properties.Add("ResetPropertyName", resetPropertyName);
        properties.Add("DeclaringTypeName", declaringTypeName);
        return properties.ToImmutable();
    }

    private static void AnalyzeLocalDeclaration(
        LocalDeclarationStatementSyntax localDecl,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, VariableState> trackedVariables)
    {
        foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
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

            ITypeSymbol type = semanticModel.GetTypeInfo(objectCreation).Type!;

            // Check if this is a CommandParameters type
            if (!IsCommandParametersType(type))
            {
                continue;
            }

            // Check if the type (or one of its base types) has a public static Reset property
            ResetPropertyInfo? resetProperty = GetResetProperty(type);
            if (resetProperty == null)
            {
                continue;
            }

            // Check if object initializer is present: new Type() { Property = value }
            bool hasObjectInitializer = objectCreation.Initializer != null && objectCreation.Initializer.Expressions.Count > 0;

            // Track this variable
            trackedVariables[variable.Identifier.Text] = new VariableState
            {
                TypeName = type.Name,
                ResetPropertyName = resetProperty.PropertyName,
                DeclaringTypeName = resetProperty.DeclaringTypeName,
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
        // Property assignment: variable.Property = value
        if (expressionStmt.Expression is AssignmentExpressionSyntax assignment)
        {
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

            return;
        }

        // Method call through a member of the variable: variable.Collection.Add(...) or
        // variable.SomeMethod(...). This configures the object just as a property assignment does — and
        // for a get-only collection property (for example SetExtraHeadersCommandParameters.Headers) it
        // is the only way to populate it — so the parameterless constructor is not a bare reset.
        if (expressionStmt.Expression is InvocationExpressionSyntax invocation &&
            invocation.Expression is MemberAccessExpressionSyntax invocationTarget)
        {
            string? variableName = GetVariableName(invocationTarget.Expression);
            if (variableName != null && trackedVariables.ContainsKey(variableName))
            {
                trackedVariables[variableName].HasPropertyAssignment = true;
            }
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
            if (baseType.Name == "CommandParameters" && baseType.ContainingNamespace!.ToString() == "WebDriverBiDi")
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static ResetPropertyInfo? GetResetProperty(ITypeSymbol type)
    {
        // Look for a public static property that starts with "Reset" and returns the constructed
        // type or one of its base types. The property may be declared on the constructed type or
        // inherited from a base class; ITypeSymbol.GetMembers() returns declared members only, so
        // the base-type chain is walked explicitly. A reset helper declared on an abstract base
        // that returns the base type (e.g. SetGeolocationOverrideCommandParameters.
        // ResetGeolocationOverride, used with the derived
        // SetGeolocationOverrideCoordinatesCommandParameters) is therefore recognized, while
        // property-level sentinels that return an unrelated type (e.g. Viewport, double) are not.
        for (ITypeSymbol? current = type; current != null; current = current.BaseType)
        {
            IEnumerable<IPropertySymbol> properties = current.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.IsStatic && p.DeclaredAccessibility == Accessibility.Public);

            foreach (IPropertySymbol property in properties)
            {
                if (property.Name.StartsWith("Reset", System.StringComparison.Ordinal) && IsSameTypeOrBaseTypeOf(property.Type, type))
                {
                    return new ResetPropertyInfo(property.Name, current.Name);
                }
            }
        }

        return null;
    }

    private static bool IsSameTypeOrBaseTypeOf(ITypeSymbol candidate, ITypeSymbol type)
    {
        for (ITypeSymbol? current = type; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(candidate, current))
            {
                return true;
            }
        }

        return false;
    }

    private class VariableState
    {
        public string TypeName { get; set; } = string.Empty;

        public string? ResetPropertyName { get; set; }

        public string DeclaringTypeName { get; set; } = string.Empty;

        public Location ConstructorLocation { get; set; } = Location.None;

        public bool HasPropertyAssignment { get; set; }
    }

    private class ResetPropertyInfo
    {
        public ResetPropertyInfo(string propertyName, string declaringTypeName)
        {
            this.PropertyName = propertyName;
            this.DeclaringTypeName = declaringTypeName;
        }

        public string PropertyName { get; }

        public string DeclaringTypeName { get; }
    }
}
