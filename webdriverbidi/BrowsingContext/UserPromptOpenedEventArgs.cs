namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class UserPromptOpenedEventArgs : EventArgs
{
    private string browsingContextId;
    private UserPromptType promptType;
    private string message;

    [JsonConstructor]
    public UserPromptOpenedEventArgs(string browsingContextId, UserPromptType promptType, string message)
    {
        this.browsingContextId = browsingContextId;
        this.promptType = promptType;
        this.message = message;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    public UserPromptType PromptType { get => this.promptType; internal set => this.promptType = value; }

    [JsonProperty("message")]
    [JsonRequired]
    public string Message { get => this.message; internal set => this.message = value; }

    [JsonProperty("type")]
    [JsonRequired]
    internal string SerializablePromptType
    {
        get { return this.promptType.ToString().ToLowerInvariant(); }
        set
        {
            UserPromptType type;
            if (!Enum.TryParse<UserPromptType>(value, true, out type))
            {
                throw new WebDriverBidiException($"Invalid value for user prompt type: '{value}'");
            }

            this.promptType = type;
        }
    }
}
