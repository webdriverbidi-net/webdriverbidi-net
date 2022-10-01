namespace WebDriverBidi;

public class DataReceivedEventArgs : EventArgs
{
    private string data;

    public DataReceivedEventArgs(string data)
    {
        this.data = data;
    }

    public string Data => this.data;
}