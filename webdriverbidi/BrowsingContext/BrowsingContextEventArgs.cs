namespace WebDriverBidi.BrowsingContext;

public class BrowsingContextEventArgs : EventArgs
{
    private readonly BrowsingContextInfo info;

    public BrowsingContextEventArgs(BrowsingContextInfo info)
    {
        this.info = info;
    }

    public string BrowsingContextId => this.info.BrowsingContextId;

    public string Url => this.info.Url;

    public IList<BrowsingContextInfo> Children => this.info.Children;

    public string? Parent => this.info.Parent;
}