namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateDescriptorResponseCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateDescriptorResponseCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateDescriptorResponseCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateDescriptorResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}