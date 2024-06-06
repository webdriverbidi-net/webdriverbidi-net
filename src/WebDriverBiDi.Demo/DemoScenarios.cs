namespace WebDriverBiDi.Demo;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client;
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
        driver.Script.OnMessage.AddHandler(( MessageEventArgs e) =>
        {
            if (e.ChannelId == "delayLoadChannel")
            {
                Console.WriteLine("Received event from preload script");
                syncEvent.Set();
            }

            return Task.CompletedTask;
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
        driver.Network.OnResponseCompleted.AddHandler((ResponseCompletedEventArgs e) =>
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

            return Task.CompletedTask;
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
        driver.Log.OnEntryAdded.AddHandler((EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"This was written to the console at {e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC: {e.Text}");
            return Task.CompletedTask;
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

    public static async Task InterceptBeforeRequestSentEvent(BiDiDriver driver, string baseUrl)
    {
        List<Task> beforeRequestSentTasks = new();
        EventObserver<BeforeRequestSentEventArgs> handler = driver.Network.OnBeforeRequestSent.AddHandler(async (BeforeRequestSentEventArgs e) =>
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

        // Some explanation of this construct is in order. If your event handler does
        // not provide anything to wait on, you run the risk of the main thread exiting
        // (and terminating your pending event handlers) before the event handlers complete
        // execution. Therefore, you need to wait for the event handlers to begin processing
        // before you can wait for them to complete.
        Task.WaitAll(beforeRequestSentTasks.ToArray());
        Console.WriteLine($"Event handlers complete");

        Console.WriteLine("Removing event handler");
        handler.Unobserve();

        navigateParams.Url = $"{baseUrl}/inputForm.html";
        Console.WriteLine($"Navigating again to {navigateParams.Url} show no event handlers fired");
        navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
    }

    public static async Task InterceptAndReplaceNetworkData(BiDiDriver driver, string baseUrl)
    {
        SubscribeCommandParameters subscribe = new();
        subscribe.Events.Add("network.beforeRequestSent");
        await driver.Session.SubscribeAsync(subscribe);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        Console.WriteLine($"Active context: {contextId}");

        AddInterceptCommandParameters addIntercept = new();
        addIntercept.BrowsingContextIds = new List<string>() { contextId };
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.UrlPatterns = new List<UrlPattern>() { new UrlPatternPattern() { PathName = "simpleContent.html" } };
        await driver.Network.AddInterceptAsync(addIntercept);

        Task? substituteTask = null;
        EventObserver<BeforeRequestSentEventArgs> handler = driver.Network.OnBeforeRequestSent.AddHandler((e) =>
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

            return Task.CompletedTask;
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
