// <copyright file="BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Code fix provider for BIDI001 that moves RegisterModule() calls before StartAsync().
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider))]
[Shared]
public class BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        InvocationExpressionSyntax invocation = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // The fix moves the registration before an existing StartAsync call in the same
        // method. The analyzer also fires in constructors and top-level programs, where the
        // rearrangement below has no method to operate on; no fix is possible there, so none
        // is offered.
        // Filter the StartAsync search to the same driver variable as the flagged RegisterModule;
        // otherwise the fix could move the registration before an unrelated receiver's StartAsync,
        // possibly ahead of the flagged driver's own declaration (CS0841).
        string? driverVariableName = GetRootIdentifierName(invocation.Expression);
        MethodDeclarationSyntax? method = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        bool startAsyncExists = method is not null && method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "StartAsync"
                && GetRootIdentifierName(memberAccess) == driverVariableName);
        if (!startAsyncExists)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Move RegisterModule() before StartAsync()",
                createChangedDocument: c => MoveRegisterModuleBeforeStartAsync(context.Document, invocation, c),
                equivalenceKey: "MoveRegisterModule"),
            diagnostic);
    }

    private static async Task<Document> MoveRegisterModuleBeforeStartAsync(
        Document document,
        InvocationExpressionSyntax registerModuleInvocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        MethodDeclarationSyntax method = registerModuleInvocation.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .First();

        // Find the statement containing RegisterModule
        StatementSyntax registerStatement = registerModuleInvocation.Ancestors()
            .OfType<StatementSyntax>()
            .First();

        // Find the StartAsync call on the same driver variable as the flagged RegisterModule.
        string driverVariableName = GetRootIdentifierName(registerModuleInvocation.Expression)!;
        InvocationExpressionSyntax startAsyncInvocation = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "StartAsync"
                && GetRootIdentifierName(memberAccess) == driverVariableName);

        StatementSyntax startAsyncStatement = startAsyncInvocation.Ancestors()
            .OfType<StatementSyntax>()
            .First();

        // Track nodes through transformations
        MethodDeclarationSyntax trackedMethod = method.TrackNodes(registerStatement, startAsyncStatement);

        // Remove RegisterModule from its current location
        StatementSyntax trackedRegisterStatement = trackedMethod.GetCurrentNode(registerStatement)!;
        MethodDeclarationSyntax methodWithoutRegister = trackedMethod.RemoveNode(trackedRegisterStatement, SyntaxRemoveOptions.KeepNoTrivia)!;

        // Find the tracked StartAsync statement in the updated tree
        StatementSyntax updatedStartAsyncStatement = methodWithoutRegister.GetCurrentNode(startAsyncStatement)!;

        // Create a copy of the register statement to insert
        StatementSyntax? registerStatementCopy = trackedRegisterStatement.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed);

        // Insert RegisterModule before StartAsync
        MethodDeclarationSyntax? newMethod = methodWithoutRegister.InsertNodesBefore(updatedStartAsyncStatement, new[] { registerStatementCopy });

        SyntaxNode newRoot = root.ReplaceNode(method, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }

    private static string? GetRootIdentifierName(ExpressionSyntax expression)
    {
        // expression is always a MemberAccessExpressionSyntax when called from this provider
        // (driver.RegisterModule / driver.StartAsync).
        ExpressionSyntax current = ((MemberAccessExpressionSyntax)expression).Expression;
        while (current is MemberAccessExpressionSyntax nestedAccess)
        {
            current = nestedAccess.Expression;
        }

        // The receiver chain may not end in a simple identifier; such receivers yield no name.
        return (current as IdentifierNameSyntax)?.Identifier.Text;
    }
}
