// <copyright file="CodeFixHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Generic;
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
/// Shared helpers for code fix providers.
/// </summary>
internal static class CodeFixHelpers
{
    private const string OptionsTypeName = "ObservableEventHandlerOptions";
    private const string RunHandlerAsynchronouslyName = "RunHandlerAsynchronously";

    /// <summary>
    /// Registers the appropriate fix for a diagnostic reported inside the lambda handler of an
    /// <c>AddObserver</c> invocation.
    /// </summary>
    /// <param name="context">The code fix context.</param>
    /// <param name="diagnostic">The diagnostic being fixed.</param>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="invocation">The AddObserver invocation enclosing the diagnostic.</param>
    /// <remarks>
    /// <c>RunHandlerAsynchronously</c> only moves the handler's returned <c>Task</c> off the
    /// dispatching thread; it does not offload the code that runs before the handler returns.
    /// The fix therefore depends on the handler:
    /// <list type="bullet">
    /// <item><description>A handler bound to the <c>Action&lt;T&gt;</c> overload or an
    /// <c>async</c> lambda only needs the option added.</description></item>
    /// <item><description>A non-<c>async</c> <c>Task</c>-returning lambda is converted to an
    /// <c>async</c> lambda that first awaits <c>Task.Yield()</c>, so everything after it runs on
    /// the thread pool, and the option is added if it is missing.</description></item>
    /// </list>
    /// Diagnostics reported inside a method passed as a method group are not enclosed by the
    /// <c>AddObserver</c> invocation, so callers never reach this method for them and no fix is
    /// offered: the method declaration itself would have to change.
    /// </remarks>
    internal static void RegisterAddObserverHandlerFix(
        CodeFixContext context,
        Diagnostic diagnostic,
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation)
    {
        AnonymousFunctionExpressionSyntax lambda = (AnonymousFunctionExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;
        IMethodSymbol addObserverMethod = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol!;
        bool boundToAction = addObserverMethod.Parameters[0].Type.Name == "Action";
        bool convertToAsync = !boundToAction && !lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);
        bool optionPresent = invocation.ArgumentList.Arguments.Any(argument => argument.Expression.ToString().Contains(RunHandlerAsynchronouslyName));
        string title = convertToAsync
            ? (optionPresent ? "Make handler async" : "Make handler async and add RunHandlerAsynchronously option")
            : "Add RunHandlerAsynchronously option";

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                createChangedDocument: cancellationToken => ApplyHandlerFixAsync(context.Document, invocation, convertToAsync, cancellationToken),
                equivalenceKey: title),
            diagnostic);
    }

    private static async Task<Document> ApplyHandlerFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        bool convertToAsync,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        SemanticModel semanticModel = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

        ArgumentSyntax optionsArgument = SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(OptionsTypeName),
                SyntaxFactory.IdentifierName(RunHandlerAsynchronouslyName)));

        ArgumentSyntax? existingOptionsArgument = invocation.ArgumentList.Arguments.FirstOrDefault(argument =>
        {
            ITypeSymbol? argumentType = semanticModel.GetTypeInfo(argument.Expression, cancellationToken).Type;
            return argumentType?.Name == OptionsTypeName;
        });

        ArgumentListSyntax newArgumentList = existingOptionsArgument != null
            ? invocation.ArgumentList.WithArguments(invocation.ArgumentList.Arguments.Replace(existingOptionsArgument, optionsArgument.WithTriviaFrom(existingOptionsArgument)))
            : invocation.ArgumentList.AddArguments(optionsArgument);

        if (convertToAsync)
        {
            ArgumentSyntax handlerArgument = newArgumentList.Arguments[0];
            AnonymousFunctionExpressionSyntax lambda = (AnonymousFunctionExpressionSyntax)handlerArgument.Expression;
            newArgumentList = newArgumentList.ReplaceNode(lambda, ConvertToAsyncLambda(lambda));
        }

        InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(newArgumentList);
        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }

    private static AnonymousFunctionExpressionSyntax ConvertToAsyncLambda(AnonymousFunctionExpressionSyntax lambda)
    {
        // Every compilable file has at least one line break (the usings, if nothing else); reuse
        // the file's own line ending so the fix never mixes styles.
        SyntaxTrivia endOfLine = lambda.SyntaxTree.GetRoot().DescendantTrivia().First(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
        SyntaxTriviaList lambdaLeadingTrivia = lambda.GetLeadingTrivia();
        SyntaxToken asyncKeyword = SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            .WithLeadingTrivia(lambdaLeadingTrivia)
            .WithTrailingTrivia(SyntaxFactory.Space);

        if (lambda.ExpressionBody is ExpressionSyntax expressionBody)
        {
            // 'args => expr' becomes a block whose braces sit at the indentation of the line the
            // lambda starts on, with the statements one level deeper.
            string braceIndentation = GetLineIndentation(lambda);
            string statementIndentation = braceIndentation + "    ";
            BlockSyntax expressionBlock = CreateBlock(
                [CreateYieldStatement(), CreateAwaitStatement(expressionBody)],
                braceIndentation,
                statementIndentation,
                endOfLine);

            LambdaExpressionSyntax expressionLambda = (LambdaExpressionSyntax)lambda;
            return expressionLambda
                .WithLeadingTrivia()
                .WithAsyncKeyword(asyncKeyword)
                .WithArrowToken(expressionLambda.ArrowToken.WithTrailingTrivia(endOfLine))
                .WithBody(expressionBlock.WithLeadingTrivia(SyntaxFactory.Whitespace(braceIndentation)));
        }

        BlockSyntax originalBlock = lambda.Block!;
        SyntaxList<StatementSyntax> originalStatements = originalBlock.Statements;
        string indentation = GetIndentation(originalStatements[0]);
        List<StatementSyntax> statements =
        [
            CreateYieldStatement().WithLeadingTrivia(SyntaxFactory.Whitespace(indentation)).WithTrailingTrivia(endOfLine),
        ];

        ReturnStatementRewriter rewriter = new(endOfLine);
        for (int index = 0; index < originalStatements.Count; index++)
        {
            StatementSyntax statement = originalStatements[index];
            if (index == originalStatements.Count - 1 && statement is ReturnStatementSyntax finalReturn)
            {
                // A trailing 'return Task.CompletedTask;' is simply dropped; a trailing
                // 'return <task>;' becomes 'await <task>;'.
                if (!IsCompletedTask(finalReturn.Expression!))
                {
                    statements.Add(CreateAwaitStatement(finalReturn.Expression!).WithTriviaFrom(finalReturn));
                }

                continue;
            }

            statements.Add((StatementSyntax)rewriter.Visit(statement)!);
        }

        return lambda
            .WithLeadingTrivia()
            .WithAsyncKeyword(asyncKeyword)
            .WithBody(originalBlock.WithStatements(SyntaxFactory.List(statements)));
    }

    private static BlockSyntax CreateBlock(
        IEnumerable<StatementSyntax> statements,
        string braceIndentation,
        string statementIndentation,
        SyntaxTrivia endOfLine)
    {
        return SyntaxFactory.Block(
            SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(endOfLine),
            SyntaxFactory.List(statements.Select(statement =>
                statement.WithLeadingTrivia(SyntaxFactory.Whitespace(statementIndentation)).WithTrailingTrivia(endOfLine))),
            SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(SyntaxFactory.Whitespace(braceIndentation)));
    }

    private static string GetLineIndentation(SyntaxNode node)
    {
        string lineText = node.SyntaxTree.GetText().Lines.GetLineFromPosition(node.SpanStart).ToString();
        return lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);
    }

    private static string GetIndentation(SyntaxNode node)
    {
        // The whitespace immediately preceding the node on its line; empty when the node has none.
        return node.GetLeadingTrivia().LastOrDefault(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)).ToString();
    }

    private static bool IsCompletedTask(ExpressionSyntax expression)
    {
        return expression.ToString().EndsWith("Task.CompletedTask", System.StringComparison.Ordinal);
    }

    private static ExpressionStatementSyntax CreateYieldStatement()
    {
        // Fully qualified so the fix compiles even without 'using System.Threading.Tasks;'; the
        // simplifier annotation lets the host reduce it to 'Task.Yield()' when the using exists.
        ExpressionSyntax taskType = SyntaxFactory.ParseExpression("System.Threading.Tasks.Task").WithAdditionalAnnotations(Simplifier.Annotation);
        return CreateAwaitStatement(
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, taskType, SyntaxFactory.IdentifierName("Yield"))));
    }

    private static ExpressionStatementSyntax CreateAwaitStatement(ExpressionSyntax expression)
    {
        return SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.Token(SyntaxKind.AwaitKeyword).WithTrailingTrivia(SyntaxFactory.Space),
                expression.WithoutTrivia()));
    }

    /// <summary>
    /// Rewrites the return statements of a Task-returning lambda body for use in an async lambda:
    /// 'return Task.CompletedTask;' becomes 'return;' and 'return <task>;' becomes
    /// '{ await <task>; return; }'. Nested lambdas and local functions are left untouched.
    /// </summary>
    private sealed class ReturnStatementRewriter(SyntaxTrivia endOfLine) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            return node is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax ? node : base.Visit(node);
        }

        public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
        {
            if (IsCompletedTask(node.Expression!))
            {
                return SyntaxFactory.ReturnStatement().WithTriviaFrom(node);
            }

            string indentation = GetIndentation(node);
            return CreateBlock(
                [CreateAwaitStatement(node.Expression!), SyntaxFactory.ReturnStatement()],
                indentation,
                indentation + "    ",
                endOfLine)
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia());
        }
    }
}
