namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;

[TestFixture]
public class SetClientHintsOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetClientHintsOverrideCommandResult? result = JsonSerializer.Deserialize<SetClientHintsOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetClientHintsOverrideCommandResult? result = JsonSerializer.Deserialize<SetClientHintsOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetClientHintsOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
