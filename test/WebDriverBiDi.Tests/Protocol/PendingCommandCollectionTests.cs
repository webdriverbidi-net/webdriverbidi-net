namespace WebDriverBiDi.Protocol;

using TestUtilities;

[TestFixture]
public class PendingCommandCollectionTests
{
    [Test]
    public async Task TestCanAddCommand()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        Assert.That(collection.PendingCommandCount, Is.EqualTo(0));

        await collection.AddPendingCommandAsync(testCommand);
        Assert.That(collection.PendingCommandCount, Is.EqualTo(1));
        Assert.That(collection.RemovePendingCommand(1, out Command removedCommand), Is.True);
        Assert.That(removedCommand.CommandName, Is.EqualTo("module.command"));
    }

    [Test]
    public async Task TestCanRemoveCommand()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand);

        Assert.That(collection.RemovePendingCommand(1, out Command removedCommand), Is.True);
        Assert.That(removedCommand.CommandName, Is.EqualTo("module.command"));
    }

    [Test]
    public async Task TestCanClearCollection()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand);
        await collection.CloseAsync();
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
    public async Task TestCannotAddCommandToClosedCollection()
    {
        PendingCommandCollection collection = new();
        await collection.CloseAsync();
        Assert.That(async () => await collection.AddPendingCommandAsync(new Command(1, new TestCommandParameters("test.command"))), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Cannot add command; pending command collection is closed"));
    }

    [Test]
    public async Task TestCannotAddCommandWithDuplicateId()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand);
        Assert.That(async () => await collection.AddPendingCommandAsync(new Command(1, new TestCommandParameters("test.command"))), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Could not add command with id 1, as id already exists"));
    }

    [Test]
    public async Task TestCanRemoveCommandFromClosedCollection()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand);
        await collection.CloseAsync();
        Assert.That(collection.PendingCommandCount, Is.EqualTo(1));
        Assert.That(collection.RemovePendingCommand(1, out Command removedCommand), Is.True);
        Assert.That(removedCommand.CommandName, Is.EqualTo("module.command"));
    }

    [Test]
    public async Task TestCannotClearCollectionUnlessClosed()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand);
        Assert.That(() => collection.Clear(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Cannot clear the collection while it can accept new incoming commands; close it with the Close method first"));
    }

    [Test]
    public async Task TestIsAcceptingCommandsPropertyHasCorrectValue()
    {
        PendingCommandCollection collection = new();
        Assert.That(collection.IsAcceptingCommands, Is.True);
        await collection.CloseAsync();
        Assert.That(collection.IsAcceptingCommands, Is.False);
    }

    [Test]
    public void TestCanDispose()
    {
        PendingCommandCollection collection = new();
        Assert.That(() => collection.Dispose(), Throws.Nothing);
    }

    [Test]
    public void TestDoubleDisposeDoesNotThrow()
    {
        PendingCommandCollection collection = new();
        collection.Dispose();
        Assert.That(() => collection.Dispose(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeAfterUse()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand);
        collection.RemovePendingCommand(1, out _);
        await collection.CloseAsync();
        collection.Clear();
        Assert.That(() => collection.Dispose(), Throws.Nothing);
    }
}
