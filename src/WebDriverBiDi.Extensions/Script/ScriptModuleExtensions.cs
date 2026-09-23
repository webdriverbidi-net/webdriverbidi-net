// <copyright file="ScriptModuleExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Provides extension methods for the Script module.
/// </summary>
public static class ScriptModuleExtensions
{
    /// <summary>
    /// Adds a preload script, which runs in each document before any of the document's own scripts.
    /// </summary>
    /// <param name="module">The <see cref="ScriptModule"/> to extend.</param>
    /// <param name="functionDeclaration">The declaration of the JavaScript function the preload script runs.</param>
    /// <param name="arguments">The channel arguments passed to the function, or <see langword="null"/> for none.</param>
    /// <param name="sandbox">The name of the sandbox in which to run the function, or <see langword="null"/> to run it in the page.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>The ID of the preload script, for <see cref="RemovePreloadScriptAsync"/>.</returns>
    public static async Task<string> AddPreloadScriptAsync(this ScriptModule module, string functionDeclaration, IEnumerable<ChannelValue>? arguments = null, string? sandbox = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        AddPreloadScriptCommandParameters parameters = new(functionDeclaration)
        {
            Sandbox = sandbox,
        };
        if (arguments is not null)
        {
            parameters.Arguments.AddRange(arguments);
        }

        AddPreloadScriptCommandResult result = await module.AddPreloadScriptAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return result.PreloadScriptId;
    }

    /// <summary>
    /// Removes a preload script.
    /// </summary>
    /// <param name="module">The <see cref="ScriptModule"/> to extend.</param>
    /// <param name="preloadScriptId">The ID of the preload script.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RemovePreloadScriptAsync(this ScriptModule module, string preloadScriptId, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        await module.RemovePreloadScriptAsync(new RemovePreloadScriptCommandParameters(preloadScriptId), timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Calls a JavaScript function in a browsing context, awaiting a returned promise, and returns its result as a
    /// specific kind of value.
    /// </summary>
    /// <typeparam name="T">The kind of value the function returns, such as <see cref="StringRemoteValue"/>.</typeparam>
    /// <param name="module">The <see cref="ScriptModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context.</param>
    /// <param name="functionDeclaration">The declaration of the function, such as "() => document.title".</param>
    /// <param name="arguments">The arguments passed to the function, or <see langword="null"/> for none.</param>
    /// <param name="sandbox">The name of the sandbox in which to run the function, or <see langword="null"/> to run it in the page.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>The function's result.</returns>
    /// <exception cref="ScriptException">Thrown when the function throws.</exception>
    /// <exception cref="WebDriverBiDiException">Thrown when the result is not a <typeparamref name="T"/>.</exception>
    public static async Task<T> CallFunctionAsync<T>(this ScriptModule module, string browsingContextId, string functionDeclaration, IEnumerable<LocalValue>? arguments = null, string? sandbox = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
        where T : RemoteValue
    {
        RemoteValue result = await module.CallFunctionAsync(browsingContextId, functionDeclaration, arguments, sandbox, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return result.As<T>();
    }

    /// <summary>
    /// Calls a JavaScript function in a browsing context, awaiting a returned promise, and returns its result.
    /// </summary>
    /// <param name="module">The <see cref="ScriptModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context.</param>
    /// <param name="functionDeclaration">The declaration of the function, such as "() => document.title".</param>
    /// <param name="arguments">The arguments passed to the function, or <see langword="null"/> for none.</param>
    /// <param name="sandbox">The name of the sandbox in which to run the function, or <see langword="null"/> to run it in the page.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>The function's result.</returns>
    /// <exception cref="ScriptException">Thrown when the function throws.</exception>
    public static async Task<RemoteValue> CallFunctionAsync(this ScriptModule module, string browsingContextId, string functionDeclaration, IEnumerable<LocalValue>? arguments = null, string? sandbox = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        ContextTarget target = new(browsingContextId)
        {
            Sandbox = sandbox,
        };
        CallFunctionCommandParameters parameters = new(functionDeclaration, target, true);
        if (arguments is not null)
        {
            parameters.Arguments.AddRange(arguments);
        }

        EvaluateResult result = await module.CallFunctionAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return result is EvaluateResultException exceptionResult
            ? throw new ScriptException(exceptionResult.ExceptionDetails)
            : result.As<EvaluateResultSuccess>().Result;
    }
}
