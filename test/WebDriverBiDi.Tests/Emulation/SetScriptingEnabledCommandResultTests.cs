namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetScriptingEnabledCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetScriptingEnabledCommandResult? result = JsonSerializer.Deserialize<SetScriptingEnabledCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetScriptingEnabledCommandResult? result = JsonSerializer.Deserialize<SetScriptingEnabledCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetScriptingEnabledCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}