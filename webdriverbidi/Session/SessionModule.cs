namespace WebDriverBidi.Session;

public sealed class SessionModule : ProtocolModule
{
    public SessionModule(Driver driver) : base(driver)
    {
    }

    public async Task<StatusCommandResult> Status(StatusCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<StatusCommandResult>(commandProperties);
    }

    public async Task<NewCommandResult> NewSession(NewCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<NewCommandResult>(commandProperties);
    }

    public async Task<EmptyResult> Subscribe(SubscribeCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    public async Task<EmptyResult> Unsubscribe(UnsubscribeCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }
}