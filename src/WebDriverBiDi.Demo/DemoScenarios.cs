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
                AddSendKeysToElementAction(inputBuilder, elementReference, "Hello WebDriver BiDi" + Keys.Enter);

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
        driver.Script.Message += (object? obj, MessageEventArgs e) =>
        {
            if (e.ChannelId == "delayLoadChannel")
            {
                Console.WriteLine("Received event from preload script");
                syncEvent.Set();
            }
        };

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
        driver.Network.ResponseCompleted += (object? obj, ResponseCompletedEventArgs e) =>
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
        };
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
        driver.Log.EntryAdded += (object? obj, EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"This was written to the console at {e.Timestamp:yyyy-MM-dd HH:mm:ss.fff)} UTC: {e.Text}");
        };
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

    private static void AddClickOnElementAction(InputBuilder builder, SharedReference elementReference)
    {
        builder.AddAction(builder.DefaultPointerInputSource.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(elementReference))))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerDown(PointerButton.Left))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerUp());
    }

    private static void AddSendKeysToElementAction(InputBuilder builder, SharedReference elementReference, string keysToSend)
    {
        foreach (char character in keysToSend)
        {
            builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown(character))
                .AddAction(builder.DefaultKeyInputSource.CreateKeyUp(character));
        }
    }
}