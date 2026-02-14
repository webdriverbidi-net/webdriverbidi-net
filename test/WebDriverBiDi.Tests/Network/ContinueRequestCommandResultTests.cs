namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class ContinueRequestCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        ContinueRequestCommandResult? result = JsonSerializer.Deserialize<ContinueRequestCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ContinueRequestCommandResult? result = JsonSerializer.Deserialize<ContinueRequestCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        ContinueRequestCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}