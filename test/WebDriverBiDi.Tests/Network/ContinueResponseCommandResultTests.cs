namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class ContinueResponseCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        ContinueResponseCommandResult? result = JsonSerializer.Deserialize<ContinueResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ContinueResponseCommandResult? result = JsonSerializer.Deserialize<ContinueResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        ContinueResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
