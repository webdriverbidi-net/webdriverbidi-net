// <copyright file="BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects UnsetCheckpoint calls without prior GetCheckpointTasks call.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI019";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "UnsetCheckpoint called without retrieving captured tasks";

    private static readonly LocalizableString MessageFormat = "UnsetCheckpoint was called on '{0}' without first calling GetCheckpointTasks to retrieve captured tasks, which may lead to unobserved task exceptions";

    private static readonly LocalizableString Description = "When UnsetCheckpoint is called on an EventObserver, any tasks that were captured during checkpoint fulfillment will be abandoned. To ensure that captured tasks are properly observed and awaited, call GetCheckpointTasks before calling UnsetCheckpoint. This transfers ownership of the tasks to your code so you can await them and handle any exceptions.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (methodDeclaration.Body == null && methodDeclaration.ExpressionBody == null)
        {
            return;
        }

        // Find all UnsetCheckpoint invocations in the method
        IEnumerable<InvocationExpressionSyntax> unsetCheckpointCalls = GetUnsetCheckpointInvocations(methodDeclaration);

        foreach (InvocationExpressionSyntax unsetCheckpointCall in unsetCheckpointCalls)
        {
            // Get the variable on which UnsetCheckpoint is called
            string? observerVariableName = GetObserverVariableName(unsetCheckpointCall);
            if (observerVariableName == null)
            {
                continue;
            }

            // Check if GetCheckpointTasks was called on the same variable before UnsetCheckpoint
            bool hasGetCheckpointTasksCall = HasGetCheckpointTasksBeforeUnset(methodDeclaration, observerVariableName, unsetCheckpointCall);

            if (!hasGetCheckpointTasksCall)
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, unsetCheckpointCall.GetLocation(), observerVariableName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static IEnumerable<InvocationExpressionSyntax> GetUnsetCheckpointInvocations(MethodDeclarationSyntax method)
    {
        IEnumerable<InvocationExpressionSyntax>? invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess.Name.Identifier.Text == "UnsetCheckpoint";
                }

                return false;
            });

        return invocations;
    }

    private static string? GetObserverVariableName(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Handle simple identifier: observer.UnsetCheckpoint()
            if (memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                return identifier.Identifier.Text;
            }

            // Handle this-qualified member access: this.observer.UnsetCheckpoint()
            if (memberAccess.Expression is MemberAccessExpressionSyntax nestedMemberAccess &&
                nestedMemberAccess.Expression is ThisExpressionSyntax)
            {
                return nestedMemberAccess.Name.Identifier.Text;
            }
        }

        return null;
    }

    private static bool HasGetCheckpointTasksBeforeUnset(MethodDeclarationSyntax method, string variableName, InvocationExpressionSyntax unsetCheckpointCall)
    {
        // Get all statements in the method body
        SyntaxNode? methodBody = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (methodBody == null)
        {
            return false;
        }

        // Find all GetCheckpointTasks invocations on the same variable
        IEnumerable<InvocationExpressionSyntax> getTasksInvocations = methodBody.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Name.Identifier.Text == "GetCheckpointTasks")
                {
                    // Check if this is a GetCheckpointTasks call on the same variable
                    // Handle both: observer.GetCheckpointTasks() and this.observer.GetCheckpointTasks()
                    string? invocationVariableName = null;

                    if (memberAccess.Expression is IdentifierNameSyntax identifier)
                    {
                        invocationVariableName = identifier.Identifier.Text;
                    }
                    else if (memberAccess.Expression is MemberAccessExpressionSyntax nestedMemberAccess &&
                             nestedMemberAccess.Expression is ThisExpressionSyntax)
                    {
                        invocationVariableName = nestedMemberAccess.Name.Identifier.Text;
                    }

                    if (invocationVariableName == variableName)
                    {
                        return true;
                    }
                }

                return false;
            });

        // Check if any GetCheckpointTasks call appears before the UnsetCheckpoint call
        // We use SpanStart to determine the order of statements in the source code
        int unsetCheckpointPosition = unsetCheckpointCall.SpanStart;

        foreach (InvocationExpressionSyntax getTasksCall in getTasksInvocations)
        {
            // If GetCheckpointTasks appears before UnsetCheckpoint in the source, we're good
            if (getTasksCall.SpanStart < unsetCheckpointPosition)
            {
                // Also check that they're in the same scope or GetCheckpointTasks is in a parent scope
                if (AreInSameScopeOrParentScope(getTasksCall, unsetCheckpointCall))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool AreInSameScopeOrParentScope(InvocationExpressionSyntax getTasksCall, InvocationExpressionSyntax unsetCheckpointCall)
    {
        // Get the containing blocks for both calls
        BlockSyntax? getTasksBlock = getTasksCall.FirstAncestorOrSelf<BlockSyntax>();
        BlockSyntax? unsetBlock = unsetCheckpointCall.FirstAncestorOrSelf<BlockSyntax>();

        if (getTasksBlock == null || unsetBlock == null)
        {
            return true; // If we can't determine scope, assume they're compatible
        }

        // Check if they're in the same block
        if (getTasksBlock == unsetBlock)
        {
            return true;
        }

        // Check if GetCheckpointTasks is in a parent scope of UnsetCheckpoint
        BlockSyntax? currentBlock = unsetBlock;
        while (currentBlock != null)
        {
            if (currentBlock == getTasksBlock)
            {
                return true;
            }

            currentBlock = currentBlock.Parent?.FirstAncestorOrSelf<BlockSyntax>();
        }

        // Special case: Check if GetCheckpointTasks is in a try block and UnsetCheckpoint is in
        // the corresponding finally block
        if (IsTryFinallyPattern(getTasksCall, unsetCheckpointCall))
        {
            return true;
        }

        return false;
    }

    private static bool IsTryFinallyPattern(InvocationExpressionSyntax getTasksCall, InvocationExpressionSyntax unsetCheckpointCall)
    {
        // Check if UnsetCheckpoint is in a finally clause
        FinallyClauseSyntax? finallyClause = unsetCheckpointCall.FirstAncestorOrSelf<FinallyClauseSyntax>();
        if (finallyClause == null)
        {
            return false;
        }

        // Get the try statement that owns this finally clause
        TryStatementSyntax? tryStatement = finallyClause.Parent as TryStatementSyntax;
        if (tryStatement == null)
        {
            return false;
        }

        // Check if GetCheckpointTasks is within the try block of the same try statement
        BlockSyntax tryBlock = tryStatement.Block;
        return IsWithinBlock(getTasksCall, tryBlock);
    }

    private static bool IsWithinBlock(InvocationExpressionSyntax invocation, BlockSyntax block)
    {
        SyntaxNode? current = invocation.Parent;
        while (current != null)
        {
            if (current == block)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }
}
