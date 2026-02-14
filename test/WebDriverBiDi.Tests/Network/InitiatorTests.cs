namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class InitiatorTests
{
    [Test]
    public void TestCanDeserializeInitiator()
    {
        string json = """
                      {
                      }
                      """;
        Initiator? initiator = JsonSerializer.Deserialize<Initiator>(json);
        Assert.That(initiator, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(initiator.Type, Is.Null);
            Assert.That(initiator.ColumnNumber, Is.Null);
            Assert.That(initiator.LineNumber, Is.Null);
            Assert.That(initiator.StackTrace, Is.Null);
            Assert.That(initiator.RequestId, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeInitiatorWithOptionalValues()
    {
        string json = """
                      {
                        "type": "script",
                        "lineNumber": 2,
                        "columnNumber": 1,
                        "stackTrace": {
                          "callFrames": [
                            {
                              "functionName": "myFunction",
                              "lineNumber": 2,
                              "columnNumber": 1,
                              "url": "http://some.url/file.js" 
                            }
                          ] 
                        },
                        "request": "myRequestId"
                      }
                      """;
        Initiator? initiator = JsonSerializer.Deserialize<Initiator>(json);
        Assert.That(initiator, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(initiator.Type, Is.EqualTo(InitiatorType.Script));
            Assert.That(initiator.ColumnNumber, Is.EqualTo(1));
            Assert.That(initiator.LineNumber, Is.EqualTo(2));
            Assert.That(initiator.StackTrace, Is.Not.Null);
            Assert.That(initiator.StackTrace!.CallFrames, Has.Count.EqualTo(1));
            Assert.That(initiator.RequestId, Is.EqualTo("myRequestId"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                      }
                      """;
        Initiator? initiator = JsonSerializer.Deserialize<Initiator>(json);
        Assert.That(initiator, Is.Not.Null);
        Initiator copy = initiator with { };
        Assert.That(copy, Is.EqualTo(initiator));
    }

    [Test]
    public void TestDeserializingInitiatorWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Initiator>(json), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("value 'invalid' is not valid for enum type"));
    }
}
