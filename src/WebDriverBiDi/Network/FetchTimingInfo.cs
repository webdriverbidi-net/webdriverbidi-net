// <copyright file="FetchTimingInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

/// <summary>
/// The timings for a fetch operation.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class FetchTimingInfo
{
    private double timeOrigin = 0;
    private double requestTime = 0;
    private double redirectStart = 0;
    private double redirectEnd = 0;
    private double fetchStart = 0;
    private double dnsStart = 0;
    private double dnsEnd = 0;
    private double connectStart = 0;
    private double connectEnd = 0;
    private double tlsStart = 0;
    private double requestStart = 0;
    private double responseStart = 0;
    private double responseEnd = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FetchTimingInfo"/> class.
    /// </summary>
    internal FetchTimingInfo()
    {
    }

    /// <summary>
    /// Gets the time origin of the fetch request.
    /// </summary>
    [JsonProperty("timeOrigin")]
    [JsonRequired]
    public double TimeOrigin { get => this.timeOrigin; internal set => this.timeOrigin = value; }

    /// <summary>
    /// Gets the request time of the fetch request.
    /// </summary>
    [JsonProperty("requestTime")]
    [JsonRequired]
    public double RequestTime { get => this.requestTime; internal set => this.requestTime = value; }

    /// <summary>
    /// Gets the redirect start time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("redirectStart")]
    [JsonRequired]
    public double RedirectStart { get => this.redirectStart; internal set => this.redirectStart = value; }

    /// <summary>
    /// Gets the redirect end time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("redirectEnd")]
    [JsonRequired]
    public double RedirectEnd { get => this.redirectEnd; internal set => this.redirectEnd = value; }

    /// <summary>
    /// Gets the fetch start time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("fetchStart")]
    [JsonRequired]
    public double FetchStart { get => this.fetchStart; internal set => this.fetchStart = value; }

    /// <summary>
    /// Gets the DNS start time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("dnsStart")]
    [JsonRequired]
    public double DnsStart { get => this.dnsStart; internal set => this.dnsStart = value; }

    /// <summary>
    /// Gets the DNS end time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("dnsEnd")]
    [JsonRequired]
    public double DnsEnd { get => this.dnsEnd; internal set => this.dnsEnd = value; }

    /// <summary>
    /// Gets the connect start time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("connectStart")]
    [JsonRequired]
    public double ConnectStart { get => this.connectStart; internal set => this.connectStart = value; }

    /// <summary>
    /// Gets the connect end time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("connectEnd")]
    [JsonRequired]
    public double ConnectEnd { get => this.connectEnd; internal set => this.connectEnd = value; }

    /// <summary>
    /// Gets the TLS start time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("tlsStart")]
    [JsonRequired]
    public double TlsStart { get => this.tlsStart; internal set => this.tlsStart = value; }

    /// <summary>
    /// Gets the request start time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("requestStart")]
    [JsonRequired]
    public double RequestStart { get => this.requestStart; internal set => this.requestStart = value; }

    /// <summary>
    /// Gets the response start time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("responseStart")]
    [JsonRequired]
    public double ResponseStart { get => this.responseStart; internal set => this.responseStart = value; }

    /// <summary>
    /// Gets the response end time offset from the time origin of the fetch request.
    /// </summary>
    [JsonProperty("responseEnd")]
    [JsonRequired]
    public double ResponseEnd { get => this.responseEnd; internal set => this.responseEnd = value; }
}