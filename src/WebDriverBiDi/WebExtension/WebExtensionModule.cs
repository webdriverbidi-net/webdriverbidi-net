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
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public WebExtensionModule(IBiDiCommandExecutor driver)
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
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>A Task containing the result of the command including the ID of the installed extension.</returns>
    public Task<InstallCommandResult> InstallAsync(InstallCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Uninstalls a web extension from the current driver session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>A Task containing the result of the asynchronous operation.</returns>
    public Task<UninstallCommandResult> UninstallAsync(UninstallCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }
}
