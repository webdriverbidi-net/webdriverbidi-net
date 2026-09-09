// <copyright file="BiDiDriver004_CancellationTokenSuggestionCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider that adds CancellationToken arguments to method calls, for BIDI004 and BIDI013.
/// </summary>
/// <remarks>
/// BIDI013 reports the same shape as BIDI004 — a call to a method with a cancellation-token overload
/// that passes no token — differing only in severity and in which operations it covers. Both are fixed
/// the same way, so one provider serves both rather than the warning-level rule having no fix at all.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver004_CancellationTokenSuggestionCodeFixProvider))]
[Shared]
public class BiDiDriver004_CancellationTokenSuggestionCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
        BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId,
        BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.DiagnosticId);

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

        SemanticModel semanticModel = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;
        INamedTypeSymbol? tokenType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

        // Write CancellationToken.None only when the type name resolves at the call site; otherwise
        // qualify it. Emitting the short name unconditionally produces code that does not compile in a
        // file without a using for System.Threading — which is the common case, since the diagnostic
        // fires precisely on calls that pass no token and so had no reason to import it.
        ExpressionSyntax noneExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IsCancellationTokenNameInScope(semanticModel, tokenType, invocation.SpanStart)
                ? SyntaxFactory.IdentifierName("CancellationToken")
                : SyntaxFactory.ParseExpression("System.Threading.CancellationToken"),
            SyntaxFactory.IdentifierName("None"));

        string tokenParameterName = FindTokenParameterName(semanticModel, invocation, context.CancellationToken);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add CancellationToken.None parameter",
                createChangedDocument: c => AddTokenArgumentAsync(
                    context.Document, invocation, noneExpression, tokenParameterName, c),
                equivalenceKey: "AddCancellationTokenNone"),
            diagnostic);

        // Offer to pass an existing token only when there is one in scope to pass. Inserting a bare
        // cancellationToken identifier where no such symbol exists produces code that does not compile,
        // and the name used is the one actually in scope rather than an assumed "cancellationToken".
        if (FindCancellationTokenInScope(semanticModel, tokenType, invocation.SpanStart) is string tokenName)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Add {tokenName} parameter",
                    createChangedDocument: c => AddTokenArgumentAsync(
                        context.Document, invocation, SyntaxFactory.IdentifierName(tokenName), tokenParameterName, c),
                    equivalenceKey: "AddCancellationTokenParameter"),
                diagnostic);
        }
    }

    /// <summary>
    /// Determines whether the simple name <c>CancellationToken</c> resolves to the token type at the
    /// given position, so that it can be written without qualification.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="tokenType">The <see cref="CancellationToken"/> type symbol, if the compilation has one.</param>
    /// <param name="position">The position of the call being fixed.</param>
    /// <returns><see langword="true"/> if the short name resolves; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The comparison is against the resolved type symbol, so a user's own type that merely shares the
    /// name does not count as the short name being available. A <see langword="null"/> token type
    /// compares equal to nothing, so an exotic compilation without one degrades to qualifying the name.
    /// </remarks>
    private static bool IsCancellationTokenNameInScope(SemanticModel semanticModel, INamedTypeSymbol? tokenType, int position)
    {
        return semanticModel.LookupNamespacesAndTypes(position, name: "CancellationToken")
            .Any(symbol => SymbolEqualityComparer.Default.Equals(symbol, tokenType));
    }

    /// <summary>
    /// Finds the name of a <see cref="CancellationToken"/>-typed symbol in scope at the given position.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="tokenType">The <see cref="CancellationToken"/> type symbol, if the compilation has one.</param>
    /// <param name="position">The position of the call being fixed.</param>
    /// <returns>The symbol's name, or <see langword="null"/> when no token is in scope.</returns>
    /// <remarks>
    /// A symbol literally named <c>cancellationToken</c> wins when there is one, so the offered fix
    /// matches the convention the surrounding code most likely uses; otherwise the first token-typed
    /// parameter, local, field or property in scope is taken.
    /// </remarks>
    private static string? FindCancellationTokenInScope(SemanticModel semanticModel, INamedTypeSymbol? tokenType, int position)
    {
        string? firstMatch = null;
        foreach (ISymbol symbol in semanticModel.LookupSymbols(position))
        {
            ITypeSymbol? symbolType = symbol switch
            {
                IParameterSymbol parameter => parameter.Type,
                ILocalSymbol local => local.Type,
                IFieldSymbol field => field.Type,
                IPropertySymbol property => property.Type,
                _ => null,
            };

            if (!SymbolEqualityComparer.Default.Equals(symbolType, tokenType))
            {
                continue;
            }

            if (symbol.Name == "cancellationToken")
            {
                return symbol.Name;
            }

            firstMatch ??= symbol.Name;
        }

        return firstMatch;
    }

    /// <summary>
    /// Finds the name of the cancellation-token parameter the inserted argument has to name.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="invocation">The call being fixed.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The parameter's name.</returns>
    /// <remarks>
    /// Both analyzers that this fix serves report calls on a <c>Module</c> subclass as well as on the
    /// driver, and a user-written module declares its own methods, whose token parameter can be called
    /// anything. Naming the argument <c>cancellationToken</c> regardless produces CS1739 on such a
    /// method. The name is read from whichever overload supplies the parameter, matching how the
    /// analyzer decided to report in the first place.
    /// </remarks>
    private static string FindTokenParameterName(
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        // The analyzer resolved this call and confirmed that some overload of it takes a token, so both
        // the method symbol and the parameter are there to be found.
        IMethodSymbol method = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol!;

        // The call binds either to a method with an optional token parameter of its own or to one whose
        // sibling overload takes the token; the first is preferred, because that is the parameter the
        // argument will actually bind to.
        IParameterSymbol tokenParameter = FindTokenParameter(method)
            ?? method.ContainingType.GetMembers(method.Name)
                .OfType<IMethodSymbol>()
                .Select(FindTokenParameter)
                .First(parameter => parameter is not null)!;

        return tokenParameter.Name;
    }

    /// <summary>
    /// Finds a method's cancellation-token parameter, if it has one.
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <returns>The parameter, or <see langword="null"/>.</returns>
    private static IParameterSymbol? FindTokenParameter(IMethodSymbol method)
    {
        return method.Parameters.FirstOrDefault(parameter => parameter.Type.Name == "CancellationToken");
    }

    private static async Task<Document> AddTokenArgumentAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        ExpressionSyntax tokenExpression,
        string tokenParameterName,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Add the token as a named argument. The target methods declare an optional parameter (for
        // example a TimeSpan? timeout) before the trailing cancellationToken, so a positional append
        // would bind to that parameter and fail to compile; naming the argument targets the token
        // parameter regardless of the intervening optional parameters.
        ArgumentSyntax tokenArgument = SyntaxFactory.Argument(tokenExpression)
            .WithNameColon(SyntaxFactory.NameColon(tokenParameterName));

        ArgumentListSyntax newArgumentList = invocation.ArgumentList.AddArguments(tokenArgument);
        InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(newArgumentList);

        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
