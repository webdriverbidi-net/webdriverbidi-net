namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetForcedColorsModeThemeOverrideCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetForcedColorsModeThemeOverrideCommandResult? result = JsonSerializer.Deserialize<SetForcedColorsModeThemeOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetForcedColorsModeThemeOverrideCommandResult? result = JsonSerializer.Deserialize<SetForcedColorsModeThemeOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetForcedColorsModeThemeOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}