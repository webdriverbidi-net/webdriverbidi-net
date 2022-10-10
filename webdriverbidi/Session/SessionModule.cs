namespace WebDriverBidi.Session;

public sealed class SessionModule : ProtocolModule
{
    public SessionModule(Driver driver) : base(driver)
    {
    }

    public async Task<StatusCommandResult> Status(StatusCommandProperties commandProperties)
    {
        return await this.Driver.ExecuteCommand<StatusCommandResult>(commandProperties);
    }

    public async Task<NewCommandResult> NewSession(NewCommandProperties commandProperties)
    {
        return await this.Driver.ExecuteCommand<NewCommandResult>(commandProperties);
    }

    public async Task<EmptyResult> Subscribe(SubscribeCommandProperties commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    public async Task<EmptyResult> Unsubscribe(UnsubscribeCommandProperties commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }
}