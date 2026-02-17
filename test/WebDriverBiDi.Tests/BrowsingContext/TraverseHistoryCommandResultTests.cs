namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class TraverseHistoryCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        TraverseHistoryCommandResult? result = JsonSerializer.Deserialize<TraverseHistoryCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        TraverseHistoryCommandResult? result = JsonSerializer.Deserialize<TraverseHistoryCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        TraverseHistoryCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
