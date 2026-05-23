namespace MantelLogAnalyser;
public class LogAnalyser : ILogAnalyser
{
    public int GetNumberOfUniqueIPs(IEnumerable<LogEntry> logEntries) => logEntries.Select(x => x.IpAddress).Distinct().Count();

    public IReadOnlyList<string> GetMostVisitedUrls(IEnumerable<LogEntry> logEntries) => [.. logEntries
     .Where(e => e.StatusCode >= 200 && e.StatusCode < 300)
     .CountBy(e => e.RequestPath)
     .OrderByDescending(kvp => kvp.Value)
     .Take(3)
     .Select(kvp => kvp.Key)];

    public IReadOnlyList<string> GetMostActiveIPs(IEnumerable<LogEntry> logEntries) => [.. logEntries
     .CountBy(e => e.IpAddress)
     .OrderByDescending(kvp => kvp.Value)
     .Take(3)
     .Select(kvp => kvp.Key)];
}
