// <copyright file="AotCompilationEnvironmentTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration;

using System.Diagnostics;
using System.Runtime.InteropServices;
using PinchHitter;

[TestFixture]
public class AotCompilationEnvironmentTests
{
    private static readonly string SmokeTestProjectDir = Path.GetFullPath(
        Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "WebDriverBiDi.AotSmokeTest"));

    private static string publishDir = string.Empty;
    private static string executablePath = string.Empty;

    [OneTimeSetUp]
    public async Task PublishAotBinary()
    {
        publishDir = Path.Combine(SmokeTestProjectDir, "bin", "AotTestPublish");

        int publishExit = await RunProcessAsync(
            "dotnet",
            $"publish \"{SmokeTestProjectDir}\" -c Release -o \"{publishDir}\"",
            workingDirectory: SmokeTestProjectDir,
            timeoutSeconds: 300);

        Assert.That(publishExit, Is.EqualTo(0), "dotnet publish of AotSmokeTest failed.");

        string executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "WebDriverBiDi.AotSmokeTest.exe"
            : "WebDriverBiDi.AotSmokeTest";
        executablePath = Path.Combine(publishDir, executableName);

        Assert.That(File.Exists(executablePath), Is.True, $"Published AOT executable not found at: {executablePath}");
    }

    [TestCase(Browser.Firefox)]
    [TestCase(Browser.Chrome)]
    public async Task TestCanExecuteInAotCompilationEnvironment(Browser browser)
    {
        // Ensure the browser is available before running each test
        // In CI: skips test if browser executable not configured
        // Locally: allows test to run with system-installed browser
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using Server server = new();
        server.RegisterHandler("/test", new WebResourceRequestHandler(WebContent.AsHtmlDocument("<h1>Welcome to the WebDriverBiDi.NET project</h1><p>You can browse using localhost</p>", "<title>WebDriverBiDi.NET Testing</title>")));
        await server.StartAsync();
        string browserArg = BrowserTestHelper.ToBrowserString(browser);
        string testUrl = $"{browserArg} http://localhost:{server.Port}/test";

        int runExit = await RunProcessAsync(
            executablePath,
            testUrl,
            workingDirectory: publishDir,
            timeoutSeconds: 120);

        Assert.That(runExit, Is.Zero, $"AotSmokeTest ({testUrl}) exited with non-zero exit code.");
    }

    private static async Task<int> RunProcessAsync(string fileName, string arguments, string workingDirectory, int timeoutSeconds)
    {
        using Process process = new();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        // Read stdout/stderr concurrently to avoid deadlocks.
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        bool exited = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));

        string stdout = await stdoutTask;
        string stderr = await stderrTask;

        TestContext.Out.WriteLine($"[{Path.GetFileName(fileName)}] stdout:\n{stdout}");
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            TestContext.Error.WriteLine($"[{Path.GetFileName(fileName)}] stderr:\n{stderr}");
        }

        if (!exited)
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"Process '{fileName}' timed out after {timeoutSeconds}s.");
        }

        return process.ExitCode;
    }
}
