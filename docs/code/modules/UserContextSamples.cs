// <copyright file="UserContextSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/browsing-context.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using WebDriverBiDi;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Snippets for creating browsing contexts in user contexts. Compiled at build time to prevent API drift.
/// </summary>
public static class UserContextSamples
{
    /// <summary>
    /// Create context in user context.
    /// </summary>
    public static async Task CreateContextInUserContext(BiDiDriver driver)
    {
#region CreateContextinUserContext
        CreateUserContextCommandResult userContext =
            await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters());

        CreateCommandParameters @params = new CreateCommandParameters(CreateType.Tab)
        {
            UserContextId = userContext.UserContextId
        };
        CreateCommandResult result = await driver.BrowsingContext.CreateAsync(@params);
#endregion
    }
}
