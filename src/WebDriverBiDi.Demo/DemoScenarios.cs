namespace WebDriverBiDi.Demo;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Inputs;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

public static class DemoScenarios
{
    /// <summary>
    /// Demonstrates submitting a form.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task SubmitFormAsync(BiDiDriver driver, string baseUrl)
    {
        StatusCommandResult status = await driver.Session.StatusAsync(new StatusCommandParameters());
        Console.WriteLine($"Is ready? {status.IsReady}");

        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add("browsingContext.navigationStarted");
        subscribe.Events.Add("browsingContext.load");
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/inputForm.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        string functionDefinition = @"() => document.querySelector('input[name=""dataToSend""]')";
        CallFunctionCommandParameters callFunctionParams = new(functionDefinition, new ContextTarget(contextId), true);
        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultSuccess scriptSuccessResult)
        {
            Console.WriteLine($"Script result type: {scriptSuccessResult.Result.Type} (.NET type: {scriptSuccessResult.Result.Value!.GetType()})");
            Console.WriteLine($"Script returned element with ID {scriptSuccessResult.Result.SharedId}");

            RemoteValue scriptResultValue = scriptSuccessResult.Result;
            NodeProperties? nodeProperties = scriptResultValue.ValueAs<NodeProperties>();
            if (nodeProperties is not null)
            {
                Console.WriteLine($"Found element on page with local name '{nodeProperties.LocalName}'");
                SharedReference elementReference = scriptResultValue.ToSharedReference();

                InputBuilder inputBuilder = new();
                AddClickOnElementAction(inputBuilder, elementReference);
                AddSendKeysToActiveElementAction(inputBuilder, "Hello WebDriver BiDi" + Keys.Enter);

                PerformActionsCommandParameters actionsParams = new(contextId);
                actionsParams.Actions.AddRange(inputBuilder.Build());
                await driver.Input.PerformActionsAsync(actionsParams);
            }
            else
            {
                Console.WriteLine($"Script result value did not describe a node; returned type {scriptResultValue.Type}");
            }
        }
        else if (scriptResult is EvaluateResultException scriptExceptionResult)
        {
            Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
        }
    }

    /// <summary>
    /// Demonstrates using a preload script to wait for a condition on the page.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task WaitForDelayLoadAsync(BiDiDriver driver, string baseUrl)
    {
        ManualResetEventSlim syncEvent = new(false);
        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "delayLoadChannel")
            {
                Console.WriteLine("Received event from preload script");
                syncEvent.Set();
            }
        });

        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add("browsingContext.navigationStarted");
        subscribe.Events.Add("browsingContext.load");
        subscribe.Events.Add("script.message");
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        string preloadScriptFunction = @"
        (channel) => {
                const interval = setInterval(() => {
                if (document.customLatch) {
                    clearInterval(interval);
                    channel({'customLatchValue': document.customLatch});
                }
            }, 1000);
        }";
        ChannelValue channelValue = new(new ChannelProperties("delayLoadChannel"));
        AddPreloadScriptCommandParameters preloadScriptParameters = new(preloadScriptFunction)
        {
            Arguments = [channelValue],
        };
        await driver.Script.AddPreloadScriptAsync(preloadScriptParameters);

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/delayLoadPage.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        bool eventTriggered = syncEvent.Wait(TimeSpan.FromSeconds(10));
        Console.WriteLine($"Event triggered from preload script: {eventTriggered}");

        string functionDefinition = @"() => {
            const element = document.querySelector('div#contentMessage');
            return window.getComputedStyle(element).backgroundColor;
        }";
        CallFunctionCommandParameters callFunctionParams = new(functionDefinition, new ContextTarget(contextId), true);
        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultSuccess scriptSuccessResult)
        {
            Console.WriteLine($"Script result type: {scriptSuccessResult.Result.Type} (.NET type: {scriptSuccessResult.Result.Value!.GetType()})");
            RemoteValue scriptResultValue = scriptSuccessResult.Result;
            Console.WriteLine($"Element background color is {scriptResultValue.ValueAs<string>()}");
        }
    }

    /// <summary>
    /// Demonstrates getting information about the network traffic.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task MonitorNetworkTraffic(BiDiDriver driver, string baseUrl)
    {
        driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            if (e.Response.Url.Contains(".html") || e.Request.Url.EndsWith('/'))
            {
                if (e.Response.Status >= 300 && e.Response.Status < 400)
                {
                    Console.WriteLine($"Request was redirected from {e.Request.Url} to {e.Response.Url} with response code {e.Response.Status}");
                }
                else
                {
                    Console.WriteLine($"Response code for {e.Response.Url}: {e.Response.Status}");
                }
            }
        });
        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add("network.responseCompleted");
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        List<string> testUrls = new()
        {
            "",
            "simpleContent.html",
            "missingPage.html",
        };

        foreach (string testUrl in testUrls)
        {
            NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/{testUrl}")
            {
                Wait = ReadinessState.Complete
            };
            NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        }
    }

    /// <summary>
    /// Demonstrates watching for entries in the browser console.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task MonitorBrowserConsole(BiDiDriver driver, string baseUrl)
    {
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"This was written to the console at {e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC: {e.Text}");
        });
        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add("log.entryAdded");
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/consoleLogPage.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");
    }

    /// <summary>
    /// Demonstrates execution of JavaScript functions.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task ExecuteJavaScriptFunctions(BiDiDriver driver, string baseUrl)
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        string functionDefinition = "(first, second) => first + second";
        List<ArgumentValue> arguments = new()
        {
            LocalValue.Number(3),
            LocalValue.Number(5),
        };

        CallFunctionCommandParameters callFunctionParams = new(functionDefinition, new ContextTarget(contextId), true);
        callFunctionParams.Arguments.AddRange(arguments);
        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultSuccess scriptSuccessResultNumber)
        {
            Console.WriteLine($"Script result type: {scriptSuccessResultNumber.Result.Type} (.NET type: {scriptSuccessResultNumber.Result.Value!.GetType()})");
            RemoteValue scriptResultValue = scriptSuccessResultNumber.Result;
            Console.WriteLine($"Return value of function is {scriptResultValue.ValueAs<long>()}");
        }

        arguments = new()
        {
            LocalValue.String("Hello, "),
            LocalValue.String("World!"),
        };

        callFunctionParams = new(functionDefinition, new ContextTarget(contextId), true);
        callFunctionParams.Arguments.AddRange(arguments);
        scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultSuccess scriptSuccessResultString)
        {
            Console.WriteLine($"Script result type: {scriptSuccessResultString.Result.Type} (.NET type: {scriptSuccessResultString.Result.Value!.GetType()})");
            RemoteValue scriptResultValue = scriptSuccessResultString.Result;
            Console.WriteLine($"Return value of function is {scriptResultValue.ValueAs<string>()}");
        }
    }

    /// <summary>
    /// Demonstrates usage of a preload script, including calling JavaScript added by the preload
    /// script after addition.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task ExecutePreloadScript(BiDiDriver driver, string baseUrl)
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        // A preload script will be executed during every navigation, ensuring
        // that the data structures are available before the page's JavaScript
        // (if any) is executed. A special note here, we are using the sandbox
        // feature to isolate the added JS from running code on the page, so as
        // not to pollute the global objects.
        AddPreloadScriptCommandParameters preloadScriptParams = new("() => window.bidi = { getTagName: (e) => e.tagName }")
        {
            Sandbox = "webdriverbidi",
        };
        AddPreloadScriptCommandResult addScriptResult = await driver.Script.AddPreloadScriptAsync(preloadScriptParams);
        string preloadScriptId = addScriptResult.PreloadScriptId;

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(new LocateNodesCommandParameters(contextId, new CssLocator(".text")));
        RemoteValue node = locateResult.Nodes[0];

        // This function will access the "bidi" object added to its window object
        // and execute the defined function. Calling the function without specifying
        // the proper sandbox will result in an error, as the object will be undefined.
        Console.WriteLine("Executing function without specifying using sandbox");
        string functionDefinition = "(e) => window.bidi.getTagName(e)";
        ContextTarget contextTarget = new(contextId);
        CallFunctionCommandParameters callFunctionParams = new(functionDefinition, contextTarget, true);
        callFunctionParams.Arguments.Add(node.ToSharedReference());

        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultException scriptExceptionResult)
        {
            Console.WriteLine($"Script exception received: {scriptExceptionResult.ExceptionDetails.Text}");
        }

        // Calling the function with the proper sandbox will yield a proper result.
        contextTarget.Sandbox = "webdriverbidi";

        Console.WriteLine("Executing function with specifying using sandbox");
        scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultSuccess scriptSuccessResult)
        {
            Console.WriteLine($"Script result type: {scriptSuccessResult.Result.Type} (.NET type: {scriptSuccessResult.Result.Value!.GetType()})");
            RemoteValue scriptResultValue = scriptSuccessResult.Result;
            Console.WriteLine($"Return value of function is {scriptResultValue.ValueAs<string>()}");
        }

        await driver.Script.RemovePreloadScriptAsync(new RemovePreloadScriptCommandParameters(preloadScriptId));

        navigateParams = new(contextId, $"{baseUrl}/inputForm.html")
        {
            Wait = ReadinessState.Complete
        };
        navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        locateResult = await driver.BrowsingContext.LocateNodesAsync(new LocateNodesCommandParameters(contextId, new CssLocator("h1")));
        node = locateResult.Nodes[0];

        // Calling the function again after removing the preload script and another navigation
        // yields an exception that the object can't be found, because the preload script didn't
        // create it on the new page after navigation.
        Console.WriteLine("Executing function after removing preload script");
        callFunctionParams.Arguments.Clear();
        callFunctionParams.Arguments.Add(node.ToSharedReference());
        scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultException removedScriptExceptionResult)
        {
            Console.WriteLine($"Script exception received: {removedScriptExceptionResult.ExceptionDetails.Text}");
        }
    }

    /// <summary>
    /// Demonstrates the ability to round-trip an element for manipulation using JavaScript
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task ExecuteElementRoundtripInJavaScript(BiDiDriver driver, string baseUrl)
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/inputForm.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);

        string firstFunctionDefinition = @"() => document.querySelector('input[name=""dataToSend""]')";
        CallFunctionCommandParameters callFunctionParams = new(firstFunctionDefinition, new ContextTarget(contextId), true);
        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        RemoteValue? elementResultValue = null;
        if (scriptResult is EvaluateResultSuccess firstScriptSuccessResult)
        {
            Console.WriteLine($"Script result type: {firstScriptSuccessResult.Result.Type} (.NET type: {firstScriptSuccessResult.Result.Value!.GetType()})");
            Console.WriteLine($"Script returned element with ID {firstScriptSuccessResult.Result.SharedId}");

            elementResultValue = firstScriptSuccessResult.Result;
        }
        else if (scriptResult is EvaluateResultException scriptExceptionResult)
        {
            Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
        }

        string secondFunctionDefinition = @"(element) => element.tagName";
        callFunctionParams = new(secondFunctionDefinition, new ContextTarget(contextId), true);
        callFunctionParams.Arguments.Add(elementResultValue!.ToSharedReference());
        scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultSuccess secondScriptSuccessResult)
        {
            Console.WriteLine($"Script result type: {secondScriptSuccessResult.Result.Type} (.NET type: {secondScriptSuccessResult.Result.Value!.GetType()})");
        }
        else if (scriptResult is EvaluateResultException scriptExceptionResult)
        {
            Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
        }

    }

    /// <summary>
    /// Demonstrates interception of network calls before the request is sent to the web server.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task InterceptBeforeRequestSentEvent(BiDiDriver driver, string baseUrl)
    {
        List<Task> beforeRequestSentTasks = new();
        EventObserver<BeforeRequestSentEventArgs> observer = driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            TaskCompletionSource taskCompletionSource = new();
            beforeRequestSentTasks.Add(taskCompletionSource.Task);
            Console.WriteLine($"Entering BeforeRequestSent event");

            // Here we ara creating an artificially long-running event handler
            // to demonstrate how to handle this in client code.
            await Task.Delay(TimeSpan.FromSeconds(4));
            Console.WriteLine($"Before request for {e.Request.Url}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Exiting BeforeRequestSent event");
            taskCompletionSource.SetResult();
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add("network.beforeRequestSent");
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Navigation command completed");

        // Some explanation of this construct is in order. The long-running event handlers
        // included here will exceed the allowable time for the navigation to complete.
        // Therefore, you need to run the handlers asynchronously, with some sort of
        // external synchronization mechanism. Otherwise, either the navigation command
        // will fail with a timeout, or the main thread will complete and exit before the
        // event handlers complete their execution. Note carefully that this also implies
        // that the event handlers will run in parallel. In this case, we've chosen to use
        // Tasks as the synchronization mechanism, so that we can wait for all of them to
        // complete before continuing.
        Task.WaitAll(beforeRequestSentTasks.ToArray());
        Console.WriteLine($"Event handlers complete");

        // Demonstrate the ability to remove the event handler, and that the event handler
        // does not get called, even though the driver is still receiving the event data
        // from the browser. The traffic from the browser can be seen by setting the log
        // level to Debug instead of its default of Info.
        Console.WriteLine("Removing event handler");
        observer.Unobserve();

        navigateParams.Url = $"{baseUrl}/inputForm.html";
        Console.WriteLine($"Navigating again to {navigateParams.Url} show no event handlers fired");
        navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
    }

    /// <summary>
    /// Demonstrates the ability to intercept a network call and replace it with custom, user-defined
    /// content.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task InterceptAndReplaceNetworkData(BiDiDriver driver, string baseUrl)
    {
        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add("network.beforeRequestSent");
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        AddInterceptCommandParameters addIntercept = new()
        {
            BrowsingContextIds = new List<string>() { contextId }
        };
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.UrlPatterns = new List<UrlPattern>() { new UrlPatternPattern() { PathName = "simpleContent.html" } };
        await driver.Network.AddInterceptAsync(addIntercept);

        // Calling a command within an event observer must be done asynchronously.
        // This is because the driver transport processes incoming messages one
        // at a time. Sending the command involves receiving the command result
        // message, which cannot be processed until processing of this event
        // message is completed. To ensure that we can wait for the command
        // response to be completely processed, capture the Task returned by the
        // command execution, and await its completion later.
        Task? substituteTask = null;
        EventObserver<BeforeRequestSentEventArgs> observer = driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            if (e.IsBlocked)
            {
                ProvideResponseCommandParameters provideResponse = new(e.Request.RequestId)
                {
                    StatusCode = 200,
                    ReasonPhrase = "OK",
                    Body = BytesValue.FromString($"<html><body><h1>Request to {e.Request.Url} has been hijacked!</h1></body></html>")
                };
                substituteTask = driver.Network.ProvideResponseAsync(provideResponse);
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        await substituteTask!;

        Console.WriteLine($"Navigation command completed");
    }

    private static void AddClickOnElementAction(InputBuilder builder, SharedReference elementReference)
    {
        builder.AddAction(builder.DefaultPointerInputSource.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(elementReference))))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerDown(PointerButton.Left))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerUp());
    }

    private static void AddSendKeysToActiveElementAction(InputBuilder builder, string keysToSend)
    {
        foreach (char character in keysToSend)
        {
            builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown(character))
                .AddAction(builder.DefaultKeyInputSource.CreateKeyUp(character));
        }
    }
}
