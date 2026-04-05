// <copyright file="CommandProcessingBenchmarks.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BenchmarkDotNet.Attributes;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// Benchmarks for command creation and processing overhead.
/// </summary>
[MemoryDiagnoser]
public class CommandProcessingBenchmarks
{
    /// <summary>
    /// Benchmark for creating simple command parameters.
    /// </summary>
    [Benchmark]
    public CaptureScreenshotCommandParameters CreateSimpleCommand()
    {
        return new CaptureScreenshotCommandParameters("test-context-id");
    }

    /// <summary>
    /// Benchmark for creating command parameters with optional fields.
    /// </summary>
    [Benchmark]
    public CaptureScreenshotCommandParameters CreateComplexCommand()
    {
        return new CaptureScreenshotCommandParameters("test-context-id")
        {
            Format = new ImageFormat()
            {
                Type = "image/png",
                Quality = 0.9
            },
            Clip = new BoxClipRectangle()
            {
                X = 0,
                Y = 0,
                Width = 1920,
                Height = 1080
            },
            Origin = ScreenshotOrigin.Viewport
        };
    }

    /// <summary>
    /// Benchmark for creating network intercept commands.
    /// </summary>
    [Benchmark]
    public AddInterceptCommandParameters CreateNetworkInterceptCommand()
    {
        return new AddInterceptCommandParameters(InterceptPhase.BeforeRequestSent)
        {
            UrlPatterns = new List<UrlPattern>
            {
                new UrlPatternString("https://example.com/*"),
                new UrlPatternString("https://test.com/*")
            }
        };
    }

    /// <summary>
    /// Benchmark for creating script evaluation commands.
    /// </summary>
    [Benchmark]
    public EvaluateCommandParameters CreateScriptEvaluateCommand()
    {
        return new EvaluateCommandParameters(
            "return document.title;",
            new ContextTarget("test-context-id"),
            awaitPromise: true);
    }

    /// <summary>
    /// Benchmark for creating script evaluation with arguments.
    /// </summary>
    [Benchmark]
    public CallFunctionCommandParameters CreateScriptCallFunctionCommand()
    {
        CallFunctionCommandParameters command = new(
            "function(a, b, c) { return a + b + c; }",
            new ContextTarget("test-context-id"),
            awaitPromise: false);

        command.Arguments.Add(LocalValue.String("test"));
        command.Arguments.Add(LocalValue.Number(42));
        command.Arguments.Add(LocalValue.Boolean(true));

        return command;
    }
}
