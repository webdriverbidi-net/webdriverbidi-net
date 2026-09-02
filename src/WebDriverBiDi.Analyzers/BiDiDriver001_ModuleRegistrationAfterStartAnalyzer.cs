// <copyright file="BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects when RegisterModule() is called after StartAsync() on a BiDiDriver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver001_ModuleRegistrationAfterStartAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI001";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Module registration after driver start";

    private static readonly LocalizableString MessageFormat = "RegisterModule() cannot be called after StartAsync(). Module '{0}' should be registered before calling StartAsync().";

    private static readonly LocalizableString Description = "Modules must be registered before calling StartAsync() on the BiDiDriver. Attempting to register modules after the driver has started will throw an InvalidOperationException at runtime.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi001");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for method, constructor, and top-level-program body analysis.
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        SemanticModel semanticModel = context.SemanticModel;

        // Track BiDiDriver variables and whether StartAsync is currently in effect for each.
        Dictionary<string, bool> driverVariables = [];

        // Walk the top-level statements; ProcessNode descends into nested blocks while forking
        // driver state per if/else branch so a StartAsync in one arm does not mark the driver as
        // started for the other arm.
        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            ProcessNode(statement, context, semanticModel, driverVariables);
        }
    }

    private static void ProcessNode(SyntaxNode node, SyntaxNodeAnalysisContext context, SemanticModel semanticModel, Dictionary<string, bool> driverVariables)
    {
        // Walk the node's statements in document order. The walk does not descend into the bodies
        // of nested functions (lambdas, anonymous methods, local functions): their code runs when
        // the delegate is invoked, not at the textual position where it is declared, so a StartAsync
        // there must not mark the driver as started for the statements that follow the declaration.
        // It also stops at if statements — including one that is itself the root, which the barrier
        // yields without descending into — and processes them below with a forked copy of the state
        // for each mutually exclusive branch.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, semanticModel, driverVariables);
            }
            else if (descendant is LocalDeclarationStatementSyntax localDecl)
            {
                AnalyzeLocalDeclaration(localDecl, semanticModel, driverVariables);
            }
            else if (descendant is ExpressionStatementSyntax expressionStmt)
            {
                AnalyzeExpressionStatement(expressionStmt, context, semanticModel, driverVariables);
            }
        }
    }

    private static void ProcessIfStatement(IfStatementSyntax ifStatement, SyntaxNodeAnalysisContext context, SemanticModel semanticModel, Dictionary<string, bool> driverVariables)
    {
        // Statements in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, semanticModel, driverVariables);

        // The branches are mutually exclusive, so each arm is walked against its own copy of the
        // state at the branch point. An else-if chain arrives here as an else clause whose statement
        // is itself an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranch = new(driverVariables);
        ProcessNode(ifStatement.Statement, context, semanticModel, thenBranch);

        Dictionary<string, bool> elseBranch = new(driverVariables);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, semanticModel, elseBranch);
        }

        // After the branch, a driver counts as started only when every path through the branch
        // leaves it started; otherwise a RegisterModule after the if on a path that never started
        // would be falsely flagged.
        foreach (string driverName in driverVariables.Keys.ToList())
        {
            driverVariables[driverName] = thenBranch[driverName] && elseBranch[driverName];
        }
    }

    private static void AnalyzeLocalDeclaration(LocalDeclarationStatementSyntax localDecl, SemanticModel semanticModel, Dictionary<string, bool> driverVariables)
    {
        foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
        {
            if (variable.Initializer == null)
            {
                continue;
            }

            TypeInfo typeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value);
            if (AnalyzerSymbolHelpers.IsDriverConfigurationType(typeInfo.Type))
            {
                driverVariables[variable.Identifier.Text] = false;
            }
        }
    }

    private static void AnalyzeExpressionStatement(ExpressionStatementSyntax expressionStmt, SyntaxNodeAnalysisContext context, SemanticModel semanticModel, Dictionary<string, bool> driverVariables)
    {
        if (expressionStmt.Expression is AwaitExpressionSyntax awaitExpr && awaitExpr.Expression is InvocationExpressionSyntax invocation)
        {
            // Handle: await driver.StartAsync(...)
            CheckForDriverMethodCall(invocation, context, semanticModel, driverVariables);
        }
        else if (expressionStmt.Expression is InvocationExpressionSyntax directInvocation)
        {
            // Handle: driver.StartAsync(...).Wait() or driver.RegisterModule(...). A blocking
            // .Wait() is unwrapped so the underlying StartAsync call is recognized as starting the
            // driver rather than the Task.Wait() wrapper being analyzed (and ignored).
            CheckForDriverMethodCall(UnwrapBlockingWait(directInvocation), context, semanticModel, driverVariables);
        }
        else if (expressionStmt.Expression is AssignmentExpressionSyntax assignment && assignment.Right is InvocationExpressionSyntax assignedInvocation)
        {
            // Handle: startTask = driver.StartAsync(...). The task may be awaited later, but
            // the connect attempt begins at the call itself, so registration after this point
            // is judged against a started driver — matching BIDI002 and BIDI003.
            CheckForDriverMethodCall(assignedInvocation, context, semanticModel, driverVariables);
        }
    }

    private static InvocationExpressionSyntax UnwrapBlockingWait(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == "Wait" &&
            memberAccess.Expression is InvocationExpressionSyntax innerInvocation)
        {
            return innerInvocation;
        }

        return invocation;
    }

    private static void CheckForDriverMethodCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, SemanticModel semanticModel, Dictionary<string, bool> driverVariables)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        string? driverVariableName = GetDriverVariableName(memberAccess.Expression);
        if (driverVariableName == null || !driverVariables.ContainsKey(driverVariableName))
        {
            return;
        }

        IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        string methodName = methodSymbol.Name;

        // Check if this is StartAsync() being called
        if (methodName == "StartAsync")
        {
            driverVariables[driverVariableName] = true;
        }

        // StopAsync() returns the driver to the not-started state; the runtime permits
        // registration again after a stop, so the tracked state must reflect that.
        if (methodName == "StopAsync")
        {
            driverVariables[driverVariableName] = false;
        }

        // Check if this is RegisterModule() being called AFTER StartAsync()
        if (methodName == "RegisterModule" && driverVariables[driverVariableName])
        {
            // Get module parameter for better error message
            string moduleName = "module";
            if (invocation.ArgumentList.Arguments.Count > 0)
            {
                ExpressionSyntax arg = invocation.ArgumentList.Arguments[0].Expression;
                moduleName = arg.ToString();
            }

            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), moduleName);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string? GetDriverVariableName(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax member => GetDriverVariableName(member.Expression),
            _ => null,
        };
    }
}
