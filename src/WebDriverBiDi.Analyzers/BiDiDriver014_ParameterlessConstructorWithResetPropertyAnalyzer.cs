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

    /// <summary>
    /// The key of the diagnostic property that carries the fully qualified name of the type declaring
    /// the reset property, for the code fix to write the replacement's receiver from.
    /// </summary>
    public const string DeclaringTypeFullNamePropertyName = "DeclaringTypeFullName";

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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi014");

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

        // A tracked object handed to a method outside the library may be configured by that method
        // (Configure(parameters) before the command is sent), which this rule cannot see; the
        // Warning severity prefers a missed report to accusing code that does configure the object.
        // Passing it to a library method — the command that sends it — is not configuration.
        MarkVariablesPassedOutsideLibrary(context.Node, semanticModel, trackedVariables);

        // Report diagnostics for variables that were never assigned properties
        foreach (KeyValuePair<string, VariableState> kvp in trackedVariables)
        {
            if (!kvp.Value.HasPropertyAssignment && kvp.Value.ResetPropertyName != null)
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    kvp.Value.ConstructorLocation,
                    CreateDiagnosticProperties(kvp.Value.TypeName, kvp.Value.ResetPropertyName, kvp.Value.DeclaringTypeName, kvp.Value.DeclaringTypeFullName, kvp.Value.ResetPropertyTypeName),
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
            // BaseObjectCreationExpressionSyntax, not ObjectCreationExpressionSyntax, so that the
            // target-typed form (`new()`) is recognized; it is the library's own reset idiom.
            if (argument.Expression is not BaseObjectCreationExpressionSyntax objectCreation)
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

            // The object reaches the diagnostic unconfigured only if nothing between here and the
            // command can configure it, which is the same test the variable form applies in
            // MarkVariablesPassedOutsideLibrary. A callee the analyzer cannot resolve, one declared
            // outside the library, or an indexer (whose symbol is a property, not a method) may all
            // set properties on the object before it is used.
            IMethodSymbol? callee = semanticModel.GetSymbolInfo(argument.Parent!.Parent!).Symbol as IMethodSymbol;
            if (callee is null || !AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(callee.ContainingType))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                objectCreation.GetLocation(),
                CreateDiagnosticProperties(type.Name, resetProperty.PropertyName, resetProperty.DeclaringTypeName, resetProperty.DeclaringTypeFullName, resetProperty.PropertyTypeName),
                type.Name,
                resetProperty.PropertyName));
        }
    }

    private static void MarkVariablesPassedOutsideLibrary(
        SyntaxNode node,
        SemanticModel semanticModel,
        Dictionary<string, VariableState> trackedVariables)
    {
        foreach (ArgumentSyntax argument in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<ArgumentSyntax>())
        {
            // An indexer argument (dictionary[parameters]) has a bracketed argument list and hands
            // the object to nothing that could configure it.
            if (argument.Expression is not IdentifierNameSyntax identifier
                || !trackedVariables.TryGetValue(identifier.Identifier.ValueText, out VariableState? state)
                || argument.Parent is not ArgumentListSyntax argumentList)
            {
                continue;
            }

            // An unresolved callee, or one declared outside the library, may configure the object.
            IMethodSymbol? callee = semanticModel.GetSymbolInfo(argumentList.Parent!).Symbol as IMethodSymbol;
            if (callee is null || !AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(callee.ContainingType))
            {
                state.HasPropertyAssignment = true;
            }
        }
    }

    private static ImmutableDictionary<string, string?> CreateDiagnosticProperties(string typeName, string resetPropertyName, string declaringTypeName, string declaringTypeFullName, string resetPropertyTypeName)
    {
        ImmutableDictionary<string, string?>.Builder properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("TypeName", typeName);
        properties.Add("ResetPropertyName", resetPropertyName);
        properties.Add("DeclaringTypeName", declaringTypeName);
        properties.Add(DeclaringTypeFullNamePropertyName, declaringTypeFullName);

        // The code fix retypes a local only when the property cannot be assigned to the declared
        // type, which the declaring type alone does not say; see ResetPropertyInfo.PropertyTypeName.
        properties.Add("ResetPropertyTypeName", resetPropertyTypeName);
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
            // BaseObjectCreationExpressionSyntax so the target-typed form (`new()`) is recognized.
            if (variable.Initializer?.Value is not BaseObjectCreationExpressionSyntax objectCreation)
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
            trackedVariables[variable.Identifier.ValueText] = new VariableState
            {
                TypeName = type.Name,
                ResetPropertyName = resetProperty.PropertyName,
                DeclaringTypeName = resetProperty.DeclaringTypeName,
                DeclaringTypeFullName = resetProperty.DeclaringTypeFullName,
                ResetPropertyTypeName = resetProperty.PropertyTypeName,
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
            IdentifierNameSyntax id => id.Identifier.ValueText,
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
                    return new ResetPropertyInfo(property.Name, current.Name, current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), property.Type.Name);
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

        public string DeclaringTypeFullName { get; set; } = string.Empty;

        public string ResetPropertyTypeName { get; set; } = string.Empty;

        public Location ConstructorLocation { get; set; } = Location.None;

        public bool HasPropertyAssignment { get; set; }
    }

    private class ResetPropertyInfo
    {
        public ResetPropertyInfo(string propertyName, string declaringTypeName, string declaringTypeFullName, string propertyTypeName)
        {
            this.PropertyName = propertyName;
            this.DeclaringTypeName = declaringTypeName;
            this.DeclaringTypeFullName = declaringTypeFullName;
            this.PropertyTypeName = propertyTypeName;
        }

        public string PropertyName { get; }

        public string DeclaringTypeName { get; }

        /// <summary>
        /// Gets the fully qualified name of the declaring type. The code fix writes the reset
        /// property's receiver from this and lets the simplifier shorten it, so the replacement
        /// resolves whether or not the declaring type's namespace is imported at the fix site.
        /// </summary>
        public string DeclaringTypeFullName { get; }

        /// <summary>
        /// Gets the name of the type the reset property returns. This is what a local initialized
        /// from the property must be able to hold, and it is not implied by the declaring type: a
        /// helper declared on a base class commonly returns the derived type (as
        /// SetGeolocationOverrideCommandParameters.ResetGeolocationOverride returns
        /// SetGeolocationOverrideCoordinatesCommandParameters), in which case a local declared with
        /// the derived type needs no change.
        /// </summary>
        public string PropertyTypeName { get; }
    }
}
