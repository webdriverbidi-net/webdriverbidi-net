// <copyright file="SessionModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// The Session module contains commands and events for monitoring the status of the remote end.
/// </summary>
public sealed class SessionModule : Module
{
    /// <summary>
    /// The name of the session module.
    /// </summary>
    public const string SessionModuleName = "session";

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public SessionModule(BiDiDriver driver)
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
    public async Task<StatusCommandResult> StatusAsync(StatusCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<StatusCommandResult>(commandProperties ?? new()).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the information new session.</returns>
    public async Task<NewCommandResult> NewSessionAsync(NewCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<NewCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Subscribes to events for this session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing the subscription ID.</returns>
    public async Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SubscribeCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Unsubscribes from events for this session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<UnsubscribeCommandResult> UnsubscribeAsync(UnsubscribeCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<UnsubscribeCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Ends the current session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EndCommandResult> EndAsync(EndCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<EndCommandResult>(commandProperties ?? new()).ConfigureAwait(false);
    }
}
