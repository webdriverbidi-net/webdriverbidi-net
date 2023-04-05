// <copyright file="InputModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

/// <summary>
/// The Input module contains commands and events relating to simulating user input.
/// </summary>
public sealed class InputModule : Module
{
    /// <summary>
    /// The name of the input module.
    /// </summary>
    public const string InputModuleName = "input";

    /// <summary>
    /// Initializes a new instance of the <see cref="InputModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public InputModule(Driver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => InputModuleName;

    /// <summary>
    /// Performs a set of actions.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> PerformActions(PerformActionsCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Releases pending actions.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> ReleaseActions(ReleaseActionsCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }
}
