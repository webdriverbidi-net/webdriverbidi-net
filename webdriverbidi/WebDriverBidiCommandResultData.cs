namespace WebDriverBidi;

using Newtonsoft.Json.Linq;

public class WebDriverBidiCommandResultData
{
    public WebDriverBidiCommandResultData(JToken result, bool isError)
    {
        this.Result = result;
        this.IsError = isError;
    }

    public JToken Result { get; }

    public bool IsError { get; }
}