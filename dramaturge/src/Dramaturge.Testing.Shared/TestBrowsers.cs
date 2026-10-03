// <copyright file="TestBrowsers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Testing;

using System.Text;

/// <summary>
/// The browsers one test opens, captured if the test fails and closed when it ends.
/// </summary>
internal sealed class TestBrowsers
{
    private const int MaximumDirectoryNameLength = 120;

    private readonly object lockObject = new();
    private readonly List<Browser> browsers = [];

    /// <summary>
    /// Gets the default directory under which a failed test's screenshots are written.
    /// </summary>
    public static string DefaultArtifactsDirectory => Path.Combine(AppContext.BaseDirectory, "TestResults", "Dramaturge");

    /// <summary>
    /// Creates a browser to be closed when the test ends.
    /// </summary>
    /// <param name="group">The group in which to create it.</param>
    /// <param name="options">The browser's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The browser.</returns>
    public async Task<Browser> CreateAsync(BrowserGroup group, BrowserOptions? options, CancellationToken cancellationToken)
    {
        Browser browser = await group.CreateBrowserAsync(options, cancellationToken).ConfigureAwait(false);
        lock (this.lockObject)
        {
            this.browsers.Add(browser);
        }

        return browser;
    }

    /// <summary>
    /// Captures every open page of the test's browsers to page-1.png, page-2.png, and so on, in a directory named for
    /// the test, replacing any earlier run's.
    /// </summary>
    /// <param name="artifactsDirectory">The directory under which the test's directory is made.</param>
    /// <param name="testName">The test's name.</param>
    /// <returns>Each page's capture, or the failure to capture it.</returns>
    public async Task<IReadOnlyList<PageCapture>> CaptureAsync(string artifactsDirectory, string testName)
    {
        List<Page> pages;
        lock (this.lockObject)
        {
            pages = [.. this.browsers.SelectMany(browser => browser.Pages).Where(page => !page.IsClosed)];
        }

        List<PageCapture> captures = [];
        if (pages.Count == 0)
        {
            return captures;
        }

        string directory = Path.Combine(artifactsDirectory, ToDirectoryName(testName));
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            Directory.CreateDirectory(directory);
        }
        catch (Exception exception)
        {
            return [new PageCapture(directory, null, exception)];
        }

        for (int index = 0; index < pages.Count; index++)
        {
            string path = Path.Combine(directory, $"page-{index + 1}.png");
            try
            {
                byte[] screenshot = await pages[index].ScreenshotAsync().ConfigureAwait(false);
                File.WriteAllBytes(path, screenshot);
                captures.Add(new PageCapture(path, screenshot, null));
            }
            catch (Exception exception)
            {
                captures.Add(new PageCapture(path, null, exception));
            }
        }

        return captures;
    }

    /// <summary>
    /// Closes the test's browsers, each even if closing another fails.
    /// </summary>
    /// <returns>The failures to close a browser.</returns>
    public async Task<IReadOnlyList<Exception>> CloseAsync()
    {
        List<Browser> closing;
        lock (this.lockObject)
        {
            closing = [.. this.browsers];
            this.browsers.Clear();
        }

        List<Exception> failures = [];
        foreach (Browser browser in closing)
        {
            try
            {
                await browser.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }

        return failures;
    }

    // Letters, digits, '.', '-', and '_' are kept, so that the name is valid on every platform.
    private static string ToDirectoryName(string testName)
    {
        StringBuilder name = new(testName.Length);
        foreach (char character in testName)
        {
            name.Append(char.IsLetterOrDigit(character) || character is '.' or '-' or '_' ? character : '_');
        }

        return name.Length > MaximumDirectoryNameLength ? name.ToString(0, MaximumDirectoryNameLength) : name.ToString();
    }
}
