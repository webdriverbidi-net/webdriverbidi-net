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
    public async Task<EmptyResult> CloseAsync(CloseCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties ?? new()).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new user context for the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An object describing information about the user context created.</returns>
    public async Task<CreateUserContextCommandResult> CreateUserContextAsync(CreateUserContextCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<CreateUserContextCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the list of open user contexts for the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>A read-only list of the user contexts open in this browser.</returns>
    public async Task<GetUserContextsCommandResult> GetUserContextsAsync(GetUserContextsCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<GetUserContextsCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes a user context for the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<EmptyResult> RemoveUserContextAsync(RemoveUserContextCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }
}
