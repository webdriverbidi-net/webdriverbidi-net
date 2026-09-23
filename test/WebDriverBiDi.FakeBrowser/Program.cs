// Stands in for a browser executable in launcher tests. It reports readiness the way the real
// browser does, using the remote debugging port the launcher passes, then waits to be killed.
// WEBDRIVERBIDI_FAKE_BROWSER_MODE selects a failure instead: "exit:<code>" exits at once with
// that code, and "silent" never reports readiness.
string? mode = Environment.GetEnvironmentVariable("WEBDRIVERBIDI_FAKE_BROWSER_MODE");
if (mode is not null && mode.StartsWith("exit:", StringComparison.Ordinal))
{
    return int.Parse(mode["exit:".Length..]);
}

if (mode != "silent")
{
    // Chrome takes "--remote-debugging-port=<port>"; Firefox takes the port as the next argument.
    const string ChromePortPrefix = "--remote-debugging-port=";
    string? chromePort = args.FirstOrDefault(arg => arg.StartsWith(ChromePortPrefix, StringComparison.Ordinal))?[ChromePortPrefix.Length..];
    int firefoxPortIndex = Array.IndexOf(args, "--remote-debugging-port") + 1;
    if (chromePort is not null)
    {
        Console.Error.WriteLine($"DevTools listening on ws://127.0.0.1:{chromePort}/devtools/browser/fake-browser");
    }
    else if (firefoxPortIndex > 0 && firefoxPortIndex < args.Length)
    {
        Console.Error.WriteLine($"WebDriver BiDi listening on ws://127.0.0.1:{args[firefoxPortIndex]}");
    }
}

Thread.Sleep(Timeout.Infinite);
return 0;
