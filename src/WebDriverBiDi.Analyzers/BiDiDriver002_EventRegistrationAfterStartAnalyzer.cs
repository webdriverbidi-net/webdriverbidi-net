// <copyright file="BiDiDriver002_EventRegistrationAfterStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects RegisterEvent() calls after StartAsync(). AddObserver() calls are
/// deliberately not reported: observers may be added at any time, including while the
/// driver is running.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver002_EventRegistrationAfterStartAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI002";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Event registration after StartAsync";

    private static readonly LocalizableString MessageFormat = "Event '{0}' is registered after calling StartAsync. Events must be registered before the driver starts.";

    private static readonly LocalizableString Description = "Events must be registered before calling StartAsync to ensure proper event handling initialization.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi002");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        // Adding an observer to an ObservableEvent<T> (AddObserver) is deliberately not reported:
        // observers may be added or removed at any time, including while the driver is running. Only
        // the registration of custom protocol events (RegisterEvent) is locked once the driver has
        // started, and that is the only call the runtime rejects.
        DriverStartStateWalker.Walk(context, AnalyzerSymbolHelpers.IsCommandExecutorType, (invocation, method, driverVariableName, isStarted) =>
        {
            if (method.Name == "RegisterEvent" && isStarted)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), GetEventName(context, invocation)));
            }
        });
    }

    private static string GetEventName(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        // RegisterEvent's first argument is the event name; report it in the message rather than the
        // literal method name. The call has already resolved to RegisterEvent(string, Func<...>), so
        // the argument is present.
        ExpressionSyntax firstArgument = invocation.ArgumentList.Arguments[0].Expression;
        if (context.SemanticModel.GetConstantValue(firstArgument) is { HasValue: true, Value: string eventName })
        {
            return eventName;
        }

        // A non-constant event name (for example a variable) cannot be resolved at compile time, so
        // fall back to the argument's source text.
        return firstArgument.ToString();
    }
}
