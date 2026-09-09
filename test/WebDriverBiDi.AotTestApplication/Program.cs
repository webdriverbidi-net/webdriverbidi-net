// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

string browser = args.Length > 0 ? args[0].ToLowerInvariant() : "firefox";
string url = args.Length > 1 ? args[1].ToLowerInvariant() : "https://github.com";
string browserExecutable = args.Length > 2 ? args[2] : string.Empty;
Console.WriteLine($"Browser: {browser}");

BrowserLauncher launcher;
switch (browser)
{
    case "firefox":
        BrowserLauncherBuilder firefoxBuilder = BrowserLauncher.Configure(BrowserKind.Firefox)
            .WithReleaseChannel(BrowserReleaseChannel.Alpha)
            .WithHeadlessOption();
        if (string.IsNullOrEmpty(browserExecutable))
        {
            firefoxBuilder.AtAutomaticallyDownloadedLocation();
        }
        else
        {
            firefoxBuilder.AtLocation(browserExecutable);
        }
        launcher = firefoxBuilder.Build();
        break;
    case "chrome":
        BrowserLauncherBuilder chromeBuilder = BrowserLauncher.Configure(BrowserKind.Chrome)
            .WithReleaseChannel(BrowserReleaseChannel.Alpha)
            .WithHeadlessOption();
        if (string.IsNullOrEmpty(browserExecutable))
        {
            chromeBuilder.AtAutomaticallyDownloadedLocation();
        }
        else
        {
            chromeBuilder.AtLocation(browserExecutable);
        }
        launcher = chromeBuilder.Build();
        break;
    default:
        Console.Error.WriteLine($"Unknown browser: {browser}. Use 'firefox' or 'chrome'.");
        return 1;
}

BiDiDriver? driver = null;
try
{
    await launcher.StartAsync();
    await launcher.LaunchBrowserAsync();
    Console.WriteLine("Browser launched.");

    Transport transport = launcher.CreateTransport();
    driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
    EventObserver<NavigationEventArgs> observer = driver.BrowsingContext.OnLoad.AddObserver((e) => Console.WriteLine($"Load event fired for {e.Url}"));

    // A consumer-defined command type is unknown to the library's source-generated context.
    // Registering our own context is the documented AOT pattern; without it, the command
    // below fails with "JsonTypeInfo metadata for type ... was not provided".
    await driver.RegisterTypeInfoResolverAsync(AotTestJsonContext.Default);
    await driver.StartAsync(launcher.WebSocketUrl);
    Console.WriteLine("BiDi connection established.");

    await driver.Session.NewSessionAsync(new NewCommandParameters());
    Console.WriteLine("Session created.");

    // log.entryAdded is subscribed alongside the load event so that the run below can deserialize a
    // log.LogLevel. That is the one enum in the library reached only through EnumValueJsonConverter<T>
    // and its StringEnumValueConverter<T>, whose constructor enumerates the enum's members with
    // Enum.GetValues<T>() and reads their attributes. Native AOT is the only configuration in which
    // that could fail, and this application is the only place the suite runs natively compiled code.
    EntryAddedEventArgs? capturedLogEntry = null;
    EventObserver<EntryAddedEventArgs> logObserver = driver.Log.OnEntryAdded.AddObserver((e) => capturedLogEntry = e);

    await driver.Session.SubscribeAsync(new SubscribeCommandParameters([driver.BrowsingContext.OnLoad.EventName, driver.Log.OnEntryAdded.EventName]));

    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
    if (tree.ContextTree.Count == 0)
    {
        throw new InvalidOperationException("No browsing contexts found.");
    }

    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Browsing context: {contextId}");

    observer.StartCapturingTasks();
    NavigateCommandParameters navigateParams = new(contextId, url)
    {
        Wait = ReadinessState.Complete
    };
    await driver.BrowsingContext.NavigateAsync(navigateParams);
    Console.WriteLine($"Navigation to {url} complete.");
    await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(1));

    EvaluateCommandParameters evalParams = new("document.title", new ContextTarget(contextId), true);
    EvaluateResult evalResult = await driver.Script.EvaluateAsync(evalParams);

    if (evalResult is not EvaluateResultSuccess success)
    {
        throw new InvalidOperationException($"Script evaluation failed: result type was {evalResult.ResultType}");
    }

    string? title = success.Result.As<StringRemoteValue>().Value;
    Console.WriteLine($"Page title: {title}");

    if (title is null || !title.Contains("WebDriverBiDi.NET", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Expected page title to contain 'WebDriverBiDi.NET', but got: '{title}'");
    }

    // Call a function with arguments — this exercises the CallFunctionCommandParameters
    // serialization path, which resolves the ArgumentValue polymorphic type hierarchy
    // including RemoteReference subtypes (RemoteObjectReference, SharedReference).
    CallFunctionCommandParameters callParams = new("(name) => `Hello, ${name}!`", new ContextTarget(contextId), true);
    callParams.Arguments.Add(LocalValue.String("World"));
    EvaluateResult callResult = await driver.Script.CallFunctionAsync(callParams);

    if (callResult is not EvaluateResultSuccess callSuccess)
    {
        throw new InvalidOperationException($"CallFunction failed: result type was {callResult.ResultType}");
    }

    string? greeting = callSuccess.Result.As<StringRemoteValue>().Value;
    Console.WriteLine($"CallFunction result: {greeting}");

    if (greeting != "Hello, World!")
    {
        throw new InvalidOperationException($"Expected 'Hello, World!' but got: '{greeting}'");
    }

    // A custom command routed through the registered resolver: session.status re-declared
    // with consumer-defined parameter and result types.
    AotStatusCommandResult status = await driver.ExecuteCommandAsync(new AotStatusCommandParameters());
    Console.WriteLine($"Custom status command: ready={status.IsReady}, message='{status.Message}'");
    if (status.Message is null)
    {
        throw new InvalidOperationException("Custom status command returned no message.");
    }

    // LocalValue.Number(decimal) must serialize under the source-generated context (CQ-3).
    CallFunctionCommandParameters decimalParams = new("(n) => n * 2", new ContextTarget(contextId), true);
    decimalParams.Arguments.Add(LocalValue.Number(2.5m));
    EvaluateResult decimalResult = await driver.Script.CallFunctionAsync(decimalParams);
    if (decimalResult is not EvaluateResultSuccess decimalSuccess)
    {
        throw new InvalidOperationException($"Decimal callFunction failed: result type was {decimalResult.ResultType}");
    }

    double doubled = decimalSuccess.Result.As<NumberRemoteValue>().Value;
    Console.WriteLine($"Decimal argument doubled: {doubled}");
    if (doubled != 5)
    {
        throw new InvalidOperationException($"Expected 5 but got {doubled}");
    }

    // Array and map remote values exercise the nested RemoteValue deserialization paths.
    EvaluateCommandParameters arrayParams = new("[1, 'two', true]", new ContextTarget(contextId), true);
    EvaluateResult arrayResult = await driver.Script.EvaluateAsync(arrayParams);
    if (arrayResult is not EvaluateResultSuccess arraySuccess)
    {
        throw new InvalidOperationException($"Array evaluate failed: result type was {arrayResult.ResultType}");
    }

    RemoteValueList array = arraySuccess.Result.As<CollectionRemoteValue>().Value
        ?? throw new InvalidOperationException("Array remote value had no value.");
    Console.WriteLine($"Array remote value with {array.Count} entries");
    if (array.Count != 3 || array[0].As<NumberRemoteValue>().Value != 1 || array[1].As<StringRemoteValue>().Value != "two" || !array[2].As<BooleanRemoteValue>().Value)
    {
        throw new InvalidOperationException("Array remote value did not round-trip.");
    }

    EvaluateCommandParameters mapParams = new("new Map([['answer', 42]])", new ContextTarget(contextId), true);
    EvaluateResult mapResult = await driver.Script.EvaluateAsync(mapParams);
    if (mapResult is not EvaluateResultSuccess mapSuccess)
    {
        throw new InvalidOperationException($"Map evaluate failed: result type was {mapResult.ResultType}");
    }

    RemoteValueDictionary map = mapSuccess.Result.As<KeyValuePairCollectionRemoteValue>().Value
        ?? throw new InvalidOperationException("Map remote value had no value.");
    Console.WriteLine($"Map remote value with {map.Count} entries");
    if (map.Count != 1 || map["answer"].As<NumberRemoteValue>().Value != 42)
    {
        throw new InvalidOperationException("Map remote value did not round-trip.");
    }

    // The polymorphic argument kinds. LocalArgumentValue.SerializableValue is object-typed, so the
    // source-generated context cannot see through it from the static type alone: these are the shapes
    // most at risk from trimming, and none of them was exercised here before. Sending them all in one
    // call means a single round trip covers map, object, regexp, date, bigint, array and set.
    CallFunctionCommandParameters polymorphicParams = new(
        "(map, obj, pattern, when, big, list, unique) => "
            + "`${map.get('k')}|${obj.k}|${pattern.source}|${when.getUTCFullYear()}|${big}|${list.length}|${unique.size}`",
        new ContextTarget(contextId),
        true);
    polymorphicParams.Arguments.Add(LocalValue.Map(new Dictionary<string, LocalValue> { ["k"] = LocalValue.String("mapValue") }));
    polymorphicParams.Arguments.Add(LocalValue.Object(new Dictionary<string, LocalValue> { ["k"] = LocalValue.String("objectValue") }));
    polymorphicParams.Arguments.Add(LocalValue.RegExp("ab+c", "i"));
    polymorphicParams.Arguments.Add(LocalValue.Date(new DateTime(2026, 9, 4, 0, 0, 0, DateTimeKind.Utc)));
    polymorphicParams.Arguments.Add(LocalValue.BigInt(BigInteger.Parse("9007199254740993", CultureInfo.InvariantCulture)));
    polymorphicParams.Arguments.Add(LocalValue.Array([LocalValue.Number(1), LocalValue.Number(2)]));
    polymorphicParams.Arguments.Add(LocalValue.Set([LocalValue.Number(1), LocalValue.Number(1), LocalValue.Number(2)]));

    EvaluateResult polymorphicResult = await driver.Script.CallFunctionAsync(polymorphicParams);
    if (polymorphicResult is not EvaluateResultSuccess polymorphicSuccess)
    {
        throw new InvalidOperationException($"Polymorphic callFunction failed: result type was {polymorphicResult.ResultType}");
    }

    string? polymorphic = polymorphicSuccess.Result.As<StringRemoteValue>().Value;
    Console.WriteLine($"Polymorphic arguments round-tripped: {polymorphic}");
    if (polymorphic != "mapValue|objectValue|ab+c|2026|9007199254740993|2|2")
    {
        throw new InvalidOperationException($"Polymorphic arguments did not round-trip; got '{polymorphic}'");
    }

    // PointerMoveAction.SerializableOrigin is object-typed for the same reason, and input.performActions
    // is the only command that reaches it.
    PerformActionsCommandParameters actionsParams = new(contextId);
    PointerSourceActions pointerActions = new("aot-pointer");
    pointerActions.Actions.Add(new PointerMoveAction { X = 5, Y = 7, Origin = Origin.Viewport });
    actionsParams.Actions.Add(pointerActions);
    await driver.Input.PerformActionsAsync(actionsParams);
    Console.WriteLine("performActions with a pointer move accepted.");

    // A script exception deserializes into EvaluateResultException with its details.
    EvaluateCommandParameters throwParams = new("(() => { throw new Error('boom'); })()", new ContextTarget(contextId), true);
    EvaluateResult throwResult = await driver.Script.EvaluateAsync(throwParams);
    if (throwResult is not EvaluateResultException scriptException || !scriptException.ExceptionDetails.Text.Contains("boom"))
    {
        throw new InvalidOperationException($"Expected a script exception mentioning 'boom' but got result type {throwResult.ResultType}");
    }

    Console.WriteLine($"Script exception captured: {scriptException.ExceptionDetails.Text}");

    // A console call raises log.entryAdded, whose level is a log.LogLevel. The specification maps the
    // "warn" console method to the "warn" level, so the expected value does not depend on the browser.
    logObserver.StartCapturingTasks();
    EvaluateCommandParameters consoleParams = new("console.warn('aot probe')", new ContextTarget(contextId), true);
    await driver.Script.EvaluateAsync(consoleParams);
    await logObserver.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromSeconds(10));

    if (capturedLogEntry is null)
    {
        throw new InvalidOperationException("No log.entryAdded event was received for the console call.");
    }

    Console.WriteLine($"Log entry captured: level={capturedLogEntry.Level}, text='{capturedLogEntry.Text}'");
    if (capturedLogEntry.Level != LogLevel.Warn)
    {
        throw new InvalidOperationException($"Expected LogLevel.Warn but got {capturedLogEntry.Level}");
    }

    logObserver.Dispose();

    // An error response deserializes into WebDriverBiDiCommandException with its error code.
    try
    {
        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("no-such-context", url));
        throw new InvalidOperationException("Navigating a nonexistent context should have failed.");
    }
    catch (WebDriverBiDiCommandException commandException)
    {
        Console.WriteLine($"Error response captured: {commandException.ErrorCode} - {commandException.ProtocolErrorMessage}");
        if (commandException.ErrorCode != ErrorCode.NoSuchFrame)
        {
            throw new InvalidOperationException($"Expected ErrorCode.NoSuchFrame but got {commandException.ErrorCode}");
        }
    }

    Console.WriteLine($"PASS: Integration test succeeded — connected to {browser}, navigated to web page, verified page title, callFunction, a custom command through a registered resolver, decimal/array/map values, the polymorphic argument kinds, input.performActions, a log entry level, a script exception and an error response.");
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
            // Best effort cleanup
        }
    }

    try
    {
        await launcher.QuitBrowserAsync();
    }
    catch
    {
        // Best effort cleanup
    }
}

/// <summary>
/// A consumer-defined command: session.status re-declared with the consumer's own types.
/// </summary>
public class AotStatusCommandParameters : CommandParameters<AotStatusCommandResult>
{
    [JsonIgnore]
    public override string MethodName => "session.status";
}

/// <summary>
/// The consumer-defined result of the command.
/// </summary>
public record AotStatusCommandResult : CommandResult
{
    [JsonIgnore]
    public override bool IsError => false;

    [JsonPropertyName("ready")]
    public bool IsReady { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// The consumer's source-generated context: just the parameters and result types, as the
/// AOT compatibility guide prescribes. The response envelope is handled by the library.
/// </summary>
[JsonSerializable(typeof(AotStatusCommandParameters))]
[JsonSerializable(typeof(AotStatusCommandResult))]
internal partial class AotTestJsonContext : JsonSerializerContext
{
}
