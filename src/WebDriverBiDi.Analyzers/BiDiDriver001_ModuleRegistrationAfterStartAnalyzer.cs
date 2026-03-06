// <copyright file="BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/core-concepts.html#timing-restrictions");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for method body analysis
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        // Track BiDiDriver variables and their states
        Dictionary<string, DriverState> driverVariables = [];

        // Walk through all statements in the method
        foreach (StatementSyntax statement in methodDeclaration.DescendantNodes().OfType<StatementSyntax>())
        {
            // Check for driver creation: var driver = new BiDiDriver(...)
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                AnalyzeLocalDeclaration(localDecl, semanticModel, driverVariables);
            }

            // Check for await driver.StartAsync(...) or driver.RegisterModule(...)
            if (statement is ExpressionStatementSyntax expressionStmt)
            {
                AnalyzeExpressionStatement(expressionStmt, context, semanticModel, driverVariables);
            }
        }
    }

    private static void AnalyzeLocalDeclaration(LocalDeclarationStatementSyntax localDecl, SemanticModel semanticModel, Dictionary<string, DriverState> driverVariables)
    {
        foreach (var variable in localDecl.Declaration.Variables)
        {
            if (variable.Initializer == null)
            {
                continue;
            }

            var typeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value);
            if (IsBiDiDriverType(typeInfo.Type))
            {
                driverVariables[variable.Identifier.Text] = new DriverState
                {
                    IsStarted = false,
                };
            }
        }
    }

    private static void AnalyzeExpressionStatement(ExpressionStatementSyntax expressionStmt, SyntaxNodeAnalysisContext context, SemanticModel semanticModel, Dictionary<string, DriverState> driverVariables)
    {
        if (expressionStmt.Expression is AwaitExpressionSyntax awaitExpr && awaitExpr.Expression is InvocationExpressionSyntax invocation)
        {
            // Handle: await driver.StartAsync(...)
            CheckForDriverMethodCall(invocation, context, semanticModel, driverVariables);
        }
        else if (expressionStmt.Expression is InvocationExpressionSyntax directInvocation)
        {
            // Handle: driver.StartAsync(...).Wait() or driver.RegisterModule(...)
            CheckForDriverMethodCall(directInvocation, context, semanticModel, driverVariables);
        }
    }

    private static void CheckForDriverMethodCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, SemanticModel semanticModel, Dictionary<string, DriverState> driverVariables)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        string? driverVariableName = GetDriverVariableName(memberAccess.Expression);
        if (driverVariableName == null || !driverVariables.ContainsKey(driverVariableName))
        {
            return;
        }

        IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        string methodName = methodSymbol.Name;

        // Check if this is StartAsync() being called
        if (methodName == "StartAsync" && IsBiDiDriverType(methodSymbol.ContainingType))
        {
            driverVariables[driverVariableName].IsStarted = true;
        }

        // Check if this is RegisterModule() being called AFTER StartAsync()
        if (methodName == "RegisterModule" && IsBiDiDriverType(methodSymbol.ContainingType) && driverVariables[driverVariableName].IsStarted)
        {
            // Get module parameter for better error message
            string moduleName = "module";
            if (invocation.ArgumentList.Arguments.Count > 0)
            {
                ExpressionSyntax arg = invocation.ArgumentList.Arguments[0].Expression;
                moduleName = arg.ToString();
            }

            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), moduleName);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string? GetDriverVariableName(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax member => GetDriverVariableName(member.Expression),
            _ => null,
        };
    }

    private static bool IsBiDiDriverType(ITypeSymbol? type)
    {
        if (type == null)
        {
            return false;
        }

        // Check if type is BiDiDriver or IBiDiDriver
        return type.Name == "BiDiDriver" || type.Name == "IBiDiDriver";
    }

    private class DriverState
    {
        public bool IsStarted { get; set; }
    }
}
