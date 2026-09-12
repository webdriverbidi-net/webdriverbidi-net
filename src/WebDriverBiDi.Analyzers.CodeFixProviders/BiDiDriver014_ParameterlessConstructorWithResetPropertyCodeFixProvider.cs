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
using Microsoft.CodeAnalysis.Simplification;

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
        string declaringTypeFullName = diagnostic.Properties[BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DeclaringTypeFullNamePropertyName]!;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use '{declaringTypeName}.{resetPropertyName}' instead",
                createChangedDocument: c => ReplaceWithResetPropertyAsync(
                    context.Document, objectCreation, typeName, declaringTypeName, declaringTypeFullName, resetPropertyName, resetPropertyTypeName, c),
                equivalenceKey: "UseResetProperty"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithResetPropertyAsync(
        Document document,
        BaseObjectCreationExpressionSyntax objectCreation,
        string typeName,
        string declaringTypeName,
        string declaringTypeFullName,
        string resetPropertyName,
        string resetPropertyTypeName,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Create the replacement: DeclaringTypeName.ResetPropertyName. An explicit `new T()` whose
        // type also declares the reset property already names something that binds here, so reuse that
        // syntax as written (a qualified or aliased name stays as it was). Otherwise the receiver has
        // to be produced rather than copied -- a target-typed `new()` writes no type at all, and an
        // inherited reset property is declared on a base type this file may never name -- so it is
        // written fully qualified and annotated for the simplifier, which shortens it to whatever
        // binds at this position. A bare simple name would not compile where the namespace is not
        // imported.
        ExpressionSyntax resetPropertyReceiver = typeName == declaringTypeName && objectCreation is ObjectCreationExpressionSyntax explicitCreation
            ? SyntaxFactory.ParseExpression(explicitCreation.Type.ToString())
            : SyntaxFactory.ParseExpression(declaringTypeFullName).WithAdditionalAnnotations(Simplifier.Annotation);

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
