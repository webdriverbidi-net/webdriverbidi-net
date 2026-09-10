// <copyright file="BiDiDriver024_DuplicateStartAsyncAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects a second <c>StartAsync</c> call on a <c>BiDiDriver</c> with no intervening
/// <c>StopAsync</c>. The transport is already connected at that point, so the call throws a
/// <c>WebDriverBiDiConnectionException</c> at runtime.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver024_DuplicateStartAsyncAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI024";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "StartAsync called on an already-started BiDiDriver";

    private static readonly LocalizableString MessageFormat = "StartAsync() has already been called on this BiDiDriver without a subsequent StopAsync(). Calling StartAsync() again throws because the transport is already connected. Call StopAsync() before starting again.";

    private static readonly LocalizableString Description = "A BiDiDriver may only be started once at a time. Calling StartAsync() while the driver is already started throws a WebDriverBiDiConnectionException because the underlying transport is already connected to a remote end. To reconnect, call StopAsync() first and then StartAsync() again.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi024");

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
        // The walker reports each StartAsync with the state before the call takes effect, so a
        // StartAsync that finds the driver already started on every path is the duplicate.
        DriverStartStateWalker.Walk(context, AnalyzerSymbolHelpers.IsCommandExecutorType, (invocation, method, driverVariableName, isStarted, isDirectDriverCall) =>
        {
            // The call must be on the driver itself: a module reached through the driver may declare
            // a command of the same name, and calling that is not a second start of the driver.
            if (method.Name == "StartAsync" && isStarted && isDirectDriverCall)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
            }
        });
    }
}
