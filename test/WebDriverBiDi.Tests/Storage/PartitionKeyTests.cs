namespace WebDriverBiDi.Storage;

using System.Text.Json;

[TestFixture]
public class PartitionKeyTests
{
    [Test]
    public void TestCanDeserializePartitionKey()
    {
        string json = "{}";
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserContextId, Is.Null);
            Assert.That(result.SourceOrigin, Is.Null);
            Assert.That(result.AdditionalData, Is.Empty);
        }
    }

    [Test]
    public void TestCanDeserializePartitionKeyWithUserContext()
    {
        string json = """
                      {
                        "userContext": "myUserContext"
                      }
                      """;
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserContextId, Is.EqualTo("myUserContext"));
            Assert.That(result.SourceOrigin, Is.Null);
            Assert.That(result.AdditionalData, Is.Empty);
        }
    }

    [Test]
    public void TestCanDeserializePartitionKeyWithSourceOrigin()
    {
        string json = """
                      {
                        "sourceOrigin": "mySourceOrigin"
                      }
                      """;
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserContextId, Is.Null);
            Assert.That(result.SourceOrigin, Is.EqualTo("mySourceOrigin"));
            Assert.That(result.AdditionalData, Is.Empty);
        }
    }

    [Test]
    public void TestCanDeserializePartitionKeyWithAdditionalData()
    {
        string json = """
                      {
                        "extraData": "myExtraData"
                      }
                      """;
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserContextId, Is.Null);
            Assert.That(result.SourceOrigin, Is.Null);
            Assert.That(result.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(result.AdditionalData, Contains.Key("extraData"));
            Assert.That(result.AdditionalData["extraData"]!.GetType, Is.EqualTo(typeof(string)));
            Assert.That(result.AdditionalData["extraData"]!, Is.EqualTo("myExtraData"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = "{}";
        PartitionKey? result = JsonSerializer.Deserialize<PartitionKey>(json);
        Assert.That(result, Is.Not.Null);
        PartitionKey copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
