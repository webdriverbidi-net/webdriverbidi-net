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
        BaseObjectCreationExpressionSyntax objectCreation = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<BaseObjectCreationExpressionSyntax>()
            .First();

        // Get the constructed type name, the reset property name, and the type that declares the
        // reset property (which may be a base class of the constructed type) from the diagnostic.
        string typeName = diagnostic.Properties["TypeName"]!;
        string resetPropertyName = diagnostic.Properties["ResetPropertyName"]!;
        string declaringTypeName = diagnostic.Properties["DeclaringTypeName"]!;
        string resetPropertyTypeName = diagnostic.Properties["ResetPropertyTypeName"]!;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use '{declaringTypeName}.{resetPropertyName}' instead",
                createChangedDocument: c => ReplaceWithResetPropertyAsync(
                    context.Document, objectCreation, typeName, declaringTypeName, resetPropertyName, resetPropertyTypeName, c),
                equivalenceKey: "UseResetProperty"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithResetPropertyAsync(
        Document document,
        BaseObjectCreationExpressionSyntax objectCreation,
        string typeName,
        string declaringTypeName,
        string resetPropertyName,
        string resetPropertyTypeName,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Create the replacement: DeclaringTypeName.ResetPropertyName. When the reset property is
        // declared on the constructed type itself, reuse that type's original syntax (for example a
        // fully-qualified or aliased name) as written at the construction site, so the replacement
        // resolves in the same way the `new` expression did. When the reset property is inherited from
        // a base type, fall back to the base type's simple name (which is in scope whenever the derived
        // type is, as they share a namespace). The target-typed form `new()` writes no type at all,
        // so there the analyzer-supplied simple name is the only name available.
        ExpressionSyntax resetPropertyReceiver = typeName == declaringTypeName
            ? SyntaxFactory.ParseExpression(objectCreation is ObjectCreationExpressionSyntax explicitCreation ? explicitCreation.Type.ToString() : typeName)
            : SyntaxFactory.IdentifierName(declaringTypeName);

        MemberAccessExpressionSyntax resetPropertyAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            resetPropertyReceiver,
            SyntaxFactory.IdentifierName(resetPropertyName));

        // Preserve the trivia from the original expression
        resetPropertyAccess = resetPropertyAccess
            .WithLeadingTrivia(objectCreation.GetLeadingTrivia())
            .WithTrailingTrivia(objectCreation.GetTrailingTrivia());

        // A local initialized from the reset property must be able to hold what the property returns.
        // The declaring type does not answer that: the library's one base-declared helper,
        // SetGeolocationOverrideCommandParameters.ResetGeolocationOverride, returns the *derived*
        // SetGeolocationOverrideCoordinatesCommandParameters, so a local declared with the derived
        // type still compiles and retyping it to the base would needlessly widen it. Retype only when
        // the property's own return type differs from the declared type, and retype to that return
        // type rather than to the declaring type. `var` locals, qualified type names, and inline
        // arguments need no change.
        if (resetPropertyTypeName != typeName
            && objectCreation.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } }
            && GetDeclaredTypeIdentifier(declaration.Type) is IdentifierNameSyntax declaredType
            && declaredType.Identifier.ValueText == typeName)
        {
            IdentifierNameSyntax newDeclaredType = SyntaxFactory.IdentifierName(resetPropertyTypeName).WithTriviaFrom(declaredType);
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
