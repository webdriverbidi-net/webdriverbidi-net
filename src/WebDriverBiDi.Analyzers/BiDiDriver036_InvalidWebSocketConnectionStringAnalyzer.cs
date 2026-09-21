// <copyright file="BiDiDriver036_InvalidWebSocketConnectionStringAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects a constant connection string that is not an absolute <c>ws://</c> or <c>wss://</c> URL passed
/// to <c>StartAsync</c> on a driver constructed with the default transport. That transport connects over a WebSocket,
/// and rejects any other connection string with an <c>ArgumentException</c> before it attempts to connect.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver036_InvalidWebSocketConnectionStringAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI036";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Connection string is not a WebSocket URL";

    private static readonly LocalizableString MessageFormat = "'{0}' is not an absolute ws:// or wss:// URL, so StartAsync throws an ArgumentException: this driver was constructed with the default transport, which connects over a WebSocket. Pass the browser's WebSocket URL, or construct the driver with a Transport over the connection this string is meant for.";

    private static readonly LocalizableString Description = "A BiDiDriver constructed without a Transport uses a WebSocket connection, which accepts only an absolute URL with the ws or wss scheme. Any other connection string, such as an http URL or a pipe path, is rejected with an ArgumentException when StartAsync is called. Pass the WebSocket URL the browser reports, or construct the driver with a Transport over a connection that accepts the string.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi036");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        // A cheap filter before the bind: only a StartAsync call made on a receiver can start a driver.
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Name.Identifier.ValueText != "StartAsync")
        {
            return;
        }

        // StartAsync(string url, CancellationToken cancellationToken) has exactly one string parameter, so the only
        // constant string argument is the connection string; matching by shape also tolerates named arguments.
        ExpressionSyntax? connectionString = invocation.ArgumentList.Arguments
            .Select(argument => argument.Expression)
            .FirstOrDefault(expression => context.SemanticModel.GetConstantValue(expression) is { HasValue: true, Value: string });
        if (connectionString is null)
        {
            return;
        }

        string value = (string)context.SemanticModel.GetConstantValue(connectionString).Value!;
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && (uri.Scheme == "ws" || uri.Scheme == "wss"))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method
            || !AnalyzerSymbolHelpers.IsCommandExecutorType(method.ContainingType)
            || GetDriverCreation(context.SemanticModel, memberAccess.Expression) is not { } creation
            || !AnalyzerSymbolHelpers.IsDefaultTransportConstruction(context.SemanticModel, creation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, connectionString.GetLocation(), value));
    }

    /// <summary>
    /// Gets the construction of the driver a <c>StartAsync</c> call is made on: the receiver itself when it is a
    /// construction, or the initializer of a local that is never rebound.
    /// </summary>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="receiver">The receiver of the <c>StartAsync</c> call.</param>
    /// <returns>The construction, or <see langword="null"/> when the driver's construction cannot be seen here.</returns>
    private static BaseObjectCreationExpressionSyntax? GetDriverCreation(SemanticModel semanticModel, ExpressionSyntax receiver)
    {
        ExpressionSyntax current = receiver;
        while (current is ParenthesizedExpressionSyntax or PostfixUnaryExpressionSyntax)
        {
            current = current is ParenthesizedExpressionSyntax parenthesized
                ? parenthesized.Expression
                : ((PostfixUnaryExpressionSyntax)current).Operand;
        }

        if (current is BaseObjectCreationExpressionSyntax creation)
        {
            return creation;
        }

        if (current is not IdentifierNameSyntax identifier
            || semanticModel.GetSymbolInfo(identifier).Symbol is not ILocalSymbol local
            || local.DeclaringSyntaxReferences[0].GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: BaseObjectCreationExpressionSyntax initializer } declarator)
        {
            return null;
        }

        // A local that is assigned again, or passed by reference, may hold a driver built some other way by the time it
        // is started. Such a rebinding can only be written in the scope the local was declared in: the nearest enclosing
        // block, which also holds a local declared by a using or for statement, or the whole file for a top-level
        // program, whose statements have no enclosing block.
        string name = identifier.Identifier.ValueText;
        SyntaxNode scope = declarator.Ancestors().OfType<BlockSyntax>().FirstOrDefault() ?? declarator.SyntaxTree.GetRoot();
        bool isRebound = scope.DescendantNodes().Any(node => node switch
        {
            AssignmentExpressionSyntax assignment => assignment.Left is IdentifierNameSyntax target && target.Identifier.ValueText == name,
            ArgumentSyntax argument => !argument.RefKindKeyword.IsKind(SyntaxKind.None) && argument.Expression is IdentifierNameSyntax target && target.Identifier.ValueText == name,
            _ => false,
        });

        return isRebound ? null : initializer;
    }

}
