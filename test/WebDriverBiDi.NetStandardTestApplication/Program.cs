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
    Console.Error.WriteLine("Usage: WebDriverBiDi.NetStandardTestApplication <websocket-url>");
    return 1;
}

string webSocketUrl = args[0];

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
    // extension path (used internally by WaitForCapturedTasksAsync below).
    EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(e => Console.WriteLine($"Log entry received: {e.Text}"));

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

    await driver.Session.EndAsync();
    Console.WriteLine("PASS: netstandard2.0 build of WebDriverBiDi connected, exchanged commands, and received an event.");
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
