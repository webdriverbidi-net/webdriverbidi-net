// <copyright file="ScriptModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

/// <summary>
/// The Script module contains commands and events relating to script realms and execution.
/// </summary>
public sealed class ScriptModule : Module
{
    /// <summary>
    /// The name of the script module.
    /// </summary>
    public const string ScriptModuleName = "script";

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public ScriptModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker<RealmInfo>("script.realmCreated", this.OnRealmCreated);
        this.RegisterEventInvoker<RealmDestroyedEventArgs>("script.realmDestroyed", this.OnRealmDestroyed);
        this.RegisterEventInvoker<MessageEventArgs>("script.message", this.OnMessage);
    }

    /// <summary>
    /// Occurs when a new script realm is created.
    /// </summary>
    public event EventHandler<RealmCreatedEventArgs>? RealmCreated;

    /// <summary>
    /// Occurs with a script realm is destroyed.
    /// </summary>
    public event EventHandler<RealmDestroyedEventArgs>? RealmDestroyed;

    /// <summary>
    /// Occurs when a preload script sends data to the client.
    /// </summary>
    public event EventHandler<MessageEventArgs>? Message;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => ScriptModuleName;

    /// <summary>
    /// Adds a preload script to each page before execution of other JavaScript included in the page source.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the ID of the created preload script.</returns>
    public async Task<AddPreloadScriptCommandResult> AddPreloadScript(AddPreloadScriptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<AddPreloadScriptCommandResult>(commandProperties);
    }

    /// <summary>
    /// Calls a function in the specified script target.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the result of the function execution.</returns>
    public async Task<EvaluateResult> CallFunction(CallFunctionCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<EvaluateResult>(commandProperties);
    }

    /// <summary>
    /// Disowns the specified handles to allow the script engine to garbage collect objects.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> Disown(DisownCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Evaluates a piece of JavaScript in the specified script target.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the result of the script evaluation.</returns>
    public async Task<EvaluateResult> Evaluate(EvaluateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<EvaluateResult>(commandProperties);
    }

    /// <summary>
    /// Gets the realms associated with a given browsing context and realm type.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing IDs of the realms.</returns>
    public async Task<GetRealmsCommandResult> GetRealms(GetRealmsCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<GetRealmsCommandResult>(commandProperties);
    }

    /// <summary>
    /// Removes a preload script from loading on each page before execution of other JavaScript included in the page source.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> RemovePreloadScript(RemovePreloadScriptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    private void OnRealmCreated(EventInfo<RealmInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a RealmInfo object, so rather than duplicate
        // the properties to directly deserialize the RealmCreatedEventArgs
        // instance, the protocol transport will deserialize to a RealmInfo,
        // then use that here to create the appropriate EventArgs instance.
        if (this.RealmCreated is not null)
        {
            RealmCreatedEventArgs eventArgs = eventData.ToEventArgs<RealmCreatedEventArgs>();
            this.RealmCreated(this, eventArgs);
        }
    }

    private void OnRealmDestroyed(EventInfo<RealmDestroyedEventArgs> eventData)
    {
        if (this.RealmDestroyed is not null)
        {
            RealmDestroyedEventArgs eventArgs = eventData.ToEventArgs<RealmDestroyedEventArgs>();
            this.RealmDestroyed(this, eventArgs);
        }
    }

    private void OnMessage(EventInfo<MessageEventArgs> eventData)
    {
        if (this.Message is not null)
        {
            MessageEventArgs eventArgs = eventData.ToEventArgs<MessageEventArgs>();
            this.Message(this, eventArgs);
        }
    }
}