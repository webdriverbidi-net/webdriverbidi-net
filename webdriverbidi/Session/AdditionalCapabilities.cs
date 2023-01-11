namespace WebDriverBidi.Session;

using System.Collections.ObjectModel;

public class AdditionalCapabilities : ReadOnlyDictionary<string, object?>
{
    public AdditionalCapabilities(Dictionary<string, object?> capabilities) : base(capabilities)
    {
    }

    public static AdditionalCapabilities EmptyAdditionalCapabilities => new(new Dictionary<string, object?>());
}