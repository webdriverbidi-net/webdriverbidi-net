// <copyright file="BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that warns when known long-running operations are called without a CancellationToken.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI013";

    private const string Category = "Reliability";

    private static readonly LocalizableString Title = "Long-running operation should use CancellationToken";

    private static readonly LocalizableString MessageFormat = "Method '{0}' is a long-running operation and should be called with a CancellationToken parameter";

    private static readonly LocalizableString Description = "Long-running operations like navigation, printing, and checkpoint waits should always accept a CancellationToken to allow graceful cancellation and prevent hangs. This is especially important for operations that can timeout or take an indeterminate amount of time.";

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

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;
        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        if (methodSymbol == null)
        {
            return;
        }

        // Check if this is a BiDiDriver or module method or EventObserver
        if (!IsTargetType(methodSymbol.ContainingType))
        {
            return;
        }

        // Check if this is one of the known long-running methods
        if (!IsLongRunningMethod(methodSymbol))
        {
            return;
        }

        // Check if this method call is passing a CancellationToken
        bool hasExplicitToken = invocation.ArgumentList.Arguments.Any(arg =>
        {
            ITypeSymbol? argType = context.SemanticModel.GetTypeInfo(arg.Expression).Type;
            return argType?.Name == "CancellationToken";
        });

        // If already passing a token, no warning needed
        if (hasExplicitToken)
        {
            return;
        }

        // Check if an overload exists that accepts CancellationToken
        INamedTypeSymbol containingType = methodSymbol.ContainingType;
        System.Collections.Generic.IEnumerable<IMethodSymbol> overloads = containingType.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>();

        bool hasTokenOverload = overloads.Any(overload => overload.Parameters.Any(p => p.Type.Name == "CancellationToken"));

        if (hasTokenOverload)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsTargetType(INamedTypeSymbol? type)
    {
        if (type == null)
        {
            return false;
        }

        // Check for BiDiDriver, modules, or EventObserver<T>
        return type.Name == "BiDiDriver"
            || type.Name == "IBiDiDriver"
            || type.Name.EndsWith("Module")
            || (type.Name == "EventObserver" && type.IsGenericType);
    }

    private static bool IsLongRunningMethod(IMethodSymbol method)
    {
        // These operations are known to be potentially long-running
        string[] longRunningOperations =
        [
            "NavigateAsync",
            "ReloadAsync",
            "PrintAsync",
            "StartAsync",
            "WaitForCheckpointAsync",
            "WaitForCheckpointAndTasksAsync",
        ];

        return longRunningOperations.Contains(method.Name);
    }
}
