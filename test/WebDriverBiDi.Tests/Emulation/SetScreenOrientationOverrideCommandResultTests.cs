namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetScreenOrientationOverrideCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetScreenOrientationOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenOrientationOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetScreenOrientationOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenOrientationOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetScreenOrientationOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}