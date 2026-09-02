// <copyright file="NetStandardCompatibilityTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Compatibility.Tests;

using System.Text.Json;
using PinchHitter;
using WebDriverBiDi.TestUtilities;

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

                // Reply to session.status with an error so the smoke app can exercise the
                // netstandard2.0 command-failure path (error-response deserialization to a
                // WebDriverBiDiCommandException) rather than only success responses.
                "session.status" => $$"""{ "type": "error", "id": {{id}}, "error": "unknown error", "message": "simulated command failure for netstandard smoke test" }""",
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
                        },
                        "compatFractionalExtra": 1.5,
                        "compatOverflowExtra": 1e400,
                        "compatNegativeOverflowExtra": -1e400
                      }
                    }
                    """;
                await server.SendWebSocketDataAsync(e.ConnectionId, eventJson);
            }
        });

        await server.StartAsync();

        RunProcessResult runResult = await ProcessRunner.RunProcessAsync(
            "dotnet",
            $"\"{this.fixture.DllPath}\" ws://localhost:{server.Port} \"{this.fixture.PipePeerPath}\"",
            workingDirectory: this.fixture.BuildDir,
            timeout: TimeSpan.FromSeconds(30),
            diagnosticReporter: (output) => TestContext.Current.SendDiagnosticMessage(output));


        await server.StopAsync();

        Assert.Contains(".NETStandard,Version=v2.0", runResult.StandardOutputConsoleContent);

        // Prove the numeric extension-data validation actually ran (rather than being silently
        // skipped), exercising JsonConverterUtilities' netstandard2.0 numeric conversion path.
        Assert.Contains("Numeric extension data converted as expected.", runResult.StandardOutputConsoleContent);

        // Prove the completion-wait timeout scenario actually ran (rather than being silently
        // skipped), exercising the netstandard2.0 TimeProvider.Delay path in the completion
        // phase of WaitForCapturedTasksCompleteAsync.
        Assert.Contains("Completion wait timed out while a handler was pending, exercising the provider-based delay path.", runResult.StandardOutputConsoleContent);

        // Prove the pipe transport round trip actually ran (rather than being silently skipped),
        // exercising the netstandard2.0 PipeConnection code paths in addition to the WebSocket ones.
        Assert.Contains("Pipe transport round-trip over the netstandard2.0 build succeeded.", runResult.StandardOutputConsoleContent);
        Assert.Contains("PASS:", runResult.StandardOutputConsoleContent);
        Assert.Equal(0, runResult.ExitCode);
    }
}
