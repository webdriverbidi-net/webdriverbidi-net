// <copyright file="Page.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using System.Numerics;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Elements;
using WebDriverBiDi.Script;

/// <summary>
/// Provides a high-level abstraction over a browsing context for navigating and interacting with web pages.
/// </summary>
public class Page
{
    private static readonly List<Type> ValidScriptArgumentTypes = [
        typeof(string),
        typeof(int),
        typeof(long),
        typeof(double),
        typeof(decimal),
        typeof(BigInteger),
        typeof(DateTime),
        typeof(WindowProxy),
        typeof(ElementProxy),
        typeof(RemoteJavaScriptObjectProxy),
        typeof(List<object?>),
        typeof(Dictionary<string, object?>),
        typeof(Dictionary<object, object?>),
    ];

    private static readonly List<Type> ValidScriptReturnTypes = [
        typeof(string),
        typeof(int),
        typeof(long),
        typeof(double),
        typeof(decimal),
        typeof(BigInteger),
        typeof(DateTime),
        typeof(WindowProxy),
        typeof(ElementProxy),
        typeof(RemoteJavaScriptObjectProxy),
        typeof(List<object?>),
        typeof(Dictionary<string, object?>),
        typeof(Dictionary<object, object?>),
    ];

    private readonly BiDiDriver driver;
    private readonly string browsingContextId;
    private readonly ElementStateInspector inspector;
    private readonly ElementLocatorSettings locatorSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="Page"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="browsingContextId">The ID of the browsing context this page wraps.</param>
    /// <param name="inspector">the <see cref="ElementStateInspector"/> used to inspect element state.</param>
    internal Page(BiDiDriver driver, string browsingContextId, ElementStateInspector inspector)
        : this(driver, browsingContextId, new ElementLocatorSettings(), inspector)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Page"/> class with custom locator settings.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="browsingContextId">The ID of the browsing context this page wraps.</param>
    /// <param name="locatorSettings">The <see cref="ElementLocatorSettings"/> to apply to element locators created by this page.</param>
    /// <param name="inspector">the <see cref="ElementStateInspector"/> used to inspect element state.</param>
    internal Page(BiDiDriver driver, string browsingContextId, ElementLocatorSettings locatorSettings, ElementStateInspector inspector)
    {
        this.driver = driver;
        this.browsingContextId = browsingContextId;
        this.locatorSettings = locatorSettings;
        this.inspector = inspector;
    }

    /// <summary>
    /// Gets the ID of this page.
    /// </summary>
    public string Id => this.browsingContextId;

    /// <summary>
    /// Navigates the page to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <param name="wait">The readiness state to wait for after navigation. Defaults to <see cref="ReadinessState.Complete"/>.</param>
    /// <param name="timeout">Optional timeout override. If null, uses the driver's default command timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the URL navigated to after any redirects.</returns>
    public async Task<string> NavigateAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null)
    {
        NavigateCommandParameters parameters = new(this.browsingContextId, url)
        {
            Wait = wait,
        };
        NavigateCommandResult result = await this.driver.BrowsingContext.NavigateAsync(parameters, timeout).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Executes a JavaScript function in the page, omitting any return values. If the function returns a
    /// Promise, it will await the resolution of the Promise.
    /// </summary>
    /// <param name="functionDefinition">The full definition of the function. This may be a "fat arrow" function definition (e.g., "(a, b) => a + b").</param>
    /// <param name="arguments">The arguments for the function.</param>
    /// <returns>The return value of the function.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the JavaScript function throws an error.</exception>
    public async Task ExecuteJavaScriptFunctionAsync(string functionDefinition, params object?[] arguments)
    {
        List<LocalValue> args = [];
        foreach (object? arg in arguments)
        {
            args.Add(this.ConvertToLocalValue(arg));
        }

        await this.driver.Script.CallFunctionAsync(this.browsingContextId, functionDefinition, args);
   }

    /// <summary>
    /// Executes a JavaScript function in the page, returning the value of the function. If the function returns a
    /// Promise, it will await the resolution of the Promise.
    /// </summary>
    /// <typeparam name="T">The return type of the JavaScript function.</typeparam>
    /// <param name="functionDefinition">The full definition of the function. This may be a "fat arrow" function definition (e.g., "(a, b) => a + b").</param>
    /// <param name="arguments">The arguments for the function.</param>
    /// <returns>The return value of the function.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the JavaScript function throws an error.</exception>
    public async Task<T?> ExecuteJavaScriptFunctionAsync<T>(string functionDefinition, params object?[] arguments)
    {
        Type requestedType = typeof(T);
        if (ValidScriptReturnTypes.Contains(requestedType))
        {
           throw new WebDriverBiDiException($"requested return type was ${requestedType}, but must be one of the following types: ${string.Join(",", ValidScriptArgumentTypes)}");
        }

        List<LocalValue> args = [];
        foreach (object? arg in arguments)
        {
            args.Add(this.ConvertToLocalValue(arg));
        }

        RemoteValue functionResult = await this.driver.Script.CallFunctionAsync(this.browsingContextId, functionDefinition, args);
        return (T?)this.ConvertFromRemoteValue(functionResult);
    }

    /// <summary>
    /// Captures a screenshot of the page.
    /// </summary>
    /// <param name="fullPage">
    /// When <see langword="true"/>, captures the full document including content outside the viewport.
    /// When <see langword="false"/> (default), captures only the visible viewport.
    /// </param>
    /// <param name="timeout">Optional timeout override. If null, uses the driver's default command timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the screenshot as a PNG byte array.</returns>
    public async Task<byte[]> GetScreenshotAsync(bool fullPage = false, TimeSpan? timeout = null)
    {
        CaptureScreenshotCommandParameters parameters = new(this.browsingContextId)
        {
            Origin = fullPage ? ScreenshotOrigin.Document : ScreenshotOrigin.Viewport,
        };
        CaptureScreenshotCommandResult result = await this.driver.BrowsingContext.CaptureScreenshotAsync(parameters, timeout).ConfigureAwait(false);
        return Convert.FromBase64String(result.Data);
    }

    /// <summary>
    /// Creates an <see cref="ElementLocator"/> for finding elements in this page using the specified locator strategy.
    /// </summary>
    /// <param name="locator">The <see cref="Locator"/> strategy to use to find elements.</param>
    /// <returns>An <see cref="ElementLocator"/> that can be used to interact with the matched elements.</returns>
    public ElementLocator LocateElement(Locator locator)
    {
        return new ElementLocator(this.driver, this.browsingContextId, locator, null, this.inspector, this.locatorSettings, null);
    }

    private object? ConvertFromRemoteValue(RemoteValue remoteValue)
    {
        object? valueObject = remoteValue switch
        {
            StringRemoteValue stringRemoteValue => stringRemoteValue.Value,
            BooleanRemoteValue booleanRemoteValue => booleanRemoteValue.Value,
            NumberRemoteValue numberRemoteValue => numberRemoteValue.Value,
            BigIntegerRemoteValue bigintRemoteValue => bigintRemoteValue.Value,
            DateRemoteValue dateRemoteValue => dateRemoteValue.Value,
            RegExpRemoteValue regExpRemoteValue => regExpRemoteValue.Value,
            NodeRemoteValue nodeRemoteValue => new ElementProxy(nodeRemoteValue.ToSharedReference()),
            WindowProxyRemoteValue windowRemoteValue => new WindowProxy(windowRemoteValue.ToRemoteObjectReference()),
            KeyValuePairCollectionRemoteValue dictionaryRemoteValue => this.ConvertFromRemoteValueDictionary(dictionaryRemoteValue.Value),
            CollectionRemoteValue collectionRemoteValue => this.ConvertFromRemoteValueList(collectionRemoteValue.Value),
            NullRemoteValue _ => null,
            UndefinedRemoteValue _ => null,
            _ => new RemoteJavaScriptObjectProxy(remoteValue.ConvertTo<ObjectReferenceRemoteValue>().ToRemoteObjectReference()),
        };

        return valueObject;
    }

    private List<object?>? ConvertFromRemoteValueList(RemoteValueList? collection)
    {
        if (collection is null)
        {
            return null;
        }

        List<object?> list = [];
        foreach (RemoteValue remoteValue in collection)
        {
            list.Add(this.ConvertFromRemoteValue(remoteValue));
        }

        return list;
    }

    private object? ConvertFromRemoteValueDictionary(RemoteValueDictionary? remoteDictionary)
    {
        if (remoteDictionary is null)
        {
            return null;
        }

        bool allStringKeys = true;
        foreach (object key in remoteDictionary.Keys)
        {
            if (key is not string)
            {
                allStringKeys = false;
                break;
            }
        }

        if (allStringKeys)
        {
            Dictionary<string, object?> stringKeyDictionary = [];
            foreach (KeyValuePair<object, RemoteValue> pair in remoteDictionary)
            {
                stringKeyDictionary[(string)pair.Key] = this.ConvertFromRemoteValue(pair.Value);
            }

            return stringKeyDictionary;
        }

        Dictionary<object, object?> dictionary = [];
        foreach (KeyValuePair<object, RemoteValue> pair in remoteDictionary)
        {
            dictionary[pair.Key] = this.ConvertFromRemoteValue(pair.Value);
        }

        return dictionary;
    }

    private LocalValue ConvertToLocalValue(object? value)
    {
        if (value is null)
        {
            return LocalValue.Null;
        }

        LocalValue local = value switch
        {
            string stringValue => LocalValue.String(stringValue),
            int intValue => LocalValue.Number(intValue),
            long longValue => LocalValue.Number(longValue),
            double doubleValue => LocalValue.Number(doubleValue),
            decimal decimalValue => LocalValue.Number(decimalValue),
            bool booleanValue => LocalValue.Boolean(booleanValue),
            BigInteger bigIntValue => LocalValue.BigInt(bigIntValue),
            DateTime dateValue => LocalValue.Date(dateValue),
            WindowProxy windowProxyValue => new RemoteObjectReference(windowProxyValue.WindowId),
            ElementProxy elementProxyValue => new SharedReference(elementProxyValue.ElementId),
            RemoteJavaScriptObjectProxy remoteObjectProxyValue => new RemoteObjectReference(remoteObjectProxyValue.RemoteObjectId),
            List<object?> listValue => LocalValue.Array(this.ConvertListToLocalValue(listValue)),
            Dictionary<string, object?> dictionaryValue => LocalValue.Object(this.ConvertDictionaryToLocalValue(dictionaryValue)),
            _ => throw new WebDriverBiDiException($"argument was of type ${value.GetType()}, but must be null or one of the following types: ${string.Join(",", ValidScriptArgumentTypes)}"),
        };

        return local;
    }

    private Dictionary<string, LocalValue> ConvertDictionaryToLocalValue(Dictionary<string, object?> dictionary)
    {
        Dictionary<string, LocalValue> localValueDictionary = [];
        foreach (KeyValuePair<string, object?> keyValuePair in dictionary)
        {
            localValueDictionary[keyValuePair.Key] = this.ConvertToLocalValue(keyValuePair.Value);
        }

        return localValueDictionary;
    }

    private List<LocalValue> ConvertListToLocalValue(List<object?> list)
    {
        List<LocalValue> localValueList = [];
        foreach (object? listElement in list)
        {
            localValueList.Add(this.ConvertToLocalValue(listElement));
        }

        return localValueList;
    }
}
