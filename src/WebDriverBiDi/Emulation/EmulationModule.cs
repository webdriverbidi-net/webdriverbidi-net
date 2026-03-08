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
    /// <param name="driver">The <see cref="IBiDiDriver"/> used in the module commands and events.</param>
    public EmulationModule(IBiDiDriver driver)
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
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetForcedColorsModeThemeOverrideCommandResult> SetForcedColorsModeThemeOverrideAsync(SetForcedColorsModeThemeOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated override for the geolocation for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetGeolocationOverrideCommandResult> SetGeolocationOverrideAsync(SetGeolocationOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated override for the locale for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetLocaleOverrideCommandResult> SetLocaleOverrideAsync(SetLocaleOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated network conditions for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetNetworkConditionsCommandResult> SetNetworkConditionsAsync(SetNetworkConditionsCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated override for the screen orientation for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetScreenOrientationOverrideCommandResult> SetScreenOrientationOverrideAsync(SetScreenOrientationOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated override for the screen settings (height, width, etc.) for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetScreenSettingsOverrideCommandResult> SetScreenSettingsOverrideAsync(SetScreenSettingsOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the override for whether JavaScript is enabled for the specified contexts or user contexts.
    /// Note carefully that this is only useful for simulating the disabling of JavaScript.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetScriptingEnabledCommandResult> SetScriptingEnabledAsync(SetScriptingEnabledCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the override for emulating the type of scroll bar displayed in the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetScrollbarTypeOverrideCommandResult> SetScrollbarTypeOverrideAsync(SetScrollbarTypeOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated override for the time zone for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetTimeZoneOverrideCommandResult> SetTimeZoneOverrideAsync(SetTimeZoneOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated override for touch events for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetTouchOverrideCommandResult> SetTouchOverrideAsync(SetTouchOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the emulated override for the user agent string for the specified contexts or user contexts.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetUserAgentOverrideCommandResult> SetUserAgentOverrideAsync(SetUserAgentOverrideCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }
}
