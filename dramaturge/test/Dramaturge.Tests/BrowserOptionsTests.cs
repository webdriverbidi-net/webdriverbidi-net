// <copyright file="BrowserOptionsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Session;

public class BrowserOptionsTests
{
    [Fact]
    public async Task BrowserWithoutOptionsKeepsTheDefaults()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        int commandsBefore = session.RemoteEnd.SentCommands.Count;

        await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);

        JsonObject command = Assert.Single(session.RemoteEnd.SentCommands.Skip(commandsBefore));
        Assert.Equal("browser.createUserContext", (string?)command["method"]);
        Assert.Empty(command["params"]!.AsObject());
    }

    [Fact]
    public async Task EveryOptionIsAppliedToTheUserContext()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        BrowserOptions options = new()
        {
            AcceptInsecureCerts = true,
            Proxy = new DirectProxyConfiguration(),
            UnhandledPromptBehavior = new UserPromptHandler() { Default = UserPromptHandlerType.Accept },
            Viewport = new Viewport() { Width = 800, Height = 600 },
            DevicePixelRatio = 2,
            Locale = "de-DE",
            TimeZone = "Europe/Berlin",
            UserAgent = "Custom/1.0",
            MediaFeatures = new MediaFeatures() { AnyHover = AnyHoverMediaFeatureValue.None },
            Geolocation = new GeolocationCoordinates(52.52, 13.405),
            Permissions =
            {
                new PermissionGrant("geolocation", PermissionState.Granted, "https://example.com"),
                new PermissionGrant(new PermissionDescriptor("notifications"), PermissionState.Denied, "https://example.org"),
            },
        };

        Browser browser = await group.CreateBrowserAsync(options, TestContext.Current.CancellationToken);

        JsonObject created = Parameters(session, "browser.createUserContext");
        Assert.True((bool?)created["acceptInsecureCerts"]);
        Assert.Equal("direct", (string?)created["proxy"]!["proxyType"]);
        Assert.Equal("accept", (string?)created["unhandledPromptBehavior"]!["default"]);
        JsonObject viewport = Parameters(session, "browsingContext.setViewport");
        Assert.Equal(800, (int?)viewport["viewport"]!["width"]);
        Assert.Equal(600, (int?)viewport["viewport"]!["height"]);
        Assert.Equal(2, (double?)viewport["devicePixelRatio"]);
        AssertForUserContext(viewport, browser);
        Assert.Equal("de-DE", (string?)AssertForUserContext(Parameters(session, "emulation.setLocaleOverride"), browser)["locale"]);
        Assert.Equal("Europe/Berlin", (string?)AssertForUserContext(Parameters(session, "emulation.setTimezoneOverride"), browser)["timezone"]);
        Assert.Equal("Custom/1.0", (string?)AssertForUserContext(Parameters(session, "emulation.setUserAgentOverride"), browser)["userAgent"]);
        Assert.Equal("none", (string?)AssertForUserContext(Parameters(session, "emulation.setMediaFeaturesOverride"), browser)["features"]!["any-hover"]);
        Assert.Equal(52.52, (double?)AssertForUserContext(Parameters(session, "emulation.setGeolocationOverride"), browser)["coordinates"]!["latitude"]);
        IReadOnlyList<JsonObject> permissions = session.RemoteEnd.CommandsFor("permissions.setPermission");
        Assert.Equal(["geolocation:granted:https://example.com", "notifications:denied:https://example.org"], permissions.Select(command => $"{command["params"]!["descriptor"]!["name"]}:{command["params"]!["state"]}:{command["params"]!["origin"]}"));
        Assert.All(permissions, command => Assert.Equal(browser.Id, (string?)command["params"]!["userContext"]));
    }

    [Fact]
    public async Task DevicePixelRatioAloneKeepsTheViewportSize()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        await group.CreateBrowserAsync(new BrowserOptions() { DevicePixelRatio = 3 }, TestContext.Current.CancellationToken);

        JsonObject viewport = Parameters(session, "browsingContext.setViewport");
        Assert.False(viewport.ContainsKey("viewport"));
        Assert.Equal(3, (double?)viewport["devicePixelRatio"]);
    }

    [Fact]
    public async Task OptionThatCannotBeAppliedRemovesTheBrowser()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.FailWith("emulation.setLocaleOverride", "invalid argument", "unknown locale");

        WebDriverBiDiCommandException exception = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => group.CreateBrowserAsync(new BrowserOptions() { Locale = "xx", TimeZone = "UTC" }, TestContext.Current.CancellationToken));

        Assert.Contains("unknown locale", exception.Message);
        Assert.Equal([Browser.DefaultBrowserId], group.Browsers.Select(browser => browser.Id));
        Assert.Equal([Browser.DefaultBrowserId], session.UserContextIds);
        Assert.Empty(session.RemoteEnd.CommandsFor("emulation.setTimezoneOverride"));
    }

    [Fact]
    public async Task BrowserThatCannotBeRemovedAfterAFailedOptionIsReported()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.FailWith("emulation.setUserAgentOverride", "invalid argument", "bad user agent");
        session.RemoteEnd.FailWith("browser.removeUserContext", "unknown error", "cannot remove");
        List<string> messages = [];
        group.OnLogMessage.AddObserver(e => messages.Add(e.Message));

        WebDriverBiDiCommandException exception = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => group.CreateBrowserAsync(new BrowserOptions() { UserAgent = "?" }, TestContext.Current.CancellationToken));

        Assert.Contains("bad user agent", exception.Message);
        Assert.Contains("cannot remove", Assert.Single(messages));
    }

    [Fact]
    public void PermissionNamedAloneHasADescriptorOfThatName()
    {
        PermissionGrant grant = new("camera", PermissionState.Prompt, "https://example.com");

        Assert.Equal("camera", grant.Descriptor.Name);
        Assert.Equal(PermissionState.Prompt, grant.State);
        Assert.Equal("https://example.com", grant.Origin);
    }

    private static JsonObject Parameters(FakeSession session, string method)
    {
        return Assert.Single(session.RemoteEnd.CommandsFor(method))["params"]!.AsObject();
    }

    private static JsonObject AssertForUserContext(JsonObject parameters, Browser browser)
    {
        Assert.Equal([browser.Id], parameters["userContexts"]!.AsArray().Select(node => (string?)node));
        return parameters;
    }
}
