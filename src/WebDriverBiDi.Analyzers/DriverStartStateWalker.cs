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
/// <para>
/// A driver whose started state the body cannot know is never tracked, and so never reported on: one the body
/// hands to other code that could start or stop it (passed as an argument, returned, stored, aliased, placed in a
/// collection, or used as the receiver of an extension method or of a method a derived driver type declares or
/// overrides), and one a nested function starts, stops, or rebinds. Passing the driver to a module's constructor is
/// not such a hand-off, because a module holds the driver to issue commands and cannot start or stop it. The names are
/// collected for the whole body before any call is judged, because the other code may run before or after a call
/// textually, and all four rules report at Error severity, for which a wrong report on correct code is worse than a
/// missed one. BIDI009 shares this classification through
/// <see cref="FindDriversWithUnknownStartedState"/>.
/// </para>
/// </remarks>
internal sealed class DriverStartStateWalker
{
    private const string StartedPropertyName = "IsStarted";

    private readonly SyntaxNodeAnalysisContext context;
    private readonly Func<ITypeSymbol?, bool> isDriverType;
    private readonly DriverInvocationHandler handler;

    // Drivers this body hands to other code, or that a nested function starts, stops, or rebinds. Their started
    // state cannot be known from the body alone, so they are never tracked. Finding them walks every identifier in the
    // body and binds the invocations among them, so it is done only once the body is found to declare a driver at all,
    // which most bodies in a compilation do not.
    private HashSet<string>? untrackableDriverNames;

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
    /// Collects the names of local variables that this member hands to something else, or that a nested
    /// function could start, stop, or rebind, so that their started state is never tracked.
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns>The set of names whose started state cannot be known from this member alone.</returns>
    internal static HashSet<string> FindDriversWithUnknownStartedState(SyntaxNode body, SemanticModel semanticModel)
    {
        HashSet<string> escapedNames = [];

        // Only the body's own code is scanned. A top-level program's compilation unit also holds the types the
        // program declares, and a same-named parameter handed on there (a module constructor's base(driver),
        // say) is not the program's local driver escaping.
        foreach (IdentifierNameSyntax identifier in AnalyzerSymbolHelpers.GetBodyDescendantNodes(body).OfType<IdentifierNameSyntax>())
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

            // The receiver of a method the library does not define: `await driver.StartWithRetryAsync(url);`.
            MemberAccessExpressionSyntax memberAccess when memberAccess.Expression == mention
                && memberAccess.Parent is InvocationExpressionSyntax invocation => InvokesMethodUnknownToLibrary(invocation, semanticModel),

            // The same method called through a null-conditional receiver:
            // `await (driver?.StartWithRetryAsync(url) ?? Task.CompletedTask);`.
            ConditionalAccessExpressionSyntax conditionalAccess when conditionalAccess.Expression == mention
                && AnalyzerSymbolHelpers.GetReceiverMemberBinding(conditionalAccess)?.Parent is InvocationExpressionSyntax conditionalInvocation => InvokesMethodUnknownToLibrary(conditionalInvocation, semanticModel),

            _ => false,
        };
    }

    /// <summary>
    /// Determines whether an invocation on the driver calls a method whose effect on the driver's started state
    /// the walk cannot know, because the library does not define it.
    /// </summary>
    /// <param name="invocation">The invocation whose receiver is the driver.</param>
    /// <param name="semanticModel">The semantic model for the member.</param>
    /// <returns><see langword="true"/> if the invoked method may start or stop the driver unseen; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// Two kinds of method qualify. An extension method receives the driver as its first argument, and the
    /// argument case already treats that as an escape; the only difference is the spelling. A method a derived
    /// driver declares itself (<c>class ConnectingDriver : BiDiDriver { public Task ConnectAsync(string url) =&gt;
    /// this.StartAsync(url); }</c>) can call the library's lifecycle methods from inside, where the walk cannot see.
    /// </para>
    /// <para>
    /// A method that overrides a library method (an override of <c>StartAsync</c>, say) is still the library's
    /// operation, so the walk goes on recognizing it by name. A method the driver inherits from
    /// <see cref="object"/> cannot touch the driver's state and is not an escape either.
    /// </para>
    /// </remarks>
    private static bool InvokesMethodUnknownToLibrary(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return false;
        }

        return method.IsExtensionMethod
            || (AnalyzerSymbolHelpers.IsCommandExecutorType(method.ContainingType) && !IsDeclaredOrOverriddenFromLibrary(method));
    }

    /// <summary>
    /// Determines whether a method is declared by the library, or overrides a method the library declares.
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <returns><see langword="true"/> if the method or a method it overrides is the library's; otherwise <see langword="false"/>.</returns>
    private static bool IsDeclaredOrOverriddenFromLibrary(IMethodSymbol method)
    {
        for (IMethodSymbol? current = method; current is not null; current = current.OverriddenMethod)
        {
            if (AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(current.ContainingType))
            {
                return true;
            }
        }

        return false;
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
        // The receiver of a start or a stop may be wrapped (driver!.StartAsync()), as the walk itself allows.
        SyntaxNode mention = AnalyzerSymbolHelpers.PeelExpressionWrappers(identifier);
        bool changesStartedState = mention.Parent switch
        {
            // Rebound to another driver: driver = await pool.RentStartedAsync();
            AssignmentExpressionSyntax assignment => assignment.Left == mention,

            // Started or stopped: driver.StartAsync() or driver.StopAsync().
            MemberAccessExpressionSyntax memberAccess => memberAccess.Expression == mention
                && memberAccess.Parent is InvocationExpressionSyntax
                && IsStartOrStop(memberAccess.Name),

            // Started or stopped through a null-conditional receiver: driver?.StartAsync().
            ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression == mention
                && AnalyzerSymbolHelpers.GetReceiverMemberBinding(conditionalAccess) is { Parent: InvocationExpressionSyntax } binding
                && IsStartOrStop(binding.Name),

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

    private static bool IsStartOrStop(SimpleNameSyntax name)
    {
        return name.Identifier.ValueText is "StartAsync" or "StopAsync";
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
            if (!this.isDriverType(initializerType))
            {
                // A sibling scope may declare something else of the same name; leaving the previous
                // entry in place would judge calls on it against the old driver's state.
                driverStartedStatus.Remove(variable.Identifier.ValueText);
                continue;
            }

            // A driver handed a Transport is started by connecting that transport, which this walk
            // never sees, so its state is not known here.
            if (AnalyzerSymbolHelpers.IsDriverConstructedFromTransport(this.context.SemanticModel, variable.Initializer.Value))
            {
                driverStartedStatus.Remove(variable.Identifier.ValueText);
                continue;
            }

            this.untrackableDriverNames ??= FindDriversWithUnknownStartedState(this.context.Node, this.context.SemanticModel);
            if (!this.untrackableDriverNames.Contains(variable.Identifier.ValueText))
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
    internal static bool TryGetStartedStateTest(ExpressionSyntax condition, Dictionary<string, bool> driverStartedStatus, out string driverVariableName, out bool startedWhenConditionHolds)
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
        // execution of the try leaves it started: it was started at entry and nothing in the try
        // could have made it otherwise. Reading the state after the full try walk instead would
        // miss a try that stops and restarts the driver, whose end state says nothing about the
        // moment a catch is entered. Judging catch and finally code
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
            if (tryStatus.ContainsKey(driverName))
            {
                bool everNotStarted = AnalyzerSymbolHelpers.ContainsCallOnVariable(tryStatement.Block, driverName, "StopAsync")
                    || AnalyzerSymbolHelpers.ContainsRebinding(tryStatement.Block, driverName);
                conservativeStatus[driverName] = entryStatus[driverName] && !everNotStarted;
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
        if (driverStartedStatus.Count == 0)
        {
            return;
        }

        // Only a call whose receiver chain roots in a tracked driver variable matters; the receiver's
        // type was checked when the variable was declared. Resolving the name first keeps the
        // expensive semantic bind to calls that can affect a tracked driver. The chain is read through
        // the wrappers a receiver may carry, so driver!.StartAsync() and driver?.StartAsync() are calls
        // on the driver exactly as driver.StartAsync() is.
        IdentifierNameSyntax? driverIdentifier = AnalyzerSymbolHelpers.GetMemberChainRoot(invocation.Expression, out int memberDepth);
        if (driverIdentifier is null || !driverStartedStatus.TryGetValue(driverIdentifier.Identifier.ValueText, out bool started))
        {
            return;
        }

        string driverVariableName = driverIdentifier.Identifier.ValueText;

        if (this.context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return;
        }

        // Whether the call is on the driver itself rather than on something reached through it. The
        // receiver chain only has to *root* in a tracked driver for the call to arrive here, so
        // driver.Session.SubscribeAsync() and driver.Tracing.StartAsync() both do.
        bool isDirectDriverCall = memberDepth == 1;

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
