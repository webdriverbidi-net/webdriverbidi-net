// <copyright file="CaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::TUnit.Core;

// Most tests here fail on purpose, so that their pages are captured; ExpectedEnding checks the capture once each ends.
// They run one at a time, because one makes the class's session unable to capture pages.
[ExpectedEnding]
[NotInParallel(nameof(CaptureTests))]
public class CaptureTests : PageTest
{
    private const string SessionName = "capture";

    public CaptureTests()
    {
        this.ArtifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-tunit-{Guid.NewGuid():N}");
    }

    [Test]
    public async Task FailedTestCapturesEveryOpenPageOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.ArtifactsDirectory, "Dramaturge.TUnit.CaptureTests.FailedTestCapturesEveryOpenPageOfItsBrowsers");
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(Path.Combine(testDirectory, "page-9.png"), "from an earlier run");
        Page closed = await this.Browser.NewPageAsync();
        await closed.CloseAsync();
        Browser second = await this.NewBrowserAsync();
        await second.NewPageAsync();

        this.ExpectEnding(context =>
        {
            string[] artifacts = [.. context.Output.Artifacts.Select(artifact => artifact.File.FullName)];
            string[] expected = [Path.Combine(testDirectory, "page-1.png"), Path.Combine(testDirectory, "page-2.png")];
            string[] files = [.. Directory.GetFiles(testDirectory).Select(Path.GetFileName).Order()!];
            return !artifacts.SequenceEqual(expected) ? $"Attached {string.Join(", ", artifacts)}"
                : !files.SequenceEqual(["page-1.png", "page-2.png"]) ? $"Saved {string.Join(", ", files)}"
                : !File.ReadAllBytes(expected[0]).SequenceEqual(FakeBrowserServer.Screenshot) ? "Saved another image"
                : null;
        });
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    [Arguments("a/b-c_d")]
    public void DirectoryIsNamedForTheTestInPortableCharacters(string value)
    {
        this.ExpectDirectory("Dramaturge.TUnit.CaptureTests.DirectoryIsNamedForTheTestInPortableCharacters_a_b-c_d_");
        throw new InvalidOperationException($"The test failed with {value}.");
    }

    [Test]
    [Arguments("012345678901234567890123456789012345678901234567890123456789")]
    public void DirectoryNameIsShortenedToAHundredAndTwentyCharacters(string value)
    {
        this.ExpectDirectory("Dramaturge.TUnit.CaptureTests.DirectoryNameIsShortenedToAHundredAndTwentyCharacters_012345678901234567890123456789012345");
        throw new InvalidOperationException($"The test failed with {value}.");
    }

    [Test]
    public void PassedTestCapturesNothing()
    {
        this.ExpectNothingCaptured();
    }

    [Test]
    public void FailedTestCapturesNothingWhenScreenshotsAreOff()
    {
        this.ScreenshotOnFailure = false;
        this.ExpectNothingCaptured();
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    public async Task FailedTestWithoutPagesCapturesNothing()
    {
        await this.Page.CloseAsync();
        this.ExpectNothingCaptured();
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    public void FailedCaptureIsWrittenToTheOutput()
    {
        FakeSession session = FakeBrowserSetUp.Server.SessionFor(SessionName);
        session.RemoteEnd.FailWith("browsingContext.captureScreenshot", "unknown error", "The page cannot be captured.");
        string expectedStart = $"Dramaturge could not save a screenshot to {Path.Combine(this.ArtifactsDirectory, "Dramaturge.TUnit.CaptureTests.FailedCaptureIsWrittenToTheOutput", "page-1.png")}: ";
        this.ExpectEnding(context =>
        {
            FakeBrowserServer.AnswerScreenshots(session);
            string output = context.Output.GetStandardOutput();
            return context.Output.Artifacts.Count != 0 ? "Attached a screenshot"
                : !output.StartsWith(expectedStart, StringComparison.Ordinal) || !output.Contains("The page cannot be captured.", StringComparison.Ordinal) ? $"Wrote '{output}'"
                : null;
        });
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    public void UnwritableDirectoryIsWrittenToTheOutput()
    {
        string root = this.ArtifactsDirectory;
        Directory.CreateDirectory(root);
        string file = Path.Combine(root, "file");
        File.WriteAllText(file, string.Empty);
        this.ArtifactsDirectory = file;
        string expectedStart = $"Dramaturge could not save a screenshot to {Path.Combine(file, "Dramaturge.TUnit.CaptureTests.UnwritableDirectoryIsWrittenToTheOutput")}: ";
        this.ExpectEnding(context =>
        {
            Directory.Delete(root, true);
            string output = context.Output.GetStandardOutput();
            return context.Output.Artifacts.Count != 0 ? "Attached a screenshot"
                : !output.StartsWith(expectedStart, StringComparison.Ordinal) ? $"Wrote '{output}'"
                : null;
        });
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    public async Task DefaultsAreTestResultsBesideTheTestsAndTheEnvironmentsLauncher()
    {
        DefaultsTest test = new();
        BrowserLauncherBuilder builder = BrowserLauncher.Configure(BrowserKind.Firefox);

        await Assert.That(test.Directory).IsEqualTo(Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge"));
        await Assert.That(test.ScreenshotsOn).IsTrue();
        await Assert.That(test.Configure(builder)).IsSameReferenceAs(builder);
        await Assert.That(new DefaultsFixture().Configure(builder)).IsSameReferenceAs(builder);
    }

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return FakeBrowserSetUp.ConnectTo(SessionName);
    }

    // Deletes the test's directory once checked.
    private void ExpectEnding(Func<TestContext, string?> check)
    {
        string directory = this.ArtifactsDirectory;
        ExpectedEndingAttribute.Expect(context =>
        {
            try
            {
                return check(context);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        });
    }

    private void ExpectDirectory(string name)
    {
        string root = this.ArtifactsDirectory;
        this.ExpectEnding(_ =>
        {
            string[] directories = [.. Directory.GetDirectories(root).Select(Path.GetFileName)!];
            return directories.SequenceEqual([name]) ? null : $"Made {string.Join(", ", directories)}";
        });
    }

    private void ExpectNothingCaptured()
    {
        string root = this.ArtifactsDirectory;
        this.ExpectEnding(context => context.Output.Artifacts.Count != 0 ? "Attached a screenshot" : Directory.Exists(root) ? "Made a directory" : null);
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
