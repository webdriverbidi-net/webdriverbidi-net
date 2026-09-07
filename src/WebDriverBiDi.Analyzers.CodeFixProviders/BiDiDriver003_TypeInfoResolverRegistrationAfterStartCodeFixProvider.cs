// <copyright file="BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Code fix provider for BIDI003 that moves RegisterTypeInfoResolverAsync before StartAsync.
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
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();
        InvocationExpressionSyntax invocation = root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // The shared fix moves the RegisterTypeInfoResolverAsync statement, and any local declarations it depends on, above the StartAsync statement on the same driver.
        CodeFixHelpers.RegisterMoveBeforeStartAsyncFix(context, diagnostic, invocation, "Move RegisterTypeInfoResolverAsync before StartAsync", "MoveRegisterTypeInfoResolverBeforeStartAsync");
    }
}
