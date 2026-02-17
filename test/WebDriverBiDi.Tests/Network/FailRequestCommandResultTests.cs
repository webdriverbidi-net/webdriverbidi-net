namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class FailRequestCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        FailRequestCommandResult? result = JsonSerializer.Deserialize<FailRequestCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        FailRequestCommandResult? result = JsonSerializer.Deserialize<FailRequestCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        FailRequestCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
