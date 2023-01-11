namespace WebDriverBidi.Script;

public class RealmCreatedEventArgs : EventArgs
{
    private readonly RealmInfo info;

    public RealmCreatedEventArgs(RealmInfo info)
    {
        this.info = info;
    }

    public string RealmId { get => this.info.RealmId; }

    public string Origin { get => this.info.Origin; }

    public string? BrowsingContext
    {
        get
        {
            if (this.info is not WindowRealmInfo windowRealm)
            {
                return null;
            }

            return windowRealm.BrowsingContext;
        }
    }

    public RealmType Type { get => this.info.Type; }
}
