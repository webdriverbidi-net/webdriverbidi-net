// <copyright file="BrowserModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Browser;

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
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public BrowserModule(Driver driver)
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
    public async Task<EmptyResult> CloseAsync(CloseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }
}
