namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateServiceCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateServiceCommandResult? result = JsonSerializer.Deserialize<SimulateServiceCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateServiceCommandResult? result = JsonSerializer.Deserialize<SimulateServiceCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateServiceCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}