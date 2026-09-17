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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Shared helpers for code fix providers.
/// </summary>
internal static class CodeFixHelpers
{
    private const string OptionsTypeName = "ObservableEventHandlerOptions";
    private const string RunHandlerAsynchronouslyName = "RunHandlerAsynchronously";

    // The diagnostic property BIDI007 and BIDI023 set for an operation that runs before an async handler's
    // first await (AnalyzerSymbolHelpers.RunsBeforeFirstAwaitPropertyName, which is internal to the analyzer
    // assembly).
    private const string RunsBeforeFirstAwaitPropertyName = "RunsBeforeFirstAwait";

    /// <summary>
    /// Gets the C# language version the document is compiled with.
    /// </summary>
    /// <param name="document">The document a fix is being offered for.</param>
    /// <returns>The effective language version of the document's project.</returns>
    /// <remarks>
    /// A fix must not emit syntax the project cannot compile. The library supports consumers on
    /// <c>netstandard2.0</c> and <c>net472</c>, whose default is C# 7.3, and on <c>net6.0</c> and
    /// <c>net7.0</c>, whose defaults are C# 10 and C# 11; a fix written only for the newest syntax
    /// hands all of them code that does not build. The version read here is already the effective
    /// one, because parse options resolve <c>latest</c>, <c>default</c> and <c>preview</c> to a
    /// concrete version when they are constructed. A C# fix is only ever offered for a C# document,
    /// whose project always carries <see cref="CSharpParseOptions"/>, so the cast cannot fail.
    /// </remarks>
    internal static LanguageVersion GetLanguageVersion(Document document)
    {
        return ((CSharpParseOptions)document.Project.ParseOptions!).LanguageVersion;
    }

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
    /// <item><description>A handler bound to the <c>Action&lt;T&gt;</c> overload only needs the option
    /// added.</description></item>
    /// <item><description>An <c>async</c> lambda whose reported operation runs after its first
    /// <c>await</c> only needs the option added. One whose reported operation runs before it has
    /// <c>await Task.Yield()</c> inserted as its first statement, so that the operation runs on the thread
    /// pool, and the option is added if it is missing.</description></item>
    /// <item><description>A non-<c>async</c> <c>Task</c>-returning lambda is converted to an
    /// <c>async</c> lambda that first awaits <c>Task.Yield()</c>, so everything after it runs on
    /// the thread pool, and the option is added if it is missing.</description></item>
    /// </list>
    /// No fix is offered for a handler passed as a method group, because the method declaration
    /// itself would have to change. A method group declared in the same file is excluded already,
    /// because the diagnostic sits inside the method body and no <c>AddObserver</c> invocation
    /// encloses it; one declared in another file is not, because the analyzer reports such a
    /// diagnostic at the <c>AddObserver</c> argument so that it appears where the handler was
    /// registered. Both are rejected here by testing the argument.
    /// </remarks>
    internal static void RegisterAddObserverHandlerFix(
        CodeFixContext context,
        Diagnostic diagnostic,
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation)
    {
        if (invocation.ArgumentList.Arguments[0].Expression is not AnonymousFunctionExpressionSyntax lambda)
        {
            return;
        }

        IMethodSymbol addObserverMethod = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol!;
        bool boundToAction = addObserverMethod.Parameters[0].Type.Name == "Action";
        bool convertToAsync = !boundToAction && !lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

        // An async lambda runs on the dispatching thread until its first await, so an operation the analyzer
        // marked as running before that await is moved off it only by awaiting first.
        bool awaitYieldFirst = !boundToAction && !convertToAsync && diagnostic.Properties.ContainsKey(RunsBeforeFirstAwaitPropertyName);
        bool optionPresent = invocation.ArgumentList.Arguments.Any(argument =>
        {
            // Resolve the option semantically rather than by source text, which fails when the option
            // is passed through a variable. RunHandlerAsynchronously has the underlying value 1; a
            // non-constant argument cannot be resolved, so treat it as present.
            if (semanticModel.GetTypeInfo(argument.Expression, context.CancellationToken).Type?.Name != OptionsTypeName)
            {
                return false;
            }

            Optional<object?> constantValue = semanticModel.GetConstantValue(argument.Expression, context.CancellationToken);
            return !constantValue.HasValue || constantValue.Value is int and 1;
        });
        string title = convertToAsync
            ? (optionPresent ? "Make handler async" : "Make handler async and add RunHandlerAsynchronously option")
            : awaitYieldFirst
                ? (optionPresent ? "Await Task.Yield() first" : "Await Task.Yield() first and add RunHandlerAsynchronously option")
                : "Add RunHandlerAsynchronously option";

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                createChangedDocument: cancellationToken => ApplyHandlerFixAsync(context.Document, invocation, convertToAsync, awaitYieldFirst, cancellationToken),
                equivalenceKey: title),
            diagnostic);
    }

    private static async Task<Document> ApplyHandlerFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        bool convertToAsync,
        bool awaitYieldFirst,
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

        if (convertToAsync || awaitYieldFirst)
        {
            // Every compilable file has at least one line break (the usings, if nothing else); reuse
            // the file's own line ending so the fix never mixes styles. It is taken from the document
            // root rather than from the rewritten argument list, which is a detached tree that holds
            // no line break at all when the handler is a single-line expression-bodied lambda.
            SyntaxTrivia endOfLine = root.DescendantTrivia().First(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            // The indentation of the line the lambda starts on is likewise read from the original,
            // attached lambda: in the detached tree the first line has none.
            string lambdaLineIndentation = GetLineIndentation(invocation.ArgumentList.Arguments[0].Expression);
            ArgumentSyntax handlerArgument = newArgumentList.Arguments[0];
            AnonymousFunctionExpressionSyntax lambda = (AnonymousFunctionExpressionSyntax)handlerArgument.Expression;
            AnonymousFunctionExpressionSyntax rewrittenLambda = convertToAsync
                ? ConvertToAsyncLambda(lambda, endOfLine, lambdaLineIndentation)
                : InsertYieldIntoAsyncLambda(lambda, endOfLine, lambdaLineIndentation);
            newArgumentList = newArgumentList.ReplaceNode(lambda, rewrittenLambda);
        }

        InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(newArgumentList);
        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }

    private static AnonymousFunctionExpressionSyntax ConvertToAsyncLambda(AnonymousFunctionExpressionSyntax lambda, SyntaxTrivia endOfLine, string lambdaLineIndentation)
    {
        SyntaxTriviaList lambdaLeadingTrivia = lambda.GetLeadingTrivia();
        SyntaxToken asyncKeyword = SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            .WithLeadingTrivia(lambdaLeadingTrivia)
            .WithTrailingTrivia(SyntaxFactory.Space);

        if (lambda.ExpressionBody is ExpressionSyntax expressionBody)
        {
            // 'args => expr' becomes a block whose braces sit at the indentation of the line the
            // lambda starts on, with the statements one level deeper.
            string braceIndentation = lambdaLineIndentation;
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

    private static AnonymousFunctionExpressionSyntax InsertYieldIntoAsyncLambda(AnonymousFunctionExpressionSyntax lambda, SyntaxTrivia endOfLine, string lambdaLineIndentation)
    {
        if (lambda.ExpressionBody is ExpressionSyntax expressionBody)
        {
            // 'async args => expr' becomes a block laid out as ConvertToAsyncLambda lays one out, which awaits
            // Task.Yield() and then evaluates the expression as a statement. The lambda is already async, so the
            // expression is kept as written rather than awaited again.
            BlockSyntax expressionBlock = CreateBlock(
                [CreateYieldStatement(), SyntaxFactory.ExpressionStatement(expressionBody.WithoutTrivia())],
                lambdaLineIndentation,
                lambdaLineIndentation + "    ",
                endOfLine);

            LambdaExpressionSyntax expressionLambda = (LambdaExpressionSyntax)lambda;
            return expressionLambda
                .WithArrowToken(expressionLambda.ArrowToken.WithTrailingTrivia(endOfLine))
                .WithBody(expressionBlock.WithLeadingTrivia(SyntaxFactory.Whitespace(lambdaLineIndentation)));
        }

        BlockSyntax block = lambda.Block!;
        StatementSyntax yieldStatement = CreateYieldStatement()
            .WithLeadingTrivia(SyntaxFactory.Whitespace(GetIndentation(block.Statements[0])))
            .WithTrailingTrivia(endOfLine);
        return lambda.WithBody(block.WithStatements(block.Statements.Insert(0, yieldStatement)));
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

    /// <summary>
    /// Gets the indentation of a node: the whitespace immediately preceding it on its line.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The indentation, or an empty string when the node has none.</returns>
    internal static string GetIndentation(SyntaxNode node)
    {
        // The whitespace immediately preceding the node on its line; empty when the node has none.
        return node.GetLeadingTrivia().LastOrDefault(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)).ToString();
    }

    /// <summary>
    /// Gets the trailing trivia for a statement being moved: its own trailing trivia, such as a comment on the
    /// same line, with the line break that ended it at its old position replaced by an elastic one.
    /// </summary>
    /// <param name="statement">The statement being moved.</param>
    /// <returns>The trailing trivia for the moved copy.</returns>
    internal static IEnumerable<SyntaxTrivia> GetTrailingTriviaForMove(StatementSyntax statement)
    {
        return statement.GetTrailingTrivia().Where(trivia => !trivia.IsKind(SyntaxKind.EndOfLineTrivia)).Append(SyntaxFactory.ElasticLineFeed);
    }

    /// <summary>
    /// Determines whether <c>await</c> is legal at a node: the nearest enclosing function is
    /// <c>async</c>, or the node is in a top-level statement (for which the compiler generates an
    /// asynchronous entry point).
    /// </summary>
    /// <param name="node">The node at which an <c>await</c> would be inserted.</param>
    /// <returns><see langword="true"/> if <c>await</c> is legal there; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A fix that inserts an <c>await</c> into a synchronous member would replace the reported
    /// problem with a compile error (CS4033), so callers offer no fix instead and leave the caller to
    /// decide whether the member should become <c>async</c>.
    /// </remarks>
    internal static bool IsInAsyncContext(SyntaxNode node)
    {
        // Every member kind that can carry executable code is a stopping point, not only the ones that
        // can be async. A constructor, an accessor, an operator or a finalizer cannot be made async, so
        // stopping only at methods and lambdas would walk straight past them to some outer node and
        // answer for that instead. BIDI010 is reported per operation and BIDI012 registers constructor
        // declarations, so both do reach these members.
        SyntaxNode? enclosingMember = node.Ancestors().FirstOrDefault(ancestor =>
            ancestor is AnonymousFunctionExpressionSyntax
                or MethodDeclarationSyntax
                or LocalFunctionStatementSyntax
                or ConstructorDeclarationSyntax
                or DestructorDeclarationSyntax
                or OperatorDeclarationSyntax
                or ConversionOperatorDeclarationSyntax
                or AccessorDeclarationSyntax
                or PropertyDeclarationSyntax
                or IndexerDeclarationSyntax
                or GlobalStatementSyntax);

        return enclosingMember switch
        {
            AnonymousFunctionExpressionSyntax anonymousFunction => anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword),
            MethodDeclarationSyntax method => method.Modifiers.Any(SyntaxKind.AsyncKeyword),
            LocalFunctionStatementSyntax localFunction => localFunction.Modifiers.Any(SyntaxKind.AsyncKeyword),

            // A top-level statement, whose generated entry point is asynchronous.
            GlobalStatementSyntax => true,

            // A member that cannot be async: an expression-bodied property or indexer, an accessor, a
            // constructor, a finalizer, or an operator. Inserting await here would replace the reported
            // problem with CS4033, so no fix is offered.
            _ => false,
        };
    }

    /// <summary>
    /// Produces a root in which <paramref name="statementToInsert"/> runs immediately before
    /// <paramref name="targetStatement"/>.
    /// </summary>
    /// <param name="root">The syntax root being rewritten.</param>
    /// <param name="targetStatement">The statement the new statement must precede.</param>
    /// <param name="statementToInsert">The statement to insert.</param>
    /// <returns>The rewritten root.</returns>
    /// <remarks>
    /// A statement is only insertable before when it belongs to a statement list, which is to say a
    /// block or a switch section. The embedded statement of an <c>if</c>, an <c>else</c> or a loop
    /// (<c>if (x) await driver.DisposeAsync();</c>) belongs to no list, and asking the list editor to
    /// insert before it throws. Such a statement is replaced by a block holding both statements, which
    /// keeps the inserted one inside the branch it belongs to rather than hoisting it out.
    /// </remarks>
    internal static SyntaxNode InsertStatementBefore(SyntaxNode root, StatementSyntax targetStatement, StatementSyntax statementToInsert)
    {
        if (targetStatement.Parent is BlockSyntax or SwitchSectionSyntax)
        {
            return root.InsertNodesBefore(targetStatement, new[] { statementToInsert });
        }

        return root.ReplaceNode(
            targetStatement,
            SyntaxFactory.Block(statementToInsert.WithoutLeadingTrivia(), targetStatement.WithoutLeadingTrivia())
                .WithAdditionalAnnotations(Formatter.Annotation));
    }

    /// <summary>
    /// Gets the identifier at the root of a member access chain: <c>driver</c> for
    /// <c>driver.Session.StartAsync</c>, matching the receiver walk the lifecycle analyzers use.
    /// </summary>
    /// <param name="expression">A member access chain, or the receiver of one.</param>
    /// <returns>The root identifier's name, or <see langword="null"/> when the chain does not root in a simple identifier (a field reached through <c>this</c>, or a call).</returns>
    /// <remarks>
    /// As in the analyzers' own receiver walk (which is internal to the analyzer assembly), the chain is read through
    /// the wrappers a receiver may carry, so <c>driver!.StartAsync</c>, <c>(driver).StartAsync</c>,
    /// <c>((IBiDiDriverLifecycleManager)driver).StartAsync</c> and <c>driver?.StartAsync</c> all root in
    /// <c>driver</c>. A member binding takes its receiver from the nearest enclosing conditional access whose
    /// non-null branch contains it.
    /// </remarks>
    internal static string? GetRootIdentifierName(ExpressionSyntax expression)
    {
        return GetRootIdentifierName(expression, out _);
    }

    /// <summary>
    /// Gets the identifier at the root of a member access chain, and how many member accesses separate it
    /// from the end of the chain.
    /// </summary>
    /// <param name="expression">A member access chain, or the receiver of one.</param>
    /// <param name="memberDepth">
    /// When this method returns, the number of member accesses between the root identifier and the end of the
    /// chain: 1 for <c>driver.StartAsync</c>, 2 for <c>driver.Session.StatusAsync</c>.
    /// </param>
    /// <returns>The root identifier's name, or <see langword="null"/> when the chain does not root in a simple identifier.</returns>
    /// <remarks>
    /// The depth mirrors the one the analyzers' own receiver walk reports, so that a fix can apply the same
    /// "on the driver itself" test the analyzer applied.
    /// </remarks>
    internal static string? GetRootIdentifierName(ExpressionSyntax expression, out int memberDepth)
    {
        memberDepth = 0;
        ExpressionSyntax current = expression;
        while (true)
        {
            switch (current)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    memberDepth++;
                    current = memberAccess.Expression;
                    break;
                case MemberBindingExpressionSyntax memberBinding:
                    memberDepth++;
                    current = memberBinding.Ancestors()
                        .OfType<ConditionalAccessExpressionSyntax>()
                        .First(conditionalAccess => conditionalAccess.WhenNotNull.Span.Contains(memberBinding.Span))
                        .Expression;
                    break;
                case ParenthesizedExpressionSyntax parenthesized:
                    current = parenthesized.Expression;
                    break;
                case CastExpressionSyntax cast:
                    current = cast.Expression;
                    break;
                case PostfixUnaryExpressionSyntax postfix:
                    current = postfix.Operand;
                    break;
                default:
                    return (current as IdentifierNameSyntax)?.Identifier.ValueText;
            }
        }
    }

    /// <summary>
    /// Determines whether an invocation calls <c>StartAsync</c> on the named driver, through whatever wrappers its
    /// receiver carries: <c>driver.StartAsync(url)</c>, <c>driver!.StartAsync(url)</c> or <c>driver?.StartAsync(url)</c>.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="driverVariableName">The name of the driver variable.</param>
    /// <returns><see langword="true"/> if the invocation starts the named driver; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The invoked name is the last token of the invocation's expression, for a member access and a member binding
    /// alike. The call must be on the driver itself, not on something reached through it: the lifecycle analyzers
    /// treat only a direct call as starting the driver, because a module can expose a <c>StartAsync</c> of its own
    /// (<c>driver.Tracing.StartAsync()</c>) that is a different lifecycle. A fix that matched the chain's root
    /// alone would take such a call for the start and move code to the wrong statement.
    /// </remarks>
    internal static bool IsStartAsyncOn(InvocationExpressionSyntax invocation, string? driverVariableName)
    {
        return invocation.Expression.GetLastToken().ValueText == "StartAsync"
            && GetRootIdentifierName(invocation.Expression, out int memberDepth) == driverVariableName
            && memberDepth == 1;
    }

    /// <summary>
    /// Registers the fix shared by BIDI001, BIDI002 and BIDI003: move the statement holding a
    /// registration call above the statement that starts the same driver.
    /// </summary>
    /// <param name="context">The code fix context.</param>
    /// <param name="diagnostic">The diagnostic being fixed.</param>
    /// <param name="registrationInvocation">The flagged registration call.</param>
    /// <param name="title">The title of the code action.</param>
    /// <param name="equivalenceKey">The equivalence key of the code action.</param>
    /// <remarks>
    /// <para>
    /// The fix rearranges the top-level statements of a block-bodied method. The analyzers also fire in
    /// constructors and top-level programs, where that shape is absent; no fix is offered there.
    /// </para>
    /// <para>
    /// The registration may use locals declared after the start — <c>CustomModule module = new(driver);</c>
    /// between <c>StartAsync</c> and <c>RegisterModule(module)</c> is the documented idiom — so every
    /// local declaration it depends on, transitively, moves with it; moving the registration alone
    /// would put the use before the declaration (CS0841). A dependency that awaits something cannot
    /// be moved before the start (it may need the started driver), and a registration that is the
    /// embedded statement of an if or a loop cannot be hoisted without making it unconditional, so
    /// no fix is offered in those cases.
    /// </para>
    /// </remarks>
    internal static void RegisterMoveBeforeStartAsyncFix(
        CodeFixContext context,
        Diagnostic diagnostic,
        InvocationExpressionSyntax registrationInvocation,
        string title,
        string equivalenceKey)
    {
        MethodDeclarationSyntax? method = registrationInvocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body is null)
        {
            return;
        }

        // The analyzers report only calls whose receiver roots in a tracked local, so the flagged
        // call always yields a name; the StartAsync search is filtered to the same driver, otherwise
        // the fix could move the registration before an unrelated receiver's StartAsync — possibly
        // ahead of the flagged driver's own declaration.
        // The analyzer walks the body's top-level statements and reported a StartAsync on this
        // driver before the registration, so one of those statements holds it.
        string driverVariableName = GetRootIdentifierName(registrationInvocation.Expression)!;
        StatementSyntax startAsyncStatement = method.Body.Statements.First(statement => statement.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation => IsStartAsyncOn(invocation, driverVariableName)));

        StatementSyntax registrationStatement = registrationInvocation.FirstAncestorOrSelf<StatementSyntax>()!;
        List<StatementSyntax>? statementsToMove = CollectStatementsToMove(registrationStatement, startAsyncStatement);
        if (statementsToMove is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                createChangedDocument: cancellationToken => MoveBeforeStartAsync(context.Document, method, statementsToMove, startAsyncStatement, cancellationToken),
                equivalenceKey),
            diagnostic);
    }

    private static List<StatementSyntax>? CollectStatementsToMove(StatementSyntax registrationStatement, StatementSyntax startAsyncStatement)
    {
        // The fix rearranges the statements of a single block, so it is offered only when the
        // registration is a statement of the very block that holds the start. Anything nested inside an
        // if, a loop or a try is conditional on that statement — hoisting it above the start would make
        // it unconditional — and a braced nested block is no different in that respect from an
        // unbraced embedded statement. Nesting also puts the dependency walk below out of reach of the
        // outer block, where a local the registration uses may be declared.
        if (registrationStatement.Parent is not BlockSyntax block || !ReferenceEquals(block, startAsyncStatement.Parent))
        {
            return null;
        }

        List<StatementSyntax> statementsToMove = [registrationStatement];

        // Walk the statements between the start and the registration backwards, pulling in every
        // local declaration that a statement already being moved refers to by name, so that a
        // declaration's own dependencies are found in turn.
        HashSet<string> referencedNames = new(GetReferencedNames(registrationStatement));
        int registrationIndex = block.Statements.IndexOf(registrationStatement);
        for (int index = registrationIndex - 1; index >= 0; index--)
        {
            StatementSyntax candidate = block.Statements[index];
            if (candidate.SpanStart <= startAsyncStatement.SpanStart)
            {
                break;
            }

            if (candidate is LocalDeclarationStatementSyntax declaration)
            {
                if (!declaration.Declaration.Variables.Any(variable => referencedNames.Contains(variable.Identifier.ValueText)))
                {
                    continue;
                }

                if (declaration.DescendantNodes().OfType<AwaitExpressionSyntax>().Any())
                {
                    return null;
                }

                statementsToMove.Insert(0, declaration);
                referencedNames.UnionWith(GetReferencedNames(declaration));
            }
            else if (WritesAnyOf(candidate, referencedNames))
            {
                // A statement that is not a declaration cannot be moved, but one that writes a name the
                // moved statements read — an assignment, or an out or ref argument — would be left behind
                // them, so the moved code would read the variable before it is assigned (CS0165) or read
                // a stale value. Decline rather than produce that.
                return null;
            }
        }

        return statementsToMove;
    }

    private static bool WritesAnyOf(StatementSyntax statement, HashSet<string> names)
    {
        foreach (SyntaxNode node in statement.DescendantNodes())
        {
            string? writtenName = node switch
            {
                AssignmentExpressionSyntax { Left: IdentifierNameSyntax target } => target.Identifier.ValueText,
                ArgumentSyntax { Expression: IdentifierNameSyntax target } argument
                    when argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) || argument.RefKindKeyword.IsKind(SyntaxKind.RefKeyword) => target.Identifier.ValueText,
                _ => null,
            };

            if (writtenName is not null && names.Contains(writtenName))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetReferencedNames(StatementSyntax statement)
    {
        return statement.DescendantNodes().OfType<IdentifierNameSyntax>().Select(identifier => identifier.Identifier.ValueText);
    }

    private static async Task<Document> MoveBeforeStartAsync(
        Document document,
        MethodDeclarationSyntax method,
        List<StatementSyntax> statementsToMove,
        StatementSyntax startAsyncStatement,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Track every statement involved through the removal so the insertion point survives it.
        MethodDeclarationSyntax trackedMethod = method.TrackNodes(statementsToMove.Append(startAsyncStatement));
        List<StatementSyntax> trackedStatementsToMove = statementsToMove.Select(statement => trackedMethod.GetCurrentNode(statement)!).ToList();
        MethodDeclarationSyntax methodWithoutMoved = trackedMethod.RemoveNodes(trackedStatementsToMove, SyntaxRemoveOptions.KeepNoTrivia)!;
        StatementSyntax currentStartAsyncStatement = methodWithoutMoved.GetCurrentNode(startAsyncStatement)!;

        // Each moved statement keeps its own trivia: the comments above it and any comment on the same line.
        IEnumerable<StatementSyntax> movedCopies = trackedStatementsToMove.Select(statement => statement.WithTrailingTrivia(GetTrailingTriviaForMove(statement)));
        MethodDeclarationSyntax newMethod = methodWithoutMoved.InsertNodesBefore(currentStartAsyncStatement, movedCopies);

        return document.WithSyntaxRoot(root.ReplaceNode(method, newMethod));
    }

    private static bool IsCompletedTask(ExpressionSyntax expression)
    {
        // `Task.CompletedTask` however the Task type is spelled (Task, Tasks.Task,
        // System.Threading.Tasks.Task), and nothing else. A textual "ends with Task.CompletedTask"
        // test would also match a conditional whose *second* arm is Task.CompletedTask — an
        // expression that still yields a task to await — and drop it from the rewritten handler.
        ExpressionSyntax current = expression;
        while (current is ParenthesizedExpressionSyntax parenthesized)
        {
            current = parenthesized.Expression;
        }

        return current is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "CompletedTask" } completedTask
            && completedTask.Expression is SimpleNameSyntax { Identifier.ValueText: "Task" } or MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Task" };
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
        // `await` binds tighter than a binary (`??`, `as`), conditional, assignment, cast or switch
        // expression, so such an operand has to be parenthesized: `await cond ? a : b` re-parses as
        // `(await cond) ? a : b`. Those are the operand shapes a Task-typed expression can take;
        // primary expressions — calls, member accesses, names, object creations, and anything
        // already parenthesized — need no parentheses.
        ExpressionSyntax operand = expression.WithoutTrivia();
        if (operand is BinaryExpressionSyntax
            or ConditionalExpressionSyntax
            or AssignmentExpressionSyntax
            or CastExpressionSyntax
            or SwitchExpressionSyntax)
        {
            operand = SyntaxFactory.ParenthesizedExpression(operand);
        }

        return SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.Token(SyntaxKind.AwaitKeyword).WithTrailingTrivia(SyntaxFactory.Space),
                operand));
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
