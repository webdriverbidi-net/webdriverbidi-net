namespace WebDriverBidi;

public class WebDriverBidiEventData
{
    public WebDriverBidiEventData(Type eventArgsType, Action<object> eventInvoker)
    {
        this.EventArgsType = eventArgsType;
        this.EventInvoker = eventInvoker;
    }

    public Type EventArgsType { get; }

    public Action<object> EventInvoker { get; }
}