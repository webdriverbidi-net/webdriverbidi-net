// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::Xunit.Sdk;

// The tests here create test objects themselves, which get the class group of this class, as tests of this class
// would, and end them as a failed test ends.
public sealed class CaptureTests(FakeBrowserFixture fixture) : IClassFixture<ClassBrowserGroup>, IDisposable
{
    private const string SessionName = "capture";

    private readonly string artifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-xunit-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.artifactsDirectory))
        {
            Directory.Delete(this.artifactsDirectory, true);
        }
    }

    [Fact]
    public async Task FailedTestCapturesEveryOpenPageOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.Xunit.CaptureTests.FailedTestCapturesEveryOpenPageOfItsBrowsers");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "page-9.png"), "from an earlier run");
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();
        Page closed = await test.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await closed.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Equal(["page-1.png", "page-2.png"], ending.Attachments.Keys.Order());
        Assert.All(ending.Attachments.Values, attachment => Assert.Equal(FakeBrowserServer.Screenshot, attachment.AsByteArray().ByteArray));
        Assert.All(ending.Attachments.Values, attachment => Assert.Equal("image/png", attachment.AsByteArray().MediaType));
        Assert.Equal(["page-1.png", "page-2.png"], Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order());
        Assert.Equal(FakeBrowserServer.Screenshot, File.ReadAllBytes(Path.Combine(testDirectory, "page-1.png")));
        Assert.Empty(ending.Warnings);
    }

    [Theory]
    [InlineData("a/b-c_d")]
    public async Task DirectoryIsNamedForTheTestInPortableCharacters(string value)
    {
        Assert.NotNull(value);
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();

        await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Equal(["Dramaturge.Xunit.CaptureTests.DirectoryIsNamedForTheTestInPortableCharacters_value___a_b-c_d__"], Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName));
    }

    [Theory]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789")]
    public async Task DirectoryNameIsShortenedToAHundredAndTwentyCharacters(string value)
    {
        Assert.NotNull(value);
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();

        await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Equal(["Dramaturge.Xunit.CaptureTests.DirectoryNameIsShortenedToAHundredAndTwentyCharacters_value___0123456789012345678901234567"], Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName));
    }

    [Fact]
    public async Task PassedTestCapturesNothing()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.ForPassed(0));

        Assert.Empty(ending.Attachments);
        Assert.False(Directory.Exists(this.artifactsDirectory));
    }

    [Fact]
    public async Task FailedTestCapturesNothingWhenScreenshotsAreOff()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory, screenshotOnFailure: false);
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Empty(ending.Attachments);
        Assert.False(Directory.Exists(this.artifactsDirectory));
    }

    [Fact]
    public async Task FailedTestWithoutPagesCapturesNothing()
    {
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();
        await test.Page.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Empty(ending.Attachments);
        Assert.False(Directory.Exists(this.artifactsDirectory));
    }

    [Fact]
    public async Task FailedCaptureIsAWarning()
    {
        FakeSession session = fixture.Server.SessionFor(SessionName);
        CapturedTest test = new(fixture, this.artifactsDirectory);
        await test.InitializeAsync();
        session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "unknown error", "The page cannot be captured.");
        try
        {
            Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

            Assert.Empty(ending.Attachments);
            string warning = Assert.Single(ending.Warnings);
            Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(this.artifactsDirectory, "Dramaturge.Xunit.CaptureTests.FailedCaptureIsAWarning", "page-1.png")}: ", warning);
            Assert.Contains("The page cannot be captured.", warning);
        }
        finally
        {
            FakeBrowserServer.AnswerScreenshots(session);
        }
    }

    [Fact]
    public async Task UnwritableDirectoryIsAWarning()
    {
        Directory.CreateDirectory(this.artifactsDirectory);
        string file = Path.Combine(this.artifactsDirectory, "file");
        File.WriteAllText(file, string.Empty);
        CapturedTest test = new(fixture, file);
        await test.InitializeAsync();

        Ending ending = await EndAsync(test, TestResultState.FromException(0, new InvalidOperationException("The test failed.")));

        Assert.Empty(ending.Attachments);
        Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(file, "Dramaturge.Xunit.CaptureTests.UnwritableDirectoryIsAWarning")}: ", Assert.Single(ending.Warnings));
    }

    [Fact]
    public void ArtifactsDirectoryDefaultsToTestResultsBesideTheTests()
    {
        DefaultsTest test = new();

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge"), test.Directory);
        Assert.True(test.ScreenshotsOn);
    }

    [Fact]
    public void DefaultLaunchersAreTheEnvironmentsUnchanged()
    {
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        Assert.Same(builder, new DefaultsTest().Configure(builder));
        Assert.Same(builder, new DefaultsFixture().Configure(builder));
    }

    // Changes to the test context made in an async method are not seen by its caller, so the current test's own
    // context is unchanged once this returns.
    private static async Task<Ending> EndAsync(BrowserTest test, TestResultState state)
    {
        ITestContext current = TestContext.Current;
        TestContext.SetForTest(current.Test!, TestEngineStatus.CleaningUp, current.CancellationToken, state, current.TestOutputHelper);
        await test.DisposeAsync();
        return new Ending(TestContext.Current.Attachments ?? new Dictionary<string, TestAttachment>(), TestContext.Current.Warnings ?? []);
    }

    private sealed record Ending(IReadOnlyDictionary<string, TestAttachment> Attachments, IReadOnlyList<string> Warnings);

    private sealed class CapturedTest : PageTest
    {
        private readonly FakeBrowserFixture fixture;

        public CapturedTest(FakeBrowserFixture fixture, string artifactsDirectory, bool screenshotOnFailure = true)
        {
            this.fixture = fixture;
            this.ArtifactsDirectory = artifactsDirectory;
            this.ScreenshotOnFailure = screenshotOnFailure;
        }

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return this.fixture.ConnectTo(SessionName);
        }
    }

    private sealed class DefaultsTest : BrowserTest
    {
        public string Directory => this.ArtifactsDirectory;

        public bool ScreenshotsOn => this.ScreenshotOnFailure;

        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }

    private sealed class DefaultsFixture : DramaturgeAssemblyFixture
    {
        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }
}
