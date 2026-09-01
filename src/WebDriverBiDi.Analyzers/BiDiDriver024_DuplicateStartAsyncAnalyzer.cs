// <copyright file="BiDiDriver024_DuplicateStartAsyncAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects a second <c>StartAsync</c> call on a <c>BiDiDriver</c> with no intervening
/// <c>StopAsync</c>. The transport is already connected at that point, so the call throws a
/// <c>WebDriverBiDiConnectionException</c> at runtime.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver024_DuplicateStartAsyncAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI024";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "StartAsync called on an already-started BiDiDriver";

    private static readonly LocalizableString MessageFormat = "StartAsync() has already been called on this BiDiDriver without a subsequent StopAsync(). Calling StartAsync() again throws because the transport is already connected. Call StopAsync() before starting again.";

    private static readonly LocalizableString Description = "A BiDiDriver may only be started once at a time. Calling StartAsync() while the driver is already started throws a WebDriverBiDiConnectionException because the underlying transport is already connected to a remote end. To reconnect, call StopAsync() first and then StartAsync() again.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi024");

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

        // Track BiDiDriver variables and whether StartAsync is currently in effect for each.
        Dictionary<string, bool> driverStartedStatus = [];

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                TrackDriverDeclarations(localDecl, semanticModel, driverStartedStatus);
            }

            ProcessNode(statement, context, driverStartedStatus);
        }
    }

    private static void TrackDriverDeclarations(
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
        Dictionary<string, bool> driverStartedStatus)
    {
        // Walk the node's descendants in document order, checking each invocation against the
        // tracked started state. The walk does not descend into the bodies of nested functions
        // (lambdas, anonymous methods, local functions): their code runs when the delegate is
        // invoked, not at the textual position where it is declared, so a StartAsync there must
        // not be judged against the driver's started state at this point in the method. It also
        // stops at if and switch statements — including one that is itself the root, which the
        // barrier yields without descending into — and processes them recursively below with a
        // forked copy of the state for each mutually exclusive branch.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, driverStartedStatus);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, driverStartedStatus);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, driverStartedStatus);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> driverStartedStatus)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, driverStartedStatus);

        // The branches are mutually exclusive: a StartAsync in one arm is never a duplicate of a
        // StartAsync in the other, so each arm is walked against its own copy of the state at the
        // branch point. An else-if chain arrives here as an else clause whose statement is itself
        // an if statement, which ProcessNode routes back into this method.
        Dictionary<string, bool> thenBranchStatus = new(driverStartedStatus);
        ProcessNode(ifStatement.Statement, context, thenBranchStatus);

        Dictionary<string, bool> elseBranchStatus = new(driverStartedStatus);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, elseBranchStatus);
        }

        // After the branch, a driver counts as started only when every path through the branch
        // leaves it started. Treating "started in some path" as started would flag correct
        // conditional stop/restart patterns as duplicates.
        foreach (string driverName in driverStartedStatus.Keys.ToList())
        {
            driverStartedStatus[driverName] = thenBranchStatus[driverName] && elseBranchStatus[driverName];
        }
    }

    private static void ProcessSwitchStatement(
        SwitchStatementSyntax switchStatement,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> driverStartedStatus)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, driverStartedStatus);

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
                ProcessNode(sectionStatement, context, sectionStatus);
            }

            sectionStatuses.Add(sectionStatus);
        }

        if (!hasDefaultSection)
        {
            sectionStatuses.Add(new Dictionary<string, bool>(driverStartedStatus));
        }

        foreach (string driverName in driverStartedStatus.Keys.ToList())
        {
            driverStartedStatus[driverName] = sectionStatuses.All(sectionStatus => sectionStatus[driverName]);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> driverStartedStatus)
    {
        // Only direct calls on a tracked driver variable (driver.StartAsync()/driver.StopAsync())
        // affect the started state; the receiver was type-checked when the variable was declared.
        if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } memberAccess)
        {
            return;
        }

        string driverVariableName = identifier.Identifier.Text;
        if (!driverStartedStatus.TryGetValue(driverVariableName, out bool started))
        {
            return;
        }

        string methodName = memberAccess.Name.Identifier.Text;
        if (methodName == "StartAsync")
        {
            if (started)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
            }
            else
            {
                driverStartedStatus[driverVariableName] = true;
            }
        }
        else if (methodName == "StopAsync")
        {
            driverStartedStatus[driverVariableName] = false;
        }
    }
}
