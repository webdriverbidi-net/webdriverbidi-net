// <copyright file="BiDiDriver006_ObserverDisposalAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects EventObserver instances created without proper disposal.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver006_ObserverDisposalAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI006";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "EventObserver should be disposed";

    private static readonly LocalizableString MessageFormat = "EventObserver '{0}' is not disposed. Consider using a 'using' statement or calling Unobserve()/Dispose() when done.";

    private static readonly LocalizableString Description = "EventObserver instances should be disposed to unregister event handlers and prevent memory leaks. Use a 'using' statement or explicitly call Unobserve() or Dispose() when the observer is no longer needed.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi006");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        // Find all local variable declarations that store AddObserver() results
        Dictionary<string, LocalDeclarationStatementSyntax> observerVariables = [];

        IEnumerable<LocalDeclarationStatementSyntax> localDeclarations = AnalyzerSymbolHelpers.GetBodyDescendantNodes(context.Node)
            .OfType<LocalDeclarationStatementSyntax>();

        foreach (LocalDeclarationStatementSyntax localDeclaration in localDeclarations)
        {
            foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
            {
                if (variable.Initializer?.Value is InvocationExpressionSyntax invocation)
                {
                    if (IsAddObserverCall(context, invocation))
                    {
                        observerVariables[variable.Identifier.Text] = localDeclaration;
                    }
                }
            }
        }

        if (observerVariables.Count == 0)
        {
            return;
        }

        // Check if observers are disposed
        foreach (KeyValuePair<string, LocalDeclarationStatementSyntax> kvp in observerVariables)
        {
            string variableName = kvp.Key;
            LocalDeclarationStatementSyntax declaration = kvp.Value;

            // Check if it's in a using statement
            if (IsInUsingStatement(declaration))
            {
                continue;
            }

            // Skip when the observer is disposed, released by id, returned, or stored elsewhere.
            if (IsObserverHandled(context.Node, variableName))
            {
                continue;
            }

            // Report diagnostic on just the variable identifier
            VariableDeclaratorSyntax variable = declaration.Declaration.Variables.First(v => v.Identifier.Text == variableName);
            Location location = variable.Identifier.GetLocation();
            Diagnostic diagnostic = Diagnostic.Create(Rule, location, variableName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsAddObserverCall(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        if (memberAccess.Name.Identifier.Text != "AddObserver")
        {
            return false;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return false;
        }

        return methodSymbol.ReturnType is INamedTypeSymbol { Name: "EventObserver" };
    }

    private static bool IsInUsingStatement(LocalDeclarationStatementSyntax declaration)
    {
        // Check if this is a using declaration (C# 8.0+)
        if (declaration.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
        {
            return true;
        }

        return false;
    }

    private static bool IsObserverHandled(SyntaxNode node, string variableName)
    {
        // The observer is not leaked when it is disposed directly, released through
        // ObservableEvent.RemoveObserver(observer.Id), returned to the caller, or stored elsewhere
        // (for example assigned to a field) so another owner disposes it later.
        return HasDisposalCall(node, variableName)
            || IsReleasedViaRemoveObserver(node, variableName)
            || IsReturnedOrStored(node, variableName);
    }

    private static bool HasDisposalCall(SyntaxNode node, string variableName)
    {
        // Look for method invocations on the variable
        IEnumerable<InvocationExpressionSyntax> invocations = AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<InvocationExpressionSyntax>();

        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                // Check if the expression is calling a method on our variable
                string? expressionName = null;
                if (memberAccess.Expression is IdentifierNameSyntax identifier)
                {
                    expressionName = identifier.Identifier.Text;
                }

                if (expressionName == variableName)
                {
                    string methodName = memberAccess.Name.Identifier.Text;
                    if (methodName == "Unobserve" || methodName == "Dispose" || methodName == "DisposeAsync")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool IsReleasedViaRemoveObserver(SyntaxNode node, string variableName)
    {
        // Look for a RemoveObserver call whose argument is the observer's Id (for example
        // driver.Log.OnEntryAdded.RemoveObserver(observer.Id)).
        foreach (InvocationExpressionSyntax invocation in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
                memberAccess.Name.Identifier.Text != "RemoveObserver")
            {
                continue;
            }

            foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
            {
                if (argument.Expression is MemberAccessExpressionSyntax argumentAccess &&
                    argumentAccess.Name.Identifier.Text == "Id" &&
                    argumentAccess.Expression is IdentifierNameSyntax argumentIdentifier &&
                    argumentIdentifier.Identifier.Text == variableName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsReturnedOrStored(SyntaxNode node, string variableName)
    {
        // Returned to the caller: return observer;
        foreach (ReturnStatementSyntax returnStatement in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<ReturnStatementSyntax>())
        {
            if (returnStatement.Expression is IdentifierNameSyntax returned && returned.Identifier.Text == variableName)
            {
                return true;
            }
        }

        // Assigned to another target (for example a field): this.observer = observer;
        foreach (AssignmentExpressionSyntax assignment in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Right is IdentifierNameSyntax assigned && assigned.Identifier.Text == variableName)
            {
                return true;
            }
        }

        return false;
    }
}
