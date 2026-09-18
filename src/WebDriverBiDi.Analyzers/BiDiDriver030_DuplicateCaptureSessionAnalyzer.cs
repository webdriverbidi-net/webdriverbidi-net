// <copyright file="BiDiDriver030_DuplicateCaptureSessionAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects a second <c>StartCapturingTasks()</c> call on an
/// <c>EventObserver&lt;T&gt;</c> whose capture session is already active.
/// </summary>
/// <remarks>
/// An observer permits one capture session at a time; starting a second throws
/// <c>WebDriverBiDiException</c>. This is the companion of BIDI020, which reports the opposite
/// mistake — reading captured tasks with no session started.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver030_DuplicateCaptureSessionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI030";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Capture session already started";

    private static readonly LocalizableString MessageFormat = "'StartCapturingTasks()' is called on '{0}', which already has an active capture session. Call 'StopCapturingTasks()' first, or read the captured tasks.";

    private static readonly LocalizableString Description = "An EventObserver allows only one capture session at a time. Calling StartCapturingTasks() while a session is already active throws WebDriverBiDiException at runtime. End the session with StopCapturingTasks(), or let a completed WaitForCapturedTasksAsync or WaitForCapturedTasksCompleteAsync end it, before starting another.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi030");

    private static readonly string[] CaptureSessionMethodNames = ["StartCapturingTasks", "StopCapturingTasks"];

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
        // Tracks, for each local EventObserver<T> variable, whether a capture session is certainly
        // active at the current point of the walk. Only locally-declared variables are tracked.
        Dictionary<string, bool> capturingState = [];

        // An observer this member hands to other code, or one a nested function opens or closes a
        // session on, may have a session this walk cannot see, so it is never tracked and never
        // reported on. The names are collected up front because the other code may run before or
        // after the StartCapturingTasks textually.
        // Deferred, because both walks cover the whole body and only an observer declaration ever asks
        // for the result. A member that declares none never pays for them.
        Lazy<HashSet<string>> untrackableNames = new(() =>
        {
            HashSet<string> names = AnalyzerSymbolHelpers.FindVariablesHandedToOtherCode(context.Node);
            names.UnionWith(AnalyzerSymbolHelpers.FindVariablesChangedInsideNestedFunctions(context.Node, CaptureSessionMethodNames));
            return names;
        });

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            ProcessNode(statement, context, reportDiagnostics: true, capturingState, untrackableNames);
        }
    }

    private static void TrackObserverDeclarations(
        VariableDeclarationSyntax declaration,
        SemanticModel semanticModel,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            // The type test comes first so that the escape walks are forced only by a declaration that
            // is actually an observer, rather than by any local the member happens to declare.
            ILocalSymbol localSymbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(variable)!;
            if (!AnalyzerSymbolHelpers.IsLibraryTypeNamed(localSymbol.Type, "EventObserver"))
            {
                continue;
            }

            if (!untrackableNames.Value.Contains(variable.Identifier.ValueText))
            {
                capturingState[variable.Identifier.ValueText] = false;
            }
        }
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        // Walk the node's descendants in document order, checking each invocation against the tracked
        // capturing state. The walk does not descend into the bodies of nested functions: their code
        // runs when the delegate is invoked, not at its textual position. It also stops at if, switch, try
        // and loop statements — including one that is itself the root, which the barrier yields without
        // descending into — and processes them recursively below with a forked copy of the state for each
        // path through the statement.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax &&
            child is not TryStatementSyntax &&
            child is not ForStatementSyntax &&
            child is not CommonForEachStatementSyntax &&
            child is not WhileStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, reportDiagnostics, capturingState, untrackableNames);
            }
            else if (descendant is ForStatementSyntax forStatement)
            {
                ProcessLoop(GetForLoopPreamble(forStatement), forStatement.Incrementors, forStatement.Statement, context, reportDiagnostics, capturingState, untrackableNames);
            }
            else if (descendant is CommonForEachStatementSyntax forEachStatement)
            {
                ProcessLoop([forEachStatement.Expression], [], forEachStatement.Statement, context, reportDiagnostics, capturingState, untrackableNames);
            }
            else if (descendant is WhileStatementSyntax whileStatement)
            {
                ProcessLoop([whileStatement.Condition], [], whileStatement.Statement, context, reportDiagnostics, capturingState, untrackableNames);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, reportDiagnostics, capturingState, untrackableNames);
            }
            else if (descendant is TryStatementSyntax tryStatement)
            {
                ProcessTryStatement(tryStatement, context, reportDiagnostics, capturingState, untrackableNames);
            }
            else if (descendant is VariableDeclarationSyntax declaration)
            {
                TrackObserverDeclarations(declaration, context.SemanticModel, capturingState, untrackableNames);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, reportDiagnostics, capturingState);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, reportDiagnostics, capturingState, untrackableNames);

        Dictionary<string, bool> thenBranchState = new(capturingState);
        ProcessNode(ifStatement.Statement, context, reportDiagnostics, thenBranchState, untrackableNames);

        Dictionary<string, bool> elseBranchState = new(capturingState);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, reportDiagnostics, elseBranchState, untrackableNames);
        }

        // After the branch an observer counts as capturing only when every path through the branch
        // leaves a session active. This is the mirror image of the merge in BIDI020, and the polarity
        // follows from what is reported: a second start is a mistake only when a session is certainly
        // active already, so a conditional start must not condemn a later one.
        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = thenBranchState[observerName] && elseBranchState[observerName];
        }
    }

    private static void ProcessTryStatement(
        TryStatementSyntax tryStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        Dictionary<string, bool> tryState = new(capturingState);
        ProcessNode(tryStatement.Block, context, reportDiagnostics, tryState, untrackableNames);

        // A catch clause may begin after any prefix of the try block has run, so inside one a session counts
        // as certainly active only when every partial execution of the try leaves it active: the conjunction
        // of the state at try entry and the state after the whole try block. A StartCapturingTasks in the try
        // may not have run yet, and a StopCapturingTasks in it may already have run. This is the mirror image
        // of the same walk in BIDI020, as the if merge is.
        Dictionary<string, bool> partialTryState = [];
        foreach (string observerName in capturingState.Keys)
        {
            partialTryState[observerName] = capturingState[observerName] && tryState[observerName];
        }

        // The try block and each catch clause are the ways the statement can complete normally, and after it a
        // session counts as certainly active only when every one of them leaves it active.
        List<Dictionary<string, bool>> completionStates = [tryState];
        foreach (CatchClauseSyntax catchClause in tryStatement.Catches)
        {
            Dictionary<string, bool> catchState = new(partialTryState);
            if (catchClause.Filter is not null)
            {
                ProcessNode(catchClause.Filter.FilterExpression, context, reportDiagnostics, catchState, untrackableNames);
            }

            ProcessNode(catchClause.Block, context, reportDiagnostics, catchState, untrackableNames);
            completionStates.Add(catchState);
        }

        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = completionStates.All(completionState => completionState[observerName]);
        }

        // A finally block runs on every way out of the statement. The code in it is judged against a state that
        // allows for all of them: the try block or a catch clause completing, or an exception from any point in
        // the try. Only normal completion reaches the code after the statement, though, so the block is walked a
        // second time, from the completion state and without reporting, to find what it leaves there: a
        // StartCapturingTasks in a finally certainly opens a session for the code that follows.
        if (tryStatement.Finally is not null)
        {
            Dictionary<string, bool> finallyEntryState = [];
            foreach (string observerName in capturingState.Keys)
            {
                finallyEntryState[observerName] = capturingState[observerName] && partialTryState[observerName];
            }

            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics, finallyEntryState, untrackableNames);
            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics: false, capturingState, untrackableNames);
        }
    }

    private static void ProcessLoop(
        IEnumerable<SyntaxNode> preamble,
        IEnumerable<SyntaxNode> incrementors,
        StatementSyntax body,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        // A for loop's declaration or initializers, a foreach loop's collection expression, and the
        // loop condition all run before the first test of the condition, so they are walked against the
        // state as it stands. The body may never run — a false condition at once, an empty collection —
        // so it (and a for loop's incrementors, which run only after it) is walked as one path and the
        // state at loop entry kept as the other, exactly as an if statement without an else is:
        // a StartCapturingTasks inside the loop does not make a session
        // certainly active after it. A do…while loop runs its body at least once and is walked
        // straight through.
        foreach (SyntaxNode node in preamble)
        {
            ProcessNode(node, context, reportDiagnostics, capturingState, untrackableNames);
        }

        Dictionary<string, bool> bodyState = new(capturingState);
        ProcessNode(body, context, reportDiagnostics, bodyState, untrackableNames);
        foreach (SyntaxNode incrementor in incrementors)
        {
            ProcessNode(incrementor, context, reportDiagnostics, bodyState, untrackableNames);
        }

        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = capturingState[observerName] && bodyState[observerName];
        }
    }

    /// <summary>
    /// Gets the parts of a for statement that run before its body is first entered: its declaration or
    /// initializers and its condition, which is to say every child except the body and the incrementors.
    /// </summary>
    /// <param name="forStatement">The for statement.</param>
    /// <returns>The nodes that run unconditionally.</returns>
    private static IEnumerable<SyntaxNode> GetForLoopPreamble(ForStatementSyntax forStatement)
    {
        return forStatement.ChildNodes().Where(child => child != forStatement.Statement && !forStatement.Incrementors.Contains(child));
    }

    private static void ProcessSwitchStatement(
        SwitchStatementSyntax switchStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, reportDiagnostics, capturingState, untrackableNames);

        List<Dictionary<string, bool>> sectionStates = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionState = new(capturingState);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, reportDiagnostics, sectionState, untrackableNames);
            }

            sectionStates.Add(sectionState);
        }

        // As for if statements, an observer counts as capturing after the switch only when every path
        // through it leaves a session active. The unchanged state at the switch is the path taken when
        // no section matches and participates in the merge; when a default section makes that path
        // impossible, including it can only suppress a report, never create a false positive.
        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = capturingState[observerName] && sectionStates.All(sectionState => sectionState[observerName]);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
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

        string receiverName = receiverIdentifier.Identifier.ValueText;
        if (!capturingState.ContainsKey(receiverName))
        {
            return;
        }

        switch (memberAccess.Name.Identifier.ValueText)
        {
            case "StartCapturingTasks":
                if (reportDiagnostics && capturingState[receiverName])
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), receiverName));
                }

                capturingState[receiverName] = true;
                break;

            case "StopCapturingTasks":
                capturingState[receiverName] = false;
                break;

            case "WaitForCapturedTasksAsync":
            case "WaitForCapturedTasksCompleteAsync":
                // A wait that collects its full batch ends the session itself, and one that times out
                // leaves it open. Which of the two happened is a runtime outcome, so the session is no
                // longer certainly active and a later start is not reported.
                capturingState[receiverName] = false;
                break;
        }
    }
}
