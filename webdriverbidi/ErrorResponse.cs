namespace WebDriverBidi;

using Newtonsoft.Json;

[JsonObject]
public class ErrorResponse
{
    private string error = string.Empty;
    private string message = string.Empty;

    private Dictionary<string, object?> additionalErrorData = new Dictionary<string, object?>();

    [JsonProperty("error")]
    public string ErrorType { get => this.error; internal set => this.error = value; }

    [JsonProperty("message")]
    public string ErrorMessage { get => this.message; internal set => this.message = value; }

    [JsonProperty("stacktrace", NullValueHandling = NullValueHandling.Ignore)]
    public string? StackTrace { get; internal set; }

    public Dictionary<string, object?> AdditionalData => this.additionalErrorData;
}