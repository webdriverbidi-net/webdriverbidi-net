// <copyright file="ScriptModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

/// <summary>
/// The Script module.
/// </summary>
public sealed class ScriptModule : ProtocolModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public ScriptModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker("script.realmCreated", typeof(RealmInfo), this.OnRealmCreated);
        this.RegisterEventInvoker("script.realmDestroyed", typeof(RealmDestroyedEventArgs), this.OnRealmDestroyed);
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
    /// Adds a preload script to each page before execution of other JavaScript included in the page source.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the ID of the created preload script.</returns>
    public async Task<AddPreloadScriptCommandResult> AddPreloadScript(AddPreloadScriptCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<AddPreloadScriptCommandResult>(commandProperties);
    }

    /// <summary>
    /// Calls a function in the specified script target.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the result of the function execution.</returns>
    public async Task<ScriptEvaluateResult> CallFunction(CallFunctionCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<ScriptEvaluateResult>(commandProperties);
    }

    /// <summary>
    /// Disowns the specified handles to allow the script engine to garbage collect objects.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> Disown(DisownCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Evaluates a piece of JavaScript in the specified script target.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the result of the script evaluation.</returns>
    public async Task<ScriptEvaluateResult> Evaluate(EvaluateCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<ScriptEvaluateResult>(commandProperties);
    }

    /// <summary>
    /// Gets the realms associated with a given browsing context and realm type.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing IDs of the realms.</returns>
    public async Task<GetRealmsCommandResult> GetRealms(GetRealmsCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<GetRealmsCommandResult>(commandProperties);
    }

    /// <summary>
    /// Removes a preload script from loading on each page before execution of other JavaScript included in the page source.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> RemovePreloadScript(RemovePreloadScriptCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    private void OnRealmCreated(object eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a RealmInfo object, so rather than duplicate
        // the properties to directly deserialize the RealmCreatedEventArgs
        // instance, the protocol transport will deserialize to a RealmInfo,
        // then use that here to create the appropriate EventArgs instance.
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        if (eventData is RealmInfo eventArgs)
        {
            RealmCreatedEventArgs e = new(eventArgs);
            if (this.RealmCreated is not null)
            {
                this.RealmCreated(this, e);
            }
        }
    }

    private void OnRealmDestroyed(object eventData)
    {
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        if (eventData is RealmDestroyedEventArgs eventArgs)
        {
            if (this.RealmDestroyed is not null)
            {
                this.RealmDestroyed(this, eventArgs);
            }
        }
    }
}