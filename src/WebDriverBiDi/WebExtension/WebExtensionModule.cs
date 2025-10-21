// <copyright file="WebExtensionModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.WebExtension;

/// <summary>
/// The WebExtension module contains commands and events relating to web extensions in the browser.
/// </summary>
public sealed class WebExtensionModule : Module
{
    /// <summary>
    /// The name of the webExtension module.
    /// </summary>
    public const string WebExtensionModuleName = "webExtension";

     /// <summary>
    /// Initializes a new instance of the <see cref="WebExtensionModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public WebExtensionModule(BiDiDriver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => WebExtensionModuleName;

    /// <summary>
    /// Installs a web extension into the current driver session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>A Task containing the result of the command including the ID of the installed extension.</returns>
    public async Task<InstallCommandResult> InstallAsync(InstallCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<InstallCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Uninstalls a web extension from the current driver session.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>A Task containing the result of the asynchronous operation.</returns>
    public async Task<UninstallCommandResult> UninstallAsync(UninstallCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<UninstallCommandResult>(commandProperties).ConfigureAwait(false);
    }
}
