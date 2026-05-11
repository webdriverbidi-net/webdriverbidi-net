namespace WebDriverBiDi.DigitalCredentials;

using System.Text.Json;

[TestFixture]
public class SetVirtualWalletBehaviorCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetVirtualWalletBehaviorCommandResult? result = JsonSerializer.Deserialize<SetVirtualWalletBehaviorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetVirtualWalletBehaviorCommandResult? result = JsonSerializer.Deserialize<SetVirtualWalletBehaviorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetVirtualWalletBehaviorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
