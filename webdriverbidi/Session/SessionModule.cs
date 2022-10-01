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

    public async Task Subscribe(SubscribeCommandProperties commandProperties)
    {
        await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    public async Task Unsubscribe(UnsubscribeCommandProperties commandProperties)
    {
        await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }
}