// <copyright file="StorageModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

/// <summary>
/// The Storage module contains commands and events relating to browser storage such as cookies.
/// </summary>
public sealed class StorageModule : Module
{
    /// <summary>
    /// The name of the storage module.
    /// </summary>
    public const string StorageModuleName = "storage";

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public StorageModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => StorageModuleName;

    /// <summary>
    /// Gets cookies from the browser session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<GetCookiesCommandResult> GetCookiesAsync(GetCookiesCommandParameters? commandParameters = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new(), timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets a cookie in the browser session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<SetCookieCommandResult> SetCookieAsync(SetCookieCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Deletes cookies from the browser session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<DeleteCookiesCommandResult> DeleteCookiesAsync(DeleteCookiesCommandParameters? commandParameters = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new(), timeoutOverride, cancellationToken);
    }
}
