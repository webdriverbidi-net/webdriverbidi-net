namespace WebDriverBiDi.Protocol;

using TestUtilities;

[TestFixture]
public class PendingCommandCollectionTests
{
    [Test]
    public void TestCanAddCommand()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        Assert.That(collection.PendingCommandCount, Is.EqualTo(0));

        collection.AddPendingCommand(testCommand);
        Assert.That(collection.PendingCommandCount, Is.EqualTo(1));
        Assert.That(collection.RemovePendingCommand(1, out Command removedCommand), Is.True);
        Assert.That(removedCommand.CommandName, Is.EqualTo("module.command"));
    }

    [Test]
    public void TestCanRemoveCommand()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        collection.AddPendingCommand(testCommand);

        Assert.That(collection.RemovePendingCommand(1, out Command removedCommand), Is.True);
        Assert.That(removedCommand.CommandName, Is.EqualTo("module.command"));
    }

    [Test]
    public void TestCanClearCollection()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        collection.AddPendingCommand(testCommand);
        collection.Close();
        collection.Clear();
        Assert.That(collection.PendingCommandCount, Is.EqualTo(0));
    }

    [Test]
    public void TestCanAttemptToRemoveNonExistentCommand()
    {
        PendingCommandCollection collection = new();
        Assert.That(collection.RemovePendingCommand(1, out Command removedCommand), Is.False);
    }

    [Test]
    public void TestCannotAddCommandToClosedCollection()
    {
        PendingCommandCollection collection = new();
        collection.Close();
        Assert.That(() => collection.AddPendingCommand(new Command(1, new TestCommandParameters("test.command"))), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Cannot add command; pending command collection is closed"));
    }

    [Test]
    public void TestCannotAddCommandWithDuplicateId()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        collection.AddPendingCommand(testCommand);
        Assert.That(() => collection.AddPendingCommand(new Command(1, new TestCommandParameters("test.command"))), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Could not add command with id 1, as id already exists"));
    }

    [Test]
    public void TestCanRemoveCommandFromClosedCollection()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        collection.AddPendingCommand(testCommand);
        collection.Close();
        Assert.That(collection.PendingCommandCount, Is.EqualTo(1));
        Assert.That(collection.RemovePendingCommand(1, out Command removedCommand), Is.True);
        Assert.That(removedCommand.CommandName, Is.EqualTo("module.command"));
    }

    [Test]
    public void TestCannotClearCollectionUnlessClosed()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        collection.AddPendingCommand(testCommand);
        Assert.That(() => collection.Clear(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Cannot clear the collection while it can accept new incoming commands; close it with the Close method first"));
    }

    [Test]
    public void TestIsAcceptingCommandsPropertyHasCorrectValue()
    {
        PendingCommandCollection collection = new();
        Assert.That(collection.IsAcceptingCommands, Is.True);
        collection.Close();
        Assert.That(collection.IsAcceptingCommands, Is.False);
    }
}
