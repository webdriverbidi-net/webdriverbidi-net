// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Arguments: [browser] [browser executable]. The browser is firefox (the default) or chrome, launched directly and
// headless; without an executable, the downloaded Nightly or Canary is used. The page is answered by a route, so no
// server or network is needed. Run the natively published application to check Dramaturge under native AOT.
using System.Text;
using System.Text.RegularExpressions;
using Dramaturge;
using Dramaturge.Browsers;
using Dramaturge.Network;
using static Dramaturge.Assertions;

const string PageUrl = "https://aot.dramaturge.test/";
const string PageHtml = """
    <!DOCTYPE html>
    <html>
      <head><title>Dramaturge AOT test page</title></head>
      <body>
        <h1>Dramaturge</h1>
        <label>Name <input id="name"></label>
        <button onclick="document.getElementById('status').textContent = 'Hello, ' + document.getElementById('name').value">Greet</button>
        <p id="status" role="status">Waiting</p>
      </body>
    </html>
    """;

string browserName = args.Length > 0 ? args[0].ToLowerInvariant() : "firefox";
string executable = args.Length > 1 ? args[1] : string.Empty;
BrowserKind kind = browserName switch
{
    "firefox" => BrowserKind.Firefox,
    "chrome" => BrowserKind.Chrome,
    _ => throw new ArgumentException($"Unknown browser '{browserName}'; use firefox or chrome."),
};

BrowserLauncherBuilder builder = BrowserLauncher.Configure(kind).WithReleaseChannel(BrowserReleaseChannel.Alpha).WithHeadlessOption();
if (executable.Length > 0)
{
    builder.AtLocation(executable);
}

List<string> checks = [];
try
{
    await using BrowserGroup group = await BrowserGroup.LaunchAsync(builder);
    Page page = await group.DefaultBrowser.NewPageAsync();
    checks.Add("launch");

    await page.RouteAsync(PageUrl, route => route.FulfillAsync(200, PageHtml, new Dictionary<string, string>() { ["Content-Type"] = "text/html" }));
    NetworkTrafficMonitorOptions monitorOptions = new();
    monitorOptions.BrowsingContextIds.Add(page.Id);
    await using NetworkTrafficMonitor monitor = new(group.Driver, monitorOptions);
    await monitor.StartMonitoringAsync();

    await page.NavigateAsync(PageUrl);
    await Expect(page).ToHaveTitleAsync("Dramaturge AOT test page");
    await Expect(page).ToHaveUrlAsync(new Regex("aot\\.dramaturge\\.test"));
    checks.Add("route and navigation");

    await page.GetByLabel("Name").FillAsync("Ada");
    await Expect(page.GetByLabel("Name")).ToHaveValueAsync("Ada");
    await page.GetByRole("button", "Greet").ClickAsync();
    await Expect(page.GetByRole("status")).ToHaveTextAsync("Hello, Ada");
    await Expect(page.Locate(new WebDriverBiDi.BrowsingContext.CssLocator("h1"))).ToHaveTextAsync("Dramaturge");
    checks.Add("locators, actions, and expectations");

    double sum = await page.EvaluateAsync<double>("() => 40 + 2");
    string[] words = await page.EvaluateAsync<string[]>("() => ['one', 'two']");
    Dictionary<string, object?> map = await page.EvaluateAsync<Dictionary<string, object?>>("() => ({ name: 'Ada', year: 1815, tags: ['a', 'b'] })");
    Require(sum == 42, $"the number was {sum}");
    Require(words.SequenceEqual(["one", "two"]), $"the strings were {string.Join(", ", words)}");
    Require(map["name"] is string name && name == "Ada", "the dictionary's name was wrong");
    Require(map["tags"] is IList<object?> { Count: 2 }, "the dictionary's list was wrong");
    checks.Add("typed evaluation");

    byte[] screenshot = await page.ScreenshotAsync();
    Require(screenshot.Length > 8 && screenshot[1] == (byte)'P' && screenshot[2] == (byte)'N' && screenshot[3] == (byte)'G', "the screenshot was not a PNG");
    checks.Add("screenshot");

    IReadOnlyList<NetworkRequest> traffic = await monitor.GetCapturedTrafficAsync(TimeSpan.FromSeconds(10));
    string har = HarGenerator.Generate(traffic);
    Require(har.Contains(PageUrl, StringComparison.Ordinal), "the HAR did not record the page's request");
    checks.Add("network capture and HAR");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FAIL after [{string.Join(", ", checks)}]: {ex.GetType().Name}: {ex.Message}");
    return 1;
}

Console.WriteLine($"PASS ({browserName}): {string.Join(", ", checks)}.");
return 0;

static void Require(bool condition, string failure)
{
    if (!condition)
    {
        throw new InvalidOperationException(failure);
    }
}
