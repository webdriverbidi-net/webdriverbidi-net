namespace WebDriverBiDi;

using System.Text.Json;
using WebDriverBiDi.Protocol;

[TestFixture]
public class ErrorResultTests
{
    [Test]
    public void TestCanCreateErrorResult()
    {
        // ErrorResult constructor is internal, so we will create it by
        // creating an ErrorResponseMessage and calling GetErrorResponseData
        // for it.
        string json = """
                      {
                        "type": "error",
                        "id": 1,
                        "error": "unknown error",
                        "message": "This is a test error message",
                        "stacktrace": "full stack trace"
                      }
                      """;
        ErrorResponseMessage? messageResult = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.That(messageResult, Is.Not.Null);
        ErrorResult result = messageResult.GetErrorResponseData();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo("unknown error"));
            Assert.That(result.ErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(result.AdditionalData, Is.Empty);
            Assert.That(result.StackTrace, Is.EqualTo("full stack trace"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        // ErrorResult constructor is internal, so we will create it by
        // creating an ErrorResponseMessage and calling GetErrorResponseData
        // for it.
        string json = """
                      {
                        "type": "error",
                        "id": 1,
                        "error": "unknown error",
                        "message": "This is a test error message",
                        "stacktrace": "full stack trace"
                      }
                      """;
        ErrorResponseMessage? messageResult = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.That(messageResult, Is.Not.Null);
        ErrorResult result = messageResult.GetErrorResponseData();
        ErrorResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}