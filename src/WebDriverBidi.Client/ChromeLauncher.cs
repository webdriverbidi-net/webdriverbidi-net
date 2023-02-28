using System.Runtime.InteropServices;

namespace WebDriverBidi.Client;

public class ChromeLauncher : BrowserLauncher
{
    private const string DefaultChromeLauncherFileName = "chromedriver";

    public ChromeLauncher(string launcherPath)
        : this(launcherPath, ChromeLauncherFileName(), 0)
    {
    }

    public ChromeLauncher(string launcherPath, string executableName)
        : this(launcherPath, executableName, 0)
    {
    }

    public ChromeLauncher(string launcherPath, string executableName, int port)
        : base(launcherPath, executableName, port)
    {
    }

    /// <summary>
    /// Gets a value indicating the time to wait for the service to terminate before forcing it to terminate.
    /// </summary>
    protected override TimeSpan TerminationTimeout => TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets a value indicating whether the service has a shutdown API that can be called to terminate
    /// it gracefully before forcing a termination.
    /// </summary>
    protected override bool HasShutdownApi => false;

    /// <summary>
    /// Creates the WebDriver Classic capabilities used to launch the browser.
    /// </summary>
    /// <returns>A dictionary containing the capabilities.</returns>
    protected override Dictionary<string, object> CreateBrowserLaunchCapabilities()
    {
        // TODO: Create a more fully-featured generation of capabilities.
        Dictionary<string, object> capabilities = new()
        {
            ["browserName"] = "chrome",
            ["webSocketUrl"] = true
        };

        return capabilities;
    }

    /// <summary>
    /// Returns the Firefox driver filename for the currently running platform
    /// </summary>
    /// <returns>The file name of the Firefox driver service executable.</returns>
    private static string ChromeLauncherFileName()
    {
        string fileName = DefaultChromeLauncherFileName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName += ".exe";
        }

        return fileName;
    }
}
