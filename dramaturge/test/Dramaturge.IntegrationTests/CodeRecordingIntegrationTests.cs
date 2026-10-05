// <copyright file="CodeRecordingIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class CodeRecordingIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task UsersActionsAreRecordedAsStatements(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("recording.html"), cancellationToken: TestContext.Current.CancellationToken);
        CodeRecording recording = await group.DefaultBrowser.RecordCodeAsync(cancellationToken: TestContext.Current.CancellationToken);
        List<string> statements = [];
        TaskCompletionSource threeSettled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        recording.OnStatement.AddObserver(e =>
        {
            statements.Add(e.Statement);
            if (statements.Count == 3)
            {
                threeSettled.TrySetResult();
            }
        });

        await page.Locate(new CssLocator("#go")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#name")).PressSequentiallyAsync("Ada", cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#agree")).CheckAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#done")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await threeSettled.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "await page.Locate(new CssLocator(\"#go\")).ClickAsync();",
                "await page.Locate(new CssLocator(\"#name\")).FillAsync(\"Ada\");",
                "await page.Locate(new CssLocator(\"#agree\")).CheckAsync();",
                "await page.Locate(new CssLocator(\"#done\")).ClickAsync();",
            ],
            statements);
    }
}
