// <copyright file="NetStandardCompatibilityTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Integration.Tests;

using System.Text.Json;
using PinchHitter;

public class NetStandardCompatibilityTests : IClassFixture<NetStandardCompatibilityFixture>
{
    private readonly NetStandardCompatibilityFixture fixture;

    public NetStandardCompatibilityTests(NetStandardCompatibilityFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task TestCanExecuteAgainstNetStandard20Build()
    {
        // No real browser is needed here. This test only needs to prove that the
        // netstandard2.0 build of WebDriverBiDi correctly connects, serializes and
        // deserializes commands, and dispatches events at runtime — so a scripted
        // WebSocket server standing in for the remote end is sufficient and keeps this
        // test independent of browser availability.
        await using Server server = new();
        server.OnDataReceived.AddObserver(async e =>
        {
            // PinchHitter's OnDataReceived fires for the raw HTTP upgrade handshake as
            // well as post-handshake WebSocket frames; only the latter carries BiDi
            // command JSON, so non-JSON payloads (the handshake) are ignored here.
            if (!e.Data.TrimStart().StartsWith('{'))
            {
                return;
            }

            using JsonDocument document = JsonDocument.Parse(e.Data);
            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("method", out JsonElement methodElement) || !root.TryGetProperty("id", out JsonElement idElement))
            {
                return;
            }

            string method = methodElement.GetString() ?? string.Empty;
            int id = idElement.GetInt32();

            string? responseJson = method switch
            {
                "session.new" => $$"""
                    {
                      "type": "success",
                      "id": {{id}},
                      "result": {
                        "sessionId": "netStandardSmokeTestSession",
                        "capabilities": {
                          "browserName": "greatBrowser",
                          "browserVersion": "101.5b",
                          "platformName": "otherOS",
                          "userAgent": "WebDriverBidi.NET/1.0",
                          "acceptInsecureCerts": true,
                          "proxy": {
                            "proxyType": "system"
                          },
                          "setWindowRect": true
                        }
                      }
                    }
                    """,
                "session.subscribe" => $$"""{ "type": "success", "id": {{id}}, "result": { "subscription": "netStandardSmokeTestSubscription" } }""",
                "session.end" => $$"""{ "type": "success", "id": {{id}}, "result": {} }""",
                _ => null,
            };

            if (responseJson is not null)
            {
                await server.SendWebSocketDataAsync(e.ConnectionId, responseJson);
            }

            // Once the smoke app subscribes, push an unsolicited log.entryAdded event so
            // it can exercise EventObserver<T>'s capture/notification path.
            if (method == "session.subscribe")
            {
                long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
                string eventJson = $$"""
                    {
                      "type": "event",
                      "method": "log.entryAdded",
                      "params": {
                        "type": "javascript",
                        "level": "debug",
                        "source": {
                          "realm": "myRealmId",
                          "context": "browsingContextId"
                        },
                        "text": "netstandard2.0 smoke test log entry",
                        "timestamp": {{epochTimestamp}},
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                    }
                    """;
                await server.SendWebSocketDataAsync(e.ConnectionId, eventJson);
            }
        });

        await server.StartAsync();

        RunProcessResult runResult = await ProcessRunner.RunProcessAsync(
            "dotnet",
            $"\"{this.fixture.DllPath}\" ws://localhost:{server.Port}",
            workingDirectory: this.fixture.BuildDir,
            timeout: TimeSpan.FromSeconds(30));

        await server.StopAsync();

        Assert.Contains(".NETStandard,Version=v2.0", runResult.StandardOutputConsoleContent);
        Assert.Contains("PASS:", runResult.StandardOutputConsoleContent);
        Assert.Equal(0, runResult.ExitCode);
    }
}
