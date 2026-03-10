// <copyright file="BiDiDriver004_CancellationTokenSuggestionAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that suggests passing CancellationToken to methods that support it.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver004_CancellationTokenSuggestionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI004";

    private const string Category = "Design";

    private static readonly LocalizableString Title = "Consider passing CancellationToken";

    private static readonly LocalizableString MessageFormat = "Method '{0}' supports cancellation; consider passing a CancellationToken parameter";

    private static readonly LocalizableString Description = "Methods that support CancellationToken allow graceful cancellation of long-running operations. This improves responsiveness and resource cleanup in your application.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
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

        // Check if this is a BiDiDriver or module method
        if (!IsBiDiDriverOrModuleType(methodSymbol.ContainingType))
        {
            return;
        }

        // Check if this method call is passing a CancellationToken
        bool hasExplicitToken = invocation.ArgumentList.Arguments.Any(arg =>
        {
            var argType = context.SemanticModel.GetTypeInfo(arg.Expression).Type;
            return argType?.Name == "CancellationToken";
        });

        // If already passing a token, no suggestion needed
        if (hasExplicitToken)
        {
            return;
        }

        // Check if an overload exists that accepts CancellationToken
        INamedTypeSymbol containingType = methodSymbol.ContainingType;
        var overloads = containingType.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>();

        bool hasTokenOverload = overloads.Any(overload => overload.Parameters.Any(p => p.Type.Name == "CancellationToken"));

        if (hasTokenOverload && ShouldSuggestToken(methodSymbol))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsBiDiDriverOrModuleType(INamedTypeSymbol? type)
    {
        if (type == null)
        {
            return false;
        }

        return AnalyzerSymbolHelpers.IsCommandExecutorType(type) || type.Name.EndsWith("Module");
    }

    private static bool ShouldSuggestToken(IMethodSymbol method)
    {
        // Suggest for potentially long-running operations
        string[] longRunningMethods =
        [
            "NavigateAsync",
            "ExecuteCommandAsync",
            "EvaluateAsync",
            "CallFunctionAsync",
            "GetTreeAsync",
            "LocateNodesAsync",
            "StartAsync",
        ];

        return longRunningMethods.Contains(method.Name);
    }
}
