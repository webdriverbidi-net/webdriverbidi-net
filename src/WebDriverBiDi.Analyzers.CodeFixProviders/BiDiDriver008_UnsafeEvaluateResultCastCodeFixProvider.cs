// <copyright file="BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Formatting;

/// <summary>
/// Code fix provider for BIDI008 that converts unsafe casts to pattern matching.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider))]
[Shared]
public class BiDiDriver008_UnsafeEvaluateResultCastCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();
        ExpressionSyntax conversion = (ExpressionSyntax)root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent!.AncestorsAndSelf()
            .First(n => n is CastExpressionSyntax || n.IsKind(SyntaxKind.AsExpression));

        // Offer the action only for shapes the conversion can actually rewrite; an action that
        // returns the document unchanged would put an item on the light-bulb menu that does nothing.
        if (!CanConvert(conversion))
        {
            return;
        }

        SemanticModel semanticModel = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;
        if (!IsRewriteFaithful(conversion, semanticModel, context.CancellationToken))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use pattern matching with 'is' expression",
                createChangedDocument: c => ConvertToPatternMatchingAsync(context.Document, conversion, c),
                equivalenceKey: "ConvertToPatternMatching"),
            diagnostic);
    }

    /// <summary>
    /// Gets the local declaration statement whose single variable the conversion directly initializes:
    /// <c>var success = (EvaluateResultSuccess)result;</c> or <c>var success = result as EvaluateResultSuccess;</c>.
    /// </summary>
    /// <param name="conversion">The cast or <c>as</c> expression.</param>
    /// <returns>The declaration, or <see langword="null"/> when the conversion is not the direct initializer of a single local.</returns>
    private static LocalDeclarationStatementSyntax? GetDirectlyInitializedDeclaration(ExpressionSyntax conversion)
    {
        // A declarator's parent is always a variable declaration, so only the declaration's own parent
        // needs testing: the same shape appears in a field declaration and in a for-loop initializer,
        // neither of which this conversion rewrites. A declaration of several variables is rewritten
        // by the nested-conversion path instead, which keeps every declarator.
        if (conversion.Parent is not EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator })
        {
            return null;
        }

        VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)declarator.Parent!;
        return declaration.Variables.Count == 1 && declaration.Parent is LocalDeclarationStatementSyntax statement
            ? statement
            : null;
    }

    private static bool CanConvert(ExpressionSyntax conversion)
    {
        // A declaration is rewritten together with the statements that use its variables, which needs
        // the enclosing statement list: a block. (A declaration elsewhere — directly in a switch
        // section, say — has no such list.) An expression statement is wrapped on its own. An `as`
        // expression is rewritten only as a local's direct initializer, where its null-on-failure
        // result is what the pattern variable replaces.
        StatementSyntax? statement = conversion.FirstAncestorOrSelf<StatementSyntax>();
        if (conversion is not CastExpressionSyntax)
        {
            return GetDirectlyInitializedDeclaration(conversion) is { Parent: BlockSyntax };
        }

        return statement is ExpressionStatementSyntax
            || statement is LocalDeclarationStatementSyntax { Parent: BlockSyntax };
    }

    /// <summary>
    /// Finds the index of the last statement that moves into the generated <c>if</c> block along with
    /// the declaration, or -1 when no statement follows it into the block.
    /// </summary>
    /// <param name="declaration">The declaration the conversion belongs to.</param>
    /// <param name="containingBlock">The block holding the declaration.</param>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The index of the last statement to move, or -1.</returns>
    /// <remarks>
    /// A declaration's variables are visible to the rest of the block, so every statement through the
    /// last one that uses any of them has to move — including intervening statements that use none of
    /// them. Stopping at the first non-referencing statement would leave a later use outside the
    /// pattern variable's scope (CS0103/CS0165). The locals those moved statements declare move with
    /// them, so a later use of one of <em>those</em> pulls its statement in as well.
    /// </remarks>
    private static int FindLastMovedStatementIndex(
        LocalDeclarationStatementSyntax declaration,
        BlockSyntax containingBlock,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        HashSet<ISymbol> movedSymbols = new(GetDeclaredLocals(declaration, semanticModel, cancellationToken), SymbolEqualityComparer.Default);
        int declarationIndex = containingBlock.Statements.IndexOf(declaration);
        int lastReferencingIndex = -1;
        for (int i = declarationIndex + 1; i < containingBlock.Statements.Count; i++)
        {
            if (StatementReferencesAny(containingBlock.Statements[i], movedSymbols, semanticModel, cancellationToken))
            {
                for (int moved = lastReferencingIndex + 1; moved <= i; moved++)
                {
                    movedSymbols.UnionWith(GetDeclaredLocals(containingBlock.Statements[moved], semanticModel, cancellationToken));
                }

                lastReferencingIndex = i;
            }
        }

        return lastReferencingIndex;
    }

    /// <summary>
    /// Determines whether wrapping the statements this fix would move in an <c>if</c> leaves code that
    /// still compiles and still does what the original did for the case the conversion fails.
    /// </summary>
    /// <param name="conversion">The cast or <c>as</c> expression.</param>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true"/> if the rewrite is faithful; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// Statements that always leave the member cease to do so once they sit inside an <c>if</c>: the
    /// code after it becomes reachable, and a method that returns a value no longer returns on every
    /// path (CS0161). <c>var success = (EvaluateResultSuccess)result; return success.Result;</c> is the
    /// short form of that. Control-flow analysis answers it exactly, so only the shapes that would stop
    /// compiling are declined.
    /// </para>
    /// <para>
    /// An <c>as</c> conversion yields <see langword="null"/> when the test fails, and a null guard is
    /// how the author handles that. The pattern variable is never null inside the block, so a guard
    /// that leaves the member — <c>if (success is null) { return; }</c> — becomes dead code and stops
    /// covering the failing case, which would then fall out of the <c>if</c> and carry on. A guard that
    /// merely wraps the work changes nothing and still converts.
    /// </para>
    /// </remarks>
    private static bool IsRewriteFaithful(
        ExpressionSyntax conversion,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        StatementSyntax statement = conversion.FirstAncestorOrSelf<StatementSyntax>()!;
        if (statement is not LocalDeclarationStatementSyntax declaration || statement.Parent is not BlockSyntax containingBlock)
        {
            // An expression statement is wrapped on its own.
            return EndPointIsReachable(semanticModel, statement, statement);
        }

        int declarationIndex = containingBlock.Statements.IndexOf(declaration);
        int lastReferencingIndex = FindLastMovedStatementIndex(declaration, containingBlock, semanticModel, cancellationToken);
        int lastReplacedIndex = lastReferencingIndex >= 0 ? lastReferencingIndex : declarationIndex;
        if (!EndPointIsReachable(semanticModel, containingBlock.Statements[declarationIndex], containingBlock.Statements[lastReplacedIndex]))
        {
            return false;
        }

        if (conversion is CastExpressionSyntax)
        {
            return true;
        }

        // A declarator in a document that compiles always has a symbol.
        ISymbol declaredVariable = semanticModel.GetDeclaredSymbol(declaration.Declaration.Variables[0], cancellationToken)!;
        return !HasExitingNullGuard(containingBlock, declarationIndex, lastReplacedIndex, declaredVariable, semanticModel, cancellationToken);
    }

    /// <summary>
    /// Determines whether control can reach the end of the given run of statements.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="firstStatement">The first statement of the run.</param>
    /// <param name="lastStatement">The last statement of the run.</param>
    /// <returns><see langword="true"/> if the end point is reachable; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// The runs asked about here are always statements of a block that the diagnostic was reported in,
    /// so the analysis always succeeds.
    /// </remarks>
    private static bool EndPointIsReachable(SemanticModel semanticModel, StatementSyntax firstStatement, StatementSyntax lastStatement)
    {
        ControlFlowAnalysis analysis = ReferenceEquals(firstStatement, lastStatement)
            ? semanticModel.AnalyzeControlFlow(firstStatement)!
            : semanticModel.AnalyzeControlFlow(firstStatement, lastStatement)!;
        return analysis.EndPointIsReachable;
    }

    /// <summary>
    /// Determines whether any statement that would move into the <c>if</c> block tests the declared
    /// variable for null and leaves the member when the test succeeds.
    /// </summary>
    /// <param name="containingBlock">The block holding the declaration.</param>
    /// <param name="declarationIndex">The index of the declaration.</param>
    /// <param name="lastReplacedIndex">The index of the last statement that moves.</param>
    /// <param name="declaredVariable">The local the conversion initializes.</param>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true"/> if such a guard is present; otherwise, <see langword="false"/>.</returns>
    private static bool HasExitingNullGuard(
        BlockSyntax containingBlock,
        int declarationIndex,
        int lastReplacedIndex,
        ISymbol declaredVariable,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        for (int i = declarationIndex + 1; i <= lastReplacedIndex; i++)
        {
            foreach (IfStatementSyntax ifStatement in containingBlock.Statements[i].DescendantNodesAndSelf().OfType<IfStatementSyntax>())
            {
                if (IsNullTestOf(ifStatement.Condition, declaredVariable, semanticModel, cancellationToken)
                    && !EndPointIsReachable(semanticModel, ifStatement.Statement, ifStatement.Statement))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether an expression tests the given variable for being null.
    /// </summary>
    /// <param name="condition">The condition to inspect.</param>
    /// <param name="declaredVariable">The local the conversion initializes.</param>
    /// <param name="semanticModel">The semantic model for the document.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true"/> if the expression is such a test; otherwise, <see langword="false"/>.</returns>
    private static bool IsNullTestOf(
        ExpressionSyntax condition,
        ISymbol declaredVariable,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Both spellings of the test, in either operand order: `success == null` and `success is null`.
        ExpressionSyntax? tested = condition switch
        {
            BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression) =>
                binary.Right.IsKind(SyntaxKind.NullLiteralExpression) ? binary.Left
                    : binary.Left.IsKind(SyntaxKind.NullLiteralExpression) ? binary.Right : null,
            IsPatternExpressionSyntax { Pattern: ConstantPatternSyntax constant } isPattern
                when constant.Expression.IsKind(SyntaxKind.NullLiteralExpression) => isPattern.Expression,
            _ => null,
        };

        return tested is not null
            && SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(tested, cancellationToken).Symbol, declaredVariable);
    }

    private static async Task<Document> ConvertToPatternMatchingAsync(
        Document document,
        ExpressionSyntax conversion,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        SemanticModel semanticModel = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

        (ExpressionSyntax operand, TypeSyntax targetType) = conversion switch
        {
            CastExpressionSyntax cast => (cast.Expression, cast.Type),
            _ => (((BinaryExpressionSyntax)conversion).Left, (TypeSyntax)((BinaryExpressionSyntax)conversion).Right),
        };

        StatementSyntax statement = conversion.FirstAncestorOrSelf<StatementSyntax>()!;
        LocalDeclarationStatementSyntax? directDeclaration = GetDirectlyInitializedDeclaration(conversion);

        // The pattern variable takes over the local's name when the conversion initializes it
        // directly; otherwise a name derived from the type is introduced.
        string variableName = directDeclaration is not null
            ? directDeclaration.Declaration.Variables[0].Identifier.Text
            : GenerateVariableName(semanticModel.GetTypeInfo(targetType, cancellationToken).Type!.Name);

        IsPatternExpressionSyntax isPattern = SyntaxFactory.IsPatternExpression(
            operand,
            SyntaxFactory.DeclarationPattern(
                targetType,
                SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(variableName))));

        if (statement is not LocalDeclarationStatementSyntax declaration || statement.Parent is not BlockSyntax containingBlock)
        {
            // An expression statement declares nothing that later statements could use, so wrapping
            // just that statement keeps every name in scope.
            StatementSyntax newStatement = statement.ReplaceNode(conversion, SyntaxFactory.IdentifierName(variableName));
            IfStatementSyntax wrapped = SyntaxFactory.IfStatement(
                isPattern,
                SyntaxFactory.Block(SyntaxFactory.SingletonList(newStatement.WithoutLeadingTrivia().WithoutTrailingTrivia())))
                .WithLeadingTrivia(statement.GetLeadingTrivia())
                .WithTrailingTrivia(statement.GetTrailingTrivia());
            return document.WithSyntaxRoot(root.ReplaceNode(statement, wrapped));
        }

        // A declaration's variables are visible to the rest of the block, so every statement through
        // the last one that uses any of them moves into the if block along with the declaration —
        // including intervening statements that use none of them. Stopping at the first
        // non-referencing statement would leave a later use outside the pattern variable's scope
        // (CS0103/CS0165). The locals those moved statements declare move with them, so a later use
        // of one of *those* pulls its statement in as well. When the conversion is the direct
        // initializer, the declaration itself is replaced by the pattern; otherwise it moves into the
        // block with the conversion replaced by the pattern variable, so that
        // `RemoteValue value = ((EvaluateResultSuccess)result).Result;` keeps `value` in scope for
        // the statements that follow it.
        int declarationIndex = containingBlock.Statements.IndexOf(declaration);
        int lastReferencingIndex = FindLastMovedStatementIndex(declaration, containingBlock, semanticModel, cancellationToken);

        List<StatementSyntax> ifBlockStatements = [];
        if (directDeclaration is null)
        {
            ifBlockStatements.Add(declaration.ReplaceNode(conversion, SyntaxFactory.IdentifierName(variableName)).WithoutLeadingTrivia());
        }

        for (int i = declarationIndex + 1; i <= lastReferencingIndex; i++)
        {
            ifBlockStatements.Add(containingBlock.Statements[i].WithoutLeadingTrivia());
        }

        // The if statement replaces the declaration and every statement through the last
        // referencing one; with no referencing statement it replaces the declaration alone.
        int lastReplacedIndex = lastReferencingIndex >= 0 ? lastReferencingIndex : declarationIndex;
        IfStatementSyntax ifStatement = SyntaxFactory.IfStatement(
            isPattern,
            SyntaxFactory.Block(SyntaxFactory.List(ifBlockStatements)))
            .WithLeadingTrivia(declaration.GetLeadingTrivia())
            .WithTrailingTrivia(containingBlock.Statements[lastReplacedIndex].GetTrailingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);

        List<StatementSyntax> newStatements = [];
        for (int i = 0; i < containingBlock.Statements.Count; i++)
        {
            if (i < declarationIndex || i > lastReplacedIndex)
            {
                newStatements.Add(containingBlock.Statements[i]);
            }
            else if (i == declarationIndex)
            {
                newStatements.Add(ifStatement);
            }
        }

        SyntaxNode newRoot = root.ReplaceNode(containingBlock, containingBlock.WithStatements(SyntaxFactory.List(newStatements)));

        // The generated if statement carries Formatter.Annotation, so the host indents it as part of
        // applying the fix. Formatting the whole root here instead would reformat untouched code, and
        // reparsing its text would rewrite every line ending in the file to match Roslyn's internal
        // \n, corrupting a document that uses \r\n.
        return document.WithSyntaxRoot(newRoot);
    }

    private static IEnumerable<ISymbol> GetDeclaredLocals(StatementSyntax statement, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        // Locals declared by a declaration statement (`int x = 1;`) and by a designation anywhere in
        // the statement (`out var x`, `is T x`).
        return statement.DescendantNodesAndSelf()
            .Where(node => node is VariableDeclaratorSyntax or SingleVariableDesignationSyntax)
            .Select(node => semanticModel.GetDeclaredSymbol(node, cancellationToken))
            .OfType<ISymbol>();
    }

    private static bool StatementReferencesAny(
        StatementSyntax statement,
        HashSet<ISymbol> variableSymbols,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Match by symbol rather than by name so a same-named identifier bound elsewhere (a member
        // access, a lambda parameter) does not drag its statement into the block. An identifier that
        // binds to nothing (a member of a dynamic receiver, say) is skipped.
        return statement.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(identifier => semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol)
            .OfType<ISymbol>()
            .Any(variableSymbols.Contains);
    }

    private static string GenerateVariableName(string typeName)
    {
        // Convert PascalCase type name to camelCase variable name
        // EvaluateResultSuccess -> success
        // EvaluateResultException -> exception
        string suffix = typeName.Substring("EvaluateResult".Length);
        return char.ToLowerInvariant(suffix[0]) + suffix.Substring(1);
    }
}
