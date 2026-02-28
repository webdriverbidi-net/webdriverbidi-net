// <copyright file="StorageModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

/// <summary>
/// The Storage module contains commands and events relating to browser storage such as cookies.
/// </summary>
public sealed class StorageModule : Module
{
    /// <summary>
    /// The name of the storage module.
    /// </summary>
    public const string StorageModuleName = "storage";

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public StorageModule(BiDiDriver driver)
        : base(driver)
    {
    }

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => StorageModuleName;

    /// <summary>
    /// Gets cookies from the browser session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<GetCookiesCommandResult> GetCookiesAsync(GetCookiesCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Sets a cookie in the browser session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<SetCookieCommandResult> SetCookieAsync(SetCookieCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Deletes cookies from the browser session.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<DeleteCookiesCommandResult> DeleteCookiesAsync(DeleteCookiesCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }
}
