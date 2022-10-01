namespace WebDriverBidi;

public class WebDriverBidiException : Exception
{
    public WebDriverBidiException(string message) : base(message)
    {
    }

    public WebDriverBidiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}