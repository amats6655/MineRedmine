namespace RedmineApp.Services;

public class FileLoggerService : ILogger, IDisposable
{
    string filePath;
    static object _lock = new object();
    public FileLoggerService(string path)
    {
        filePath = path;
    }
    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }
 
    public void Dispose() { }
 
    public bool IsEnabled(LogLevel logLevel)
    {
        //return logLevel == LogLevel.Trace;
        return true;
    }
 
    public void Log<TState>(LogLevel logLevel, EventId eventId,
        TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_lock)
        {
            File.AppendAllText(filePath, formatter(state, exception) + Environment.NewLine);
        }
    }
}