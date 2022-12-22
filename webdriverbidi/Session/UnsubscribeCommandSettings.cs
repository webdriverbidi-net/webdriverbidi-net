namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class UnsubscribeCommandSettings : SubscribeCommandSettings
{
    public UnsubscribeCommandSettings()
    {
    }

    public override string MethodName => "session.unsubscribe";
}