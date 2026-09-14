// <copyright file="BiDiDriver009_CommandExecutionBeforeStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
        // code is worse than a missed report.
        HashSet<string> escapedNames = FindEscapedVariableNames(context.Node, semanticModel);

        // Walk through all statements in the method
        IEnumerable<StatementSyntax> statements = AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node);

        foreach (StatementSyntax statement in statements)
        {
            // ProcessNode registers driver declarations and checks driver method calls,
            // wherever in the statement's subtree they appear.
            ProcessNode(statement, context, reportDiagnostics: true, semanticModel, driverStartedStatus, escapedNames);
        }
    }

    /// <summary>
    /// Collects the names of local variables that this member hands to something else, or that a nested
    /// function could start, stop, or rebind.
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns>The set of names whose started state cannot be known from this member alone.</returns>
    private static HashSet<string> FindEscapedVariableNames(SyntaxNode body, SemanticModel semanticModel)
    {
        HashSet<string> escapedNames = [];
        foreach (IdentifierNameSyntax identifier in body.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            // The nested-function classification is asked first so that each predicate answers only for
            // the shape it owns: the driver on the right of an assignment is an escape wherever it is
            // written, and this order lets that case reach the escape check rather than being absorbed
            // by the rebind test.
            if (IsStartedStoppedOrReboundInsideNestedFunction(identifier, body) || IsEscapingPosition(identifier, semanticModel))
            {
                escapedNames.Add(identifier.Identifier.ValueText);
            }
        }

        return escapedNames;
    }

    /// <summary>
    /// Determines whether a mention of a variable hands it to something else, so that other code could
    /// start it.
    /// </summary>
    /// <param name="identifier">The mention of the variable.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns><see langword="true"/> if the variable escapes at this position; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// This mirrors the escape classification in BIDI006. A mention that merely uses the driver
    /// (<c>driver.Session.StatusAsync()</c>, a null test, a using statement) has a parent that is not in
    /// the list below and is correctly not treated as an escape.
    /// </remarks>
    private static bool IsEscapingPosition(IdentifierNameSyntax identifier, SemanticModel semanticModel)
    {
        // As in BIDI006, the mention may be wrapped (parenthesized, cast to an interface, null-forgiven,
        // or one arm of a conditional) before it reaches the construct that hands the driver out.
        SyntaxNode mention = AnalyzerSymbolHelpers.PeelExpressionWrappers(identifier);
        return mention.Parent switch
        {
            // Returned to the caller: return driver; or yield return driver;
            ReturnStatementSyntax or YieldStatementSyntax => true,

            // Assigned to another target, for example a field: this.driver = driver;
            AssignmentExpressionSyntax assignment => assignment.Right == mention,

            // Passed to a method or constructor that may start it: await StartHelperAsync(driver);
            ArgumentSyntax argument => !IsModuleConstructionArgument(argument, semanticModel),

            // Placed in a collection expression, an initializer, or used to initialize another
            // variable that may itself be started.
            ExpressionElementSyntax or InitializerExpressionSyntax or EqualsValueClauseSyntax => true,

            // The receiver of an extension method: `await driver.StartWithRetryAsync(url);`. The
            // driver is the method's first argument, the walk cannot see whether the method starts
            // it, and the argument case above already treats that as an escape; the only difference
            // here is the spelling. A call on the driver's own members is not affected, because it
            // binds to an instance method rather than an extension method.
            MemberAccessExpressionSyntax memberAccess when memberAccess.Expression == mention
                && memberAccess.Parent is InvocationExpressionSyntax extensionInvocation
                && semanticModel.GetSymbolInfo(extensionInvocation).Symbol is IMethodSymbol { IsExtensionMethod: true } => true,

            _ => false,
        };
    }

    /// <summary>
    /// Determines whether an argument hands the driver to the constructor of a module.
    /// </summary>
    /// <param name="argument">The argument mentioning the driver.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns><see langword="true"/> if the argument constructs a module; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The documented way to add a custom module is <c>driver.RegisterModule(new CustomModule(driver))</c>:
    /// the driver is passed to the module's constructor, which hands it to the <c>Module</c> base class
    /// so the module can issue commands through it later. A module holds the driver; it does not start
    /// it. Treating that argument as an escape would switch this rule off for every method that
    /// registers a custom module, which is precisely the set-up code the rule exists to check.
    /// </remarks>
    private static bool IsModuleConstructionArgument(ArgumentSyntax argument, SemanticModel semanticModel)
    {
        return argument.Parent is ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax creation }
            && semanticModel.GetTypeInfo(creation).Type is INamedTypeSymbol createdType
            && AnalyzerSymbolHelpers.IsModuleSubclass(createdType);
    }

    /// <summary>
    /// Determines whether a mention of a variable inside a nested function starts it, stops it, or
    /// rebinds it to a different driver.
    /// </summary>
    /// <param name="identifier">The mention of the variable.</param>
    /// <param name="body">The member body being analyzed.</param>
    /// <returns><see langword="true"/> if a nested function can change the variable's started state; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A nested function runs when its delegate is invoked, not where it is declared, so a
    /// <c>StartAsync</c>, a <c>StopAsync</c>, or an assignment inside one can change the driver's state
    /// at a point this rule's textual walk cannot place. An assignment counts because the variable then
    /// names a driver that came from somewhere else and may already be started, which is the same
    /// reason <see cref="TrackDriverAssignment"/> stops tracking such a rebind on a straight-line path.
    /// Only those mentions make the state unknown: a nested function that merely issues commands on the
    /// driver leaves the state alone, and treating every capture as an escape would stop the rule
    /// reporting a genuine error elsewhere in the same method.
    /// </remarks>
    private static bool IsStartedStoppedOrReboundInsideNestedFunction(IdentifierNameSyntax identifier, SyntaxNode body)
    {
        bool changesStartedState = identifier.Parent switch
        {
            // Rebound to another driver: driver = await pool.RentStartedAsync();
            AssignmentExpressionSyntax assignment => assignment.Left == identifier,

            // Started or stopped: driver.StartAsync() or driver.StopAsync().
            MemberAccessExpressionSyntax memberAccess => memberAccess.Expression == identifier
                && memberAccess.Parent is InvocationExpressionSyntax
                && memberAccess.Name.Identifier.ValueText is "StartAsync" or "StopAsync",

            _ => false,
        };

        if (!changesStartedState)
        {
            return false;
        }

        // The identifier came from the body's descendants, so the body is always an ancestor and always
        // stops the walk.
        return identifier.Ancestors()
            .TakeWhile(ancestor => ancestor != body)
            .Any(ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);
    }

    private static void AnalyzeLocalDeclaration(
        VariableDeclarationSyntax declaration,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        HashSet<string> escapedNames)
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

            if (escapedNames.Contains(variable.Identifier.ValueText))
            {
                continue;
            }

            ITypeSymbol? typeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value).Type;
            if (AnalyzerSymbolHelpers.IsCommandExecutorType(typeInfo))
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
        HashSet<string> escapedNames)
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
        HashSet<string> escapedNames)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, reportDiagnostics, semanticModel, driverStartedStatus, escapedNames);

        // The branches are mutually exclusive, so each arm is walked against its own copy of
        // the state at the branch point: a StopAsync in one arm must not poison a command in
        // the other. An else-if chain arrives here as an else clause whose statement is itself
        // an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchStatus = new(driverStartedStatus);
        ProcessNode(ifStatement.Statement, context, reportDiagnostics, semanticModel, thenBranchStatus, escapedNames);

        Dictionary<string, bool> elseBranchStatus = new(driverStartedStatus);
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
        HashSet<string> escapedNames)
    {
        Dictionary<string, bool> entryStatus = new(driverStartedStatus);
        Dictionary<string, bool> tryStatus = new(driverStartedStatus);
        ProcessNode(tryStatement.Block, context, reportDiagnostics, semanticModel, tryStatus, escapedNames);

        // A catch clause (or a finally block) may begin executing after any prefix of the try block
        // has run, so inside one a driver counts as started when *any* partial execution of the try
        // could leave it started: the disjunction of the state at try entry and the state after the
        // full try walk. A StartAsync inside the try may already have run (started after the try is
        // true), and a StopAsync inside the try may not have run yet (started at entry is true).
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
            if (tryStatus.TryGetValue(driverName, out bool startedAfterTryBlock))
            {
                mightBeStartedStatus[driverName] = entryStatus[driverName] || startedAfterTryBlock;
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
        HashSet<string> escapedNames)
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
        HashSet<string> escapedNames)
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
        HashSet<string> escapedNames)
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
        HashSet<string> escapedNames)
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
        HashSet<string> escapedNames)
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

        IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        string methodName = methodSymbol.Name;

        // Get the driver variable name if this is a method call on a driver or module
        string? driverVariableName = GetDriverVariableNameFromInvocation(invocation, context.Node, semanticModel);
        if (driverVariableName == null || !driverStartedStatus.ContainsKey(driverVariableName))
        {
            return;
        }

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
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Direct call on driver: driver.ExecuteCommandAsync(...)
            if (memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                ITypeSymbol? type = semanticModel.GetTypeInfo(identifier).Type;
                if (AnalyzerSymbolHelpers.IsCommandExecutorType(type))
                {
                    return identifier.Identifier.ValueText;
                }

                // Call on a module held in a local: context.GetTreeAsync(), where the local was bound
                // to driver.BrowsingContext. The nested-member-access form below cannot match an
                // identifier receiver, so this is the whole of that case.
                return AnalyzerSymbolHelpers.GetDriverOfModuleAlias(identifier, body, semanticModel);
            }

            // Call on module: driver.BrowsingContext.NavigateAsync(...)
            if (memberAccess.Expression is MemberAccessExpressionSyntax nestedMemberAccess &&
                nestedMemberAccess.Expression is IdentifierNameSyntax nestedIdentifier)
            {
                ITypeSymbol? type = semanticModel.GetTypeInfo(nestedIdentifier).Type;
                if (AnalyzerSymbolHelpers.IsCommandExecutorType(type))
                {
                    return nestedIdentifier.Identifier.ValueText;
                }
            }
        }

        return null;
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
