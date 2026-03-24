// <copyright file="ApiIndexSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/api/index.md

#pragma warning disable CS8600, CS8602, CS8618, CS8603, CS8625

namespace WebDriverBiDi.Docs.Code.Api;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for API reference documentation.
/// </summary>
public class ApiIndexSamples
{
    /// <summary>
    /// Command parameters follow this pattern: required in constructor, optional as properties.
    /// </summary>
#region CommandParametersPattern
    public class CommandNameCommandParameters : CommandParameters<CommandNameCommandResult>
    {
        // Required parameters in constructor
        public CommandNameCommandParameters(string requiredParam)
        {
        }

        // Optional parameters as properties
        public string? OptionalParam { get; set; }

        public override string MethodName => "custom.moduleName";
    }
    #endregion

    #region CommandResultsPattern
    public record CommandNameCommandResult : CommandResult
    {
        // Properties are read-only (immutable)
        public string ResultProperty { get; }
    }
    #endregion

    #region EventArgumentsPattern
    public record EventNameEventArgs : WebDriverBiDiEventArgs
    {
        // Properties are read-only (immutable)
        public string EventData { get; }
    }
    #endregion

    #region AsyncMethodsPattern
    public async Task<TResult> MethodNameAsync(TParameters parameters)
    #endregion
    {
        return default;
    }

    /// <summary>
    /// Command results have read-only (immutable) properties.
    /// </summary>
    public static async Task CommandResultsPattern(BiDiDriver driver)
    {
        GetTreeCommandResult result = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        string id = result.ContextTree[0].BrowsingContextId;
    }

    /// <summary>
    /// Event arguments inherit from WebDriverBiDiEventArgs with read-only properties.
    /// </summary>
    public static void EventArgumentsPattern(BiDiDriver driver)
    {
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            string text = e.Text;
        });
    }

    /// <summary>
    /// Async methods pattern.
    /// </summary>
    public static async Task AsyncMethodsPattern()
    {
        BiDiDriver driver = new BiDiDriver();
        _ = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
    }

    /// <summary>
    /// Module access pattern.
    /// </summary>
    public static void ModuleAccess()
    {
        #region ModuleAccess
        BiDiDriver driver = new BiDiDriver();
        BrowsingContextModule browsingContext = driver.BrowsingContext;
        ScriptModule script = driver.Script;
        NetworkModule network = driver.Network;
        #endregion
    }

    /// <summary>
    /// Event subscription pattern.
    /// </summary>
    public async Task EventSubscription(BiDiDriver driver)
    {
        SampleModule module = new();
        Action<TEventArgs> handler = (TEventArgs e) =>
        {
            string eventData = e.EventData;
        };

        SubscribeCommandParameters subscribeParams = new SubscribeCommandParameters("module.eventName");

        #region EventSubscription
        // Access observable event
        ObservableEvent<TEventArgs> observableEvent = module.OnEventName;

        // Add observer
        EventObserver<TEventArgs> observer = observableEvent.AddObserver(handler);

        // Subscribe through Session
        await driver.Session.SubscribeAsync(subscribeParams);
        #endregion
    }

    public class SampleModule : Module
    {
        public SampleModule() : base(null) { }
        public ObservableEvent<TEventArgs> OnEventName { get; }

        public override string ModuleName => throw new NotImplementedException();

        public Task<TResult> MethodNameAsync(TParameters parameters) => Task.FromResult(new TResult());
        // Other module members...
    }

    public class TResult
    {
    }

    public class TParameters
    {
    }

    public record TEventArgs : WebDriverBiDiEventArgs
    {
        public string EventData { get; }
    }
}
