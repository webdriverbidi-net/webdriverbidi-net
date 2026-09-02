// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Reflection;
using System.Runtime.Versioning;
using WebDriverBiDi;
using WebDriverBiDi.Log;
using WebDriverBiDi.Session;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: WebDriverBiDi.NetStandardTestApplication <websocket-url> [pipe-peer-dll-path]");
    return 1;
}

string webSocketUrl = args[0];

// Optional: when supplied, the path to the NamedPipeTestApplication is used to run a PipeConnection
// round trip that exercises the netstandard2.0 pipe code paths in addition to the WebSocket ones.
string? pipePeerDllPath = args.Length > 1 ? args[1] : null;

// Defense-in-depth: confirm this process actually loaded the netstandard2.0 build of
// WebDriverBiDi. The SetTargetFramework metadata on this project's ProjectReference
// pins that selection at build time; this check catches a silent regression (e.g.,
// someone removing that metadata, or a future SDK change altering resolution) that
// would otherwise make the rest of this smoke test validate nothing.
TargetFrameworkAttribute? targetFrameworkAttribute = typeof(BiDiDriver).Assembly.GetCustomAttribute<TargetFrameworkAttribute>();
string? frameworkName = targetFrameworkAttribute?.FrameworkName;
Console.WriteLine($"WebDriverBiDi assembly target framework: {frameworkName}");
if (frameworkName is null || !frameworkName.StartsWith(".NETStandard,Version=v2.0", StringComparison.Ordinal))
{
    Console.Error.WriteLine($"FAIL: expected to load the netstandard2.0 build of WebDriverBiDi, but loaded '{frameworkName}'.");
    return 1;
}

BiDiDriver? driver = null;
try
{
    driver = new BiDiDriver(TimeSpan.FromSeconds(10));

    // Exercises EventObserver<T>'s netstandard2.0-specific CancellationTokenSource/TimeProvider
    // extension path (used internally by WaitForCapturedTasksAsync below). The event args are
    // captured so that the extension-data conversion path can be validated after the wait.
    EntryAddedEventArgs? receivedEntry = null;
    EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e =>
    {
        receivedEntry = e;
        Console.WriteLine($"Log entry received: {e.Text}");
    });

    await driver.StartAsync(webSocketUrl);
    Console.WriteLine("Connected.");

    // The no-argument overload exercises the optional-parameters path added for
    // Session.NewSessionAsync; the round trip through Transport/WebSocketConnection
    // exercises the JSON serialization and UTF8 encoding paths that also differ under
    // netstandard2.0 (Encoding.GetString(byte[]) vs. the Span-based overload).
    NewCommandResult session = await driver.Session.NewSessionAsync();
    Console.WriteLine($"Session created: {session.SessionId}");

    observer.StartCapturingTasks();
    await driver.Session.SubscribeAsync(new SubscribeCommandParameters([driver.Log.OnEntryAdded.EventName]));

    Task[] capturedTasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
    if (capturedTasks.Length != 1)
    {
        throw new InvalidOperationException("Did not receive the expected log entry event within the timeout.");
    }

    // Exercise the netstandard2.0 numeric extension-data conversion path in
    // JsonConverterUtilities: the scripted event carries a fractional extension
    // property and two whose magnitudes exceed the range of double. Modern runtimes
    // round the over-range values to signed infinity when parsing; the netstandard2.0
    // build contains an explicit fallback that produces the same signed infinities
    // when running on .NET Framework, where that parse fails instead. The expected
    // values are therefore identical on every runtime this application can run on.
    if (receivedEntry is null)
    {
        throw new InvalidOperationException("The log entry event args were not captured.");
    }

    object? fractionalValue = receivedEntry.AdditionalData["compatFractionalExtra"];
    if (fractionalValue is not double fractional || fractional != 1.5)
    {
        throw new InvalidOperationException($"Expected fractional extension datum to convert to 1.5, but was '{fractionalValue}'.");
    }

    object? overflowValue = receivedEntry.AdditionalData["compatOverflowExtra"];
    if (overflowValue is not double overflow || !double.IsPositiveInfinity(overflow))
    {
        throw new InvalidOperationException($"Expected overflowing extension datum to convert to positive infinity, but was '{overflowValue}'.");
    }

    object? negativeOverflowValue = receivedEntry.AdditionalData["compatNegativeOverflowExtra"];
    if (negativeOverflowValue is not double negativeOverflow || !double.IsNegativeInfinity(negativeOverflow))
    {
        throw new InvalidOperationException($"Expected negative overflowing extension datum to convert to negative infinity, but was '{negativeOverflowValue}'.");
    }

    Console.WriteLine("Numeric extension data converted as expected.");

    // Exercise the command-failure path: the scripted server replies to session.status with a BiDi
    // error response, which must surface as a WebDriverBiDiCommandException after the netstandard2.0
    // error-message deserialization path (UTF8 decoding and the error-response DTO mapping). This also
    // exercises the no-argument optional-parameters overload of a result-returning command.
    try
    {
        await driver.Session.StatusAsync();
        throw new InvalidOperationException("Expected the failing session.status command to throw a WebDriverBiDiCommandException.");
    }
    catch (WebDriverBiDiCommandException ex) when (ex.Message.IndexOf("simulated command failure", StringComparison.Ordinal) >= 0)
    {
        Console.WriteLine($"Command failure surfaced as WebDriverBiDiCommandException as expected: {ex.Message}");
    }

    await driver.Session.EndAsync();

    // Exercise the netstandard2.0 pipe transport as well, when the pipe-peer application path was
    // supplied. This runs a PipeConnection round trip against the NamedPipeTestApplication, covering
    // the netstandard2.0-specific (#else) branches of PipeConnection's send and receive paths that the
    // WebSocket flow above does not reach.
    if (pipePeerDllPath is not null)
    {
        await PipeTransportScenario.RunAsync(pipePeerDllPath);
        Console.WriteLine("Pipe transport round-trip over the netstandard2.0 build succeeded.");
    }

    Console.WriteLine("PASS: netstandard2.0 build of WebDriverBiDi connected, exchanged commands, received an event, and surfaced a command failure.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}
finally
{
    if (driver is not null)
    {
        try
        {
            await driver.StopAsync();
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
