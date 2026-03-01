// <copyright file="ScriptModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// The Script module contains commands and events relating to script realms and execution.
/// </summary>
public sealed class ScriptModule : Module
{
    /// <summary>
    /// The name of the script module.
    /// </summary>
    public const string ScriptModuleName = "script";

    private const string RealmCreatedEventName = $"{ScriptModuleName}.realmCreated";
    private const string RealmDestroyedEventName = $"{ScriptModuleName}.realmDestroyed";
    private const string MessageEventName = $"{ScriptModuleName}.message";

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public ScriptModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterObservableEvent<RealmInfo, RealmCreatedEventArgs>(this.OnRealmCreated, info => new RealmCreatedEventArgs(info));
        this.RegisterObservableEvent(this.OnRealmDestroyed);
        this.RegisterObservableEvent(this.OnMessage);
    }

    /// <summary>
    /// Gets an observable event that notifies when a new script realm is created.
    /// </summary>
    public ObservableEvent<RealmCreatedEventArgs> OnRealmCreated { get; } = new(RealmCreatedEventName);

    /// <summary>
    /// Gets an observable event that notifies with a script realm is destroyed.
    /// </summary>
    public ObservableEvent<RealmDestroyedEventArgs> OnRealmDestroyed { get; } = new(RealmDestroyedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a preload script sends data to the client.
    /// </summary>
    public ObservableEvent<MessageEventArgs> OnMessage { get; } = new(MessageEventName);

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => ScriptModuleName;

    /// <summary>
    /// Adds a preload script to each page before execution of other JavaScript included in the page source.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing the ID of the created preload script.</returns>
    public Task<AddPreloadScriptCommandResult> AddPreloadScriptAsync(AddPreloadScriptCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Calls a function in the specified script target.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing the result of the function execution.</returns>
    public Task<EvaluateResult> CallFunctionAsync(CallFunctionCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Disowns the specified handles to allow the script engine to garbage collect objects.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<DisownCommandResult> DisownAsync(DisownCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Evaluates a piece of JavaScript in the specified script target.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing the result of the script evaluation.</returns>
    public Task<EvaluateResult> EvaluateAsync(EvaluateCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Gets the realms associated with a given browsing context and realm type.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing IDs of the realms.</returns>
    public Task<GetRealmsCommandResult> GetRealmsAsync(GetRealmsCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Removes a preload script from loading on each page before execution of other JavaScript included in the page source.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<RemovePreloadScriptCommandResult> RemovePreloadScriptAsync(RemovePreloadScriptCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }
}
