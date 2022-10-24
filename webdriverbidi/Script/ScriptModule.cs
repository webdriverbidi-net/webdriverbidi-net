namespace WebDriverBidi.Script;

public sealed class ScriptModule : ProtocolModule
{
    public ScriptModule(Driver driver) : base(driver)
    {
        this.RegisterEventInvoker("script.realmCreated", typeof(RealmInfo), this.OnRealmCreated);
        this.RegisterEventInvoker("script.realmDestroyed", typeof(RealmDestroyedEventArgs), this.OnRealmDestroyed);
    }

    public event EventHandler<RealmCreatedEventArgs>? RealmCreated;

    public event EventHandler<RealmDestroyedEventArgs>? RealmDestroyed;

    public async Task<AddPreloadScriptCommandResult> AddPreloadScript(AddPreloadScriptCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<AddPreloadScriptCommandResult>(commandProperties);
    }

    public async Task<ScriptEvaluateResult> CallFunction(CallFunctionCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<ScriptEvaluateResult>(commandProperties);
    }

    public async Task<EmptyResult> Disown(DisownCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    public async Task<ScriptEvaluateResult> Evaluate(EvaluateCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<ScriptEvaluateResult>(commandProperties);
    }

    public async Task<GetRealmsCommandResult> GetRealms(GetRealmsCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<GetRealmsCommandResult>(commandProperties);
    }

    public async Task<EmptyResult> RemovePreloadScript(RemovePreloadScriptCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    private void OnRealmCreated(object eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a RealmInfo object, so rather than duplicate
        // the properties to directly deserialize the RealmCreatedEventArgs
        // instance, the protocol transport will deserialize to a RealmInfo,
        // then use that here to create the appropriate EventArgs instance.
        var eventArgs = eventData as RealmInfo;
        if (eventArgs is null)
        {
            throw new WebDriverBidiException("Unable to cast event data to RealmInfo");
        }

        RealmCreatedEventArgs e = new RealmCreatedEventArgs(eventArgs);
        if (this.RealmCreated is not null)
        {
            this.RealmCreated(this, e);
        }
    }

    private void OnRealmDestroyed(object eventData)
    {
        var eventArgs = eventData as RealmDestroyedEventArgs;
        if (eventArgs is null)
        {
            throw new WebDriverBidiException("Unable to cast event data to RealmDestroyedEventArgs");
        }

        if (this.RealmDestroyed is not null)
        {
            this.RealmDestroyed(this, eventArgs);
        }
    }
}