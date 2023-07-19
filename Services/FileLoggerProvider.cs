namespace RedmineApp.Services;

public class FileLoggerProvider : ILoggerProvider
{
    string path;
    public FileLoggerProvider(string path)
    {
        this.path = path;
    }
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLoggerService(path);
    }
 
    public void Dispose() {}
}