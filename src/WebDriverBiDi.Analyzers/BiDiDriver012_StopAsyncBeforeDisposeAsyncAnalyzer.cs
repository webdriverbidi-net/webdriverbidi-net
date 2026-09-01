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

    private static readonly LocalizableString Description = "While DisposeAsync internally calls StopAsync, explicitly calling StopAsync before DisposeAsync provides better error handling and distinguishes intentional shutdown from disposal errors. When any TransportErrorBehavior is set to Collect, the collected errors are thrown only by StopAsync; DisposeAsync catches, logs, and discards them.";

    private static readonly LocalizableString CollectModeMessageFormat = "Call StopAsync on '{0}' before calling DisposeAsync; a TransportErrorBehavior is set to Collect, and DisposeAsync discards collected errors without throwing them";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi012");

    // Same ID, category, and default severity as Rule (so release tracking is unchanged), but a
    // message that explains the concrete consequence. It is reported with an effective severity of
    // Warning, because losing collected errors is a silent data loss rather than a style preference.
    private static readonly DiagnosticDescriptor CollectModeRule = new(
        DiagnosticId,
        Title,
        CollectModeMessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi012");

    /// <summary>
    /// The key of the diagnostic property that marks a diagnostic reported on an <c>await using</c>
    /// declaration or statement rather than on a <c>DisposeAsync()</c> invocation.
    /// </summary>
    public const string FormPropertyName = "Form";

    /// <summary>
    /// The value of the <see cref="FormPropertyName"/> property for the <c>await using</c> form.
    /// </summary>
    public const string AwaitUsingFormValue = "AwaitUsing";

    private static readonly string[] ErrorBehaviorPropertyNames =
    [
        "EventHandlerExceptionBehavior",
        "ProtocolErrorBehavior",
        "UnknownMessageBehavior",
        "UnexpectedErrorBehavior",
    ];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, CollectModeRule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        // Find all DisposeAsync invocations in the method, constructor, or top-level program.
        IEnumerable<InvocationExpressionSyntax> disposeAsyncCalls = GetDisposeAsyncInvocations(context.Node, context.SemanticModel);

        // Computed lazily: only needed once a DisposeAsync call without a preceding StopAsync is found.
        bool? hasCollectBehaviorAssignment = null;

        foreach (InvocationExpressionSyntax disposeAsyncCall in disposeAsyncCalls)
        {
            // Get the variable on which DisposeAsync is called. GetDisposeAsyncInvocations only
            // yields invocations whose expression is a member access, so the cast is safe.
            string? driverVariableName = GetDriverVariableName((MemberAccessExpressionSyntax)disposeAsyncCall.Expression);
            if (driverVariableName == null)
            {
                continue;
            }

            // Check if StopAsync was called on the same variable before DisposeAsync
            bool hasStopAsyncBefore = HasStopAsyncBefore(context.Node, driverVariableName, disposeAsyncCall);

            if (!hasStopAsyncBefore)
            {
                hasCollectBehaviorAssignment ??= HasCollectBehaviorAssignment(context.Node, context.SemanticModel);
                Diagnostic diagnostic = hasCollectBehaviorAssignment.Value
                    ? Diagnostic.Create(CollectModeRule, disposeAsyncCall.GetLocation(), DiagnosticSeverity.Warning, additionalLocations: null, properties: null, driverVariableName)
                    : Diagnostic.Create(Rule, disposeAsyncCall.GetLocation(), driverVariableName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        // The `await using` forms dispose the driver implicitly at the end of the enclosing
        // scope, so there is no DisposeAsync() invocation to find. A StopAsync() anywhere later
        // in that scope runs before the implicit disposal and counts as "before".
        foreach ((Location location, string driverVariableName, IEnumerable<StatementSyntax> scope) in GetAwaitUsingDrivers(context.Node, context.SemanticModel))
        {
            if (!ContainsStopAsync(scope, driverVariableName))
            {
                hasCollectBehaviorAssignment ??= HasCollectBehaviorAssignment(context.Node, context.SemanticModel);
                ImmutableDictionary<string, string?> properties = ImmutableDictionary<string, string?>.Empty.Add(FormPropertyName, AwaitUsingFormValue);
                Diagnostic diagnostic = hasCollectBehaviorAssignment.Value
                    ? Diagnostic.Create(CollectModeRule, location, DiagnosticSeverity.Warning, additionalLocations: null, properties: properties, driverVariableName)
                    : Diagnostic.Create(Rule, location, properties, driverVariableName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    /// <summary>
    /// Finds every driver that is disposed implicitly by an <c>await using</c> declaration
    /// (<c>await using var driver = ...;</c>) or an <c>await using</c> statement
    /// (<c>await using (driver) { ... }</c>), together with the statements that run before the
    /// implicit disposal.
    /// </summary>
    /// <param name="method">The method being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the method.</param>
    /// <returns>The location to report, the driver variable name, and the statements in scope for each driver.</returns>
    private static IEnumerable<(Location Location, string DriverVariableName, IEnumerable<StatementSyntax> Scope)> GetAwaitUsingDrivers(
        SyntaxNode node,
        SemanticModel semanticModel)
    {
        foreach (LocalDeclarationStatementSyntax declaration in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<LocalDeclarationStatementSyntax>())
        {
            // A using declaration is only permitted directly inside a block (CS8647 otherwise), and
            // the implicit disposal happens at the end of that block, so every statement after the
            // declaration in the block runs before it. Code that violates the placement rule has no
            // well-defined scope and is skipped.
            if (declaration.AwaitKeyword.IsKind(SyntaxKind.None) || declaration.UsingKeyword.IsKind(SyntaxKind.None) || declaration.Parent is not BlockSyntax block)
            {
                continue;
            }

            // The declared type covers both explicitly typed and `var` declarations.
            if (!AnalyzerSymbolHelpers.IsCommandExecutorType(semanticModel.GetTypeInfo(declaration.Declaration.Type).Type))
            {
                continue;
            }

            IEnumerable<StatementSyntax> scope = block.Statements.SkipWhile(s => s != declaration).Skip(1);
            foreach (VariableDeclaratorSyntax declarator in declaration.Declaration.Variables)
            {
                yield return (declarator.Identifier.GetLocation(), declarator.Identifier.Text, scope);
            }
        }

        foreach (UsingStatementSyntax usingStatement in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<UsingStatementSyntax>())
        {
            if (usingStatement.AwaitKeyword.IsKind(SyntaxKind.None))
            {
                continue;
            }

            if (usingStatement.Declaration is not null)
            {
                if (AnalyzerSymbolHelpers.IsCommandExecutorType(semanticModel.GetTypeInfo(usingStatement.Declaration.Type).Type))
                {
                    foreach (VariableDeclaratorSyntax declarator in usingStatement.Declaration.Variables)
                    {
                        yield return (declarator.Identifier.GetLocation(), declarator.Identifier.Text, [usingStatement.Statement]);
                    }
                }
            }
            else if (usingStatement.Expression is IdentifierNameSyntax identifier && AnalyzerSymbolHelpers.IsCommandExecutorType(semanticModel.GetTypeInfo(identifier).Type))
            {
                yield return (identifier.GetLocation(), identifier.Identifier.Text, [usingStatement.Statement]);
            }
        }
    }

    private static bool ContainsStopAsync(IEnumerable<StatementSyntax> statements, string variableName)
    {
        return statements.Any(s => s.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(inv => inv.Expression is MemberAccessExpressionSyntax ma
                && ma.Name.Identifier.Text == "StopAsync"
                && ma.Expression is IdentifierNameSyntax id
                && id.Identifier.Text == variableName));
    }

    /// <summary>
    /// Determines whether the method assigns <c>TransportErrorBehavior.Collect</c> to any of the
    /// four error-behavior properties, on any receiver (a <c>BiDiDriver</c>, a <c>Transport</c>, or
    /// an object initializer for either), so that the diagnostic can explain the consequence of
    /// disposing without stopping.
    /// </summary>
    /// <param name="method">The method being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the method.</param>
    /// <returns><see langword="true"/> if a <c>Collect</c> assignment is present; otherwise <see langword="false"/>.</returns>
    private static bool HasCollectBehaviorAssignment(SyntaxNode node, SemanticModel semanticModel)
    {
        foreach (AssignmentExpressionSyntax assignment in AnalyzerSymbolHelpers.GetBodyDescendantNodes(node).OfType<AssignmentExpressionSyntax>())
        {
            if (!IsErrorBehaviorProperty(assignment.Left))
            {
                continue;
            }

            if (semanticModel.GetSymbolInfo(assignment.Right).Symbol is IFieldSymbol { Name: "Collect" } field
                && field.ContainingType.Name == "TransportErrorBehavior")
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsErrorBehaviorProperty(ExpressionSyntax assignmentTarget)
    {
        // Handles both `driver.ProtocolErrorBehavior = ...` (member access) and the object
        // initializer form `new Transport(connection) { ProtocolErrorBehavior = ... }` (identifier).
        string? propertyName = assignmentTarget switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => null,
        };

        return propertyName is not null && ErrorBehaviorPropertyNames.Contains(propertyName);
    }

    private static IEnumerable<InvocationExpressionSyntax> GetDisposeAsyncInvocations(
        SyntaxNode node,
        SemanticModel semanticModel)
    {
        IEnumerable<InvocationExpressionSyntax>? invocations = AnalyzerSymbolHelpers.GetBodyDescendantNodes(node)
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Name.Identifier.Text == "DisposeAsync")
                {
                    ITypeSymbol? receiverType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                    if (AnalyzerSymbolHelpers.IsCommandExecutorType(receiverType))
                    {
                        return true;
                    }
                }

                return false;
            });

        return invocations;
    }

    private static string? GetDriverVariableName(MemberAccessExpressionSyntax memberAccess)
    {
        if (memberAccess.Expression is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.Text;
        }

        return null;
    }

    private static bool HasStopAsyncBefore(
        SyntaxNode node,
        string variableName,
        InvocationExpressionSyntax disposeAsyncCall)
    {
        // First, try to find StopAsync in the same containing block as DisposeAsync
        SyntaxNode containingBlock = GetContainingBlock(disposeAsyncCall);
        if (HasStopAsyncBeforeInBlock(containingBlock, variableName, disposeAsyncCall))
        {
            return true;
        }

        // A DisposeAsync inside a finally clause runs after the associated try block, so a
        // StopAsync anywhere in that try block executes before it even though the two calls
        // share no containing block. A StopAsync that appears only in a catch block is
        // deliberately not counted: it runs only on the exceptional path, so the normal path
        // would still dispose a driver that was never stopped.
        for (SyntaxNode? ancestor = disposeAsyncCall.Parent; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (!AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(ancestor))
            {
                // The DisposeAsync call runs when the enclosing delegate is invoked, not at its
                // textual position, so an outer try block's timing does not apply to it.
                break;
            }

            if (ancestor is FinallyClauseSyntax finallyClause &&
                finallyClause.Parent is TryStatementSyntax tryStatement &&
                ContainsStopAsyncForVariable(tryStatement.Block, variableName))
            {
                return true;
            }
        }

        // Fall back to checking at the member level (method, constructor, or top-level program).
        IEnumerable<StatementSyntax>? statements = AnalyzerSymbolHelpers.GetTopLevelStatements(node);
        return HasStopAsyncBeforeInStatements(statements, variableName, disposeAsyncCall);
    }

    private static bool ContainsStopAsyncForVariable(SyntaxNode scope, string variableName)
    {
        return scope.DescendantNodes(descendIntoChildren: AnalyzerSymbolHelpers.DoesNotBeginNestedFunction)
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation => invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "StopAsync"
                && memberAccess.Expression is IdentifierNameSyntax identifier
                && identifier.Identifier.Text == variableName);
    }

    private static SyntaxNode GetContainingBlock(SyntaxNode node)
    {
        SyntaxNode? current = node.Parent;
        while (true)
        {
            if (current is BlockSyntax or MethodDeclarationSyntax or ConstructorDeclarationSyntax or CompilationUnitSyntax)
            {
                return current!;
            }

            current = current!.Parent;
        }
    }

    private static bool HasStopAsyncBeforeInBlock(SyntaxNode block, string variableName, InvocationExpressionSyntax disposeAsyncCall)
    {
        IEnumerable<StatementSyntax> statements = block is BlockSyntax blockSyntax
            ? blockSyntax.Statements
            : AnalyzerSymbolHelpers.GetTopLevelStatements(block);

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

        // Look for StopAsync calls on the same variable in all statements before DisposeAsync.
        return statements
            .TakeWhile(s => s != disposeStatement)
            .Any(s => s.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv => inv.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "StopAsync"
                    && ma.Expression is IdentifierNameSyntax id
                    && id.Identifier.Text == variableName));
    }

}
