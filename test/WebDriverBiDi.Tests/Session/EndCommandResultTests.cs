namespace WebDriverBiDi.Session;

using System.Text.Json;

[TestFixture]
public class EndCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        EndCommandResult? result = JsonSerializer.Deserialize<EndCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        EndCommandResult? result = JsonSerializer.Deserialize<EndCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        EndCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}