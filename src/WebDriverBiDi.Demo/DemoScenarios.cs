namespace WebDriverBiDi.Demo;

using System.ComponentModel.Design;
using System.Text;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Elements;
using WebDriverBiDi.Client.Inputs;
using WebDriverBiDi.Client.Network;
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
        subscribe.Events.Add(driver.BrowsingContext.OnNavigationStarted.EventName);
        subscribe.Events.Add(driver.BrowsingContext.OnLoad.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/inputForm.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
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
                inputBuilder.AddClickOnElementAction(elementReference);
                inputBuilder.AddSendKeysToActiveElementAction("Hello WebDriver BiDi" + Keys.Enter);

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
        EventObserver<MessageEventArgs> observer = driver.Script.OnMessage.AddObserver((e) =>
        {
            if (e.ChannelId == "delayLoadChannel")
            {
                Console.WriteLine("Received event from preload script");
            }
        });

        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add(driver.BrowsingContext.OnNavigationStarted.EventName);
        subscribe.Events.Add(driver.BrowsingContext.OnLoad.EventName);
        subscribe.Events.Add(driver.Script.OnMessage.EventName);
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

        observer.SetCheckpoint();
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/delayLoadPage.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        bool eventTriggered = observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));
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
        subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        List<string> testUrls =
        [
            "",
            "simpleContent.html",
            "missingPage.html",
        ];

        foreach (string testUrl in testUrls)
        {
            NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/{testUrl}")
            {
                Wait = ReadinessState.Complete
            };
            NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
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
        subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/consoleLogPage.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
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
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        string functionDefinition = "(first, second) => first + second";
        List<ArgumentValue> arguments =
        [
            LocalValue.Number(3),
            LocalValue.Number(5),
        ];

        CallFunctionCommandParameters callFunctionParams = new(functionDefinition, new ContextTarget(contextId), true);
        callFunctionParams.Arguments.AddRange(arguments);
        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        if (scriptResult is EvaluateResultSuccess scriptSuccessResultNumber)
        {
            Console.WriteLine($"Script result type: {scriptSuccessResultNumber.Result.Type} (.NET type: {scriptSuccessResultNumber.Result.Value!.GetType()})");
            RemoteValue scriptResultValue = scriptSuccessResultNumber.Result;
            Console.WriteLine($"Return value of function is {scriptResultValue.ValueAs<long>()}");
        }

        arguments =
        [
            LocalValue.String("Hello, "),
            LocalValue.String("World!"),
        ];

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
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
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
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);

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

        if (elementResultValue is not null)
        {
            string secondFunctionDefinition = @"(element) => `The element tag name is ${element.tagName}`";
            callFunctionParams = new(secondFunctionDefinition, new ContextTarget(contextId), true);
            callFunctionParams.Arguments.Add(elementResultValue.ToSharedReference());
            scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
            if (scriptResult is EvaluateResultSuccess secondScriptSuccessResult)
            {
                Console.WriteLine($"Script result type: {secondScriptSuccessResult.Result.Type} (.NET type: {secondScriptSuccessResult.Result.Value!.GetType()})");
                Console.WriteLine($"Script result: {secondScriptSuccessResult.Result.ValueAs<string>()}");
            }
            else if (scriptResult is EvaluateResultException scriptExceptionResult)
            {
                Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
            }
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
        // List<Task> beforeRequestSentTasks = new();
        EventObserver<BeforeRequestSentEventArgs> observer = driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            TaskCompletionSource taskCompletionSource = new();
            // beforeRequestSentTasks.Add(taskCompletionSource.Task);
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
        subscribe.Events.Add(driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        // Navigating to simpleContent.html generates 5 requests, one for the HTML page itself,
        // two for CSS stylesheets, one for a JavaScript script file, and one for an image.
        observer.SetCheckpoint(5);
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
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
        observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));
        Task.WaitAll(observer.GetCheckpointTasks());
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
        subscribe.Events.Add(driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        AddInterceptCommandParameters addIntercept = new()
        {
            BrowsingContextIds = [contextId]
        };
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.UrlPatterns = [new UrlPatternPattern() { PathName = "simpleContent.html" }];
        await driver.Network.AddInterceptAsync(addIntercept);

        // Calling a command within an event observer must be done asynchronously.
        // This is because the driver transport processes incoming messages one
        // at a time. Sending the command involves receiving the command result
        // message, which cannot be processed until processing of this event
        // message is completed. To ensure that we can wait for the command
        // response to be completely processed, capture the Task returned by the
        // command execution, and await its completion later. We do not need an
        // external data structure because we do not need any information from the
        // return of the command spawned in the handler; we only need to know that
        // the handler was executed.
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
                return driver.Network.ProvideResponseAsync(provideResponse);
            }

            return Task.CompletedTask;
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // OnBeforeRequestSent will only be raised once, since we are hijacking the
        // request, and providing a response that does not require any other requests
        // to the web server.
        observer.SetCheckpoint();
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        observer.WaitForCheckpoint(TimeSpan.FromSeconds(3));
        Task.WaitAll(observer.GetCheckpointTasks());

        Console.WriteLine($"Navigation command completed");
    }

    /// <summary>
    /// Demonstrates capturing a network response, including the response body.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task CaptureNetworkResponse(BiDiDriver driver, string baseUrl)
    {
        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        // Allocate a small amount of memory for this demonstration.
        // Actual practice would probably require a larger allocation
        // for data collection.
        AddDataCollectorCommandParameters addCollectorParameters = new(Convert.ToUInt64(Math.Pow(2, 24)));
        addCollectorParameters.BrowsingContexts.Add(contextId);
        AddDataCollectorCommandResult collectorResult = await driver.Network.AddDataCollectorAsync(addCollectorParameters);
        string collectorId = collectorResult.CollectorId;

        // Calling a command within an event observer must be done asynchronously.
        // This is because the driver transport processes incoming messages one
        // at a time. Sending the command involves receiving the command result
        // message, which cannot be processed until processing of this event
        // message is completed. To ensure that we can wait for the command
        // response to be completely processed, capture the Task returned by the
        // command execution, and await its completion later.
        string responseStartLine = string.Empty;
        List<ReadOnlyHeader> responseHeaders = [];
        EventObserver<ResponseCompletedEventArgs> observer = driver.Network.OnResponseCompleted.AddObserver((e) =>
        {
            // Limit processing to the retrieval just of the HTML file.
            if (e.Response.Url.Contains("simpleContent.html"))
            {
                responseStartLine = $"{e.Response.Protocol} {e.Response.Status} {e.Response.StatusText}";
                responseHeaders.AddRange(e.Response.Headers);

                // Be a good citizen and release the collected data when collected.
                GetDataCommandParameters getDataParameters = new(e.Request.RequestId)
                {
                    CollectorId = collectorId,
                    DisownCollectedData = true,
                };
                return driver.Network.GetDataAsync(getDataParameters);
            }

            return Task.CompletedTask;
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // Navigating to simpleContent.html generates 5 responses, one for the HTML page itself,
        // two for CSS stylesheets, one for a JavaScript script file, and one for an image.
        observer.SetCheckpoint(5);
        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);

        bool checkpointFulfilled = observer.WaitForCheckpoint(TimeSpan.FromSeconds(3));
        if (!checkpointFulfilled)
        {
            Console.WriteLine("Error: Checkpoint not fulfilled");
            return;
        }

        // Only one task in the captured tasks will be from the Network.getData command,
        // which is the task that retrieves the response body, so find that Task in the
        // captured tasks array, and cast it to the correct type so we can await that task's
        // result.
        Task[] checkpointTasks = observer.GetCheckpointTasks();
        Task<GetDataCommandResult>[] bodyRetrievalTasks = checkpointTasks.FilterTasksForType<GetDataCommandResult>();
        if (bodyRetrievalTasks.Length == 0)
        {
            Console.WriteLine("Error: Body retrieval failed");
        }
        else if (bodyRetrievalTasks.Length > 1)
        {
            Console.WriteLine("Error: Too many body retrieval tasks found");
        }
        else
        {
            Task<GetDataCommandResult> bodyRetrievalTask = bodyRetrievalTasks[0];
            await bodyRetrievalTask;
            BytesValue bodyResult = bodyRetrievalTask.Result.Bytes;
            string bodyText = string.Empty;
            if (bodyResult.Type == BytesValueType.Base64)
            {
                bodyText = Encoding.UTF8.GetString(Convert.FromBase64String(bodyResult.Value));
            }
            else
            {
                bodyText = bodyResult.Value;
            }

            Console.WriteLine($"Response received for {baseUrl}/simpleContent.html");
            Console.WriteLine(responseStartLine);
            foreach (ReadOnlyHeader header in responseHeaders)
            {
                Console.WriteLine($"{header.Name}: {header.Value.Value}");
            }
            Console.WriteLine();
            Console.WriteLine(bodyText);
        }
    }
    
    /// <summary>
    /// Demonstrates an approach to capturing all network traffic during a session, including request and response bodies.
    /// </summary>
   /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task CaptureAllNetworkTraffic(BiDiDriver driver, string baseUrl)
    {
        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add(driver.BrowsingContext.OnNavigationStarted.EventName);
        subscribe.Events.Add(driver.BrowsingContext.OnLoad.EventName);
        SubscribeCommandResult subscribeResult = await driver.Session.SubscribeAsync(subscribe);
        string navigationSubscriptionId = subscribeResult.SubscriptionId;

        EventObserver<NavigationEventArgs> navigationObserver = driver.BrowsingContext.OnLoad.AddObserver((e) => { });

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        // We will use a NetworkTrafficMonitor class from the WebDriverBidi client library.
        // It encapsulates the logic for collecting the network traffic. All of the concepts
        // used by that class are demonstrated in standalone form by other scenarios in this
        // class, so they are not explicitly performed here.
        NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(contextId);

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/inputForm.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
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
                inputBuilder.AddClickOnElementAction(elementReference);
                inputBuilder.AddSendKeysToActiveElementAction("Hello WebDriver BiDi" + Keys.Enter);

                PerformActionsCommandParameters actionsParams = new(contextId);
                actionsParams.Actions.AddRange(inputBuilder.Build());

                navigationObserver.SetCheckpoint();
                await driver.Input.PerformActionsAsync(actionsParams);
                bool navigationCompleted = navigationObserver.WaitForCheckpoint(TimeSpan.FromSeconds(3));
                if (!navigationCompleted)
                {
                    Console.WriteLine("Navigation completion not detected within three seconds");
                }

                List<NetworkRequest> postRequests = await monitor.GetCapturedTrafficAsync();
                Console.WriteLine($"Captured {postRequests.Count} requests");
                foreach (NetworkRequest request in postRequests)
                {
                    Console.WriteLine($"Traffic from {request.Url} (request id: {request.RequestId}):");
                    Console.WriteLine("------- Begin Request Content -------");
                    Console.Write(request.GetRequestText());
                    Console.WriteLine("------- End Request Content -------");
                    Console.WriteLine();
                    Console.WriteLine("------- Begin Response Content -------");
                    Console.Write(request.GetResponseText());
                    Console.WriteLine("------- End Response Content -------");
                }
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

        navigateParams = new(contextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete
        };
        navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");
        List<NetworkRequest> navigationRequests = await monitor.GetCapturedTrafficAsync();
        Console.WriteLine($"Captured {navigationRequests.Count} requests");
        foreach (NetworkRequest request in navigationRequests)
        {
            Console.WriteLine($"Traffic from {request.Url} (request id: {request.RequestId}):");
            Console.WriteLine("------- Begin Request Content -------");
            Console.Write(request.GetRequestText());
            Console.WriteLine("------- End Request Content -------");
            Console.WriteLine();
            Console.WriteLine("------- Begin Response Content -------");
            Console.Write(request.GetResponseText());
            Console.WriteLine("------- End Response Content -------");
        }

        Console.WriteLine("Stopping network traffic monitor");
        await monitor.StopMonitoringAsync();
        Console.WriteLine("Network traffic monitor stopped");

        UnsubscribeByIdsCommandParameters unsubscribe = new();
        unsubscribe.SubscriptionIds.Add(navigationSubscriptionId);
        await driver.Session.UnsubscribeAsync(unsubscribe);
    }

    /// <summary>
    /// Sample scenario showing addition of a state inspector.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task InteractWithComplexWebPage(BiDiDriver driver, string baseUrl)
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        // We will use an ElementStateInspector class from the WebDriverBidi client library.
        // It encapsulates the logic for querying element states. All of the concepts
        // used by that class, including executing JavaScript functions, using a sandbox
        // to isolate driver script execution from the DOM of the page, and using a preload
        // script, are demonstrated in standalone form by other scenarios in this class,
        // so they are not explicitly performed here.
        ElementStateInspector inspector = new(driver);
        await inspector.AddInspectorAsync();

        NavigateCommandParameters navigateParams = new(contextId, $"{baseUrl}/complexContent.html")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
        Console.WriteLine($"Performed navigation to {navigation.Url}");

        LocateNodesCommandResult locateButtonResult = await driver.BrowsingContext.LocateNodesAsync(new LocateNodesCommandParameters(contextId, new CssLocator("#toggle-button")));
        RemoteValue toggleButtonNode = locateButtonResult.Nodes[0];
        SharedReference toggleButton = toggleButtonNode.ToSharedReference();

        // This function will access the "webdriverInspector" object added to its window object
        // and execute the defined function. We must use the sandbox for the global property
        // name to be recognized. The function being called should not return unless the
        // element being queried is available for interaction (has stable position, is visible,
        // is enabled, and is in the view port).
        Console.WriteLine("Executing function to wait for button to be ready for interaction");
        try
        {
            await inspector.WaitForInteractionReadyAsync(contextId, toggleButton, InteractionType.Click, TimeSpan.FromSeconds(10));
        }
        catch (WebDriverBiDiException e)
        {
            Console.WriteLine($"Wait was unsuccessful, error returned was '{e.Message}'");
            return;
        }

        Console.WriteLine("Wait for element ready for interaction was successful");

        LocateNodesCommandResult locateImageResult = await driver.BrowsingContext.LocateNodesAsync(new LocateNodesCommandParameters(contextId, new CssLocator("img")));
        RemoteValue imageElementNode = locateImageResult.Nodes[0];
        SharedReference imageElement = imageElementNode.ToSharedReference();

        // Check the visibility of the image element on the page (it should be invisible).
        // Note that we can reuse the ContextTarget that we used for interaction wait script.
        Console.WriteLine("Executing first check for image element visibility");
        bool isImageVisible = await inspector.IsElementVisibleAsync(contextId, imageElement);
        Console.WriteLine($"Image element visible? {isImageVisible}");

        // Click the button to toggle the image visibility.
        InputBuilder inputBuilder = new();
        inputBuilder.AddClickOnElementAction(toggleButton);

        PerformActionsCommandParameters actionsParams = new(contextId);
        actionsParams.Actions.AddRange(inputBuilder.Build());
        await driver.Input.PerformActionsAsync(actionsParams);

        // Check the visibility of the image element on the page again (it should be visible now).
        // Note that we can simply reuse the parameters to CallFunctionAsync; no need to create
        // another parameter object.
        Console.WriteLine("Executing second check for image element visibility");
        isImageVisible = await inspector.IsElementVisibleAsync(contextId, imageElement);
        Console.WriteLine($"Image element visible? {isImageVisible}");

        // Removing the preload script is optional, especially since this is the
        // end of the demo method, but it's put here for completeness.
        await inspector.RemoveInspectorAsync();
    }

    /// <summary>
    /// Demonstrates how a test would discriminate between events subscribed to by different
    /// user contexts.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance to use to drive the browser.
    /// It is assumed the driver is initialized and connected to the remote end.</param>
    /// <param name="baseUrl">The base URL to the web server.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static async Task HandleEventsInMultipleUserContexts(BiDiDriver driver, string baseUrl)
    {
        Dictionary<string, string> userContextMap = [];
        EventObserver<BrowsingContextEventArgs> observer = driver.BrowsingContext.OnContextCreated.AddObserver((e) => userContextMap[e.BrowsingContextId] = e.UserContextId);
        SubscribeCommandResult contextCreatedSubscribeResult = await driver.Session.SubscribeAsync(new SubscribeCommandParameters([driver.BrowsingContext.OnContextCreated.EventName]));
        string contextCreatedSubscriptionId = contextCreatedSubscribeResult.SubscriptionId;

        // Get default browsing context
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string defaultBrowsingContextId = tree.ContextTree[0].BrowsingContextId;

        // Create first user context
        CreateUserContextCommandResult createFirstUserContextResult = await driver.Browser.CreateUserContextAsync(new());
        string firstUserContextId = createFirstUserContextResult.UserContextId;

        // Create first browsing context
        CreateCommandResult createFirstBrowsingContextResult = await driver.BrowsingContext.CreateAsync(new CreateCommandParameters(CreateType.Tab)
        {
            UserContextId = firstUserContextId,
        });
        string firstBrowsingContextId = createFirstBrowsingContextResult.BrowsingContextId;

        // Create second user context
        CreateUserContextCommandResult createSecondUserContextResult = await driver.Browser.CreateUserContextAsync(new());
        string secondUserContextId = createSecondUserContextResult.UserContextId;

        // Create second browsing context
        CreateCommandResult createSecondBrowsingContextResult = await driver.BrowsingContext.CreateAsync(new CreateCommandParameters(CreateType.Tab)
        {
            UserContextId = secondUserContextId,
        });
        string secondBrowsingContextId = createSecondBrowsingContextResult.BrowsingContextId;

        // Close the default browsing context; we don't need it anymore
        await driver.BrowsingContext.CloseAsync(new BrowsingContext.CloseCommandParameters(defaultBrowsingContextId));

        // Simulate first test event subscription
        EventObserver<BeforeRequestSentEventArgs> firstNetworkObserver = driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            // Using the first browsing context ID, get the user context it belongs to.
            // Limit the console writing to only the HTML page, so as not to clutter things up.
            if (e.BrowsingContextId == firstBrowsingContextId && userContextMap.TryGetValue(e.BrowsingContextId, out string? value) && e.Request.Url.EndsWith(".html"))
            {
                Console.WriteLine($"I came from the first user context (ID: {value}). I'm navigating to {e.Request.Url}");
            }
        });
        SubscribeCommandParameters firstNetworkSubscribeParameters = new([driver.Network.OnBeforeRequestSent.EventName]);
        firstNetworkSubscribeParameters.UserContexts.Add(firstUserContextId);
        SubscribeCommandResult firstNetworkSubscriptionResult = await driver.Session.SubscribeAsync(firstNetworkSubscribeParameters);
        string firstNetworkSubscriptionId = firstNetworkSubscriptionResult.SubscriptionId;

        // Simulate second test event subscription
        EventObserver<BeforeRequestSentEventArgs> secondNetworkObserver = driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            // Using the second browsing context ID, get the user context it belongs to.
            // Limit the console writing to only the HTML page, so as not to clutter things up.
            if (e.BrowsingContextId == secondBrowsingContextId && userContextMap.TryGetValue(e.BrowsingContextId, out string? value) && e.Request.Url.EndsWith(".html"))
            {
                Console.WriteLine($"I came from the second user context (ID: {value}). I'm navigating to {e.Request.Url}");
            }
        });
        SubscribeCommandParameters secondNetworkSubscribeParameters = new([driver.Network.OnBeforeRequestSent.EventName]);
        secondNetworkSubscribeParameters.UserContexts.Add(secondUserContextId);
        SubscribeCommandResult secondNetworkSubscriptionResult = await driver.Session.SubscribeAsync(secondNetworkSubscribeParameters);
        string secondNetworkSubscriptionId = secondNetworkSubscriptionResult.SubscriptionId;

        // Perform the navigation operations asynchronously, to simulate parallel test runs.
        Task firstNavigationTask = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters(firstBrowsingContextId, $"{baseUrl}/simpleContent.html")
        {
            Wait = ReadinessState.Complete,
        });
        Task secondNavigationTask = driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters(secondBrowsingContextId, $"{baseUrl}/inputForm.html")
        {
            Wait = ReadinessState.Complete,
        });
        Task.WaitAll([firstNavigationTask, secondNavigationTask]);
    }
}
