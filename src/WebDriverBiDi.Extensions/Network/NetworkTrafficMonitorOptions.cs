// <copyright file="NetworkTrafficMonitorOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

/// <summary>
/// Settings for a <see cref="NetworkTrafficMonitor"/>, read when monitoring starts; changes made after
/// that do not affect the monitoring in progress.
/// </summary>
public sealed class NetworkTrafficMonitorOptions
{
    private int maxRetainedRequests = 10_000;
    private ulong maxBodySize = 20 * 1024 * 1024;
    private int maxAuthAttempts = 3;

    /// <summary>
    /// Gets the IDs of the top-level browsing contexts whose traffic is monitored. If empty, the traffic of every
    /// browsing context is monitored.
    /// </summary>
    public IList<string> BrowsingContextIds { get; } = new List<string>();

    /// <summary>
    /// Gets the modifications made to matching requests before they are sent.
    /// </summary>
    public IList<NetworkRequestModification> RequestModifications { get; } = new List<NetworkRequestModification>();

    /// <summary>
    /// Gets the credentials offered in answer to authentication challenges, the first matching one for each challenge.
    /// </summary>
    public IList<AuthChallengeCredentials> AuthCredentials { get; } = new List<AuthChallengeCredentials>();

    /// <summary>
    /// Gets or sets a value indicating whether request and response bodies are captured. Defaults to <see langword="true"/>.
    /// </summary>
    public bool CaptureBodies { get; set; } = true;

    /// <summary>
    /// Gets or sets the largest body, in bytes, that is captured; a larger body is reported as unavailable.
    /// Defaults to 20 MB.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to zero.</exception>
    public ulong MaxBodySize
    {
        get => this.maxBodySize;
        set => this.maxBodySize = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "The maximum body size must be positive.");
    }

    /// <summary>
    /// Gets or sets the most requests kept before they are retrieved with
    /// <see cref="NetworkTrafficMonitor.GetCapturedTrafficAsync"/>. Requests beyond it are not recorded, and are
    /// counted by <see cref="NetworkTrafficMonitor.DroppedRequestCount"/>. Defaults to 10,000.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that is not positive.</exception>
    public int MaxRetainedRequests
    {
        get => this.maxRetainedRequests;
        set => this.maxRetainedRequests = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "The maximum number of retained requests must be positive.");
    }

    /// <summary>
    /// Gets or sets how many times credentials are offered for one request before its challenge is canceled, so that
    /// rejected credentials are not offered forever. Defaults to 3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that is not positive.</exception>
    public int MaxAuthAttempts
    {
        get => this.maxAuthAttempts;
        set => this.maxAuthAttempts = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "The maximum number of authentication attempts must be positive.");
    }
}
