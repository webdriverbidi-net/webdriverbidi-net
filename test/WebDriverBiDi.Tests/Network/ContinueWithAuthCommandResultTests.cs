namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class ContinueWithAuthCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        ContinueWithAuthCommandResult? result = JsonSerializer.Deserialize<ContinueWithAuthCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ContinueWithAuthCommandResult? result = JsonSerializer.Deserialize<ContinueWithAuthCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        ContinueWithAuthCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}