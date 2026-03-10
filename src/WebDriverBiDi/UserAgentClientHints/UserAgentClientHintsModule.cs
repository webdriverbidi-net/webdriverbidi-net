// <copyright file="UserAgentClientHintsModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.UserAgentClientHints;

/// <summary>
/// The UserAgentClientHints module contains commands for managing user agent client hints.
/// </summary>
public sealed class UserAgentClientHintsModule : Module
{
    /// <summary>
    /// The name of the userAgentClientHints module.
    /// </summary>
    public const string UserAgentClientHintsModuleName = "userAgentClientHints";

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAgentClientHintsModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public UserAgentClientHintsModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => UserAgentClientHintsModuleName;

    /// <summary>
    /// Sets overrides for the user agent client hints.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetClientHintsOverrideCommandResult> SetClientHintsOverrideAsync(SetClientHintsOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }
}
