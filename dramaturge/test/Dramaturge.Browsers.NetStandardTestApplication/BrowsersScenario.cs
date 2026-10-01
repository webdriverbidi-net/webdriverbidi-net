// <copyright file="BrowsersScenario.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Reflection;
using System.Runtime.Versioning;
using Dramaturge.Browsers;

/// <summary>
/// Exercises the netstandard2.0-only code of Dramaturge.Browsers, which the unit tests, running the
/// net10.0 build, never reach: the process tree kill, the symbolic link check for confined executables,
/// the cache layout marker, the timeout on reading a browser's version, and adding a request header.
/// </summary>
internal static class BrowsersScenario
{
    // The version the fake browser reports.
    private const string FakeBrowserVersion = "130.0.2849.80";

    public static async Task RunAsync(string fakeBrowserPath)
    {
        string? frameworkName = typeof(BrowserLauncher).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        if (frameworkName is null || !frameworkName.StartsWith(".NETStandard,Version=v2.0", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected to load the netstandard2.0 build of Dramaturge.Browsers, but loaded '{frameworkName}'.");
        }

        // Launching as Firefox checks whether the executable is confined; killing it kills its process tree.
        await using (BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox).AtLocation(fakeBrowserPath).Build())
        {
            await launcher.StartAsync();
            await launcher.LaunchBrowserAsync();
            await launcher.KillBrowserAsync();
            if (launcher.IsRunning)
            {
                throw new InvalidOperationException("The killed browser is still running.");
            }
        }

        Console.WriteLine("Browsers: a browser was launched and killed through the netstandard2.0 build.");

        // Locking a new cache marks its layout. Outside Windows, the version is read from the browser's
        // "--version" output under a timeout, and the failure names it.
        string cacheDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-compatibility-{Guid.NewGuid():N}");
        try
        {
            BrowserDownloadOptions options = new() { CacheDirectory = cacheDirectory, SkipDownload = true };
            try
            {
                await DriverLocator.FindDriverAsync(BrowserKind.Chrome, locationBehavior: FileLocationBehavior.UseCustomLocation, customPath: fakeBrowserPath, downloadOptions: options);
                throw new InvalidOperationException("Expected the driver lookup to fail with downloads disabled.");
            }
            catch (BrowserDownloadException ex) when (ex.Message.Contains(FakeBrowserVersion))
            {
            }

            if (!File.Exists(Path.Combine(cacheDirectory, ".layout-version")))
            {
                throw new InvalidOperationException("The new cache was not marked with its layout version.");
            }
        }
        finally
        {
            if (Directory.Exists(cacheDirectory))
            {
                Directory.Delete(cacheDirectory, true);
            }
        }

        Console.WriteLine("Browsers: the installed browser's version was read and the new cache was marked.");

        RemoteGridOptions gridOptions = new() { Headers = { ["X-Compatibility"] = "netstandard2.0" } };
        await using (BrowserLauncher remoteLauncher = BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri("http://localhost:4444/"), gridOptions).Build())
        {
        }

        Console.WriteLine("Browsers: a remote grid header was added.");
    }
}
