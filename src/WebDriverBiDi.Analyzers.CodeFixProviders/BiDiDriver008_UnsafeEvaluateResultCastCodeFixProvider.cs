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
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        SyntaxNode node = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .First(n => n is CastExpressionSyntax || n.IsKind(SyntaxKind.AsExpression));

        // Offer the action only for shapes the conversion can actually rewrite. Both transforms used
        // to return the document unchanged for the rest, which put an item on the light-bulb menu that
        // did nothing when chosen.
        if (node is CastExpressionSyntax castExpression)
        {
            if (!CanConvertCast(castExpression))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use pattern matching with 'is' expression",
                    createChangedDocument: c => ConvertCastToPatternMatchingAsync(
                        context.Document, castExpression, c),
                    equivalenceKey: "ConvertToPatternMatching"),
                diagnostic);
        }
        else
        {
            BinaryExpressionSyntax asExpression = (BinaryExpressionSyntax)node;
            if (GetInitializedDeclaration(asExpression) is null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use pattern matching with 'is' expression",
                    createChangedDocument: c => ConvertAsToPatternMatchingAsync(
                        context.Document, asExpression, c),
                    equivalenceKey: "ConvertToPatternMatching"),
                diagnostic);
        }
    }

    /// <summary>
    /// Gets the local declaration an expression initializes, if it initializes one.
    /// </summary>
    /// <param name="expression">The cast or <c>as</c> expression.</param>
    /// <returns>The declaration statement, or <see langword="null"/> when the expression is not a local's initializer.</returns>
    private static LocalDeclarationStatementSyntax? GetInitializedDeclaration(ExpressionSyntax expression)
    {
        // The last step is a cast rather than a pattern because a variable declaration is not always a
        // local declaration statement: the same shape appears in a field declaration and in a for-loop
        // initializer, neither of which this conversion rewrites.
        // A declarator's parent is always a variable declaration, so only the declaration's own parent
        // needs testing: the same shape appears in a field declaration and in a for-loop initializer,
        // neither of which this conversion rewrites.
        return expression.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }
            ? declarator.Parent!.Parent as LocalDeclarationStatementSyntax
            : null;
    }

    /// <summary>
    /// Determines whether a cast can be rewritten as a pattern match.
    /// </summary>
    /// <param name="castExpression">The cast to inspect.</param>
    /// <returns><see langword="true"/> if the conversion would change the document; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A cast that initializes a local is always convertible. Any other cast is rewritten by wrapping
    /// its enclosing statement in an <c>if</c>, which is only valid when that statement neither
    /// transfers control out of the containing member nor produces a required value: wrapping a
    /// return, throw or yield would leave a path that no longer returns or assigns (CS0161/CS0165).
    /// </remarks>
    private static bool CanConvertCast(CastExpressionSyntax castExpression)
    {
        return GetInitializedDeclaration(castExpression) is not null
            || castExpression.FirstAncestorOrSelf<StatementSyntax>() is ExpressionStatementSyntax or LocalDeclarationStatementSyntax;
    }

    private static async Task<Document> ConvertCastToPatternMatchingAsync(
        Document document,
        CastExpressionSyntax castExpression,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        SemanticModel semanticModel = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

        // Get the target type name
        ITypeSymbol targetType = semanticModel.GetTypeInfo(castExpression.Type, cancellationToken).Type!;

        // Check if this is a variable declaration: var success = (EvaluateResultSuccess)result;
        if (GetInitializedDeclaration(castExpression) is LocalDeclarationStatementSyntax declarationStatement)
        {
            return await ConvertCastInVariableDeclarationAsync(
                document,
                root,
                castExpression,
                (VariableDeclaratorSyntax)castExpression.Parent!.Parent!,
                declarationStatement,
                cancellationToken).ConfigureAwait(false);
        }

        // For inline casts (not in variable declarations), wrap just that expression. The action is
        // registered only for a statement shape that can be wrapped, so this is one.
        StatementSyntax statement = castExpression.FirstAncestorOrSelf<StatementSyntax>()!;

        // Generate a variable name based on the type
        string variableName = GenerateVariableName(targetType.Name);

        // Create pattern matching
        IsPatternExpressionSyntax isPattern = SyntaxFactory.IsPatternExpression(
            castExpression.Expression,
            SyntaxFactory.DeclarationPattern(
                castExpression.Type,
                SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(variableName))));

        // Replace the cast with the variable name in the statement
        StatementSyntax newStatement = statement.ReplaceNode(castExpression, SyntaxFactory.IdentifierName(variableName));

        // Wrap in an if statement with proper trivia
        IfStatementSyntax ifStatement = SyntaxFactory.IfStatement(
            isPattern,
            SyntaxFactory.Block(
                SyntaxFactory.SingletonList(newStatement.WithoutLeadingTrivia().WithoutTrailingTrivia())))
            .WithLeadingTrivia(statement.GetLeadingTrivia())
            .WithTrailingTrivia(statement.GetTrailingTrivia());

        SyntaxNode newRoot = root.ReplaceNode(statement, ifStatement);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ConvertCastInVariableDeclarationAsync(
        Document document,
        SyntaxNode root,
        CastExpressionSyntax castExpression,
        VariableDeclaratorSyntax variableDeclarator,
        LocalDeclarationStatementSyntax declarationStatement,
        CancellationToken cancellationToken)
    {
        string existingVariableName = variableDeclarator.Identifier.Text;

        // Find the containing block to look for dependent statements
        BlockSyntax containingBlock = (BlockSyntax)declarationStatement.Parent!;

        // Find the index of the declaration statement
        int declarationIndex = containingBlock.Statements.IndexOf(declarationStatement);

        // Get semantic model to find references
        SemanticModel semanticModel = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

        // Find the last statement after the declaration that references the variable, then move every
        // statement through it (including any intervening statements that do not reference it) into the
        // if block. Stopping at the first non-referencing statement would leave a later reference
        // outside the pattern variable's scope (CS0103/CS0165).
        ISymbol? variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken);

        int lastReferencingIndex = -1;
        for (int i = declarationIndex + 1; i < containingBlock.Statements.Count; i++)
        {
            if (StatementReferencesVariable(containingBlock.Statements[i], existingVariableName, semanticModel, variableSymbol, cancellationToken))
            {
                lastReferencingIndex = i;
            }
        }

        List<StatementSyntax> dependentStatements = [];
        for (int i = declarationIndex + 1; i <= lastReferencingIndex; i++)
        {
            dependentStatements.Add(containingBlock.Statements[i]);
        }

        // Create the pattern matching if statement
        IsPatternExpressionSyntax isPattern = SyntaxFactory.IsPatternExpression(
            castExpression.Expression,
            SyntaxFactory.DeclarationPattern(
                castExpression.Type,
                SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(existingVariableName))));

        // Create the statements for the if block, preserving their formatting
        List<StatementSyntax> ifBlockStatements = [];

        foreach (StatementSyntax stmt in dependentStatements)
        {
            ifBlockStatements.Add(stmt.WithoutLeadingTrivia());
        }

        // Create the if statement
        IfStatementSyntax ifStatement = SyntaxFactory.IfStatement(
            isPattern,
            SyntaxFactory.Block(SyntaxFactory.List(ifBlockStatements)))
            .WithLeadingTrivia(declarationStatement.GetLeadingTrivia())
            .WithTrailingTrivia(dependentStatements.LastOrDefault()?.GetTrailingTrivia() ?? declarationStatement.GetTrailingTrivia())
            .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

        // Build a new statement list for the containing block
        List<StatementSyntax> newStatements = [];

        for (int i = 0; i < containingBlock.Statements.Count; i++)
        {
            // Skip dependent statements (they're already in the if block)
            if (i < declarationIndex)
            {
                // Keep statements before the declaration
                newStatements.Add(containingBlock.Statements[i]);
            }
            else if (i == declarationIndex)
            {
                // Replace declaration with if statement
                newStatements.Add(ifStatement);
            }
            else if (i > declarationIndex + dependentStatements.Count)
            {
                // Keep statements after the dependent statements
                newStatements.Add(containingBlock.Statements[i]);
            }
        }

        // Create the new block
        BlockSyntax newBlock = containingBlock.WithStatements(SyntaxFactory.List(newStatements));

        // Replace the block in the tree
        SyntaxNode newRoot = root.ReplaceNode(containingBlock, newBlock);

        // Apply formatting to normalize whitespace
        newRoot = Microsoft.CodeAnalysis.Formatting.Formatter.Format(newRoot, document.Project.Solution.Workspace);

        // Normalize line endings — Roslyn always uses \n internally after parsing.
        string newRootText = newRoot.ToFullString();
        string normalizedText = newRootText.Replace("\r\n", "\n").Replace("\r", "\n");
        newRoot = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(normalizedText).GetRoot();

        return document.WithSyntaxRoot(newRoot);
    }

    private static bool StatementReferencesVariable(
        StatementSyntax statement,
        string variableName,
        SemanticModel semanticModel,
        ISymbol? variableSymbol,
        CancellationToken cancellationToken)
    {

        // Find all identifier nodes in the statement
        IEnumerable<IdentifierNameSyntax> identifiers = statement.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(id => id.Identifier.Text == variableName);

        foreach (IdentifierNameSyntax identifier in identifiers)
        {
            ISymbol? symbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
            if (SymbolEqualityComparer.Default.Equals(symbol, variableSymbol))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<Document> ConvertAsToPatternMatchingAsync(
        Document document,
        BinaryExpressionSyntax asExpression,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        SemanticModel semanticModel = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

        // Get the target type
        ITypeSymbol targetType = semanticModel.GetTypeInfo(asExpression.Right, cancellationToken).Type!;

        // The action is registered only when the expression initializes a local, so both of these are
        // present: var success = result as EvaluateResultSuccess;
        LocalDeclarationStatementSyntax declarationStatement = GetInitializedDeclaration(asExpression)!;
        VariableDeclaratorSyntax variableDeclarator = (VariableDeclaratorSyntax)asExpression.Parent!.Parent!;

        return await ConvertAsInVariableDeclarationAsync(
            document,
            root,
            asExpression,
            variableDeclarator,
            declarationStatement,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Document> ConvertAsInVariableDeclarationAsync(
        Document document,
        SyntaxNode root,
        BinaryExpressionSyntax asExpression,
        VariableDeclaratorSyntax variableDeclarator,
        LocalDeclarationStatementSyntax declarationStatement,
        CancellationToken cancellationToken)
    {
        string existingVariableName = variableDeclarator.Identifier.Text;

        // Find the containing block to look for dependent statements
        BlockSyntax containingBlock = (BlockSyntax)declarationStatement.Parent!;

        // Find the index of the declaration statement
        int declarationIndex = containingBlock.Statements.IndexOf(declarationStatement);

        // Get semantic model to find references
        SemanticModel semanticModel = (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

        // Find the last statement after the declaration that references the variable, then move every
        // statement through it (including any intervening statements that do not reference it) into the
        // if block. Stopping at the first non-referencing statement would leave a later reference
        // outside the pattern variable's scope (CS0103/CS0165).
        ISymbol? variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken);

        int lastReferencingIndex = -1;
        for (int i = declarationIndex + 1; i < containingBlock.Statements.Count; i++)
        {
            if (StatementReferencesVariable(containingBlock.Statements[i], existingVariableName, semanticModel, variableSymbol, cancellationToken))
            {
                lastReferencingIndex = i;
            }
        }

        List<StatementSyntax> dependentStatements = [];
        for (int i = declarationIndex + 1; i <= lastReferencingIndex; i++)
        {
            dependentStatements.Add(containingBlock.Statements[i]);
        }

        // Create: if (result is EvaluateResultSuccess success) instead of var success = result as ...
        IsPatternExpressionSyntax isPattern = SyntaxFactory.IsPatternExpression(
            asExpression.Left,
            SyntaxFactory.DeclarationPattern(
                (TypeSyntax)asExpression.Right,
                SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(existingVariableName))));

        // Create the statements for the if block, preserving their formatting
        List<StatementSyntax> ifBlockStatements = [];

        foreach (StatementSyntax stmt in dependentStatements)
        {
            ifBlockStatements.Add(stmt.WithoutLeadingTrivia());
        }

        // Create the if statement
        IfStatementSyntax ifStatement = SyntaxFactory.IfStatement(
            isPattern,
            SyntaxFactory.Block(SyntaxFactory.List(ifBlockStatements)))
            .WithLeadingTrivia(declarationStatement.GetLeadingTrivia())
            .WithTrailingTrivia(dependentStatements.LastOrDefault()?.GetTrailingTrivia() ?? declarationStatement.GetTrailingTrivia())
            .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

        // Build a new statement list for the containing block
        List<StatementSyntax> newStatements = [];

        for (int i = 0; i < containingBlock.Statements.Count; i++)
        {
            // Skip dependent statements (they're already in the if block)
            if (i < declarationIndex)
            {
                // Keep statements before the declaration
                newStatements.Add(containingBlock.Statements[i]);
            }
            else if (i == declarationIndex)
            {
                // Replace declaration with if statement
                newStatements.Add(ifStatement);
            }
            else if (i > declarationIndex + dependentStatements.Count)
            {
                // Keep statements after the dependent statements
                newStatements.Add(containingBlock.Statements[i]);
            }
        }

        // Create the new block
        BlockSyntax newBlock = containingBlock.WithStatements(SyntaxFactory.List(newStatements));

        // Replace the block in the tree
        SyntaxNode newRoot = root.ReplaceNode(containingBlock, newBlock);

        // Apply formatting to normalize whitespace
        newRoot = Microsoft.CodeAnalysis.Formatting.Formatter.Format(newRoot, document.Project.Solution.Workspace);

        // Normalize line endings — Roslyn always uses \n internally after parsing.
        string newRootText = newRoot.ToFullString();
        string normalizedText = newRootText.Replace("\r\n", "\n").Replace("\r", "\n");
        newRoot = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(normalizedText).GetRoot();

        return document.WithSyntaxRoot(newRoot);
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
