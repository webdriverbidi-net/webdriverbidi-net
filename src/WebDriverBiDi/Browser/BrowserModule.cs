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
    /// <param name="driver">The <see cref="IBiDiDriver"/> used in the module commands and events.</param>
    public BrowserModule(IBiDiDriver driver)
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
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<CloseCommandResult> CloseAsync(CloseCommandParameters? commandParameters = null)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new());
    }

    /// <summary>
    /// Creates a new user context for the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An object describing information about the user context created.</returns>
    public Task<CreateUserContextCommandResult> CreateUserContextAsync(CreateUserContextCommandParameters? commandParameters = null)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new());
    }

    /// <summary>
    /// Gets the list of information about the client windows for the current browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>A read-only list of the client windows open in this browser.</returns>
    public Task<GetClientWindowsCommandResult> GetClientWindowsAsync(GetClientWindowsCommandParameters? commandParameters = null)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new());
    }

    /// <summary>
    /// Gets the list of open user contexts for the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>A read-only list of the user contexts open in this browser.</returns>
    public Task<GetUserContextsCommandResult> GetUserContextsAsync(GetUserContextsCommandParameters? commandParameters = null)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new());
    }

    /// <summary>
    /// Removes a user context for the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<RemoveUserContextCommandResult> RemoveUserContextAsync(RemoveUserContextCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Sets the state of a client window, including size and position. Note that the remote
    /// end may not be capable of setting the window to the requested state, and this may
    /// not necessarily return an error.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The actual state of the window after setting the state.</returns>
    public Task<SetClientWindowStateCommandResult> SetClientWindowStateAsync(SetClientWindowStateCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Sets the download behavior for the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetDownloadBehaviorCommandResult> SetDownloadBehaviorAsync(SetDownloadBehaviorCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }
}
