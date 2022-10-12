namespace WebDriverBidi.Session;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StatusCommandSettings : CommandSettings
{
    public StatusCommandSettings()
    {
    }

    public override string MethodName => "session.status";

    public override Type ResultType => typeof(StatusCommandResult);
}