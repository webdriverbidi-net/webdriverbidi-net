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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi008");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        ITypeSymbol targetType = context.SemanticModel.GetTypeInfo(castExpression.Type).Type!;
        if (!IsEvaluateResultDerivedType(targetType))
        {
            return;
        }

        ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(castExpression.Expression).Type;

        // Check if the expression is of type EvaluateResult (base type)
        if (IsEvaluateResultBaseType(expressionType))
        {
            // Check if this cast is already inside a safe context (like try-catch or is expression)
            if (IsInSafeContext(context, castExpression))
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

        ITypeSymbol targetType = context.SemanticModel.GetTypeInfo(asExpression.Right).Type!;
        if (!IsEvaluateResultDerivedType(targetType))
        {
            return;
        }

        ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(asExpression.Left).Type;

        // Check if the expression is of type EvaluateResult (base type)
        if (IsEvaluateResultBaseType(expressionType))
        {
            // 'as' expressions that are followed by null checks are safer, but still not ideal
            // Report the diagnostic to suggest pattern matching
            Diagnostic diagnostic = Diagnostic.Create(Rule, asExpression.GetLocation(), targetType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsEvaluateResultBaseType(ITypeSymbol? type)
    {
        // The operand of a cast or 'as' can have no type (for example a null literal), so guard
        // against a null symbol rather than dereferencing it (which would surface as AD0001).
        return type is not null && type.Name == "EvaluateResult" && type.ContainingNamespace!.ToString() == "WebDriverBiDi.Script";
    }

    private static bool IsEvaluateResultDerivedType(ITypeSymbol? type)
    {
        // Unlike the cast/'as' operand handled by IsEvaluateResultBaseType, the target type passed
        // here comes from a type-name syntax and always resolves to a (possibly error) symbol, so no
        // null guard is required.
        if (type!.Name is "EvaluateResultSuccess" or "EvaluateResultException")
        {
            return type.ContainingNamespace!.ToString() == "WebDriverBiDi.Script";
        }

        return false;
    }

    private static bool IsInSafeContext(SyntaxNodeAnalysisContext context, CastExpressionSyntax castExpression)
    {
        // A cast is protected only when it sits in the *try block* of a try statement that has a catch
        // clause able to catch an InvalidCastException. Treating any enclosing try statement as
        // protection was wrong three ways: a try/finally catches nothing at all; a catch of an
        // unrelated type (IOException, say) never sees the cast failure; and a cast inside a catch or
        // finally block of the try is not covered by that try at all.
        SyntaxNode? current = castExpression;
        while (current != null)
        {
            // Stop at a method boundary; a try statement outside it does not enclose this code.
            // A lambda body is deliberately *not* treated as a boundary here: a lambda declared and
            // invoked inside the try does run under its catch, and this rule has no way to tell that
            // apart from one stored for later, so the existing behaviour is left alone.
            if (current is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
            {
                break;
            }

            if (current.Parent is TryStatementSyntax tryStatement
                && tryStatement.Block == current
                && HasCatchForInvalidCast(context, tryStatement))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a try statement has a catch clause that would catch an
    /// <see cref="InvalidCastException"/>.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="tryStatement">The try statement enclosing the cast.</param>
    /// <returns><see langword="true"/> if a catch clause covers an invalid cast; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// An untyped <c>catch</c> catches everything. A typed one covers the cast when its type is
    /// <see cref="InvalidCastException"/> or one of its base types. A clause carrying a <c>when</c>
    /// filter is still treated as covering: the filter may reject at run time, but the common idiom
    /// <c>catch (Exception ex) when (ex is InvalidCastException)</c> is deliberate handling, and
    /// reporting it would be a false positive on code that already does the right thing.
    /// </remarks>
    private static bool HasCatchForInvalidCast(SyntaxNodeAnalysisContext context, TryStatementSyntax tryStatement)
    {
        INamedTypeSymbol? invalidCastException = context.Compilation.GetTypeByMetadataName("System.InvalidCastException");
        foreach (CatchClauseSyntax catchClause in tryStatement.Catches)
        {
            if (catchClause.Declaration is null)
            {
                return true;
            }

            // The clause covers the cast when the caught type is InvalidCastException itself or one
            // of its base types (SystemException, Exception, object). The walk therefore runs up
            // InvalidCastException's own chain looking for the caught type — not up the caught type's
            // chain, which would match only InvalidCastException itself and would report the ordinary
            // catch (Exception) as unprotected.
            ITypeSymbol? caughtType = context.SemanticModel.GetTypeInfo(catchClause.Declaration.Type).Type;
            for (ITypeSymbol? current = invalidCastException; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(current, caughtType))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
