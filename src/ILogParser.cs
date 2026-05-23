namespace MantelLogAnalyser;
public interface ILogParser
{
    LogEntry? ParseLine(string line);
    IEnumerable<LogEntry> ParseFile(string filePath);
}
