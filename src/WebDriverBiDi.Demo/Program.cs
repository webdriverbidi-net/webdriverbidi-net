using WebDriverBiDi;
using WebDriverBiDi.Browser;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Demo;
using WebDriverBiDi.DemoWebSite;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Session;

// DemoWebSiteServer implements IAsyncDisposable, so it will automatically clean
// up the server process without needing to explicitly call ShutdownAsync().
await using DemoWebSiteServer demoSiteServer = new();
await demoSiteServer.LaunchAsync();
string baseDemoSiteUrl = $"http://localhost:{demoSiteServer.Port}";

// To use a specific port, change the below to a non-zero value.
// A value of zero (0) indicates to use a random port.
int port = 0;

// The level at which to log to the console in this demo app. Adjust this
// to control how verbose the logging is.
WebDriverBiDiLogLevel logReportingLevel = WebDriverBiDiLogLevel.Info;

// Select the browser type for which to run this demo.
Browser testBrowserType = Browser.Chrome;

// Select the release channel of the browser tu use for this demo.
// The release channel is browser-agnostic, but is mapped to a
// browser-specific value when creating a browser launcher.
BrowserReleaseChannel releaseChannel = BrowserReleaseChannel.Stable;

// Optionally change the below value run the browsers invisibly (headless).
bool isHeadless = false;

// The amount of time to pause after execution so that the browser can
// be viewed to validate the results of the demo.
TimeSpan viewResultsDelayTimeSpan = TimeSpan.FromSeconds(3);

// Create and configure the browser launcher. Note that you can change how the browser
// is launched (e.g., using a driver executable, connecting to a remote grid) by changing
// the builder methods used here. See the BrowserLauncherBuilder class for more details.
BrowserLauncherBuilder launcherBuilder = BrowserLauncher.Configure(testBrowserType)
    .WithReleaseChannel(releaseChannel)
    .AtAutomaticallyDownloadedLocation()
    .WithPort(port)
    .WithHeadlessOption(isHeadless);

// BrowserLauncher implements IAsyncDisposable, so it will automatically clean up any
// launched browser processes calling DisposeAsync(), which, in turn, calls
// QuitBrowserAsync() and StopAsync() to ensure proper cleanup of resources.
await using BrowserLauncher launcher = launcherBuilder.Build();
launcher.OnLogMessage.AddObserver(OnLogMessage);

await launcher.StartAsync();
await launcher.LaunchBrowserAsync();

// BiDiDriver also implements IAsyncDisposable, so it will clean up the session
// and transport when disposed, calling DisposeAsync(), which also calls StopAsync().
await using BiDiDriver driver = InitializeDriver(launcher.CreateTransport());
await driver.StartAsync(launcher.WebSocketUrl);

if (!launcher.IsBiDiSessionInitialized)
{
    // Using a classic WebDriver browser driver to launch the browser
    // automatically gives you a WebDriver BiDi session. Without the
    // driver executable, you must start your own session.
    await driver.Session.NewSessionAsync(new NewCommandParameters());
}

await DemoScenarios.SubmitFormAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.WaitForDelayLoadAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.ManipulateCookiesAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.MonitorNetworkTrafficAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.MonitorBrowserConsoleAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.ExecuteJavaScriptFunctionsAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.ExecutePreloadScriptAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.InterceptBeforeRequestSentEventAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.InterceptAndReplaceNetworkDataAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.ExecuteElementRoundtripInJavaScriptAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.CaptureNetworkResponseAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.CaptureAllNetworkTrafficAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.HandleEventsInMultipleUserContextsAsync(driver, baseDemoSiteUrl);
// await DemoScenarios.ManipulateShadowRootsAsync(driver, baseDemoSiteUrl);

Console.WriteLine($"Pausing {viewResultsDelayTimeSpan.TotalSeconds} seconds to view results");
await Task.Delay(viewResultsDelayTimeSpan);

if (launcher.IsBrowserCloseAllowed)
{
    await driver.Browser.CloseAsync(new CloseCommandParameters());
}

// The driver, launcher, and demo web site server will all be automatically
// cleaned up when they go out of scope. Without using the `await using` pattern,
// you would need to explicitly call StopAsync() on the driver, QuitBrowserAsync()
// and StopAsync() on the launcher, and ShutdownAsync() on the server, in a
// `finally` block to ensure proper cleanup of resources. The below call to
// StopAsync() is not strictly necessary when using `await using`, but is included
// here to demonstrate that you can call it directly if needed to stop the driver
// before the variable goes out of scope.
await driver.StopAsync();

BiDiDriver InitializeDriver(Transport transport)
{
    BiDiDriver driver = new(TimeSpan.FromSeconds(10), transport);
    driver.OnLogMessage.AddObserver(OnLogMessage);
    driver.BrowsingContext.OnNavigationStarted.AddObserver((e) =>
    {
        Console.WriteLine($"Navigation to {e.Url} started");
    });

    driver.BrowsingContext.OnLoad.AddObserver((e) =>
    {
        Console.WriteLine($"Load of {e.Url} complete!");
    });

    return driver;
}

void OnLogMessage(LogMessageEventArgs e)
{
    if (e.Level >= logReportingLevel)
    {
        Console.WriteLine($"Log message: {e.Message}");
    }
}
