// <copyright file="BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI010 that awaits a fire-and-forget module command.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider))]
[Shared]
public class BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();
        InvocationExpressionSyntax invocation = root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent!
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // Only offer the fix where `await` is legal. Adding it inside a synchronous method would
        // replace one compile error's worth of trouble with another (CS4033), so a caller that has to
        // become async first is left to make that decision.
        if (!CodeFixHelpers.IsInAsyncContext(invocation))
        {
            return;
        }

        // A call made through `?.` is reported at the inner invocation, which starts at the member
        // binding's dot. Awaiting that node alone would splice the `await` into the middle of the
        // conditional access, so climb out of every conditional access the call is the right-hand
        // side of: the whole access is the expression to await.
        ExpressionSyntax target = invocation;
        while (target.Parent is ConditionalAccessExpressionSyntax conditionalAccess && conditionalAccess.WhenNotNull == target)
        {
            target = conditionalAccess;
        }

        // A conditional access over a value-type awaitable yields that type's nullable form, which only
        // `default` stands in for; the reference-typed awaitables take Task.CompletedTask. The analyzer
        // reports an invocation only once it has bound the called method, so the symbol is present and
        // is a method: the null-forgiving operator and the cast express an invariant this library owns.
        SemanticModel semanticModel = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;
        IMethodSymbol method = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol!;
        bool isValueTypeAwaitable = method.ReturnType.IsValueType;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Await the command",
                createChangedDocument: c => AddAwaitAsync(context.Document, root, target, isValueTypeAwaitable, c),
                equivalenceKey: "AwaitModuleCommand"),
            diagnostic);
    }

    private static Task<Document> AddAwaitAsync(
        Document document,
        SyntaxNode root,
        ExpressionSyntax target,
        bool isValueTypeAwaitable,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        ExpressionSyntax operand = target.WithoutLeadingTrivia();
        if (target is ConditionalAccessExpressionSyntax)
        {
            // `driver?.CommandAsync()` is null when the receiver is, and awaiting null throws, so the
            // access is coalesced to an already-completed awaitable. That keeps what `?.` asked for --
            // do nothing when the receiver is null -- instead of introducing a NullReferenceException
            // the discarded call never had. `Task.CompletedTask` is fully qualified so the fix compiles
            // without a using, and is reduced again where one exists. The coalesced type of a
            // `Task<T>` is `Task`, which is all the rewritten statement needs: BIDI010 fires only where
            // the result was discarded.
            ExpressionSyntax completedAwaitable = isValueTypeAwaitable
                ? SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)
                : SyntaxFactory.ParseExpression("System.Threading.Tasks.Task.CompletedTask").WithAdditionalAnnotations(Simplifier.Annotation);

            SyntaxToken coalesceOperator = SyntaxFactory.Token(
                SyntaxFactory.TriviaList(SyntaxFactory.Space),
                SyntaxKind.QuestionQuestionToken,
                SyntaxFactory.TriviaList(SyntaxFactory.Space));

            // `await` binds tighter than `??`, so the coalesce has to be parenthesized.
            operand = SyntaxFactory.ParenthesizedExpression(
                SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, operand, coalesceOperator, completedAwaitable));
        }

        // The diagnostic fires only where the call stands alone as an expression statement, so the
        // awaited node carries the statement's leading trivia; move it to the await so the indentation
        // and any preceding comment stay put.
        AwaitExpressionSyntax awaitExpression = SyntaxFactory.AwaitExpression(operand)
            .WithLeadingTrivia(target.GetLeadingTrivia());

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(target, awaitExpression)));
    }
}
