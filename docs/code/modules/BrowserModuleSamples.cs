// <copyright file="BrowserModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/browser.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using CloseCommandParameters = WebDriverBiDi.Browser.CloseCommandParameters;
using BrowsingContextCloseCommandParameters = WebDriverBiDi.BrowsingContext.CloseCommandParameters;

/// <summary>
/// Snippets for Browser module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class BrowserModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        BrowserModule browser = driver.Browser;
#endregion
    }

    /// <summary>
    /// Create user context.
    /// </summary>
    public static async Task CreateUserContext(BiDiDriver driver)
    {
#region CreateUserContext
        CreateUserContextCommandParameters parameters = new CreateUserContextCommandParameters();
        CreateUserContextCommandResult result = await driver.Browser.CreateUserContextAsync(parameters);

        string userContextId = result.UserContextId;
        Console.WriteLine($"Created user context: {userContextId}");
#endregion
    }

    /// <summary>
    /// Get user contexts.
    /// </summary>
    public static async Task GetUserContexts(BiDiDriver driver)
    {
#region GetUserContexts
        GetUserContextsCommandParameters parameters = new GetUserContextsCommandParameters();
        GetUserContextsCommandResult result = await driver.Browser.GetUserContextsAsync(parameters);

        foreach (UserContextInfo context in result.UserContexts)
        {
            Console.WriteLine($"User context: {context.UserContextId}");
        }
#endregion
    }

    /// <summary>
    /// Remove user context.
    /// </summary>
    public static async Task RemoveUserContext(BiDiDriver driver, string userContextId)
    {
#region RemoveUserContext
        RemoveUserContextCommandParameters @params =
            new RemoveUserContextCommandParameters(userContextId);

        await driver.Browser.RemoveUserContextAsync(@params);
#endregion
    }

    /// <summary>
    /// Create tab in user context.
    /// </summary>
    public static async Task CreateTabInUserContext(BiDiDriver driver)
    {
#region CreateTabinUserContext
        CreateUserContextCommandResult userContextResult = 
            await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters());

        // Create browsing context in that user context
        CreateCommandParameters createTabParams = new CreateCommandParameters(CreateType.Tab)
        {
            UserContextId = userContextResult.UserContextId
        };

        CreateCommandResult tabResult = await driver.BrowsingContext.CreateAsync(createTabParams);
#endregion
    }

    /// <summary>
    /// Get client windows.
    /// </summary>
    public static async Task GetClientWindows(BiDiDriver driver)
    {
#region GetClientWindows
        GetClientWindowsCommandParameters parameters = new GetClientWindowsCommandParameters();
        GetClientWindowsCommandResult result = await driver.Browser.GetClientWindowsAsync(parameters);

        foreach (ClientWindowInfo window in result.ClientWindows)
        {
            Console.WriteLine($"Window: {window.ClientWindowId}");
            Console.WriteLine($"  State: {window.State}");
            Console.WriteLine($"  Width: {window.Width}");
            Console.WriteLine($"  Height: {window.Height}");
        }
#endregion
    }

    /// <summary>
    /// Set window state.
    /// </summary>
    public static async Task SetWindowState(BiDiDriver driver)
    {
#region SetWindowState
    // Get the client window
    GetClientWindowsCommandResult windowsResult = 
        await driver.Browser.GetClientWindowsAsync(new GetClientWindowsCommandParameters());

    string clientWindowId = windowsResult.ClientWindows[0].ClientWindowId;

    // Maximize window
    SetClientWindowStateCommandParameters parameters = 
        new SetClientWindowStateCommandParameters(clientWindowId)
        {
            State = ClientWindowState.Maximized,
        };

    await driver.Browser.SetClientWindowStateAsync(parameters);

    // Other states: Minimized, Fullscreen, Normal
#endregion
    }

    /// <summary>
    /// Set window size.
    /// </summary>
    public static async Task SetWindowSize(BiDiDriver driver, string clientWindowId)
    {
#region SetWindowSize
        SetClientWindowStateCommandParameters parameters =
            new SetClientWindowStateCommandParameters(clientWindowId)
            {
                State = ClientWindowState.Normal,
                Width = 1280,
                Height = 720
            };

        await driver.Browser.SetClientWindowStateAsync(parameters);
#endregion
    }

    /// <summary>
    /// Allow downloads.
    /// </summary>
    public static async Task AllowDownloads(BiDiDriver driver)
    {
#region AllowDownloads
        SetDownloadBehaviorCommandParameters parameters = new SetDownloadBehaviorCommandParameters();
        parameters.DownloadBehavior = new DownloadBehaviorAllowed("/path/to/downloads");

        await driver.Browser.SetDownloadBehaviorAsync(parameters);
#endregion
    }

    /// <summary>
    /// Deny downloads.
    /// </summary>
    public static async Task DenyDownloads(BiDiDriver driver)
    {
#region DenyDownloads
        SetDownloadBehaviorCommandParameters parameters = new SetDownloadBehaviorCommandParameters();
        parameters.DownloadBehavior = new DownloadBehaviorDenied();

        await driver.Browser.SetDownloadBehaviorAsync(parameters);
#endregion
    }

    /// <summary>
    /// Close browser.
    /// </summary>
    public static async Task CloseBrowser(BiDiDriver driver)
    {
#region CloseBrowser
        CloseCommandParameters parameters = new CloseCommandParameters();
        await driver.Browser.CloseAsync(parameters);
#endregion
    }

    /// <summary>
    /// Isolated session pattern.
    /// </summary>
    public static async Task IsolatedSessionPattern(BiDiDriver driver)
    {
#region IsolatedSessionPattern
        // Create isolated user context
        CreateUserContextCommandResult userContext = 
            await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters());

        // Create tab in isolated context
        CreateCommandParameters tabParams = new CreateCommandParameters(CreateType.Tab)
        {
            UserContextId = userContext.UserContextId
        };
        CreateCommandResult tab = await driver.BrowsingContext.CreateAsync(tabParams);

        // Use the tab...
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(tab.BrowsingContextId, "https://example.com"));

        // Clean up: close tab and remove context
        // WebDriverBiDi.BrowsingContext.CloseCommandParameters is aliased as BrowsingContextCloseCommandParameters.
        await driver.BrowsingContext.CloseAsync(
            new BrowsingContextCloseCommandParameters(tab.BrowsingContextId));
        await driver.Browser.RemoveUserContextAsync(
            new RemoveUserContextCommandParameters(userContext.UserContextId));
#endregion
    }

    /// <summary>
    /// Multi-window testing.
    /// </summary>
    public static async Task MultiWindowTesting(BiDiDriver driver)
    {
#region Multi-WindowTesting
        // Create multiple windows
        List<string> windowIds = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            CreateCommandResult window = await driver.BrowsingContext.CreateAsync(
                new CreateCommandParameters(CreateType.Window));
            windowIds.Add(window.BrowsingContextId);
        }

        // Get client windows and manipulate them
        GetClientWindowsCommandResult clientWindows =
            await driver.Browser.GetClientWindowsAsync(new GetClientWindowsCommandParameters());

        foreach (ClientWindowInfo window in clientWindows.ClientWindows)
        {
            // Tile windows side by side
            await driver.Browser.SetClientWindowStateAsync(
                new SetClientWindowStateCommandParameters(window.ClientWindowId)
                {
                    State = ClientWindowState.Normal,
                    Width = 640,
                    Height = 480
                });
        }
#endregion
    }
}
