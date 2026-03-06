// <copyright file="BiDiDriver011_EventObserverCheckpointMisuseAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects checkpoint misuse on EventObserver instances.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver011_EventObserverCheckpointMisuseAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI011";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "EventObserver checkpoint should be waited for or unset";

    private static readonly LocalizableString MessageFormat = "SetCheckpoint was called on '{0}' but the checkpoint is never waited for or unset before the method exits";

    private static readonly LocalizableString Description = "When SetCheckpoint is called on an EventObserver, the checkpoint must be waited for using WaitForCheckpointAsync/WaitForCheckpointAndTasksAsync, have its tasks retrieved using GetCheckpointTasks, or be explicitly unset using UnsetCheckpoint. Leaving a checkpoint set causes resource leaks and prevents setting new checkpoints.";

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

        // Find all SetCheckpoint invocations in the method
        IEnumerable<InvocationExpressionSyntax> setCheckpointCalls = GetSetCheckpointInvocations(methodDeclaration);

        foreach (InvocationExpressionSyntax setCheckpointCall in setCheckpointCalls)
        {
            // Get the variable on which SetCheckpoint is called
            string? observerVariableName = GetObserverVariableName(setCheckpointCall);
            if (observerVariableName == null)
            {
                continue;
            }

            // Check if the same variable has checkpoint handling methods called on it
            bool hasCheckpointHandling = HasCheckpointHandlingMethod(methodDeclaration, observerVariableName);

            if (!hasCheckpointHandling)
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, setCheckpointCall.GetLocation(), observerVariableName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static IEnumerable<InvocationExpressionSyntax> GetSetCheckpointInvocations(MethodDeclarationSyntax method)
    {
        IEnumerable<InvocationExpressionSyntax>? invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess.Name.Identifier.Text == "SetCheckpoint";
                }

                return false;
            });

        return invocations;
    }

    private static string? GetObserverVariableName(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                return identifier.Identifier.Text;
            }
        }

        return null;
    }

    private static bool HasCheckpointHandlingMethod(MethodDeclarationSyntax method, string variableName)
    {
        string[] checkpointHandlingMethods =
        [
            "WaitForCheckpointAsync",
            "WaitForCheckpointAndTasksAsync",
            "GetCheckpointTasks",
            "UnsetCheckpoint",
        ];

        IEnumerable<InvocationExpressionSyntax>? invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    // Check if this is a call on the same variable
                    if (memberAccess.Expression is IdentifierNameSyntax identifier &&
                        identifier.Identifier.Text == variableName)
                    {
                        return checkpointHandlingMethods.Contains(memberAccess.Name.Identifier.Text);
                    }
                }

                return false;
            });

        return invocations.Any();
    }
}
