// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

// Use the library-provided source-generated context as the type info resolver.
// WebDriverBiDiJsonSerializerContext provides AOT-compatible metadata for all
// serializable types and roots enum array types in its static constructor.
var options = new JsonSerializerOptions
{
    TypeInfoResolver = WebDriverBiDiJsonSerializerContext.Default,
};

int failures = 0;

// Test 1: CommandJsonConverter with NavigateCommandParameters
try
{
    var parameters = new NavigateCommandParameters("ctx-1", "https://example.com");
    var command = new Command(1, parameters);
    byte[] json = JsonSerializer.SerializeToUtf8Bytes(command, options);
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    bool pass = root.GetProperty("id").GetInt64() == 1
        && root.GetProperty("method").GetString() == "browsingContext.navigate"
        && root.GetProperty("params").GetProperty("context").GetString() == "ctx-1"
        && root.GetProperty("params").GetProperty("url").GetString() == "https://example.com";

    Console.WriteLine(pass ? "PASS: CommandJsonConverter + NavigateCommandParameters" : "FAIL: CommandJsonConverter + NavigateCommandParameters");
    if (!pass) failures++;
}
catch (Exception ex)
{
    Console.WriteLine($"FAIL: CommandJsonConverter + NavigateCommandParameters — {ex.GetType().Name}: {ex.Message}");
    failures++;
}

// Test 2: ProxyConfigurationJsonConverter with ManualProxyConfiguration
try
{
    var newParams = new NewCommandParameters();
    newParams.Capabilities = new CapabilitiesRequest
    {
        AlwaysMatch = new CapabilityRequest
        {
            Proxy = new ManualProxyConfiguration
            {
                HttpProxy = "proxy.example.com:8080",
            },
        },
    };
    var command = new Command(2, newParams);
    byte[] json = JsonSerializer.SerializeToUtf8Bytes(command, options);
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    var proxy = root.GetProperty("params")
        .GetProperty("capabilities")
        .GetProperty("alwaysMatch")
        .GetProperty("proxy");

    bool pass = root.GetProperty("method").GetString() == "session.new"
        && proxy.GetProperty("proxyType").GetString() == "manual"
        && proxy.GetProperty("httpProxy").GetString() == "proxy.example.com:8080";

    Console.WriteLine(pass ? "PASS: ProxyConfigurationJsonConverter + ManualProxyConfiguration" : "FAIL: ProxyConfigurationJsonConverter + ManualProxyConfiguration");
    if (!pass) failures++;
}
catch (Exception ex)
{
    Console.WriteLine($"FAIL: ProxyConfigurationJsonConverter + ManualProxyConfiguration — {ex.GetType().Name}: {ex.Message}");
    failures++;
}

// Test 3: ScriptTargetJsonConverter with RealmTarget and ContextTarget
try
{
    var realmParams = new EvaluateCommandParameters("1+1", new RealmTarget("realm-1"), true);
    var command3a = new Command(3, realmParams);
    byte[] json3a = JsonSerializer.SerializeToUtf8Bytes(command3a, options);
    using var doc3a = JsonDocument.Parse(json3a);
    var target3a = doc3a.RootElement.GetProperty("params").GetProperty("target");

    bool passRealm = target3a.GetProperty("realm").GetString() == "realm-1"
        && !target3a.TryGetProperty("context", out _);

    var contextParams = new EvaluateCommandParameters("2+2", new ContextTarget("ctx-2"), false);
    var command3b = new Command(4, contextParams);
    byte[] json3b = JsonSerializer.SerializeToUtf8Bytes(command3b, options);
    using var doc3b = JsonDocument.Parse(json3b);
    var target3b = doc3b.RootElement.GetProperty("params").GetProperty("target");

    bool passContext = target3b.GetProperty("context").GetString() == "ctx-2"
        && !target3b.TryGetProperty("realm", out _);

    bool pass = passRealm && passContext;
    Console.WriteLine(pass ? "PASS: ScriptTargetJsonConverter + RealmTarget/ContextTarget" : "FAIL: ScriptTargetJsonConverter + RealmTarget/ContextTarget");
    if (!pass) failures++;
}
catch (Exception ex)
{
    Console.WriteLine($"FAIL: ScriptTargetJsonConverter + RealmTarget/ContextTarget — {ex.GetType().Name}: {ex.Message}");
    failures++;
}

// Test 4: ConditionalNullPropertyJsonConverter<Viewport>
try
{
    var viewportParams = new SetViewportCommandParameters
    {
        BrowsingContextId = "ctx-3",
        Viewport = new Viewport { Width = 1024, Height = 768 },
    };
    var command = new Command(5, viewportParams);
    byte[] json = JsonSerializer.SerializeToUtf8Bytes(command, options);
    using var doc = JsonDocument.Parse(json);
    var vp = doc.RootElement.GetProperty("params").GetProperty("viewport");

    bool pass = vp.GetProperty("width").GetUInt64() == 1024
        && vp.GetProperty("height").GetUInt64() == 768;

    Console.WriteLine(pass ? "PASS: ConditionalNullPropertyJsonConverter<Viewport>" : "FAIL: ConditionalNullPropertyJsonConverter<Viewport>");
    if (!pass) failures++;
}
catch (Exception ex)
{
    Console.WriteLine($"FAIL: ConditionalNullPropertyJsonConverter<Viewport> — {ex.GetType().Name}: {ex.Message}");
    failures++;
}

if (failures == 0)
{
    Console.WriteLine("ALL TESTS PASSED");
}
else
{
    Console.WriteLine($"{failures} TEST(S) FAILED");
}

return failures == 0 ? 0 : 1;
