namespace MantelLogAnalyser;
public interface ILogAnalyser
{
    int GetNumberOfUniqueIPs(IEnumerable<LogEntry> logEntries);

    IReadOnlyList<string> GetMostVisitedUrls(IEnumerable<LogEntry> logEntries);

    IReadOnlyList<string> GetMostActiveIPs(IEnumerable<LogEntry> logEntries);
}