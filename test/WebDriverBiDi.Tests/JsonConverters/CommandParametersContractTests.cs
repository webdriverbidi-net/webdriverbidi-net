namespace WebDriverBiDi.JsonConverters;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using TestUtilities;
using WebDriverBiDi.Protocol;

/// <summary>
/// Tests of the contract rule the transport adds to every type info resolver it serializes through, which keeps the
/// members that describe a command out of its <c>params</c> object.
/// </summary>
/// <remarks>
/// The rule is internal, so for the source-generated case it is reached here by reflection, as
/// <see cref="ExtensionDataNameGuardTests"/> reaches the extension-data guard: under reflection the library's own
/// resolver answers every type, so a registered source-generated context is never consulted by a transport in these
/// tests.
/// </remarks>
public class CommandParametersContractTests
{
    private static readonly Action<JsonTypeInfo> RemoveCommandDescriptionProperties = (Action<JsonTypeInfo>)typeof(Transport).Assembly
        .GetType("WebDriverBiDi.JsonConverters.CommandParametersContract", throwOnError: true)!
        .GetMethod("RemoveCommandDescriptionProperties", BindingFlags.Public | BindingFlags.Static)!
        .CreateDelegate(typeof(Action<JsonTypeInfo>));

    [Fact]
    public async Task TestMethodNameOverrideWithoutJsonIgnoreIsNotSentInParams()
    {
        JObject parameters = await SendAndCaptureParametersAsync(new UnannotatedCommandParameters());

        Assert.Equal("value", parameters["value"]?.Value<string>());
        Assert.Null(parameters["MethodName"]);
        Assert.Null(parameters["ResponseType"]);
    }

    [Fact]
    public async Task TestExtensionDataEntryNamedForARemovedMemberIsSent()
    {
        // The member is removed before the extension-data guard runs, so an entry that uses its name does not
        // duplicate anything written in params, and is sent rather than rejected.
        UnannotatedCommandParameters commandParameters = new();
        commandParameters.AdditionalData["MethodName"] = "extension value";

        JObject parameters = await SendAndCaptureParametersAsync(commandParameters);

        Assert.Equal("extension value", parameters["MethodName"]?.Value<string>());
    }

    [Fact]
    public async Task TestPropertyNamedMethodNameOnANestedObjectIsSent()
    {
        // Only the members of a parameters type that describe the command are removed. An object inside the
        // parameters is not a parameters type, and a property of its own that happens to share the name is kept.
        JObject parameters = await SendAndCaptureParametersAsync(new NestingCommandParameters());

        Assert.Equal("nested value", parameters["nested"]?["MethodName"]?.Value<string>());
    }

    [Fact]
    public async Task TestPropertyHidingMethodNameIsNotRemoved()
    {
        // A member declared with 'new' shares the name but not the declaration it overrides, so it is not one of the
        // members that describe the command. Having no getter, it writes nothing, but it is left in the contract.
        JsonSerializerOptions options = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(RemoveCommandDescriptionProperties),
        };

        JsonTypeInfo typeInfo = options.GetTypeInfo(typeof(HidingCommandParameters));

        JsonPropertyInfo hidingProperty = Assert.Single(typeInfo.Properties, property => property.Name == "MethodName");
        Assert.Null(hidingProperty.Get);
        Assert.Equal("""{"value":"value"}""", JsonSerializer.Serialize(new HidingCommandParameters(), typeInfo));
    }

    [Fact]
    public void TestMethodNameOverrideIsRemovedUnderSourceGenerationWithANamingPolicy()
    {
        // A source-generated context with a naming policy writes the member as "methodName", so the rule must find it
        // by the declaration it overrides rather than by name.
        JsonSerializerOptions options = new(CommandParametersContractTestJsonContext.Default.Options);
        options.TypeInfoResolver = CommandParametersContractTestJsonContext.Default.WithAddedModifier(RemoveCommandDescriptionProperties);

        string json = JsonSerializer.Serialize(new UnannotatedCommandParameters(), options.GetTypeInfo(typeof(UnannotatedCommandParameters)));

        Assert.Equal("""{"value":"value"}""", json);
    }

    [Fact]
    public void TestMethodNameOverrideIsWrittenWithoutTheRule()
    {
        // The rule is what keeps the member out: without it, System.Text.Json writes the override, because it does not
        // carry [JsonIgnore] onto an override from the member it overrides.
        string json = JsonSerializer.Serialize(new UnannotatedCommandParameters(), CommandParametersContractTestJsonContext.Default.UnannotatedCommandParameters);

        Assert.Contains("\"methodName\":\"test.unannotated\"", json);
    }

    private static async Task<JObject> SendAndCaptureParametersAsync(CommandParameters commandParameters)
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        JObject message = JObject.Parse(connection.DataSent ?? string.Empty);
        return Assert.IsType<JObject>(message["params"]);
    }
}

/// <summary>
/// A consumer's parameters type whose <see cref="MethodName"/> override omits <c>[JsonIgnore]</c>.
/// </summary>
public class UnannotatedCommandParameters : CommandParameters<CommandParametersContractTestResult>
{
    public override string MethodName => "test.unannotated";

    [JsonPropertyName("value")]
    public string Value { get; set; } = "value";
}

public class NestingCommandParameters : CommandParameters<CommandParametersContractTestResult>
{
    public override string MethodName => "test.nesting";

    [JsonPropertyName("nested")]
    public NestedObjectWithMethodName Nested { get; set; } = new();
}

public class NestedObjectWithMethodName
{
    public string MethodName { get; set; } = "nested value";
}

public class HidingCommandParameters : UnannotatedCommandParameters
{
    public new string MethodName
    {
        set
        {
        }
    }
}

public record CommandParametersContractTestResult : CommandResult;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UnannotatedCommandParameters))]
internal partial class CommandParametersContractTestJsonContext : JsonSerializerContext
{
}
