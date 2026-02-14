namespace WebDriverBiDi.Browser;

using System.Text.Json;

[TestFixture]
public class SetDownloadBehaviorCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetDownloadBehaviorCommandResult? result = JsonSerializer.Deserialize<SetDownloadBehaviorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetDownloadBehaviorCommandResult? result = JsonSerializer.Deserialize<SetDownloadBehaviorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetDownloadBehaviorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}