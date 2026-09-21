// <copyright file="BiDiDriver009_CommandExecutionBeforeStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects when commands are executed before StartAsync is called on a BiDiDriver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver009_CommandExecutionBeforeStartAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI009";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Commands executed before StartAsync";

    private static readonly LocalizableString MessageFormat = "Method '{0}' cannot be called before StartAsync() on the BiDiDriver. Call StartAsync() first to establish the connection.";

    private static readonly LocalizableString Description = "Commands cannot be executed before calling StartAsync() on the BiDiDriver. StartAsync() establishes the connection to the remote end, and all commands require an active connection. Attempting to execute commands before the driver has started will fail.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi009");

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
        SemanticModel semanticModel = context.SemanticModel;

        // Track BiDiDriver variables and whether StartAsync has been called
        Dictionary<string, bool> driverStartedStatus = [];

        // A driver that this method hands to something else may be started by that other code, which
        // this rule cannot see. Its started state is therefore unknown from the outset, so it is never
        // tracked and never reported on. Collecting the escaping names up front, rather than at the
        // point of escape, is what the Error severity of this rule demands: the helper that starts the
        // driver may be called before or after the command textually, and a wrong Error on correct
        // code is worse than a missed report. Collecting them walks the whole body and binds its invocations,
        // so it is deferred until the body is found to create a driver, which most bodies do not.
        Lazy<HashSet<string>> escapedNames = new(() => DriverStartStateWalker.FindDriversWithUnknownStartedState(context.Node, semanticModel), LazyThreadSafetyMode.None);

        // Walk through all statements in the method
        IEnumerable<StatementSyntax> statements = AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node);

        foreach (StatementSyntax statement in statements)
        {
            // ProcessNode registers driver declarations and checks driver method calls,
            // wherever in the statement's subtree they appear.
            ProcessNode(statement, context, reportDiagnostics: true, semanticModel, driverStartedStatus, escapedNames);
        }
    }

    private static void AnalyzeLocalDeclaration(
        VariableDeclarationSyntax declaration,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            if (variable.Initializer == null)
            {
                continue;
            }

            // Only a variable initialized directly with an object creation expression is
            // known to hold a driver that has not been started. A driver obtained any other
            // way (from a factory method call, an awaited task, a property, and so on) may
            // already have been started by the code that produced it, and the Error severity
            // of this rule demands certainty, so such variables are not tracked.
            if (variable.Initializer.Value is not BaseObjectCreationExpressionSyntax)
            {
                continue;
            }

            // A driver handed a Transport is started by connecting that transport, which this walk
            // never sees, so its state is not known here.
            if (AnalyzerSymbolHelpers.IsDriverConstructedFromTransport(semanticModel, variable.Initializer.Value))
            {
                continue;
            }

            ITypeSymbol? typeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value).Type;
            if (AnalyzerSymbolHelpers.IsCommandExecutorType(typeInfo) && !escapedNames.Value.Contains(variable.Identifier.ValueText))
            {
                driverStartedStatus[variable.Identifier.ValueText] = false;
            }
        }
    }

    /// <summary>
    /// Stops tracking every driver that at least one path through a branch stopped tracking, and
    /// reports the names that survive and can therefore be merged.
    /// </summary>
    /// <param name="driverStartedStatus">The state at the branch point, updated in place.</param>
    /// <param name="pathStatuses">The state at the end of each path through the branch.</param>
    /// <returns>The names still tracked on every path.</returns>
    /// <remarks>
    /// A path that rebound a variable to a driver the walk cannot see dropped it from that path's
    /// state, so after the branch its started state is unknown. Tracking stops for it, exactly as
    /// <see cref="TrackDriverAssignment"/> does on a straight-line path, and an untracked variable
    /// produces no reports at all, which is what this Error-severity rule wants when it cannot be
    /// certain. Reading the dropped key out of that path instead would throw, which is reported as
    /// AD0001 and suppresses this rule for the whole file.
    /// </remarks>
    private static List<string> DropDriversUntrackedOnAnyPath(
        Dictionary<string, bool> driverStartedStatus,
        IReadOnlyList<Dictionary<string, bool>> pathStatuses)
    {
        List<string> mergeableDriverNames = [];
        foreach (string driverName in driverStartedStatus.Keys.ToList())
        {
            if (pathStatuses.Any(pathStatus => !pathStatus.ContainsKey(driverName)))
            {
                driverStartedStatus.Remove(driverName);
                continue;
            }

            mergeableDriverNames.Add(driverName);
        }

        return mergeableDriverNames;
    }

    /// <summary>
    /// Updates the tracked state for an assignment to a driver variable the walk is following.
    /// </summary>
    /// <param name="assignment">The assignment to process.</param>
    /// <param name="driverStartedStatus">The tracked started state, keyed by variable name.</param>
    /// <remarks>
    /// Rebinding a tracked variable replaces the driver it names, and with it the history the walk has
    /// accumulated. A freshly constructed driver has not been started, so tracking continues against
    /// the new one. A driver from anywhere else — a factory method, an awaited task, a pool — may
    /// already have been started by whatever produced it, and this rule reports at Error severity, so
    /// tracking stops rather than judging the new driver against the old one's history. Only names the
    /// walk already tracks are considered, matching how a declaration starts the tracking.
    /// </remarks>
    private static void TrackDriverAssignment(
        AssignmentExpressionSyntax assignment,
        Dictionary<string, bool> driverStartedStatus)
    {
        if (assignment.Left is not IdentifierNameSyntax identifier ||
            !driverStartedStatus.ContainsKey(identifier.Identifier.ValueText))
        {
            return;
        }

        if (assignment.Right is BaseObjectCreationExpressionSyntax)
        {
            driverStartedStatus[identifier.Identifier.ValueText] = false;
            return;
        }

        driverStartedStatus.Remove(identifier.Identifier.ValueText);
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        // Walk the node's descendants in document order, checking each invocation against the
        // tracked started state. The walk does not descend into the bodies of nested functions
        // (lambdas, anonymous methods, local functions): their code runs when the delegate is
        // invoked, not at the textual position where it is declared — for example when an event
        // handler fires after the connection is started — so it must not be judged against the
        // driver's started state at this point in the method. It also stops at if, switch, try, for,
        // foreach and while statements — including one that is itself the root, which the barrier
        // yields without descending into — and processes them recursively below with a forked copy
        // of the state for each path through the statement.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            !AnalyzerSymbolHelpers.IsShortCircuitOperation(child) &&
            child is not ConditionalExpressionSyntax &&
            child is not SwitchExpressionSyntax &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax &&
            child is not TryStatementSyntax &&
            child is not ForStatementSyntax &&
            child is not CommonForEachStatementSyntax &&
            child is not WhileStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is TryStatementSyntax tryStatement)
            {
                ProcessTryStatement(tryStatement, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is ForStatementSyntax forStatement)
            {
                ProcessLoop(GetForLoopPreamble(forStatement), forStatement.Incrementors, forStatement.Statement, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is CommonForEachStatementSyntax forEachStatement)
            {
                ProcessLoop([forEachStatement.Expression], [], forEachStatement.Statement, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is WhileStatementSyntax whileStatement)
            {
                ProcessLoop([whileStatement.Condition], [], whileStatement.Statement, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is ConditionalExpressionSyntax conditional)
            {
                ProcessConditionalExpression(conditional, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is SwitchExpressionSyntax switchExpression)
            {
                ProcessSwitchExpression(switchExpression, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is BinaryExpressionSyntax shortCircuit && AnalyzerSymbolHelpers.IsShortCircuitOperation(shortCircuit))
            {
                ProcessShortCircuit(shortCircuit.Left, shortCircuit.Right, null, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is AssignmentExpressionSyntax coalesceAssignment && AnalyzerSymbolHelpers.IsShortCircuitOperation(coalesceAssignment))
            {
                ProcessShortCircuit(coalesceAssignment.Left, coalesceAssignment.Right, coalesceAssignment, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is VariableDeclarationSyntax declaration)
            {
                // Register driver declarations wherever they appear: a local declaration
                // statement, a using declaration, the declaration of a classic
                // using (T x = ...) statement, or a for initializer, including inside nested
                // blocks such as try statements. The pre-order walk visits the declaration
                // before any later use of the variable.
                AnalyzeLocalDeclaration(declaration, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is AssignmentExpressionSyntax assignment)
            {
                TrackDriverAssignment(assignment, driverStartedStatus);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, reportDiagnostics, semanticModel, driverStartedStatus);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);

        // The branches are mutually exclusive, so each arm is walked against its own copy of
        // the state at the branch point: a StopAsync in one arm must not poison a command in
        // the other. An else-if chain arrives here as an else clause whose statement is itself
        // an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchStatus = new(driverStartedStatus);
        Dictionary<string, bool> elseBranchStatus = new(driverStartedStatus);

        // A condition that tests IsStarted settles the driver's state inside each arm, as it does for
        // the walker-based lifecycle rules. Without this, a command written under the guard that makes
        // it safe -- `if (driver.IsStarted) { await driver.Session.StatusAsync(); }` -- is reported.
        if (DriverStartStateWalker.TryGetStartedStateTest(ifStatement.Condition, driverStartedStatus, out string guardedDriverName, out bool startedWhenConditionHolds))
        {
            thenBranchStatus[guardedDriverName] = startedWhenConditionHolds;
            elseBranchStatus[guardedDriverName] = !startedWhenConditionHolds;
        }

        ProcessNode(ifStatement.Statement, context, reportDiagnostics, semanticModel, thenBranchStatus, escapedNames);

        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, reportDiagnostics, semanticModel, elseBranchStatus, escapedNames);
        }

        // After the branch, a driver counts as not started only when every path through the
        // branch leaves it not started. This rule reports commands on a driver that has not
        // been started, so treating "started on some path only" as not started would flag
        // correct conditional stop/restart patterns with an Error-severity false positive;
        // the Error severity demands that the command fail on every path.
        foreach (string driverName in DropDriversUntrackedOnAnyPath(driverStartedStatus, [thenBranchStatus, elseBranchStatus]))
        {
            driverStartedStatus[driverName] = thenBranchStatus[driverName] || elseBranchStatus[driverName];
        }
    }

    private static void ProcessTryStatement(
        TryStatementSyntax tryStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        Dictionary<string, bool> entryStatus = new(driverStartedStatus);
        Dictionary<string, bool> tryStatus = new(driverStartedStatus);
        ProcessNode(tryStatement.Block, context, reportDiagnostics, semanticModel, tryStatus, escapedNames);

        // A catch clause (or a finally block) may begin executing after any prefix of the try block
        // has run, so inside one a driver counts as started when *any* partial execution of the try
        // could leave it started: it was started at entry, or the try contains a StartAsync that may
        // already have run. Reading the state after the full try walk instead would miss a try that
        // starts and then stops the driver, whose end state says nothing about the moment a catch is
        // entered.
        //
        // This is the mirror image of the same walk in BIDI024, which conjoins the two instead. The
        // polarity follows from what each rule reports: BIDI024 reports a *duplicate* start, so it
        // must be pessimistic about a driver being started; this rule reports a command on a driver
        // that was *never* started, so it must be optimistic. Both choices keep an Error-severity
        // diagnostic to cases that are certain on every path.
        //
        // A variable the try block rebound to a driver the walk cannot see is no longer tracked after
        // that walk, so its state inside a catch or a finally is unknown. It is left out here rather
        // than read out of a state that no longer holds it.
        Dictionary<string, bool> mightBeStartedStatus = [];
        foreach (string driverName in entryStatus.Keys)
        {
            if (tryStatus.ContainsKey(driverName))
            {
                bool everStarted = AnalyzerSymbolHelpers.ContainsCallOnVariable(tryStatement.Block, driverName, "StartAsync");
                mightBeStartedStatus[driverName] = entryStatus[driverName] || everStarted;
            }
        }

        // The try block and each catch clause are the ways the statement can complete normally. After it, a
        // driver counts as started when any of them leaves it started, matching how the if and switch merges
        // treat mutually exclusive branches: this rule reports only a driver that is not started on every
        // path, so a stop confined to one catch clause must not poison code that follows the statement.
        List<Dictionary<string, bool>> completionStatuses = [tryStatus];
        foreach (CatchClauseSyntax catchClause in tryStatement.Catches)
        {
            Dictionary<string, bool> catchStatus = new(mightBeStartedStatus);
            if (catchClause.Filter is not null)
            {
                ProcessNode(catchClause.Filter.FilterExpression, context, reportDiagnostics, semanticModel, catchStatus, escapedNames);
            }

            ProcessNode(catchClause.Block, context, reportDiagnostics, semanticModel, catchStatus, escapedNames);
            completionStatuses.Add(catchStatus);
        }

        foreach (string driverName in DropDriversUntrackedOnAnyPath(driverStartedStatus, completionStatuses))
        {
            driverStartedStatus[driverName] = completionStatuses.Any(completionStatus => completionStatus[driverName]);
        }

        // A finally block runs on every way out of the statement. The code in it is judged against a state that
        // allows for all of them: the try block or a catch clause completing, or an exception from any point in
        // the try. Only normal completion reaches the code after the statement, though, so the block is walked a
        // second time, from the completion state and without reporting, to find what it leaves there: a StopAsync
        // in a finally certainly leaves the driver stopped for the code that follows. Every driver still tracked
        // after the merge above was tracked through the try block, so it has a partial-execution state.
        if (tryStatement.Finally is not null)
        {
            Dictionary<string, bool> finallyEntryStatus = [];
            foreach (string driverName in driverStartedStatus.Keys)
            {
                finallyEntryStatus[driverName] = driverStartedStatus[driverName] || mightBeStartedStatus[driverName];
            }

            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics, semanticModel, finallyEntryStatus, escapedNames);
            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics: false, semanticModel, driverStartedStatus, escapedNames);
        }
    }

    private static void ProcessConditionalExpression(
        ConditionalExpressionSyntax conditional,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        // The condition is evaluated before either arm, and exactly one arm is evaluated after it: the shape of an
        // if statement with an else clause, forked and merged the same way: a driver counts as started after it when either arm may have started it.
        ProcessNode(conditional.Condition, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);

        Dictionary<string, bool> whenTrueStatus = new(driverStartedStatus);
        ProcessNode(conditional.WhenTrue, context, reportDiagnostics, semanticModel, whenTrueStatus, escapedNames);

        Dictionary<string, bool> whenFalseStatus = new(driverStartedStatus);
        ProcessNode(conditional.WhenFalse, context, reportDiagnostics, semanticModel, whenFalseStatus, escapedNames);

        foreach (string driverName in DropDriversUntrackedOnAnyPath(driverStartedStatus, [whenTrueStatus, whenFalseStatus]))
        {
            driverStartedStatus[driverName] = whenTrueStatus[driverName] || whenFalseStatus[driverName];
        }
    }

    private static void ProcessSwitchExpression(
        SwitchExpressionSyntax switchExpression,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        // The governing expression is evaluated before any arm, and the arms are mutually exclusive. Matching no arm
        // throws rather than continuing after the expression, so the arms are the only paths out; with no arms at
        // all nothing after the expression is reached, and the state is left alone.
        ProcessNode(switchExpression.GoverningExpression, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);

        List<Dictionary<string, bool>> armStatuses = [];
        foreach (SwitchExpressionArmSyntax arm in switchExpression.Arms)
        {
            Dictionary<string, bool> armStatus = new(driverStartedStatus);
            if (arm.WhenClause is not null)
            {
                ProcessNode(arm.WhenClause.Condition, context, reportDiagnostics, semanticModel, armStatus, escapedNames);
            }

            ProcessNode(arm.Expression, context, reportDiagnostics, semanticModel, armStatus, escapedNames);
            armStatuses.Add(armStatus);
        }

        if (armStatuses.Count > 0)
        {
            foreach (string driverName in DropDriversUntrackedOnAnyPath(driverStartedStatus, armStatuses))
            {
                driverStartedStatus[driverName] = armStatuses.Any(armStatus => armStatus[driverName]);
            }
        }
    }

    private static void ProcessShortCircuit(
        ExpressionSyntax left,
        ExpressionSyntax right,
        AssignmentExpressionSyntax? coalesceAssignment,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        // The left operand is always evaluated, and the right one only when the left does not settle the result
        // (&&, ||) or is null (??, ??=). The right operand is therefore walked as a path that may not run, as the
        // branch of an if statement without an else is, and a ??= assigns only on that path.
        ProcessNode(left, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);

        Dictionary<string, bool> rightStatus = new(driverStartedStatus);
        ProcessNode(right, context, reportDiagnostics, semanticModel, rightStatus, escapedNames);
        if (coalesceAssignment is not null)
        {
            TrackDriverAssignment(coalesceAssignment, rightStatus);
        }

        foreach (string driverName in DropDriversUntrackedOnAnyPath(driverStartedStatus, [rightStatus]))
        {
            driverStartedStatus[driverName] = driverStartedStatus[driverName] || rightStatus[driverName];
        }
    }

    private static void ProcessLoop(
        IEnumerable<SyntaxNode> preamble,
        IEnumerable<SyntaxNode> incrementors,
        StatementSyntax body,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        // A for loop's declaration or initializers, a foreach loop's collection expression, and the loop
        // condition all run before the first test of the condition, so they are walked against the state as
        // it stands. The body may never run, so it (and a for loop's incrementors, which run only after it) is
        // walked as one path and the state at loop entry kept as the other, exactly as an if statement without
        // an else is: a StopAsync inside the loop does not certainly stop the driver for the code after it,
        // and since this rule reports only a driver that is not started on every path, a StartAsync inside
        // the loop still counts. A do…while loop runs its body at least once and is walked straight through.
        foreach (SyntaxNode node in preamble)
        {
            ProcessNode(node, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);
        }

        Dictionary<string, bool> bodyStatus = new(driverStartedStatus);
        ProcessNode(body, context, reportDiagnostics, semanticModel, bodyStatus, escapedNames);
        foreach (SyntaxNode incrementor in incrementors)
        {
            ProcessNode(incrementor, context, reportDiagnostics, semanticModel, bodyStatus, escapedNames);
        }

        foreach (string driverName in DropDriversUntrackedOnAnyPath(driverStartedStatus, [bodyStatus]))
        {
            driverStartedStatus[driverName] = driverStartedStatus[driverName] || bodyStatus[driverName];
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
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        Lazy<HashSet<string>> escapedNames)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);

        // Sections are mutually exclusive in the same way if/else branches are.
        List<Dictionary<string, bool>> sectionStatuses = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionStatus = new(driverStartedStatus);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, reportDiagnostics, semanticModel, sectionStatus, escapedNames);
            }

            sectionStatuses.Add(sectionStatus);
        }

        // As for if statements, a driver counts as not started after the switch only when no
        // path through the switch leaves it started. The unchanged state at the switch (the
        // path taken when no section matches) participates in the merge alongside every
        // section; when a default section makes that path impossible, including it can only
        // suppress a report, never create a false positive, so default detection is not needed.
        foreach (string driverName in DropDriversUntrackedOnAnyPath(driverStartedStatus, sectionStatuses))
        {
            driverStartedStatus[driverName] = driverStartedStatus[driverName] || sectionStatuses.Any(sectionStatus => sectionStatus[driverName]);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus)
    {
        // Nothing to report until a driver variable is being tracked; skip the expensive semantic bind
        // for every invocation seen before the first driver is declared.
        if (driverStartedStatus.Count == 0)
        {
            return;
        }

        // Resolve the receiver before binding the invocation. The receiver walk starts from syntax and
        // gives up on anything that is not a one- or two-deep chain rooted in an identifier, so most of
        // the invocations in a file never reach a bind at all; binding first paid for every one of them.
        string? driverVariableName = GetDriverVariableNameFromInvocation(invocation, context.Node, semanticModel);
        if (driverVariableName == null || !driverStartedStatus.ContainsKey(driverVariableName))
        {
            return;
        }

        IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        string methodName = methodSymbol.Name;

        // If this is StartAsync, mark the driver as started
        if (methodName == "StartAsync" && AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            driverStartedStatus[driverVariableName] = true;
            return;
        }

        // If this is StopAsync, the driver is no longer started: commands issued after it,
        // without another StartAsync, fail at runtime with a connection exception.
        if (methodName == "StopAsync" && AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            driverStartedStatus[driverVariableName] = false;
            return;
        }

        // If the driver hasn't been started yet, check if this is a command that requires a connection
        if (reportDiagnostics && !driverStartedStatus[driverVariableName] && IsCommandMethod(methodSymbol))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string? GetDriverVariableNameFromInvocation(InvocationExpressionSyntax invocation, SyntaxNode body, SemanticModel semanticModel)
    {
        // A direct call on the driver (driver.ExecuteCommandAsync(...)) or a call on one of its modules
        // (driver.BrowsingContext.NavigateAsync(...)), through whatever wrappers the receiver carries
        // (driver!.ExecuteCommandAsync(...), driver?.BrowsingContext.NavigateAsync(...)).
        IdentifierNameSyntax? identifier = AnalyzerSymbolHelpers.GetMemberChainRoot(invocation.Expression, out int memberDepth);
        if (identifier is null || memberDepth > 2)
        {
            return null;
        }

        if (AnalyzerSymbolHelpers.IsCommandExecutorType(semanticModel.GetTypeInfo(identifier).Type))
        {
            return identifier.Identifier.ValueText;
        }

        // Call on a module held in a local: context.GetTreeAsync(), where the local was bound to
        // driver.BrowsingContext. Only a direct call can be made on such a local.
        return memberDepth == 1 ? AnalyzerSymbolHelpers.GetDriverOfModuleAlias(identifier, body, semanticModel) : null;
    }

    private static bool IsCommandMethod(IMethodSymbol method)
    {
        INamedTypeSymbol? containingType = method.ContainingType;

        // Check if this is ExecuteCommandAsync on BiDiDriver
        if (AnalyzerSymbolHelpers.IsCommandExecutorType(containingType) && method.Name == "ExecuteCommandAsync")
        {
            return true;
        }

        // Check if this is a command method on a Module
        if (IsModuleType(containingType) && IsModuleCommandMethod(method))
        {
            return true;
        }

        return false;
    }

    private static bool IsModuleCommandMethod(IMethodSymbol method)
    {
        // Module command methods typically:
        // 1. Return Task<T> where T is a CommandResult
        // 2. Are named with "Async" suffix
        if (!method.Name.EndsWith("Async", System.StringComparison.Ordinal))
        {
            return false;
        }

        ITypeSymbol? returnType = method.ReturnType;
        if (returnType is INamedTypeSymbol namedReturnType &&
            namedReturnType.Name == "Task" &&
            namedReturnType.IsGenericType &&
            namedReturnType.TypeArguments.Length == 1)
        {
            ITypeSymbol taskArgument = namedReturnType.TypeArguments[0];
            return InheritsFromCommandResult(taskArgument);
        }

        return false;
    }

    private static bool InheritsFromCommandResult(ITypeSymbol type)
    {
        INamedTypeSymbol? currentType = type as INamedTypeSymbol;
        while (currentType != null)
        {
            if (AnalyzerSymbolHelpers.IsLibraryTypeNamed(currentType, "CommandResult"))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    private static bool IsModuleType(INamedTypeSymbol? type)
    {
        // Check if the type inherits from Module
        INamedTypeSymbol? currentType = type!.BaseType;
        while (currentType != null)
        {
            if (AnalyzerSymbolHelpers.IsLibraryTypeNamed(currentType, "Module"))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}
