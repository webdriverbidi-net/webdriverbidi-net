namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class DisownCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        DisownCommandResult? result = JsonSerializer.Deserialize<DisownCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        DisownCommandResult? result = JsonSerializer.Deserialize<DisownCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        DisownCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}