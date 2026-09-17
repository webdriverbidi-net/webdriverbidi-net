// <copyright file="SessionModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// The Session module contains commands for monitoring the status of the remote end and for managing
/// event subscriptions. The specification defines no events for this module.
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
    /// <param name="driver">The <see cref="IBiDiModuleHost"/> used in the module commands and events.</param>
    public SessionModule(IBiDiModuleHost driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => SessionModuleName;

    /// <summary>
    /// Gets the status of the remote end: whether it is in a state in which it can create new sessions, along with any
    /// implementation-specific information it reports.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command. If omitted, or if <see langword="null"/>, the command is sent with default parameters.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command containing the information about the remote end status.</returns>
    /// <include file="../ModuleCommandExceptions.xml" path="exceptions/command/*"/>
    public Task<StatusCommandResult> StatusAsync(StatusCommandParameters? commandParameters = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new(), timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command. If omitted, or if <see langword="null"/>, the command is sent with default parameters.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command containing the information about the new session.</returns>
    /// <include file="../ModuleCommandExceptions.xml" path="exceptions/command/*"/>
    public Task<NewCommandResult> NewSessionAsync(NewCommandParameters? commandParameters = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new(), timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Subscribes to events for this session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command containing the subscription ID.</returns>
    /// <include file="../ModuleCommandExceptions.xml" path="exceptions/required-parameters/*"/>
    /// <include file="../ModuleCommandExceptions.xml" path="exceptions/command/*"/>
    public Task<SubscribeCommandResult> SubscribeAsync(SubscribeCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Unsubscribes from events for this session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    /// <include file="../ModuleCommandExceptions.xml" path="exceptions/required-parameters/*"/>
    /// <include file="../ModuleCommandExceptions.xml" path="exceptions/command/*"/>
    public Task<UnsubscribeCommandResult> UnsubscribeAsync(UnsubscribeCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Ends the current session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command. If omitted, or if <see langword="null"/>, the command is sent with default parameters.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    /// <include file="../ModuleCommandExceptions.xml" path="exceptions/command/*"/>
    public Task<EndCommandResult> EndAsync(EndCommandParameters? commandParameters = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new(), timeoutOverride, cancellationToken);
    }
}
