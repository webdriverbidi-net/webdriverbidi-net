// <copyright file="BiDiDriver004_CancellationTokenSuggestionAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Generic;
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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi004");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        // Rule out the overwhelming majority of invocations on their name alone, before paying for
        // the semantic model. Every command's name ends with Async; ShouldSuggestToken below remains
        // the authoritative test against the resolved symbol.
        if (!AnalyzerSymbolHelpers.CouldInvokeNameEndingWith(invocation, "Async"))
        {
            return;
        }

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
            ITypeSymbol? argType = context.SemanticModel.GetTypeInfo(arg.Expression).Type;
            return argType?.Name == "CancellationToken";
        });

        // If already passing a token, no suggestion needed
        if (hasExplicitToken)
        {
            return;
        }

        // Check if an overload exists that accepts CancellationToken
        INamedTypeSymbol containingType = methodSymbol.ContainingType;
        IEnumerable<IMethodSymbol> overloads = containingType.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>();

        bool hasTokenOverload = overloads.Any(overload => overload.Parameters.Any(p => p.Type.Name == "CancellationToken"));

        if (hasTokenOverload && ShouldSuggestToken(methodSymbol))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsBiDiDriverOrModuleType(INamedTypeSymbol? type)
    {
        return AnalyzerSymbolHelpers.IsCommandExecutorType(type) || AnalyzerSymbolHelpers.IsLibraryModuleType(type);
    }

    /// <summary>
    /// The operations BIDI013 reports at Warning severity for the same omission. Suggesting a token
    /// here as well would produce two diagnostics for one call.
    /// </summary>
    private static readonly string[] LongRunningOperations =
    [
        "NavigateAsync",
        "PrintAsync",
        "ReloadAsync",
        "StartAsync",
        "WaitForCapturedTasksAsync",
        "WaitForCapturedTasksCompleteAsync",
    ];

    /// <summary>
    /// Determines whether a call is one this rule suggests a token for.
    /// </summary>
    /// <param name="method">The method the invocation binds to.</param>
    /// <returns><see langword="true"/> if the call should carry a token.</returns>
    /// <remarks>
    /// Every method of a module is judged, rather than a list of names that left one command suggested
    /// and the next one not; the caller pays for a token overload only where the method declares one,
    /// which the caller has already established. The driver's own <c>ExecuteCommandAsync</c> sends a
    /// command over the same connection and is judged with them, while its lifecycle members --
    /// <c>StopAsync</c>, <c>RegisterTypeInfoResolverAsync</c> -- are not commands and are left alone.
    /// </remarks>
    private static bool ShouldSuggestToken(IMethodSymbol method)
    {
        // A delegate's Invoke is not a command, however module-shaped the delegate type is.
        return method.MethodKind == MethodKind.Ordinary
            && !LongRunningOperations.Contains(method.Name)
            && (AnalyzerSymbolHelpers.IsLibraryModuleType(method.ContainingType) || method.Name == "ExecuteCommandAsync");
    }
}
