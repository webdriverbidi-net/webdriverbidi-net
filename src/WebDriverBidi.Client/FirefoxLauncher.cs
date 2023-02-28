using System.Runtime.InteropServices;

namespace WebDriverBidi.Client;

public class FirefoxLauncher : BrowserLauncher
{
    private const string DefaultFirefoxLauncherFileName = "geckodriver";

    public FirefoxLauncher(string launcherPath)
        : this(launcherPath, FirefoxLauncherFileName(), 0)
    {
    }

    public FirefoxLauncher(string launcherPath, string executableName)
        : this(launcherPath, executableName, 0)
    {
    }

    public FirefoxLauncher(string launcherPath, string executableName, int port)
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
            ["browserName"] = "firefox",
            ["webSocketUrl"] = true
        };

        return capabilities;
    }

    /// <summary>
    /// Returns the Firefox driver filename for the currently running platform
    /// </summary>
    /// <returns>The file name of the Firefox driver service executable.</returns>
    private static string FirefoxLauncherFileName()
    {
        string fileName = DefaultFirefoxLauncherFileName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName += ".exe";
        }

        return fileName;
    }
}
