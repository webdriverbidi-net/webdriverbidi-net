// <copyright file="BiDiDriver008_UnsafeEvaluateResultCastAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Linq;
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

    private static readonly LocalizableString Title = "Use pattern matching or TryAs for EvaluateResult type checking";

    private static readonly LocalizableString MessageFormat = "Unsafe cast to '{0}' detected. Use pattern matching (e.g., 'if (result is {0} success)') or TryAs (e.g., 'if (result.TryAs(out {0}? success))') to safely check the result type.";

    private static readonly LocalizableString Description = "EvaluateResult can be either EvaluateResultSuccess or EvaluateResultException. Direct casting without type checking can cause InvalidCastException. Use pattern matching or TryAs<T>() to safely handle both cases, or As<T>() when the result type is known, which throws a WebDriverBiDiException if it is not.";

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
            // A cast that cannot fail is not unsafe: one whose failure is caught, or one that runs
            // only after the operand's type has been established.
            if (IsInProtectedTryBlock(context, castExpression) || IsGuardedByTypeTest(context, castExpression))
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

    private static bool IsInProtectedTryBlock(SyntaxNodeAnalysisContext context, CastExpressionSyntax castExpression)
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

    /// <summary>
    /// Determines whether the cast runs only after a test has established that the operand is of the
    /// target type, so that the cast cannot fail.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="castExpression">The cast.</param>
    /// <returns><see langword="true"/> if a type test guards the cast; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Two tests establish the type: the discriminator the library exposes for exactly this purpose,
    /// <c>result.ResultType == EvaluateResultType.Success</c> (or <c>Exception</c>), and a type test,
    /// <c>result is EvaluateResultSuccess</c>. The test guards the cast when the cast sits in the
    /// then-branch of an <c>if</c> on it, in the true arm of a conditional expression on it, on the
    /// right of an <c>&amp;&amp;</c> whose left operand is it, in a <c>switch</c> section every label
    /// of which selects it, in a <c>switch</c> expression arm that selects it, or after an <c>if</c> on
    /// its negation whose body leaves the enclosing block (a return, throw, break or continue guard). A
    /// label or arm selects it by its pattern, or by a <c>when</c> clause on it, which guards the
    /// section's statements or the arm's expression but not the clause itself. Only a syntactically identical operand counts:
    /// the test and the cast must name the same expression.
    /// </remarks>
    private static bool IsGuardedByTypeTest(SyntaxNodeAnalysisContext context, CastExpressionSyntax castExpression)
    {
        // The target type resolved when the cast was classified as an EvaluateResult-derived cast.
        TypeTest test = new(context.SemanticModel, castExpression.Expression, context.SemanticModel.GetTypeInfo(castExpression.Type).Type!);

        SyntaxNode child = castExpression;
        for (SyntaxNode? current = castExpression.Parent; current is not null; child = current, current = current.Parent)
        {
            // A guard outside the member, or outside a nested function, does not hold inside it: a
            // nested function's body runs when the delegate is invoked, not where it is written.
            if (current is MethodDeclarationSyntax || !AnalyzerSymbolHelpers.DoesNotBeginNestedFunction(current))
            {
                break;
            }

            ExpressionSyntax? condition = current switch
            {
                IfStatementSyntax ifStatement when ifStatement.Statement == child => ifStatement.Condition,
                ConditionalExpressionSyntax conditional when conditional.WhenTrue == child => conditional.Condition,
                BinaryExpressionSyntax logicalAnd when logicalAnd.IsKind(SyntaxKind.LogicalAndExpression) && logicalAnd.Right == child => logicalAnd.Left,
                _ => null,
            };

            if (condition is not null && test.IsEstablishedBy(condition))
            {
                return true;
            }

            if (current is SwitchSectionSyntax section && test.IsEstablishedBySwitchSection(section, child))
            {
                return true;
            }

            if (current is SwitchExpressionArmSyntax arm && test.IsEstablishedBySwitchExpressionArm(arm, child))
            {
                return true;
            }

            if (current is BlockSyntax block && child is StatementSyntax statement && test.IsEstablishedByEarlyExitBefore(block, statement))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Recognizes the conditions that establish, or rule out, that a given operand is of a given
    /// <c>EvaluateResult</c>-derived type.
    /// </summary>
    private sealed class TypeTest
    {
        private readonly SemanticModel semanticModel;
        private readonly ExpressionSyntax operand;
        private readonly ITypeSymbol targetType;

        // The EvaluateResultType member that corresponds to the target type: Success for
        // EvaluateResultSuccess, Exception for EvaluateResultException.
        private readonly string discriminatorValueName;

        public TypeTest(SemanticModel semanticModel, ExpressionSyntax operand, ITypeSymbol targetType)
        {
            this.semanticModel = semanticModel;
            this.operand = Unparenthesize(operand);
            this.targetType = targetType;
            this.discriminatorValueName = targetType.Name.Substring("EvaluateResult".Length);
        }

        /// <summary>
        /// Determines whether a condition being true establishes the operand's type.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns><see langword="true"/> if the condition establishes the type; otherwise <see langword="false"/>.</returns>
        public bool IsEstablishedBy(ExpressionSyntax condition)
        {
            return Unparenthesize(condition) switch
            {
                // Either conjunct being true is enough: both hold when the whole condition does.
                BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.LogicalAndExpression)
                    => this.IsEstablishedBy(binary.Left) || this.IsEstablishedBy(binary.Right),
                BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression)
                    => this.IsDiscriminatorComparison(binary.Left, binary.Right) || this.IsDiscriminatorComparison(binary.Right, binary.Left),
                // Two readings of one syntax: the parser gives `x is Name` and `x is Qualified.Name`
                // this shape whichever the name turns out to be, so the type test
                // (`result is EvaluateResultSuccess`) and the discriminator test
                // (`result.ResultType is EvaluateResultType.Success`, the pattern spelling of the
                // equality test above) both arrive here.
                BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.IsExpression)
                    => (this.IsOperand(binary.Left) && this.IsTargetType(binary.Right))
                        || this.IsDiscriminatorComparison(binary.Left, binary.Right),
                IsPatternExpressionSyntax isPattern
                    => this.IsOperand(isPattern.Expression) && this.IsTargetTypePattern(isPattern.Pattern),
                _ => false,
            };
        }

        /// <summary>
        /// Determines whether a condition being false establishes the operand's type — the shape of an
        /// early-exit guard, <c>if (result.ResultType != EvaluateResultType.Success) return;</c>.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns><see langword="true"/> if the condition's negation establishes the type; otherwise <see langword="false"/>.</returns>
        public bool IsNegatedBy(ExpressionSyntax condition)
        {
            return Unparenthesize(condition) switch
            {
                // When a disjunction is false every disjunct is false, so one of them ruling the
                // type out is enough.
                BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.LogicalOrExpression)
                    => this.IsNegatedBy(binary.Left) || this.IsNegatedBy(binary.Right),
                BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.NotEqualsExpression)
                    => this.IsDiscriminatorComparison(binary.Left, binary.Right) || this.IsDiscriminatorComparison(binary.Right, binary.Left),
                PrefixUnaryExpressionSyntax logicalNot when logicalNot.IsKind(SyntaxKind.LogicalNotExpression)
                    => this.IsEstablishedBy(logicalNot.Operand),

                // As in IsEstablishedBy, the negated pattern reads either as a type test
                // (`result is not EvaluateResultSuccess`) or as a discriminator test
                // (`result.ResultType is not EvaluateResultType.Success`); both are early-exit guards.
                IsPatternExpressionSyntax { Pattern: UnaryPatternSyntax notPattern } isPattern when notPattern.IsKind(SyntaxKind.NotPattern)
                    => (this.IsOperand(isPattern.Expression) && this.IsTargetTypePattern(notPattern.Pattern))
                        || (notPattern.Pattern is ConstantPatternSyntax constantPattern
                            && this.IsDiscriminatorComparison(isPattern.Expression, constantPattern.Expression)),
                _ => false,
            };
        }

        /// <summary>
        /// Determines whether every label of a switch section selects the operand's type, either by
        /// switching on the discriminator (<c>case EvaluateResultType.Success:</c>), by a type
        /// pattern on the operand itself (<c>case EvaluateResultSuccess:</c>), or, for the section's
        /// statements, by the label's <c>when</c> clause.
        /// </summary>
        /// <param name="section">The switch section.</param>
        /// <param name="child">The child of the section that contains the cast: a label or a statement.</param>
        /// <returns><see langword="true"/> if the cast runs only for the operand's type; otherwise <see langword="false"/>.</returns>
        public bool IsEstablishedBySwitchSection(SwitchSectionSyntax section, SyntaxNode child)
        {
            // A section always belongs to a switch statement and always carries at least one label.
            // `case EvaluateResultSuccess:` parses as a case label whose value is a name, not as a
            // pattern label; it is a type test when that name resolves to the target type. A when
            // clause runs only after its label's pattern has matched, and the statements only after
            // the clause holds, so the clause guards the statements but not a cast inside itself.
            ExpressionSyntax governing = ((SwitchStatementSyntax)section.Parent!).Expression;
            bool isInStatements = child is StatementSyntax;
            return section.Labels.All(label => label switch
            {
                CaseSwitchLabelSyntax caseLabel => this.IsDiscriminatorComparison(governing, caseLabel.Value)
                    || (this.IsOperand(governing) && this.IsTargetType(caseLabel.Value)),
                CasePatternSwitchLabelSyntax patternLabel => this.IsSelectedByPattern(governing, patternLabel.Pattern)
                    || (isInStatements && patternLabel.WhenClause is not null && this.IsEstablishedBy(patternLabel.WhenClause.Condition)),
                _ => false,
            });
        }

        /// <summary>
        /// Determines whether a switch expression arm is selected only for the operand's type: by its
        /// pattern, as a switch section label is, or, for the arm's expression, by its <c>when</c> clause.
        /// </summary>
        /// <param name="arm">The switch expression arm.</param>
        /// <param name="child">The child of the arm that contains the cast: its when clause or its expression.</param>
        /// <returns><see langword="true"/> if the cast runs only for the operand's type; otherwise <see langword="false"/>.</returns>
        public bool IsEstablishedBySwitchExpressionArm(SwitchExpressionArmSyntax arm, SyntaxNode child)
        {
            // An arm always belongs to a switch expression. As in a section, the when clause runs only
            // after the pattern has matched and the expression only after the clause holds, so the
            // clause guards the expression but not a cast inside itself.
            ExpressionSyntax governing = ((SwitchExpressionSyntax)arm.Parent!).GoverningExpression;
            return this.IsSelectedByPattern(governing, arm.Pattern)
                || (child == arm.Expression && arm.WhenClause is not null && this.IsEstablishedBy(arm.WhenClause.Condition));
        }

        /// <summary>
        /// Determines whether a statement in a block is preceded by an <c>if</c> that leaves the block
        /// whenever the operand is not of the target type.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="statement">The statement containing the cast.</param>
        /// <returns><see langword="true"/> if an early exit guards the statement; otherwise <see langword="false"/>.</returns>
        public bool IsEstablishedByEarlyExitBefore(BlockSyntax block, StatementSyntax statement)
        {
            return block.Statements
                .TakeWhile(preceding => preceding != statement)
                .Any(preceding => preceding is IfStatementSyntax { Else: null } guard
                    && AlwaysExits(guard.Statement)
                    && this.IsNegatedBy(guard.Condition));
        }

        private static ExpressionSyntax Unparenthesize(ExpressionSyntax expression)
        {
            ExpressionSyntax current = expression;
            while (current is ParenthesizedExpressionSyntax parenthesized)
            {
                current = parenthesized.Expression;
            }

            return current;
        }

        private static bool AlwaysExits(StatementSyntax statement)
        {
            return statement switch
            {
                ReturnStatementSyntax or ThrowStatementSyntax or BreakStatementSyntax or ContinueStatementSyntax => true,
                BlockSyntax block => block.Statements.Count > 0 && AlwaysExits(block.Statements[block.Statements.Count - 1]),
                _ => false,
            };
        }

        private bool IsOperand(ExpressionSyntax expression)
        {
            return SyntaxFactory.AreEquivalent(Unparenthesize(expression), this.operand);
        }

        private bool IsSelectedByPattern(ExpressionSyntax governing, PatternSyntax pattern)
        {
            // The discriminator matched against the member for the target type
            // (`EvaluateResultType.Success`), or the operand matched against the target type
            // (`EvaluateResultSuccess`, `EvaluateResultSuccess success`, `EvaluateResultSuccess { }`).
            return (pattern is ConstantPatternSyntax constant && this.IsDiscriminatorComparison(governing, constant.Expression))
                || (this.IsOperand(governing) && this.IsTargetTypePattern(pattern));
        }

        private bool IsTargetType(ExpressionSyntax type)
        {
            return SymbolEqualityComparer.Default.Equals(this.semanticModel.GetTypeInfo(type).Type, this.targetType);
        }

        private bool IsTargetTypePattern(PatternSyntax pattern)
        {
            // `is EvaluateResultSuccess success`, `is EvaluateResultSuccess { ... }`, and the bare name
            // in `is not EvaluateResultSuccess` or `case EvaluateResultSuccess when ...:`, which the
            // parser reads as a constant pattern and the compiler binds as a type.
            ExpressionSyntax? type = pattern switch
            {
                DeclarationPatternSyntax declarationPattern => declarationPattern.Type,
                RecursivePatternSyntax recursivePattern => recursivePattern.Type,
                ConstantPatternSyntax constantPattern => constantPattern.Expression,
                _ => null,
            };

            return type is not null && this.IsTargetType(type);
        }

        private bool IsDiscriminatorComparison(ExpressionSyntax discriminator, ExpressionSyntax value)
        {
            // result.ResultType compared with the EvaluateResultType member for the target type.
            return Unparenthesize(discriminator) is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "ResultType" } access
                && this.IsOperand(access.Expression)
                && this.semanticModel.GetSymbolInfo(value).Symbol is IFieldSymbol field
                && field.Name == this.discriminatorValueName
                && AnalyzerSymbolHelpers.IsLibraryTypeNamed(field.ContainingType, "EvaluateResultType");
        }
    }
}
