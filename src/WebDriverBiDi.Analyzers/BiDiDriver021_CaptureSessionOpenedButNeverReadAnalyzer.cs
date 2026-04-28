// <copyright file="BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects <see cref="EventObserver{T}.StartCapturingTasks"/> calls that are never
/// followed by a read method (<see cref="EventObserver{T}.WaitForCapturedTasksAsync"/>,
/// <see cref="EventObserver{T}.WaitForCapturedTasksCompleteAsync"/>, or
/// <see cref="EventObserver{T}.GetCapturedTasks"/>) in the same method body.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI021";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Capture session opened but never read";

    private static readonly LocalizableString MessageFormat = "'{0}' has StartCapturingTasks() called but its captured tasks are never retrieved in this method. Call WaitForCapturedTasksAsync(), WaitForCapturedTasksCompleteAsync(), or GetCapturedTasks() to consume the captured tasks.";

    private static readonly LocalizableString Description = "Starting a capture session without reading its results is likely a mistake. Call WaitForCapturedTasksAsync(), WaitForCapturedTasksCompleteAsync(), or GetCapturedTasks() to retrieve the captured handler tasks; otherwise the capture session serves no purpose and any handler task exceptions may go unobserved.";

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

        // For each local EventObserver<T> variable, record the location of the most recent
        // StartCapturing call that has not been satisfied by a read yet.
        Dictionary<string, Location?> pendingStartCapturingTasks = [];

        // Track whether any read was seen after the most recent StartCapturingTasks.
        Dictionary<string, bool> hasRead = [];

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
                        pendingStartCapturingTasks[variable.Identifier.Text] = null;
                        hasRead[variable.Identifier.Text] = false;
                    }
                }
            }

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
                if (!pendingStartCapturingTasks.ContainsKey(receiverName))
                {
                    continue;
                }

                string methodName = memberAccess.Name.Identifier.Text;
                switch (methodName)
                {
                    case "StartCapturingTasks":
                        pendingStartCapturingTasks[receiverName] = invocation.GetLocation();
                        hasRead[receiverName] = false;
                        break;

                    case "WaitForCapturedTasksAsync":
                    case "WaitForCapturedTasksCompleteAsync":
                    case "GetCapturedTasks":
                        hasRead[receiverName] = true;
                        break;
                }
            }
        }

        // Report a warning for any observer whose last StartCapturingTasks call had no subsequent read.
        foreach (KeyValuePair<string, Location?> kvp in pendingStartCapturingTasks)
        {
            string variableName = kvp.Key;
            Location? startLocation = kvp.Value;
            if (startLocation is not null && !hasRead[variableName])
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, startLocation, variableName));
            }
        }
    }
}
