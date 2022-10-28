namespace WebDriverBidi.Script;

using System.Collections.ObjectModel;

public class RemoteValueList : ReadOnlyCollection<RemoteValue>
{
    internal RemoteValueList(List<RemoteValue> list) : base(list)
    {
    }
}