namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateAdapterCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateAdapterCommandResult? result = JsonSerializer.Deserialize<SimulateAdapterCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateAdapterCommandResult? result = JsonSerializer.Deserialize<SimulateAdapterCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateAdapterCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}