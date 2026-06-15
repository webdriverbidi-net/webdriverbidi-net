namespace WebDriverBiDi.Script;

using System.Text.Json;

public class EvaluateResultExceptionTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "type": "exception",
                        "realm": "myRealm",
                        "exceptionDetails": {
                          "text": "exception thrown",
                          "lineNumber": 1,
                          "columnNumber": 5,
                          "stackTrace": {
                            "callFrames": []
                          },
                          "exception": {
                            "type": "string",
                            "value": "exception value"
                          }
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(result);
        Assert.IsType<EvaluateResultException>(result);
        EvaluateResultException exceptionResult = (EvaluateResultException)result;

        Assert.Equal("myRealm", exceptionResult.RealmId);
        Assert.Equal("exception thrown", exceptionResult.ExceptionDetails.Text);
        Assert.Equal(1, exceptionResult.ExceptionDetails.LineNumber);
        Assert.Equal(5, exceptionResult.ExceptionDetails.ColumnNumber);
        Assert.Empty(exceptionResult.ExceptionDetails.StackTrace.CallFrames);
        Assert.Equal("exception value", exceptionResult.ExceptionDetails.Exception.ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "exception",
                        "realm": "myRealm",
                        "exceptionDetails": {
                          "text": "exception thrown",
                          "lineNumber": 1,
                          "columnNumber": 5,
                          "stackTrace": {
                            "callFrames": []
                          },
                          "exception": {
                            "type": "string",
                            "value": "exception value"
                          }
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(result);
        Assert.IsType<EvaluateResultException>(result);
        EvaluateResultException exceptionResult = (EvaluateResultException)result;
        EvaluateResultException copy = exceptionResult with { };
        Assert.Equal(exceptionResult, copy);
    }
}
