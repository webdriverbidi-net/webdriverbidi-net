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
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SetForcedColorsModeThemeOverrideAsync(SetForcedColorsModeThemeOverrideCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the geolocation for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SetGeolocationOverrideAsync(SetGeolocationOverrideCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the locale for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SetLocaleOverrideAsync(SetLocaleOverrideCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the screen orientation for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SetScreenOrientationOverrideAsync(SetScreenOrientationOverrideCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the emulated override for the time zone for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> SetTimeZoneOverrideAsync(SetTimeZoneOverrideCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }
}
