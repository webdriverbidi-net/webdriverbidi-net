namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StatusCommandSettings : CommandSettings
{
    public override string MethodName => "session.status";

    public StatusCommandSettings()
    {
    }
}