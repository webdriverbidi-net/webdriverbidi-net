namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetTimeZoneOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetTimeZoneOverrideCommandResult? result = JsonSerializer.Deserialize<SetTimeZoneOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetTimeZoneOverrideCommandResult? result = JsonSerializer.Deserialize<SetTimeZoneOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetTimeZoneOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
