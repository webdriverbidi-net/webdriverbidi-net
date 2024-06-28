using WebDriverBiDi;
using WebDriverBiDi.Client;
using WebDriverBiDi.DemoWebSite;
using WebDriverBiDi.Demo;

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

    BiDiDriver driver = InitializeDriver();
    await driver.StartAsync(launcher.WebSocketUrl);

    await DemoScenarios.SubmitFormAsync(driver, baseDemoSiteUrl);
    // await DemoScenarios.WaitForDelayLoadAsync(driver, baseDemoSiteUrl);
    // await DemoScenarios.MonitorNetworkTraffic(driver, baseDemoSiteUrl);
    // await DemoScenarios.MonitorBrowserConsole(driver, baseDemoSiteUrl);
    // await DemoScenarios.ExecuteJavaScriptFunctions(driver, baseDemoSiteUrl);
    // await DemoScenarios.InterceptBeforeRequestSentEvent(driver, baseDemoSiteUrl);
    // await DemoScenarios.InterceptAndReplaceNetworkData(driver, baseDemoSiteUrl);

    Console.WriteLine("Pausing 3 seconds to view results");
    await Task.Delay(TimeSpan.FromSeconds(3));

    await driver.StopAsync();
}
finally
{
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
    demoSiteServer.Shutdown();
}

BiDiDriver InitializeDriver()
{
    BiDiDriver driver = new(TimeSpan.FromSeconds(10));
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
