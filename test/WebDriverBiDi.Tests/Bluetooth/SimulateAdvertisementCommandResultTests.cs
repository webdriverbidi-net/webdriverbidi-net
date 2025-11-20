namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateAdvertisementCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateAdvertisementCommandResult? result = JsonSerializer.Deserialize<SimulateAdvertisementCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateAdvertisementCommandResult? result = JsonSerializer.Deserialize<SimulateAdvertisementCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateAdvertisementCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}