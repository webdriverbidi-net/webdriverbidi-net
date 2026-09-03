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
/// Analyzer that detects RegisterEvent() calls after StartAsync(). AddObserver() calls are
/// deliberately not reported: observers may be added at any time, including while the
/// driver is running.
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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi002");

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
        ImmutableDictionary<string, DriverVariableState> driverVariables = ImmutableDictionary<string, DriverVariableState>.Empty;

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            driverVariables = ProcessNode(context, statement, driverVariables);
        }
    }

    private static ImmutableDictionary<string, DriverVariableState> ProcessNode(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node,
        ImmutableDictionary<string, DriverVariableState> driverVariables)
    {
        ImmutableDictionary<string, DriverVariableState> state = driverVariables;

        // Walk the node's statements in document order. The walk does not descend into the bodies of
        // nested functions (lambdas, anonymous methods, local functions): their code runs when the
        // delegate is invoked, not at the textual position where it is declared. It also stops at if
        // statements — including one that is itself the root, which the barrier yields without
        // descending into — and processes them below with a forked copy of the state per branch.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                state = ProcessIfStatement(context, ifStatement, state);
            }
            else if (descendant is LocalDeclarationStatementSyntax localDeclaration)
            {
                state = AnalyzeLocalDeclaration(context, localDeclaration, state);
            }
            else if (descendant is ExpressionStatementSyntax expressionStatement)
            {
                state = AnalyzeExpressionStatement(context, expressionStatement, state);
            }
        }

        return state;
    }

    private static ImmutableDictionary<string, DriverVariableState> ProcessIfStatement(
        SyntaxNodeAnalysisContext context,
        IfStatementSyntax ifStatement,
        ImmutableDictionary<string, DriverVariableState> driverVariables)
    {
        // Statements in the condition execute unconditionally, before either branch.
        ImmutableDictionary<string, DriverVariableState> state = ProcessNode(context, ifStatement.Condition, driverVariables);

        // The branches are mutually exclusive, so each arm is walked against its own copy of the
        // state at the branch point. An else-if chain arrives here as an else clause whose statement
        // is itself an if statement, which ProcessNode routes back into this method.
        ImmutableDictionary<string, DriverVariableState> thenState = ProcessNode(context, ifStatement.Statement, state);
        ImmutableDictionary<string, DriverVariableState> elseState = ifStatement.Else is not null
            ? ProcessNode(context, ifStatement.Else.Statement, state)
            : state;

        // After the branch, a driver counts as started only when every path through the branch
        // leaves it started; otherwise a RegisterEvent after the if on a path that never started
        // would be falsely flagged.
        ImmutableDictionary<string, DriverVariableState> merged = state;
        foreach (string driverName in state.Keys)
        {
            bool started = thenState[driverName].IsStarted && elseState[driverName].IsStarted;
            merged = merged.SetItem(driverName, new DriverVariableState { IsStarted = started });
        }

        return merged;
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
        if (methodName == "StartAsync")
        {
            DriverVariableState currentState = new DriverVariableState { IsStarted = true };
            updatedVariables = updatedVariables.SetItem(driverVariableName, currentState);
        }

        // StopAsync() returns the driver to the not-started state; the runtime permits
        // registration again after a stop, so the tracked state must reflect that.
        if (methodName == "StopAsync")
        {
            DriverVariableState stoppedState = new DriverVariableState { IsStarted = false };
            updatedVariables = updatedVariables.SetItem(driverVariableName, stoppedState);
        }

        // Check for RegisterEvent after StartAsync. Note that adding an observer to an
        // ObservableEvent<T> (AddObserver) is deliberately not reported: observers may be
        // added or removed at any time, including while the driver is running. Only the
        // registration of custom protocol events (RegisterEvent) is locked once the driver
        // has started, and that is the only call the runtime rejects.
        if (methodName == "RegisterEvent")
        {
            DriverVariableState state = updatedVariables[driverVariableName];
            if (state.IsStarted)
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), GetEventName(context, invocation));
                context.ReportDiagnostic(diagnostic);
            }
        }

        return updatedVariables;
    }

    private static string GetEventName(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        // RegisterEvent's first argument is the event name; report it in the message rather than the
        // literal method name. The call has already resolved to RegisterEvent(string, Func<...>), so
        // the argument is present.
        ExpressionSyntax firstArgument = invocation.ArgumentList.Arguments[0].Expression;
        if (context.SemanticModel.GetConstantValue(firstArgument) is { HasValue: true, Value: string eventName })
        {
            return eventName;
        }

        // A non-constant event name (for example a variable) cannot be resolved at compile time, so
        // fall back to the argument's source text.
        return firstArgument.ToString();
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

    private class DriverVariableState
    {
        public bool IsStarted { get; set; }
    }
}
