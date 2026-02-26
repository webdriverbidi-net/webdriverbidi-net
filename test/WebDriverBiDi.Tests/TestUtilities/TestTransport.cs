namespace WebDriverBiDi.TestUtilities;

using System.Reflection;
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

    public bool CancelCommand { get; set; }

    public bool ReturnUncompletedCommand { get; set; }

    public TimeSpan MessageProcessingDelay { get => this.messageProcessingDelay; set => this.messageProcessingDelay = value; }

    public CommandResult? CustomReturnValue { get; set; }

    public override async Task<Command> SendCommandAsync(CommandParameters commandParameters)
    {
        if (this.ReturnUncompletedCommand)
        {
            return new TestCommand(LastCommandId, commandParameters);
        }

        if (this.CancelCommand)
        {
            Command returnedCommand = new Command(LastCommandId, commandParameters);
            returnedCommand.Cancel();
            return returnedCommand;
        }

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

    public Connection GetConnection()
    {
        return this.Connection;
    }

    /// <summary>
    /// Registers an arbitrary type for an event name, bypassing the normal
    /// EventMessage&lt;T&gt; wrapping. Used to test deserialization failure paths
    /// where the deserialized type is not an EventMessage.
    /// </summary>
    public void RegisterInvalidEventMessageType(string eventName, Type type)
    {
        this.AddEventMessageType(eventName, type);
    }
}
