namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetGeolocationOverrideCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetGeolocationOverrideCommandResult? result = JsonSerializer.Deserialize<SetGeolocationOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetGeolocationOverrideCommandResult? result = JsonSerializer.Deserialize<SetGeolocationOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetGeolocationOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}