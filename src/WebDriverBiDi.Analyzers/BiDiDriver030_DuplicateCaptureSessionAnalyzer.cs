// <copyright file="BiDiDriver030_DuplicateCaptureSessionAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects a second <c>StartCapturingTasks()</c> call on an
/// <c>EventObserver&lt;T&gt;</c> whose capture session is already active.
/// </summary>
/// <remarks>
/// An observer permits one capture session at a time; starting a second throws
/// <c>WebDriverBiDiException</c>. This is the companion of BIDI020, which reports the opposite
/// mistake — reading captured tasks with no session started.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver030_DuplicateCaptureSessionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI030";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Capture session already started";

    private static readonly LocalizableString MessageFormat = "'StartCapturingTasks()' is called on '{0}', which already has an active capture session. Call 'StopCapturingTasks()' first, or read the captured tasks.";

    private static readonly LocalizableString Description = "An EventObserver allows only one capture session at a time. Calling StartCapturingTasks() while a session is already active throws WebDriverBiDiException at runtime. End the session with StopCapturingTasks(), or let a completed WaitForCapturedTasksAsync or WaitForCapturedTasksCompleteAsync end it, before starting another.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi030");

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
        // Tracks, for each local EventObserver<T> variable, whether a capture session is certainly
        // active at the current point of the walk. Only locally-declared variables are tracked.
        Dictionary<string, bool> capturingState = [];

        // An observer this member hands to other code, or one a nested function opens or closes a
        // session on, may have a session this walk cannot see, so it is never tracked and never
        // reported on. The names are collected up front because the other code may run before or
        // after the StartCapturingTasks textually.
        HashSet<string> untrackableNames = AnalyzerSymbolHelpers.FindVariablesHandedToOtherCode(context.Node);
        untrackableNames.UnionWith(AnalyzerSymbolHelpers.FindVariablesChangedInsideNestedFunctions(context.Node, CaptureSessionMethodNames));

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            ProcessNode(statement, context, capturingState, untrackableNames);
        }
    }

    private static void TrackObserverDeclarations(
        VariableDeclarationSyntax declaration,
        SemanticModel semanticModel,
        Dictionary<string, bool> capturingState,
        HashSet<string> untrackableNames)
    {
        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            if (untrackableNames.Contains(variable.Identifier.ValueText))
            {
                continue;
            }

            ILocalSymbol localSymbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(variable)!;
            if (AnalyzerSymbolHelpers.IsLibraryTypeNamed(localSymbol.Type, "EventObserver"))
            {
                capturingState[variable.Identifier.ValueText] = false;
            }
        }
    }

    private static void ProcessNode(
        SyntaxNode node,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> capturingState,
        HashSet<string> untrackableNames)
    {
        // Walk the node's descendants in document order, checking each invocation against the tracked
        // capturing state. The walk does not descend into the bodies of nested functions: their code
        // runs when the delegate is invoked, not at its textual position. It also stops at if and switch
        // statements — including one that is itself the root, which the barrier yields without descending
        // into — and processes them recursively below with a forked copy of the state for each mutually
        // exclusive branch.
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: child =>
            AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(child) &&
            child is not IfStatementSyntax &&
            child is not SwitchStatementSyntax))
        {
            if (descendant is IfStatementSyntax ifStatement)
            {
                ProcessIfStatement(ifStatement, context, capturingState, untrackableNames);
            }
            else if (descendant is SwitchStatementSyntax switchStatement)
            {
                ProcessSwitchStatement(switchStatement, context, capturingState, untrackableNames);
            }
            else if (descendant is VariableDeclarationSyntax declaration)
            {
                TrackObserverDeclarations(declaration, context.SemanticModel, capturingState, untrackableNames);
            }
            else if (descendant is InvocationExpressionSyntax invocation)
            {
                CheckInvocation(invocation, context, capturingState);
            }
        }
    }

    private static void ProcessIfStatement(
        IfStatementSyntax ifStatement,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> capturingState,
        HashSet<string> untrackableNames)
    {
        // Invocations in the condition execute unconditionally, before either branch.
        ProcessNode(ifStatement.Condition, context, capturingState, untrackableNames);

        Dictionary<string, bool> thenBranchState = new(capturingState);
        ProcessNode(ifStatement.Statement, context, thenBranchState, untrackableNames);

        Dictionary<string, bool> elseBranchState = new(capturingState);
        if (ifStatement.Else is not null)
        {
            ProcessNode(ifStatement.Else.Statement, context, elseBranchState, untrackableNames);
        }

        // After the branch an observer counts as capturing only when every path through the branch
        // leaves a session active. This is the mirror image of the merge in BIDI020, and the polarity
        // follows from what is reported: a second start is a mistake only when a session is certainly
        // active already, so a conditional start must not condemn a later one.
        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = thenBranchState[observerName] && elseBranchState[observerName];
        }
    }

    private static void ProcessSwitchStatement(
        SwitchStatementSyntax switchStatement,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> capturingState,
        HashSet<string> untrackableNames)
    {
        // The governing expression executes unconditionally, before any section.
        ProcessNode(switchStatement.Expression, context, capturingState, untrackableNames);

        List<Dictionary<string, bool>> sectionStates = [];
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            Dictionary<string, bool> sectionState = new(capturingState);
            foreach (StatementSyntax sectionStatement in section.Statements)
            {
                ProcessNode(sectionStatement, context, sectionState, untrackableNames);
            }

            sectionStates.Add(sectionState);
        }

        // As for if statements, an observer counts as capturing after the switch only when every path
        // through it leaves a session active. The unchanged state at the switch is the path taken when
        // no section matches and participates in the merge; when a default section makes that path
        // impossible, including it can only suppress a report, never create a false positive.
        foreach (string observerName in capturingState.Keys.ToList())
        {
            capturingState[observerName] = capturingState[observerName] && sectionStates.All(sectionState => sectionState[observerName]);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
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

        switch (memberAccess.Name.Identifier.ValueText)
        {
            case "StartCapturingTasks":
                if (capturingState[receiverName])
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), receiverName));
                }

                capturingState[receiverName] = true;
                break;

            case "StopCapturingTasks":
                capturingState[receiverName] = false;
                break;

            case "WaitForCapturedTasksAsync":
            case "WaitForCapturedTasksCompleteAsync":
                // A wait that collects its full batch ends the session itself, and one that times out
                // leaves it open. Which of the two happened is a runtime outcome, so the session is no
                // longer certainly active and a later start is not reported.
                capturingState[receiverName] = false;
                break;
        }
    }
}
