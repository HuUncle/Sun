namespace Sun.Logging
{
    public interface ILogStore
    {
        void Post(LogEntry entry);
    }
}