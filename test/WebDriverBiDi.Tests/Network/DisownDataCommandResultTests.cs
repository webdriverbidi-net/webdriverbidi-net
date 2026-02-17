namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class DisownDataCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        DisownDataCommandResult? result = JsonSerializer.Deserialize<DisownDataCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        DisownDataCommandResult? result = JsonSerializer.Deserialize<DisownDataCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        DisownDataCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
