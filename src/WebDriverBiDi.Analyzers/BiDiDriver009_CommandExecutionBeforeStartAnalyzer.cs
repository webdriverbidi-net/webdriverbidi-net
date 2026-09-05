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
        HashSet<string> escapedNames = FindEscapedVariableNames(context.Node);

        // Walk through all statements in the method
        IEnumerable<StatementSyntax> statements = AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node);

        foreach (StatementSyntax statement in statements)
        {
            // ProcessNode registers driver declarations and checks driver method calls,
            // wherever in the statement's subtree they appear.
            ProcessNode(statement, context, semanticModel, driverStartedStatus, escapedNames);
        }
    }

    /// <summary>
    /// Collects the names of local variables that this member hands to something else, or that a nested
    /// function could start or stop.
    /// </summary>
    /// <param name="body">The member body being analyzed.</param>
    /// <returns>The set of names whose started state cannot be known from this member alone.</returns>
    private static HashSet<string> FindEscapedVariableNames(SyntaxNode body)
    {
        HashSet<string> escapedNames = [];
        foreach (IdentifierNameSyntax identifier in body.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (IsEscapingPosition(identifier) || IsStartedOrStoppedInsideNestedFunction(identifier, body))
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
    /// <returns><see langword="true"/> if the variable escapes at this position; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// This mirrors the escape classification in BIDI006. A mention that merely uses the driver
    /// (<c>driver.Session.StatusAsync()</c>, a null test, a using statement) has a parent that is not in
    /// the list below and is correctly not treated as an escape.
    /// </remarks>
    private static bool IsEscapingPosition(IdentifierNameSyntax identifier)
    {
        return identifier.Parent switch
        {
            // Returned to the caller: return driver; or yield return driver;
            ReturnStatementSyntax or YieldStatementSyntax => true,

            // Assigned to another target, for example a field: this.driver = driver;
            AssignmentExpressionSyntax assignment => assignment.Right == identifier,

            // Passed to a method or constructor that may start it: await StartHelperAsync(driver);
            ArgumentSyntax => true,

            // Placed in a collection expression, an initializer, or used to initialize another
            // variable that may itself be started.
            ExpressionElementSyntax or InitializerExpressionSyntax or EqualsValueClauseSyntax => true,

            _ => false,
        };
    }

    /// <summary>
    /// Determines whether a mention of a variable inside a nested function starts or stops it.
    /// </summary>
    /// <param name="identifier">The mention of the variable.</param>
    /// <param name="body">The member body being analyzed.</param>
    /// <returns><see langword="true"/> if a nested function can change the variable's started state; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A nested function runs when its delegate is invoked, not where it is declared, so a
    /// <c>StartAsync</c> or <c>StopAsync</c> inside one can change the driver's state at a point this
    /// rule's textual walk cannot place. Only those two calls make the state unknown: a nested function
    /// that merely issues commands on the driver leaves the state alone, and treating every capture as
    /// an escape would stop the rule reporting a genuine error elsewhere in the same method.
    /// </remarks>
    private static bool IsStartedOrStoppedInsideNestedFunction(IdentifierNameSyntax identifier, SyntaxNode body)
    {
        if (identifier.Parent is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Expression != identifier ||
            memberAccess.Parent is not InvocationExpressionSyntax)
        {
            return false;
        }

        string methodName = memberAccess.Name.Identifier.ValueText;
        if (methodName != "StartAsync" && methodName != "StopAsync")
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
        LocalDeclarationStatementSyntax localDecl,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        HashSet<string> escapedNames)
    {
        foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
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
                driverStartedStatus[variable.Identifier.Text] = false;
            }
        }
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        HashSet<string> escapedNames)
    {
        // Walk the node's descendants in document order, checking each invocation against the
        // tracked started state. The walk does not descend into the bodies of nested functions
        // (lambdas, anonymous methods, local functions): their code runs when the delegate is
        // invoked, not at the textual position where it is declared — for example when an event
        // handler fires after the connection is started — so it must not be judged against the
        // driver's started state at this point in the method. It also stops at if, switch, and try
        // statements — including one that is itself the root, which the barrier yields without
        // descending into — and processes them recursively below with a forked copy of the
        // state for each mutually exclusive branch.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax &&
            child is not TryStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is TryStatementSyntax tryStatement)
            {
                ProcessTryStatement(tryStatement, context, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is LocalDeclarationStatementSyntax localDecl)
            {
                // Register driver declarations wherever they appear (including inside nested
                // blocks such as try or using statements); the pre-order walk visits the
                // declaration before any later use of the variable.
                AnalyzeLocalDeclaration(localDecl, semanticModel, driverStartedStatus, escapedNames);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, semanticModel, driverStartedStatus);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        HashSet<string> escapedNames)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, semanticModel, driverStartedStatus, escapedNames);

        // The branches are mutually exclusive, so each arm is walked against its own copy of
        // the state at the branch point: a StopAsync in one arm must not poison a command in
        // the other. An else-if chain arrives here as an else clause whose statement is itself
        // an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchStatus = new(driverStartedStatus);
        ProcessNode(ifStatement.Statement, context, semanticModel, thenBranchStatus, escapedNames);

        Dictionary<string, bool> elseBranchStatus = new(driverStartedStatus);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, semanticModel, elseBranchStatus, escapedNames);
        }

        // After the branch, a driver counts as not started only when every path through the
        // branch leaves it not started. This rule reports commands on a driver that has not
        // been started, so treating "started on some path only" as not started would flag
        // correct conditional stop/restart patterns with an Error-severity false positive;
        // the Error severity demands that the command fail on every path.
        foreach (string driverName in driverStartedStatus.Keys.ToList())
        {
            driverStartedStatus[driverName] = thenBranchStatus[driverName] || elseBranchStatus[driverName];
        }
    }

    private static void ProcessTryStatement(
        TryStatementSyntax tryStatement,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        HashSet<string> escapedNames)
    {
        Dictionary<string, bool> entryStatus = new(driverStartedStatus);
        Dictionary<string, bool> tryStatus = new(driverStartedStatus);
        ProcessNode(tryStatement.Block, context, semanticModel, tryStatus, escapedNames);

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
        Dictionary<string, bool> mightBeStartedStatus = [];
        foreach (string driverName in entryStatus.Keys)
        {
            mightBeStartedStatus[driverName] = entryStatus[driverName] || tryStatus[driverName];
        }

        List<Dictionary<string, bool>> exitStatuses = [tryStatus];
        foreach (CatchClauseSyntax catchClause in tryStatement.Catches)
        {
            Dictionary<string, bool> catchStatus = new(mightBeStartedStatus);
            if (catchClause.Filter is not null)
            {
                ProcessNode(catchClause.Filter.FilterExpression, context, semanticModel, catchStatus, escapedNames);
            }

            ProcessNode(catchClause.Block, context, semanticModel, catchStatus, escapedNames);
            exitStatuses.Add(catchStatus);
        }

        if (tryStatement.Finally is not null)
        {
            Dictionary<string, bool> finallyStatus = new(mightBeStartedStatus);
            ProcessNode(tryStatement.Finally.Block, context, semanticModel, finallyStatus, escapedNames);
            exitStatuses.Add(finallyStatus);
        }

        // After the try statement, a driver counts as started when any completion path leaves it
        // started, matching how the if and switch merges treat mutually exclusive branches: this
        // rule reports only a driver that is not started on every path, so a stop confined to one
        // catch clause must not poison code that follows the statement.
        foreach (string driverName in driverStartedStatus.Keys.ToList())
        {
            driverStartedStatus[driverName] = exitStatuses.Any(exitStatus => exitStatus[driverName]);
        }
    }

    private static void ProcessSwitchStatement(
        SwitchStatementSyntax switchStatement,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus,
        HashSet<string> escapedNames)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, semanticModel, driverStartedStatus, escapedNames);

        // Sections are mutually exclusive in the same way if/else branches are.
        List<Dictionary<string, bool>> sectionStatuses = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionStatus = new(driverStartedStatus);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, semanticModel, sectionStatus, escapedNames);
            }

            sectionStatuses.Add(sectionStatus);
        }

        // As for if statements, a driver counts as not started after the switch only when no
        // path through the switch leaves it started. The unchanged state at the switch (the
        // path taken when no section matches) participates in the merge alongside every
        // section; when a default section makes that path impossible, including it can only
        // suppress a report, never create a false positive, so default detection is not needed.
        foreach (string driverName in driverStartedStatus.Keys.ToList())
        {
            driverStartedStatus[driverName] = driverStartedStatus[driverName] || sectionStatuses.Any(sectionStatus => sectionStatus[driverName]);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
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
        string? driverVariableName = GetDriverVariableNameFromInvocation(invocation, semanticModel);
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
        if (!driverStartedStatus[driverVariableName] && IsCommandMethod(methodSymbol))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string? GetDriverVariableNameFromInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Direct call on driver: driver.ExecuteCommandAsync(...)
            if (memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                ITypeSymbol? type = semanticModel.GetTypeInfo(identifier).Type;
                if (AnalyzerSymbolHelpers.IsCommandExecutorType(type))
                {
                    return identifier.Identifier.Text;
                }
            }

            // Call on module: driver.BrowsingContext.NavigateAsync(...)
            if (memberAccess.Expression is MemberAccessExpressionSyntax nestedMemberAccess &&
                nestedMemberAccess.Expression is IdentifierNameSyntax nestedIdentifier)
            {
                ITypeSymbol? type = semanticModel.GetTypeInfo(nestedIdentifier).Type;
                if (AnalyzerSymbolHelpers.IsCommandExecutorType(type))
                {
                    return nestedIdentifier.Identifier.Text;
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
