// <copyright file="BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that suggests calling StopAsync before DisposeAsync on BiDiDriver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI012";

    private const string Category = "Design";

    private static readonly LocalizableString Title = "Call StopAsync before DisposeAsync";

    private static readonly LocalizableString MessageFormat = "Consider calling StopAsync on '{0}' before calling DisposeAsync for cleaner shutdown";

    private static readonly LocalizableString Description = "While DisposeAsync internally calls StopAsync, explicitly calling StopAsync before DisposeAsync provides better error handling and distinguishes intentional shutdown from disposal errors. This follows best practices for resource cleanup.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
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

        // Find all DisposeAsync invocations in the method
        IEnumerable<InvocationExpressionSyntax> disposeAsyncCalls = GetDisposeAsyncInvocations(methodDeclaration, context.SemanticModel);

        foreach (InvocationExpressionSyntax disposeAsyncCall in disposeAsyncCalls)
        {
            // Get the variable on which DisposeAsync is called
            string? driverVariableName = GetDriverVariableName(disposeAsyncCall);
            if (driverVariableName == null)
            {
                continue;
            }

            // Check if StopAsync was called on the same variable before DisposeAsync
            bool hasStopAsyncBefore = HasStopAsyncBefore(methodDeclaration, driverVariableName, disposeAsyncCall);

            if (!hasStopAsyncBefore)
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, disposeAsyncCall.GetLocation(), driverVariableName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static IEnumerable<InvocationExpressionSyntax> GetDisposeAsyncInvocations(
        MethodDeclarationSyntax method,
        SemanticModel semanticModel)
    {
        IEnumerable<InvocationExpressionSyntax>? invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Name.Identifier.Text == "DisposeAsync")
                {
                    // Verify it's actually a BiDiDriver or IBiDiDriver
                    IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (methodSymbol != null)
                    {
                        INamedTypeSymbol? containingType = methodSymbol.ContainingType;
                        return containingType?.Name == "BiDiDriver" || containingType?.Name == "IBiDiDriver";
                    }
                }

                return false;
            });

        return invocations;
    }

    private static string? GetDriverVariableName(InvocationExpressionSyntax invocation)
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

    private static bool HasStopAsyncBefore(
        MethodDeclarationSyntax method,
        string variableName,
        InvocationExpressionSyntax disposeAsyncCall)
    {
        // First, try to find StopAsync in the same containing block as DisposeAsync
        SyntaxNode? containingBlock = GetContainingBlock(disposeAsyncCall);
        if (containingBlock != null)
        {
            if (HasStopAsyncBeforeInBlock(containingBlock, variableName, disposeAsyncCall))
            {
                return true;
            }
        }

        // Fall back to checking at the method level
        IEnumerable<StatementSyntax>? statements = GetStatements(method);
        return HasStopAsyncBeforeInStatements(statements, variableName, disposeAsyncCall);
    }

    private static SyntaxNode? GetContainingBlock(SyntaxNode node)
    {
        SyntaxNode? current = node.Parent;
        while (current != null)
        {
            if (current is BlockSyntax or MethodDeclarationSyntax)
            {
                return current;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool HasStopAsyncBeforeInBlock(SyntaxNode block, string variableName, InvocationExpressionSyntax disposeAsyncCall)
    {
        IEnumerable<StatementSyntax>? statements = block switch
        {
            BlockSyntax blockSyntax => blockSyntax.Statements,
            MethodDeclarationSyntax method => GetStatements(method),
            _ => Enumerable.Empty<StatementSyntax>(),
        };

        return HasStopAsyncBeforeInStatements(statements, variableName, disposeAsyncCall);
    }

    private static bool HasStopAsyncBeforeInStatements(
        IEnumerable<StatementSyntax> statements,
        string variableName,
        InvocationExpressionSyntax disposeAsyncCall)
    {
        // Find the statement containing the DisposeAsync call
        StatementSyntax? disposeStatement = statements.FirstOrDefault(s => s.Contains(disposeAsyncCall));
        if (disposeStatement == null)
        {
            return false;
        }

        // Look for StopAsync calls on the same variable before the DisposeAsync call
        foreach (StatementSyntax statement in statements)
        {
            // Stop when we reach the DisposeAsync statement
            if (statement == disposeStatement)
            {
                break;
            }

            // Check if this statement contains a StopAsync call on the variable
            IEnumerable<InvocationExpressionSyntax>? stopAsyncCalls = statement.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation =>
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name.Identifier.Text == "StopAsync")
                    {
                        if (memberAccess.Expression is IdentifierNameSyntax identifier)
                        {
                            return identifier.Identifier.Text == variableName;
                        }
                    }

                    return false;
                });

            if (stopAsyncCalls.Any())
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<StatementSyntax> GetStatements(MethodDeclarationSyntax method)
    {
        if (method.Body != null)
        {
            return method.Body.Statements;
        }

        return Enumerable.Empty<StatementSyntax>();
    }
}
