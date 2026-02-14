namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class RemoveDataCollectorCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        RemoveDataCollectorCommandResult? result = JsonSerializer.Deserialize<RemoveDataCollectorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemoveDataCollectorCommandResult? result = JsonSerializer.Deserialize<RemoveDataCollectorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        RemoveDataCollectorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}