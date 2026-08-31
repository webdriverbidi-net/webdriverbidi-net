// <copyright file="BiDiDriver024_DuplicateStartAsyncAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
        SemanticModel semanticModel = context.SemanticModel;

        // Track BiDiDriver variables and whether StartAsync is currently in effect for each.
        Dictionary<string, bool> driverStartedStatus = [];

        foreach (StatementSyntax statement in AnalyzerSymbolHelpers.GetTopLevelStatements(context.Node))
        {
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                TrackDriverDeclarations(localDecl, semanticModel, driverStartedStatus);
            }

            AnalyzeStatementForStartStopCalls(statement, context, driverStartedStatus);
        }
    }

    private static void TrackDriverDeclarations(
        LocalDeclarationStatementSyntax localDecl,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus)
    {
        foreach (VariableDeclaratorSyntax variable in localDecl.Declaration.Variables)
        {
            if (variable.Initializer == null)
            {
                continue;
            }

            ITypeSymbol? typeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value).Type;
            if (AnalyzerSymbolHelpers.IsCommandExecutorType(typeInfo))
            {
                driverStartedStatus[variable.Identifier.Text] = false;
            }
        }
    }

    private static void AnalyzeStatementForStartStopCalls(
        StatementSyntax statement,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> driverStartedStatus)
    {
        // Do not descend into the bodies of nested functions (lambdas, anonymous methods, local
        // functions): their code runs when the delegate is invoked, not at the textual position where
        // it is declared, so a StartAsync there must not be judged against the driver's started state
        // at this point in the method.
        IEnumerable<InvocationExpressionSyntax> invocations = statement
            .DescendantNodes(descendIntoChildren: node => node is not (
                SimpleLambdaExpressionSyntax or
                ParenthesizedLambdaExpressionSyntax or
                AnonymousMethodExpressionSyntax or
                LocalFunctionStatementSyntax))
            .OfType<InvocationExpressionSyntax>();

        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            CheckInvocation(invocation, context, driverStartedStatus);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        Dictionary<string, bool> driverStartedStatus)
    {
        // Only direct calls on a tracked driver variable (driver.StartAsync()/driver.StopAsync())
        // affect the started state; the receiver was type-checked when the variable was declared.
        if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } memberAccess)
        {
            return;
        }

        string driverVariableName = identifier.Identifier.Text;
        if (!driverStartedStatus.TryGetValue(driverVariableName, out bool started))
        {
            return;
        }

        string methodName = memberAccess.Name.Identifier.Text;
        if (methodName == "StartAsync")
        {
            if (started)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
            }
            else
            {
                driverStartedStatus[driverVariableName] = true;
            }
        }
        else if (methodName == "StopAsync")
        {
            driverStartedStatus[driverVariableName] = false;
        }
    }
}
