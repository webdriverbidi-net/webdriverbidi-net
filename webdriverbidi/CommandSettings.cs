namespace WebDriverBidi;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public abstract class CommandSettings
{
    public abstract string MethodName { get; }

    public abstract Type ResultType { get; }
}