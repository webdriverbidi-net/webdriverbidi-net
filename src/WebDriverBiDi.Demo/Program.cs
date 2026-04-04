using System.Data;
using WebDriverBiDi;
using WebDriverBiDi.Browser;
using WebDriverBiDi.Client;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Demo;
using WebDriverBiDi.DemoWebSite;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Session;

DemoWebSiteServer demoSiteServer = new();

// To use a specific port, change the below to a non-zero value.
// A value of zero (0) indicates to use a random port.
int port = 0;

// Path to the directory containing the browser launcher executables.
// We use the WebDriver Classic browser drivers (chromedriver, geckodriver, etc.)
// as browser launchers.
string browserLauncherDirectory = string.Empty;

// The level at which to log to the console in this demo app. Adjust this
// to control how verbose the logging is.
WebDriverBiDiLogLevel logReportingLevel = WebDriverBiDiLogLevel.Info;

// Select the browser type for which to run this demo.
BrowserType testBrowserType = BrowserType.Chrome;

// Optionally select the location of the browser executable to use.
// The empty string will launch the browser executable from its default
// installed location.
// You can also use a BrowserLocator to automatically locate and download a browser
// executable, using either a specific version of the browser, or the latest version
// from a specific browser distribution channel.
// Examples of using a BrowserLocator:
// BrowserLocator browserLocator = BrowserLocator.Create(FirefoxChannel.Stable);
// BrowserLocator browserLocator = BrowserLocator.Create(FirefoxChannel.Nightly);
// BrowserLocator browserLocator = BrowserLocator.Create(BrowserType.Firefox, "148.0.2");
// BrowserLocator browserLocator = BrowserLocator.Create(ChromeChannel.Stable);
// BrowserLocator browserLocator = BrowserLocator.Create(ChromeChannel.Canary);
// BrowserLocator browserLocator = BrowserLocator.Create(BrowserType.Chrome, "146.0.7680.165");
// string browserExecutableLocation = await browserLocator.LocateBrowserExecutablePathAsync();
string browserExecutableLocation = string.Empty;

// Optionally change the below value run the browsers invisibly (headless).
bool isHeadless = false;

// The amount of time to pause after execution so that the browser can
// be viewed to validate the results of the demo.
TimeSpan viewResultsDelayTimeSpan = TimeSpan.FromSeconds(3);

BrowserLauncher launcher = BrowserLauncher.Create(testBrowserType, browserLauncherDirectory, browserExecutableLocation);
launcher.Port = port;
launcher.IsBrowserHeadless = isHeadless;
launcher.OnLogMessage.AddObserver(OnLogMessage);
try
{
    await demoSiteServer.LaunchAsync();
    string baseDemoSiteUrl = $"http://localhost:{demoSiteServer.Port}";

    await launcher.StartAsync();
    await launcher.LaunchBrowserAsync();

    BiDiDriver driver = InitializeDriver(launcher.CreateTransport());
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

    await driver.StopAsync();
}
finally
{
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
    await demoSiteServer.ShutdownAsync();
}


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
