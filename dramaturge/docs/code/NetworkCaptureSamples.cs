// <copyright file="NetworkCaptureSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/network-capture.md

namespace Dramaturge.Docs.Code;

using Dramaturge.Network;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;

/// <summary>
/// Snippets for the network capture guide. Compiled at build time to prevent API drift.
/// </summary>
public static class NetworkCaptureSamples
{
    /// <summary>
    /// Capturing a page's traffic and writing it as a HAR file.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">The browsing context to monitor.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task NetworkCapture(BiDiDriver driver, string contextId)
    {
        #region NetworkCapture
        NetworkTrafficMonitorOptions options = new();
        options.BrowsingContextIds.Add(contextId);

        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync();
        await driver.BrowsingContext.NavigateAsync(contextId, "https://example.com", ReadinessState.Complete);

        // Waits up to ten seconds for requests still in flight; any that are still unfinished are kept for the next call.
        IReadOnlyList<NetworkRequest> traffic = await monitor.GetCapturedTrafficAsync(TimeSpan.FromSeconds(10));
        foreach (NetworkRequest request in traffic)
        {
            Console.WriteLine($"{request.ResponseStatusCode} {request.Method} {request.Url}");
        }

        File.WriteAllText("example.har", HarGenerator.Generate(traffic));
        #endregion
    }

    /// <summary>
    /// Modifying requests and answering authentication challenges.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task NetworkModification(BiDiDriver driver)
    {
        #region NetworkModification
        NetworkTrafficMonitorOptions options = new()
        {
            CaptureBodies = false,
        };

        NetworkRequestModification toStaging = new("https://api.example.com/v1/orders")
        {
            ReplacementUrl = "https://staging-api.example.com/v1/orders",
        };
        toStaging.AdditionalHeaders["X-Test-Run"] = "nightly";
        options.RequestModifications.Add(toStaging);

        options.AuthCredentials.Add(new AuthChallengeCredentials("tester", "secret") { Realm = "staging" });

        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync();
        #endregion
    }

    /// <summary>
    /// Printing captured traffic as HTTP text.
    /// </summary>
    /// <param name="traffic">Captured requests.</param>
    public static void NetworkRequestText(IReadOnlyList<NetworkRequest> traffic)
    {
        #region NetworkRequestText
        foreach (NetworkRequest request in traffic)
        {
            Console.WriteLine(request.GetRequestText());

            // Binary bodies are summarized unless asked for; Display shows the base64, Decode the bytes as UTF-8.
            Console.WriteLine(request.GetResponseText(Base64DisplayBehavior.NoDisplay));
        }
        #endregion
    }
}
