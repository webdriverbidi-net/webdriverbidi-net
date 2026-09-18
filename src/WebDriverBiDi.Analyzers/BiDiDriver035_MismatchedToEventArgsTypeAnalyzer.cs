// <copyright file="BiDiDriver035_MismatchedToEventArgsTypeAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects a call to the parameterless <c>EventInfo&lt;T&gt;.ToEventArgs&lt;TEventArgs&gt;()</c> whose
/// <c>TEventArgs</c> is not <c>T</c>. That overload uses the event data itself as the event args, and throws a
/// <c>WebDriverBiDiException</c> at runtime for any other type, a base or derived type included.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver035_MismatchedToEventArgsTypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI035";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "ToEventArgs requested for a type other than the event's own";

    private static readonly LocalizableString MessageFormat = "ToEventArgs<{0}>() on an EventInfo<{1}> always throws a WebDriverBiDiException: the parameterless overload returns the event data itself, which is a {1}. Call ToEventArgs<{1}>(), or pass a factory that builds a {0} from the event data.";

    private static readonly LocalizableString Description = "The parameterless EventInfo<T>.ToEventArgs<TEventArgs>() hands back the deserialized event data as the event args, so it succeeds only when TEventArgs is exactly T; for any other type, including a base or derived type, it throws a WebDriverBiDiException. Use ToEventArgs<T>(), or the overload that takes a factory from T to TEventArgs, which is also the one to use in AOT or trimmed applications.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi035");

    // Hoisted so the filter below, which runs for every argument-less invocation in the compilation, does not
    // allocate the array each time.
    private static readonly string[] ToEventArgsMethodName = ["ToEventArgs"];

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

        // Only a call with no arguments can bind to the overload that throws; a cheap filter before the bind.
        if (invocation.ArgumentList.Arguments.Count != 0
            || !AnalyzerSymbolHelpers.CouldInvokeAnyOf(invocation, ToEventArgsMethodName))
        {
            return;
        }

        // The library's EventInfo<T> declares ToEventArgs twice, each with one type parameter, and the overload a call
        // with no arguments binds to is the parameterless one. A call whose name was not evident to the filter above (a
        // delegate's Invoke) is declared elsewhere, so the containing type alone identifies the overload.
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method
            || !AnalyzerSymbolHelpers.IsLibraryTypeNamed(method.ContainingType, "EventInfo"))
        {
            return;
        }

        ITypeSymbol eventDataType = method.ContainingType.TypeArguments[0];
        ITypeSymbol requestedType = method.TypeArguments[0];

        // A type parameter may be bound to the same type at the call site that closes it, which this call cannot see.
        if (eventDataType is ITypeParameterSymbol
            || requestedType is ITypeParameterSymbol
            || SymbolEqualityComparer.Default.Equals(eventDataType, requestedType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            invocation.GetLocation(),
            requestedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            eventDataType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }
}
