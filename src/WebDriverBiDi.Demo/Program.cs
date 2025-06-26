using WebDriverBiDi;
using WebDriverBiDi.Client;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.DemoWebSite;
using WebDriverBiDi.Demo;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Session;
using WebDriverBiDi.Browser;

DemoWebSiteServer demoSiteServer = new();

// To use a specific port, uncomment the line below and modify it to
// use the desired port.
// int port = 38267;

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
string browserExecutableLocation = string.Empty;

BrowserLauncher launcher = BrowserLauncher.Create(testBrowserType, browserLauncherDirectory, browserExecutableLocation);
launcher.OnLogMessage.AddObserver(OnLogMessage);
try
{
    demoSiteServer.Launch();
    string baseDemoSiteUrl = $"http://localhost:{demoSiteServer.Port}";

    await launcher.StartAsync();
    await launcher.LaunchBrowserAsync();

    BiDiDriver driver = InitializeDriver(launcher.CreateTransport());
    await driver.StartAsync(launcher.WebSocketUrl);

    if (testBrowserType == BrowserType.Chrome || testBrowserType == BrowserType.Firefox)
    {
        // Using a classic WebDriver browser driver to launch the browser
        // automatically gives you a WebDriver BiDi session. Without the
        // driver executable, you must start your own session.
        await driver.Session.NewSessionAsync(new NewCommandParameters());
    }

    await DemoScenarios.SubmitFormAsync(driver, baseDemoSiteUrl);
    // await DemoScenarios.WaitForDelayLoadAsync(driver, baseDemoSiteUrl);
    // await DemoScenarios.MonitorNetworkTraffic(driver, baseDemoSiteUrl);
    // await DemoScenarios.MonitorBrowserConsole(driver, baseDemoSiteUrl);
    // await DemoScenarios.ExecuteJavaScriptFunctions(driver, baseDemoSiteUrl);
    // await DemoScenarios.ExecutePreloadScript(driver, baseDemoSiteUrl);
    // await DemoScenarios.InterceptBeforeRequestSentEvent(driver, baseDemoSiteUrl);
    // await DemoScenarios.InterceptAndReplaceNetworkData(driver, baseDemoSiteUrl);
    // await DemoScenarios.ExecuteElementRoundtripInJavaScript(driver, baseDemoSiteUrl);
    // await DemoScenarios.CaptureNetworkResponse(driver, baseDemoSiteUrl);

    Console.WriteLine("Pausing 3 seconds to view results");
    await Task.Delay(TimeSpan.FromSeconds(3));

    await driver.Browser.CloseAsync(new CloseCommandParameters());
    await driver.StopAsync();
}
finally
{
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
    demoSiteServer.Shutdown();
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
