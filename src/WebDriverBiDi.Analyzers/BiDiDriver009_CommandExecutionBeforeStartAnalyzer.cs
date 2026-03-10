// <copyright file="BiDiDriver009_CommandExecutionBeforeStartAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that detects when commands are executed before StartAsync is called on a BiDiDriver.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver009_CommandExecutionBeforeStartAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI009";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Commands executed before StartAsync";

    private static readonly LocalizableString MessageFormat = "Method '{0}' cannot be called before StartAsync() on the BiDiDriver. Call StartAsync() first to establish the connection.";

    private static readonly LocalizableString Description = "Commands cannot be executed before calling StartAsync() on the BiDiDriver. StartAsync() establishes the connection to the remote end, and all commands require an active connection. Attempting to execute commands before the driver has started will fail.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        // Track BiDiDriver variables and whether StartAsync has been called
        Dictionary<string, bool> driverStartedStatus = [];

        // Walk through all statements in the method
        IEnumerable<StatementSyntax>? statements = methodDeclaration.Body?.Statements ?? Enumerable.Empty<StatementSyntax>();

        foreach (StatementSyntax statement in statements)
        {
            // Check for driver creation
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                AnalyzeLocalDeclaration(localDecl, semanticModel, driverStartedStatus);
            }

            // Check for method calls on driver
            AnalyzeStatementForDriverMethodCalls(statement, context, semanticModel, driverStartedStatus);
        }
    }

    private static void AnalyzeLocalDeclaration(
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

    private static void AnalyzeStatementForDriverMethodCalls(
        StatementSyntax statement,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus)
    {
        // Find all invocations in this statement
        IEnumerable<InvocationExpressionSyntax>? invocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            CheckInvocation(invocation, context, semanticModel, driverStartedStatus);
        }
    }

    private static void CheckInvocation(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        SemanticModel semanticModel,
        Dictionary<string, bool> driverStartedStatus)
    {
        IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        string methodName = methodSymbol.Name;

        // Get the driver variable name if this is a method call on a driver or module
        string? driverVariableName = GetDriverVariableNameFromInvocation(invocation, semanticModel);
        if (driverVariableName == null || !driverStartedStatus.ContainsKey(driverVariableName))
        {
            return;
        }

        // If this is StartAsync, mark the driver as started
        if (methodName == "StartAsync" && AnalyzerSymbolHelpers.IsCommandExecutorType(methodSymbol.ContainingType))
        {
            driverStartedStatus[driverVariableName] = true;
            return;
        }

        // If the driver hasn't been started yet, check if this is a command that requires a connection
        if (!driverStartedStatus[driverVariableName] && IsCommandMethod(methodSymbol))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string? GetDriverVariableNameFromInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Direct call on driver: driver.ExecuteCommandAsync(...)
            if (memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                ITypeSymbol? type = semanticModel.GetTypeInfo(identifier).Type;
                if (AnalyzerSymbolHelpers.IsCommandExecutorType(type))
                {
                    return identifier.Identifier.Text;
                }
            }

            // Call on module: driver.BrowsingContext.NavigateAsync(...)
            if (memberAccess.Expression is MemberAccessExpressionSyntax nestedMemberAccess &&
                nestedMemberAccess.Expression is IdentifierNameSyntax nestedIdentifier)
            {
                ITypeSymbol? type = semanticModel.GetTypeInfo(nestedIdentifier).Type;
                if (AnalyzerSymbolHelpers.IsCommandExecutorType(type))
                {
                    return nestedIdentifier.Identifier.Text;
                }
            }
        }

        return null;
    }

    private static bool IsCommandMethod(IMethodSymbol method)
    {
        INamedTypeSymbol? containingType = method.ContainingType;

        // Check if this is ExecuteCommandAsync on BiDiDriver
        if (AnalyzerSymbolHelpers.IsCommandExecutorType(containingType) && method.Name == "ExecuteCommandAsync")
        {
            return true;
        }

        // Check if this is a command method on a Module
        if (IsModuleType(containingType) && IsModuleCommandMethod(method))
        {
            return true;
        }

        return false;
    }

    private static bool IsModuleCommandMethod(IMethodSymbol method)
    {
        // Module command methods typically:
        // 1. Return Task<T> where T is a CommandResult
        // 2. Are named with "Async" suffix
        if (!method.Name.EndsWith("Async"))
        {
            return false;
        }

        ITypeSymbol? returnType = method.ReturnType;
        if (returnType is INamedTypeSymbol namedReturnType &&
            namedReturnType.Name == "Task" &&
            namedReturnType.IsGenericType &&
            namedReturnType.TypeArguments.Length == 1)
        {
            ITypeSymbol taskArgument = namedReturnType.TypeArguments[0];
            return InheritsFromCommandResult(taskArgument);
        }

        return false;
    }

    private static bool InheritsFromCommandResult(ITypeSymbol type)
    {
        INamedTypeSymbol? currentType = type as INamedTypeSymbol;
        while (currentType != null)
        {
            if (currentType.Name == "CommandResult")
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    private static bool IsModuleType(INamedTypeSymbol? type)
    {
        if (type == null)
        {
            return false;
        }

        // Check if the type inherits from Module
        INamedTypeSymbol? currentType = type.BaseType;
        while (currentType != null)
        {
            if (currentType.Name == "Module")
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}
