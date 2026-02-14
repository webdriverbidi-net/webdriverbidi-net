namespace WebDriverBiDi.Browser;

using System.Text.Json;

[TestFixture]
public class CloseCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        CloseCommandResult? result = JsonSerializer.Deserialize<CloseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        CloseCommandResult? result = JsonSerializer.Deserialize<CloseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        CloseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}