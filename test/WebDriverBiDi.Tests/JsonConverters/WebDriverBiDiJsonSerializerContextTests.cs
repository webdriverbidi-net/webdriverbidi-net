namespace WebDriverBiDi.JsonConverters;

using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.DigitalCredentials;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

public class WebDriverBiDiJsonSerializerContextTests
{
    private const string PointerMoveOriginMember = "PointerMoveAction.SerializableOrigin";
    private const string WheelScrollOriginMember = "WheelScrollAction.SerializableOrigin";
    private const string LocalArgumentValueMember = "LocalArgumentValue.SerializableValue";
    private const string PrintPageRangesMember = "PrintCommandParameters.SerializablePageRanges";
    private const string VirtualWalletResponseMember = "SetVirtualWalletBehaviorCommandParameters.Response";

    // Every shape the library sends whose serialization the source-generated context cannot settle from declared
    // types alone: a member typed as, or holding, object is written according to the runtime type of its value,
    // and that type needs metadata of its own. Polymorphic hierarchies are included for the same reason. Each
    // case names the object-carrying members it exercises, so that the guard test below can require one.
    private static readonly Dictionary<string, ContextSerializationCase> ContextSerializationCases = new()
    {
        ["css locator"] = LocateNodesCase(new CssLocator("div.selector")),
        ["xpath locator"] = LocateNodesCase(new XPathLocator("//div")),
        ["innerText locator"] = LocateNodesCase(new InnerTextLocator("text")),
        ["innerText locator with options"] = LocateNodesCase(new InnerTextLocator("text") { IgnoreCase = true, MatchType = InnerTextMatchType.Partial, MaxDepth = 2 }),
        ["accessibility locator"] = LocateNodesCase(new AccessibilityLocator { Name = "accessibleName", Role = "button" }),
        ["accessibility locator without attributes"] = LocateNodesCase(new AccessibilityLocator()),
        ["context locator"] = LocateNodesCase(new ContextLocator("childContext")),
        ["pointer move without origin"] = PointerMoveCase(null),
        ["pointer move from viewport"] = PointerMoveCase(Origin.Viewport),
        ["pointer move from pointer"] = PointerMoveCase(Origin.Pointer),
        ["pointer move from element"] = PointerMoveCase(Origin.Element(new SharedReference("elementId"))),
        ["wheel scroll without origin"] = WheelScrollCase(null),
        ["wheel scroll from viewport"] = WheelScrollCase(Origin.Viewport),
        ["wheel scroll from pointer"] = WheelScrollCase(Origin.Pointer),
        ["wheel scroll from element"] = WheelScrollCase(Origin.Element(new ElementOrigin(new SharedReference("elementId") { Handle = "handle" }))),
        ["undefined argument"] = ArgumentCase(LocalValue.Undefined),
        ["null argument"] = ArgumentCase(LocalValue.Null),
        ["NaN argument"] = ArgumentCase(LocalValue.NaN),
        ["negative zero argument"] = ArgumentCase(LocalValue.NegativeZero),
        ["infinity argument"] = ArgumentCase(LocalValue.Infinity),
        ["negative infinity argument"] = ArgumentCase(LocalValue.NegativeInfinity),
        ["string argument"] = ArgumentCase(LocalValue.String("value")),
        ["int argument"] = ArgumentCase(LocalValue.Number(42)),
        ["long argument"] = ArgumentCase(LocalValue.Number(9007199254740991L)),
        ["double argument"] = ArgumentCase(LocalValue.Number(3.5)),
        ["decimal argument"] = ArgumentCase(LocalValue.Number(2.5m)),
        ["boolean argument"] = ArgumentCase(LocalValue.Boolean(true)),
        ["bigint argument"] = ArgumentCase(LocalValue.BigInt(BigInteger.Parse("9007199254740993", CultureInfo.InvariantCulture))),
        ["date argument"] = ArgumentCase(LocalValue.Date(new DateTime(2026, 9, 15, 1, 2, 3, DateTimeKind.Utc))),
        ["array argument"] = ArgumentCase(LocalValue.Array([LocalValue.Number(1), LocalValue.String("two")])),
        ["set argument"] = ArgumentCase(LocalValue.Set([LocalValue.Boolean(false)])),
        ["map argument with string keys"] = ArgumentCase(LocalValue.Map(new Dictionary<string, LocalValue> { ["key"] = LocalValue.String("value") })),
        ["map argument with LocalValue keys"] = ArgumentCase(LocalValue.Map(new Dictionary<LocalValue, LocalValue> { [LocalValue.Number(1)] = LocalValue.String("value") })),
        ["map argument with mixed keys"] = ArgumentCase(LocalValue.Map(new Dictionary<object, LocalValue> { ["key"] = LocalValue.Null, [LocalValue.Boolean(true)] = LocalValue.Undefined })),
        ["object argument with string keys"] = ArgumentCase(LocalValue.Object(new Dictionary<string, LocalValue> { ["key"] = LocalValue.Number(1.5) })),
        ["object argument with LocalValue keys"] = ArgumentCase(LocalValue.Object(new Dictionary<LocalValue, LocalValue> { [LocalValue.String("key")] = LocalValue.Number(1) })),
        ["object argument with mixed keys"] = ArgumentCase(LocalValue.Object(new Dictionary<object, LocalValue> { ["key"] = LocalValue.Array([]), [LocalValue.String("other")] = LocalValue.Set([]) })),
        ["nested argument"] = ArgumentCase(LocalValue.Array([LocalValue.Map(new Dictionary<string, LocalValue> { ["inner"] = LocalValue.Object(new Dictionary<string, LocalValue> { ["deep"] = LocalValue.Date(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) }) })])),
        ["regexp argument"] = ArgumentCase(LocalValue.RegExp("ab+c", "gi")),
        ["regexp argument without flags"] = ArgumentCase(LocalValue.RegExp("ab+c")),
        ["channel argument"] = new(typeof(CallFunctionCommandParameters), () => CreateCallFunctionParameters(new ChannelValue(new ChannelProperties("channelId") { Ownership = ResultOwnership.Root })), []),
        ["shared reference argument"] = new(typeof(CallFunctionCommandParameters), () => CreateCallFunctionParameters(new SharedReference("sharedId") { Handle = "handle" }), []),
        ["remote object reference argument"] = new(typeof(CallFunctionCommandParameters), () => CreateCallFunctionParameters(new RemoteObjectReference("handle") { SharedId = "sharedId" }), []),
        ["print without page ranges"] = PrintCase(),
        ["print with numeric and string page ranges"] = PrintCase(1UL, "3-5", 8UL),
        ["virtual wallet response"] = new(
            typeof(SetVirtualWalletBehaviorCommandParameters),
            () => new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Respond)
            {
                Protocol = "openid4vp-v1-unsigned",
                Response = new Dictionary<string, object?>
                {
                    ["text"] = "value",
                    ["flag"] = true,
                    ["integer"] = 7,
                    ["long"] = 9007199254740991L,
                    ["number"] = 1.25,
                    ["missing"] = null,
                    ["list"] = new List<object?> { "item", 2, null },
                    ["nested"] = new Dictionary<string, object?> { ["inner"] = "value" },
                },
            },
            [VirtualWalletResponseMember]),
    };

    public static TheoryData<string> ContextSerializationCaseNames => [.. ContextSerializationCases.Keys];

    // Every shape the library receives whose deserialization the source-generated context cannot settle from
    // declared types alone: a payload whose runtime type is chosen by a discriminator (RemoteValue and its
    // 26 productions, EvaluateResult) or that is read through a converter of the library's own. Serializing
    // these shapes proves nothing about receiving them — the source generator emits separate read and write
    // metadata, and an application published with native AOT reads every message through this context.
    private static readonly Dictionary<string, ContextDeserializationCase> ContextDeserializationCases = new()
    {
        ["undefined result"] = EvaluateResultCase("""{"type":"undefined"}"""),
        ["null result"] = EvaluateResultCase("""{"type":"null"}"""),
        ["string result"] = EvaluateResultCase("""{"type":"string","value":"myStringValue"}"""),
        ["number result"] = EvaluateResultCase("""{"type":"number","value":42}"""),
        ["fractional number result"] = EvaluateResultCase("""{"type":"number","value":3.25}"""),
        ["NaN result"] = EvaluateResultCase("""{"type":"number","value":"NaN"}"""),
        ["negative zero result"] = EvaluateResultCase("""{"type":"number","value":"-0"}"""),
        ["infinity result"] = EvaluateResultCase("""{"type":"number","value":"Infinity"}"""),
        ["negative infinity result"] = EvaluateResultCase("""{"type":"number","value":"-Infinity"}"""),
        ["boolean result"] = EvaluateResultCase("""{"type":"boolean","value":true}"""),
        ["bigint result"] = EvaluateResultCase("""{"type":"bigint","value":"9007199254740993"}"""),
        ["date result"] = EvaluateResultCase("""{"type":"date","value":"2026-09-15T01:02:03.000Z"}"""),
        ["symbol result"] = EvaluateResultCase("""{"type":"symbol","handle":"myHandle"}"""),
        ["function result"] = EvaluateResultCase("""{"type":"function","handle":"myHandle","internalId":"1"}"""),
        ["regexp result"] = EvaluateResultCase("""{"type":"regexp","value":{"pattern":"ab+c","flags":"gi"}}"""),
        ["regexp result without flags"] = EvaluateResultCase("""{"type":"regexp","value":{"pattern":"ab+c"}}"""),
        ["array result"] = EvaluateResultCase("""{"type":"array","value":[{"type":"number","value":1},{"type":"string","value":"two"}]}"""),
        ["nested array result"] = EvaluateResultCase("""{"type":"array","value":[{"type":"array","value":[{"type":"date","value":"2026-01-01T00:00:00.000Z"}]}]}"""),
        ["set result"] = EvaluateResultCase("""{"type":"set","value":[{"type":"boolean","value":false}]}"""),
        ["object result"] = EvaluateResultCase("""{"type":"object","value":[["name",{"type":"string","value":"John"}],["age",{"type":"number","value":30}]]}"""),
        ["map result with remote value keys"] = EvaluateResultCase("""{"type":"map","value":[[{"type":"number","value":1},{"type":"string","value":"one"}],["key",{"type":"null"}]]}"""),
        ["object result without contents"] = EvaluateResultCase("""{"type":"object","internalId":"7"}"""),
        ["error result"] = EvaluateResultCase("""{"type":"error","handle":"myHandle"}"""),
        ["promise result"] = EvaluateResultCase("""{"type":"promise"}"""),
        ["proxy result"] = EvaluateResultCase("""{"type":"proxy","handle":"myHandle"}"""),
        ["generator result"] = EvaluateResultCase("""{"type":"generator"}"""),
        ["weakmap result"] = EvaluateResultCase("""{"type":"weakmap"}"""),
        ["weakset result"] = EvaluateResultCase("""{"type":"weakset"}"""),
        ["typedarray result"] = EvaluateResultCase("""{"type":"typedarray"}"""),
        ["arraybuffer result"] = EvaluateResultCase("""{"type":"arraybuffer"}"""),
        ["window result"] = EvaluateResultCase("""{"type":"window","value":{"context":"myContextId"},"handle":"myHandle","internalId":"3"}"""),
        ["node result"] = EvaluateResultCase(
            """
            {"type":"node","sharedId":"mySharedId","value":{"nodeType":1,"nodeValue":"","childNodeCount":1,"localName":"div","namespaceURI":"http://www.w3.org/1999/xhtml","attributes":{"id":"myId","class":"myClass"},"children":[{"type":"node","sharedId":"myChildSharedId","value":{"nodeType":3,"nodeValue":"text","childNodeCount":0}}],"shadowRoot":null}}
            """),
        ["node result without contents"] = EvaluateResultCase("""{"type":"node","sharedId":"mySharedId"}"""),
        ["nodelist result"] = EvaluateResultCase("""{"type":"nodelist","value":[{"type":"node","sharedId":"mySharedId","value":{"nodeType":1,"nodeValue":"","childNodeCount":0}}]}"""),
        ["htmlcollection result"] = EvaluateResultCase("""{"type":"htmlcollection","value":[{"type":"node","sharedId":"mySharedId","value":{"nodeType":1,"nodeValue":"","childNodeCount":0}}]}"""),
        ["script exception"] = new(
            typeof(CommandResponseMessage<EvaluateResult>),
            """
            {"type":"success","id":1,"result":{"type":"exception","realm":"myRealmId","exceptionDetails":{"text":"kaboom","lineNumber":5,"columnNumber":9,"stackTrace":{"callFrames":[{"functionName":"myFunction","url":"http://example.com/script.js","lineNumber":5,"columnNumber":9}]},"exception":{"type":"error","handle":"myHandle"}}}}
            """),
        ["new session capabilities"] = new(
            typeof(CommandResponseMessage<NewCommandResult>),
            """
            {"type":"success","id":1,"result":{"sessionId":"mySessionId","capabilities":{"acceptInsecureCerts":true,"browserName":"myBrowser","browserVersion":"1.0","platformName":"myPlatform","setWindowRect":false,"userAgent":"myUserAgent","webSocketUrl":"ws://localhost:5555","proxy":{"proxyType":"manual","httpProxy":"http://proxy.example.com:8080","noProxy":["localhost"]},"unhandledPromptBehavior":{"default":"dismiss"},"goog:extension":{"nested":true}}}}
            """),
        ["get cookies result"] = new(
            typeof(CommandResponseMessage<GetCookiesCommandResult>),
            """
            {"type":"success","id":1,"result":{"cookies":[{"name":"myCookie","value":{"type":"string","value":"myValue"},"domain":"example.com","path":"/","size":16,"httpOnly":false,"secure":true,"sameSite":"lax","expiry":1735689600,"goog:priority":"high"}],"partitionKey":{"userContext":"myUserContext"}}}
            """),
        ["locate nodes result"] = new(
            typeof(CommandResponseMessage<LocateNodesCommandResult>),
            """
            {"type":"success","id":1,"result":{"nodes":[{"type":"node","sharedId":"mySharedId","value":{"nodeType":1,"nodeValue":"","childNodeCount":0,"localName":"button","attributes":{"type":"submit"}}}]}}
            """),
        ["before request sent event"] = new(
            typeof(EventMessage<BeforeRequestSentEventArgs>),
            """
            {"type":"event","method":"network.beforeRequestSent","params":{"context":"myContextId","navigation":"myNavigationId","redirectCount":0,"isBlocked":false,"timestamp":1735689600000,"request":{"request":"myRequestId","url":"http://example.com/","method":"GET","headers":[{"name":"Accept","value":{"type":"string","value":"text/html"}}],"cookies":[],"headersSize":42,"bodySize":0,"timings":{"timeOrigin":0,"requestTime":1,"redirectStart":0,"redirectEnd":0,"fetchStart":2,"dnsStart":3,"dnsEnd":4,"connectStart":5,"connectEnd":6,"tlsStart":7,"requestStart":8,"responseStart":9,"responseEnd":10},"destination":"document","initiatorType":null},"initiator":{"type":"other"}}}
            """),
        ["response completed event"] = new(
            typeof(EventMessage<ResponseCompletedEventArgs>),
            """
            {"type":"event","method":"network.responseCompleted","params":{"context":"myContextId","navigation":null,"redirectCount":0,"isBlocked":false,"timestamp":1735689600000,"request":{"request":"myRequestId","url":"http://example.com/","method":"GET","headers":[],"cookies":[],"headersSize":42,"bodySize":0,"timings":{"timeOrigin":0,"requestTime":1,"redirectStart":0,"redirectEnd":0,"fetchStart":2,"dnsStart":3,"dnsEnd":4,"connectStart":5,"connectEnd":6,"tlsStart":7,"requestStart":8,"responseStart":9,"responseEnd":10},"destination":"","initiatorType":null},"response":{"url":"http://example.com/","protocol":"http/1.1","status":200,"statusText":"OK","fromCache":false,"headers":[{"name":"Content-Type","value":{"type":"string","value":"text/html"}}],"mimeType":"text/html","bytesReceived":1024,"headersSize":128,"bodySize":896,"content":{"size":896},"authChallenges":[{"scheme":"Basic","realm":"myRealm"}]}}}
            """),
        ["fetch error event"] = new(
            typeof(EventMessage<FetchErrorEventArgs>),
            """
            {"type":"event","method":"network.fetchError","params":{"context":"myContextId","navigation":null,"redirectCount":0,"isBlocked":false,"timestamp":1735689600000,"request":{"request":"myRequestId","url":"http://example.com/","method":"GET","headers":[],"cookies":[],"headersSize":0,"bodySize":0,"timings":{"timeOrigin":0,"requestTime":0,"redirectStart":0,"redirectEnd":0,"fetchStart":0,"dnsStart":0,"dnsEnd":0,"connectStart":0,"connectEnd":0,"tlsStart":0,"requestStart":0,"responseStart":0,"responseEnd":0},"destination":"","initiatorType":null},"errorText":"net::ERR_CONNECTION_REFUSED"}}
            """),
        ["auth required event"] = new(
            typeof(EventMessage<AuthRequiredEventArgs>),
            """
            {"type":"event","method":"network.authRequired","params":{"context":"myContextId","navigation":null,"redirectCount":0,"isBlocked":true,"timestamp":1735689600000,"request":{"request":"myRequestId","url":"http://example.com/","method":"GET","headers":[],"cookies":[],"headersSize":0,"bodySize":0,"timings":{"timeOrigin":0,"requestTime":0,"redirectStart":0,"redirectEnd":0,"fetchStart":0,"dnsStart":0,"dnsEnd":0,"connectStart":0,"connectEnd":0,"tlsStart":0,"requestStart":0,"responseStart":0,"responseEnd":0},"destination":"","initiatorType":null},"response":{"url":"http://example.com/","protocol":"http/1.1","status":401,"statusText":"Unauthorized","fromCache":false,"headers":[],"mimeType":"text/html","bytesReceived":0,"headersSize":0,"bodySize":0,"content":{"size":0},"authChallenges":[{"scheme":"Basic","realm":"myRealm"}]}}}
            """),
        ["navigation event"] = new(
            typeof(EventMessage<NavigationEventArgs>),
            """
            {"type":"event","method":"browsingContext.load","params":{"context":"myContextId","navigation":"myNavigationId","timestamp":1735689600000,"url":"http://example.com/"}}
            """),
        // The log module deserializes a LogEntry and converts it to the event args, so LogEntry — itself a
        // discriminated union over the entry's "type" — is the received type.
        ["console log entry event"] = new(
            typeof(EventMessage<LogEntry>),
            """
            {"type":"event","method":"log.entryAdded","params":{"type":"console","level":"warn","source":{"realm":"myRealmId","context":"myContextId"},"text":"myMessage","timestamp":1735689600000,"method":"warn","args":[{"type":"string","value":"myArgument"},{"type":"number","value":"NaN"}],"stackTrace":{"callFrames":[{"functionName":"myFunction","url":"http://example.com/script.js","lineNumber":5,"columnNumber":9}]}}}
            """),
        ["window realm created event"] = new(
            typeof(EventMessage<RealmInfo>),
            """
            {"type":"event","method":"script.realmCreated","params":{"type":"window","realm":"myRealmId","origin":"http://example.com","context":"myContextId","sandbox":"mySandbox","userContext":"myUserContext"}}
            """),
        ["dedicated worker realm created event"] = new(
            typeof(EventMessage<RealmInfo>),
            """
            {"type":"event","method":"script.realmCreated","params":{"type":"dedicated-worker","realm":"myRealmId","origin":"http://example.com","owners":["myOwnerRealmId"]}}
            """),
        ["get realms result"] = new(
            typeof(CommandResponseMessage<GetRealmsCommandResult>),
            """
            {"type":"success","id":1,"result":{"realms":[{"type":"window","realm":"myRealmId","origin":"http://example.com","context":"myContextId"},{"type":"service-worker","realm":"myWorkerRealmId","origin":"http://example.com"}]}}
            """),
        ["download complete event"] = new(
            typeof(EventMessage<DownloadEndEventArgs>),
            """
            {"type":"event","method":"browsingContext.downloadEnd","params":{"status":"complete","download":"myDownloadId","context":"myContextId","navigation":"myNavigationId","timestamp":1735689600000,"url":"http://example.com/file.zip","filepath":"/tmp/file.zip"}}
            """),
        ["download canceled event"] = new(
            typeof(EventMessage<DownloadEndEventArgs>),
            """
            {"type":"event","method":"browsingContext.downloadEnd","params":{"status":"canceled","download":"myDownloadId","context":"myContextId","navigation":null,"timestamp":1735689600000,"url":"http://example.com/file.zip"}}
            """),
        ["javascript log entry event"] = new(
            typeof(EventMessage<LogEntry>),
            """
            {"type":"event","method":"log.entryAdded","params":{"type":"javascript","level":"error","source":{"realm":"myRealmId"},"text":"myError","timestamp":1735689600000,"stackTrace":{"callFrames":[]}}}
            """),
        ["generic log entry event"] = new(
            typeof(EventMessage<LogEntry>),
            """
            {"type":"event","method":"log.entryAdded","params":{"type":"myCustomType","level":"info","source":{"realm":"myRealmId"},"text":null,"timestamp":1735689600000}}
            """),
    };

    public static TheoryData<string> ContextDeserializationCaseNames => [.. ContextDeserializationCases.Keys];

    [Fact]
    public void TestAllCommandParametersAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the CommandParameters classes defined.
        // From all of the types in the assmebly, get all that are explicitly subclasses
        // of CommandParameters, are not abstract classes, and are in a namespace that
        // starts with "WebDriverBiDi.". If any type meeting this criteria are not in
        // the list of types registered with the custom JsonSerializerContext, add them
        // to the list of missing types.
        Type[] assemblyTypes = assembly.GetTypes();
        List<Type> missingTypes = [];
        foreach (Type assemblyType in assemblyTypes)
        {
            if (assemblyType.IsAssignableTo(typeof(CommandParameters)) && !assemblyType.IsAbstract &&
                assemblyType != typeof(CommandParameters) && IsLibraryNamespace(assemblyType) &&
                !registeredTypes.Contains(assemblyType))
            {
                missingTypes.Add(assemblyType);
            }
        }

        // There should be no missing types.
        Assert.True(missingTypes.Count == 0, FormatMissingMessage("command parameter", missingTypes));
    }

    [Fact]
    public void TestAllCommandResultsHaveResponseMessageInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;
        Type openResponseType = typeof(CommandResponseMessage<>);

        // Get a list of all the CommandResponse classes defined.
        // From all of the types in the assmebly, get all that are explicitly subclasses
        // of CommandParameters, are not abstract classes, and are in a namespace that
        // starts with "WebDriverBiDi.". From that type, get the type of the class's 
        // CommandResponse type and add it to the list.
        List<Type> commandResultTypes = [];
        Type[] assemblyTypes = assembly.GetTypes();
        foreach (Type assemblyType in assemblyTypes)
        {
            if (assemblyType.IsAssignableTo(typeof(CommandParameters)) && !assemblyType.IsAbstract && IsLibraryNamespace(assemblyType))
            {
                Type? commandResultType = GetCommandResultTypeFromParameters(assemblyType);
                if (commandResultType is not null && !commandResultTypes.Contains(commandResultType))
                {
                    commandResultTypes.Add(commandResultType);
                }
            }
        }

        // Given that we now have a list of CommandResponse types, iterate through that
        // list, create a CommandResponseMessage<T>. Add any of the CommandResponseMessage<T>
        // types that are not in the list of types registered with the custom JsonSerializerContext,
        // to the list of missing types.
        List<Type> missingTypes = [];
        foreach (Type commandResultType in commandResultTypes)
        {
            Type commandResponseMessageType = openResponseType.MakeGenericType(commandResultType);
            if (!registeredTypes.Contains(commandResponseMessageType))
            {
                missingTypes.Add(commandResponseMessageType);
            }
        }

        // There should be no missing types.
        Assert.True(missingTypes.Count == 0, FormatMissingMessage("command result response message", missingTypes));
    }

    [Fact]
    public void TestAllEventMessagesAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();

        // Get a list of all the EventMessage classes defined.
        // Create an instance of BiDiDriver to register all of the modules and create
        // EventMessage<T> objects for all of the registered events. The list of these
        // types is stored in the private Values list in the eventMessageTypes field of
        // the Transport class. We introspect into the object using reflection to get
        // this list, as it's not, nor should it be, exposed to consumers of the
        // Transport class.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        _ = new BiDiDriver(TimeSpan.FromSeconds(1), transport);

        FieldInfo? field = transport.GetType().GetField("eventMessageTypes", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);

        // The registry maps event names to an internal registration object exposing the message type.
        System.Collections.IDictionary? eventMessageTypes = field.GetValue(transport) as System.Collections.IDictionary;
        Assert.NotNull(eventMessageTypes);

        // Add any of the EventMessage<T> types that are not in the list of types registered with
        // the custom JsonSerializerContext to the list of missing types. 
        List<Type> missingTypes = [];
        foreach (object? registration in eventMessageTypes.Values)
        {
            Assert.NotNull(registration);
            Type? eventMessageType = registration.GetType().GetProperty("EventMessageType")?.GetValue(registration) as Type;
            Assert.NotNull(eventMessageType);
            if (!registeredTypes.Contains(eventMessageType) && !missingTypes.Contains(eventMessageType))
            {
                missingTypes.Add(eventMessageType);
            }
        }

        // There should be no missing types.
        Assert.True(missingTypes.Count == 0, FormatMissingMessage("event message", missingTypes));
    }

    [Fact]
    public void TestAllSerializableEnumsAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the serializable enums defined.
        // From all of the types in the assmebly, get all that are enum types, are in
        // a namespace that starts with "WebDriverBiDi.", and have a [JsonConverter]
        // attribute. If any type meeting this criteria are not in the list of types
        // registered with the custom JsonSerializerContext, add them to the list of
        // missing types.
        List<Type> missingTypes = [];
        foreach (Type assemblyType in assembly.GetTypes())
        {
            if (assemblyType.IsEnum && IsLibraryNamespace(assemblyType) &&
                assemblyType.GetCustomAttribute<JsonConverterAttribute>() is not null &&
                !registeredTypes.Contains(assemblyType))
            {
                missingTypes.Add(assemblyType);
            }
        }

        // There should be no missing types.
        Assert.True(missingTypes.Count == 0, FormatMissingMessage("serializable enum", missingTypes));
    }

    [Fact]
    public void TestAllTypesWithCustomConvertersAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> coveredTypes = GetAllCoveredTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the types using a custom JsonConverter.
        // From all of the types in the assmebly, get all that are not enum types, are in
        // a namespace that starts with "WebDriverBiDi.", and have a [JsonConverter]
        // attribute. If any type meeting this criteria are not in the list of types
        // registered with the custom JsonSerializerContext, add them to the list of
        // missing types.
        List<Type> missingTypes = [];
        foreach (Type assemblyType in assembly.GetTypes())
        {
            if (!assemblyType.IsEnum && IsLibraryNamespace(assemblyType) &&
                assemblyType.GetCustomAttribute<JsonConverterAttribute>() is not null &&
                !(assemblyType.IsAbstract && assemblyType.IsGenericType) &&
                !coveredTypes.Contains(assemblyType))
            {
                missingTypes.Add(assemblyType);
            }
        }

        // There should be no missing types.
        Assert.True(missingTypes.Count == 0, FormatMissingMessage("custom converter", missingTypes));
    }

    [Fact]
    public void TestAllDerivedTypesAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> coveredTypes = GetAllCoveredTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the types that are polymorphically serializable.
        // From all of the types in the assmebly, get all that have a [JsonDerivedType]
        // attribute. If any type meeting this criteria are not in the list of types
        // registered with the custom JsonSerializerContext, add them to the list of
        // missing types.
        List<Type> missingTypes = [];
        foreach (Type assemblyType in assembly.GetTypes())
        {
            if (!IsLibraryNamespace(assemblyType))
            {
                continue;
            }

            foreach (JsonDerivedTypeAttribute derivedTypeAttribute in assemblyType.GetCustomAttributes<JsonDerivedTypeAttribute>())
            {
                if (!coveredTypes.Contains(derivedTypeAttribute.DerivedType) && !missingTypes.Contains(derivedTypeAttribute.DerivedType))
                {
                    missingTypes.Add(derivedTypeAttribute.DerivedType);
                }
            }

            foreach (DiscriminatedDerivedTypeAttribute discriminatedDerivedTypeAttribute in assemblyType.GetCustomAttributes<DiscriminatedDerivedTypeAttribute>())
            {
                if (!coveredTypes.Contains(discriminatedDerivedTypeAttribute.DerivedType) && !missingTypes.Contains(discriminatedDerivedTypeAttribute.DerivedType))
                {
                    missingTypes.Add(discriminatedDerivedTypeAttribute.DerivedType);
                }
            }
        }

        // There should be no missing types.
        Assert.True(missingTypes.Count == 0, FormatMissingMessage("derived", missingTypes));
    }

    [Fact]
    public void TestAllReferencedPropertyTypesAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> coveredTypes = GetAllCoveredTypes();

        HashSet<Type> visited = [];
        Queue<Type> toProcess = new(coveredTypes);
        HashSet<Type> allReferencedLibraryTypes = [];

        // Get a list of all types referenced by properties.
        // Recursively perform a breadth-first search through all types registered
        // with the custom JsonSerializerContext. Examine all properties of those
        // classes, and add those types to the list. This list should also include
        // types that are generic parameters to generic types, like List<T>.
        while (toProcess.Count > 0)
        {
            Type current = toProcess.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            if (current.GetCustomAttribute<JsonConverterAttribute>() is not null)
            {
                continue;
            }

            foreach (PropertyInfo property in GetSerializableProperties(current))
            {
                foreach (Type extractedType in ExtractReferencedLibraryTypes(property.PropertyType))
                {
                    allReferencedLibraryTypes.Add(extractedType);
                    if (!visited.Contains(extractedType))
                    {
                        toProcess.Enqueue(extractedType);
                    }
                }
            }
        }

        // Given that we now have a list of all types that should be accessed by the custom
        // JsonSerializerContext, add any of those types that are not registered with the
        // custom context to the list of missing types.
        List<Type> missingTypes = [];
        foreach (Type referencedType in allReferencedLibraryTypes)
        {
            if (!coveredTypes.Contains(referencedType) && !referencedType.IsInterface)
            {
                missingTypes.Add(referencedType);
            }
        }

        // There should be no missing types.
        Assert.True(missingTypes.Count == 0, FormatMissingMessage("referenced property", missingTypes));
    }

    [Theory]
    [MemberData(nameof(ContextSerializationCaseNames))]
    public void TestSentShapeSerializesUnderSourceGeneratedContextAsUnderReflection(string caseName)
    {
        // A native AOT application serializes through the source-generated context alone, while every other test
        // serializes through reflection, which can describe any runtime type. Serializing with only the context
        // reproduces what native code does, including its failure for a runtime type the context has no metadata
        // for; the reflection output is the reference the context must match.
        ContextSerializationCase serializationCase = ContextSerializationCases[caseName];
        object value = serializationCase.CreateValue();
        JsonSerializerOptions reflectionOptions = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            RespectNullableAnnotations = true,
        };
        JsonSerializerOptions contextOptions = new()
        {
            TypeInfoResolver = WebDriverBiDiJsonSerializerContext.Default,
            RespectNullableAnnotations = true,
        };

        string reflectionJson = JsonSerializer.Serialize(value, serializationCase.RootType, reflectionOptions);
        string contextJson = JsonSerializer.Serialize(value, serializationCase.RootType, contextOptions);

        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(reflectionJson), JsonNode.Parse(contextJson)),
            $"Serialization under the source-generated context differs from reflection.\nReflection: {reflectionJson}\nContext:    {contextJson}");
    }

    [Theory]
    [MemberData(nameof(ContextDeserializationCaseNames))]
    public void TestReceivedShapeDeserializesUnderSourceGeneratedContextAsUnderReflection(string caseName)
    {
        // The mirror of the test above, for the direction every other test reaches only through reflection.
        // A native AOT application reads each message through the source-generated context alone, and the
        // generator emits read metadata separately from write metadata: a payload the context serializes
        // correctly can still fail, or produce a different graph, when the context deserializes it. The
        // reflection result is the reference.
        ContextDeserializationCase deserializationCase = ContextDeserializationCases[caseName];
        JsonSerializerOptions reflectionOptions = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            RespectNullableAnnotations = true,
        };
        JsonSerializerOptions contextOptions = new()
        {
            TypeInfoResolver = WebDriverBiDiJsonSerializerContext.Default,
            RespectNullableAnnotations = true,
        };

        object? fromReflection = JsonSerializer.Deserialize(deserializationCase.Json, deserializationCase.RootType, reflectionOptions);
        object? fromContext = JsonSerializer.Deserialize(deserializationCase.Json, deserializationCase.RootType, contextOptions);
        Assert.NotNull(fromReflection);
        Assert.NotNull(fromContext);

        // The runtime type of the payload is the whole point of a discriminated union, and two graphs of
        // different types can still compare equivalent member by member, so it is asserted on its own.
        object reflectionPayload = SelectPayload(fromReflection);
        object contextPayload = SelectPayload(fromContext);
        Assert.Equal(reflectionPayload.GetType(), contextPayload.GetType());
        Assert.Equivalent(fromReflection, fromContext, strict: true);
    }

    [Fact]
    public void TestEveryReceivedDiscriminatedUnionHasAContextDeserializationCase()
    {
        // The receive-side counterpart of the object-carrying guard below. A discriminated union is read
        // into whichever derived type its discriminator names, which no static analysis of the context can
        // follow, so the only proof the context can read one is reading it. Requiring a case per union
        // keeps the theory above from falling behind as unions are added.
        //
        // Two of the library's unions are only ever sent — Target names where a script runs, and
        // ProxyConfiguration is a capability the caller asks for (CapabilityRequest.Proxy,
        // CreateUserContextCommandParameters.Proxy); what comes back is ProxyConfigurationResult, which is
        // not a union. Their serialization is covered by the sent-shape cases instead.
        HashSet<Type> sentOnlyUnions = [typeof(Target), typeof(ProxyConfiguration)];

        List<Type> receivedUnions = [];
        foreach (Type assemblyType in typeof(BiDiDriver).Assembly.GetTypes())
        {
            if (!IsLibraryNamespace(assemblyType) || sentOnlyUnions.Contains(assemblyType))
            {
                continue;
            }

            if (assemblyType.GetCustomAttribute<JsonConverterAttribute>(inherit: false)?.ConverterType is { IsGenericType: true } converterType
                && converterType.GetGenericTypeDefinition() == typeof(DiscriminatedUnionJsonConverter<>))
            {
                receivedUnions.Add(assemblyType);
            }
        }

        // Guard against the sweep reaching nothing, which would make the check below vacuous. There are
        // five received unions today: RemoteValue, EvaluateResult, LogEntry, RealmInfo and
        // DownloadEndEventArgs.
        Assert.True(receivedUnions.Count >= 5, $"The discriminated-union sweep found only {receivedUnions.Count} received unions; the walk is broken.");

        JsonSerializerOptions reflectionOptions = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            RespectNullableAnnotations = true,
        };

        // The types the cases actually produce, rather than the types they claim to: each deserialized
        // message is walked, so a case cannot cover a union by assertion alone.
        HashSet<Type> producedTypes = [];
        foreach (ContextDeserializationCase deserializationCase in ContextDeserializationCases.Values)
        {
            object? message = JsonSerializer.Deserialize(deserializationCase.Json, deserializationCase.RootType, reflectionOptions);
            CollectRuntimeTypes(message, producedTypes, []);
        }

        List<string> uncoveredUnions =
        [
            .. receivedUnions
                .Where(union => !producedTypes.Any(union.IsAssignableFrom))
                .Select(union => union.Name)
                .Order(StringComparer.Ordinal)
        ];
        Assert.True(
            uncoveredUnions.Count == 0,
            "The following received types choose their runtime type from a discriminator, but no context deserialization case produces one:\n"
                + string.Join("\n", uncoveredUnions.Select(union => $"  - {union}")));
    }

    [Fact]
    public void TestAllObjectCarryingSerializedMembersHaveContextSerializationCases()
    {
        // A serialized member whose declared type is or contains object is written according to the runtime types
        // of its values, which no static analysis of the context can follow. The only proof that the context covers
        // them is serializing each shape the library sends, so every such member must be exercised by a case above.
        // Extension data is excluded: its values arrive from the remote end, or are supplied by the caller, and are
        // documented as limited to the types the context registers.
        HashSet<string> objectCarryingMembers = [];
        foreach (Type assemblyType in typeof(BiDiDriver).Assembly.GetTypes())
        {
            if (!IsLibraryNamespace(assemblyType))
            {
                continue;
            }

            foreach (PropertyInfo property in GetSerializableProperties(assemblyType))
            {
                if (ContainsObjectType(property.PropertyType))
                {
                    objectCarryingMembers.Add($"{property.DeclaringType!.Name}.{property.Name}");
                }
            }
        }

        HashSet<string> exercisedMembers = [];
        foreach (ContextSerializationCase serializationCase in ContextSerializationCases.Values)
        {
            exercisedMembers.UnionWith(serializationCase.ObjectCarryingMembers);
        }

        List<string> unexercisedMembers = [.. objectCarryingMembers.Where(member => !exercisedMembers.Contains(member)).Order(StringComparer.Ordinal)];
        Assert.True(
            unexercisedMembers.Count == 0,
            "The following serialized members hold object values but no context serialization case exercises them:\n"
                + string.Join("\n", unexercisedMembers.Select(member => $"  - {member}")));

        List<string> staleMembers = [.. exercisedMembers.Where(member => !objectCarryingMembers.Contains(member)).Order(StringComparer.Ordinal)];
        Assert.True(
            staleMembers.Count == 0,
            "The following members are named by context serialization cases but are no longer serialized members holding object values:\n"
                + string.Join("\n", staleMembers.Select(member => $"  - {member}")));
    }

    private static HashSet<Type> GetRegisteredSerializableTypes()
    {
        // return [.. typeof(WebDriverBiDiJsonSerializerContext)
        //         .GetCustomAttributesData()
        //         .Where(a => a.AttributeType == typeof(JsonSerializableAttribute))
        //         .Select(a => (Type)a.ConstructorArguments[0].Value!)];
        HashSet<Type> set = [];
        IList<CustomAttributeData> customAttributeDataObjects = typeof(WebDriverBiDiJsonSerializerContext).GetCustomAttributesData();
        foreach (CustomAttributeData customAttributeDataObject in customAttributeDataObjects)
        {
            if (customAttributeDataObject.AttributeType == typeof(JsonSerializableAttribute))
            {
                Type? registeredType = customAttributeDataObject.ConstructorArguments[0].Value as Type;
                if (registeredType is not null)
                {
                    set.Add(registeredType);
                }
            }
        }

        return set;
    }

    private static HashSet<Type> GetAllCoveredTypes()
    {
        HashSet<Type> coveredTypes = [];
        IList<CustomAttributeData> customAttributeDataObjects = typeof(WebDriverBiDiJsonSerializerContext).GetCustomAttributesData();
        foreach (CustomAttributeData customAttributeDataObject in customAttributeDataObjects)
        {
            if (customAttributeDataObject.AttributeType == typeof(JsonSerializableAttribute))
            {
                Type? registeredType = customAttributeDataObject.ConstructorArguments[0].Value as Type;
                if (registeredType is null)
                {
                    continue;
                }

                coveredTypes.Add(registeredType);
                if (registeredType.IsGenericType)
                {
                    foreach (Type genericArgument in registeredType.GetGenericArguments())
                    {
                        coveredTypes.Add(genericArgument);
                    }
                }
            }
        }

        return coveredTypes;
    }

    private static bool IsLibraryNamespace(Type type)
    {
        return type.Namespace is not null && type.Namespace.StartsWith("WebDriverBiDi", StringComparison.Ordinal);
    }

    private static Type? GetCommandResultTypeFromParameters(Type commandParamsType)
    {
        Type? current = commandParamsType;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(CommandParameters<>))
            {
                return current.GetGenericArguments()[0];
            }

            current = current.BaseType;
        }

        return null;
    }

    private static List<PropertyInfo> GetSerializableProperties(Type type)
    {
        List<PropertyInfo> result = [];
        HashSet<string> seen = [];
        Type? current = type;

        while (current is not null && current != typeof(object))
        {
            foreach (PropertyInfo property in current.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (!seen.Add(property.Name))
                {
                    continue;
                }

                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                if (property.GetCustomAttribute<JsonExtensionDataAttribute>() is not null)
                {
                    continue;
                }

                JsonIgnoreAttribute? jsonIgnore = property.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnore is not null && jsonIgnore.Condition == JsonIgnoreCondition.Always)
                {
                    continue;
                }

                bool hasJsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>() is not null;
                bool hasJsonInclude = property.GetCustomAttribute<JsonIncludeAttribute>() is not null;

                if (hasJsonPropertyName || hasJsonInclude)
                {
                    result.Add(property);
                }
            }

            current = current.BaseType;
        }

        return result;
    }

    private static List<Type> ExtractReferencedLibraryTypes(Type type)
    {
        List<Type> result = [];
        CollectLibraryTypes(type, result);
        return result;
    }

    private static void CollectLibraryTypes(Type type, List<Type> result)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying.IsGenericType)
        {
            foreach (Type arg in underlying.GetGenericArguments())
            {
                CollectLibraryTypes(arg, result);
            }

            return;
        }

        if (underlying.IsArray)
        {
            Type? elementType = underlying.GetElementType();
            if (elementType is not null)
            {
                CollectLibraryTypes(elementType, result);
            }

            return;
        }

        if (IsLibraryNamespace(underlying))
        {
            result.Add(underlying);
        }
    }

    private static string FormatMissingMessage(string category, List<Type> missingTypes)
    {
        return $"The following {category} types are missing [JsonSerializable] attributes "
               + $"in {nameof(WebDriverBiDiJsonSerializerContext)}:\n"
               + string.Join("\n", missingTypes.Select(t => $"  - {t.FullName}"));
    }

    private static bool ContainsObjectType(Type type)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying == typeof(object))
        {
            return true;
        }

        if (underlying.IsArray)
        {
            return ContainsObjectType(underlying.GetElementType()!);
        }

        return underlying.IsGenericType && underlying.GetGenericArguments().Any(ContainsObjectType);
    }

    // Collects the runtime type of every library object reachable from a deserialized message, following
    // properties and the contents of collections. Reference identity guards against a graph that points
    // back at itself; nothing in a received message does today, and the walk should not depend on that.
    private static void CollectRuntimeTypes(object? value, HashSet<Type> collected, HashSet<object> visited)
    {
        if (value is null or string || value.GetType().IsPrimitive)
        {
            return;
        }

        if (!visited.Add(value))
        {
            return;
        }

        Type valueType = value.GetType();
        if (IsLibraryNamespace(valueType))
        {
            collected.Add(valueType);
        }

        if (value is System.Collections.IDictionary dictionary)
        {
            foreach (System.Collections.DictionaryEntry entry in dictionary)
            {
                CollectRuntimeTypes(entry.Key, collected, visited);
                CollectRuntimeTypes(entry.Value, collected, visited);
            }

            return;
        }

        if (value is System.Collections.IEnumerable enumerable)
        {
            foreach (object? element in enumerable)
            {
                CollectRuntimeTypes(element, collected, visited);
            }

            return;
        }

        if (!IsLibraryNamespace(valueType))
        {
            return;
        }

        foreach (PropertyInfo property in valueType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || property.GetMethod is null)
            {
                continue;
            }

            CollectRuntimeTypes(property.GetValue(value), collected, visited);
        }
    }

    // The payload of a received message: the command result, or the event's arguments. Both carry the types
    // whose deserialization the context has to settle, and both are what the driver hands a consumer.
    private static object SelectPayload(object message)
    {
        Type messageType = message.GetType();
        PropertyInfo? payloadProperty = messageType.GetProperty("Result") ?? messageType.GetProperty("EventData");
        Assert.NotNull(payloadProperty);
        object? payload = payloadProperty.GetValue(message);
        Assert.NotNull(payload);
        return payload;
    }

    private static ContextDeserializationCase EvaluateResultCase(string remoteValueJson)
    {
        return new(
            typeof(CommandResponseMessage<EvaluateResult>),
            """{"type":"success","id":1,"result":{"type":"success","realm":"myRealmId","result":"""
                + remoteValueJson.Trim()
                + "}}");
    }

    private static ContextSerializationCase LocateNodesCase(Locator locator)
    {
        return new(typeof(LocateNodesCommandParameters), () => new LocateNodesCommandParameters("myContext", locator), []);
    }

    private static ContextSerializationCase PointerMoveCase(Origin? origin)
    {
        return new(
            typeof(PerformActionsCommandParameters),
            () =>
            {
                PerformActionsCommandParameters parameters = new("myContext");
                PointerSourceActions pointerActions = new("pointer");
                pointerActions.Actions.Add(new PointerMoveAction { X = 1.5, Y = 2, Origin = origin });
                parameters.Actions.Add(pointerActions);
                return parameters;
            },
            [PointerMoveOriginMember]);
    }

    private static ContextSerializationCase WheelScrollCase(Origin? origin)
    {
        return new(
            typeof(PerformActionsCommandParameters),
            () =>
            {
                PerformActionsCommandParameters parameters = new("myContext");
                WheelSourceActions wheelActions = new("wheel");
                wheelActions.Actions.Add(new WheelScrollAction { X = 1, Y = 2, DeltaX = 3, DeltaY = 4, Origin = origin });
                parameters.Actions.Add(wheelActions);
                return parameters;
            },
            [WheelScrollOriginMember]);
    }

    private static ContextSerializationCase ArgumentCase(LocalValue argument)
    {
        return new(typeof(CallFunctionCommandParameters), () => CreateCallFunctionParameters(argument), [LocalArgumentValueMember]);
    }

    private static CallFunctionCommandParameters CreateCallFunctionParameters(LocalValue argument)
    {
        CallFunctionCommandParameters parameters = new("(arg) => arg", new ContextTarget("myContext"), false);
        parameters.Arguments.Add(argument);
        return parameters;
    }

    private static ContextSerializationCase PrintCase(params PageRange[] pageRanges)
    {
        return new(
            typeof(PrintCommandParameters),
            () =>
            {
                PrintCommandParameters parameters = new("myContext");
                parameters.PageRanges.AddRange(pageRanges);
                return parameters;
            },
            [PrintPageRangesMember]);
    }

    /// <summary>
    /// A shape the library sends, with the object-carrying members serializing it exercises.
    /// </summary>
    /// <param name="RootType">The type the value is serialized as, which is how the transport serializes it.</param>
    /// <param name="CreateValue">Creates the value to serialize.</param>
    /// <param name="ObjectCarryingMembers">The members, named as <c>DeclaringType.Property</c>, whose object values the case writes.</param>
    private sealed record ContextSerializationCase(Type RootType, Func<object> CreateValue, string[] ObjectCarryingMembers);

    /// <summary>
    /// A shape the library receives, as the transport reads it: the envelope type and the message's JSON.
    /// </summary>
    /// <param name="RootType">The type the message is deserialized as, which is how the transport reads it.</param>
    /// <param name="Json">The message as the remote end sends it.</param>
    private sealed record ContextDeserializationCase(Type RootType, string Json);
}
