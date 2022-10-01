namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StatusCommandProperties : CommandProperties
{
    public override string MethodName => "session.status";

    public StatusCommandProperties()
    {
    }
}