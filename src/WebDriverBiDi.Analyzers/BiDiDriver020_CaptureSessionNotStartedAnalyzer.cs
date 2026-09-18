// <copyright file="BiDiDriver020_CaptureSessionNotStartedAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
        // Track whether StartCapturingTasks has been seen for each local EventObserver<T> variable.
        // Only locally-declared variables are tracked; parameter-passed observers are not.
        Dictionary<string, bool> capturingState = [];

        // An observer this member hands to other code, or one a nested function opens or closes a
        // session on, may have a session this walk cannot see. Its capturing state is therefore
        // unknown from the outset, so it is never tracked and never reported on. Collecting the names
        // up front, rather than at the point of escape, is what the Error severity of this rule
        // demands: the other code may run before or after the wait textually, and a wrong Error on
        // correct code is worse than a missed report.
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
            // ProcessNode registers observer declarations and checks observer method calls,
            // wherever in the statement's subtree they appear.
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
            // Track only an observer this walk can reason about from its first statement: one the
            // declaration itself obtains from AddObserver. An observer handed over by something the
            // walk cannot see into -- a factory or helper that may already have opened a capture
            // session -- would otherwise start out recorded as not capturing, and its first
            // WaitForCapturedTasksAsync would be reported although the session is open. The handle
            // this returns is already known to be an EventObserver, so the local's own type needs no
            // separate test. This shape test comes first so that the escape walks are forced only by
            // a declaration that is actually an observer.
            if (variable.Initializer?.Value is not InvocationExpressionSyntax initializer
                || AnalyzerSymbolHelpers.GetEventSubscriptionHandle(semanticModel, initializer) is not { MethodName: "AddObserver" })
            {
                continue;
            }

            if (untrackableNames.Value.Contains(variable.Identifier.ValueText))
            {
                continue;
            }

            capturingState[variable.Identifier.ValueText] = false;
        }
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        // Walk the node's descendants in document order, checking each invocation against the
        // tracked capturing state. The walk does not descend into the bodies of nested
        // functions (lambdas, anonymous methods, local functions): their code runs when the
        // delegate is invoked, not at its textual position, so a call there must not be
        // judged against the capturing state at that position. It also stops at if, switch,
        // try, for, foreach and while statements — including one that is itself the root, which
        // the barrier yields without descending into — and processes them recursively below
        // with a forked copy of the state for each path through the statement.
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
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, reportDiagnostics, capturingState, untrackableNames);
            }
            else if (descendant is TryStatementSyntax tryStatement)
            {
                ProcessTryStatement(tryStatement, context, reportDiagnostics, capturingState, untrackableNames);
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
            else if (descendant is VariableDeclarationSyntax declaration)
            {
                // Register observer declarations wherever they appear: a local declaration
                // statement, a using declaration, or the declaration of a classic
                // using (T x = ...) statement, including inside nested blocks such as try
                // statements. The pre-order walk visits the declaration before any later use
                // of the variable.
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

        // The branches are mutually exclusive, so each arm is walked against its own copy of
        // the state at the branch point: a StopCapturingTasks in one arm must not poison a
        // wait in the other. An else-if chain arrives here as an else clause whose statement
        // is itself an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchState = new(capturingState);
        ProcessNode(ifStatement.Statement, context, reportDiagnostics, thenBranchState, untrackableNames);

        Dictionary<string, bool> elseBranchState = new(capturingState);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, reportDiagnostics, elseBranchState, untrackableNames);
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
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, reportDiagnostics, capturingState, untrackableNames);

        // Sections are mutually exclusive in the same way if/else branches are.
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

    private static void ProcessTryStatement(
        TryStatementSyntax tryStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        Dictionary<string, bool> capturingState,
        Lazy<HashSet<string>> untrackableNames)
    {
        Dictionary<string, bool> tryState = new(capturingState);
        ProcessNode(tryStatement.Block, context, reportDiagnostics, tryState, untrackableNames);

        // A catch clause may begin after any prefix of the try block has run, so inside one an observer
        // counts as capturing when any partial execution of the try could leave it capturing: the
        // disjunction of the state at try entry and the state after the whole try block. A
        // StartCapturingTasks in the try may already have run, and a StopCapturingTasks in it may not have
        // run yet. This is the polarity of BIDI009's walk, for the same reason: this rule reports a wait
        // only when no path can have opened a session.
        Dictionary<string, bool> partialTryState = [];
        foreach (string observerName in capturingState.Keys)
        {
            partialTryState[observerName] = capturingState[observerName] || tryState[observerName];
        }

        // The try block and each catch clause are the ways the statement can complete normally, and after it
        // an observer counts as capturing when any of them leaves it capturing: a StopCapturingTasks in a
        // catch that rethrows must not condemn the wait after the statement.
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
            capturingState[observerName] = completionStates.Any(completionState => completionState[observerName]);
        }

        // A finally block runs on every way out of the statement. The code in it is judged against a state that
        // allows for all of them: the try block or a catch clause completing, or an exception from any point in
        // the try. Only normal completion reaches the code after the statement, though, so the block is walked a
        // second time, from the completion state and without reporting, to find what it leaves there: a
        // StopCapturingTasks in a finally certainly ends the session for the code that follows.
        if (tryStatement.Finally is not null)
        {
            Dictionary<string, bool> finallyEntryState = [];
            foreach (string observerName in capturingState.Keys)
            {
                finallyEntryState[observerName] = capturingState[observerName] || partialTryState[observerName];
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
        // A for loop's declaration or initializers, a foreach loop's collection expression, and the loop
        // condition all run before the first test of the condition, so they are walked against the state as it
        // stands. The body may never run, so it (and a for loop's incrementors, which run only after it) is
        // walked as one path and the state at loop entry kept as the other, exactly as an if statement without
        // an else is: a StopCapturingTasks inside the loop does not certainly end a session for the code after
        // it. A do…while loop runs its body at least once and is walked straight through.
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
            capturingState[observerName] = capturingState[observerName] || bodyState[observerName];
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

        string methodName = memberAccess.Name.Identifier.ValueText;
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
                if (reportDiagnostics && !capturingState[receiverName])
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), methodName, receiverName));
                }

                break;
        }
    }
}
