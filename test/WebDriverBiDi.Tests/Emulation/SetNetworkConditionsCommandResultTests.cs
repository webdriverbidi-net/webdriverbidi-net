namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetNetworkConditionsCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetNetworkConditionsCommandResult? result = JsonSerializer.Deserialize<SetNetworkConditionsCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetNetworkConditionsCommandResult? result = JsonSerializer.Deserialize<SetNetworkConditionsCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetNetworkConditionsCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}