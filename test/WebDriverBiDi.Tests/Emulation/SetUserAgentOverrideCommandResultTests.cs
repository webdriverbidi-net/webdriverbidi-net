namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetUserAgentOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetUserAgentOverrideCommandResult? result = JsonSerializer.Deserialize<SetUserAgentOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetUserAgentOverrideCommandResult? result = JsonSerializer.Deserialize<SetUserAgentOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetUserAgentOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
