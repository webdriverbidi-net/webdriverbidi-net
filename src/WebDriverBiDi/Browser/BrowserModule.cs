// <copyright file="BrowserModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

/// <summary>
/// The Browser module contains commands for managing the remote end browser process.
/// </summary>
public sealed class BrowserModule : Module
{
    /// <summary>
    /// The name of the browser module.
    /// </summary>
    public const string BrowserModuleName = "browser";

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public BrowserModule(BiDiDriver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => BrowserModuleName;

    /// <summary>
    /// Terminates all WebDriver sessions and cleans up automation state in the remote browser instance.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<CloseCommandResult> CloseAsync(CloseCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<CloseCommandResult>(commandProperties ?? new()).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new user context for the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An object describing information about the user context created.</returns>
    public async Task<CreateUserContextCommandResult> CreateUserContextAsync(CreateUserContextCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<CreateUserContextCommandResult>(commandProperties ?? new()).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the list of information about the client windows for the current browser.
    /// </summary>
    /// <param name="commandProperties">THe parameters for the command.</param>
    /// <returns>An read-only list of the client windows open in this browser.</returns>
    public async Task<GetClientWindowsCommandResult> GetClientWindowsAsync(GetClientWindowsCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<GetClientWindowsCommandResult>(commandProperties ?? new()).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the list of open user contexts for the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>A read-only list of the user contexts open in this browser.</returns>
    public async Task<GetUserContextsCommandResult> GetUserContextsAsync(GetUserContextsCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<GetUserContextsCommandResult>(commandProperties ?? new()).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes a user context for the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<RemoveUserContextCommandResult> RemoveUserContextAsync(RemoveUserContextCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<RemoveUserContextCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the state of a client window, including size and position. Note that the remote
    /// end may not be capable of setting the window to the requested state, and this may
    /// not necessarily return an error.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The actual state of the window after setting the state.</returns>
    public async Task<SetClientWindowStateCommandResult> SetClientWindowStateAsync(SetClientWindowStateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SetClientWindowStateCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the download behavior for the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetDownloadBehaviorCommandResult> SetDownloadBehaviorAsync(SetDownloadBehaviorCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SetDownloadBehaviorCommandResult>(commandProperties).ConfigureAwait(false);
    }
}
