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
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing the information about the remote end status.</returns>
    public Task<StatusCommandResult> StatusAsync(StatusCommandParameters? commandParameters = null)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new());
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing the information new session.</returns>
    public Task<NewCommandResult> NewSessionAsync(NewCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Subscribes to events for this session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing the subscription ID.</returns>
    public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Unsubscribes from events for this session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<UnsubscribeCommandResult> UnsubscribeAsync(UnsubscribeCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Ends the current session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<EndCommandResult> EndAsync(EndCommandParameters? commandParameters = null)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new());
    }
}
