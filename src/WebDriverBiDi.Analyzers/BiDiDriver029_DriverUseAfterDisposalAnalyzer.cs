// <copyright file="BiDiDriver029_DriverUseAfterDisposalAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects a <c>BiDiDriver</c> being used after it has been disposed.
/// </summary>
/// <remarks>
/// Disposal is terminal: <c>BiDiDriver.DisposeAsync</c> marks the driver disposed before it tears
/// anything down, and every member that guards on that state throws
/// <see cref="System.ObjectDisposedException"/> from then on. A disposed driver cannot be restarted, so
/// the only remedy is a new driver.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver029_DriverUseAfterDisposalAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI029";

    private const string Category = "Usage";

    /// <summary>
    /// The members of <c>BiDiDriver</c> that throw once the driver is disposed. Every one of them
    /// begins with a disposal guard; the members deliberately absent from this set do not.
    /// <c>StopAsync</c> is a safe no-op on a disposed driver (its teardown finds the transport already
    /// disconnected), and <c>DisposeAsync</c> is idempotent, so neither is reported.
    /// </summary>
    private static readonly HashSet<string> DisposalGuardedDriverMethods =
    [
        "StartAsync",
        "ExecuteCommandAsync",
        "RegisterEvent",
        "RegisterModule",
        "GetModule",
        "RegisterTypeInfoResolverAsync",
    ];

    private static readonly LocalizableString Title = "Driver used after disposal";

    private static readonly LocalizableString MessageFormat = "'{0}' is called on '{1}' after it has been disposed. A disposed BiDiDriver cannot be reused; create a new one instead.";

    private static readonly LocalizableString Description = "A BiDiDriver marks itself disposed before releasing anything, and its command execution, registration, and start members throw ObjectDisposedException from that point on. Disposal is terminal: the driver cannot be restarted, so code that needs a connection after disposing must construct a new driver.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi029");

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

        // Tracks, for each local driver variable, whether it is disposed at the current point of the walk.
        Dictionary<string, bool> driverDisposedStatus = [];

        // A driver whose disposal or rebinding this member cannot place in its own execution order is
        // never tracked. Collecting those names up front, rather than at the point of escape, is what
        // the Error severity demands: the nested function that disposes the driver may run before or
        // after the use textually, and a wrong Error on correct code is worse than a missed report.
        HashSet<string> untrackableNames = FindUntrackableVariableNames(context.Node, semanticModel);

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            ProcessNode(statement, context, reportDiagnostics: true, semanticModel, driverDisposedStatus, untrackableNames);
        }
    }

    /// <summary>
    /// Collects the names of local variables whose disposal state this member cannot determine.
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns>The set of names that must not be tracked.</returns>
    private static HashSet<string> FindUntrackableVariableNames(SyntaxNode body, SemanticModel semanticModel)
    {
        HashSet<string> untrackableNames = [];
        foreach (IdentifierNameSyntax identifier in body.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (IsPassedByReference(identifier, semanticModel) || IsDisposedOrReboundInsideNestedFunction(identifier, body))
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
    private static bool IsPassedByReference(IdentifierNameSyntax identifier, SemanticModel semanticModel)
    {
        return identifier.Parent is ArgumentSyntax argument
            && !argument.RefKindKeyword.IsKind(SyntaxKind.None)
            && AnalyzerSymbolHelpers.IsCommandExecutorType(semanticModel.GetTypeInfo(identifier).Type);
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
    private static bool IsDisposedOrReboundInsideNestedFunction(IdentifierNameSyntax identifier, SyntaxNode body)
    {
        bool disposesOrRebinds = identifier.Parent switch
        {
            AssignmentExpressionSyntax assignment => assignment.Left == identifier,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Expression == identifier
                && memberAccess.Parent is InvocationExpressionSyntax
                && memberAccess.Name.Identifier.ValueText == "DisposeAsync",
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

    private static void TrackDriverDeclarations(
        VariableDeclarationSyntax declaration,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            if (untrackableNames.Contains(variable.Identifier.ValueText))
            {
                continue;
            }

            ILocalSymbol localSymbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(variable)!;
            if (AnalyzerSymbolHelpers.IsCommandExecutorType(localSymbol.Type))
            {
                driverDisposedStatus[variable.Identifier.ValueText] = false;
            }
        }
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
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
                ProcessIfStatement(ifStatement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is ForStatementSyntax forStatement)
            {
                ProcessLoop(GetForLoopPreamble(forStatement), forStatement.Incrementors, forStatement.Statement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is CommonForEachStatementSyntax forEachStatement)
            {
                ProcessLoop([forEachStatement.Expression], [], forEachStatement.Statement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is WhileStatementSyntax whileStatement)
            {
                ProcessLoop([whileStatement.Condition], [], whileStatement.Statement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is TryStatementSyntax tryStatement)
            {
                ProcessTryStatement(tryStatement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is UsingStatementSyntax usingStatement)
            {
                ProcessUsingStatement(usingStatement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is ConditionalExpressionSyntax conditional)
            {
                ProcessConditionalExpression(conditional, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is SwitchExpressionSyntax switchExpression)
            {
                ProcessSwitchExpression(switchExpression, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is BinaryExpressionSyntax shortCircuit && AnalyzerSymbolHelpers.IsShortCircuitOperation(shortCircuit))
            {
                ProcessShortCircuit(shortCircuit.Left, shortCircuit.Right, null, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is AssignmentExpressionSyntax coalesceAssignment && AnalyzerSymbolHelpers.IsShortCircuitOperation(coalesceAssignment))
            {
                ProcessShortCircuit(coalesceAssignment.Left, coalesceAssignment.Right, coalesceAssignment, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is VariableDeclarationSyntax declaration)
            {
                TrackDriverDeclarations(declaration, semanticModel, driverDisposedStatus, untrackableNames);
            }
            else if (descendant is AssignmentExpressionSyntax assignment)
            {
                CheckAssignment(assignment, driverDisposedStatus);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, reportDiagnostics, semanticModel, driverDisposedStatus);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);

        Dictionary<string, bool> thenBranchStatus = new(driverDisposedStatus);
        ProcessNode(ifStatement.Statement, context, reportDiagnostics, semanticModel, thenBranchStatus, untrackableNames);

        Dictionary<string, bool> elseBranchStatus = new(driverDisposedStatus);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, reportDiagnostics, semanticModel, elseBranchStatus, untrackableNames);
        }

        // After the branch a driver counts as disposed only when every path through it disposed the
        // driver. This is the mirror image of the merge in BIDI009, and the polarity follows from what
        // is reported: a use is an error only if the driver is certainly disposed on every path that
        // reaches it, so a conditional dispose must not condemn the code that follows.
        foreach (string driverName in driverDisposedStatus.Keys.ToList())
        {
            driverDisposedStatus[driverName] = thenBranchStatus[driverName] && elseBranchStatus[driverName];
        }
    }

    private static void ProcessConditionalExpression(
        ConditionalExpressionSyntax conditional,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        // The condition is evaluated before either arm, and exactly one arm is evaluated after it: the shape of an
        // if statement with an else clause, forked and merged the same way: a driver counts as disposed after it only when both arms dispose it.
        ProcessNode(conditional.Condition, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);

        Dictionary<string, bool> whenTrueStatus = new(driverDisposedStatus);
        ProcessNode(conditional.WhenTrue, context, reportDiagnostics, semanticModel, whenTrueStatus, untrackableNames);

        Dictionary<string, bool> whenFalseStatus = new(driverDisposedStatus);
        ProcessNode(conditional.WhenFalse, context, reportDiagnostics, semanticModel, whenFalseStatus, untrackableNames);

        foreach (string driverName in driverDisposedStatus.Keys.ToList())
        {
            driverDisposedStatus[driverName] = whenTrueStatus[driverName] && whenFalseStatus[driverName];
        }
    }

    private static void ProcessSwitchExpression(
        SwitchExpressionSyntax switchExpression,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        // The governing expression is evaluated before any arm, and the arms are mutually exclusive. Matching no arm
        // throws rather than continuing after the expression, so the arms are the only paths out; with no arms at
        // all nothing after the expression is reached, and the state is left alone.
        ProcessNode(switchExpression.GoverningExpression, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);

        List<Dictionary<string, bool>> armStatuses = [];
        foreach (SwitchExpressionArmSyntax arm in switchExpression.Arms)
        {
            Dictionary<string, bool> armStatus = new(driverDisposedStatus);
            if (arm.WhenClause is not null)
            {
                ProcessNode(arm.WhenClause.Condition, context, reportDiagnostics, semanticModel, armStatus, untrackableNames);
            }

            ProcessNode(arm.Expression, context, reportDiagnostics, semanticModel, armStatus, untrackableNames);
            armStatuses.Add(armStatus);
        }

        if (armStatuses.Count > 0)
        {
            foreach (string driverName in driverDisposedStatus.Keys.ToList())
            {
                driverDisposedStatus[driverName] = armStatuses.All(armStatus => armStatus[driverName]);
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
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        // The left operand is always evaluated, and the right one only when the left does not settle the result
        // (&&, ||) or is null (??, ??=). The right operand is therefore walked as a path that may not run, as the
        // branch of an if statement without an else is, and a ??= assigns only on that path.
        ProcessNode(left, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);

        Dictionary<string, bool> rightStatus = new(driverDisposedStatus);
        ProcessNode(right, context, reportDiagnostics, semanticModel, rightStatus, untrackableNames);
        if (coalesceAssignment is not null)
        {
            CheckAssignment(coalesceAssignment, rightStatus);
        }

        foreach (string driverName in driverDisposedStatus.Keys.ToList())
        {
            driverDisposedStatus[driverName] = driverDisposedStatus[driverName] && rightStatus[driverName];
        }
    }

    private static void ProcessLoop(
        IEnumerable<SyntaxNode> preamble,
        IEnumerable<SyntaxNode> incrementors,
        StatementSyntax body,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
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
            ProcessNode(node, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);
        }

        Dictionary<string, bool> bodyStatus = new(driverDisposedStatus);
        ProcessNode(body, context, reportDiagnostics, semanticModel, bodyStatus, untrackableNames);
        foreach (SyntaxNode incrementor in incrementors)
        {
            ProcessNode(incrementor, context, reportDiagnostics, semanticModel, bodyStatus, untrackableNames);
        }

        foreach (string driverName in driverDisposedStatus.Keys.ToList())
        {
            driverDisposedStatus[driverName] = driverDisposedStatus[driverName] && bodyStatus[driverName];
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
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);

        List<Dictionary<string, bool>> sectionStatuses = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionStatus = new(driverDisposedStatus);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, reportDiagnostics, semanticModel, sectionStatus, untrackableNames);
            }

            sectionStatuses.Add(sectionStatus);
        }

        // As for if statements, a driver counts as disposed after the switch only when every path
        // through it disposed the driver. The unchanged state at the switch is the path taken when no
        // section matches, and it participates in the merge; when a default section makes that path
        // impossible, including it can only suppress a report, never create a false positive.
        foreach (string driverName in driverDisposedStatus.Keys.ToList())
        {
            driverDisposedStatus[driverName] = driverDisposedStatus[driverName] && sectionStatuses.All(sectionStatus => sectionStatus[driverName]);
        }
    }

    private static void ProcessTryStatement(
        TryStatementSyntax tryStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        Dictionary<string, bool> entryStatus = new(driverDisposedStatus);
        Dictionary<string, bool> tryStatus = new(driverDisposedStatus);
        ProcessNode(tryStatement.Block, context, reportDiagnostics, semanticModel, tryStatus, untrackableNames);

        // A catch clause (or a finally block) may begin executing after any prefix of the try block has
        // run, so inside one a driver counts as disposed only when every partial execution of the try
        // leaves it disposed: the conjunction of the state at try entry and the state after the full try
        // walk. A DisposeAsync inside the try may not have run yet, and an assignment of a fresh driver
        // inside the try may not have run either.
        Dictionary<string, bool> certainlyDisposedStatus = [];
        foreach (string driverName in entryStatus.Keys)
        {
            certainlyDisposedStatus[driverName] = entryStatus[driverName] && tryStatus[driverName];
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
                ProcessNode(catchClause.Filter.FilterExpression, context, reportDiagnostics, semanticModel, catchStatus, untrackableNames);
            }

            ProcessNode(catchClause.Block, context, reportDiagnostics, semanticModel, catchStatus, untrackableNames);
            completionStatuses.Add(catchStatus);
        }

        foreach (string driverName in driverDisposedStatus.Keys.ToList())
        {
            driverDisposedStatus[driverName] = completionStatuses.All(completionStatus => completionStatus[driverName]);
        }

        // A finally block runs on every way out of the statement. The code in it is judged against a state that
        // allows for all of them: the try block or a catch clause completing, or an exception from any point in
        // the try. Only normal completion reaches the code after the statement, though, so the block is walked a
        // second time, from the completion state and without reporting, to find what it leaves there: a DisposeAsync
        // in a finally certainly leaves the driver disposed for the code that follows.
        if (tryStatement.Finally is not null)
        {
            Dictionary<string, bool> finallyEntryStatus = [];
            foreach (string driverName in driverDisposedStatus.Keys)
            {
                finallyEntryStatus[driverName] = driverDisposedStatus[driverName] && certainlyDisposedStatus[driverName];
            }

            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics, semanticModel, finallyEntryStatus, untrackableNames);
            ProcessNode(tryStatement.Finally.Block, context, reportDiagnostics: false, semanticModel, driverDisposedStatus, untrackableNames);
        }
    }

    private static void ProcessUsingStatement(
        UsingStatementSyntax usingStatement,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus,
        HashSet<string> untrackableNames)
    {
        // A classic using statement disposes its resource when the body finishes, so the body is walked
        // against the state as it stands and the disposal is applied afterwards. A using *declaration*
        // needs none of this: it disposes at the end of the enclosing block, after every statement the
        // walk can reach, and it arrives here as an ordinary variable declaration.
        if (usingStatement.Declaration is not null)
        {
            TrackDriverDeclarations(usingStatement.Declaration, semanticModel, driverDisposedStatus, untrackableNames);
        }

        ProcessNode(usingStatement.Statement, context, reportDiagnostics, semanticModel, driverDisposedStatus, untrackableNames);

        // `await using (driver) { ... }` over an already-declared driver leaves it disposed for the
        // statements that follow. The variable a using statement declares itself goes out of scope here,
        // so marking it disposed can never report anything.
        if (usingStatement.Expression is IdentifierNameSyntax resourceIdentifier &&
            driverDisposedStatus.ContainsKey(resourceIdentifier.Identifier.ValueText))
        {
            driverDisposedStatus[resourceIdentifier.Identifier.ValueText] = true;
        }
    }

    private static void CheckAssignment(AssignmentExpressionSyntax assignment, Dictionary<string, bool> driverDisposedStatus)
    {
        // Rebinding the variable makes it name a different driver, so whatever was disposed before is no
        // longer what the name refers to. The new driver's own state is unknown unless it is constructed
        // here, and treating it as not disposed is the conservative choice either way: this rule reports
        // only certain misuse.
        if (assignment.Left is IdentifierNameSyntax target && driverDisposedStatus.ContainsKey(target.Identifier.ValueText))
        {
            driverDisposedStatus[target.Identifier.ValueText] = false;
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        bool reportDiagnostics,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverDisposedStatus)
    {
        // Nothing to report until a driver variable is being tracked; skip the expensive semantic bind
        // for every invocation seen before the first driver is declared.
        if (driverDisposedStatus.Count == 0)
        {
            return;
        }

        // A late-bound (dynamic) invocation resolves to no symbol at all, so there is nothing to classify.
        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        string? driverVariableName = GetDriverVariableName(invocation, context.Node, semanticModel);
        if (driverVariableName is null || !driverDisposedStatus.ContainsKey(driverVariableName))
        {
            return;
        }

        if (methodSymbol.Name == "DisposeAsync" && AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            driverDisposedStatus[driverVariableName] = true;
            return;
        }

        if (reportDiagnostics && driverDisposedStatus[driverVariableName] && ThrowsAfterDisposal(methodSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name, driverVariableName));
        }
    }

    private static string? GetDriverVariableName(InvocationExpressionSyntax invocation, SyntaxNode body, SemanticModel semanticModel)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        // Direct call on the driver (driver.ExecuteCommandAsync(...)), or a call on one of its modules
        // (driver.BrowsingContext.NavigateAsync(...)), which reaches the driver's disposal guard through
        // ExecuteCommandAsync.
        IdentifierNameSyntax? receiver = memberAccess.Expression switch
        {
            IdentifierNameSyntax identifier => identifier,
            MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax nestedIdentifier } => nestedIdentifier,
            _ => null,
        };

        if (receiver is null)
        {
            return null;
        }

        // A receiver that is not the driver itself may be a local holding one of its modules:
        // context.GetTreeAsync(), where the local was bound to driver.BrowsingContext.
        return AnalyzerSymbolHelpers.IsCommandExecutorType(semanticModel.GetTypeInfo(receiver).Type)
            ? receiver.Identifier.ValueText
            : AnalyzerSymbolHelpers.GetDriverOfModuleAlias(receiver, body, semanticModel);
    }

    /// <summary>
    /// Determines whether a method throws once the driver it belongs to is disposed.
    /// </summary>
    /// <param name="method">The resolved method.</param>
    /// <returns><see langword="true"/> if the method is guarded by the driver's disposal check; otherwise <see langword="false"/>.</returns>
    private static bool ThrowsAfterDisposal(IMethodSymbol method)
    {
        if (AnalyzerSymbolHelpers.IsCommandExecutorType(method.ContainingType))
        {
            return DisposalGuardedDriverMethods.Contains(method.Name);
        }

        // A module command reaches the driver's disposal guard through ExecuteCommandAsync, so it throws
        // exactly as a direct command would.
        return AnalyzerSymbolHelpers.IsLibraryModuleType(method.ContainingType) && IsModuleCommandMethod(method);
    }

    private static bool IsModuleCommandMethod(IMethodSymbol method)
    {
        return method.Name.EndsWith("Async", System.StringComparison.Ordinal)
            && method.ReturnType is INamedTypeSymbol { Name: "Task", IsGenericType: true } returnType
            && returnType.TypeArguments.Length == 1
            && InheritsFromCommandResult(returnType.TypeArguments[0]);
    }

    private static bool InheritsFromCommandResult(ITypeSymbol type)
    {
        for (INamedTypeSymbol? current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
        {
            if (AnalyzerSymbolHelpers.IsLibraryTypeNamed(current, "CommandResult"))
            {
                return true;
            }
        }

        return false;
    }
}
