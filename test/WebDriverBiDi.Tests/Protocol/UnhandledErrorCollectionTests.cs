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

    [Test]
    public void TestSettingUnknownMessageBehaviorCollectsErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            UnknownMessageBehavior = TransportErrorBehavior.Collect
        };
        Assert.That(unhandledErrors.UnknownMessageBehavior, Is.EqualTo(TransportErrorBehavior.Collect));
        unhandledErrors.AddUnhandledError(UnhandledErrorType.UnknownMessage, new WebDriverBiDiException("unknown message"));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.True);
            Assert.That(unhandledErrors.Exceptions, Has.Count.EqualTo(1));
            Assert.That(unhandledErrors.Exceptions[0], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("unknown message"));
        }
    }

    [Test]
    public void TestSettingEventHandlerExceptionBehaviorCollectsErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect
        };
        Assert.That(unhandledErrors.EventHandlerExceptionBehavior, Is.EqualTo(TransportErrorBehavior.Collect));
        unhandledErrors.AddUnhandledError(UnhandledErrorType.EventHandlerException, new WebDriverBiDiException("event handler exception"));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect), Is.True);
            Assert.That(unhandledErrors.Exceptions, Has.Count.EqualTo(1));
            Assert.That(unhandledErrors.Exceptions[0], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("event handler exception"));
        }
    }

    [Test]
    public void TestTryGetExceptionsReturnsFalseForIgnoreBehavior()
    {
        UnhandledErrorCollection unhandledErrors = new();
        bool result = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Ignore, out IList<Exception> exceptions);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(exceptions, Is.Empty);
        }
    }

    [Test]
    public void TestTryGetExceptionsReturnsFalseWhenNoMatchingErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        bool result = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Terminate, out IList<Exception> exceptions);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(exceptions, Is.Empty);
        }
    }

    [Test]
    public void TestTryGetExceptionsReturnsMatchingExceptions()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorType.ProtocolError, new WebDriverBiDiException("collected error"));
        unhandledErrors.AddUnhandledError(UnhandledErrorType.UnexpectedError, new WebDriverBiDiException("terminal error"));

        bool collectResult = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Collect, out IList<Exception> collectExceptions);
        bool terminateResult = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Terminate, out IList<Exception> terminateExceptions);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(collectResult, Is.True);
            Assert.That(collectExceptions, Has.Count.EqualTo(1));
            Assert.That(collectExceptions[0], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("collected error"));
            Assert.That(terminateResult, Is.True);
            Assert.That(terminateExceptions, Has.Count.EqualTo(1));
            Assert.That(terminateExceptions[0], Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("terminal error"));
        }
    }
}
