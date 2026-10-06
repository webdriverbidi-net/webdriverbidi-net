// <copyright file="DramaturgeTool.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

using System.CommandLine;
using System.CommandLine.Help;
using System.Globalization;
using Dramaturge.Browsers;
using WebDriverBiDi;

/// <summary>
/// The dramaturge tool: installs, lists, and removes the browsers and drivers that Dramaturge.Browsers caches, and
/// records the actions a user takes in a browser as C#.
/// </summary>
public static class DramaturgeTool
{
    private const string CommandName = "dramaturge";

    /// <summary>
    /// Runs the tool.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="output">The writer for results.</param>
    /// <param name="error">The writer for progress and errors.</param>
    /// <param name="createDownloadOptions">
    /// Creates the download options from the --path value, if given, and a progress reporter, or <see langword="null"/>
    /// for the options the environment variables configure.
    /// </param>
    /// <param name="configureLauncher">
    /// Gives the launcher of the browser codegen records in, from the one the command configures, or
    /// <see langword="null"/> for that one.
    /// </param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task whose result is the exit code: 0 on success, and 1 if anything failed.</returns>
    public static Task<int> RunAsync(string[] args, TextWriter output, TextWriter error, Func<string?, IProgress<BrowserDownloadProgress>, BrowserDownloadOptions>? createDownloadOptions = null, Func<BrowserLauncherBuilder, BrowserLauncherBuilder>? configureLauncher = null, CancellationToken cancellationToken = default)
    {
        createDownloadOptions ??= CreateDefaultDownloadOptions;
        configureLauncher ??= builder => builder;
        ProgressWriter progress = new(error);
        Option<string?> pathOption = new("--path")
        {
            Description = "The cache directory. Defaults to DRAMATURGE_BROWSERS_PATH, or the per-user cache directory.",
            Recursive = true,
        };

        Argument<string[]> installTargets = new("targets")
        {
            Arity = ArgumentArity.OneOrMore,
            Description = "The browsers and drivers to install, as name[@channel|@milestone|@version], such as chrome@beta, chromedriver@131, or firefox@134.0. Names: chrome, chrome-headless-shell, firefox, chromedriver, geckodriver, msedgedriver.",
        };
        Option<bool> installDryRun = new("--dry-run") { Description = "Show the version and URL each target resolves to, and whether it is cached, without downloading." };
        Command install = new("install", "Download browsers and drivers into the cache.") { installTargets, installDryRun };
        install.SetAction((parseResult, token) => InstallAsync(
            parseResult.GetValue(installTargets)!,
            parseResult.GetValue(installDryRun),
            createDownloadOptions(parseResult.GetValue(pathOption), progress),
            output,
            error,
            token));

        Command list = new("list", "List the browsers and drivers in the cache.");
        list.SetAction(parseResult => List(createDownloadOptions(parseResult.GetValue(pathOption), progress), output, error));

        Argument<string[]> clearTargets = new("targets")
        {
            Arity = ArgumentArity.ZeroOrMore,
            Description = "The installations to remove, as name[@channel|@milestone|@version], such as chrome@canary or chromedriver@131. With none, everything is removed.",
        };
        Option<bool> clearDryRun = new("--dry-run") { Description = "Show what would be removed, without removing it." };
        Command clear = new("clear", "Remove browsers and drivers from the cache.") { clearTargets, clearDryRun };
        clear.SetAction((parseResult, token) => ClearAsync(
            parseResult.GetValue(clearTargets) ?? [],
            parseResult.GetValue(clearDryRun),
            createDownloadOptions(parseResult.GetValue(pathOption), progress),
            output,
            error,
            token));

        Argument<string?> codegenUrl = new("url") { Arity = ArgumentArity.ZeroOrOne, Description = "The address to open first." };
        Option<CodeTarget> codegenTarget = new("--target") { Description = "The kind of file to write.", DefaultValueFactory = _ => CodeTarget.Program };
        Option<BrowserKind> codegenBrowser = new("--browser") { Description = "The browser to record in.", DefaultValueFactory = _ => BrowserKind.Chrome };
        Option<string?> codegenChannel = new("--channel") { Description = "The browser's release channel, as install names it: stable (the default), beta, dev, and canary for Chrome and Edge, or nightly and esr for Firefox." };
        Option<string?> codegenTestId = new("--test-id-attribute") { Description = "The attribute test IDs are read from. Defaults to data-testid." };
        Option<string?> codegenOutput = new("--output", "-o") { Description = "A file to keep the whole code in as it is recorded." };
        Command codegen = new("codegen", "Record the actions you take in a browser as C#, until you close the browser or press Ctrl+C.") { codegenUrl, codegenTarget, codegenBrowser, codegenChannel, codegenTestId, codegenOutput };
        codegen.SetAction((parseResult, token) => Codegen.RunAsync(
            new CodegenSettings(
                parseResult.GetValue(codegenUrl),
                parseResult.GetValue(codegenTarget),
                parseResult.GetValue(codegenBrowser),
                parseResult.GetValue(codegenChannel),
                parseResult.GetValue(codegenTestId),
                parseResult.GetValue(codegenOutput)),
            createDownloadOptions(parseResult.GetValue(pathOption), progress),
            configureLauncher,
            output,
            error,
            token));

        // A RootCommand would name itself for the entry assembly, Dramaturge.Tool, rather than the command.
        Command root = new(CommandName, "Installs, lists, and removes the browsers and drivers that Dramaturge.Browsers caches, and records browser actions as C#.") { install, list, clear, codegen };
        root.Options.Add(pathOption);
        root.Options.Add(new HelpOption());
        root.Options.Add(new VersionOption());
        return root.Parse(args).InvokeAsync(new InvocationConfiguration() { Output = output, Error = error }, cancellationToken);
    }

    private static BrowserDownloadOptions CreateDefaultDownloadOptions(string? path, IProgress<BrowserDownloadProgress> progress)
    {
        return path is null ? new BrowserDownloadOptions() { Progress = progress } : new BrowserDownloadOptions() { CacheDirectory = path, Progress = progress };
    }

    private static async Task<int> InstallAsync(string[] targetTexts, bool dryRun, BrowserDownloadOptions options, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        if (ParseTargets(targetTexts, Installer.ValidateForInstall, error) is not List<Target> targets)
        {
            return 1;
        }

        int exitCode = 0;
        foreach (Target target in targets)
        {
            try
            {
                if (dryRun)
                {
                    ResolvedDownload resolved = await Installer.ResolveAsync(target, options, cancellationToken).ConfigureAwait(false);
                    output.WriteLine($"{target.Text}: {resolved} ({(resolved.IsCached ? "cached" : "to download")}) {resolved.Url}");
                }
                else
                {
                    string path = await Installer.InstallAsync(target, options, cancellationToken).ConfigureAwait(false);
                    output.WriteLine($"{target.Text}: {path}");
                }
            }
            catch (Exception ex) when (ex is WebDriverBiDiException || ex is ArgumentException || ex is NotSupportedException)
            {
                error.WriteLine($"{target.Text}: {ex.Message}");
                exitCode = 1;
            }
        }

        return exitCode;
    }

    private static int List(BrowserDownloadOptions options, TextWriter output, TextWriter error)
    {
        IReadOnlyList<CachedInstallation> installations;
        try
        {
            installations = BrowserCache.List(options);
        }
        catch (BrowserDownloadException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }

        if (installations.Count == 0)
        {
            output.WriteLine($"No browsers or drivers are cached in {options.CacheDirectory}.");
            return 0;
        }

        foreach (CachedInstallation installation in installations)
        {
            string size = string.Format(CultureInfo.InvariantCulture, "{0:0.0} MB", installation.Size / 1048576.0);
            string resolved = installation.LastResolved is DateTimeOffset lastResolved ? lastResolved.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "-";
            output.WriteLine($"{installation,-40} {installation.Channel ?? "driver",-8} {size,10}  {resolved,-10}  {installation.Directory}");
        }

        return 0;
    }

    private static async Task<int> ClearAsync(string[] targetTexts, bool dryRun, BrowserDownloadOptions options, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        if (ParseTargets(targetTexts, Installer.ValidateForRemoval, error) is not List<Target> targets)
        {
            return 1;
        }

        List<CachedInstallation> selected;
        try
        {
            selected = [.. BrowserCache.List(options).Where(installation => targets.Count == 0 || targets.Any(target => Installer.Selects(target, installation)))];
        }
        catch (BrowserDownloadException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }

        if (selected.Count == 0)
        {
            output.WriteLine("Nothing to remove.");
            return 0;
        }

        int exitCode = 0;
        foreach (CachedInstallation installation in selected)
        {
            if (dryRun)
            {
                output.WriteLine($"Would remove {installation} ({installation.Channel ?? "driver"})");
                continue;
            }

            try
            {
                await BrowserCache.RemoveAsync(installation, options, cancellationToken).ConfigureAwait(false);
                output.WriteLine($"Removed {installation} ({installation.Channel ?? "driver"})");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is WebDriverBiDiException)
            {
                error.WriteLine($"{installation}: {ex.Message}");
                exitCode = 1;
            }
        }

        return exitCode;
    }

    // Every target is checked before anything is done, so that a mistyped one does not leave the rest half done.
    private static List<Target>? ParseTargets(string[] targetTexts, Action<Target> validate, TextWriter error)
    {
        List<Target> targets = [];
        bool isValid = true;
        foreach (string text in targetTexts)
        {
            try
            {
                Target target = Target.Parse(text);
                validate(target);
                targets.Add(target);
            }
            catch (ArgumentException ex)
            {
                error.WriteLine(ex.Message);
                isValid = false;
            }
        }

        return isValid ? targets : null;
    }
}
