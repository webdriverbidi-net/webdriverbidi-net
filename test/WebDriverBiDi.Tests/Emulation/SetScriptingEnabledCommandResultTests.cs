namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetScriptingEnabledCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetScriptingEnabledCommandResult? result = JsonSerializer.Deserialize<SetScriptingEnabledCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetScriptingEnabledCommandResult? result = JsonSerializer.Deserialize<SetScriptingEnabledCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetScriptingEnabledCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}