// <copyright file="Target.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

using Dramaturge.Browsers;

/// <summary>
/// A browser or driver named on the command line as "name" or "name@spec", where the spec is a channel (a word), a
/// milestone (a number without dots), or a version (a number with dots).
/// </summary>
internal sealed class Target
{
    private static readonly string[] ChromeChannels = ["stable", "beta", "dev", "canary"];
    private static readonly string[] FirefoxChannels = ["stable", "beta", "dev", "nightly", "esr"];
    private static readonly string[] Names = ["chrome", "chrome-headless-shell", "firefox", "chromedriver", "geckodriver", "msedgedriver"];

    private Target(string text, string name, TargetSpecKind specKind, string spec)
    {
        this.Text = text;
        this.Name = name;
        this.SpecKind = specKind;
        this.Spec = spec;
    }

    /// <summary>
    /// Gets the target as written.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the name of the browser or driver, in lowercase.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the kind of spec.
    /// </summary>
    public TargetSpecKind SpecKind { get; }

    /// <summary>
    /// Gets the spec, in lowercase, or an empty string for none.
    /// </summary>
    public string Spec { get; }

    /// <summary>
    /// Gets a value indicating whether the target is a driver.
    /// </summary>
    public bool IsDriver => this.Name.EndsWith("driver", StringComparison.Ordinal);

    /// <summary>
    /// Gets the channels of the target, in its browser's own names.
    /// </summary>
    public IReadOnlyList<string> Channels => this.Name switch
    {
        "firefox" => FirefoxChannels,
        "geckodriver" => [],
        _ => ChromeChannels,
    };

    /// <summary>
    /// Gets the release channel the spec names.
    /// </summary>
    public BrowserReleaseChannel Channel => this.Spec switch
    {
        "beta" => BrowserReleaseChannel.Beta,
        "dev" => BrowserReleaseChannel.DeveloperPreview,
        "canary" or "nightly" => BrowserReleaseChannel.Alpha,
        "esr" => BrowserReleaseChannel.ExtendedSupport,
        _ => BrowserReleaseChannel.Stable,
    };

    /// <summary>
    /// Parses a target.
    /// </summary>
    /// <param name="text">The target as written.</param>
    /// <returns>The target.</returns>
    /// <exception cref="ArgumentException">Thrown when the target names nothing this tool can install, or its spec is not one its browser has.</exception>
    public static Target Parse(string text)
    {
        int separator = text.IndexOf('@');
        string name = (separator < 0 ? text : text.Substring(0, separator)).ToLowerInvariant();
        string spec = separator < 0 ? string.Empty : text.Substring(separator + 1).ToLowerInvariant();
        if (name == "edge")
        {
            throw new ArgumentException($"{text}: Microsoft Edge is never downloaded; install it from Microsoft. msedgedriver downloads the driver for the installed Edge.");
        }

        if (name == "safari")
        {
            throw new ArgumentException($"{text}: Safari is part of macOS, and is never downloaded.");
        }

        if (Array.IndexOf(Names, name) < 0)
        {
            throw new ArgumentException($"{text}: '{name}' is not a browser or driver this tool installs. Use one of: {string.Join(", ", Names)}.");
        }

        if (separator >= 0 && spec.Length == 0)
        {
            throw new ArgumentException($"{text}: name a channel or version after '@'.");
        }

        if (spec.Length > 0 && spec.All(char.IsDigit) && !int.TryParse(spec, out _))
        {
            throw new ArgumentException($"{text}: '{spec}' is too large to be a milestone.");
        }

        TargetSpecKind specKind = spec.Length == 0 ? TargetSpecKind.None
            : spec.All(char.IsDigit) ? TargetSpecKind.Milestone
            : char.IsDigit(spec[0]) ? TargetSpecKind.Version
            : TargetSpecKind.Channel;
        Target target = new(text, name, specKind, spec);
        if (specKind == TargetSpecKind.Channel && !target.Channels.Contains(spec))
        {
            string channels = target.Channels.Count == 0 ? $"{name} has no channels" : $"{name} channels are {string.Join(", ", target.Channels)}";
            throw new ArgumentException($"{text}: '{spec}' is not a channel; {channels}.");
        }

        return target;
    }
}
