// <copyright file="BiDiDriver020_CaptureSessionNotStartedAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects <see cref="EventObserver{T}.WaitForCapturedTasksAsync"/> or
/// <see cref="EventObserver{T}.WaitForCapturedTasksCompleteAsync"/> calls on an observer that has no
/// active capture session (i.e., <see cref="EventObserver{T}.StartCapturingTasks"/> was not called
/// first in the same method).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver020_CaptureSessionNotStartedAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI020";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Capture session not started";

    private static readonly LocalizableString MessageFormat = "'{0}' is called on '{1}' but no capture session is active. Call 'StartCapturingTasks()' before calling '{0}'.";

    private static readonly LocalizableString Description = "WaitForCapturedTasksAsync and WaitForCapturedTasksCompleteAsync require an active capture session. Call StartCapturingTasks() before invoking these methods; calling them without an active session throws InvalidOperationException at runtime.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi020");

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
        // Track whether StartCapturingTasks has been seen for each local EventObserver<T> variable.
        // Only locally-declared variables are tracked; parameter-passed observers are not.
        Dictionary<string, bool> capturingState = [];

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            // ProcessNode registers observer declarations and checks observer method calls,
            // wherever in the statement's subtree they appear.
            ProcessNode(statement, context, capturingState);
        }
    }

    private static void TrackObserverDeclarations(
        LocalDeclarationStatementSyntax localDecl,
        SemanticModel semanticModel,
        Dictionary<string, bool> capturingState)
    {
        foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
        {
            ILocalSymbol localSymbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(variable)!;
            if (localSymbol.Type is INamedTypeSymbol { Name: "EventObserver" })
            {
                capturingState[variable.Identifier.Text] = false;
            }
        }
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> capturingState)
    {
        // Walk the node's descendants in document order, checking each invocation against the
        // tracked capturing state. The walk does not descend into the bodies of nested
        // functions (lambdas, anonymous methods, local functions): their code runs when the
        // delegate is invoked, not at its textual position, so a call there must not be
        // judged against the capturing state at that position. It also stops at if and
        // switch statements — including one that is itself the root, which the barrier
        // yields without descending into — and processes them recursively below with a
        // forked copy of the state for each mutually exclusive branch.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, capturingState);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, capturingState);
            }
            else if (descendant is LocalDeclarationStatementSyntax localDecl)
            {
                // Register observer declarations wherever they appear (including inside nested
                // blocks such as try or using statements); the pre-order walk visits the
                // declaration before any later use of the variable.
                TrackObserverDeclarations(localDecl, context.SemanticModel, capturingState);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, capturingState);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> capturingState)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, capturingState);

        // The branches are mutually exclusive, so each arm is walked against its own copy of
        // the state at the branch point: a StopCapturingTasks in one arm must not poison a
        // wait in the other. An else-if chain arrives here as an else clause whose statement
        // is itself an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchState = new(capturingState);
        ProcessNode(ifStatement.Statement, context, thenBranchState);

        Dictionary<string, bool> elseBranchState = new(capturingState);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, elseBranchState);
        }

        // After the branch, an observer counts as not capturing only when every path through
        // the branch leaves it not capturing. This rule reports waits on an observer with no
        // active capture session, so treating "capturing on some path only" as not capturing
        // would flag correct conditional stop patterns with an Error-severity false positive;
        // the Error severity demands that the wait fail on every path.
        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = thenBranchState[observerName] || elseBranchState[observerName];
        }
    }

    private static void ProcessSwitchStatement(
        SwitchStatementSyntax switchStatement,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> capturingState)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, capturingState);

        // Sections are mutually exclusive in the same way if/else branches are.
        List<Dictionary<string, bool>> sectionStates = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionState = new(capturingState);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, sectionState);
            }

            sectionStates.Add(sectionState);
        }

        // As for if statements, an observer counts as not capturing after the switch only
        // when no path through the switch leaves it capturing. The unchanged state at the
        // switch (the path taken when no section matches) participates in the merge; when a
        // default section makes that path impossible, including it can only suppress a
        // report, never create a false positive, so default detection is not needed.
        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = capturingState[observerName] || sectionStates.Any(sectionState => sectionState[observerName]);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> capturingState)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Expression is not IdentifierNameSyntax receiverIdentifier)
        {
            return;
        }

        string receiverName = receiverIdentifier.Identifier.Text;
        if (!capturingState.ContainsKey(receiverName))
        {
            return;
        }

        string methodName = memberAccess.Name.Identifier.Text;
        switch (methodName)
        {
            case "StartCapturingTasks":
                capturingState[receiverName] = true;
                break;

            case "StopCapturingTasks":
                capturingState[receiverName] = false;
                break;

            case "WaitForCapturedTasksAsync":
            case "WaitForCapturedTasksCompleteAsync":
                if (!capturingState[receiverName])
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), methodName, receiverName));
                }

                break;
        }
    }
}
