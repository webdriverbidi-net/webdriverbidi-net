namespace WebDriverBiDi.Protocol;

using System.Text.Json;

[TestFixture]
public class ErrorReceivedEventArgsTests
{
    public void TestCanCreateErrorReceivedEventArgs()
    {
        string json = """
                      {
                        "id": 123,
                        "type": "error",
                        "error": "my error code",
                        "message": "error message",
                        "stacktrace": "stack trace"
                      }
                      """;
        ErrorResponseMessage? errorMessage = JsonSerializer.Deserialize<ErrorResponseMessage>(json)!;
        ErrorReceivedEventArgs eventArgs = new(errorMessage.GetErrorResponseData());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.ErrorData, Is.Not.Null);
            Assert.That(eventArgs.ErrorData.IsError, Is.True);
            Assert.That(eventArgs.ErrorData.ErrorType, Is.EqualTo("my error code"));
            Assert.That(eventArgs.ErrorData.ErrorMessage, Is.EqualTo("error message"));
            Assert.That(eventArgs.ErrorData.StackTrace, Is.EqualTo("stack trace"));
        };
    }

    [Test]
    public void TestCreateErrorReceivedEventArgsWithNullErrorDataThrows()
    {
#pragma warning disable CS8625 // Converting null literal or possible null value to non-nullable type.
        Assert.That(() => new ErrorReceivedEventArgs(null), Throws.ArgumentNullException);
#pragma warning restore CS8625 // Converting null literal or possible null value to non-nullable type.
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "id": 123,
                        "type": "error",
                        "error": "my error code",
                        "message": "error message",
                        "stacktrace": "stack trace"
                      }
                      """;
        ErrorResponseMessage? errorMessage = JsonSerializer.Deserialize<ErrorResponseMessage>(json)!;
        ErrorReceivedEventArgs eventArgs = new(errorMessage.GetErrorResponseData());
        ErrorReceivedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));        
    }
}
