// <copyright file="UserAgentClientHintsModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.UserAgentClientHints;

/// <summary>
/// The UserAgentClientHints module contains commands for managing user agent client hints.
/// </summary>
public sealed class UserAgentClientHintsModule : Module
{
    /// <summary>
    /// The name of the userAgentClientHints module.
    /// </summary>
    public const string UserAgentClientHintsModuleName = "userAgentClientHints";

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAgentClientHintsModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public UserAgentClientHintsModule(BiDiDriver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => UserAgentClientHintsModuleName;

    /// <summary>
    /// Sets overrides for the user agent client hints.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<SetClientHintsOverrideCommandResult> SetClientHintsOverrideAsync(SetClientHintsOverrideCommandParameters? commandProperties = null)
    {
        return await this.Driver.ExecuteCommandAsync<SetClientHintsOverrideCommandResult>(commandProperties ?? new()).ConfigureAwait(false);
    }
}
