namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetScreenSettingsOverrideCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetScreenSettingsOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenSettingsOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetScreenSettingsOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenSettingsOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetScreenSettingsOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}