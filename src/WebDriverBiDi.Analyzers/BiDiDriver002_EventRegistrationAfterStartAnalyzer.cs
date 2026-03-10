// <copyright file="BiDiDriver002_EventRegistrationAfterStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects RegisterEvent() or AddObserver() calls after StartAsync().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver002_EventRegistrationAfterStartAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI002";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Event registration after StartAsync";

    private static readonly LocalizableString MessageFormat = "Event '{0}' is registered after calling StartAsync. Events must be registered before the driver starts.";

    private static readonly LocalizableString Description = "Events must be registered before calling StartAsync to ensure proper event handling initialization.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
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
        ImmutableDictionary<string, DriverVariableState> driverVariables = ImmutableDictionary<string, DriverVariableState>.Empty;

        foreach (StatementSyntax statement in method.Body?.Statements ?? [])
        {
            driverVariables = AnalyzeStatement(context, statement, driverVariables);
        }
    }

    private static ImmutableDictionary<string, DriverVariableState> AnalyzeStatement(
        SyntaxNodeAnalysisContext context,
        StatementSyntax statement,
        ImmutableDictionary<string, DriverVariableState> driverVariables)
    {
        ImmutableDictionary<string, DriverVariableState> updatedVariables = driverVariables;

        if (statement is LocalDeclarationStatementSyntax localDeclaration)
        {
            updatedVariables = AnalyzeLocalDeclaration(context, localDeclaration, updatedVariables);
        }
        else if (statement is ExpressionStatementSyntax expressionStatement)
        {
            updatedVariables = AnalyzeExpressionStatement(context, expressionStatement, updatedVariables);
        }

        return updatedVariables;
    }

    private static ImmutableDictionary<string, DriverVariableState> AnalyzeLocalDeclaration(
        SyntaxNodeAnalysisContext context,
        LocalDeclarationStatementSyntax localDeclaration,
        ImmutableDictionary<string, DriverVariableState> driverVariables)
    {
        ImmutableDictionary<string, DriverVariableState> updatedVariables = driverVariables;

        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            if (variable.Initializer?.Value == null)
            {
                continue;
            }

            ITypeSymbol? variableType = context.SemanticModel.GetTypeInfo(variable.Initializer.Value).Type;
            if (AnalyzerSymbolHelpers.IsCommandExecutorType(variableType))
            {
                updatedVariables = updatedVariables.Add(variable.Identifier.Text, new DriverVariableState());
            }
        }

        return updatedVariables;
    }

    private static ImmutableDictionary<string, DriverVariableState> AnalyzeExpressionStatement(
        SyntaxNodeAnalysisContext context,
        ExpressionStatementSyntax expressionStatement,
        ImmutableDictionary<string, DriverVariableState> driverVariables)
    {
        ImmutableDictionary<string, DriverVariableState> updatedVariables = driverVariables;

        if (expressionStatement.Expression is InvocationExpressionSyntax invocation)
        {
            updatedVariables = AnalyzeInvocation(context, invocation, updatedVariables);
        }
        else if (expressionStatement.Expression is AwaitExpressionSyntax awaitExpression)
        {
            if (awaitExpression.Expression is InvocationExpressionSyntax awaitedInvocation)
            {
                updatedVariables = AnalyzeInvocation(context, awaitedInvocation, updatedVariables);
            }
        }
        else if (expressionStatement.Expression is AssignmentExpressionSyntax assignment)
        {
            if (assignment.Right is InvocationExpressionSyntax assignmentInvocation)
            {
                updatedVariables = AnalyzeInvocation(context, assignmentInvocation, updatedVariables);
            }
        }

        return updatedVariables;
    }

    private static ImmutableDictionary<string, DriverVariableState> AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        ImmutableDictionary<string, DriverVariableState> driverVariables)
    {
        ImmutableDictionary<string, DriverVariableState> updatedVariables = driverVariables;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return updatedVariables;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return updatedVariables;
        }

        string? driverVariableName = GetDriverVariableName(memberAccess);
        if (driverVariableName == null || !updatedVariables.ContainsKey(driverVariableName))
        {
            return updatedVariables;
        }

        string methodName = methodSymbol.Name;

        // Track StartAsync calls
        if (methodName == "StartAsync" && AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            DriverVariableState currentState = new DriverVariableState { IsStarted = true };
            updatedVariables = updatedVariables.SetItem(driverVariableName, currentState);
        }

        // Check for RegisterEvent after StartAsync
        if (methodName == "RegisterEvent" && AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            DriverVariableState state = updatedVariables[driverVariableName];
            if (state.IsStarted)
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), "RegisterEvent");
                context.ReportDiagnostic(diagnostic);
            }
        }

        // Check for AddObserver after StartAsync
        if (methodName == "AddObserver")
        {
            // Check if AddObserver is being called on a BiDiDriver or Module observable event
            if (IsDriverOrModuleObservableEvent(context, memberAccess.Expression))
            {
                DriverVariableState state = updatedVariables[driverVariableName];
                if (state.IsStarted)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), "AddObserver");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        return updatedVariables;
    }

    private static bool IsDriverOrModuleObservableEvent(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(expression).Type;
        if (typeSymbol == null)
        {
            return false;
        }

        // Check if the type is ObservableEvent<T>
        if (typeSymbol is INamedTypeSymbol namedType && namedType.Name == "ObservableEvent")
        {
            // Trace back through member accesses to find the root object
            ExpressionSyntax current = expression;
            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                current = memberAccess.Expression;
            }

            // Check if the root is a BiDiDriver or Module type
            ITypeSymbol? rootType = context.SemanticModel.GetTypeInfo(current).Type;
            if (rootType != null && (AnalyzerSymbolHelpers.IsCommandExecutorType(rootType) || IsModuleType(rootType)))
            {
                return true;
            }
        }

        return false;
    }

    private static string? GetDriverVariableName(MemberAccessExpressionSyntax memberAccess)
    {
        ExpressionSyntax current = memberAccess.Expression;

        // Walk through the member access chain to find the base identifier
        while (current is MemberAccessExpressionSyntax nestedMemberAccess)
        {
            current = nestedMemberAccess.Expression;
        }

        if (current is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.Text;
        }

        return null;
    }

    private static bool IsModuleType(ITypeSymbol type)
    {
        return type.Name.EndsWith("Module");
    }

    private class DriverVariableState
    {
        public bool IsStarted { get; set; }
    }
}
