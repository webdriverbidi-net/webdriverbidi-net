namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class RegularExpressionProperties
{
    private string pattern;
    private string? flags;

    [JsonConstructor]
    public RegularExpressionProperties(string pattern) : this(pattern, null)
    {
    }

    public RegularExpressionProperties(string pattern, string? flags)
    {
        this.pattern = pattern;
        this.flags = flags;
    }

    [JsonProperty("pattern")]
    [JsonRequired]
    public string Pattern { get => this.pattern; internal set => this.pattern = value; }

    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public string? Flags { get => this.flags; internal set => this.flags = value; }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        RegularExpressionProperties? other = obj as RegularExpressionProperties;
        if (other is null)
        {
            return false;
        }

        bool areEqual = this.pattern == other.pattern;
        if (this.flags is null && other.flags is null)
        {
            return areEqual;
        }

        return areEqual && this.flags == other.flags;
    }
}