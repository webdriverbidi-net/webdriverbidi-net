// <copyright file="DriverStartStateWalker.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Walks an executable body (a method, a constructor, or a top-level program) tracking, for each
/// local driver variable, whether a <c>StartAsync</c> call is in effect at each textual position, and
/// hands every invocation made through a tracked driver to a callback together with that state.
/// </summary>
/// <remarks>
/// <para>
/// Shared by the rules that judge a driver call against the driver's started state: BIDI001, BIDI002
/// and BIDI003 (a registration on a started driver) and BIDI024 (a second start). All four report at
/// Error severity, so all four need the same conservative merge: after a branch, a driver counts as
/// started only when <em>every</em> path through the branch leaves it started. Treating "started on
/// some path" as started would report correct conditional start/stop code.
/// </para>
/// <para>
/// A driver variable is tracked from any declaration whose initializer has the driver type: a local
/// declaration statement, a <c>using</c> declaration, the declaration of a classic
/// <c>using (T x = ...)</c> statement, or a <c>for</c> initializer. The same name declared again in a
/// sibling scope (two <c>foreach</c> bodies, say) simply restarts the tracking. The walk does not
/// descend into nested functions (lambdas, anonymous methods, local functions): their code runs when
/// the delegate is invoked, not where it is written. <c>if</c>, <c>switch</c> and <c>try</c>
/// statements are walked with a forked copy of the state per mutually exclusive branch, and the body
/// of a <c>for</c>, <c>foreach</c> or <c>while</c> loop is walked as a path that may not run at all. A
/// <c>finally</c> block runs on every way out of its <c>try</c>, so what it does holds for the code after
/// the statement. The conditional and <c>switch</c> expressions are forked like the statements, and the
/// right operand of <c>&amp;&amp;</c>, <c>||</c>, <c>??</c> and <c>??=</c> is walked as a path that may not run.
/// </para>
/// </remarks>
internal sealed class DriverStartStateWalker
{
    private const string StartedPropertyName = "IsStarted";

    private readonly SyntaxNodeAnalysisContext context;
    private readonly Func<ITypeSymbol?, bool> isDriverType;
    private readonly DriverInvocationHandler handler;

    // Whether invocations are handed to the handler. Cleared while a finally block is walked a second time
    // to find the state it leaves for the code after its try statement, a walk whose invocations have already
    // been handed over by the first.
    private bool reportInvocations = true;

    private DriverStartStateWalker(SyntaxNodeAnalysisContext context, Func<ITypeSymbol?, bool> isDriverType, DriverInvocationHandler handler)
    {
        this.context = context;
        this.isDriverType = isDriverType;
        this.handler = handler;
    }

    /// <summary>
    /// Receives an invocation made through a tracked driver variable.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="method">The method the invocation binds to.</param>
    /// <param name="driverVariableName">The name of the driver variable the invocation is made through.</param>
    /// <param name="isStarted">
    /// <see langword="true"/> if a <c>StartAsync</c> call on the driver is in effect on every path that
    /// reaches the invocation; otherwise <see langword="false"/>. For a <c>StartAsync</c> invocation
    /// this is the state before the call takes effect.
    /// </param>
    /// <param name="isDirectDriverCall">
    /// <see langword="true"/> when the method is invoked on the driver variable itself
    /// (<c>driver.StartAsync()</c>); <see langword="false"/> when it is invoked on something reached
    /// through the driver (<c>driver.Session.SubscribeAsync()</c>). A rule whose subject is the
    /// driver's own lifecycle must require this; one that treats anything reached through the driver
    /// as part of its registration surface need not.
    /// </param>
    internal delegate void DriverInvocationHandler(InvocationExpressionSyntax invocation, IMethodSymbol method, string driverVariableName, bool isStarted, bool isDirectDriverCall);

    /// <summary>
    /// Walks the executable body of the analysis context's node.
    /// </summary>
    /// <param name="context">The analysis context whose node is the body to walk.</param>
    /// <param name="isDriverType">Determines whether a declaration's initializer type is a driver to track.</param>
    /// <param name="handler">Receives every invocation made through a tracked driver.</param>
    internal static void Walk(SyntaxNodeAnalysisContext context, Func<ITypeSymbol?, bool> isDriverType, DriverInvocationHandler handler)
    {
        DriverStartStateWalker walker = new(context, isDriverType, handler);
        Dictionary<string, bool> driverStartedStatus = [];
        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            walker.ProcessNode(statement, driverStartedStatus);
        }
    }

    /// <summary>
    /// Gets the identifier at the root of a member access chain: <c>driver</c> for
    /// <c>driver.Session.StartAsync</c>.
    /// </summary>
    /// <param name="expression">The receiver expression of a member access.</param>
    /// <returns>The root identifier's name, or <see langword="null"/> when the chain does not root in a simple identifier.</returns>
    internal static string? GetRootIdentifierName(ExpressionSyntax expression)
    {
        ExpressionSyntax current = expression;
        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            current = memberAccess.Expression;
        }

        return (current as IdentifierNameSyntax)?.Identifier.ValueText;
    }

    private void ProcessNode(SyntaxNode node, Dictionary<string, bool> driverStartedStatus)
    {
        // Walk the node's descendants in document order. The walk stops at every branching construct
        // — the if, switch and try statements, the conditional and switch expressions, the operators
        // whose right operand may not be evaluated, and the loops whose body may not run, including one
        // that is itself the root, which the barrier yields without descending into — and processes each
        // recursively below with a forked copy of the state for each path.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            !AnalyzerSymbolHelpers.IsShortCircuitOperation(child) &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax &&
            child is not TryStatementSyntax &&
            child is not ConditionalExpressionSyntax &&
            child is not SwitchExpressionSyntax &&
            child is not ForStatementSyntax &&
            child is not CommonForEachStatementSyntax &&
            child is not WhileStatementSyntax))
        {
            switch (descendant)
            {
                case IfStatementSyntax ifStatement:
                    this.ProcessIfStatement(ifStatement, driverStartedStatus);
                    break;

                case ForStatementSyntax forStatement:
                    this.ProcessLoop(GetForLoopPreamble(forStatement), forStatement.Incrementors, forStatement.Statement, driverStartedStatus);
                    break;

                case CommonForEachStatementSyntax forEachStatement:
                    this.ProcessLoop([forEachStatement.Expression], [], forEachStatement.Statement, driverStartedStatus);
                    break;

                case WhileStatementSyntax whileStatement:
                    this.ProcessLoop([whileStatement.Condition], [], whileStatement.Statement, driverStartedStatus);
                    break;

                case SwitchStatementSyntax switchStatement:
                    this.ProcessSwitchStatement(switchStatement, driverStartedStatus);
                    break;

                case TryStatementSyntax tryStatement:
                    this.ProcessTryStatement(tryStatement, driverStartedStatus);
                    break;

                case ConditionalExpressionSyntax conditional:
                    this.ProcessConditionalExpression(conditional, driverStartedStatus);
                    break;

                case SwitchExpressionSyntax switchExpression:
                    this.ProcessSwitchExpression(switchExpression, driverStartedStatus);
                    break;

                case BinaryExpressionSyntax shortCircuit when AnalyzerSymbolHelpers.IsShortCircuitOperation(shortCircuit):
                    this.ProcessShortCircuit(shortCircuit.Left, shortCircuit.Right, null, driverStartedStatus);
                    break;

                case AssignmentExpressionSyntax coalesceAssignment when AnalyzerSymbolHelpers.IsShortCircuitOperation(coalesceAssignment):
                    this.ProcessShortCircuit(coalesceAssignment.Left, coalesceAssignment.Right, coalesceAssignment, driverStartedStatus);
                    break;

                case AssignmentExpressionSyntax assignment:
                    TrackDriverAssignment(assignment, driverStartedStatus);
                    break;

                case VariableDeclarationSyntax declaration:
                    // Register driver declarations wherever they appear; the pre-order walk visits
                    // the declaration before any later use of the variable, and before the
                    // invocations inside its own initializer.
                    this.TrackDriverDeclarations(declaration, driverStartedStatus);
                    break;

                case InvocationExpressionSyntax invocation:
                    this.CheckInvocation(invocation, driverStartedStatus);
                    break;
            }
        }
    }

    private void TrackDriverDeclarations(VariableDeclarationSyntax declaration, Dictionary<string, bool> driverStartedStatus)
    {
        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            if (variable.Initializer is null)
            {
                continue;
            }

            ITypeSymbol? initializerType = this.context.SemanticModel.GetTypeInfo(variable.Initializer.Value).Type;
            if (this.isDriverType(initializerType))
            {
                driverStartedStatus[variable.Identifier.ValueText] = false;
            }
        }
    }

    private static void TrackDriverAssignment(AssignmentExpressionSyntax assignment, Dictionary<string, bool> driverStartedStatus)
    {
        // Assigning to a tracked variable replaces the driver it names, and with it the started state
        // the walk has accumulated. Keeping the old state would judge calls on the new driver against
        // the old one's history, which is how an Error-severity rule ends up reporting correct code.
        if (assignment.Left is not IdentifierNameSyntax identifier ||
            !driverStartedStatus.ContainsKey(identifier.Identifier.ValueText))
        {
            return;
        }

        if (assignment.Right is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax)
        {
            // A freshly constructed driver has not been started.
            driverStartedStatus[identifier.Identifier.ValueText] = false;
        }
        else
        {
            // The driver came from somewhere the walk cannot see, such as a factory method or a
            // parameter, so its started state is unknown. Tracking stops rather than guessing: an
            // untracked variable produces no reports at all.
            driverStartedStatus.Remove(identifier.Identifier.ValueText);
        }
    }

    /// <summary>
    /// Determines whether a condition tests the started state of a tracked driver, so that each arm of
    /// the branch can be walked with the state the condition establishes for it.
    /// </summary>
    /// <param name="condition">The condition of the branch.</param>
    /// <param name="driverStartedStatus">The drivers being tracked at the branch point.</param>
    /// <param name="driverVariableName">When this method returns <see langword="true"/>, the driver the condition tests.</param>
    /// <param name="startedWhenConditionHolds">When this method returns <see langword="true"/>, the driver's state where the condition holds.</param>
    /// <returns><see langword="true"/> if the condition tests a tracked driver's started state; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Only <c>driver.IsStarted</c> and <c>!driver.IsStarted</c> are recognized. A compound condition
    /// establishes nothing on its own -- the other operand may decide the branch -- and leaving it
    /// unrecognized keeps the walk conservative, which for these Error-severity rules means carrying
    /// the pre-branch state in rather than inventing one.
    /// </remarks>
    private static bool TryGetStartedStateTest(ExpressionSyntax condition, Dictionary<string, bool> driverStartedStatus, out string driverVariableName, out bool startedWhenConditionHolds)
    {
        driverVariableName = string.Empty;
        startedWhenConditionHolds = true;

        ExpressionSyntax expression = condition;
        // The operator is not tested: the operand below must be the bool-typed IsStarted, and logical
        // negation is the only prefix unary operator defined over a bool, so any prefix unary that
        // reaches the test below is one. A prefix unary over anything else fails that test anyway.
        if (expression is PrefixUnaryExpressionSyntax logicalNot)
        {
            startedWhenConditionHolds = false;
            expression = logicalNot.Operand;
        }

        // The receiver has to be a bare identifier the walk is tracking. A driver reached any other
        // way (a field, a property, an element of a collection) is not tracked in the first place, so
        // there is no state to seed for it.
        if (expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: StartedPropertyName } memberAccess
            && memberAccess.Expression is IdentifierNameSyntax driverIdentifier
            && driverStartedStatus.ContainsKey(driverIdentifier.Identifier.ValueText))
        {
            driverVariableName = driverIdentifier.Identifier.ValueText;
            return true;
        }

        return false;
    }

    private void ProcessIfStatement(IfStatementSyntax ifStatement, Dictionary<string, bool> driverStartedStatus)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        this.ProcessNode(ifStatement.Condition, driverStartedStatus);

        // The branches are mutually exclusive, so each arm is walked against its own copy of the
        // state at the branch point. An else-if chain arrives here as an else clause whose statement
        // is itself an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchStatus = new(driverStartedStatus);
        Dictionary<string, bool> elseBranchStatus = new(driverStartedStatus);

        // A condition that tests IsStarted settles the driver's state inside each arm, so seed the
        // forks from it. Carrying the state from before the test into both arms instead would report
        // the recovery the driver documents -- after a remote disconnect IsStarted is false and
        // StartAsync proceeds -- as a duplicate start, and would likewise reject a registration made
        // in the arm where the driver is known to be stopped.
        if (TryGetStartedStateTest(ifStatement.Condition, driverStartedStatus, out string guardedDriverName, out bool startedWhenConditionHolds))
        {
            thenBranchStatus[guardedDriverName] = startedWhenConditionHolds;
            elseBranchStatus[guardedDriverName] = !startedWhenConditionHolds;
        }

        this.ProcessNode(ifStatement.Statement, thenBranchStatus);

        if (ifStatement.Else is not null)
        {
            this.ProcessNode(ifStatement.Else.Statement, elseBranchStatus);
        }

        MergeAllPaths(driverStartedStatus, [thenBranchStatus, elseBranchStatus]);
    }

    private void ProcessLoop(
        IEnumerable<SyntaxNode> preamble,
        IEnumerable<SyntaxNode> incrementors,
        StatementSyntax body,
        Dictionary<string, bool> driverStartedStatus)
    {
        // What runs before the first test of the loop condition runs unconditionally: a for loop's
        // declaration or initializers, a foreach loop's collection expression, and the condition
        // itself, which is evaluated at least once. The body is another matter: a for or while loop
        // whose condition is false at once, or a foreach over an empty collection, never enters it.
        // The body (and a for loop's incrementors, which run only after it) is therefore walked as one
        // path and the state at loop entry kept as the other, exactly as an if statement without an
        // else is, so that a StartAsync inside the loop does not count as having certainly run for the
        // code after it. A do…while loop is not routed here: its body runs at least once, so it is
        // walked straight through.
        foreach (SyntaxNode node in preamble)
        {
            this.ProcessNode(node, driverStartedStatus);
        }

        Dictionary<string, bool> bodyStatus = new(driverStartedStatus);
        this.ProcessNode(body, bodyStatus);
        foreach (SyntaxNode incrementor in incrementors)
        {
            this.ProcessNode(incrementor, bodyStatus);
        }

        MergeAllPaths(driverStartedStatus, [bodyStatus, new Dictionary<string, bool>(driverStartedStatus)]);
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

    private void ProcessConditionalExpression(ConditionalExpressionSyntax conditional, Dictionary<string, bool> driverStartedStatus)
    {
        // The condition is evaluated before either arm, and exactly one arm is evaluated after it.
        // That is the same shape as an if statement with an else clause, and it is forked the same
        // way: without the fork, a StartAsync in one arm would count as started on both paths.
        this.ProcessNode(conditional.Condition, driverStartedStatus);

        Dictionary<string, bool> whenTrueStatus = new(driverStartedStatus);
        this.ProcessNode(conditional.WhenTrue, whenTrueStatus);

        Dictionary<string, bool> whenFalseStatus = new(driverStartedStatus);
        this.ProcessNode(conditional.WhenFalse, whenFalseStatus);

        MergeAllPaths(driverStartedStatus, [whenTrueStatus, whenFalseStatus]);
    }

    private void ProcessSwitchExpression(SwitchExpressionSyntax switchExpression, Dictionary<string, bool> driverStartedStatus)
    {
        // The governing expression is evaluated before any arm, and the arms are mutually exclusive.
        // Unlike a switch statement, matching no arm does not fall through to the code after the
        // expression: it throws. The arms are therefore the only paths out, except when there are no
        // arms at all, in which case nothing after the expression is reached and the state at the
        // expression is left alone.
        this.ProcessNode(switchExpression.GoverningExpression, driverStartedStatus);

        List<Dictionary<string, bool>> armStatuses = [];
        foreach (SwitchExpressionArmSyntax arm in switchExpression.Arms)
        {
            Dictionary<string, bool> armStatus = new(driverStartedStatus);
            if (arm.WhenClause is not null)
            {
                this.ProcessNode(arm.WhenClause.Condition, armStatus);
            }

            this.ProcessNode(arm.Expression, armStatus);
            armStatuses.Add(armStatus);
        }

        if (armStatuses.Count > 0)
        {
            MergeAllPaths(driverStartedStatus, armStatuses);
        }
    }

    private void ProcessShortCircuit(ExpressionSyntax left, ExpressionSyntax right, AssignmentExpressionSyntax? coalesceAssignment, Dictionary<string, bool> driverStartedStatus)
    {
        // The left operand is always evaluated, and the right one only when the left does not settle the result
        // (&&, ||) or is null (??, ??=). The right operand is therefore walked as a path that may not run, as the
        // branch of an if statement without an else is, and a ??= assigns only on that path.
        this.ProcessNode(left, driverStartedStatus);

        Dictionary<string, bool> rightStatus = new(driverStartedStatus);
        this.ProcessNode(right, rightStatus);
        if (coalesceAssignment is not null)
        {
            TrackDriverAssignment(coalesceAssignment, rightStatus);
        }

        MergeAllPaths(driverStartedStatus, [rightStatus, new Dictionary<string, bool>(driverStartedStatus)]);
    }

    private void ProcessSwitchStatement(SwitchStatementSyntax switchStatement, Dictionary<string, bool> driverStartedStatus)
    {
        // The governing expression executes unconditionally, before any section.
        this.ProcessNode(switchStatement.Expression, driverStartedStatus);

        // Sections are mutually exclusive in the same way if/else branches are. When no default
        // section exists, the switch may match nothing, so the unchanged state at the switch is
        // one of the possible paths.
        bool hasDefaultSection = false;
        List<Dictionary<string, bool>> sectionStatuses = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            if (section.Labels.Any(label => label is DefaultSwitchLabelSyntax))
            {
                hasDefaultSection = true;
            }

            Dictionary<string, bool> sectionStatus = new(driverStartedStatus);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                this.ProcessNode(sectionStatement, sectionStatus);
            }

            sectionStatuses.Add(sectionStatus);
        }

        if (!hasDefaultSection)
        {
            sectionStatuses.Add(new Dictionary<string, bool>(driverStartedStatus));
        }

        MergeAllPaths(driverStartedStatus, sectionStatuses);
    }

    private void ProcessTryStatement(TryStatementSyntax tryStatement, Dictionary<string, bool> driverStartedStatus)
    {
        Dictionary<string, bool> entryStatus = new(driverStartedStatus);
        Dictionary<string, bool> tryStatus = new(driverStartedStatus);
        this.ProcessNode(tryStatement.Block, tryStatus);

        // A catch clause (or a finally block) may begin executing after any prefix of the try
        // block has run, so inside one a driver counts as started only when every partial
        // execution of the try leaves it started. That is the conjunction of the state at try
        // entry and the state after the full try walk: a StartAsync inside the try may not
        // have run yet (started at entry is false), and a StopAsync inside the try may
        // already have run (started after the try is false). Judging catch and finally code
        // against this conjunction keeps an Error-severity diagnostic to statically certain
        // cases: a registration or a StartAsync retry in a catch after a failed StartAsync in
        // the try is not reported (the library rolls the driver back to not-started when a
        // start fails), while the same call in a catch on a driver that was already started
        // before the try (with nothing in the try stopping it) still is.
        //
        // A variable the try block rebound to something the walk cannot see is no longer tracked
        // after that walk, so its state inside a catch or a finally is unknown. It is left out of
        // the conservative status rather than read out of a state that no longer holds it.
        Dictionary<string, bool> conservativeStatus = [];
        foreach (string driverName in entryStatus.Keys)
        {
            if (tryStatus.TryGetValue(driverName, out bool startedAfterTryBlock))
            {
                conservativeStatus[driverName] = entryStatus[driverName] && startedAfterTryBlock;
            }
        }

        // The try block and each catch clause are the ways the statement can complete normally, and after it a
        // driver counts as started only when every one of them leaves it started.
        List<Dictionary<string, bool>> completionStatuses = [tryStatus];
        foreach (CatchClauseSyntax catchClause in tryStatement.Catches)
        {
            Dictionary<string, bool> catchStatus = new(conservativeStatus);
            if (catchClause.Filter is not null)
            {
                this.ProcessNode(catchClause.Filter.FilterExpression, catchStatus);
            }

            this.ProcessNode(catchClause.Block, catchStatus);
            completionStatuses.Add(catchStatus);
        }

        MergeAllPaths(driverStartedStatus, completionStatuses);

        // A finally block runs on every way out of the statement. The code in it is judged against a state that
        // allows for all of them: the try block or a catch clause completing, or an exception from any point in
        // the try. Only normal completion reaches the code after the statement, though, so the block is walked a
        // second time, from the completion state and without reporting, to find what it leaves there: a StartAsync
        // in a finally certainly leaves the driver started for the code that follows, and a StopAsync there
        // certainly leaves it stopped.
        if (tryStatement.Finally is not null)
        {
            Dictionary<string, bool> finallyEntryStatus = new(driverStartedStatus);
            MergeAllPaths(finallyEntryStatus, [new Dictionary<string, bool>(driverStartedStatus), conservativeStatus]);
            this.ProcessNode(tryStatement.Finally.Block, finallyEntryStatus);

            bool reportInvocationsOnEntry = this.reportInvocations;
            this.reportInvocations = false;
            try
            {
                this.ProcessNode(tryStatement.Finally.Block, driverStartedStatus);
            }
            finally
            {
                this.reportInvocations = reportInvocationsOnEntry;
            }
        }
    }

    private void CheckInvocation(InvocationExpressionSyntax invocation, Dictionary<string, bool> driverStartedStatus)
    {
        // Nothing to do until a driver variable is being tracked; skip the semantic bind for every
        // invocation seen before the first driver is declared.
        if (driverStartedStatus.Count == 0 || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Only a call whose receiver chain roots in a tracked driver variable matters; the receiver's
        // type was checked when the variable was declared. Resolving the name first keeps the
        // expensive semantic bind to calls that can affect a tracked driver.
        string? driverVariableName = GetRootIdentifierName(memberAccess.Expression);
        if (driverVariableName is null || !driverStartedStatus.TryGetValue(driverVariableName, out bool started))
        {
            return;
        }

        if (this.context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return;
        }

        // Whether the call is on the driver itself rather than on something reached through it. The
        // receiver chain only has to *root* in a tracked driver for the call to arrive here, so
        // driver.Session.SubscribeAsync() and driver.Tracing.StartAsync() both do.
        bool isDirectDriverCall = memberAccess.Expression is IdentifierNameSyntax receiverIdentifier
            && receiverIdentifier.Identifier.ValueText == driverVariableName;

        if (this.reportInvocations)
        {
            this.handler(invocation, method, driverVariableName, started, isDirectDriverCall);
        }

        // StartAsync puts the driver in the started state; StopAsync returns it to the not-started
        // state, in which the runtime permits registration and a new start again.
        //
        // Only a call on the driver itself moves this state. A method of the same name on something
        // the driver exposes is a different lifecycle: a custom module with a command named StartAsync
        // (driver.Tracing.StartAsync()) would otherwise mark the driver started, making its real
        // StartAsync a reported duplicate, and a module's StopAsync would re-open registration while
        // the driver is running. The test is on the receiver rather than on the method's containing
        // type, because a custom driver type implementing the library's interfaces declares StartAsync
        // itself, and that call must still count.
        if (!isDirectDriverCall)
        {
            return;
        }

        if (method.Name == "StartAsync")
        {
            driverStartedStatus[driverVariableName] = true;
        }
        else if (method.Name == "StopAsync")
        {
            driverStartedStatus[driverVariableName] = false;
        }
    }

    private static void MergeAllPaths(Dictionary<string, bool> driverStartedStatus, List<Dictionary<string, bool>> pathStatuses)
    {
        // After a branch, a driver counts as started only when every path through it leaves the
        // driver started. A driver declared inside one path is scoped to that path, so only the
        // drivers known at the branch point are merged.
        //
        // A path that rebound the variable to something the walk cannot see — a factory call or an
        // awaited expression — dropped it from that path's state, so after the branch its started
        // state is unknown. Tracking stops for it, exactly as TrackDriverAssignment does on a
        // straight-line path, and an untracked variable produces no reports at all. Reading the
        // dropped key out of that path instead would throw, taking down every rule that shares this
        // walk for the whole body being analyzed.
        foreach (string driverName in driverStartedStatus.Keys.ToList())
        {
            if (pathStatuses.Any(pathStatus => !pathStatus.ContainsKey(driverName)))
            {
                driverStartedStatus.Remove(driverName);
                continue;
            }

            driverStartedStatus[driverName] = pathStatuses.All(pathStatus => pathStatus[driverName]);
        }
    }
}
