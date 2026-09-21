// <copyright file="BiDiDriver021_CaptureSessionOpenedButNeverReadAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi021");

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
        // A member that never names StartCapturingTasks cannot produce this diagnostic, and the test
        // costs a token scan rather than a bind per local declaration.
        if (!AnalyzerSymbolHelpers.ContainsIdentifier(context.Node, "StartCapturingTasks"))
        {
            return;
        }

        SemanticModel semanticModel = context.SemanticModel;

        // For each local EventObserver<T> variable, record the location of the most recent
        // StartCapturing call that has not been satisfied by a read yet.
        Dictionary<string, Location?> pendingStartCapturingTasks = [];

        // Track whether any read was seen after the most recent StartCapturingTasks.
        Dictionary<string, bool> hasRead = [];

        // Track whether a read appears inside a nested function (a lambda, an anonymous method, or
        // a local function). Such a read runs when the delegate is invoked, which the textual walk
        // cannot place, so it is not ordered against the StartCapturingTasks calls; it does mean
        // the session is read, so it satisfies every StartCapturingTasks in the member.
        Dictionary<string, bool> hasDeferredRead = [];

        // An observer this member hands to other code may be read by that code, which this rule
        // cannot see, so it is never tracked and never reported on. A nested function needs no such
        // treatment here: a read inside one is recorded as deferred below. The walk covers the whole
        // body and only an observer declaration asks for its result, so it is deferred until one does.
        Lazy<HashSet<string>> escapedNames = new(() => AnalyzerSymbolHelpers.FindVariablesHandedToOtherCode(context.Node));

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            // Observer declarations are registered wherever they appear: a local declaration
            // statement, a using declaration, or the declaration of a classic using (T x = ...)
            // statement, including inside nested blocks such as try statements. The pre-order walk
            // visits a declaration before any later use of the variable.
            foreach (SyntaxNode node in statement.DescendantNodesAndSelf())
            {
                if (node is VariableDeclarationSyntax declaration)
                {
                    foreach (VariableDeclaratorSyntax variable in declaration.Variables)
                    {
                        // The type test comes first so that the escape walk is forced only by a
                        // declaration that is actually an observer.
                        ILocalSymbol localSymbol = (ILocalSymbol)semanticModel.GetDeclaredSymbol(variable)!;
                        if (AnalyzerSymbolHelpers.IsLibraryTypeNamed(localSymbol.Type, "EventObserver")
                            && !escapedNames.Value.Contains(variable.Identifier.ValueText))
                        {
                            pendingStartCapturingTasks[variable.Identifier.ValueText] = null;
                            hasRead[variable.Identifier.ValueText] = false;
                            hasDeferredRead[variable.Identifier.ValueText] = false;
                        }
                    }

                    continue;
                }

                if (node is not InvocationExpressionSyntax invocation)
                {
                    continue;
                }

                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                {
                    continue;
                }

                if (memberAccess.Expression is not IdentifierNameSyntax receiverIdentifier)
                {
                    continue;
                }

                string receiverName = receiverIdentifier.Identifier.ValueText;
                if (!pendingStartCapturingTasks.ContainsKey(receiverName))
                {
                    continue;
                }

                // A call inside a nested function runs when the delegate is invoked, not at its
                // textual position. A StartCapturingTasks there cannot be judged against the
                // reads that follow it textually, so it is not tracked; a read there is recorded
                // as deferred.
                bool insideNestedFunction = invocation.Ancestors()
                    .TakeWhile(ancestor => ancestor != statement)
                    .Any(ancestor => !AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(ancestor));

                string methodName = memberAccess.Name.Identifier.ValueText;
                switch (methodName)
                {
                    case "StartCapturingTasks" when !insideNestedFunction:
                        pendingStartCapturingTasks[receiverName] = invocation.GetLocation();
                        hasRead[receiverName] = false;
                        break;

                    case "WaitForCapturedTasksAsync":
                    case "WaitForCapturedTasksCompleteAsync":
                    case "GetCapturedTasks":
                        if (insideNestedFunction)
                        {
                            hasDeferredRead[receiverName] = true;
                        }
                        else
                        {
                            hasRead[receiverName] = true;
                        }

                        break;
                }
            }
        }

        // Report a warning for any observer whose last StartCapturingTasks call had no subsequent read.
        foreach (KeyValuePair<string, Location?> kvp in pendingStartCapturingTasks)
        {
            string variableName = kvp.Key;
            Location? startLocation = kvp.Value;
            if (startLocation is not null && !hasRead[variableName] && !hasDeferredRead[variableName])
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, startLocation, variableName));
            }
        }
    }
}
