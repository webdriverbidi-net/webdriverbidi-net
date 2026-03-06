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
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (methodDeclaration.Body == null && methodDeclaration.ExpressionBody == null)
        {
            return;
        }

        // Find all local variable declarations that store AddObserver() results
        Dictionary<string, LocalDeclarationStatementSyntax> observerVariables = [];

        IEnumerable<LocalDeclarationStatementSyntax> localDeclarations = methodDeclaration.DescendantNodes()
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

            // Check if Unobserve() or Dispose() is called on this variable
            if (HasDisposalCall(context, methodDeclaration, variableName))
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

        // Check if the return type is EventObserver<T>
        INamedTypeSymbol? returnType = methodSymbol.ReturnType as INamedTypeSymbol;
        if (returnType == null)
        {
            return false;
        }

        return returnType.Name == "EventObserver";
    }

    private static bool IsInUsingStatement(LocalDeclarationStatementSyntax declaration)
    {
        // Check if this is a using declaration (C# 8.0+)
        if (declaration.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
        {
            return true;
        }

        // Check if this declaration is inside a using statement
        SyntaxNode? parent = declaration.Parent;
        while (parent != null)
        {
            if (parent is UsingStatementSyntax usingStatement)
            {
                // Check if the declaration is the resource of the using statement
                if (usingStatement.Declaration != null && usingStatement.Declaration.Variables.Any(v => declaration.Declaration.Variables.Any(dv => dv.Identifier.Text == v.Identifier.Text)))
                {
                    return true;
                }
            }

            parent = parent.Parent;
        }

        return false;
    }

    private static bool HasDisposalCall(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        string variableName)
    {
        // Look for method invocations on the variable
        IEnumerable<InvocationExpressionSyntax> invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();

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

        // Check for await using statements
        IEnumerable<AwaitExpressionSyntax> awaitExpressions = methodDeclaration.DescendantNodes()
            .OfType<AwaitExpressionSyntax>();

        foreach (AwaitExpressionSyntax awaitExpression in awaitExpressions)
        {
            if (awaitExpression.Expression is InvocationExpressionSyntax awaitedInvocation && awaitedInvocation.Expression is MemberAccessExpressionSyntax awaitedMemberAccess)
            {
                if (awaitedMemberAccess.Expression is IdentifierNameSyntax awaitedIdentifier && awaitedIdentifier.Identifier.Text == variableName && awaitedMemberAccess.Name.Identifier.Text == "DisposeAsync")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
