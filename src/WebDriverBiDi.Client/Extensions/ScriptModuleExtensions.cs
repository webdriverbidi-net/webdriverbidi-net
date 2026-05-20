// <copyright file="ScriptModuleExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Numerics;

/// <summary>
/// Provides extension methods for the Script module.
/// </summary>
public static class ScriptModuleExtensions
{
    private static readonly Dictionary<RemoteValueType, List<Type>> RemoteValueTypeConversionDictionary = new()
    {
        [RemoteValueType.String] = [typeof(string)],
        [RemoteValueType.Number] = [typeof(int), typeof(long), typeof(double)],
        [RemoteValueType.Boolean] = [typeof(bool)],
        [RemoteValueType.BigInt] = [typeof(BigInteger)],
        [RemoteValueType.Date] = [typeof(DateTime)],
        [RemoteValueType.RegExp] = [typeof(RegularExpressionValue)],
        [RemoteValueType.Window] = [typeof(WindowProxyProperties)],
        [RemoteValueType.Node] = [typeof(NodeProperties), typeof(SharedReference)],
        [RemoteValueType.HtmlCollection] = [typeof(RemoteValueList)],
        [RemoteValueType.NodeList] = [typeof(RemoteValueList)],
        [RemoteValueType.Set] = [typeof(RemoteValueList)],
        [RemoteValueType.Array] = [typeof(RemoteValueList)],
        [RemoteValueType.Map] = [typeof(RemoteValueDictionary)],
        [RemoteValueType.Object] = [typeof(RemoteValueDictionary)],
    };

    /// <summary>
    /// Adds a preload script to be executed in each browsing context opened during the session.
    /// </summary>
    /// <param name="module">The <see cref="ScriptModule"/> to extend.</param>
    /// <param name="functionDeclaration">The declaration of the JavaScript function included in the preload script.</param>
    /// <param name="arguments">The arguments for the function.</param>
    /// <param name="sandbox">An optional name of the sandbox in which the JavaScript function should be executed.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The ID of the added preload script.</returns>
    public static async Task<string> AddPreloadScriptAsync(this ScriptModule module, string functionDeclaration, List<ChannelValue>? arguments = null, string? sandbox = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        AddPreloadScriptCommandParameters parameters = new(functionDeclaration)
        {
            Arguments = arguments,
            Sandbox = sandbox,
        };

        AddPreloadScriptCommandResult result = await module.AddPreloadScriptAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return result.PreloadScriptId;
    }

    /// <summary>
    /// Removes a preload script.
    /// </summary>
    /// <param name="module">The <see cref="ScriptModule"/> to extend.</param>
    /// <param name="preloadScriptId">The ID of the preload script to remove.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An Task representing the asynchronous operation.</returns>
    public static async Task RemovePreloadScriptAsync(this ScriptModule module, string preloadScriptId, TimeSpan? timeoutOverride, CancellationToken cancellationToken)
    {
        RemovePreloadScriptCommandParameters parameters = new(preloadScriptId);
        await module.RemovePreloadScriptAsync(parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a JavaScript function in the specified browsing context.
    /// </summary>
    /// <typeparam name="T">The type to which to convert the result of the JavaScript function.</typeparam>
    /// <param name="module">The <see cref="ScriptModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context in which to execute the function.</param>
    /// <param name="functionDeclaration">The declaration of the JavaScript function.</param>
    /// <param name="arguments">The arguments for the function.</param>
    /// <param name="sandbox">An optional name of the sandbox in which the JavaScript function should be executed.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The value of the function, converted to the requested base type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when the result of the JavaScript function cannot be converted to the requested type.</exception>
    public static async Task<T?> CallFunctionAsync<T>(this ScriptModule module, string browsingContextId, string functionDeclaration, List<LocalValue>? arguments = null, string? sandbox = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
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
        if (result is EvaluateResultException exceptionResult)
        {
            throw new WebDriverBiDiException(exceptionResult.ExceptionDetails.Text);
        }

        return ConvertRemoteValue<T>(((EvaluateResultSuccess)result).Result);
    }

    private static T? ConvertRemoteValue<T>(RemoteValue value)
    {
        Type requestedReturnType = typeof(T);
        List<Type> validConversionTypes = [typeof(RemoteObjectReference)];
        if (RemoteValueTypeConversionDictionary.ContainsKey(value.Type))
        {
            validConversionTypes = RemoteValueTypeConversionDictionary[value.Type];
        }

        if (!validConversionTypes.Contains(requestedReturnType))
        {
            throw new WebDriverBiDiException($"Requested return type {requestedReturnType}, but value returned from function is {value.Type}, and can only be converted to one of the following types: {string.Join(",", validConversionTypes)}");
        }

        object? valueObject = value switch
        {
            StringRemoteValue stringRemoteValue => stringRemoteValue.Value,
            BooleanRemoteValue booleanRemoteValue => booleanRemoteValue.Value,
            NumberRemoteValue numberRemoteValue => ConvertNumericValue(numberRemoteValue, requestedReturnType),
            BigIntegerRemoteValue bigintRemoteValue => bigintRemoteValue.Value,
            DateRemoteValue dateRemoteValue => dateRemoteValue.Value,
            RegExpRemoteValue regExpRemoteValue => regExpRemoteValue.Value,
            NodeRemoteValue nodeRemoteValue => requestedReturnType == typeof(NodeProperties) ? nodeRemoteValue.GetNodeProperties() : nodeRemoteValue.ToSharedReference(),
            WindowProxyRemoteValue windowProxyRemoteValue => windowProxyRemoteValue.Value,
            KeyValuePairCollectionRemoteValue dictionaryRemoteValue => dictionaryRemoteValue.Value,
            CollectionRemoteValue collectionRemoteValue => collectionRemoteValue.Value,
            NullRemoteValue _ => null,
            UndefinedRemoteValue _ => null,
            _ => value.ConvertTo<ObjectReferenceRemoteValue>().ToRemoteObjectReference(),
        };

        return (T?)valueObject;
    }

    private static object ConvertNumericValue(NumberRemoteValue numberRemoteValue, Type convertedType)
    {
        if (convertedType == typeof(int))
        {
            return numberRemoteValue.ToInt();
        }

        if (convertedType == typeof(long))
        {
            return numberRemoteValue.ToLong();
        }

        return numberRemoteValue.Value;
    }
}
