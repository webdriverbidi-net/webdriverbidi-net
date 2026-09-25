// <copyright file="BrowsersTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Diagnostics;
using System.Reflection;
using WebDriverBiDi.Browsers;

public class BrowsersTests
{
    private static readonly string FakeBrowserPath = Path.Combine(
        typeof(BrowsersTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(attribute => attribute.Key == "FakeBrowserDirectory").Value!,
        "WebDriverBiDi.FakeBrowser.exe");

    // .NET Framework rejects a malformed header name by throwing, where later runtimes return false.
    [Fact]
    public void BuildRejectsMalformedHeaderName()
    {
        RemoteGridOptions options = new() { Headers = { ["Not A Header Name"] = "value" } };

        Assert.Throws<BrowserLauncherConfigurationException>(BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri("http://grid.example/"), options).Build);
    }

    // Without the .NET 5 API, the process tree is killed with taskkill.
    [Fact]
    public async Task KillEndsBrowserAndItsChildProcess()
    {
        string childProcessIdFile = Path.Combine(Path.GetTempPath(), $"webdriverbidi-netfx-{Guid.NewGuid():N}.pid");
        try
        {
            await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox)
                .AtLocation(FakeBrowserPath)
                .WithEnvironmentVariable("WEBDRIVERBIDI_FAKE_BROWSER_CHILD_PID_FILE", childProcessIdFile)
                .Build();
            await launcher.StartAsync(TestContext.Current.CancellationToken);
            await launcher.LaunchBrowserAsync(TestContext.Current.CancellationToken);
            using Process child = Process.GetProcessById(int.Parse(File.ReadAllText(childProcessIdFile)));

            await launcher.KillBrowserAsync(TestContext.Current.CancellationToken);

            Assert.False(launcher.IsRunning);
            Assert.True(child.WaitForExit(10000));
        }
        finally
        {
            File.Delete(childProcessIdFile);
        }
    }
}
