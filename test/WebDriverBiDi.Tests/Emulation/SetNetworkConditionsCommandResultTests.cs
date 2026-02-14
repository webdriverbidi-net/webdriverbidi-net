namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetNetworkConditionsCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetNetworkConditionsCommandResult? result = JsonSerializer.Deserialize<SetNetworkConditionsCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetNetworkConditionsCommandResult? result = JsonSerializer.Deserialize<SetNetworkConditionsCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetNetworkConditionsCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}