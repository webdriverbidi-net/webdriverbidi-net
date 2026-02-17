namespace WebDriverBiDi.Browser;

using System.Text.Json;

[TestFixture]
public class RemoveUserContextCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        RemoveUserContextCommandResult? result = JsonSerializer.Deserialize<RemoveUserContextCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemoveUserContextCommandResult? result = JsonSerializer.Deserialize<RemoveUserContextCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        RemoveUserContextCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
