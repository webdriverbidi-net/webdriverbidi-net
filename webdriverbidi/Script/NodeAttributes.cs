namespace WebDriverBidi.Script;

using System.Collections.ObjectModel;

public class NodeAttributes : ReadOnlyDictionary<string, string>
{
    public NodeAttributes(Dictionary<string, string> dictionary) : base(dictionary)
    {
    }
}