// <copyright file="BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
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

    private static readonly LocalizableString Description = "Fire-and-forget async calls to module commands, to the driver's own lifecycle operations, and to the library's other asynchronous operations -- a transport's or connection's connect, disconnect, send and disposal, an observer's waits for captured tasks, and an observer's or collector's disposal -- can lead to unhandled exceptions, race conditions, and operations that never execute. Always await async operations or explicitly capture the Task for later handling.";

    /// <summary>
    /// The driver's own asynchronous lifecycle operations. Discarding one of these tasks is at least
    /// as damaging as discarding a command's: an un-awaited StartAsync leaves the connect racing the
    /// next command, which then fails because the transport is not yet connected; an un-awaited
    /// StopAsync discards the AggregateException that carries every error collected under
    /// TransportErrorBehavior.Collect, the only point at which those are surfaced; an un-awaited
    /// RegisterTypeInfoResolverAsync may not have taken effect before the driver starts; and an
    /// un-awaited DisposeAsync leaves the connection and its receive loop running.
    /// </summary>
    private static readonly string[] DriverLifecycleMethodNames =
    [
        "StartAsync",
        "StopAsync",
        "RegisterTypeInfoResolverAsync",
        "DisposeAsync",
    ];

    /// <summary>
    /// The library types whose asynchronous operations are as damaging to discard as a command's.
    /// A wait for captured tasks that is never awaited returns nothing the caller can act on; an
    /// un-awaited <c>DisposeAsync</c> leaves an observer subscribed, a collector's channel open, or a
    /// connection and its receive loop running; and an un-awaited connect, disconnect or send races
    /// whatever the caller does next, with any failure going unobserved.
    /// </summary>
    private static readonly string[] AsyncOperationOwnerTypeNames =
    [
        "Transport",
        "Connection",
        "EventObserver",
        "EventDataCollector",
    ];

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi010");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        // A module command, ExecuteCommandAsync on the driver itself (which sends a command over the
        // same connection a module command does), or one of the driver's own lifecycle operations.
        // Discarding any of their tasks is the same class of hazard.
        bool isDriverLifecycleCall = IsDriverLifecycleMethod(method);
        bool isLibraryAsyncOperation = IsLibraryAsyncOperation(method);
        if (!IsModuleType(method.ContainingType) && !IsExecuteCommandAsync(method) && !isDriverLifecycleCall && !isLibraryAsyncOperation)
        {
            return;
        }

        // A module command always answers with a result, so only Task<T> is a command's task. The
        // lifecycle operations answer with no value: StartAsync, StopAsync and
        // RegisterTypeInfoResolverAsync return a bare Task and DisposeAsync a ValueTask, none of which
        // the generic test admits.
        if (!(isDriverLifecycleCall || isLibraryAsyncOperation ? IsAwaitableReturningMethod(method) : IsTaskReturningMethod(method)))
        {
            return;
        }

        // A call is fire-and-forget only when its result is discarded. Rather than trying to
        // enumerate every way a result can be consumed (await, assignment, argument, array/collection
        // element, conditional, and so on), detect the single discard shape: the call stands alone as
        // an expression statement.
        if (!IsResultDiscarded(invocation))
        {
            return;
        }

        // Report diagnostic for fire-and-forget call
        Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsModuleType(INamedTypeSymbol type)
    {
        // Any type deriving from the library's Module base class is a module, whatever it is named:
        // a custom class GoogleCdp : Module is registered and invoked exactly as one named
        // GoogleCdpModule would be.
        return HasModuleBaseClass(type);
    }

    private static bool IsExecuteCommandAsync(IMethodSymbol method)
    {
        return method.Name == "ExecuteCommandAsync" && AnalyzerSymbolHelpers.IsCommandExecutorType(method.ContainingType);
    }

    private static bool IsDriverLifecycleMethod(IMethodSymbol method)
    {
        // The containing-type test is what keeps this to the driver's own lifecycle: a module, or any
        // other type reached through the driver, may declare a method of the same name, and
        // discarding that task is not this rule's subject.
        return System.Array.IndexOf(DriverLifecycleMethodNames, method.Name) >= 0
            && AnalyzerSymbolHelpers.IsCommandExecutorType(method.ContainingType);
    }

    /// <summary>
    /// Determines whether a method is one of the library's own asynchronous operations outside the
    /// command pipeline.
    /// </summary>
    /// <param name="method">The method the invocation binds to.</param>
    /// <returns><see langword="true"/> if the library declares the operation.</returns>
    /// <remarks>
    /// The declaring type decides, so that a user's own asynchronous method on a type derived from
    /// <c>Connection</c> is not judged, while an override of one of the library's own is: an overridden
    /// <c>SendDataAsync</c> is still the send whose failure would go unobserved.
    /// </remarks>
    private static bool IsLibraryAsyncOperation(IMethodSymbol method)
    {
        IMethodSymbol declaration = method;
        while (declaration.OverriddenMethod is not null)
        {
            declaration = declaration.OverriddenMethod;
        }

        if (!AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(declaration.ContainingType))
        {
            return false;
        }

        for (INamedTypeSymbol? current = declaration.ContainingType; current is not null; current = current.BaseType)
        {
            if (System.Array.IndexOf(AsyncOperationOwnerTypeNames, current.Name) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasModuleBaseClass(INamedTypeSymbol type)
    {
        INamedTypeSymbol? currentType = type.BaseType;
        while (currentType != null)
        {
            if (AnalyzerSymbolHelpers.IsLibraryTypeNamed(currentType, "Module"))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    private static bool IsTaskReturningMethod(IMethodSymbol method)
    {
        return method.ReturnType is INamedTypeSymbol namedType && namedType.Name == "Task" && namedType.IsGenericType;
    }

    private static bool IsAwaitableReturningMethod(IMethodSymbol method)
    {
        // Task, Task<T>, ValueTask and ValueTask<T> alike: what matters is that the call yields
        // something whose completion the caller is meant to observe.
        return method.ReturnType is INamedTypeSymbol { Name: "Task" or "ValueTask" };
    }

    private static bool IsResultDiscarded(IOperation operation)
    {
        IOperation? parent = operation.Parent;

        // Follow conversions unconditionally: they preserve the value.
        if (parent is IConversionOperation)
        {
            return IsResultDiscarded(parent);
        }

        // A call made through a null-conditional receiver (driver?.BrowsingContext.NavigateAsync(p))
        // is the WhenNotNull part of a conditional access; the value that is kept or discarded is
        // that of the conditional access as a whole.
        if (parent is IConditionalAccessOperation)
        {
            return IsResultDiscarded(parent);
        }

        // Follow a chained member call only when it still yields an awaitable wrapper of the
        // command's task (a Task-returning continuation, or the ConfiguredTaskAwaitable from
        // .ConfigureAwait(false)); the chain is fire-and-forget only if that outer value is
        // itself discarded. A chained call that consumes the task — such as the void-returning
        // .Wait(), which blocks until completion and propagates exceptions — is not
        // fire-and-forget and must end the chain without a diagnostic.
        if (parent is IInvocationOperation parentInvocation)
        {
            return parentInvocation.TargetMethod.ReturnType.Name is "Task" or "ValueTask" or "ConfiguredTaskAwaitable" or "ConfiguredValueTaskAwaitable"
                && IsResultDiscarded(parent);
        }

        // The result is discarded when the (outer) expression stands alone as a statement. Every other
        // context — await, assignment, argument, array/collection element, conditional, return —
        // consumes the value.
        return parent is IExpressionStatementOperation;
    }
}
