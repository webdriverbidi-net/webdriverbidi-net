// <copyright file="BiDiDriver025_AsyncVoidEventHandlerAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects an <c>async void</c> handler passed to <c>AddObserver</c>. Such a handler
/// binds to the <c>Action&lt;T&gt;</c> overload rather than <c>Func&lt;T, Task&gt;</c>, so it runs
/// fire-and-forget: exceptions thrown after the first <c>await</c> are unobserved, and the observer is
/// considered complete before the handler's asynchronous work finishes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver025_AsyncVoidEventHandlerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI025";

    private const string Category = "Reliability";

    private static readonly LocalizableString Title = "async void handler passed to AddObserver";

    private static readonly LocalizableString MessageFormat = "The event handler '{0}' is an 'async void' method. Passed to AddObserver it binds to the Action<T> overload and runs fire-and-forget: exceptions thrown after its first await are unobserved and can crash the process, and the observer is considered complete before the handler's asynchronous work finishes. Use an 'async Task' handler, which binds to the Func<T, Task> overload.";

    private static readonly LocalizableString Description = "An 'async void' method passed to AddObserver binds to the Action<T> overload, not Func<T, Task>. The library sees the handler complete at its first 'await', so capture sessions and RunHandlerAsynchronously do not track the remaining work, and any exception thrown after the first 'await' is an unobserved async-void fault that can terminate the process. Declare the handler as 'async Task' so it binds to the Func<T, Task> overload and its work is awaited and its faults observed.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi025");

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

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name.Identifier.Text != "AddObserver")
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        // Only the library's AddObserver, which returns EventObserver<T>, is of interest; a same-named
        // method on an unrelated type is not.
        if (methodSymbol.ReturnType is not INamedTypeSymbol { Name: "EventObserver" })
        {
            return;
        }

        ArgumentSyntax? handlerArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (handlerArgument == null)
        {
            return;
        }

        // An 'async void' handler resolves to an IMethodSymbol that is async and returns void: it bound
        // to the Action<T> overload. An 'async Task' lambda or method group binds to Func<T, Task>
        // (ReturnsVoid is false) and is correct, so it is not reported.
        if (context.SemanticModel.GetSymbolInfo(handlerArgument.Expression).Symbol is IMethodSymbol { IsAsync: true, ReturnsVoid: true } handlerMethod)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, handlerArgument.Expression.GetLocation(), handlerMethod.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
