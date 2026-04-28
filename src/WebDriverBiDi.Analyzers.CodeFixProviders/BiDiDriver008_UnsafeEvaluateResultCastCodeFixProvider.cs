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

        SyntaxNode? node = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .FirstOrDefault(n => n is CastExpressionSyntax || n is BinaryExpressionSyntax);

        if (node == null)
        {
            return;
        }

        if (node is CastExpressionSyntax castExpression)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use pattern matching with 'is' expression",
                    createChangedDocument: c => ConvertCastToPatternMatchingAsync(
                        context.Document, castExpression, c),
                    equivalenceKey: "ConvertToPatternMatching"),
                diagnostic);
        }
        else if (node is BinaryExpressionSyntax asExpression && asExpression.IsKind(SyntaxKind.AsExpression))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use pattern matching with 'is' expression",
                    createChangedDocument: c => ConvertAsToPatternMatchingAsync(
                        context.Document, asExpression, c),
                    equivalenceKey: "ConvertToPatternMatching"),
                diagnostic);
        }
    }

    private static async Task<Document> ConvertCastToPatternMatchingAsync(
        Document document,
        CastExpressionSyntax castExpression,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        // Get the target type name
        ITypeSymbol? targetType = semanticModel.GetTypeInfo(castExpression.Type, cancellationToken).Type;
        if (targetType == null)
        {
            return document;
        }

        // Check if this is a variable declaration: var success = (EvaluateResultSuccess)result;
        if (castExpression.Parent is EqualsValueClauseSyntax equalsValue &&
            equalsValue.Parent is VariableDeclaratorSyntax variableDeclarator &&
            variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration &&
            variableDeclaration.Parent is LocalDeclarationStatementSyntax declarationStatement)
        {
            return await ConvertCastInVariableDeclarationAsync(
                document,
                root,
                castExpression,
                variableDeclarator,
                declarationStatement,
                cancellationToken).ConfigureAwait(false);
        }

        // For inline casts (not in variable declarations), wrap just that expression
        StatementSyntax? statement = castExpression.FirstAncestorOrSelf<StatementSyntax>();
        if (statement == null)
        {
            return document;
        }

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
        if (declarationStatement.Parent is not BlockSyntax containingBlock)
        {
            return document;
        }

        // Find the index of the declaration statement
        int declarationIndex = containingBlock.Statements.IndexOf(declarationStatement);
        if (declarationIndex == -1)
        {
            return document;
        }

        // Get semantic model to find references
        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        // Find all statements after the declaration that reference the variable
        List<StatementSyntax> dependentStatements = [];
        ISymbol? variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken);

        for (int i = declarationIndex + 1; i < containingBlock.Statements.Count; i++)
        {
            StatementSyntax statement = containingBlock.Statements[i];

            // Check if this statement references the variable
            if (StatementReferencesVariable(statement, existingVariableName, semanticModel, variableSymbol, cancellationToken))
            {
                dependentStatements.Add(statement);
            }
            else
            {
                // Stop at the first statement that doesn't use the variable
                break;
            }
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

        // Normalize line endings to match the original document's line ending style
        string originalText = root.ToFullString();
        string lineEnding = originalText.Contains("\r\n") ? "\r\n" : "\n";
        string newRootText = newRoot.ToFullString();
        string normalizedText = newRootText.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", lineEnding);
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
        if (variableSymbol == null)
        {
            // Fallback to simple text matching
            return statement.ToString().Contains(variableName);
        }

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
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        // Get the target type
        ITypeSymbol? targetType = semanticModel.GetTypeInfo(asExpression.Right, cancellationToken).Type;
        if (targetType == null)
        {
            return document;
        }

        // Find the variable declaration or assignment that uses this 'as' expression
        SyntaxNode? parent = asExpression.Parent;

        // Check if this is a variable declaration: var success = result as EvaluateResultSuccess;
        if (parent is EqualsValueClauseSyntax equalsValue &&
            equalsValue.Parent is VariableDeclaratorSyntax variableDeclarator &&
            variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration &&
            variableDeclaration.Parent is LocalDeclarationStatementSyntax declarationStatement)
        {
            return await ConvertAsInVariableDeclarationAsync(
                document,
                root,
                asExpression,
                variableDeclarator,
                declarationStatement,
                cancellationToken).ConfigureAwait(false);
        }

        // For other cases, we can't safely refactor
        return document;
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
        if (declarationStatement.Parent is not BlockSyntax containingBlock)
        {
            return document;
        }

        // Find the index of the declaration statement
        int declarationIndex = containingBlock.Statements.IndexOf(declarationStatement);
        if (declarationIndex == -1)
        {
            return document;
        }

        // Get semantic model to find references
        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        // Find all statements after the declaration that reference the variable
        List<StatementSyntax> dependentStatements = [];
        ISymbol? variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken);

        for (int i = declarationIndex + 1; i < containingBlock.Statements.Count; i++)
        {
            StatementSyntax statement = containingBlock.Statements[i];

            // Check if this statement references the variable
            if (StatementReferencesVariable(statement, existingVariableName, semanticModel, variableSymbol, cancellationToken))
            {
                dependentStatements.Add(statement);
            }
            else
            {
                // Stop at the first statement that doesn't use the variable
                break;
            }
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

        // Normalize line endings to match the original document's line ending style
        string originalText = root.ToFullString();
        string lineEnding = originalText.Contains("\r\n") ? "\r\n" : "\n";
        string newRootText = newRoot.ToFullString();
        string normalizedText = newRootText.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", lineEnding);
        newRoot = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(normalizedText).GetRoot();

        return document.WithSyntaxRoot(newRoot);
    }

    private static string GenerateVariableName(string typeName)
    {
        // Convert PascalCase type name to camelCase variable name
        // EvaluateResultSuccess -> success
        // EvaluateResultException -> exception
        if (typeName.StartsWith("EvaluateResult"))
        {
            string suffix = typeName.Substring("EvaluateResult".Length);
            if (!string.IsNullOrEmpty(suffix))
            {
                return char.ToLowerInvariant(suffix[0]) + suffix.Substring(1);
            }
        }

        // Fallback: just lowercase the first character
        return char.ToLowerInvariant(typeName[0]) + typeName.Substring(1);
    }
}
