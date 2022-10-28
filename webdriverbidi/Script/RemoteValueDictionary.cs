namespace WebDriverBidi.Script;

using System.Collections.ObjectModel;

public class RemoteValueDictionary : ReadOnlyDictionary<object, RemoteValue>
{
    internal RemoteValueDictionary(Dictionary<object, RemoteValue> dictionary) : base(dictionary)
    {
    }
}