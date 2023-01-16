// <copyright file="SessionModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

/// <summary>
/// The Session module.
/// </summary>
public sealed class SessionModule : ProtocolModule
{
    /// <summary>
    /// The name of the session module.
    /// </summary>
    public const string SessionModuleName = "session";

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public SessionModule(Driver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => SessionModuleName;

    /// <summary>
    /// Gets the status of the current connection.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the information about the remote end status.</returns>
    public async Task<StatusCommandResult> Status(StatusCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<StatusCommandResult>(commandProperties);
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the information new session.</returns>
    public async Task<NewCommandResult> NewSession(NewCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<NewCommandResult>(commandProperties);
    }

    /// <summary>
    /// Subscribes to events for this session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> Subscribe(SubscribeCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Unsubscribes from events for this session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> Unsubscribe(UnsubscribeCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }
}