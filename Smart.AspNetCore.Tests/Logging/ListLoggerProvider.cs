namespace Smart.AspNetCore.Logging;

using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

// ReSharper disable once NotAccessedPositionalProperty.Global
internal sealed record LogEntry(LogLevel Level, EventId EventId, string Message);

internal sealed class ListLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> entries = new();

    public IReadOnlyCollection<LogEntry> Entries => entries;

    public ILogger CreateLogger(string categoryName) => new ListLogger(entries);

    public void Dispose()
    {
    }

    private sealed class ListLogger : ILogger
    {
        private readonly ConcurrentQueue<LogEntry> entries;

        public ListLogger(ConcurrentQueue<LogEntry> entries)
        {
            this.entries = entries;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            entries.Enqueue(new LogEntry(logLevel, eventId, formatter(state, exception)));
        }
    }
}
