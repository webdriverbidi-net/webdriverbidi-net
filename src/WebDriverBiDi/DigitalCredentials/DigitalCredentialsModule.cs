// <copyright file="DigitalCredentialsModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DigitalCredentials;

/// <summary>
/// The DigitalCredentials module contains commands and events relating to digital credentials
/// as defined in the W3C Digital Credentials specification (https://www.w3.org/TR/digital-credentials/).
/// </summary>
public sealed class DigitalCredentialsModule : Module
{
    /// <summary>
    /// The name of the digitalCredentials module.
    /// </summary>
    public const string DigitalCredentialsModuleName = "digitalCredentials";

    /// <summary>
    /// Initializes a new instance of the <see cref="DigitalCredentialsModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public DigitalCredentialsModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => DigitalCredentialsModuleName;

    /// <summary>
    /// Sets the behavior of the virtual wallet holding the digital credentials.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetVirtualWalletBehaviorCommandResult> SetVirtualWalletBehaviorAsync(SetVirtualWalletBehaviorCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }
}
