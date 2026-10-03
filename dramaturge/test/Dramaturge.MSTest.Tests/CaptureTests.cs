// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

// The tests here create test objects themselves, which share the group of this class, as its tests would, and end
// them as a failed test ends.
[TestClass]
public class CaptureTests
{
    private const string SessionName = "capture";

    private readonly string artifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-mstest-{Guid.NewGuid():N}");

    public TestContext TestContext { get; set; } = null!;

    [ClassCleanup]
    public static Task CloseGroupAsync(TestContext context) => BrowserTest.CloseClassGroupAsync(context);

    [TestCleanup]
    public void DeleteArtifactsDirectory()
    {
        if (Directory.Exists(this.artifactsDirectory))
        {
            Directory.Delete(this.artifactsDirectory, true);
        }
    }

    [TestMethod]
    public async Task FailedTestCapturesEveryOpenPageOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.artifactsDirectory, "Dramaturge.MSTest.CaptureTests.FailedTestCapturesEveryOpenPageOfItsBrowsers");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "page-9.png"), "from an earlier run");
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);
        Page closed = await test.Browser.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await test.NewBrowserAsync();
        await second.NewPageAsync();

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        CollectionAssert.AreEqual(new[] { Path.Combine(testDirectory, "page-1.png"), Path.Combine(testDirectory, "page-2.png") }, ending.ResultFiles);
        CollectionAssert.AreEqual(new[] { "page-1.png", "page-2.png" }, Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order().ToList());
        CollectionAssert.AreEqual(FakeBrowserServer.Screenshot, File.ReadAllBytes(Path.Combine(testDirectory, "page-1.png")));
        Assert.AreEqual(string.Empty, ending.Output);
    }

    [TestMethod]
    public async Task DirectoryNameIsShortenedToAHundredAndTwentyCharactersWhenTheNamesOfTheClassAndTheMethodTogetherAreLongerThanThat()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);

        await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        CollectionAssert.AreEqual(new[] { "Dramaturge.MSTest.CaptureTests.DirectoryNameIsShortenedToAHundredAndTwentyCharactersWhenTheNamesOfTheClassAndTheMethodTo" }, Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName).ToList());
    }

    [TestMethod]
    public async Task DirectoryIsNamedForTheTestInPortableCharacters()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);

        await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed, "Named (a/b-c_d)");

        CollectionAssert.AreEqual(new[] { "Dramaturge.MSTest.CaptureTests.Named__a_b-c_d_" }, Directory.GetDirectories(this.artifactsDirectory).Select(Path.GetFileName).ToList());
    }

    [TestMethod]
    public async Task PassedTestCapturesNothing()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Passed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.IsFalse(Directory.Exists(this.artifactsDirectory));
    }

    [TestMethod]
    public async Task FailedTestCapturesNothingWhenScreenshotsAreOff()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory, screenshotOnFailure: false);

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.IsFalse(Directory.Exists(this.artifactsDirectory));
    }

    [TestMethod]
    public async Task FailedTestWithoutPagesCapturesNothing()
    {
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);
        await test.Page.CloseAsync();

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.IsFalse(Directory.Exists(this.artifactsDirectory));
    }

    [TestMethod]
    public async Task FailedCaptureIsWrittenToTheOutput()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(SessionName);
        CapturedTest test = await this.StartAsync(this.artifactsDirectory);
        session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "unknown error", "The page cannot be captured.");
        try
        {
            EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

            Assert.IsEmpty(ending.ResultFiles);
            Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(this.artifactsDirectory, "Dramaturge.MSTest.CaptureTests.FailedCaptureIsWrittenToTheOutput", "page-1.png")}: ", ending.Output);
            Assert.Contains("The page cannot be captured.", ending.Output);
        }
        finally
        {
            FakeBrowserServer.AnswerScreenshots(session);
        }
    }

    [TestMethod]
    public async Task UnwritableDirectoryIsWrittenToTheOutput()
    {
        Directory.CreateDirectory(this.artifactsDirectory);
        string file = Path.Combine(this.artifactsDirectory, "file");
        File.WriteAllText(file, string.Empty);
        CapturedTest test = await this.StartAsync(file);

        EndingContext ending = await EndingContext.EndAsync(test, this.TestContext, UnitTestOutcome.Failed);

        Assert.IsEmpty(ending.ResultFiles);
        Assert.StartsWith($"Dramaturge could not save a screenshot to {Path.Combine(file, "Dramaturge.MSTest.CaptureTests.UnwritableDirectoryIsWrittenToTheOutput")}: ", ending.Output);
    }

    [TestMethod]
    public void DefaultsAreTestResultsBesideTheTestsAndTheEnvironmentsLauncher()
    {
        DefaultsTest test = new();
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge"), test.Directory);
        Assert.IsTrue(test.ScreenshotsOn);
        Assert.AreSame(builder, test.Configure(builder));
        Assert.AreSame(builder, new DefaultsFixture().Configure(builder));
    }

    private async Task<CapturedTest> StartAsync(string artifactsDirectory, bool screenshotOnFailure = true)
    {
        CapturedTest test = new(artifactsDirectory, screenshotOnFailure) { TestContext = this.TestContext };
        await test.SetUpBrowsersAsync();
        await test.OpenPageAsync();
        return test;
    }

    private sealed class CapturedTest : PageTest
    {
        public CapturedTest(string artifactsDirectory, bool screenshotOnFailure)
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

    private sealed class DefaultsFixture : DramaturgeAssemblyFixture
    {
        public BrowserLauncherBuilder Configure(BrowserLauncherBuilder builder) => this.ConfigureLauncher(builder);
    }
}
