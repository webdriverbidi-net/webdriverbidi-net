namespace WebDriverBiDi.TestUtilities;

using System.Threading.Tasks;
using Protocol;
using WebDriverBiDi;

public class TestTransport : Transport
{
    private TimeSpan messageProcessingDelay = TimeSpan.Zero;

    public TestTransport(TimeSpan commandWaitTimeout, Connection connection) : base(commandWaitTimeout, connection)
    {
    }

    public long LastTestCommandId => this.LastCommandId;

    public bool ReturnCustomValue { get; set; }

    public TimeSpan MessageProcessingDelay { get => this.messageProcessingDelay; set => this.messageProcessingDelay = value; }

    public CommandResult? CustomReturnValue { get; set; }

    public bool AddTestCommand(Command testPendingCommand)
    {
        return this.AddPendingCommand(testPendingCommand);
    }

    public override async Task<CommandResult> SendCommandAndWaitAsync(CommandParameters command)
    {
        if (this.ReturnCustomValue)
        {
            return this.CustomReturnValue!;
        }

        return await base.SendCommandAndWaitAsync(command);
    }
}