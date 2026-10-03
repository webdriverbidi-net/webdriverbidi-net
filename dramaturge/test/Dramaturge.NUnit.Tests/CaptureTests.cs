// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::NUnit.Framework.Interfaces;

// The tests here create test objects themselves, which share the group of this fixture, as its tests would, and end
// them as a failed test ends.
public class CaptureTests
{
    private const string SessionName = "capture";

    private string artifactsDirectory = string.Empty;

    [OneTimeTearDown]
    public Task CloseGroupAsync() => BrowserTest.CloseFixtureGroupAsync();

    [SetUp]
    public void CreateArtifactsDirectoryName()
    {
        this.artifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-nunit-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void DeleteArtifactsDirectory()
    {
        if (Directory.Exists(this.artifactsDirectory))
        {
            Directory.Delete(this.artifactsDirectory, true);
        }
    }

    [Test]
    public async Task FailedTestCapturesEveryOpenPageOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.NUnit.CaptureTests.FailedTestCapturesEveryOpenPageOfItsBrowsers");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "page-9.png"), "from an earlier run");
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();
        Page closed = await test.Browser.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(ending.Attachments, Is.EqualTo(new[] { Path.Combine(testDirectory, "page-1.png"), Path.Combine(testDirectory, "page-2.png") }));
        Assert.That(Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order(), Is.EqualTo(new[] { "page-1.png", "page-2.png" }));
        Assert.That(File.ReadAllBytes(Path.Combine(testDirectory, "page-1.png")), Is.EqualTo(FakeBrowserServer.Screenshot));
        Assert.That(ending.Output, Is.Empty);
    }

    [TestCase("a/b-c_d")]
    public async Task DirectoryIsNamedForTheTestInPortableCharacters(string value)
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();

        await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName), Is.EqualTo(new[] { "Dramaturge.NUnit.CaptureTests.DirectoryIsNamedForTheTestInPortableCharacters__a_b-c_d__" }));
    }

    [TestCase("012345678901234567890123456789012345678901234567890123456789")]
    public async Task DirectoryNameIsShortenedToAHundredAndTwentyCharacters(string value)
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();

        await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName), Is.EqualTo(new[] { "Dramaturge.NUnit.CaptureTests.DirectoryNameIsShortenedToAHundredAndTwentyCharacters__01234567890123456789012345678901234" }));
    }

    [Test]
    public async Task PassedTestCapturesNothing()
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Success);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(Directory.Exists(this.artifactsDirectory), Is.False);
    }

    [Test]
    public async Task FailedTestCapturesNothingWhenScreenshotsAreOff()
    {
        CapturedTest test = new(this.artifactsDirectory, screenshotOnFailure: false);
        await test.SetUpBrowsersAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(Directory.Exists(this.artifactsDirectory), Is.False);
    }

    [Test]
    public async Task FailedTestWithoutPagesCapturesNothing()
    {
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();
        await test.Page.CloseAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(Directory.Exists(this.artifactsDirectory), Is.False);
    }

    [Test]
    public async Task FailedCaptureIsWrittenToTheOutput()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(SessionName);
        CapturedTest test = new(this.artifactsDirectory);
        await test.SetUpBrowsersAsync();
        session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "unknown error", "The page cannot be captured.");
        try
        {
            TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

            Assert.That(ending.Attachments, Is.Empty);
            Assert.That(ending.Output, Does.StartWith($"Dramaturge could not save a screenshot to {Path.Combine(this.artifactsDirectory, "Dramaturge.NUnit.CaptureTests.FailedCaptureIsWrittenToTheOutput", "page-1.png")}: "));
            Assert.That(ending.Output, Does.Contain("The page cannot be captured."));
        }
        finally
        {
            FakeBrowserServer.AnswerScreenshots(session);
        }
    }

    [Test]
    public async Task UnwritableDirectoryIsWrittenToTheOutput()
    {
        Directory.CreateDirectory(this.artifactsDirectory);
        string file = Path.Combine(this.artifactsDirectory, "file");
        File.WriteAllText(file, string.Empty);
        CapturedTest test = new(file);
        await test.SetUpBrowsersAsync();

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Failure);

        Assert.That(ending.Attachments, Is.Empty);
        Assert.That(ending.Output, Does.StartWith($"Dramaturge could not save a screenshot to {Path.Combine(file, "Dramaturge.NUnit.CaptureTests.UnwritableDirectoryIsWrittenToTheOutput")}: "));
    }

    [Test]
    public void DefaultsAreTestResultsBesideTheTestsAndTheEnvironmentsLauncher()
    {
        DefaultsTest test = new();
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        Assert.That(test.Directory, Is.EqualTo(Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge")));
        Assert.That(test.ScreenshotsOn, Is.True);
        Assert.That(test.Configure(builder), Is.SameAs(builder));
        Assert.That(new DefaultsFixture().Configure(builder), Is.SameAs(builder));
    }

    private sealed class CapturedTest : PageTest
    {
        public CapturedTest(string artifactsDirectory, bool screenshotOnFailure = true)
        {
            this.ArtifactsDirectory = artifactsDirectory;
            this.ScreenshotOnFailure = screenshotOnFailure;
        }

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return FakeBrowserSetUp.ConnectTo(SessionName);
        }
    }

    private sealed class DefaultsTest : BrowserTest
    {
        public string Directory => this.ArtifactsDirectory;

        public bool ScreenshotsOn => this.ScreenshotOnFailure;

        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }

    private sealed class DefaultsFixture : DramaturgeSetUpFixture
    {
        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }
}
