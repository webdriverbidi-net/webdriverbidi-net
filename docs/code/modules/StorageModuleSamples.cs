// <copyright file="StorageModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/storage.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Storage;

/// <summary>
/// Snippets for Storage module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class StorageModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        StorageModule storage = driver.Storage;
#endregion
    }

    /// <summary>
    /// Get all cookies.
    /// </summary>
    public static async Task GetAllCookies(BiDiDriver driver, string contextId)
    {
#region GetAllCookies
        GetCookiesCommandParameters parameters = new GetCookiesCommandParameters();
        parameters.Partition = new BrowsingContextPartitionDescriptor(contextId);

        GetCookiesCommandResult result = await driver.Storage.GetCookiesAsync(parameters);

        foreach (Cookie cookie in result.Cookies)
        {
            Console.WriteLine($"Name: {cookie.Name}");
            Console.WriteLine($"Value: {cookie.Value.Value}");
            Console.WriteLine($"Domain: {cookie.Domain}");
            Console.WriteLine($"Path: {cookie.Path}");
            Console.WriteLine($"Secure: {cookie.Secure}");
            Console.WriteLine($"HttpOnly: {cookie.HttpOnly}");
            Console.WriteLine($"SameSite: {cookie.SameSite}");
        }
#endregion
    }

    /// <summary>
    /// Get cookies by filter.
    /// </summary>
    public static async Task GetCookiesByFilter(BiDiDriver driver, string contextId)
    {
#region GetCookiesbyFilter
        GetCookiesCommandParameters parameters = new GetCookiesCommandParameters();
        parameters.Partition = new BrowsingContextPartitionDescriptor(contextId);
        parameters.Filter = new CookieFilter
        {
            Name = "sessionId"
        };

        GetCookiesCommandResult result = await driver.Storage.GetCookiesAsync(parameters);
#endregion
    }

    /// <summary>
    /// Set cookie.
    /// </summary>
    public static async Task SetCookie(BiDiDriver driver)
    {
#region SetCookie
        PartialCookie cookie = new PartialCookie(
            "sessionId",
            BytesValue.FromString("abc123"),
            "example.com")
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = CookieSameSiteValue.Strict
        };

        SetCookieCommandParameters parameters = new SetCookieCommandParameters(cookie);
        await driver.Storage.SetCookieAsync(parameters);
#endregion
    }

    /// <summary>
    /// Set cookie with expiry.
    /// </summary>
    public static async Task SetCookieWithExpiry(BiDiDriver driver)
    {
#region SetCookiewithExpiry
        PartialCookie cookie = new PartialCookie(
            "rememberMe",
            BytesValue.FromString("true"),
            "example.com")
        {
            Path = "/",
            Expires = DateTimeOffset.Now.AddDays(30).UtcDateTime
        };

        SetCookieCommandParameters parameters = new SetCookieCommandParameters(cookie);
        await driver.Storage.SetCookieAsync(parameters);
#endregion
    }

    /// <summary>
    /// Delete cookie.
    /// </summary>
    public static async Task DeleteCookie(BiDiDriver driver)
    {
#region DeleteCookie
        CookieFilter filter = new CookieFilter
        {
            Name = "sessionId",
            Domain = "example.com"
        };

        DeleteCookiesCommandParameters parameters = new DeleteCookiesCommandParameters();
        parameters.Filter = filter;

        await driver.Storage.DeleteCookiesAsync(parameters);
#endregion
    }

    /// <summary>
    /// Delete all cookies.
    /// </summary>
    public static async Task DeleteAllCookies(BiDiDriver driver, string contextId)
    {
#region DeleteAllCookies
        DeleteCookiesCommandParameters parameters = new DeleteCookiesCommandParameters();
        parameters.Partition = new BrowsingContextPartitionDescriptor(contextId);

        await driver.Storage.DeleteCookiesAsync(parameters);
#endregion
    }

    /// <summary>
    /// Save and restore session cookies.
    /// </summary>
    public static async Task SaveAndRestoreSession(BiDiDriver driver, string contextId)
    {
#region SaveandRestoreSession
        // Save cookies
        GetCookiesCommandResult savedCookies = await driver.Storage.GetCookiesAsync(
            new GetCookiesCommandParameters
            {
                Partition = new BrowsingContextPartitionDescriptor(contextId)
            });

        // ... later, restore cookies
        foreach (Cookie cookie in savedCookies.Cookies)
        {
            PartialCookie newCookie = new PartialCookie(cookie.Name, cookie.Value, cookie.Domain)
            {
                Path = cookie.Path,
                Secure = cookie.Secure,
                HttpOnly = cookie.HttpOnly,
                SameSite = cookie.SameSite,
                Expires = cookie.Expires
            };

            await driver.Storage.SetCookieAsync(new SetCookieCommandParameters(newCookie));
        }
#endregion
    }

    /// <summary>
    /// Clean state between tests.
    /// </summary>
    public static async Task CleanStateBetweenTests(BiDiDriver driver, string contextId)
    {
#region CleanStateBetweenTests
        // Clear all cookies
        await driver.Storage.DeleteCookiesAsync(
            new DeleteCookiesCommandParameters
            {
                Partition = new BrowsingContextPartitionDescriptor(contextId)
            });
#endregion
    }

    /// <summary>
    /// Set authentication cookie.
    /// </summary>
    public static async Task SetAuthenticationCookie(BiDiDriver driver, string contextId)
    {
#region SetAuthenticationCookie
        PartialCookie authCookie = new PartialCookie(
            "authToken",
            BytesValue.FromString("your-auth-token"),
            "example.com")
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = CookieSameSiteValue.Strict
        };

        SetCookieCommandParameters parameters = new SetCookieCommandParameters(authCookie);
        await driver.Storage.SetCookieAsync(parameters);

        // Now navigate - cookie will be sent
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com/dashboard"));
#endregion
    }
}
