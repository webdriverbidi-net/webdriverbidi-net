// <copyright file="BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI001 that moves RegisterModule() calls before StartAsync().
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider))]
[Shared]
public class BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    // Every diagnostic in a member moves its call to the same place, so the batch fixer would keep
    // one edit and drop the rest; the fixes are applied one after another instead.
    public sealed override FixAllProvider GetFixAllProvider() => SequentialDocumentFixAllProvider.Create(this);

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();
        InvocationExpressionSyntax invocation = root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // The shared fix moves the RegisterModule statement, and any local declarations it depends on, above the StartAsync statement on the same driver.
        CodeFixHelpers.RegisterMoveBeforeStartAsyncFix(context, diagnostic, invocation, "Move RegisterModule() before StartAsync()", "MoveRegisterModule");
    }
}
