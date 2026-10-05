// <copyright file="VideoIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

public sealed class VideoIntegrationTests : IDisposable
{
    // Every WebM file begins with the EBML header's ID.
    private static readonly byte[] WebMSignature = [0x1A, 0x45, 0xDF, 0xA3];

    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-video-it-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.directory))
        {
            Directory.Delete(this.directory, true);
        }
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task PageVideoIsWrittenToThePathAskedFor(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Chrome, "Chrome does not implement browsingContext.startScreencast.");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("animation.html"), cancellationToken: TestContext.Current.CancellationToken);
        string path = Path.Combine(this.directory, "videos", "animation.webm");

        string written;
        await using (VideoRecording recording = await page.RecordVideoAsync(path, new VideoRecordingOptions() { Width = 320, Height = 240, FrameRate = 10 }, TestContext.Current.CancellationToken))
        {
            // The animation runs while the video records it.
            await page.WaitForFunctionAsync<bool>("() => new Promise((resolve) => setTimeout(() => resolve(true), 1000))", cancellationToken: TestContext.Current.CancellationToken);
            written = await recording.StopAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(Path.GetFullPath(path), written);
        byte[] video = File.ReadAllBytes(written);
        Assert.True(video.Length > 1000);
        Assert.Equal(WebMSignature, video.Take(4));
        Assert.Equal(["animation.webm"], Directory.GetFiles(Path.GetDirectoryName(written)!).Select(Path.GetFileName));
    }
}
