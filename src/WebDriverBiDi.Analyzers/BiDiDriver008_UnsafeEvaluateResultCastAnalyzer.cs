// <copyright file="BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects unsafe casts from EvaluateResult to EvaluateResultSuccess.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver008_UnsafeEvaluateResultCastAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI008";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Use pattern matching for EvaluateResult type checking";

    private static readonly LocalizableString MessageFormat = "Unsafe cast to '{0}' detected. Use pattern matching (e.g., 'if (result is {0} success)') to safely check the result type.";

    private static readonly LocalizableString Description = "EvaluateResult can be either EvaluateResultSuccess or EvaluateResultException. Direct casting without type checking can cause InvalidCastException. Use pattern matching to safely handle both cases.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeCastExpression, SyntaxKind.CastExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAsExpression, SyntaxKind.AsExpression);
    }

    private static void AnalyzeCastExpression(SyntaxNodeAnalysisContext context)
    {
        CastExpressionSyntax castExpression = (CastExpressionSyntax)context.Node;

        ITypeSymbol? targetType = context.SemanticModel.GetTypeInfo(castExpression.Type).Type;
        if (targetType == null)
        {
            return;
        }

        // Check if casting to EvaluateResultSuccess or EvaluateResultException
        if (!IsEvaluateResultDerivedType(targetType))
        {
            return;
        }

        ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(castExpression.Expression).Type;
        if (expressionType == null)
        {
            return;
        }

        // Check if the expression is of type EvaluateResult (base type)
        if (IsEvaluateResultBaseType(expressionType))
        {
            // Check if this cast is already inside a safe context (like try-catch or is expression)
            if (IsInSafeContext(castExpression))
            {
                return;
            }

            Diagnostic diagnostic = Diagnostic.Create(Rule, castExpression.GetLocation(), targetType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeAsExpression(SyntaxNodeAnalysisContext context)
    {
        BinaryExpressionSyntax asExpression = (BinaryExpressionSyntax)context.Node;

        ITypeSymbol? targetType = context.SemanticModel.GetTypeInfo(asExpression.Right).Type;
        if (targetType == null)
        {
            return;
        }

        // Check if casting to EvaluateResultSuccess or EvaluateResultException
        if (!IsEvaluateResultDerivedType(targetType))
        {
            return;
        }

        ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(asExpression.Left).Type;
        if (expressionType == null)
        {
            return;
        }

        // Check if the expression is of type EvaluateResult (base type)
        if (IsEvaluateResultBaseType(expressionType))
        {
            // 'as' expressions that are followed by null checks are safer, but still not ideal
            // Report with info severity to suggest pattern matching
            Diagnostic diagnostic = Diagnostic.Create(Rule, asExpression.GetLocation(), targetType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsEvaluateResultBaseType(ITypeSymbol type)
    {
        return type.Name == "EvaluateResult" && type.ContainingNamespace?.ToString() == "WebDriverBiDi.Script";
    }

    private static bool IsEvaluateResultDerivedType(ITypeSymbol type)
    {
        if (type.Name is "EvaluateResultSuccess" or "EvaluateResultException")
        {
            return type.ContainingNamespace?.ToString() == "WebDriverBiDi.Script";
        }

        return false;
    }

    private static bool IsInSafeContext(CastExpressionSyntax castExpression)
    {
        // Check if the cast is inside a try-catch block
        SyntaxNode? current = castExpression.Parent;
        while (current != null)
        {
            if (current is TryStatementSyntax)
            {
                return true;
            }

            // Stop at method boundary
            if (current is MethodDeclarationSyntax || current is LocalFunctionStatementSyntax)
            {
                break;
            }

            current = current.Parent;
        }

        return false;
    }
}
