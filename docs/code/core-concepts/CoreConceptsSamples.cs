// <copyright file="CoreConceptsSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/core-concepts.md

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS0168 // Variable declared but never used
#pragma warning disable CS0219 // Variable assigned but never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace WebDriverBiDi.Docs.Code.CoreConcepts;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;
using WebDriverBiDi.UserAgentClientHints;

/// <summary>
/// Snippets for core concepts documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class CoreConceptsSamples
{
    /// <summary>
    /// BiDiDriver creation and basic lifecycle.
    /// </summary>
    public static async Task DriverCreationAndLifecycle()
    {
        #region DriverCreationandLifecycle
        // Create a driver with default timeout (60 seconds)
        BiDiDriver driver = new BiDiDriver();

        // Create a driver with a specific command timeout
        BiDiDriver driverWithTimeout = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Start the connection
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Stop the connection when done
        await driver.StopAsync();
        #endregion
    }

    public class MyCustomModule : Module
    {
        public MyCustomModule(IBiDiCommandExecutor driver) : base(driver) { }
        public override string ModuleName => throw new NotImplementedException();
    }

    /// <summary>
    /// Driver lifecycle with IsStarted check.
    /// </summary>
    public static async Task DriverLifecycleWithCheck(
        MyCustomModule customModule,
        string webSocketUrl,
        NavigateCommandParameters navParams)
    {
        #region DriverLifecycle
        // 1. Create driver
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // 2. Register modules and event handlers BEFORE starting
        driver.RegisterModule(customModule);
        driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));

        // 3. Start the driver
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // 4. Check if started
        if (driver.IsStarted)
        {
            // Execute commands...
            await driver.BrowsingContext.NavigateAsync(navParams);
        }

        // 5. Stop when done
        await driver.StopAsync();
        #endregion
    }

    public static async Task IsStartedCheck(BiDiDriver driver, string webSocketUrl, NavigateCommandParameters navParams)
    {
        #region DriverLifecyclewithCheck
        if (!driver.IsStarted)
        {
            await driver.StartAsync(webSocketUrl);
        }

        // Execute commands only when started
        if (driver.IsStarted)
        {
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        #endregion
    }

    /// <summary>
    /// Timing restrictions - correct: register before starting.
    /// </summary>
    public static async Task TimingRestrictionsCorrect(string webSocketUrl)
    {
        #region TimingRestrictions-Correct
        // ✅ CORRECT: Register before starting
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        driver.RegisterModule(new CustomModule(driver));
        driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));

        await driver.StartAsync(webSocketUrl);
        #endregion
    }

    /// <summary>
    /// Timing restrictions - wrong: cannot register after starting.
    /// </summary>
    public static async Task TimingRestrictionsWrong(string webSocketUrl)
    {
        #region TimingRestrictions-Wrong
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);

        // ❌ WRONG: Cannot register after starting - This will throw an exception!
        driver.RegisterModule(new CustomModule(driver));
        driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));
        #endregion
    }

    /// <summary>
    /// Thread safety - Parallel.Invoke for RegisterModule.
    /// </summary>
    public static void ThreadSafeRegistration(BiDiDriver driver)
    {
        #region ThreadSafeRegistration
        CoreConceptsDocModule customModule1 = new CoreConceptsDocModule(driver);
        CoreConceptsDocModule customModule2 = new CoreConceptsDocModule(driver);

        // This is safe - concurrent registration is handled properly
        Parallel.Invoke(
            () => driver.RegisterModule(customModule1),
            () => driver.RegisterModule(customModule2));
        #endregion
    }

    /// <summary>
    /// Command timeout configuration.
    /// </summary>
    public static void CommandTimeoutConfiguration()
    {
        #region CommandTimeoutConfiguration
        // Default timeout (60 seconds)
        BiDiDriver defaultDriver = new BiDiDriver();

        // Custom timeout (30 seconds)
        BiDiDriver customDriver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Short timeout for fast operations
        BiDiDriver shortDriver = new BiDiDriver(TimeSpan.FromSeconds(5));

        // Long timeout for slow operations
        BiDiDriver longDriver = new BiDiDriver(TimeSpan.FromMinutes(10));
        #endregion
    }

    /// <summary>
    /// Per-command timeout override.
    /// </summary>
    public static async Task PerCommandTimeoutOverride(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region Per-CommandTimeoutOverride
        // Use driver's default timeout (30 seconds)
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Override with custom timeout for this command
        await driver.ExecuteCommandAsync<NavigateCommandResult>(
            navParams,
            TimeSpan.FromSeconds(60));
        #endregion
    }

    /// <summary>
    /// Proper disposal - try/finally.
    /// </summary>
    public static async Task ProperDisposalTryFinally(
        string webSocketUrl)
    {
        #region ProperDisposal-tryfinally
        // Using statement (recommended)
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        try
        {
            await driver.StartAsync(webSocketUrl);
            // Use driver...
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Proper disposal - await using.
    /// </summary>
    public static async Task ProperDisposalAwaitUsing(string webSocketUrl)
    {
        #region ProperDisposal-awaitusing
        // Or with async disposal
        await using BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);
        // Use driver...
        // Automatically disposed at end of scope
        #endregion
    }

    /// <summary>
    /// Complete lifecycle example.
    /// </summary>
    public static async Task CompleteLifecycleExample(
        string webSocketUrl)
    {
        #region CompleteLifecycleExample
        // Create driver with custom timeout
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        try
        {
            // Register event handlers before starting
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                Console.WriteLine($"[{e.Level}] {e.Text}");
            });

            driver.BrowsingContext.OnLoad.AddObserver((e) =>
            {
                Console.WriteLine($"Page loaded: {e.Url}");
            });

            // Start the driver
            await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

            // Verify driver is started
            if (!driver.IsStarted)
            {
                throw new InvalidOperationException("Driver failed to start");
            }

            // Subscribe to events
            SubscribeCommandParameters subscribe = new(
                [
                    driver.Log.OnEntryAdded.EventName,
                    driver.BrowsingContext.OnLoad.EventName,
                ]
            );
            await driver.Session.SubscribeAsync(subscribe);

            // Execute commands
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            NavigateCommandParameters navParams = new(contextId, "https://example.com")
            {
                Wait = ReadinessState.Complete
            };
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        finally
        {
            // Always stop the driver
            if (driver.IsStarted)
            {
                await driver.StopAsync();
            }
        }
        #endregion
    }

    /// <summary>
    /// Accessing modules.
    /// </summary>
    public static void AccessingModules(BiDiDriver driver)
    {
        #region AccessingModules
        // Access a module through the driver
        BrowsingContextModule browsingContext = driver.BrowsingContext;
        NetworkModule network = driver.Network;
        ScriptModule script = driver.Script;
        #endregion
    }

    /// <summary>
    /// Command structure: create parameters, execute, use result.
    /// </summary>

    public static async Task CommandStructureExample(BiDiDriver driver, string contextId)
    {
        #region CommandStructure
        // 1. Create parameters
        NavigateCommandParameters parameters = new NavigateCommandParameters(
            contextId,
            "https://example.com")
        {
            Wait = ReadinessState.Complete
        };

        // 2. Execute command
        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(parameters);

        // 3. Use the result
        Console.WriteLine($"Navigated to: {result.Url}");
        #endregion
    }

    /// <summary>
    /// Command parameters - required and optional.
    /// </summary>
    public static async Task CommandParametersExample(
        BiDiDriver driver,
        string contextId,
        string url)
    {
        #region CommandParameters
        // Required parameters in constructor
        NavigateCommandParameters @params = new NavigateCommandParameters(contextId, url);

        // Optional parameters via properties
        @params.Wait = ReadinessState.Complete;

        // Timeout overrides are supplied when executing the command
        await driver.BrowsingContext.NavigateAsync(
            @params,
            TimeSpan.FromSeconds(30));
        #endregion
    }

    /// <summary>
    /// Optional parameters - GetTreeAsync, StatusAsync, GetCookiesAsync.
    /// </summary>
    public static async Task OptionalParameters(BiDiDriver driver)
    {
        #region OptionalParameters
        // ✅ CORRECT: Parameters optional—use defaults
        GetTreeCommandResult tree1 = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        // Or equivalently:
        GetTreeCommandResult tree2 = await driver.BrowsingContext.GetTreeAsync(null);

        // ✅ CORRECT: Same for other optional-parameter commands
        StatusCommandResult status = await driver.Session.StatusAsync(null);
        GetCookiesCommandResult cookies = await driver.Storage.GetCookiesAsync(null);
        #endregion
    }

    /// <summary>
    /// Required parameters - SetClientHintsOverride with reset and set.
    /// </summary>
    public static async Task RequiredParametersWithReset(BiDiDriver driver)
    {
        #region RequiredParameters
        // ✅ CORRECT: Use the reset property when resetting
        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(
            SetClientHintsOverrideCommandParameters.ResetClientHintsOverride);

        // ✅ CORRECT: Pass explicit parameters when setting
        SetClientHintsOverrideCommandParameters setParams = new SetClientHintsOverrideCommandParameters();
        setParams.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion>() { new BrandVersion("MyBrowser", "120.0") }
        };
        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(setParams);

        // ❌ WRONG: SetClientHintsOverrideAsync always requires parameters
        // The command name alone doesn't indicate whether you're setting or resetting
        // await driver.UserAgentClientHints.SetClientHintsOverrideAsync(null);  // Not allowed
        #endregion
    }

    /// <summary>
    /// Command results - error handling.
    /// </summary>
    public static async Task CommandResultsErrorHandling(
        BiDiDriver driver,
        NavigateCommandParameters @params)
    {
        #region CommandResultsErrorHandling
        try
        {
            NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(@params);
            // Result properties are read-only
            string url = result.Url;
            string navigationId = result.NavigationId;
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Command failed: {ex.Message}");
        }
        #endregion
    }

    /// <summary>
    /// Observable event property names by module.
    /// </summary>
    public static void ObservableEventNames(BiDiDriver driver)
    {
        #region ObservableEventNames
        // Events on BrowsingContext module
        _ = driver.BrowsingContext.OnLoad;
        _ = driver.BrowsingContext.OnDomContentLoaded;
        _ = driver.BrowsingContext.OnNavigationStarted;

        // Events on Network module
        _ = driver.Network.OnBeforeRequestSent;
        _ = driver.Network.OnResponseCompleted;

        // Events on Log module
        _ = driver.Log.OnEntryAdded;
        #endregion
    }

    /// <summary>
    /// Event subscription.
    /// </summary>
    public static async Task EventSubscription(BiDiDriver driver)
    {
        #region EventSubscription
        // Create subscription parameters
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            [
                driver.Log.OnEntryAdded.EventName,
                driver.Network.OnResponseCompleted.EventName,
            ]
        );

        // Subscribe to events
        await driver.Session.SubscribeAsync(subscribe);
        #endregion
    }

    /// <summary>
    /// Event observers - simple and async.
    /// </summary>
    public static void EventObservers(BiDiDriver driver)
    {
        #region EventObservers
        // Add a simple observer
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"Console log: {e.Text}");
        });

        // Add an async observer
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (BeforeRequestSentEventArgs e) =>
            {
                Console.WriteLine($"Request to: {e.Request.Url}");
                await Task.Delay(100); // Can perform async operations
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        #endregion
    }

    /// <summary>
    /// Event observer pattern with checkpoint.
    /// </summary>
    public static async Task EventObserverPattern(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region EventObserverPattern
        // Create an observer with reference
        EventObserver<EntryAddedEventArgs> observer =
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                Console.WriteLine(e.Text);
            });

        // Set a checkpoint to wait for N events
        observer.SetCheckpoint(5); // Wait for 5 events

        // Perform operations that trigger events
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for the checkpoint
        bool fulfilled = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

        // Remove the observer
        observer.Unobserve();
        #endregion
    }

    /// <summary>
    /// Context IDs - get tree and navigate.
    /// </summary>
    public static async Task ContextIds(
        BiDiDriver driver,
        string url)
    {
        #region ContextIDs
        // Get all contexts
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
            new GetTreeCommandParameters());

        // Get the first context ID
        string contextId = tree.ContextTree[0].BrowsingContextId;

        // Navigate in that context
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, url));
        #endregion
    }

    /// <summary>
    /// Context tree - iterate contexts.
    /// </summary>
    public static async Task ContextTree(BiDiDriver driver)
    {
        #region ContextTree
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
            new GetTreeCommandParameters());

        foreach (BrowsingContextInfo context in tree.ContextTree)
        {
            Console.WriteLine($"Context: {context.BrowsingContextId}");
            Console.WriteLine($"  URL: {context.Url}");
            Console.WriteLine($"  Children: {context.Children.Count}");
        }
        #endregion
    }

    /// <summary>
    /// Creating contexts.
    /// </summary>
    public static async Task CreatingContexts(BiDiDriver driver)
    {
        #region CreatingContexts
        // Create a new tab
        CreateCommandParameters createParams = new CreateCommandParameters(CreateType.Tab);
        CreateCommandResult newContext = await driver.BrowsingContext.CreateAsync(createParams);

        string newContextId = newContext.BrowsingContextId;
        #endregion
    }

    /// <summary>
    /// Accessing values with ValueAs.
    /// </summary>
    public static async Task AccessingValues(
        BiDiDriver driver,
        string contextId)
    {
        #region AccessingValues
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("42", new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success &&
            success.Result is LongRemoteValue remoteValue)
        {
            // Convert to appropriate .NET type
            long number = remoteValue.Value; // JavaScript number -> long

            // Check the type
            Console.WriteLine($"Type: {remoteValue.Type}"); // "number"
        }
        #endregion
    }

    /// <summary>
    /// Working with DOM elements.
    /// </summary>
    public static async Task WorkingWithDomElements(
        BiDiDriver driver,
        string contextId)
    {
        #region WorkingwithDOMElements
        // Get a script target against which to run JavaScript
        Target target = new ContextTarget(contextId);

        // Get an element
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('button')",
                target,
                true));

        if (result is EvaluateResultSuccess success &&
            success.Result is NodeRemoteValue element)
        {
            // Get node properties
            NodeProperties nodeProps = element.Value;
            Console.WriteLine($"Tag: {nodeProps.LocalName}");

            // Create a reference to use in other commands
            SharedReference elementRef = element.ToSharedReference();

            // Use the reference in another script call
            CallFunctionCommandParameters clickParams = new CallFunctionCommandParameters(
                "(element) => element.click()",
                target,
                false);
            clickParams.Arguments.Add(elementRef);
            await driver.Script.CallFunctionAsync(clickParams);
        }
        #endregion
    }

    /// <summary>
    /// Creating local values.
    /// </summary>
    public static async Task CreatingLocalValues(
        BiDiDriver driver,
        string contextId)
    {
        #region CreatingLocalValues
        CallFunctionCommandParameters @params = new CallFunctionCommandParameters(
            "(a, b, c) => a + b + c.length",
            new ContextTarget(contextId),
            true);

        // Add arguments as local values
        @params.Arguments.Add(LocalValue.Number(5));
        @params.Arguments.Add(LocalValue.Number(10));
        @params.Arguments.Add(LocalValue.String("hello"));

        EvaluateResult result = await driver.Script.CallFunctionAsync(@params);
        // Result: 20 (5 + 10 + 5)
        #endregion
    }

    /// <summary>
    /// Best practices - await async operations.
    /// </summary>
    public static async Task BestPracticesAwait(
        BiDiDriver driver,
        NavigateCommandParameters parameters)
    {
        #region BestPracticesAwait
        // ✅ Good: Await async operations
        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(parameters);

        // ❌ Bad: Don't block with .Result or .Wait()
        var badResult = driver.BrowsingContext.NavigateAsync(parameters).Result; // Can deadlock

        // ✅ Good: Use ConfigureAwait(false) in library code
        await driver.BrowsingContext.NavigateAsync(parameters).ConfigureAwait(false);
        #endregion
    }

    /// <summary>
    /// Parallel operations.
    /// </summary>
    public static async Task ParallelOperations(
        BiDiDriver driver,
        string contextId1,
        string contextId2,
        string url1,
        string url2)
    {
        #region ParallelOperations
        // Execute multiple navigations in parallel
        Task<NavigateCommandResult> nav1 = driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId1, url1));
        Task<NavigateCommandResult> nav2 = driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId2, url2));

        await Task.WhenAll(nav1, nav2);

        Console.WriteLine($"Context 1: {nav1.Result.Url}");
        Console.WriteLine($"Context 2: {nav2.Result.Url}");
        #endregion
    }

    /// <summary>
    /// WebDriverBiDiException handling.
    /// </summary>
    public static async Task WebDriverBiDiExceptionHandling(
        BiDiDriver driver,
        NavigateCommandParameters @params)
    {
        #region WebDriverBiDiExceptionHandling
        try
        {
            await driver.BrowsingContext.NavigateAsync(@params);
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"BiDi error: {ex.Message}");
            // ex.Message contains the error type and message from the browser
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        #endregion
    }

    /// <summary>
    /// Script exceptions - EvaluateResultException.
    /// </summary>
    public static async Task ScriptExceptions(
        BiDiDriver driver,
        Target target)
    {
        #region ScriptExceptions
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("throw new Error('Oops!')", target, true));

        if (result is EvaluateResultException exception)
        {
            Console.WriteLine($"Script error: {exception.ExceptionDetails.Text}");
            Console.WriteLine($"Line: {exception.ExceptionDetails.LineNumber}");
            Console.WriteLine($"Column: {exception.ExceptionDetails.ColumnNumber}");
        }
        #endregion
    }

    /// <summary>
    /// Timeout handling.
    /// </summary>
    public static async Task TimeoutHandling(
        NavigateCommandParameters parameters)
    {
        #region TimeoutHandling
        // Set a 5-second timeout for this driver
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(5));

        try
        {
            // This will timeout if it takes longer than the driver's configured timeout
            await driver.BrowsingContext.NavigateAsync(parameters);
        }
        catch (WebDriverBiDiTimeoutException)
        {
            Console.WriteLine("Navigation took too long");
        }
        #endregion
    }

    /// <summary>
    /// Immutable - result properties are read-only.
    /// </summary>
    public static async Task ImmutableResult(
        BiDiDriver driver,
        NavigateCommandParameters @params)
    {
        #region ImmutableResult
        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(@params);

        // ❌ Cannot modify - properties are read-only
        // result.Url = "something else"; // Compilation error
        #endregion
    }

    /// <summary>
    /// Mutable - parameters can be modified.
    /// </summary>
    public static async Task MutableParameters(
        BiDiDriver driver,
        string contextId,
        string url)
    {
        #region MutableParameters
        NavigateCommandParameters parameters = new NavigateCommandParameters(contextId, url);

        // ✅ Can modify - properties are settable
        parameters.Wait = ReadinessState.Complete;

        // Timeout overrides are supplied when executing the command
        await driver.BrowsingContext.NavigateAsync(
            parameters,
            TimeSpan.FromSeconds(30));
        #endregion
    }

    /// <summary>
    /// Advanced abstractions - BiDiDriver direct use.
    /// </summary>
    public static async Task AdvancedAbstractions(
        string webSocketUrl)
    {
        #region AdvancedAbstractions
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);
        #endregion
    }

    /// <summary>
    /// Registering and using a custom module.
    /// </summary>
    public static async Task RegisteringAndUsingCustomModule(
        string webSocketUrl)
    {
        #region RegisteringandUsingaCustomModule
        // Create driver
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Register custom module BEFORE starting
        CustomModule customModule = new CustomModule(driver);
        driver.RegisterModule(customModule);

        // Register custom event
        driver.RegisterEvent<CustomEventArgs>(
            customModule.OnCustomEvent.EventName,
            async (eventInfo) =>
            {
                await customModule.OnCustomEvent.NotifyObserversAsync(eventInfo.ToEventArgs<CustomEventArgs>());
            });

        // NOW start the driver
        await driver.StartAsync(webSocketUrl);

        // Use custom module like built-in modules
        CustomCommandParameters parameters = new CustomCommandParameters
        {
            CustomProperty = "value"
        };
        CustomCommandResult result = await customModule.MyCustomCommandAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Custom JSON type resolvers for AOT scenarios.
    /// </summary>
    public static async Task CustomJsonTypeResolvers(string webSocketUrl)
    {
        #region RegisterCustomResolverBeforeStarting
        // Register the custom resolver BEFORE starting
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.RegisterTypeInfoResolver(CustomJsonContext.Default);

        await driver.StartAsync(webSocketUrl);
        #endregion
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS0168 // Variable declared but never used
#pragma warning restore CS0219 // Variable assigned but never used
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
