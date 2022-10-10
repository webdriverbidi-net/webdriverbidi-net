namespace WebDriverBidi.Script;

public class RealmCreatedEventArgs : EventArgs
{
    private RealmInfo info;

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
            WindowRealmInfo? windowRealm = this.info as WindowRealmInfo;
            if (windowRealm is null)
            {
                return null;
            }

            return windowRealm.BrowsingContext;
        }
    }

    public RealmType Type { get => this.info.Type; }
}
