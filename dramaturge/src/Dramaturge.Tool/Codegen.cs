// <copyright file="Codegen.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

using Dramaturge.Browsers;
using WebDriverBiDi;

/// <summary>
/// The codegen command: records the user's actions in a browser it shows, as C#.
/// </summary>
internal static class Codegen
{
    /// <summary>
    /// Records until the browser's last page closes or the command is cancelled, writing each statement to the output
    /// as it settles, and the whole file, kept current, to a file if asked; without one, the whole file is written
    /// to the output at the end.
    /// </summary>
    /// <param name="settings">The command's settings.</param>
    /// <param name="downloadOptions">Where and how the browser is downloaded, if it must be.</param>
    /// <param name="configureLauncher">Gives the launcher of the browser from the one the command configures.</param>
    /// <param name="output">The writer for the code.</param>
    /// <param name="error">The writer for progress and errors.</param>
    /// <param name="cancellationToken">A token that ends the recording, as Ctrl+C does.</param>
    /// <returns>A task whose result is the exit code: 0 once the recording ends, and 1 if it cannot start.</returns>
    public static async Task<int> RunAsync(CodegenSettings settings, BrowserDownloadOptions downloadOptions, Func<BrowserLauncherBuilder, BrowserLauncherBuilder> configureLauncher, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        DramaturgeOptions options;
        try
        {
            options = settings.TestIdAttribute is null ? new DramaturgeOptions() : new DramaturgeOptions() { TestIdAttribute = settings.TestIdAttribute };
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }

        BrowserLauncherBuilder builder = BrowserLauncher.Configure(settings.Browser).WithHeadlessOption(false).WithDownloadOptions(downloadOptions);
        if (settings.Channel is BrowserReleaseChannel channel)
        {
            builder = builder.WithReleaseChannel(channel);
        }

        BrowserGroup group;
        try
        {
            group = await BrowserGroup.LaunchAsync(configureLauncher(builder), options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is WebDriverBiDiException or OperationCanceledException)
        {
            error.WriteLine($"The browser could not be started: {ex.Message}");
            return 1;
        }

        await using (group.ConfigureAwait(false))
        {
            return await RecordAsync(group.DefaultBrowser, settings, output, error, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<int> RecordAsync(Browser browser, CodegenSettings settings, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        TaskCompletionSource closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable closing = browser.OnPageClosed.AddObserver(_ =>
        {
            if (browser.Pages.Count == 0)
            {
                closed.TrySetResult();
            }
        });
        Page page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        CodeRecording recording = await browser.RecordCodeAsync(new CodeRecordingOptions() { Target = settings.Target }, cancellationToken).ConfigureAwait(false);
        await using (recording.ConfigureAwait(false))
        {
            if (!Save(recording, settings.OutputPath, error))
            {
                return 1;
            }

            recording.OnStatement.AddObserver(e =>
            {
                output.WriteLine(e.Statement);
                Save(recording, settings.OutputPath, error);
            });
            recording.OnLocatorPicked.AddObserver(e => error.WriteLine($"Picked: {e.Code}"));
            if (settings.Url is not null)
            {
                await page.NavigateAsync(settings.Url, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            error.WriteLine("Recording. Close the browser, or press Ctrl+C, to stop.");
            await Task.WhenAny(closed.Task, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            string code = await recording.StopAsync(CancellationToken.None).ConfigureAwait(false);
            if (settings.OutputPath is null)
            {
                output.WriteLine();
                output.Write(code);
            }
            else if (Save(recording, settings.OutputPath, error))
            {
                error.WriteLine($"Wrote {Path.GetFullPath(settings.OutputPath)}");
            }

            return 0;
        }
    }

    // Writes the whole file, if one was asked for, reporting a failure.
    private static bool Save(CodeRecording recording, string? path, TextWriter error)
    {
        if (path is null)
        {
            return true;
        }

        try
        {
            File.WriteAllText(path, recording.Code);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            error.WriteLine($"{path}: {ex.Message}");
            return false;
        }
    }
}
