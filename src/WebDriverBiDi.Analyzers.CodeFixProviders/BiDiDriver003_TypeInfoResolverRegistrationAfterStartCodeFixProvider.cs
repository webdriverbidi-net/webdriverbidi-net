// <copyright file="BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Code fix provider for BIDI003 that moves RegisterTypeInfoResolver before StartAsync.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider))]
[Shared]
public class BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        InvocationExpressionSyntax? invocation = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        if (invocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Move RegisterTypeInfoResolver before StartAsync",
                createChangedDocument: c => MoveRegisterTypeInfoResolverBeforeStartAsync(
                    context.Document, invocation, c),
                equivalenceKey: "MoveRegisterTypeInfoResolverBeforeStartAsync"),
            diagnostic);
    }

    private static async Task<Document> MoveRegisterTypeInfoResolverBeforeStartAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Find the statement containing the RegisterTypeInfoResolver call
        StatementSyntax? registerStatement = invocation.FirstAncestorOrSelf<StatementSyntax>();
        if (registerStatement == null)
        {
            return document;
        }

        // Find the method containing this statement
        MethodDeclarationSyntax? method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body == null)
        {
            return document;
        }

        // Find the StartAsync call
        StatementSyntax? startAsyncStatement = FindStartAsyncStatement(method, invocation);
        if (startAsyncStatement == null)
        {
            return document;
        }

        // Track both statements through the transformation
        SyntaxNode trackedMethod = method.TrackNodes(registerStatement, startAsyncStatement);

        // Get the current tracked register statement and remove it
        StatementSyntax? trackedRegisterStatement = trackedMethod.GetCurrentNode(registerStatement);
        if (trackedRegisterStatement == null)
        {
            return document;
        }

        SyntaxNode? methodWithoutRegister = trackedMethod.RemoveNode(trackedRegisterStatement, SyntaxRemoveOptions.KeepNoTrivia);
        if (methodWithoutRegister == null)
        {
            return document;
        }

        // Get the current tracked StartAsync statement
        StatementSyntax? updatedStartAsyncStatement = methodWithoutRegister.GetCurrentNode(startAsyncStatement);
        if (updatedStartAsyncStatement == null)
        {
            return document;
        }

        // Insert the register statement before StartAsync
        StatementSyntax registerStatementCopy = trackedRegisterStatement.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        SyntaxNode newMethod = methodWithoutRegister.InsertNodesBefore(updatedStartAsyncStatement, new[] { registerStatementCopy });

        SyntaxNode newRoot = root.ReplaceNode(method, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }

    private static StatementSyntax? FindStartAsyncStatement(MethodDeclarationSyntax method, InvocationExpressionSyntax registerTypeInfoResolverCall)
    {
        if (method.Body == null)
        {
            return null;
        }

        // Get the driver variable name from the RegisterTypeInfoResolver call
        string? driverVariableName = GetDriverVariableName(registerTypeInfoResolverCall);
        if (driverVariableName == null)
        {
            return null;
        }

        // Find the first StartAsync call on the same driver variable
        foreach (StatementSyntax statement in method.Body.Statements)
        {
            InvocationExpressionSyntax? invocation = statement.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (invocation == null)
            {
                continue;
            }

            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            if (memberAccess.Name.Identifier.Text != "StartAsync")
            {
                continue;
            }

            string? invocationDriverName = GetDriverVariableName(invocation);
            if (invocationDriverName == driverVariableName)
            {
                return statement;
            }
        }

        return null;
    }

    private static string? GetDriverVariableName(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        ExpressionSyntax current = memberAccess.Expression;

        // Walk through the member access chain to find the base identifier
        while (current is MemberAccessExpressionSyntax nestedMemberAccess)
        {
            current = nestedMemberAccess.Expression;
        }

        if (current is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.Text;
        }

        return null;
    }
}
