namespace WebDriverBiDi.TestUtilities;

public class TestValidEventData
{
    private readonly string name;

    public TestValidEventData(string name)
    {
        this.name = name;
    }

    public string Name => this.name;
}
