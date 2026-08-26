namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.TestUtilities;

/// <summary>
/// Verifies that a resolver registered with <see cref="Transport.RegisterTypeInfoResolverAsync"/> actually
/// participates in (de)serialization. Under reflection the library's default resolver answers every type, so
/// the source-generation proof is made against the same combination the transport builds:
/// <c>JsonTypeInfoResolver.Combine(WebDriverBiDiJsonSerializerContext.Default, custom)</c>.
/// </summary>
public class TypeInfoResolverRegistrationTests
{
    [Fact]
    public void TestLibraryContextAloneDoesNotResolveCustomTypes()
    {
        // The premise of RegisterTypeInfoResolverAsync: without a registered resolver, the
        // source-generated context knows nothing about a consumer's custom command types.
        Assert.Null(WebDriverBiDiJsonSerializerContext.Default.GetTypeInfo(typeof(CustomCommandParameters)));
        Assert.Null(WebDriverBiDiJsonSerializerContext.Default.GetTypeInfo(typeof(CustomCommandResult)));
    }

    [Fact]
    public void TestCombinedResolverRoundTripsCustomCommandUnderSourceGeneration()
    {
        // Exactly what Transport.RegisterTypeInfoResolverAsync does with its options.
        JsonSerializerOptions options = new()
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(WebDriverBiDiJsonSerializerContext.Default, CustomCommandJsonContext.Default),
            RespectNullableAnnotations = true,
        };

        // Outbound: the library's Command wrapper (from its own context) carrying custom parameters (from ours).
        Command command = new(7, new CustomCommandParameters("hello"));
        string json = JsonSerializer.Serialize(command, options.GetTypeInfo(typeof(Command)));
        JObject serialized = JObject.Parse(json);
        Assert.Equal(7, serialized["id"]!.Value<long>());
        Assert.Equal("custom.command", serialized["method"]!.Value<string>());
        Assert.Equal("hello", serialized["params"]!["input"]!.Value<string>());

        // Inbound: the envelope's type info is built by the library (as CommandParameters<T> does for the
        // transport), so only the result type has to come from the combined resolver chain.
        Assert.NotNull(options.GetTypeInfo(typeof(CustomCommandResult)));
        JsonTypeInfo<CommandResponseMessage<CustomCommandResult>> responseTypeInfo =
            JsonMetadataServices.CreateValueInfo<CommandResponseMessage<CustomCommandResult>>(options, new CommandResponseMessageJsonConverter<CustomCommandResult>());
        string responseJson = """{"type":"success","id":7,"result":{"output":"HELLO"}}""";
        CommandResponseMessage<CustomCommandResult>? response = JsonSerializer.Deserialize(responseJson, responseTypeInfo);
        Assert.NotNull(response);
        CustomCommandResult result = Assert.IsType<CustomCommandResult>(response.Result);
        Assert.Equal("HELLO", result.Output);
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task TestRegisteredResolverIsUsedByDriverForCustomCommand()
    {
        string? sentJson = null;
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            sentJson = connection.DataSent;
            await connection.RaiseDataReceivedEventAsync("""{"type":"success","id":1,"goog:channel":"c","result":{"output":"HELLO","goog:extra":"x"}}""");
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), transport);
        await driver.RegisterTypeInfoResolverAsync(CustomCommandJsonContext.Default, TestContext.Current.CancellationToken);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        CustomCommandResult result = await driver.ExecuteCommandAsync(new CustomCommandParameters("hello"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("HELLO", result.Output);

        // Root extension data is computed from the consumer type's metadata, so it needs no attribute.
        Assert.Equal("x", result.AdditionalData["goog:extra"]);
        Assert.Equal("c", result.AdditionalResponseProperties["goog:channel"]);
        Assert.NotNull(sentJson);
        JObject sent = JObject.Parse(sentJson);
        Assert.Equal("custom.command", sent["method"]!.Value<string>());
        Assert.Equal("hello", sent["params"]!["input"]!.Value<string>());
    }
}

/// <summary>
/// A consumer-defined command, as a custom module would declare it.
/// </summary>
public class CustomCommandParameters : CommandParameters<CustomCommandResult>
{
    public CustomCommandParameters(string input)
    {
        this.Input = input;
    }

    [JsonIgnore]
    public override string MethodName => "custom.command";

    [JsonPropertyName("input")]
    public string Input { get; set; }
}

/// <summary>
/// The result of the consumer-defined command.
/// </summary>
public record CustomCommandResult : CommandResult
{
    [JsonIgnore]
    public override bool IsError => false;

    [JsonPropertyName("output")]
    public string? Output { get; set; }
}

/// <summary>
/// The consumer's source-generated context, registering the parameters type and the closed
/// generic response wrapper, as the AOT compatibility guide prescribes.
/// </summary>
[JsonSerializable(typeof(CustomCommandParameters))]
[JsonSerializable(typeof(CustomCommandResult))]
internal partial class CustomCommandJsonContext : JsonSerializerContext
{
}
