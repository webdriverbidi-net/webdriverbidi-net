namespace WebDriverBiDi.Protocol;

public class UnhandledErrorCollectionTests()
{
    [Fact]
    public void TestDefaultPropertyValues()
    {
        UnhandledErrorCollection unhandledErrors = new();

        Assert.Equal(TransportErrorBehavior.Ignore, unhandledErrors.ProtocolErrorBehavior);
        Assert.Equal(TransportErrorBehavior.Ignore, unhandledErrors.UnexpectedErrorBehavior);
        Assert.Equal(TransportErrorBehavior.Ignore, unhandledErrors.UnknownMessageBehavior);
        Assert.Equal(TransportErrorBehavior.Ignore, unhandledErrors.EventHandlerExceptionBehavior);
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate));
        Assert.Equal("No unhandled errors.", Assert.ThrowsAny<InvalidOperationException>(() => unhandledErrors.Exceptions).Message);
    }

    [Fact]
    public void TestAddingUnhandledErrorWithIgnoreDoesNotAddErrorToCollection()
    {
        UnhandledErrorCollection unhandledErrors = new();
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.ProtocolError, new WebDriverBiDiException("new exception"));

        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate));
    }

    [Fact]
    public void TestAddingUnhandledErrorWithCollect()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.ProtocolError, new WebDriverBiDiException("new exception"));

        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore));
        Assert.True(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate));
    }

    [Fact]
    public void TestAddingUnhandledErrorWithTerminate()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Terminate
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.ProtocolError, new WebDriverBiDiException("new exception"));

        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.True(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate));
    }

    [Fact]
    public void TestClearingUnhandledErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.ProtocolError, new WebDriverBiDiException("new exception"));

        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore));
        Assert.True(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate));

        unhandledErrors.ClearUnhandledErrors();

        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate));
    }

    [Fact]
    public void TestCanGetExceptions()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.ProtocolError, new WebDriverBiDiException("new exception"));
        Assert.Single(unhandledErrors.Exceptions);
        Assert.IsType<WebDriverBiDiException>(unhandledErrors.Exceptions[0]);
        WebDriverBiDiException? typedException = unhandledErrors.Exceptions[0] as WebDriverBiDiException;
        Assert.NotNull(typedException);
        Assert.Equal("new exception", typedException.Message);
    }

    [Fact]
    public void TestCanCaptureErrorsForDifferentBehaviorTypes()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.ProtocolError, new WebDriverBiDiException("invalid protocol message"));
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.UnexpectedError, new WebDriverBiDiException("unexpected error"));
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.EventHandlerException, new WebDriverBiDiException("event handler"));

        Assert.False(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Ignore));
        Assert.True(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.True(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate));
        Assert.Equal(2, unhandledErrors.Exceptions.Count);

        Assert.IsType<WebDriverBiDiException>(unhandledErrors.Exceptions[0]);
        WebDriverBiDiException? exception0 = unhandledErrors.Exceptions[0] as WebDriverBiDiException;
        Assert.NotNull(exception0);
        Assert.Equal("invalid protocol message", exception0.Message);

        Assert.IsType<WebDriverBiDiException>(unhandledErrors.Exceptions[1]);
        WebDriverBiDiException? exception1 = unhandledErrors.Exceptions[1] as WebDriverBiDiException;
        Assert.NotNull(exception1);
        Assert.Equal("unexpected error", exception1.Message);
    }

    [Fact]
    public void TestSettingUnknownMessageBehaviorCollectsErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            UnknownMessageBehavior = TransportErrorBehavior.Collect
        };
        Assert.Equal(TransportErrorBehavior.Collect, unhandledErrors.UnknownMessageBehavior);
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.UnknownMessage, new WebDriverBiDiException("unknown message"));

        Assert.True(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.Single(unhandledErrors.Exceptions);
        Assert.IsType<WebDriverBiDiException>(unhandledErrors.Exceptions[0]);
        WebDriverBiDiException? unknownException = unhandledErrors.Exceptions[0] as WebDriverBiDiException;
        Assert.NotNull(unknownException);
        Assert.Equal("unknown message", unknownException.Message);
    }

    [Fact]
    public void TestSettingEventHandlerExceptionBehaviorCollectsErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect
        };
        Assert.Equal(TransportErrorBehavior.Collect, unhandledErrors.EventHandlerExceptionBehavior);
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.EventHandlerException, new WebDriverBiDiException("event handler exception"));

        Assert.True(unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect));
        Assert.Single(unhandledErrors.Exceptions);
        Assert.IsType<WebDriverBiDiException>(unhandledErrors.Exceptions[0]);
        WebDriverBiDiException? handlerException = unhandledErrors.Exceptions[0] as WebDriverBiDiException;
        Assert.NotNull(handlerException);
        Assert.Equal("event handler exception", handlerException.Message);
    }

    [Fact]
    public void TestTryGetExceptionsReturnsFalseForIgnoreBehavior()
    {
        UnhandledErrorCollection unhandledErrors = new();
        bool result = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Ignore, out IList<Exception> exceptions);

        Assert.False(result);
        Assert.Empty(exceptions);
    }

    [Fact]
    public void TestTryGetExceptionsReturnsFalseWhenNoMatchingErrors()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect
        };
        bool result = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Terminate, out IList<Exception> exceptions);

        Assert.False(result);
        Assert.Empty(exceptions);
    }

    [Fact]
    public void TestTryGetExceptionsReturnsMatchingExceptions()
    {
        UnhandledErrorCollection unhandledErrors = new()
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate
        };
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.ProtocolError, new WebDriverBiDiException("collected error"));
        unhandledErrors.AddUnhandledError(UnhandledErrorKind.UnexpectedError, new WebDriverBiDiException("terminal error"));

        bool collectResult = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Collect, out IList<Exception> collectExceptions);
        bool terminateResult = unhandledErrors.TryGetExceptions(TransportErrorBehavior.Terminate, out IList<Exception> terminateExceptions);

        Assert.True(collectResult);
        Assert.Single(collectExceptions);
        Assert.IsType<WebDriverBiDiException>(collectExceptions[0]);
        WebDriverBiDiException? collectedException = collectExceptions[0] as WebDriverBiDiException;
        Assert.NotNull(collectedException);
        Assert.Equal("collected error", collectedException.Message);

        Assert.True(terminateResult);
        Assert.Single(terminateExceptions);
        Assert.IsType<WebDriverBiDiException>(terminateExceptions[0]);
        WebDriverBiDiException? terminatedException = terminateExceptions[0] as WebDriverBiDiException;
        Assert.NotNull(terminatedException);
        Assert.Equal("terminal error", terminatedException.Message);
    }
}
