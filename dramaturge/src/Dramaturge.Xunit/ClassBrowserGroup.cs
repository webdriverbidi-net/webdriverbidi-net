// <copyright file="ClassBrowserGroup.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;
using Dramaturge.Testing;

/// <summary>
/// The browser group of a <see cref="BrowserTest"/> class that overrides how its group is launched, shared by the
/// class's tests and closed when the class finishes. xUnit creates one for each such class; a class that uses the
/// assembly's shared group never launches it.
/// </summary>
public sealed class ClassBrowserGroup : IAsyncDisposable
{
    private readonly SharedBrowserGroup group = new();

    /// <summary>
    /// Closes the group, if it was launched.
    /// </summary>
    /// <returns>A task that completes when the group is closed.</returns>
    public ValueTask DisposeAsync()
    {
        return this.group.DisposeAsync();
    }

    /// <summary>
    /// Gets the group, launching it on the first call.
    /// </summary>
    /// <param name="configureLauncher">Configures the launcher, given the one the environment chooses.</param>
    /// <param name="options">The group's options.</param>
    /// <returns>The group.</returns>
    internal Task<BrowserGroup> GetAsync(Func<BrowserLauncherBuilder, BrowserLauncherBuilder> configureLauncher, DramaturgeOptions? options)
    {
        return this.group.GetAsync(configureLauncher, options);
    }
}
