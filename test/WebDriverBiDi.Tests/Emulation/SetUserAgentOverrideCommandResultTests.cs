namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetUserAgentOverrideCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetUserAgentOverrideCommandResult? result = JsonSerializer.Deserialize<SetUserAgentOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetUserAgentOverrideCommandResult? result = JsonSerializer.Deserialize<SetUserAgentOverrideCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetUserAgentOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}