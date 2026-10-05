// <copyright file="TraceCaptureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using System.IO.Compression;
using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using global::TUnit.Core;

// Most tests here fail on purpose, so that their traces are kept; ExpectedEnding checks them once each ends. The tests
// open their own browsers, so that the class's session can be made to fail first, and they run one at a time.
[ExpectedEnding]
[NotInParallel(nameof(TraceCaptureTests))]
public class TraceCaptureTests : BrowserTest
{
    private const string SessionName = "trace-capture";

    public TraceCaptureTests()
    {
        this.ArtifactsDirectory = Path.Combine(Path.GetTempPath(), $"dramaturge-tunit-{Guid.NewGuid():N}");
        this.TraceOnFailure = true;
    }

    private static FakeSession Session => FakeBrowserSetUp.Server.SessionFor(SessionName);

    [Test]
    public async Task FailedTestKeepsOneTraceOfItsBrowsers()
    {
        string testDirectory = Path.Combine(this.ArtifactsDirectory, "Dramaturge.TUnit.TraceCaptureTests.FailedTestKeepsOneTraceOfItsBrowsers");
        Browser first = await this.NewBrowserAsync();
        await first.NewPageAsync();
        Browser second = await this.NewBrowserAsync();
        await second.NewPageAsync();

        this.ExpectEnding(context =>
        {
            Artifact? trace = context.Output.Artifacts.SingleOrDefault(artifact => artifact.File.Name == "trace.zip");
            if (trace is null)
            {
                return $"Attached {string.Join(", ", context.Output.Artifacts.Select(artifact => artifact.File.Name))}";
            }

            using ZipArchive zip = ZipFile.OpenRead(Path.Combine(testDirectory, "trace.zip"));
            string[] entries = [.. zip.Entries.Select(entry => entry.FullName)];
            string[] contexts = [.. entries.Where(entry => entry.Contains("trace.", StringComparison.Ordinal)).Order()];
            return trace.Description != "A trace of the failed test's browsers" ? $"Described {trace.Description}"
                : !contexts.SequenceEqual(["0-trace.network", "0-trace.trace", "1-trace.network", "1-trace.trace"]) ? $"Merged {string.Join(", ", contexts)}"
                : !entries.Any(entry => entry.StartsWith("src/", StringComparison.Ordinal)) ? "Left out the sources"
                : entries.Distinct().Count() != entries.Length ? "Wrote a file twice"
                : context.Output.GetStandardOutput().Length != 0 ? $"Wrote '{context.Output.GetStandardOutput()}'"
                : null;
        });
        throw new InvalidOperationException("The test failed.");
    }

    [Test]
    public async Task PassedTestsTracesAreDeleted()
    {
        this.TraceOptions = new TraceRecordingOptions();
        Browser browser = await this.NewBrowserAsync();
        await browser.NewPageAsync();

        string root = this.ArtifactsDirectory;
        this.ExpectEnding(context => context.Output.Artifacts.Count != 0 ? "Attached a trace" : Directory.Exists(root) ? "Made a directory" : null);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task TraceThatCannotStartIsWrittenForAFailedTest(bool failed)
    {
        this.ScreenshotOnFailure = false;
        Session.RemoteEnd.FailWith("network.addDataCollector", "unknown error", "No collectors.");
        await this.NewBrowserAsync();

        this.ExpectEnding(context =>
        {
            Session.RemoteEnd.AnswerWith("network.addDataCollector", new JsonObject() { ["collector"] = "collector-restored" });
            string output = context.Output.GetStandardOutput();
            return context.Output.Artifacts.Count != 0 ? "Attached a trace"
                : !failed ? (output.Length != 0 ? $"Wrote '{output}'" : null)
                : !output.StartsWith("Dramaturge could not record a trace of browser 1: ", StringComparison.Ordinal) || !output.Contains("No collectors.", StringComparison.Ordinal) ? $"Wrote '{output}'"
                : null;
        });
        if (failed)
        {
            throw new InvalidOperationException("The test failed.");
        }
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
}
