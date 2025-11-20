namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class RemoveUserContextCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        RemoveUserContextCommandResult? result = JsonSerializer.Deserialize<RemoveUserContextCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemoveUserContextCommandResult? result = JsonSerializer.Deserialize<RemoveUserContextCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        RemoveUserContextCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}