namespace WebDriverBidi.TestUtilities;

using System.Threading.Tasks;
using Protocol;
using WebDriverBidi;

public class TestTransport : Transport
{
    public TestTransport(TimeSpan commandWaitTimeout, Connection connection) : base(commandWaitTimeout, connection)
    {
    }

    public long LastTestCommandId => this.LastCommandId;

    public bool ReturnCustomValue { get; set; }

    public CommandResult? CustomReturnValue { get; set; }

    public bool AddTestCommand(Command testPendingCommand)
    {
        return this.AddPendingCommand(testPendingCommand);
    }

    public override async Task<CommandResult> SendCommandAndWait(CommandParameters command)
    {
        if (this.ReturnCustomValue)
        {
            return this.CustomReturnValue!;
        }

        return await base.SendCommandAndWait(command);
    }
}