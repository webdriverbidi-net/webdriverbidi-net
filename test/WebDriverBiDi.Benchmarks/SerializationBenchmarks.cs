// <copyright file="SerializationBenchmarks.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json;
using BenchmarkDotNet.Attributes;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// Benchmarks for JSON serialization and deserialization of WebDriver BiDi protocol messages.
/// </summary>
[MemoryDiagnoser]
public class SerializationBenchmarks
{
    private string captureScreenshotCommandJson = string.Empty;
    private string captureScreenshotResultJson = string.Empty;
    private string beforeRequestSentEventJson = string.Empty;
    private string numberRemoteValueJson = string.Empty;
    private string complexRemoteValueJson = string.Empty;
    private JsonSerializerOptions jsonOptions = new();

    /// <summary>
    /// Sets up test data for benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Command parameters example
        CaptureScreenshotCommandParameters captureCommand = new("test-context-id")
        {
            Format = new ImageFormat()
            {
                Type = "image/png",
                Quality = 1.0
            },
            Origin = ScreenshotOrigin.Document
        };
        this.captureScreenshotCommandJson = JsonSerializer.Serialize(captureCommand);

        // Command result with large base64-encoded data (simulating a screenshot)
        this.captureScreenshotResultJson = """
        {
          "data": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
        }
        """;

        // Network event example
        this.beforeRequestSentEventJson = """
        {
          "context": "test-context",
          "navigation": "test-navigation",
          "isBlocked": false,
          "redirectCount": 0,
          "request": {
            "request": "test-request-id",
            "url": "https://example.com/test",
            "method": "GET",
            "destination": "document",
            "initiatorType": "parser",
            "headers": [
              { "name": "User-Agent", "value": { "type": "string", "value": "test-agent" } }
            ],
            "cookies": [],
            "headersSize": 100,
            "bodySize": 0,
            "timings": {
              "timeOrigin": 1234567890000.0,
              "requestTime": 1234567890000.0,
              "redirectStart": 0.0,
              "redirectEnd": 0.0,
              "fetchStart": 0.0,
              "dnsStart": 0.0,
              "dnsEnd": 0.0,
              "connectStart": 0.0,
              "connectEnd": 0.0,
              "tlsStart": 0.0,
              "requestStart": 0.0,
              "responseStart": 0.0,
              "responseEnd": 0.0
            }
          },
          "timestamp": 1234567890,
          "initiator": {
            "type": "parser"
          }
        }
        """;

        // Simple RemoteValue
        this.numberRemoteValueJson = """
        {
          "type": "number",
          "value": 42
        }
        """;

        // Complex RemoteValue (array)
        this.complexRemoteValueJson = """
        {
          "type": "array",
          "value": [
            { "type": "string", "value": "test" },
            { "type": "number", "value": 123 },
            { "type": "boolean", "value": true },
            { "type": "null" }
          ]
        }
        """;

        this.jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Benchmark for serializing command parameters.
    /// </summary>
    [Benchmark]
    public string SerializeCommandParameters()
    {
        CaptureScreenshotCommandParameters command = new("test-context-id")
        {
            Format = new ImageFormat()
            {
                Type = "image/png",
                Quality = 1.0
            },
            Origin = ScreenshotOrigin.Document
        };
        return JsonSerializer.Serialize(command);
    }

    /// <summary>
    /// Benchmark for deserializing command results with large payloads.
    /// </summary>
    [Benchmark]
    public CaptureScreenshotCommandResult? DeserializeCommandResult()
    {
        return JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(this.captureScreenshotResultJson);
    }

    /// <summary>
    /// Benchmark for deserializing network events.
    /// </summary>
    [Benchmark]
    public BeforeRequestSentEventArgs? DeserializeNetworkEvent()
    {
        return JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(this.beforeRequestSentEventJson);
    }

    /// <summary>
    /// Benchmark for deserializing simple RemoteValue.
    /// </summary>
    [Benchmark]
    public RemoteValue? DeserializeSimpleRemoteValue()
    {
        return JsonSerializer.Deserialize<RemoteValue>(this.numberRemoteValueJson);
    }

    /// <summary>
    /// Benchmark for deserializing complex RemoteValue (discriminated union).
    /// </summary>
    [Benchmark]
    public RemoteValue? DeserializeComplexRemoteValue()
    {
        return JsonSerializer.Deserialize<RemoteValue>(this.complexRemoteValueJson);
    }
}
