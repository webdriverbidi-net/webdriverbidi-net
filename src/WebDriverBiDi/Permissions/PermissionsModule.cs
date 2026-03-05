// <copyright file="PermissionsModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Permissions;

/// <summary>
/// The Permissions module contains commands and events relating to browser permissions
/// as defined in the W3C Permissions specification (https://www.w3.org/TR/permissions/).
/// </summary>
public sealed class PermissionsModule : Module
{
    /// <summary>
    /// The name of the permissions module.
    /// </summary>
    public const string PermissionsModuleName = "permissions";

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionsModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiDriver"/> used in the module commands and events.</param>
    public PermissionsModule(IBiDiDriver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => PermissionsModuleName;

    /// <summary>
    /// Sets a permission for a given web site.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public Task<SetPermissionCommandResult> SetPermissionAsync(SetPermissionCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }
}
