// <copyright file="NetworkTrafficMonitorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.Network;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.Network;
using static Dramaturge.TestUtilities.NetworkEvents;

public class NetworkTrafficMonitorTests
{
    [Fact]
    public async Task StartScopesCollectorAndSubscriptionToContexts()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitorOptions options = new() { MaxBodySize = 1024 };
        options.BrowsingContextIds.Add(ContextId);
        await using NetworkTrafficMonitor monitor = new(driver, options);

        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        Assert.True(monitor.IsMonitoring);
        JsonNode collector = Assert.Single(remoteEnd.CommandsFor("network.addDataCollector"))["params"]!;
        Assert.Equal(1024, (int?)collector["maxEncodedDataSize"]);
        Assert.Equal($"""["{ContextId}"]""", collector["contexts"]!.ToJsonString());
        JsonNode subscribe = Assert.Single(remoteEnd.CommandsFor("session.subscribe"))["params"]!;
        Assert.Equal($"""["{ContextId}"]""", subscribe["contexts"]!.ToJsonString());
        Assert.Equal(3, subscribe["events"]!.AsArray().Count);
        Assert.Empty(remoteEnd.CommandsFor("network.addIntercept"));
    }

    [Fact]
    public async Task MonitoringEveryContextSendsNoContexts()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);

        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        Assert.Null(Assert.Single(remoteEnd.CommandsFor("network.addDataCollector"))["params"]!["contexts"]);
        Assert.Null(Assert.Single(remoteEnd.CommandsFor("session.subscribe"))["params"]!["contexts"]);
    }

    [Fact]
    public async Task StartingTwiceIsRejectedButRestartingAfterStopIsNot()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => monitor.StartMonitoringAsync(TestContext.Current.CancellationToken));
        await monitor.StopMonitoringAsync(TestContext.Current.CancellationToken);
        await monitor.StopMonitoringAsync(TestContext.Current.CancellationToken);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        Assert.True(monitor.IsMonitoring);
        Assert.Equal(2, remoteEnd.CommandsFor("session.subscribe").Count);
        Assert.Single(remoteEnd.CommandsFor("session.unsubscribe"));
    }

    [Fact]
    public async Task FailedStartRemovesWhatItSetUp()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.FailWith("session.subscribe", "invalid argument", "No such event");
        NetworkTrafficMonitorOptions options = new();
        options.RequestModifications.Add(new NetworkRequestModification("https://example.com/*"));
        options.AuthCredentials.Add(new AuthChallengeCredentials("user", "password"));
        await using NetworkTrafficMonitor monitor = new(driver, options);

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => monitor.StartMonitoringAsync(TestContext.Current.CancellationToken));
        await BeforeRequestSentAsync(remoteEnd, "request-1");
        await FlushAsync(driver);

        Assert.False(monitor.IsMonitoring);
        Assert.Single(remoteEnd.CommandsFor("network.removeDataCollector"));
        Assert.Equal(2, remoteEnd.CommandsFor("network.removeIntercept").Count);
        Assert.Empty(await monitor.GetCapturedTrafficAsync(TimeSpan.Zero, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FailedStartReportsItsOwnFailureWhenCleanupAlsoFails()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.FailWith("session.subscribe", "invalid argument", "No such event");
        remoteEnd.FailWith("network.removeDataCollector", "no such network collector", "Gone");
        await using NetworkTrafficMonitor monitor = new(driver);

        WebDriverBiDiCommandException exception = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => monitor.StartMonitoringAsync(TestContext.Current.CancellationToken));

        Assert.Contains("No such event", exception.Message);
        Assert.False(monitor.IsMonitoring);
    }

    [Fact]
    public async Task FirstOutcomeOfARequestStands()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1");
        await FetchErrorAsync(remoteEnd, "request-1", "net::ERR_ABORTED");
        await ResponseCompletedAsync(remoteEnd, "request-1");
        await BeforeRequestSentAsync(remoteEnd, "request-2");
        await ResponseCompletedAsync(remoteEnd, "request-2");
        await FlushAsync(driver);
        await monitor.DisposeAsync();
        IReadOnlyList<NetworkRequest> requests = await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["request-1:net::ERR_ABORTED", "request-2:"], requests.Select(request => $"{request.RequestId}:{request.FetchErrorText}").Order());
    }

    [Fact]
    public async Task OutcomesForRequestsNotRecordedAreIgnored()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await ResponseCompletedAsync(remoteEnd, "request-1");
        await FetchErrorAsync(remoteEnd, "request-2", "net::ERR_FAILED");
        await FlushAsync(driver);

        Assert.Empty(await monitor.GetCapturedTrafficAsync(TimeSpan.Zero, TestContext.Current.CancellationToken));
        Assert.Empty(remoteEnd.CommandsFor("network.getData"));
    }

    [Fact]
    public async Task RequestAndResponseAreCapturedWithBodies()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1", "DELETE", bodySize: 12);
        await ResponseCompletedAsync(remoteEnd, "request-1");

        await FlushAsync(driver);
        NetworkRequest request = Assert.Single(await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("DELETE", request.Method);
        Assert.Equal(ContextId, request.BrowsingContextId);
        Assert.Equal("request body", request.RequestBody);
        Assert.Equal("response body", request.ResponseBody);
        Assert.Equal(200ul, request.ResponseStatusCode);
        Assert.Empty(await monitor.GetCapturedTrafficAsync(TimeSpan.Zero, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RequestWithoutBodyIsNotAskedForOne()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1", "POST");
        await ResponseCompletedAsync(remoteEnd, "request-1");
        await FlushAsync(driver);
        await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("response", (string?)Assert.Single(remoteEnd.CommandsFor("network.getData"))["params"]!["dataType"]);
    }

    // The response arrives right behind the request, as it does when the browser is quick.
    [Fact]
    public async Task ResponseRightBehindRequestWithBodyIsNotLost()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        Task request = BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 12);
        Task response = ResponseCompletedAsync(remoteEnd, "request-1");
        await Task.WhenAll(request, response);

        await FlushAsync(driver);
        NetworkRequest captured = Assert.Single(await monitor.GetCapturedTrafficAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
        Assert.Equal(200ul, captured.ResponseStatusCode);
    }

    [Fact]
    public async Task TimingsAreThoseReportedWhenTheResponseCompleted()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);
        JsonObject completedMarks = new()
        {
            ["timeOrigin"] = 0, ["requestTime"] = 0, ["redirectStart"] = 0, ["redirectEnd"] = 0, ["fetchStart"] = 3, ["dnsStart"] = 0, ["dnsEnd"] = 0,
            ["connectStart"] = 0, ["connectEnd"] = 0, ["tlsStart"] = 0, ["requestStart"] = 4, ["responseStart"] = 40, ["responseEnd"] = 45,
        };

        await BeforeRequestSentAsync(remoteEnd, "request-1");
        await ResponseCompletedAsync(remoteEnd, "request-1", timings: completedMarks);
        await FlushAsync(driver);

        NetworkRequest request = Assert.Single(await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(40, request.Timings.ResponseStart);
        Assert.Equal(45, request.Timings.ResponseEnd);
    }

    [Theory]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(303)]
    [InlineData(307)]
    [InlineData(308)]
    public async Task EachRedirectHopIsCapturedSeparately(ulong status)
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1", url: "https://example.com/old");
        await ResponseCompletedAsync(remoteEnd, "request-1", status, headers: new() { ["location"] = "https://example.com/new" });
        await BeforeRequestSentAsync(remoteEnd, "request-1", redirectCount: 1, url: "https://example.com/new");
        await ResponseCompletedAsync(remoteEnd, "request-1", 200, redirectCount: 1);

        await FlushAsync(driver);
        IReadOnlyList<NetworkRequest> requests = await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([$"https://example.com/old:0:{status}", "https://example.com/new:1:200"], requests.Select(request => $"{request.Url}:{request.RedirectCount}:{request.ResponseStatusCode}"));
        Assert.Single(remoteEnd.CommandsFor("network.getData"));
        Assert.Equal(string.Empty, requests[0].ResponseBody);
    }

    [Fact]
    public async Task FailedRequestIsCapturedWithItsError()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1");
        await FetchErrorAsync(remoteEnd, "request-1", "net::ERR_NAME_NOT_RESOLVED");

        await FlushAsync(driver);
        NetworkRequest request = Assert.Single(await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.True(request.IsFailed);
        Assert.Equal("net::ERR_NAME_NOT_RESOLVED", request.FetchErrorText);
    }

    [Fact]
    public async Task UnavailableBodyIsReported()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.FailWith("network.getData", "no such network data", "Too large");
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 12);
        await ResponseCompletedAsync(remoteEnd, "request-1");

        await FlushAsync(driver);
        NetworkRequest request = Assert.Single(await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Too large", request.RequestBodyErrorText);
        Assert.Contains("Too large", request.ResponseBodyErrorText);
    }

    [Fact]
    public async Task BodiesAreNotCapturedWhenTurnedOff()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver, new NetworkTrafficMonitorOptions() { CaptureBodies = false });
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 12);
        await ResponseCompletedAsync(remoteEnd, "request-1");
        await FlushAsync(driver);
        await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(remoteEnd.CommandsFor("network.addDataCollector"));
        Assert.Empty(remoteEnd.CommandsFor("network.getData"));
    }

    [Fact]
    public async Task MatchingRequestIsModifiedAsConfiguredWhenStarted()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkRequestModification modification = new("https://example.com/*") { ReplacementUrl = "https://example.com/replaced", ReplacementMethod = "POST", ReplacementBody = "body" };
        modification.AdditionalHeaders["Accept"] = "text/plain";
        NetworkTrafficMonitorOptions options = new();
        options.RequestModifications.Add(modification);
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);
        string interceptPattern = (string)Assert.Single(remoteEnd.CommandsFor("network.addIntercept"))["params"]!["urlPatterns"]![0]!["pattern"]!;
        modification.ReplacementUrl = "https://example.com/changed-after-start";

        await BeforeRequestSentAsync(remoteEnd, "request-1", intercepts: [InterceptIdFor(remoteEnd, 1)]);

        JsonNode continued = (await remoteEnd.WaitForCommandAsync("network.continueRequest"))["params"]!;
        Assert.Equal("https://example.com/*", interceptPattern);
        Assert.Equal("https://example.com/replaced", (string?)continued["url"]);
        Assert.Equal("POST", (string?)continued["method"]);
        Assert.Equal("body", (string?)continued["body"]!["value"]);
        Assert.Equal("""[{"name":"Accept","value":{"type":"string","value":"text/plain"}}]""", continued["headers"]!.ToJsonString());
    }

    [Fact]
    public async Task RequestThatCannotBeContinuedIsFailed()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.FailWith("network.continueRequest", "invalid argument", "Bad URL");
        NetworkTrafficMonitorOptions options = new();
        options.RequestModifications.Add(new NetworkRequestModification("https://example.com/*") { ReplacementUrl = "not a url" });
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1", intercepts: [InterceptIdFor(remoteEnd, 1)]);

        Assert.Equal("request-1", (string?)(await remoteEnd.WaitForCommandAsync("network.failRequest"))["params"]!["request"]);
    }

    [Fact]
    public async Task RequestBlockedByAnotherInterceptIsLeftAlone()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitorOptions options = new();
        options.RequestModifications.Add(new NetworkRequestModification("https://example.com/*"));
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1", intercepts: ["someone-elses-intercept"]);
        await FlushAsync(driver);

        Assert.Empty(remoteEnd.CommandsFor("network.continueRequest"));
    }

    [Fact]
    public async Task CredentialsAreOfferedUntilAttemptsRunOutThenChallengeIsCanceled()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitorOptions options = new() { MaxAuthAttempts = 2 };
        options.AuthCredentials.Add(new AuthChallengeCredentials("user", "password") { Scheme = "Basic" });
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);
        string authInterceptId = InterceptIdFor(remoteEnd, 1);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            await AuthRequiredAsync(remoteEnd, authInterceptId);
            await remoteEnd.WaitForCommandAsync("network.continueWithAuth", attempt);
        }

        Assert.Equal(["provideCredentials", "provideCredentials", "cancel"], remoteEnd.CommandsFor("network.continueWithAuth").Select(command => (string)command["params"]!["action"]!));
        Assert.Equal("user", (string?)remoteEnd.CommandsFor("network.continueWithAuth")[0]["params"]!["credentials"]!["username"]);
    }

    [Fact]
    public async Task ChallengeWithoutMatchingCredentialsGetsDefaultBehavior()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitorOptions options = new();
        options.AuthCredentials.Add(new AuthChallengeCredentials("user", "password") { Realm = "other realm" });
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await AuthRequiredAsync(remoteEnd, InterceptIdFor(remoteEnd, 1));

        Assert.Equal("default", (string?)(await remoteEnd.WaitForCommandAsync("network.continueWithAuth"))["params"]!["action"]);
    }

    [Fact]
    public async Task ChallengeNotBlockedByTheMonitorIsLeftAlone()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitorOptions options = new();
        options.AuthCredentials.Add(new AuthChallengeCredentials("user", "password"));
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await AuthRequiredAsync(remoteEnd, null, "request-1");
        await AuthRequiredAsync(remoteEnd, "intercept-other", "request-2");
        await AuthRequiredAsync(remoteEnd, InterceptIdFor(remoteEnd, 1), "request-3");
        await remoteEnd.WaitForCommandAsync("network.continueWithAuth");
        await FlushAsync(driver);

        Assert.Equal("request-3", (string?)Assert.Single(remoteEnd.CommandsFor("network.continueWithAuth"))["params"]!["request"]);
    }

    [Theory]
    [InlineData("", null, null, "provideCredentials")]
    [InlineData("[]", null, null, "provideCredentials")]
    [InlineData("", "Basic", null, "default")]
    [InlineData("", null, "site", "default")]
    public async Task ChallengeThatListsNoSchemesIsAnsweredOnlyByUnrestrictedCredentials(string challenges, string? scheme, string? realm, string expectedAction)
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitorOptions options = new();
        options.AuthCredentials.Add(new AuthChallengeCredentials("user", "password") { Scheme = scheme, Realm = realm });
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await AuthRequiredAsync(remoteEnd, InterceptIdFor(remoteEnd, 1), challenges: challenges);

        Assert.Equal(expectedAction, (string?)(await remoteEnd.WaitForCommandAsync("network.continueWithAuth"))["params"]!["action"]);
    }

    [Fact]
    public async Task ChallengeThatCannotBeAnsweredIsCanceled()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.FailWith("network.continueWithAuth", "no such request", "Gone");
        NetworkTrafficMonitorOptions options = new();
        options.AuthCredentials.Add(new AuthChallengeCredentials("user", "password"));
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await AuthRequiredAsync(remoteEnd, InterceptIdFor(remoteEnd, 1));

        Assert.Equal("cancel", (string?)(await remoteEnd.WaitForCommandAsync("network.continueWithAuth", 2))["params"]!["action"]);
    }

    [Fact]
    public async Task RequestsBeyondTheLimitAreCountedButStillContinued()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitorOptions options = new() { MaxRetainedRequests = 1 };
        options.RequestModifications.Add(new NetworkRequestModification("https://example.com/*"));
        await using NetworkTrafficMonitor monitor = new(driver, options);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);

        await BeforeRequestSentAsync(remoteEnd, "request-1");
        await BeforeRequestSentAsync(remoteEnd, "request-2", intercepts: [InterceptIdFor(remoteEnd, 1)]);
        await ResponseCompletedAsync(remoteEnd, "request-1");
        await ResponseCompletedAsync(remoteEnd, "request-2");

        await FlushAsync(driver);
        Assert.Equal("request-1", Assert.Single(await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken)).RequestId);
        Assert.Equal(1, monitor.DroppedRequestCount);
        Assert.Equal("request-2", (string?)(await remoteEnd.WaitForCommandAsync("network.continueRequest"))["params"]!["request"]);
    }

    [Fact]
    public async Task RequestInFlightIsKeptForLaterUnlessMonitoringStops()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);
        await BeforeRequestSentAsync(remoteEnd, "request-1");
        await FlushAsync(driver);

        IReadOnlyList<NetworkRequest> whileInFlight = await monitor.GetCapturedTrafficAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        await monitor.DisposeAsync();
        await monitor.DisposeAsync();
        IReadOnlyList<NetworkRequest> afterStop = await monitor.GetCapturedTrafficAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(whileInFlight);
        Assert.True(Assert.Single(afterStop).IsFailed);
        Assert.Single(remoteEnd.CommandsFor("network.removeDataCollector"));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => monitor.StartMonitoringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WaitForTrafficIsCancellable()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);
        await BeforeRequestSentAsync(remoteEnd, "request-1");
        await FlushAsync(driver);
        using CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => monitor.GetCapturedTrafficAsync(cancellationToken: cancellationSource.Token));
    }

    [Fact]
    public async Task DisposingWhenDriverIsGoneDoesNotThrow()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await ConnectAsync();
        NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);
        await driver.StopAsync(TestContext.Current.CancellationToken);

        await monitor.DisposeAsync();

        Assert.False(monitor.IsMonitoring);
    }

    [Fact]
    public void OptionLimitsMustBePositive()
    {
        NetworkTrafficMonitorOptions options = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxBodySize = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxRetainedRequests = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxAuthAttempts = 0);
    }

    private static async Task<(BiDiDriver Driver, FakeRemoteEnd RemoteEnd)> ConnectAsync()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        remoteEnd.AnswerWith("network.getData", parameters => new JsonObject() { ["bytes"] = new JsonObject() { ["type"] = "string", ["value"] = $"{parameters["dataType"]} body" } });
        return (driver, remoteEnd);
    }

    private static string InterceptIdFor(FakeRemoteEnd remoteEnd, int index) => (string)remoteEnd.ResultsFor("network.addIntercept")[index - 1]["intercept"]!;
}
