namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetDownloadBehaviorCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetDownloadBehaviorCommandResult? result = JsonSerializer.Deserialize<SetDownloadBehaviorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetDownloadBehaviorCommandResult? result = JsonSerializer.Deserialize<SetDownloadBehaviorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetDownloadBehaviorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}