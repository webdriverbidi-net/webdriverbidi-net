// <copyright file="BiDiDriver002_EventRegistrationAfterStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI002 that moves event registration before StartAsync.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver002_EventRegistrationAfterStartCodeFixProvider))]
[Shared]
public class BiDiDriver002_EventRegistrationAfterStartCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver002_EventRegistrationAfterStartAnalyzer.DiagnosticId);

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

        // The shared fix moves the RegisterEvent statement, and any local declarations it depends on, above the StartAsync statement on the same driver.
        CodeFixHelpers.RegisterMoveBeforeStartAsyncFix(context, diagnostic, invocation, "Move event registration before StartAsync", "MoveEventRegistrationBeforeStartAsync");
    }
}
