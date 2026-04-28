// <copyright file="BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Analyzer that detects fire-and-forget async calls to module command methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI010";

    private const string Category = "Reliability";

    private static readonly LocalizableString Title = "Async module command should be awaited";

    private static readonly LocalizableString MessageFormat = "Async method '{0}' should be awaited, assigned to a variable, or passed as an argument to avoid fire-and-forget behavior";

    private static readonly LocalizableString Description = "Fire-and-forget async calls to module commands can lead to unhandled exceptions, race conditions, and commands that never execute. Always await async operations or explicitly capture the Task for later handling.";

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

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        IInvocationOperation invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;

        // Check if this is a module method
        if (!IsModuleType(method.ContainingType))
        {
            return;
        }

        // Check if the method returns Task<T>
        if (!IsTaskReturningMethod(method))
        {
            return;
        }

        // Check if the return value is used (awaited, assigned, or passed as argument)
        if (IsReturnValueUsed(invocation))
        {
            return;
        }

        // Report diagnostic for fire-and-forget call
        Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsModuleType(INamedTypeSymbol? type)
    {
        if (type == null)
        {
            return false;
        }

        // Check if the type is a Module subclass
        return type.Name.EndsWith("Module") && HasModuleBaseClass(type);
    }

    private static bool HasModuleBaseClass(INamedTypeSymbol type)
    {
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

    private static bool IsTaskReturningMethod(IMethodSymbol method)
    {
        ITypeSymbol? returnType = method.ReturnType;
        if (returnType == null)
        {
            return false;
        }

        // Check if return type is Task<T> (generic Task)
        if (returnType is INamedTypeSymbol namedType && namedType.Name == "Task" && namedType.IsGenericType)
        {
            return true;
        }

        return false;
    }

    private static bool IsReturnValueUsed(IInvocationOperation invocation)
    {
        IOperation? parent = invocation.Parent;

        if (parent == null)
        {
            return false;
        }

        // Check if the invocation is awaited
        if (parent is IAwaitOperation)
        {
            return true;
        }

        // Check if assigned to a variable
        if (parent is IVariableInitializerOperation or ISimpleAssignmentOperation)
        {
            return true;
        }

        // Check if used as an argument
        if (parent is IArgumentOperation)
        {
            return true;
        }

        // Check if used as a return value
        if (parent is IReturnOperation)
        {
            return true;
        }

        // Check if used in a conversion (e.g., casting to Task)
        if (parent is IConversionOperation conversionOperation)
        {
            // If it's part of a conversion, check if that conversion is used
            return IsReturnValueUsed(conversionOperation);
        }

        // Check if this invocation is the instance of another invocation (e.g., task.ConfigureAwait())
        // In this case, we need to check if that parent invocation is used
        if (parent is IInvocationOperation parentInvocation)
        {
            return IsOperationUsed(parentInvocation);
        }

        return false;
    }

    private static bool IsReturnValueUsed(IConversionOperation conversion)
    {
        IOperation? parent = conversion.Parent;

        if (parent == null)
        {
            return false;
        }

        // Check the same conditions as for invocation
        if (parent is IAwaitOperation or
            IVariableInitializerOperation or
            ISimpleAssignmentOperation or
            IArgumentOperation or
            IReturnOperation)
        {
            return true;
        }

        return false;
    }

    private static bool IsOperationUsed(IOperation operation)
    {
        IOperation? parent = operation.Parent;

        if (parent == null)
        {
            return false;
        }

        // Check if the operation result is awaited
        if (parent is IAwaitOperation)
        {
            return true;
        }

        // Check if assigned to a variable
        if (parent is IVariableInitializerOperation or ISimpleAssignmentOperation)
        {
            return true;
        }

        // Check if used as an argument
        if (parent is IArgumentOperation)
        {
            return true;
        }

        // Check if used as a return value
        if (parent is IReturnOperation)
        {
            return true;
        }

        // Check if it's part of another invocation (e.g., .ConfigureAwait(false))
        if (parent is IInvocationOperation)
        {
            // Recursively check if that invocation is used
            return IsOperationUsed(parent);
        }

        // Check if used in a conversion
        if (parent is IConversionOperation)
        {
            return IsOperationUsed(parent);
        }

        return false;
    }
}
