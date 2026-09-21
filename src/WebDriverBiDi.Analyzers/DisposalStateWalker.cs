// <copyright file="DisposalStateWalker.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Walks an executable body tracking, for each local of a disposable type the caller names, whether it
/// is disposed at the current point, and hands back every call that a disposed instance would answer
/// with <see cref="ObjectDisposedException"/>.
/// </summary>
/// <remarks>
/// Branches, loops, switches, try statements and using statements each walk against their own copy of
/// the state, so a call is reported only when disposal has happened on every path that reaches it.
/// BIDI029 tracks a <c>BiDiDriver</c> this way and BIDI038 an <c>EventObserver&lt;T&gt;</c>; the two
/// differ only in the rules they supply.
/// </remarks>
internal static class DisposalStateWalker
{
    /// <summary>
    /// Receives a call made on an instance that is disposed at that point.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="method">The method the invocation binds to.</param>
    /// <param name="variableName">The name of the variable the call is made through.</param>
    internal delegate void DisposedUseHandler(InvocationExpressionSyntax invocation, IMethodSymbol method, string variableName);

    /// <summary>
    /// Walks the executable body of the analysis context's node.
    /// </summary>
    /// <param name="context">The analysis context whose node is the body to walk.</param>
    /// <param name="rules">What the rule tracks, and which of its members throw once it is disposed.</param>
    /// <param name="handler">Receives every call made on a disposed instance.</param>
    internal static void Walk(SyntaxNodeAnalysisContext context, DisposalRules rules, DisposedUseHandler handler)
    {
        SemanticModel semanticModel = context.SemanticModel;

        // Tracks, for each local of the tracked type, whether it is disposed at the current point of the walk.
        Dictionary<string, bool> disposedStatus = [];

        // An instance whose disposal or rebinding this member cannot place in its own execution order is
        // never tracked. The names are collected for the whole member rather than at the point of
        // escape, as the Error severity demands: the nested function that disposes it may run before or
        // after the use textually, and a wrong Error on correct code is worse than a missed report. The
        // walk is deferred until a declaration asks for it, so a member that declares none never pays.
        Lazy<HashSet<string>> untrackableNames = new(() => FindUntrackableVariableNames(context.Node, semanticModel, rules));

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            ProcessNode(statement, context, reportDiagnostics: true, semanticModel, disposedStatus, untrackableNames, rules, handler);
        }
    }

    /// <summary>
    /// Collects the names of local variables whose disposal state this member cannot determine.
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns>The set of names that must not be tracked.</returns>
    private static HashSet<string> FindUntrackableVariableNames(SyntaxNode body, SemanticModel semanticModel, DisposalRules rules)
    {
        HashSet<string> untrackableNames = [];
        foreach (IdentifierNameSyntax identifier in AnalyzerSymbolHelpers.GetBodyDescendantNodes(body).OfType<IdentifierNameSyntax>())
        {
            if (IsPassedByReference(identifier, semanticModel, rules) || IsDisposedOrReboundInsideNestedFunction(identifier, body, rules))
            {
                untrackableNames.Add(identifier.Identifier.ValueText);
            }
        }

        return untrackableNames;
    }

    /// <summary>
    /// Determines whether a mention of a variable hands it to a method by reference, which lets that
    /// method rebind the caller's variable to a different driver.
    /// </summary>
    /// <param name="identifier">The mention of the variable.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns><see langword="true"/> if the variable is passed by reference; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Passing a driver by value cannot change which object the caller's variable names, and nothing the
    /// callee does can undo disposal, so an ordinary argument is not an escape for this rule — unlike
    /// BIDI009, where a callee starting the driver does change the state being tracked.
    /// </remarks>
    private static bool IsPassedByReference(IdentifierNameSyntax identifier, SemanticModel semanticModel, DisposalRules rules)
    {
        return identifier.Parent is ArgumentSyntax argument
            && !argument.RefKindKeyword.IsKind(SyntaxKind.None)
            && rules.IsTrackedType(semanticModel.GetTypeInfo(identifier).Type);
    }

    /// <summary>
    /// Determines whether a mention of a variable inside a nested function disposes it or rebinds it.
    /// </summary>
    /// <param name="identifier">The mention of the variable.</param>
    /// <param name="body">The member body being analyzed.</param>
    /// <returns><see langword="true"/> if a nested function can change the variable's disposal state; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A nested function runs when its delegate is invoked, not where it is written, so a
    /// <c>DisposeAsync</c> or an assignment inside one changes the driver's state at a point this rule's
    /// textual walk cannot place. A nested function that merely uses the driver leaves the state alone,
    /// and treating every capture as an escape would stop the rule reporting a genuine error elsewhere
    /// in the same member.
    /// </remarks>
    private static bool IsDisposedOrReboundInsideNestedFunction(IdentifierNameSyntax identifier, SyntaxNode body, DisposalRules rules)
    {
        bool disposesOrRebinds = identifier.Parent switch
        {
            AssignmentExpressionSyntax assignment => assignment.Left == identifier,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Expression == identifier
                && memberAccess.Parent is InvocationExpressionSyntax
                && rules.DisposalMethodNames.Contains(memberAccess.Name.Identifier.ValueText),
            _ => false,
        };

        if (!disposesOrRebinds)
        {
            return false;
        }

        // The identifier came from the body's descendants, so the body is always an ancestor and always
        // stops the walk.
        return identifier.Ancestors()
            .TakeWhile(ancestor => ancestor != body)
            .Any(ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);
    }

    private static void TrackDeclarations(
        VariableDeclarationSyntax declaration,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            // The type test comes first so that the escape walk is forced only by a declaration that
            // is actually a driver, as the sibling capture-session rules do.
            ILocalSymbol localSymbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(variable)!;
            if (rules.IsTrackedType(localSymbol.Type)
                && !untrackableNames.Value.Contains(variable.Identifier.ValueText))
            {
                disposedStatus[variable.Identifier.ValueText] = false;
            }
        }
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // Walk the node's descendants in document order, checking each invocation against the tracked
        // disposal state. The walk does not descend into the bodies of nested functions: their code runs
        // when the delegate is invoked, not at its textual position. It also stops at if, switch, try and
        // using statements — including one that is itself the root, which the barrier yields without
        // descending into — and processes them recursively below, so that each mutually exclusive branch
        // is walked against its own copy of the state and a using statement's disposal is applied after
        // its body rather than before it.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            !AnalyzerSymbolHelpers.IsShortCircuitOperation(child) &&
            child is not ConditionalExpressionSyntax &&
            child is not SwitchExpressionSyntax &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax &&
            child is not TryStatementSyntax &&
            child is not UsingStatementSyntax &&
            child is not ForStatementSyntax &&
            child is not CommonForEachStatementSyntax &&
            child is not WhileStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is ForStatementSyntax forStatement)
            {
                ProcessLoop(GetForLoopPreamble(forStatement), forStatement.Incrementors, forStatement.Statement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is CommonForEachStatementSyntax forEachStatement)
            {
                ProcessLoop([forEachStatement.Expression], [], forEachStatement.Statement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is WhileStatementSyntax whileStatement)
            {
                ProcessLoop([whileStatement.Condition], [], whileStatement.Statement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is TryStatementSyntax tryStatement)
            {
                ProcessTryStatement(tryStatement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is UsingStatementSyntax usingStatement)
            {
                ProcessUsingStatement(usingStatement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is ConditionalExpressionSyntax conditional)
            {
                ProcessConditionalExpression(conditional, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is SwitchExpressionSyntax switchExpression)
            {
                ProcessSwitchExpression(switchExpression, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is BinaryExpressionSyntax shortCircuit && AnalyzerSymbolHelpers.IsShortCircuitOperation(shortCircuit))
            {
                ProcessShortCircuit(shortCircuit.Left, shortCircuit.Right, null, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is AssignmentExpressionSyntax coalesceAssignment && AnalyzerSymbolHelpers.IsShortCircuitOperation(coalesceAssignment))
            {
                ProcessShortCircuit(coalesceAssignment.Left, coalesceAssignment.Right, coalesceAssignment, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is VariableDeclarationSyntax declaration)
            {
                TrackDeclarations(declaration, semanticModel, disposedStatus, untrackableNames, rules, handler);
            }
            else if (descendant is AssignmentExpressionSyntax assignment)
            {
                CheckAssignment(assignment, disposedStatus);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, reportDiagnostics, semanticModel, disposedStatus, rules, handler);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);

        Dictionary<string, bool> thenBranchStatus = new(disposedStatus);
        ProcessNode(ifStatement.Statement, context, reportDiagnostics, semanticModel, thenBranchStatus, untrackableNames, rules, handler);

        Dictionary<string, bool> elseBranchStatus = new(disposedStatus);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, reportDiagnostics, semanticModel, elseBranchStatus, untrackableNames, rules, handler);
        }

        // After the branch a driver counts as disposed only when every path through it disposed the
        // driver. This is the mirror image of the merge in BIDI009, and the polarity follows from what
        // is reported: a use is an error only if the driver is certainly disposed on every path that
        // reaches it, so a conditional dispose must not condemn the code that follows.
        foreach (string driverName in disposedStatus.Keys.ToList())
        {
            disposedStatus[driverName] = thenBranchStatus[driverName] && elseBranchStatus[driverName];
        }
    }

    private static void ProcessConditionalExpression(
        ConditionalExpressionSyntax conditional,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // The condition is evaluated before either arm, and exactly one arm is evaluated after it: the shape of an
        // if statement with an else clause, forked and merged the same way: a driver counts as disposed after it only when both arms dispose it.
        ProcessNode(conditional.Condition, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);

        Dictionary<string, bool> whenTrueStatus = new(disposedStatus);
        ProcessNode(conditional.WhenTrue, context, reportDiagnostics, semanticModel, whenTrueStatus, untrackableNames, rules, handler);

        Dictionary<string, bool> whenFalseStatus = new(disposedStatus);
        ProcessNode(conditional.WhenFalse, context, reportDiagnostics, semanticModel, whenFalseStatus, untrackableNames, rules, handler);

        foreach (string driverName in disposedStatus.Keys.ToList())
        {
            disposedStatus[driverName] = whenTrueStatus[driverName] && whenFalseStatus[driverName];
        }
    }

    private static void ProcessSwitchExpression(
        SwitchExpressionSyntax switchExpression,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // The governing expression is evaluated before any arm, and the arms are mutually exclusive. Matching no arm
        // throws rather than continuing after the expression, so the arms are the only paths out; with no arms at
        // all nothing after the expression is reached, and the state is left alone.
        ProcessNode(switchExpression.GoverningExpression, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);

        List<Dictionary<string, bool>> armStatuses = [];
        foreach (SwitchExpressionArmSyntax arm in switchExpression.Arms)
        {
            Dictionary<string, bool> armStatus = new(disposedStatus);
            if (arm.WhenClause is not null)
            {
                ProcessNode(arm.WhenClause.Condition, context, reportDiagnostics, semanticModel, armStatus, untrackableNames, rules, handler);
            }

            ProcessNode(arm.Expression, context, reportDiagnostics, semanticModel, armStatus, untrackableNames, rules, handler);
            armStatuses.Add(armStatus);
        }

        if (armStatuses.Count > 0)
        {
            foreach (string driverName in disposedStatus.Keys.ToList())
            {
                disposedStatus[driverName] = armStatuses.All(armStatus => armStatus[driverName]);
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
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // The left operand is always evaluated, and the right one only when the left does not settle the result
        // (&&, ||) or is null (??, ??=). The right operand is therefore walked as a path that may not run, as the
        // branch of an if statement without an else is, and a ??= assigns only on that path.
        ProcessNode(left, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);

        Dictionary<string, bool> rightStatus = new(disposedStatus);
        ProcessNode(right, context, reportDiagnostics, semanticModel, rightStatus, untrackableNames, rules, handler);
        if (coalesceAssignment is not null)
        {
            CheckAssignment(coalesceAssignment, rightStatus);
        }

        foreach (string driverName in disposedStatus.Keys.ToList())
        {
            disposedStatus[driverName] = disposedStatus[driverName] && rightStatus[driverName];
        }
    }

    private static void ProcessLoop(
        IEnumerable<SyntaxNode> preamble,
        IEnumerable<SyntaxNode> incrementors,
        StatementSyntax body,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // A for loop's declaration or initializers, a foreach loop's collection expression, and the
        // loop condition all run before the first test of the condition, so they are walked against the
        // state as it stands. The body may never run — a false condition at once, an empty collection —
        // so it (and a for loop's incrementors, which run only after it) is walked as one path and the
        // state at loop entry kept as the other, exactly as an if statement without an else is:
        // a DisposeAsync inside the loop does not make the driver
        // certainly disposed after it. A do…while loop runs its body at least once and is walked
        // straight through.
        foreach (SyntaxNode node in preamble)
        {
            ProcessNode(node, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);
        }

        Dictionary<string, bool> bodyStatus = new(disposedStatus);
        ProcessNode(body, context, reportDiagnostics, semanticModel, bodyStatus, untrackableNames, rules, handler);
        foreach (SyntaxNode incrementor in incrementors)
        {
            ProcessNode(incrementor, context, reportDiagnostics, semanticModel, bodyStatus, untrackableNames, rules, handler);
        }

        foreach (string driverName in disposedStatus.Keys.ToList())
        {
            disposedStatus[driverName] = disposedStatus[driverName] && bodyStatus[driverName];
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
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);

        List<Dictionary<string, bool>> sectionStatuses = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionStatus = new(disposedStatus);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, reportDiagnostics, semanticModel, sectionStatus, untrackableNames, rules, handler);
            }

            sectionStatuses.Add(sectionStatus);
        }

        // As for if statements, a driver counts as disposed after the switch only when every path
        // through it disposed the driver. The unchanged state at the switch is the path taken when no
        // section matches, and it participates in the merge; when a default section makes that path
        // impossible, including it can only suppress a report, never create a false positive.
        foreach (string driverName in disposedStatus.Keys.ToList())
        {
            disposedStatus[driverName] = disposedStatus[driverName] && sectionStatuses.All(sectionStatus => sectionStatus[driverName]);
        }
    }

    private static void ProcessTryStatement(
        TryStatementSyntax tryStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        Dictionary<string, bool> entryStatus = new(disposedStatus);
        Dictionary<string, bool> tryStatus = new(disposedStatus);
        ProcessNode(tryStatement.Block, context, reportDiagnostics, semanticModel, tryStatus, untrackableNames, rules, handler);

        // A catch clause (or a finally block) may begin executing after any prefix of the try block has
        // run, so inside one a driver counts as disposed only when every partial execution of the try
        // leaves it disposed: it was disposed at entry and the try cannot have replaced it with a fresh
        // driver. A DisposeAsync inside the try may not have run yet, so it cannot raise the state here,
        // and an assignment inside the try may have run, so it lowers it.
        Dictionary<string, bool> certainlyDisposedStatus = [];
        foreach (string driverName in entryStatus.Keys)
        {
            certainlyDisposedStatus[driverName] = entryStatus[driverName]
                && !AnalyzerSymbolHelpers.ContainsRebinding(tryStatement.Block, driverName);
        }

        // The try block and each catch clause are the ways the statement can complete normally, and after it a
        // driver counts as disposed only when every one of them leaves it disposed, matching how the if and
        // switch merges treat mutually exclusive branches.
        List<Dictionary<string, bool>> completionStatuses = [tryStatus];
        foreach (CatchClauseSyntax catchClause in tryStatement.Catches)
        {
            Dictionary<string, bool> catchStatus = new(certainlyDisposedStatus);
            if (catchClause.Filter is not null)
            {
                ProcessNode(catchClause.Filter.FilterExpression, context, reportDiagnostics, semanticModel, catchStatus, untrackableNames, rules, handler);
            }

            ProcessNode(catchClause.Block, context, reportDiagnostics, semanticModel, catchStatus, untrackableNames, rules, handler);
            completionStatuses.Add(catchStatus);
        }

        foreach (string driverName in disposedStatus.Keys.ToList())
        {
            disposedStatus[driverName] = completionStatuses.All(completionStatus => completionStatus[driverName]);
        }

        // A finally block runs on every way out of the statement. The code in it is judged against a state that
        // allows for all of them: the try block or a catch clause completing, or an exception from any point in
        // the try. Only normal completion reaches the code after the statement, though, so the block is walked a
        // second time, from the completion state and without reporting, to find what it leaves there: a DisposeAsync
        // in a finally certainly leaves the driver disposed for the code that follows.
        if (tryStatement.Finally is not null)
        {
            Dictionary<string, bool> finallyEntryStatus = [];
            foreach (string driverName in disposedStatus.Keys)
            {
                finallyEntryStatus[driverName] = disposedStatus[driverName] && certainlyDisposedStatus[driverName];
            }

            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics, semanticModel, finallyEntryStatus, untrackableNames, rules, handler);
            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics: false, semanticModel, disposedStatus, untrackableNames, rules, handler);
        }
    }

    private static void ProcessUsingStatement(
        UsingStatementSyntax usingStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        Lazy<HashSet<string>> untrackableNames,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // A classic using statement disposes its resource when the body finishes, so the body is walked
        // against the state as it stands and the disposal is applied afterwards. A using *declaration*
        // needs none of this: it disposes at the end of the enclosing block, after every statement the
        // walk can reach, and it arrives here as an ordinary variable declaration.
        if (usingStatement.Declaration is not null)
        {
            TrackDeclarations(usingStatement.Declaration, semanticModel, disposedStatus, untrackableNames, rules, handler);
        }

        ProcessNode(usingStatement.Statement, context, reportDiagnostics, semanticModel, disposedStatus, untrackableNames, rules, handler);

        // `await using (driver) { ... }` over an already-declared driver leaves it disposed for the
        // statements that follow. The variable a using statement declares itself goes out of scope here,
        // so marking it disposed can never report anything.
        if (usingStatement.Expression is IdentifierNameSyntax resourceIdentifier &&
            disposedStatus.ContainsKey(resourceIdentifier.Identifier.ValueText))
        {
            disposedStatus[resourceIdentifier.Identifier.ValueText] = true;
        }
    }

    private static void CheckAssignment(AssignmentExpressionSyntax assignment, Dictionary<string, bool> disposedStatus)
    {
        // Rebinding the variable makes it name a different driver, so whatever was disposed before is no
        // longer what the name refers to. The new driver's own state is unknown unless it is constructed
        // here, and treating it as not disposed is the conservative choice either way: this rule reports
        // only certain misuse.
        if (assignment.Left is IdentifierNameSyntax target && disposedStatus.ContainsKey(target.Identifier.ValueText))
        {
            disposedStatus[target.Identifier.ValueText] = false;
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> disposedStatus,
        DisposalRules rules,
        DisposedUseHandler handler)
    {
        // Nothing to report until a driver variable is being tracked; skip the expensive semantic bind
        // for every invocation seen before the first driver is declared.
        if (disposedStatus.Count == 0)
        {
            return;
        }

        // Resolve the receiver before binding the invocation. The receiver walk starts from syntax and
        // gives up on anything that is not a one- or two-deep chain rooted in an identifier, so most of
        // the invocations in a file never reach a bind at all; binding first paid for every one of them.
        string? variableName = rules.ResolveReceiverName(invocation, context.Node, semanticModel, out bool isDirectCall);
        if (variableName is null || !disposedStatus.ContainsKey(variableName))
        {
            return;
        }

        // A late-bound (dynamic) invocation resolves to no symbol at all, so there is nothing to classify.
        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Disposal is recognized by its receiver being the driver itself, not by the type declaring DisposeAsync: a
        // call through a cast, `((IAsyncDisposable)driver).DisposeAsync()`, binds to IAsyncDisposable's member but
        // disposes the same driver.
        if (isDirectCall && rules.DisposalMethodNames.Contains(methodSymbol.Name))
        {
            disposedStatus[variableName] = true;
            return;
        }

        if (reportDiagnostics && disposedStatus[variableName] && rules.ThrowsAfterDisposal(methodSymbol))
        {
            handler(invocation, methodSymbol, variableName);
        }
    }
}
