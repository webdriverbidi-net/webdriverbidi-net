// <copyright file="AotCompilationEnvironmentTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration.Tests;

using PinchHitter;

public class AotCompilationEnvironmentTests : IClassFixture<AotCompilationEnvironmentFixture>
{
    private readonly AotCompilationEnvironmentFixture fixture;

    public AotCompilationEnvironmentTests(AotCompilationEnvironmentFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanExecuteInAotCompilationEnvironment(TestBrowser browser)
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

        int runExit = await AotCompilationEnvironmentFixture.RunProcessAsync(
            this.fixture.ExecutablePath,
            testUrl,
            workingDirectory: this.fixture.PublishDir,
            timeoutSeconds: 120);

        Assert.Equal(0, runExit);
    }
}
