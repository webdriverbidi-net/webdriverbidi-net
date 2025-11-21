namespace WebDriverBiDi.Protocol;

[TestFixture]
public class UnhandledErrorCollectionTests()
{
    [Test]
    public void TestDefaultPropertyValues()
    {
        UnhandledErrorCollection unhandledErrors = new();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.ProtocolErrorBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
            Assert.That(unhandledErrors.UnexpectedErrorBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
            Assert.That(unhandledErrors.UnknownMessageBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
            Assert.That(unhandledErrors.EventHandlerExceptionBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate), Is.False);
            Assert.That(() => unhandledErrors.Exceptions, Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("No unhandled errors."));
        }
    }

    [Test]
    public void TestAddingUnhandledErrorWithIgnoreDoesNotAddErrorToCollection()
    {
        UnhandledErrorCollection unhandledErrors = new();
        unhandledErrors.AddUnhandledError(UnhandledErrorType.ProtocolError, new WebDriverBiDiException("new exception"));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate), Is.False);
        }
    }

    [Test]
    public void TestAddingUnhandledErrorWithCollect()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorType.ProtocolError, new WebDriverBiDiException("new exception"));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.True);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate), Is.False);
        }
    }

    [Test]
    public void TestAddingUnhandledErrorWithTerminate()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Terminate
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorType.ProtocolError, new WebDriverBiDiException("new exception"));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate), Is.True);
        }
    }


    [Test]
    public void TestClearingUnhandledErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorType.ProtocolError, new WebDriverBiDiException("new exception"));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.True);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate), Is.False);
        }
        unhandledErrors.ClearUnhandledErrors();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate), Is.False);
        }
    }

    [Test]
    public void TestCanGetExceptions()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorType.ProtocolError, new WebDriverBiDiException("new exception"));
        Assert.That(unhandledErrors.Exceptions, Has.Count.EqualTo(1));
        Assert.That(unhandledErrors.Exceptions[0], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("new exception"));
    }

    [Test]
    public void TestCanCaptureErrorsForDifferentBehaviorTypes()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorType.ProtocolError, new WebDriverBiDiException("invalid protocol message"));
        unhandledErrors.AddUnhandledError(UnhandledErrorType.UnexpectedError, new WebDriverBiDiException("unexpected error"));
        unhandledErrors.AddUnhandledError(UnhandledErrorType.EventHandlerException, new WebDriverBiDiException("event handler"));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore), Is.False);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.True);
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate), Is.True);
            Assert.That(unhandledErrors.Exceptions, Has.Count.EqualTo(2));
            Assert.That(unhandledErrors.Exceptions[0], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("invalid protocol message"));
            Assert.That(unhandledErrors.Exceptions[1], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("unexpected error"));
        }
    }
}