namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class SetCacheBehaviorCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetCacheBehaviorCommandResult? result = JsonSerializer.Deserialize<SetCacheBehaviorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetCacheBehaviorCommandResult? result = JsonSerializer.Deserialize<SetCacheBehaviorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetCacheBehaviorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}