// <copyright file="EmulationModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

/// <summary>
/// The Emulation module contains commands and events relating to emulating various conditions in the browser.
/// </summary>
public sealed class EmulationModule : Module
{
    /// <summary>
    /// The name of the emulation module.
    /// </summary>
    public const string EmulationModuleName = "emulation";

    /// <summary>
    /// Initializes a new instance of the <see cref="EmulationModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public EmulationModule(BiDiDriver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => EmulationModuleName;

    /// <summary>
    /// Sets the emulated override for forced color themes for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetForcedColorsModeThemeOverrideCommandResult> SetForcedColorsModeThemeOverrideAsync(SetForcedColorsModeThemeOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetForcedColorsModeThemeOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the geolocation for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetGeolocationOverrideCommandResult> SetGeolocationOverrideAsync(SetGeolocationOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetGeolocationOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the locale for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetLocaleOverrideCommandResult> SetLocaleOverrideAsync(SetLocaleOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetLocaleOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated network conditions for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetNetworkConditionsCommandResult> SetNetworkConditionsAsync(SetNetworkConditionsCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetNetworkConditionsCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the screen orientation for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetScreenOrientationOverrideCommandResult> SetScreenOrientationOverrideAsync(SetScreenOrientationOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetScreenOrientationOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the screen settings (height, width, etc.) for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetScreenSettingsOverrideCommandResult> SetScreenSettingsOverrideAsync(SetScreenSettingsOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetScreenSettingsOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the override for whether JavaScript is enabled for the specified contexts or user contexts.
    /// Note carefully that this is only useful for simulating the disabling of JavaScript.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetScriptingEnabledCommandResult> SetScriptingEnabledAsync(SetScriptingEnabledCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetScriptingEnabledCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the override for emulating the type of scroll bar displayed in the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetScrollbarTypeOverrideCommandResult> SetScrollbarTypeOverrideAsync(SetScrollbarTypeOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetScrollbarTypeOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the time zone for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetTimeZoneOverrideCommandResult> SetTimeZoneOverrideAsync(SetTimeZoneOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetTimeZoneOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for touch events for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetTouchOverrideCommandResult> SetTouchOverrideAsync(SetTouchOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetTouchOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the user agent string for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetUserAgentOverrideCommandResult> SetUserAgentOverrideAsync(SetUserAgentOverrideCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<SetUserAgentOverrideCommandResult>(commandParameters).ConfigureAwait(false);
    }
}
