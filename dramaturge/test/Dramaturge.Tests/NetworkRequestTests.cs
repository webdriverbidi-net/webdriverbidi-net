// <copyright file="NetworkRequestTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Network;
using static Dramaturge.TestUtilities.NetworkEvents;

public class NetworkRequestTests
{
    private static readonly string NewLine = Environment.NewLine;

    [Fact]
    public async Task RequestAndResponseAreFormattedAsHttpText()
    {
        NetworkRequest request = Assert.Single(await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 4);
            await ResponseCompletedAsync(remoteEnd, "request-1", headers: new() { ["content-type"] = "text/plain" });
        }));
        await request.WaitForRequestBodyAsync();
        await request.WaitForResponseBodyAsync();

        Assert.Equal($"POST https://example.com/{NewLine}accept: */*{NewLine}{NewLine}body{NewLine}", request.GetRequestText());
        Assert.Equal($"http/1.1 200 OK{NewLine}content-type: text/plain{NewLine}{NewLine}body{NewLine}", request.GetResponseText());
    }

    [Fact]
    public async Task RequestWithoutBodyEndsAfterItsHeaders()
    {
        NetworkRequest request = Assert.Single(await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1");
            await ResponseCompletedAsync(remoteEnd, "request-1");
        }));

        Assert.Equal($"GET https://example.com/{NewLine}accept: */*{NewLine}{NewLine}", request.GetRequestText());
    }

    [Theory]
    [InlineData(Base64DisplayBehavior.NoDisplay, "[Base64-encoded binary data (length: 4)]\n")]
    [InlineData(Base64DisplayBehavior.Display, "aGk=\n")]
    [InlineData(Base64DisplayBehavior.Decode, "hi")]
    public async Task Base64BodiesAreShownAsAsked(Base64DisplayBehavior behavior, string expectedBody)
    {
        NetworkRequest request = Assert.Single(await CaptureAsync(
            async remoteEnd =>
            {
                await BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 2);
                await ResponseCompletedAsync(remoteEnd, "request-1");
            },
            _ => Bytes("base64", "aGk=")));
        expectedBody = expectedBody.Replace("\n", NewLine);

        Assert.EndsWith($"{NewLine}{NewLine}{expectedBody}", request.GetRequestText(behavior));
        Assert.EndsWith($"{NewLine}{NewLine}{expectedBody}", request.GetResponseText(behavior));
    }

    [Fact]
    public async Task UnavailableBodiesAreExplained()
    {
        NetworkRequest request = Assert.Single(await CaptureAsync(async remoteEnd =>
        {
            remoteEnd.FailWith("network.getData", "no such network data", "Evicted");
            await BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 2);
            await ResponseCompletedAsync(remoteEnd, "request-1");
        }));

        Assert.All(
            [request.GetRequestText(), request.GetResponseText()],
            text => Assert.Matches($"{NewLine}{NewLine}\\[Body unavailable: .*Evicted\\]{NewLine}$", text));
    }

    [Fact]
    public async Task FailedRequestHasNoResponseText()
    {
        NetworkRequest request = Assert.Single(await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1");
            await FetchErrorAsync(remoteEnd, "request-1", "net::ERR_FAILED");
        }));

        Assert.Equal($"[Request failed: net::ERR_FAILED]{NewLine}", request.GetResponseText());
    }
}
