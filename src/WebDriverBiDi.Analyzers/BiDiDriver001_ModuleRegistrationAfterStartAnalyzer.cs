// <copyright file="BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects when RegisterModule() is called after StartAsync() on a BiDiDriver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver001_ModuleRegistrationAfterStartAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI001";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Module registration after driver start";

    private static readonly LocalizableString MessageFormat = "RegisterModule() cannot be called after StartAsync(). Module '{0}' should be registered before calling StartAsync().";

    private static readonly LocalizableString Description = "Modules must be registered before calling StartAsync() on the BiDiDriver. Attempting to register modules after the driver has started will throw an InvalidOperationException at runtime.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi001");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for method, constructor, and top-level-program body analysis.
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, AnalyzerSymbolHelpers.ExecutableBodyKinds);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        // The call must be on the driver itself: a custom module reached through the driver may
        // declare a method of the same name, and calling that is not a registration on the driver.
        DriverStartStateWalker.Walk(context, AnalyzerSymbolHelpers.IsDriverConfigurationType, (invocation, method, driverVariableName, isStarted, isDirectDriverCall) =>
        {
            if (method.Name != "RegisterModule" || !isStarted || !isDirectDriverCall
                || !AnalyzerSymbolHelpers.IsDriverConfigurationType(method.ContainingType))
            {
                return;
            }

            // Name the module in the message. RegisterModule(Module) always has its one argument
            // when it binds, but the call is reported from source that may still be being typed.
            string moduleName = invocation.ArgumentList.Arguments.Count > 0
                ? invocation.ArgumentList.Arguments[0].Expression.ToString()
                : "module";
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), moduleName));
        });
    }
}
