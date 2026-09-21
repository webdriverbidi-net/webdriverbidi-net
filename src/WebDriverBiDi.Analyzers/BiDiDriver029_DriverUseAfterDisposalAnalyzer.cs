// <copyright file="BiDiDriver029_DriverUseAfterDisposalAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects a <c>BiDiDriver</c> being used after it has been disposed.
/// </summary>
/// <remarks>
/// Disposal is terminal: <c>BiDiDriver.DisposeAsync</c> marks the driver disposed before it tears
/// anything down, and every member that guards on that state throws
/// <see cref="System.ObjectDisposedException"/> from then on. A disposed driver cannot be restarted, so
/// the only remedy is a new driver.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver029_DriverUseAfterDisposalAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI029";

    private const string Category = "Usage";

    /// <summary>
    /// The members of <c>BiDiDriver</c> that throw once the driver is disposed. Every one of them
    /// begins with a disposal guard; the members deliberately absent from this set do not.
    /// <c>StopAsync</c> is a safe no-op on a disposed driver (its teardown finds the transport already
    /// disconnected), and <c>DisposeAsync</c> is idempotent, so neither is reported.
    /// </summary>
    private static readonly HashSet<string> DisposalGuardedDriverMethods =
    [
        "StartAsync",
        "ExecuteCommandAsync",
        "RegisterEvent",
        "RegisterModule",
        "GetModule",
        "RegisterTypeInfoResolverAsync",
    ];

    private static readonly LocalizableString Title = "Driver used after disposal";

    private static readonly LocalizableString MessageFormat = "'{0}' is called on '{1}' after it has been disposed. A disposed BiDiDriver cannot be reused; create a new one instead.";

    private static readonly LocalizableString Description = "A BiDiDriver marks itself disposed before releasing anything, and its command execution, registration, and start members throw ObjectDisposedException from that point on. Disposal is terminal: the driver cannot be restarted, so code that needs a connection after disposing must construct a new driver.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi029");


    private static readonly DisposalRules DriverRules = new(
        AnalyzerSymbolHelpers.IsCommandExecutorType,
        GetDriverVariableName,
        ["DisposeAsync"],
        ThrowsAfterDisposal);

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
        DisposalStateWalker.Walk(
            context,
            DriverRules,
            (invocation, method, driverVariableName) =>
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), method.Name, driverVariableName)));
    }


    private static string? GetDriverVariableName(InvocationExpressionSyntax invocation, SyntaxNode body, SemanticModel semanticModel, out bool isDirectCall)
    {
        isDirectCall = false;

        // Direct call on the driver (driver.ExecuteCommandAsync(...)), or a call on one of its modules
        // (driver.BrowsingContext.NavigateAsync(...)), which reaches the driver's disposal guard through
        // ExecuteCommandAsync; either may be made through a wrapped receiver (driver!.DisposeAsync(),
        // driver?.BrowsingContext.NavigateAsync(...)).
        IdentifierNameSyntax? receiver = AnalyzerSymbolHelpers.GetMemberChainRoot(invocation.Expression, out int memberDepth);
        if (receiver is null || memberDepth > 2)
        {
            return null;
        }

        // A receiver that is not the driver itself may be a local holding one of its modules:
        // context.GetTreeAsync(), where the local was bound to driver.BrowsingContext.
        if (!AnalyzerSymbolHelpers.IsCommandExecutorType(semanticModel.GetTypeInfo(receiver).Type))
        {
            return AnalyzerSymbolHelpers.GetDriverOfModuleAlias(receiver, body, semanticModel);
        }

        isDirectCall = memberDepth == 1;
        return receiver.Identifier.ValueText;
    }

    /// <summary>
    /// Determines whether a method throws once the driver it belongs to is disposed.
    /// </summary>
    /// <param name="method">The resolved method.</param>
    /// <returns><see langword="true"/> if the method is guarded by the driver's disposal check; otherwise <see langword="false"/>.</returns>
    private static bool ThrowsAfterDisposal(IMethodSymbol method)
    {
        if (AnalyzerSymbolHelpers.IsCommandExecutorType(method.ContainingType))
        {
            return DisposalGuardedDriverMethods.Contains(method.Name);
        }

        // A module command reaches the driver's disposal guard through ExecuteCommandAsync, so it throws
        // exactly as a direct command would.
        return AnalyzerSymbolHelpers.IsLibraryModuleType(method.ContainingType) && IsModuleCommandMethod(method);
    }

    private static bool IsModuleCommandMethod(IMethodSymbol method)
    {
        return method.Name.EndsWith("Async", System.StringComparison.Ordinal)
            && method.ReturnType is INamedTypeSymbol { Name: "Task", IsGenericType: true } returnType
            && returnType.TypeArguments.Length == 1
            && InheritsFromCommandResult(returnType.TypeArguments[0]);
    }

    private static bool InheritsFromCommandResult(ITypeSymbol type)
    {
        for (INamedTypeSymbol? current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
        {
            if (AnalyzerSymbolHelpers.IsLibraryTypeNamed(current, "CommandResult"))
            {
                return true;
            }
        }

        return false;
    }
}
