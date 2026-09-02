namespace WebDriverBiDi.Logging.TestUtilities;

using Microsoft.Extensions.Logging;
using WebDriverBiDi.Logging;

/// <summary>
/// Fake ILogger that captures Log calls for verification in tests.
/// </summary>
public sealed class TestLogger : ILogger<WebDriverBiDiEventSourceLogger>
{
    private readonly List<LogEntry> entries = new();

    /// <summary>
    /// Gets the captured log entries.
    /// </summary>
    public IReadOnlyList<LogEntry> Entries => this.entries;

    /// <summary>
    /// Gets or sets a value indicating whether this logger reports itself as enabled from
    /// <see cref="IsEnabled"/>. Defaults to <see langword="true"/>; set to <see langword="false"/> to
    /// simulate a category that has been filtered off.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => this.Enabled;

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        string message = formatter?.Invoke(state, exception) ?? string.Empty;
        lock (this.entries)
        {
            this.entries.Add(new LogEntry(logLevel, eventId, state, exception, message));
        }
    }

    /// <summary>
    /// Clears all captured entries.
    /// </summary>
    public void Clear()
    {
        lock (this.entries)
        {
            this.entries.Clear();
        }
    }

    /// <summary>
    /// Represents a captured log entry.
    /// </summary>
    /// <param name="LogLevel">The log level.</param>
    /// <param name="EventId">The event ID.</param>
    /// <param name="State">The log state.</param>
    /// <param name="Exception">The exception, if any.</param>
    /// <param name="Message">The formatted message.</param>
    public sealed record LogEntry(
        LogLevel LogLevel,
        EventId EventId,
        object? State,
        Exception? Exception,
        string Message);
}
