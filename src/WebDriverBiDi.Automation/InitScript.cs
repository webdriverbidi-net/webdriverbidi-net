// <copyright file="InitScript.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Script;

/// <summary>
/// A script added to run in each new document of a page, before the document's own scripts.
/// </summary>
public sealed class InitScript
{
    private readonly BiDiDriver driver;

    /// <summary>
    /// Initializes a new instance of the <see cref="InitScript"/> class.
    /// </summary>
    /// <param name="driver">The driver that removes the script.</param>
    /// <param name="id">The ID of the browser's preload script.</param>
    internal InitScript(BiDiDriver driver, string id)
    {
        this.driver = driver;
        this.Id = id;
    }

    /// <summary>
    /// Gets the ID of the browser's preload script.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Stops the script running in documents loaded from now on.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the script has been removed.</returns>
    public Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        return this.driver.Script.RemovePreloadScriptAsync(new RemovePreloadScriptCommandParameters(this.Id), cancellationToken: cancellationToken);
    }
}
