namespace WebDriverBiDi.Session;

using System.Text.Json;

[TestFixture]
public class UnsubscribeCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        UnsubscribeCommandResult? result = JsonSerializer.Deserialize<UnsubscribeCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        UnsubscribeCommandResult? result = JsonSerializer.Deserialize<UnsubscribeCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        UnsubscribeCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
