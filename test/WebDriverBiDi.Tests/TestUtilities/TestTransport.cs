namespace WebDriverBiDi.TestUtilities;

using System.Threading.Tasks;
using Protocol;
using WebDriverBiDi;

public class TestTransport : Transport
{
    private TimeSpan messageProcessingDelay = TimeSpan.Zero;

    public TestTransport(Connection connection) : base(connection)
    {
    }

    public long LastTestCommandId => this.LastCommandId;

    public bool ReturnCustomValue { get; set; }

    public TimeSpan MessageProcessingDelay { get => this.messageProcessingDelay; set => this.messageProcessingDelay = value; }

    public CommandResult? CustomReturnValue { get; set; }

    public override async Task<Command> SendCommandAsync(CommandParameters commandParameters)
    {
        if (this.ReturnCustomValue)
        {
            Command returnedCommand = new Command(LastCommandId, commandParameters)
            {
                Result = this.CustomReturnValue
            };

            return returnedCommand;
        }

        return await base.SendCommandAsync(commandParameters);
    }
}
