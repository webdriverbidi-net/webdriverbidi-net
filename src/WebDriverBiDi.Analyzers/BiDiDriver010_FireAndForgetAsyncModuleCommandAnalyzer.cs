// <copyright file="BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Analyzer that detects fire-and-forget async calls to module command methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI010";

    private const string Category = "Reliability";

    private static readonly LocalizableString Title = "Async module command should be awaited";

    private static readonly LocalizableString MessageFormat = "Async method '{0}' should be awaited, assigned to a variable, or passed as an argument to avoid fire-and-forget behavior";

    private static readonly LocalizableString Description = "Fire-and-forget async calls to module commands can lead to unhandled exceptions, race conditions, and commands that never execute. Always await async operations or explicitly capture the Task for later handling.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        IInvocationOperation invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;

        // Check if this is a module method
        if (!IsModuleType(method.ContainingType))
        {
            return;
        }

        // Check if the method returns Task<T>
        if (!IsTaskReturningMethod(method))
        {
            return;
        }

        // A call is fire-and-forget only when its result is discarded. Rather than trying to
        // enumerate every way a result can be consumed (await, assignment, argument, array/collection
        // element, conditional, and so on), detect the single discard shape: the call stands alone as
        // an expression statement.
        if (!IsResultDiscarded(invocation))
        {
            return;
        }

        // Report diagnostic for fire-and-forget call
        Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsModuleType(INamedTypeSymbol? type)
    {
        return type != null && type.Name.EndsWith("Module", System.StringComparison.Ordinal) && HasModuleBaseClass(type);
    }

    private static bool HasModuleBaseClass(INamedTypeSymbol type)
    {
        INamedTypeSymbol? currentType = type.BaseType;
        while (currentType != null)
        {
            if (currentType.Name == "Module")
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    private static bool IsTaskReturningMethod(IMethodSymbol method)
    {
        return method.ReturnType is INamedTypeSymbol namedType && namedType.Name == "Task" && namedType.IsGenericType;
    }

    private static bool IsResultDiscarded(IOperation operation)
    {
        IOperation? parent = operation.Parent;

        // Follow chained member calls (for example .ConfigureAwait(false)) and conversions to the
        // outermost Task-valued expression; the chain is fire-and-forget only if that outer value is
        // itself discarded.
        if (parent is IInvocationOperation or IConversionOperation)
        {
            return IsResultDiscarded(parent);
        }

        // The result is discarded when the (outer) expression stands alone as a statement. Every other
        // context — await, assignment, argument, array/collection element, conditional, return —
        // consumes the value.
        return parent is IExpressionStatementOperation;
    }
}
