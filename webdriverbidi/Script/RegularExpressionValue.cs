namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class RegularExpressionValue
{
    private string pattern;
    private string? flags;

    [JsonConstructor]
    public RegularExpressionValue(string pattern) : this(pattern, null)
    {
    }

    public RegularExpressionValue(string pattern, string? flags)
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

        if (obj is not RegularExpressionValue other)
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