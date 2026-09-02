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

        // Walk through all statements in the method
        IEnumerable<StatementSyntax> statements = AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node);

        foreach (StatementSyntax statement in statements)
        {
            // ProcessNode registers driver declarations and checks driver method calls,
            // wherever in the statement's subtree they appear.
            ProcessNode(statement, context, semanticModel, driverStartedStatus);
        }
    }

    private static void AnalyzeLocalDeclaration(
        LocalDeclarationStatementSyntax localDecl,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus)
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
        Dictionary<string, bool> driverStartedStatus)
    {
        // Walk the node's descendants in document order, checking each invocation against the
        // tracked started state. The walk does not descend into the bodies of nested functions
        // (lambdas, anonymous methods, local functions): their code runs when the delegate is
        // invoked, not at the textual position where it is declared — for example when an event
        // handler fires after the connection is started — so it must not be judged against the
        // driver's started state at this point in the method. It also stops at if and switch
        // statements — including one that is itself the root, which the barrier yields without
        // descending into — and processes them recursively below with a forked copy of the
        // state for each mutually exclusive branch.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, semanticModel, driverStartedStatus);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, semanticModel, driverStartedStatus);
            }
            else if (descendant is LocalDeclarationStatementSyntax localDecl)
            {
                // Register driver declarations wherever they appear (including inside nested
                // blocks such as try or using statements); the pre-order walk visits the
                // declaration before any later use of the variable.
                AnalyzeLocalDeclaration(localDecl, semanticModel, driverStartedStatus);
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
        Dictionary<string, bool> driverStartedStatus)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, semanticModel, driverStartedStatus);

        // The branches are mutually exclusive, so each arm is walked against its own copy of
        // the state at the branch point: a StopAsync in one arm must not poison a command in
        // the other. An else-if chain arrives here as an else clause whose statement is itself
        // an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchStatus = new(driverStartedStatus);
        ProcessNode(ifStatement.Statement, context, semanticModel, thenBranchStatus);

        Dictionary<string, bool> elseBranchStatus = new(driverStartedStatus);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, semanticModel, elseBranchStatus);
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

    private static void ProcessSwitchStatement(
        SwitchStatementSyntax switchStatement,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, semanticModel, driverStartedStatus);

        // Sections are mutually exclusive in the same way if/else branches are.
        List<Dictionary<string, bool>> sectionStatuses = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionStatus = new(driverStartedStatus);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, semanticModel, sectionStatus);
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
            if (currentType.Name == "CommandResult")
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
            if (currentType.Name == "Module")
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}
