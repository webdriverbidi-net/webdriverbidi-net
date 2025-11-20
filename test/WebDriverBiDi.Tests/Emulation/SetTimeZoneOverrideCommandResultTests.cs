namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetTimeZoneOverrideCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetTimeZoneOverrideCommandResult? result = JsonSerializer.Deserialize<SetTimeZoneOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetTimeZoneOverrideCommandResult? result = JsonSerializer.Deserialize<SetTimeZoneOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetTimeZoneOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}