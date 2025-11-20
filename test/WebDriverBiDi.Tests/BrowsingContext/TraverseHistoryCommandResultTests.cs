namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class TraverseHistoryCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        TraverseHistoryCommandResult? result = JsonSerializer.Deserialize<TraverseHistoryCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        TraverseHistoryCommandResult? result = JsonSerializer.Deserialize<TraverseHistoryCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        TraverseHistoryCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}