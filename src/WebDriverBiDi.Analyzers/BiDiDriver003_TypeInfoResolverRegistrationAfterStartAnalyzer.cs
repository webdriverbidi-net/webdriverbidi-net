// <copyright file="BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects RegisterTypeInfoResolverAsync() calls after StartAsync().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI003";

    private const string Category = "Usage";

    private static readonly LocalizableString Title =
        "Type info resolver registration after StartAsync";

    private static readonly LocalizableString MessageFormat = "RegisterTypeInfoResolverAsync is called after calling StartAsync. Type info resolvers must be registered before the driver starts.";

    private static readonly LocalizableString Description = "Type info resolvers must be registered before calling StartAsync to ensure proper JSON serialization configuration.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi003");

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
        // Only a call to RegisterTypeInfoResolverAsync is ever reported, and the walk binds every declaration it sees.
        if (!AnalyzerSymbolHelpers.ContainsIdentifier(context.Node, "RegisterTypeInfoResolverAsync"))
        {
            return;
        }

        // The call must be on the driver itself: a custom module reached through the driver may
        // declare a method of the same name, and calling that is not a registration on the driver.
        DriverStartStateWalker.Walk(context, AnalyzerSymbolHelpers.IsDriverConfigurationType, (invocation, method, driverVariableName, isStarted, isDirectDriverCall) =>
        {
            if (method.Name == "RegisterTypeInfoResolverAsync" && isStarted && isDirectDriverCall
                && AnalyzerSymbolHelpers.IsDriverConfigurationType(method.ContainingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
            }
        });
    }
}
