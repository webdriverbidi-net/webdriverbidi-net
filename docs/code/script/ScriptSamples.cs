// <copyright file="ScriptSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/script.md and docs/articles/advanced/error-handling.md

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace WebDriverBiDi.Docs.Code.Script;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for Script module and error handling documentation. Compiled at build time to prevent API drift.
/// </summary>
public class ScriptSamples
{

    /// <summary>
    /// Accessing the Script module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingtheModule
        ScriptModule script = driver.Script;
#endregion
    }

    /// <summary>
    /// Evaluate expression and get result.
    /// </summary>
    public static async Task EvaluateExpression(
        BiDiDriver driver,
        string contextId)
    {
#region EvaluateExpression
        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            "document.title",
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);

        if (result is EvaluateResultSuccess success)
        {
            string title = success.Result.ValueAs<string>();
            Console.WriteLine($"Title: {title}");
        }
#endregion
    }

    /// <summary>
    /// Evaluate with complex expression.
    /// </summary>
    public static async Task EvaluateComplexExpression(
        BiDiDriver driver,
        string contextId)
    {
#region EvaluateComplexExpression
        string expression = """
            {
                title: document.title,
                url: window.location.href,
                elementCount: document.querySelectorAll('*').length
            }
            """;

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            expression,
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);
#endregion
    }

    /// <summary>
    /// Call function with LocalValue arguments.
    /// </summary>
    public static async Task CallFunctionWithArguments(
        BiDiDriver driver,
        string contextId)
    {
#region CallFunctionwithArguments
        string functionDefinition = "(a, b) => a + b";

        CallFunctionCommandParameters parameters = new CallFunctionCommandParameters(
            functionDefinition,
            new ContextTarget(contextId),
            true);
        parameters.Arguments.Add(LocalValue.Number(5));
        parameters.Arguments.Add(LocalValue.Number(10));

        EvaluateResult result = await driver.Script.CallFunctionAsync(parameters);

        if (result is EvaluateResultSuccess success)
        {
            long sum = success.Result.ValueAs<long>();
            Console.WriteLine($"Sum: {sum}");  // 15
        }
#endregion
    }

    /// <summary>
    /// Call function with DOM element via ToSharedReference.
    /// </summary>
    public static async Task CallFunctionWithDomElement(
        BiDiDriver driver,
        string contextId)
    {
#region CallFunctionwithDOMElement
        EvaluateResult elementResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('button')",
                new ContextTarget(contextId),
                true));

        if (elementResult is EvaluateResultSuccess elementSuccess)
        {
            RemoteValue element = elementSuccess.Result;

            string functionDefinition = "(element) => element.click()";

            CallFunctionCommandParameters parameters = new CallFunctionCommandParameters(
                functionDefinition,
                new ContextTarget(contextId),
                false);
            parameters.Arguments.Add(element.ToSharedReference());

            await driver.Script.CallFunctionAsync(parameters);
        }
#endregion
    }

    /// <summary>
    /// Call method on object - getAttribute with ToSharedReference and LocalValue.
    /// </summary>
    public static async Task CallMethodOnObject(
        BiDiDriver driver,
        string contextId)
    {
#region CallMethodonObject
        // Get object reference
        EvaluateResult objectResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.getElementById('myDiv')",
                new ContextTarget(contextId),
                true));

        if (objectResult is EvaluateResultSuccess objectSuccess)
        {
            RemoteValue divElement = objectSuccess.Result;

            // Call getAttribute method
            string functionDefinition = "(element, attrName) => element.getAttribute(attrName)";

            CallFunctionCommandParameters parameters = new CallFunctionCommandParameters(
                functionDefinition,
                new ContextTarget(contextId),
                false);
            parameters.Arguments.Add(divElement.ToSharedReference());
            parameters.Arguments.Add(LocalValue.String("class"));

            EvaluateResult result = await driver.Script.CallFunctionAsync(parameters);
            if (result is EvaluateResultSuccess success)
            {
                string className = success.Result.ValueAs<string>();
                Console.WriteLine($"Class: {className}");
            }
        }
#endregion
    }

    /// <summary>
    /// Context target for script execution.
    /// </summary>
    public static void ContextTargetExample(string contextId)
    {
#region ContextTarget
        ContextTarget target = new ContextTarget(contextId);

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            "document.title",
            target,
            true);
#endregion
    }

    /// <summary>
    /// Realm target for script execution.
    /// </summary>
    public static void RealmTargetExample(string realmId)
    {
#region RealmTarget
        RealmTarget target = new RealmTarget(realmId);

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            "window.myCustomProperty",
            target,
            true);
#endregion
    }

    /// <summary>
    /// Sandboxed execution - ContextTarget with Sandbox.
    /// </summary>
    public static async Task SandboxedExecution(
        BiDiDriver driver,
        string contextId)
    {
#region SandboxedExecution
        ContextTarget target = new ContextTarget(contextId)
        {
            Sandbox = "myIsolatedSandbox",
        };

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            "window.isolatedData = { value: 42 }",
            target,
            false);
        await driver.Script.EvaluateAsync(parameters);

        // Later, access the same sandbox
        target = new ContextTarget(contextId)
        {
            Sandbox = "myIsolatedSandbox",
        };
        parameters = new EvaluateCommandParameters(
            "window.isolatedData.value",
            target,
            true);
        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);
#endregion
    }

    /// <summary>
    /// Add preload script.
    /// </summary>
    public static async Task AddPreloadScript(BiDiDriver driver)
    {
#region AddPreloadScript
        string preloadScript = """
            () => {
                window.myUtility = {
                    getElementTag: (element) => element.tagName
                };
            }
            """;

        AddPreloadScriptCommandParameters parameters = new AddPreloadScriptCommandParameters(preloadScript);

        AddPreloadScriptCommandResult result = await driver.Script.AddPreloadScriptAsync(parameters);

        string preloadScriptId = result.PreloadScriptId;
#endregion
    }

    /// <summary>
    /// Preload script with channel - ChannelValue for messaging.
    /// </summary>
    public static async Task PreloadScriptWithChannel(BiDiDriver driver)
    {
#region PreloadScriptwithChannel
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Script.OnMessage.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "myChannel")
            {
                Console.WriteLine($"Message from preload: {e.Data.ValueAs<string>()}");
            }
        });

        string preloadScript = """
            (channel) => {
                window.addEventListener('load', () => {
                    channel('Page loaded');
                });
            }
            """;

        ChannelValue channel = new ChannelValue(new ChannelProperties("myChannel"));

        AddPreloadScriptCommandParameters parameters = new AddPreloadScriptCommandParameters(preloadScript);
        parameters.Arguments = new List<ChannelValue> { channel };

        await driver.Script.AddPreloadScriptAsync(parameters);
#endregion
    }

    /// <summary>
    /// Sandboxed preload script.
    /// </summary>
    public static async Task SandboxedPreloadScript(
        BiDiDriver driver,
        string contextId)
    {
#region SandboxedPreloadScript
        string preloadScript = """
            () => {
                window.isolatedUtils = {
                    getPageTitle: () => document.title
                };
            }
            """;

        AddPreloadScriptCommandParameters parameters = new AddPreloadScriptCommandParameters(preloadScript)
        {
            Sandbox = "utilsSandbox",
        };

        await driver.Script.AddPreloadScriptAsync(parameters);

        // Later, call the utility from the same sandbox
        ContextTarget target = new ContextTarget(contextId)
        {
            Sandbox = "utilsSandbox",
        };

        EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
            "window.isolatedUtils.getPageTitle()",
            target,
            true);
        EvaluateResult result = await driver.Script.EvaluateAsync(evalParams);
#endregion
    }

    /// <summary>
    /// Remove preload script.
    /// </summary>
    public static async Task RemovePreloadScript(BiDiDriver driver, string preloadScriptId)
    {
#region RemovePreloadScript
        RemovePreloadScriptCommandParameters parameters =
            new RemovePreloadScriptCommandParameters(preloadScriptId);

        await driver.Script.RemovePreloadScriptAsync(parameters);
#endregion
    }

    /// <summary>
    /// Accessing primitive values from EvaluateResult.
    /// </summary>
    public static async Task AccessingPrimitiveValues(
        BiDiDriver driver,
        string contextId)
    {
#region AccessingPrimitiveValues
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("42", new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValue value = success.Result;

            switch (value.Type)
            {
                case "string":
                    string str = value.ValueAs<string>();
                    break;
                case "number":
                    long num = value.ValueAs<long>();
                    break;
                case "boolean":
                    bool flag = value.ValueAs<bool>();
                    break;
                case "null":
                case "undefined":
                    break;
            }
        }
#endregion
    }

    /// <summary>
    /// Accessing objects with RemoteValueDictionary.
    /// </summary>
    public static async Task AccessingObjects(
        BiDiDriver driver,
        string contextId)
    {
#region AccessingObjects
        string expression = """
            (
                name: 'John',
                age: 30,
                active: true
            }
            """;

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            expression,
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);

        if (result is EvaluateResultSuccess success)
        {
            RemoteValue obj = success.Result;

            // Access as RemoteValueDictionary
            RemoteValueDictionary dict = obj.ValueAs<RemoteValueDictionary>();
            Console.WriteLine($"Name: {dict["name"].ValueAs<string>()}");
            Console.WriteLine($"Age: {dict["age"].ValueAs<long>()}");
        }
#endregion
    }

    /// <summary>
    /// Accessing arrays with RemoteValueList.
    /// </summary>
    public static async Task AccessingArrays(
        BiDiDriver driver,
        string contextId)
    {
#region AccessingArrays
        string expression = "[1, 2, 3, 4, 5]";

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            expression,
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);

        if (result is EvaluateResultSuccess success)
        {
            RemoteValue array = success.Result;

            // Access as RemoteValueList
            RemoteValueList list = array.ValueAs<RemoteValueList>();
            Console.WriteLine($"Array length: {list.Count}");
            foreach (RemoteValue item in list)
            {
                Console.WriteLine($"  Item: {item.ValueAs<long>()}");
            }
        }
#endregion
    }

    /// <summary>
    /// Working with DOM elements - node properties.
    /// </summary>
    public static async Task WorkingWithDomElements(
        BiDiDriver driver,
        string contextId)
    {
#region WorkingwithDOMElements
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('button')",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValue element = success.Result;

            // Check if it's a node
            if (element.Type == "node")
            {
                // Get node properties
                NodeProperties? nodeProps = element.ValueAs<NodeProperties>();

                if (nodeProps != null)
                {
                    Console.WriteLine($"Tag: {nodeProps.LocalName}");
                    Console.WriteLine($"Node type: {nodeProps.NodeType}");

                    // Get attributes
                    if (nodeProps.Attributes != null)
                    {
                        foreach (var attr in nodeProps.Attributes)
                        {
                            Console.WriteLine($"  {attr.Key} = {attr.Value}");
                        }
                    }
                }

                // Get shared ID for later use
                string? sharedId = element.SharedId;

                // Create reference to pass to other commands
                SharedReference elementRef = element.ToSharedReference();
            }
        }
#endregion
    }

    /// <summary>
    /// LocalValue primitives for CallFunction.
    /// </summary>
    public static void LocalValuePrimitivesForCallFunction(
        BiDiDriver driver,
        string contextId)
    {
#region LocalValuePrimitives
        CallFunctionCommandParameters parameters = new CallFunctionCommandParameters(
            "(str, num, bool) => ({ str, num, bool })",
            new ContextTarget(contextId),
            true);

        parameters.Arguments.Add(LocalValue.String("Hello"));
        parameters.Arguments.Add(LocalValue.Number(42));
        parameters.Arguments.Add(LocalValue.Boolean(true));
#endregion
    }

    /// <summary>
    /// LocalValue special values - Null, Undefined, Infinity, NaN.
    /// </summary>
    public static void LocalValueSpecialValuesForCallFunction(
        CallFunctionCommandParameters parameters)
    {
#region LocalValueSpecialValues
        parameters.Arguments.Add(LocalValue.Null);
        parameters.Arguments.Add(LocalValue.Undefined);
        parameters.Arguments.Add(LocalValue.Number(double.PositiveInfinity));
        parameters.Arguments.Add(LocalValue.Number(double.NaN));
#endregion
    }

    /// <summary>
    /// LocalValue object for CallFunction.
    /// </summary>
    public static void LocalValueObjectForCallFunction(
        CallFunctionCommandParameters parameters)
    {
#region LocalValueObject
        Dictionary<string, LocalValue> obj = new Dictionary<string, LocalValue>
        {
            { "name", LocalValue.String("John") },
            { "age", LocalValue.Number(30) },
            { "active", LocalValue.Boolean(true) },
        };

        parameters.Arguments.Add(LocalValue.Object(obj));
#endregion
    }

    /// <summary>
    /// LocalValue array for CallFunction.
    /// </summary>
    public static void LocalValueArrayForCallFunction(
        CallFunctionCommandParameters parameters)
    {
#region LocalValueArray
        List<LocalValue> array = new List<LocalValue>
        {
            LocalValue.Number(1),
            LocalValue.Number(2),
            LocalValue.Number(3),
        };

        parameters.Arguments.Add(LocalValue.Array(array));
#endregion
    }

    /// <summary>
    /// LocalValue Date.
    /// </summary>
    public static void LocalValueDate(CallFunctionCommandParameters parameters)
    {
#region LocalValueDate
        parameters.Arguments.Add(LocalValue.Date(DateTime.Now));
#endregion
    }

    /// <summary>
    /// LocalValue RegularExpression.
    /// </summary>
    public static void LocalValueRegExp(CallFunctionCommandParameters parameters)
    {
#region LocalValueRegExp
        parameters.Arguments.Add(LocalValue.RegExp("\\d+", "g"));
#endregion
    }

    /// <summary>
    /// Try-catch pattern for script errors.
    /// </summary>
    public static async Task TryCatchPattern(
        BiDiDriver driver,
        string contextId)
    {
#region Try-CatchPattern
        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            "throw new Error('Something went wrong')",
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);

        if (result is EvaluateResultException exception)
        {
            Console.WriteLine($"Error: {exception.ExceptionDetails.Text}");
            Console.WriteLine($"Line: {exception.ExceptionDetails.LineNumber}");
            Console.WriteLine($"Column: {exception.ExceptionDetails.ColumnNumber}");

            if (exception.ExceptionDetails.StackTrace != null)
            {
                Console.WriteLine($"Stack: {exception.ExceptionDetails.StackTrace.CallFrames.Count} frames");
            }
        }
#endregion
    }

    /// <summary>
    /// Catching JavaScript errors in script.
    /// </summary>
    public static async Task CatchingJavaScriptErrors(
        BiDiDriver driver,
        string contextId)
    {
#region CatchingJavaScriptErrors
        string safeExpression = """
            try {
                return dangerousOperation();
            } catch (error) {
                return { error: error.message };
            }
            """;

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            safeExpression,
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);
        // Will always be success, check for error property in result
#endregion
    }

    /// <summary>
    /// Automatic promise resolution with fetch.
    /// </summary>
    public static async Task AutomaticPromiseResolution(
        BiDiDriver driver,
        string contextId)
    {
#region AutomaticPromiseResolution
        // Fetch API returns a promise
       string expression = """
            fetch('https://api.example.com/data')
                .then(response => response.json())
            """;

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            expression,
            new ContextTarget(contextId),
            true  // awaitPromise = true
        );

        EvaluateResult result = await driver.Script.EvaluateAsync(parameters);
        // Result will be the resolved JSON data
#endregion
    }

    /// <summary>
    /// Async functions with awaitPromise.
    /// </summary>
    public static async Task AsyncFunctions(
        BiDiDriver driver,
        string contextId)
    {
#region AsyncFunctions
        string functionDefinition = """
            async () => {
                const response = await fetch('/api/data');
                const data = await response.json();
                return data;
            }
            """;

        CallFunctionCommandParameters parameters = new CallFunctionCommandParameters(
            functionDefinition,
            new ContextTarget(contextId),
            true  // awaitPromise = true
        );

        EvaluateResult result = await driver.Script.CallFunctionAsync(parameters);
#endregion
    }

    /// <summary>
    /// Script message event observer.
    /// </summary>
    public static async Task ScriptMessageEvent(
        BiDiDriver driver)
    {
#region ScriptMessageEvent
        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            Console.WriteLine($"Channel: {e.ChannelId}");
            Console.WriteLine($"Data: {e.Data.ValueAs<string>()}");
            Console.WriteLine($"Source context: {e.Source.Context}");
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Script.OnMessage.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }

    /// <summary>
    /// Realm created and destroyed events.
    /// </summary>
    public static void RealmCreatedDestroyed(BiDiDriver driver)
    {
#region RealmCreated/Destroyed
        driver.Script.OnRealmCreated.AddObserver((RealmCreatedEventArgs e) =>
        {
            Console.WriteLine($"Realm created: {e.RealmId}");
            Console.WriteLine($"Type: {e.Type}");
        });

        driver.Script.OnRealmDestroyed.AddObserver((RealmDestroyedEventArgs e) =>
        {
            Console.WriteLine($"Realm destroyed: {e.RealmId}");
        });
#endregion
    }

    /// <summary>
    /// Element interaction pattern - locate and click.
    /// </summary>
    public static async Task ElementInteractionPattern(
        BiDiDriver driver,
        string contextId)
    {
#region ElementInteractionPattern
        // Find element
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(
            new LocateNodesCommandParameters(contextId, new CssLocator("button")));

        RemoteValue element = locateResult.Nodes[0];

        // Click element
        CallFunctionCommandParameters clickParams = new CallFunctionCommandParameters(
            "(element) => element.click()",
            new ContextTarget(contextId),
            false);
        clickParams.Arguments.Add(element.ToSharedReference());
        await driver.Script.CallFunctionAsync(clickParams);
#endregion
    }

    /// <summary>
    /// Get element properties pattern.
    /// </summary>
    public static async Task GetElementPropertiesPattern(
        BiDiDriver driver,
        string contextId,
        SharedReference elementReference)
    {
#region GetElementPropertiesPattern
        // Get multiple properties at once
        string functionDefinition = """
            (element) => ({
                tag: element.tagName,
                text: element.textContent,
                visible: element.offsetParent !== null,
                enabled: !element.disabled,
                value: element.value
            })
            """;

        CallFunctionCommandParameters parameters = new CallFunctionCommandParameters(
            functionDefinition,
            new ContextTarget(contextId),
            false);
        parameters.Arguments.Add(elementReference);

        EvaluateResult result = await driver.Script.CallFunctionAsync(parameters);
#endregion
    }

    /// <summary>
    /// Wait for condition pattern with preload script.
    /// </summary>
    public static async Task WaitForConditionPattern(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region WaitforConditionPattern
        string preloadScript = """
            (channel) => {
                const checkCondition = () => {
                    if (document.querySelector('.loaded')) {
                        channel({ loaded: true });
                    } else {
                        setTimeout(checkCondition, 100);
                    }
                };
                checkCondition();
            }
            """;

        TaskCompletionSource<bool> conditionMet = new TaskCompletionSource<bool>();

        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "conditionChannel")
            {
                conditionMet.SetResult(true);
            }
        });

        ChannelValue channel = new ChannelValue(
            new ChannelProperties("conditionChannel"));

        AddPreloadScriptCommandParameters parameters =
            new AddPreloadScriptCommandParameters(preloadScript);
        parameters.Arguments = new List<ChannelValue> { channel };

        await driver.Script.AddPreloadScriptAsync(parameters);
        await driver.BrowsingContext.NavigateAsync(navParams);

        await conditionMet.Task;
        Console.WriteLine("Condition met!");
#endregion
    }

    /// <summary>
    /// Handling JavaScript exceptions - EvaluateResultException.
    /// </summary>
    public static async Task HandlingJavaScriptExceptions(
        BiDiDriver driver,
        string contextId)
    {
#region HandlingJavaScriptExceptions
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('.missing').textContent",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            string text = success.Result.ValueAs<string>();
            Console.WriteLine($"Text: {text}");
        }
        else if (result is EvaluateResultException exception)
        {
            Console.WriteLine($"JavaScript error: {exception.ExceptionDetails.Text}");
            Console.WriteLine($"Line: {exception.ExceptionDetails.LineNumber}");
            Console.WriteLine($"Column: {exception.ExceptionDetails.ColumnNumber}");
            
            if (exception.ExceptionDetails.StackTrace != null)
            {
                Console.WriteLine("Stack trace:");
                foreach (var frame in exception.ExceptionDetails.StackTrace.CallFrames)
                {
                    Console.WriteLine($"  at {frame.FunctionName} ({frame.Url}:{frame.LineNumber})");
                }
            }
            
            // Handle the error appropriately
        }
#endregion
    }

    /// <summary>
    /// Safe script execution pattern - TryEvaluateAsync.
    /// </summary>
#region TryEvaluateAsync
    public async Task<T?> TryEvaluateAsync<T>(
        BiDiDriver driver,
        string expression,
        string contextId,
        T? defaultValue = default)
    {
        try
        {
            EvaluateResult result = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(
                    expression,
                    new ContextTarget(contextId),
                    true));

            if (result is EvaluateResultSuccess success)
            {
                return success.Result.ValueAs<T>();
            }
            else if (result is EvaluateResultException exception)
            {
                Console.WriteLine($"Script error: {exception.ExceptionDetails.Text}");
                return defaultValue;
            }
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Command error: {ex.Message}");
        }
        
        return defaultValue;
    }
#endregion

    /// <summary>
    /// Usage of TryEvaluateAsync.
    /// </summary>
    public async Task TryEvaluateUsage(
        BiDiDriver driver,
        string contextId)
    {
#region TryEvaluateAsyncUsage
        // Usage
        string? title = await TryEvaluateAsync<string>(
            driver, 
            "document.title", 
            contextId,
            "Unknown");
#endregion
    }
}

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.