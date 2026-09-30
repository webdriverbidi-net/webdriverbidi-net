// <copyright file="CookieTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text;
using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Network;

public class CookieTests
{
    [Fact]
    public async Task CookiesAreReadFromTheBrowsersUserContext()
    {
        (BiDiDriver driver, FakeSession session, Browser browser) = await OpenBrowserAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("storage.getCookies", new JsonObject()
        {
            ["cookies"] = new JsonArray(
                Cookie("text", new JsonObject() { ["type"] = "string", ["value"] = "plain" }, expiry: 1790000000),
                Cookie("bytes", new JsonObject() { ["type"] = "base64", ["value"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("décoded")) }, expiry: null)),
            ["partitionKey"] = new JsonObject() { ["userContext"] = browser.Id },
        });

        IReadOnlyList<BrowserCookie> all = await browser.GetCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);
        await browser.GetCookiesAsync(domain: "example.com", name: "text", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(new BrowserCookie("text", "plain", "example.com") { Path = "/", HttpOnly = true, Secure = false, SameSite = CookieSameSiteValue.Lax, Expires = DateTimeOffset.FromUnixTimeSeconds(1790000000).UtcDateTime }, all[0]);
        Assert.Equal("décoded", all[1].Value);
        Assert.Null(all[1].Expires);
        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("storage.getCookies");
        Assert.All(commands, command =>
        {
            Assert.Equal("storageKey", (string?)command["params"]!["partition"]!["type"]);
            Assert.Equal(browser.Id, (string?)command["params"]!["partition"]!["userContext"]);
        });
        Assert.False(commands[0]["params"]!.AsObject().ContainsKey("filter"));
        Assert.Equal("example.com", (string?)commands[1]["params"]!["filter"]!["domain"]);
        Assert.Equal("text", (string?)commands[1]["params"]!["filter"]!["name"]);
    }

    [Fact]
    public async Task CookiesAreAddedOneByOneToTheBrowsersUserContext()
    {
        (BiDiDriver driver, FakeSession session, Browser browser) = await OpenBrowserAsync();
        await using BiDiDriver ownedDriver = driver;
        DateTime expires = new(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        session.RemoteEnd.AnswerWith("storage.setCookie", new JsonObject() { ["partitionKey"] = new JsonObject() });

        await browser.AddCookiesAsync(
            [
                new BrowserCookie("first", "1", "example.com") { Path = "/app", HttpOnly = true, Secure = true, SameSite = CookieSameSiteValue.Strict, Expires = expires },
                new BrowserCookie("second", "2", "example.com"),
            ],
            TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("storage.setCookie");
        Assert.Equal(2, commands.Count);
        JsonObject first = commands[0]["params"]!.AsObject();
        Assert.Equal(browser.Id, (string?)first["partition"]!["userContext"]);
        JsonObject cookie = first["cookie"]!.AsObject();
        Assert.Equal("first", (string?)cookie["name"]);
        Assert.Equal("1", (string?)cookie["value"]!["value"]);
        Assert.Equal("example.com", (string?)cookie["domain"]);
        Assert.Equal("/app", (string?)cookie["path"]);
        Assert.True((bool?)cookie["httpOnly"]);
        Assert.True((bool?)cookie["secure"]);
        Assert.Equal("strict", (string?)cookie["sameSite"]);
        Assert.Equal(new DateTimeOffset(expires).ToUnixTimeSeconds(), (long?)cookie["expiry"]);
        Assert.False(commands[1]["params"]!["cookie"]!.AsObject().ContainsKey("path"));
    }

    [Fact]
    public async Task CookiesAreClearedFromTheBrowsersUserContext()
    {
        (BiDiDriver driver, FakeSession session, Browser browser) = await OpenBrowserAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("storage.deleteCookies", new JsonObject() { ["partitionKey"] = new JsonObject() });

        await browser.ClearCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);
        await browser.ClearCookiesAsync(name: "session", cancellationToken: TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("storage.deleteCookies");
        Assert.All(commands, command => Assert.Equal(browser.Id, (string?)command["params"]!["partition"]!["userContext"]));
        Assert.False(commands[0]["params"]!.AsObject().ContainsKey("filter"));
        Assert.Equal("session", (string?)commands[1]["params"]!["filter"]!["name"]);
        Assert.False(commands[1]["params"]!["filter"]!.AsObject().ContainsKey("domain"));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Browser Browser)> OpenBrowserAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Browser browser = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, browser);
    }

    private static JsonObject Cookie(string name, JsonObject value, long? expiry)
    {
        JsonObject cookie = new() { ["name"] = name, ["value"] = value, ["domain"] = "example.com", ["path"] = "/", ["size"] = 10, ["httpOnly"] = true, ["secure"] = false, ["sameSite"] = "lax" };
        if (expiry is not null)
        {
            cookie["expiry"] = expiry;
        }

        return cookie;
    }
}
