namespace WebDriverBiDi.Input;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetFilesCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetFilesCommandResult? result = JsonSerializer.Deserialize<SetFilesCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetFilesCommandResult? result = JsonSerializer.Deserialize<SetFilesCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetFilesCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
