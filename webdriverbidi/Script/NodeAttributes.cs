namespace WebDriverBidi.Script;

using System.Collections.ObjectModel;

public class NodeAttributes : ReadOnlyDictionary<string, string>
{
    internal NodeAttributes(Dictionary<string, string> dictionary) : base(dictionary)
    {
    }
}