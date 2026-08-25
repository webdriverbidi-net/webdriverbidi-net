// <copyright file="BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI014 that replaces parameterless constructor with Reset property.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider))]
[Shared]
public class BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the object creation expression that triggered the diagnostic
        ObjectCreationExpressionSyntax objectCreation = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<ObjectCreationExpressionSyntax>()
            .First();

        // Get the constructed type name, the reset property name, and the type that declares the
        // reset property (which may be a base class of the constructed type) from the diagnostic.
        string typeName = diagnostic.Properties["TypeName"]!;
        string resetPropertyName = diagnostic.Properties["ResetPropertyName"]!;
        string declaringTypeName = diagnostic.Properties["DeclaringTypeName"]!;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use '{declaringTypeName}.{resetPropertyName}' instead",
                createChangedDocument: c => ReplaceWithResetPropertyAsync(
                    context.Document, objectCreation, typeName, declaringTypeName, resetPropertyName, c),
                equivalenceKey: "UseResetProperty"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithResetPropertyAsync(
        Document document,
        ObjectCreationExpressionSyntax objectCreation,
        string typeName,
        string declaringTypeName,
        string resetPropertyName,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Create the replacement: DeclaringTypeName.ResetPropertyName
        MemberAccessExpressionSyntax resetPropertyAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(declaringTypeName),
            SyntaxFactory.IdentifierName(resetPropertyName));

        // Preserve the trivia from the original expression
        resetPropertyAccess = resetPropertyAccess
            .WithLeadingTrivia(objectCreation.GetLeadingTrivia())
            .WithTrailingTrivia(objectCreation.GetTrailingTrivia());

        // When the reset property is declared on a base class, it returns that base type. A local
        // declared with the derived type (`Derived x = new Derived();`) would no longer compile, so
        // retype the declaration to the declaring type as well. `var` locals, qualified type names,
        // and inline arguments need no change.
        if (typeName != declaringTypeName
            && objectCreation.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } }
            && GetDeclaredTypeIdentifier(declaration.Type) is IdentifierNameSyntax declaredType
            && declaredType.Identifier.Text == typeName)
        {
            IdentifierNameSyntax newDeclaredType = SyntaxFactory.IdentifierName(declaringTypeName).WithTriviaFrom(declaredType);
            SyntaxNode retypedRoot = root.ReplaceNodes(
                new SyntaxNode[] { objectCreation, declaredType },
                (original, _) => original == objectCreation ? resetPropertyAccess : newDeclaredType);
            return document.WithSyntaxRoot(retypedRoot);
        }

        // Replace the object creation with the reset property access
        SyntaxNode newRoot = root.ReplaceNode(objectCreation, resetPropertyAccess);

        return document.WithSyntaxRoot(newRoot);
    }

    private static IdentifierNameSyntax? GetDeclaredTypeIdentifier(TypeSyntax declaredType)
    {
        // Locate the identifier that names the declared type so it can be swapped for the
        // declaring type: `Derived x`, `Ns.Derived x`, and `Derived? x` are all retyped;
        // `var`, predefined types such as `object`, and anything else are left alone
        // (`var` is an IdentifierNameSyntax whose text never equals the constructed type name).
        return declaredType switch
        {
            IdentifierNameSyntax identifier => identifier,
            QualifiedNameSyntax qualified => qualified.Right as IdentifierNameSyntax,
            NullableTypeSyntax nullable => GetDeclaredTypeIdentifier(nullable.ElementType),
            _ => null,
        };
    }
}
