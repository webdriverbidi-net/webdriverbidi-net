namespace WebDriverBiDi.JsonConverters;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Network;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

/// <summary>
/// Tests of the guard the transport adds to every type info resolver it serializes through, which rejects an
/// extension-data entry named for a property its object already serializes.
/// </summary>
/// <remarks>
/// The guard is internal, so it is reached here by reflection, as other tests reach private transport state. That
/// lets it be applied to the source-generated context, which the transport selects only when reflection is
/// disabled, as well as to the reflection resolver the transport uses everywhere else.
/// </remarks>
public class ExtensionDataNameGuardTests
{
    private const string Origin = "https://example.com";

    private static readonly Action<JsonTypeInfo> GuardExtensionData = (Action<JsonTypeInfo>)typeof(Transport).Assembly
        .GetType("WebDriverBiDi.JsonConverters.ExtensionDataNameGuard", throwOnError: true)!
        .GetMethod("GuardExtensionData", BindingFlags.Public | BindingFlags.Static)!
        .CreateDelegate(typeof(Action<JsonTypeInfo>));

    // Every library type a command sends that carries its own extension data, each with an entry named for one of
    // the type's serialized properties.
    private static readonly Dictionary<string, ShadowingCase> ShadowingCases = new()
    {
        ["command parameters"] = new(
            () =>
            {
                SetPermissionCommandParameters parameters = new("geolocation", PermissionState.Granted, Origin);
                parameters.AdditionalData["origin"] = "https://other.example.com";
                return parameters;
            },
            "origin",
            typeof(SetPermissionCommandParameters)),
        ["permission descriptor"] = new(
            () =>
            {
                PermissionDescriptor descriptor = new("midi");
                descriptor.AdditionalData["name"] = "camera";
                return new SetPermissionCommandParameters(descriptor, PermissionState.Granted, Origin);
            },
            "name",
            typeof(PermissionDescriptor)),
        ["always-match capability request"] = new(
            () =>
            {
                CapabilityRequest request = new();
                request.AdditionalCapabilities["browserName"] = "firefox";
                NewCommandParameters parameters = new();
                parameters.Capabilities.AlwaysMatch = request;
                return parameters;
            },
            "browserName",
            typeof(CapabilityRequest)),
        ["first-match capability request"] = new(
            () =>
            {
                CapabilityRequest request = new();
                request.AdditionalCapabilities["acceptInsecureCerts"] = true;
                NewCommandParameters parameters = new();
                parameters.Capabilities.FirstMatch.Add(request);
                return parameters;
            },
            "acceptInsecureCerts",
            typeof(CapabilityRequest)),
        ["proxy configuration"] = new(
            () =>
            {
                DirectProxyConfiguration proxy = new();
                proxy.AdditionalData["proxyType"] = "manual";
                NewCommandParameters parameters = new();
                parameters.Capabilities.AlwaysMatch = new CapabilityRequest { Proxy = proxy };
                return parameters;
            },
            "proxyType",
            typeof(DirectProxyConfiguration)),
        ["remote reference"] = new(
            () =>
            {
                SharedReference reference = new("sharedId");
                reference.AdditionalData["sharedId"] = "otherSharedId";
                CallFunctionCommandParameters parameters = new("(arg) => arg", new ContextTarget("myContext"), false);
                parameters.Arguments.Add(reference);
                return parameters;
            },
            "sharedId",
            typeof(SharedReference)),
        ["partial cookie"] = new(
            () =>
            {
                PartialCookie cookie = new("cookieName", BytesValue.FromString("cookieValue"), "example.com");
                cookie.AdditionalData["domain"] = "other.example.com";
                return new SetCookieCommandParameters(cookie);
            },
            "domain",
            typeof(PartialCookie)),
        ["cookie filter"] = new(
            () =>
            {
                CookieFilter filter = new();
                filter.AdditionalData["name"] = "cookieName";
                return new GetCookiesCommandParameters { Filter = filter };
            },
            "name",
            typeof(CookieFilter)),
        ["storage key partition descriptor"] = new(
            () =>
            {
                StorageKeyPartitionDescriptor partition = new();
                partition.AdditionalData["sourceOrigin"] = Origin;
                return new GetCookiesCommandParameters { Partition = partition };
            },
            "sourceOrigin",
            typeof(StorageKeyPartitionDescriptor)),
    };

    public static TheoryData<string, bool> ShadowingCaseData
    {
        get
        {
            TheoryData<string, bool> data = [];
            foreach (string caseName in ShadowingCases.Keys)
            {
                data.Add(caseName, false);
                data.Add(caseName, true);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ShadowingCaseData))]
    public void TestExtensionDataEntryNamedForASerializedPropertyThrows(string caseName, bool useSourceGeneratedContext)
    {
        ShadowingCase shadowingCase = ShadowingCases[caseName];
        Command command = new(1, shadowingCase.CreateParameters());
        JsonSerializerOptions options = CreateGuardedOptions(useSourceGeneratedContext);

        WebDriverBiDiSerializationException exception = Assert.Throws<WebDriverBiDiSerializationException>(() => JsonSerializer.SerializeToUtf8Bytes(command, options.GetTypeInfo(typeof(Command))));
        Assert.Contains($"entry '{shadowingCase.ShadowedName}'", exception.Message);
        Assert.Contains(shadowingCase.OwnerType.FullName!, exception.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestExtensionDataEntriesWithOtherNamesAreWritten(bool useSourceGeneratedContext)
    {
        PartialCookie cookie = new("cookieName", BytesValue.FromString("cookieValue"), "example.com");
        cookie.AdditionalData["goog:cookieExtra"] = "cookieExtraValue";
        SetCookieCommandParameters parameters = new(cookie);
        parameters.AdditionalData["goog:parametersExtra"] = "parametersExtraValue";
        Command command = new(1, parameters);
        JsonSerializerOptions options = CreateGuardedOptions(useSourceGeneratedContext);

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(command, options.GetTypeInfo(typeof(Command))));

        Assert.Equal("parametersExtraValue", serialized["params"]!["goog:parametersExtra"]!.Value<string>());
        Assert.Equal("cookieExtraValue", serialized["params"]!["cookie"]!["goog:cookieExtra"]!.Value<string>());
        Assert.Equal("example.com", serialized["params"]!["cookie"]!["domain"]!.Value<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestExtensionDataEntryNamedForAnIgnoredPropertyIsWritten(bool useSourceGeneratedContext)
    {
        // MethodName is [JsonIgnore]d, so it consumes no name in the payload and cannot be shadowed.
        SetPermissionCommandParameters parameters = new("geolocation", PermissionState.Granted, Origin);
        parameters.AdditionalData["MethodName"] = "notAConflict";
        Command command = new(1, parameters);
        JsonSerializerOptions options = CreateGuardedOptions(useSourceGeneratedContext);

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(command, options.GetTypeInfo(typeof(Command))));

        Assert.Equal("notAConflict", serialized["params"]!["MethodName"]!.Value<string>());
    }

    [Fact]
    public void TestGuardAppliesToConsumerTypesFromARegisteredSourceGeneratedResolver()
    {
        // The combination the transport builds when a consumer registers a resolver: the library's guarded context,
        // then the consumer's guarded context. The nested object comes from the consumer's metadata.
        JsonSerializerOptions options = new()
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                WebDriverBiDiJsonSerializerContext.Default.WithAddedModifier(GuardExtensionData),
                ExtensionDataNameGuardTestJsonContext.Default.WithAddedModifier(GuardExtensionData)),
            RespectNullableAnnotations = true,
        };
        GuardedConsumerCommandParameters parameters = new();
        parameters.Nested.AdditionalData["label"] = "shadowingValue";
        Command command = new(1, parameters);

        WebDriverBiDiSerializationException exception = Assert.Throws<WebDriverBiDiSerializationException>(() => JsonSerializer.SerializeToUtf8Bytes(command, options.GetTypeInfo(typeof(Command))));
        Assert.Contains("entry 'label'", exception.Message);
        Assert.Contains(typeof(GuardedConsumerNestedObject).FullName!, exception.Message);
    }

    [Fact]
    public void TestElementExtensionDataEntryNamedForASerializedPropertyThrows()
    {
        ElementExtensionDataObject value = new();
        value.ExtensionData["name"] = JsonSerializer.SerializeToElement("shadowingValue");

        WebDriverBiDiSerializationException exception = Assert.Throws<WebDriverBiDiSerializationException>(() => JsonSerializer.Serialize(value, CreateGuardedOptions(false)));
        Assert.Contains("ExtensionData entry 'name'", exception.Message);
    }

    [Fact]
    public void TestElementExtensionDataEntryNamedForAPropertyWithNoGetterIsWritten()
    {
        // A property with no getter is never written, so it consumes no name in the payload.
        ElementExtensionDataObject value = new();
        value.ExtensionData["writeOnly"] = JsonSerializer.SerializeToElement("extraValue");

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(value, CreateGuardedOptions(false)));

        Assert.Equal("value", serialized["name"]!.Value<string>());
        Assert.Equal("extraValue", serialized["writeOnly"]!.Value<string>());
    }

    [Fact]
    public void TestNullExtensionDataIsWritten()
    {
        NullableExtensionDataObject value = new();

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(value, CreateGuardedOptions(false)));

        Assert.Single(serialized);
        Assert.Equal("value", serialized["name"]!.Value<string>());
    }

    [Fact]
    public void TestExtensionDataOnATypeWithNoOtherPropertiesIsWritten()
    {
        ExtensionDataOnlyObject value = new();
        value.ExtensionData["name"] = "anyValue";

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(value, CreateGuardedOptions(false)));

        Assert.Equal("anyValue", serialized["name"]!.Value<string>());
    }

    private static JsonSerializerOptions CreateGuardedOptions(bool useSourceGeneratedContext)
    {
        IJsonTypeInfoResolver resolver = useSourceGeneratedContext
            ? WebDriverBiDiJsonSerializerContext.Default
            : new DefaultJsonTypeInfoResolver();
        return new JsonSerializerOptions
        {
            TypeInfoResolver = resolver.WithAddedModifier(GuardExtensionData),
            RespectNullableAnnotations = true,
        };
    }

    private sealed record ShadowingCase(Func<CommandParameters> CreateParameters, string ShadowedName, Type OwnerType);
}

public class ElementExtensionDataObject
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "value";

    [JsonPropertyName("writeOnly")]
    public string WriteOnly
    {
        set
        {
        }
    }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionData { get; } = [];
}

public class NullableExtensionDataObject
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "value";

    [JsonExtensionData]
    public Dictionary<string, object?>? ExtensionData { get; set; }
}

public class ExtensionDataOnlyObject
{
    [JsonExtensionData]
    public Dictionary<string, object?> ExtensionData { get; } = [];
}

public class GuardedConsumerCommandParameters : CommandParameters<CustomCommandResult>
{
    [JsonIgnore]
    public override string MethodName => "custom.guardedCommand";

    [JsonPropertyName("nested")]
    public GuardedConsumerNestedObject Nested { get; } = new();
}

public class GuardedConsumerNestedObject
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = "value";

    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; } = [];
}

[JsonSerializable(typeof(GuardedConsumerCommandParameters))]
internal partial class ExtensionDataNameGuardTestJsonContext : JsonSerializerContext
{
}
