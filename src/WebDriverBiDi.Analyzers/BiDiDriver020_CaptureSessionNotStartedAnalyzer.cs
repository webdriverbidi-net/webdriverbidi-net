// <copyright file="BiDiDriver020_CaptureSessionNotStartedAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects <see cref="EventObserver{T}.WaitForAsync"/> or
/// <see cref="EventObserver{T}.WaitForCapturedTasksAsync"/> calls on an observer that has no
/// active capture session (i.e., <see cref="EventObserver{T}.StartCapturing"/> was not called
/// first in the same method).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver020_CaptureSessionNotStartedAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI020";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Capture session not started";

    private static readonly LocalizableString MessageFormat = "'{0}' is called on '{1}' but no capture session is active. Call 'StartCapturing()' before calling '{0}'.";

    private static readonly LocalizableString Description = "WaitForAsync and WaitForCapturedTasksAsync require an active capture session. Call StartCapturing() before invoking these methods; calling them without an active session throws InvalidOperationException at runtime.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)context.Node;
        if (methodDeclaration.Body == null)
        {
            return;
        }

        SemanticModel semanticModel = context.SemanticModel;

        // Track whether StartCapturing has been seen for each local EventObserver<T> variable.
        // Only locally-declared variables are tracked; parameter-passed observers are not.
        Dictionary<string, bool> capturingState = [];

        foreach (StatementSyntax statement in methodDeclaration.Body.Statements)
        {
            // Register newly declared EventObserver<T> local variables.
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
                {
                    ILocalSymbol? localSymbol = semanticModel.GetDeclaredSymbol(variable) as ILocalSymbol;
                    if (localSymbol?.Type is INamedTypeSymbol { Name: "EventObserver" })
                    {
                        capturingState[variable.Identifier.Text] = false;
                    }
                }
            }

            // Walk all invocations in this statement (including those inside nested blocks).
            // DescendantNodes visits in source order, so a StartCapturing inside an if-block
            // that precedes a WaitForAsync outside it is correctly seen first.
            foreach (InvocationExpressionSyntax invocation in statement.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                {
                    continue;
                }

                if (memberAccess.Expression is not IdentifierNameSyntax receiverIdentifier)
                {
                    continue;
                }

                string receiverName = receiverIdentifier.Identifier.Text;
                if (!capturingState.ContainsKey(receiverName))
                {
                    continue;
                }

                string methodName = memberAccess.Name.Identifier.Text;
                switch (methodName)
                {
                    case "StartCapturing":
                        capturingState[receiverName] = true;
                        break;

                    case "StopCapturing":
                        capturingState[receiverName] = false;
                        break;

                    case "WaitForAsync":
                    case "WaitForCapturedTasksAsync":
                        if (!capturingState[receiverName])
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), methodName, receiverName));
                        }

                        break;
                }
            }
        }
    }
}
