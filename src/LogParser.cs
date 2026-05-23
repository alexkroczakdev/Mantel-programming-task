using System.Globalization;
using System.Text.RegularExpressions;

namespace MantelLogAnalyser;
public partial class LogParser : ILogParser
{
    [GeneratedRegex(@"
        ^(?<ip>\S+)[ ]                      # client IP
        \S+[ ] \S+[ ]                       # ident, user (not needed)
        \[[^\]]+\][ ]                       # timestamp (not needed)
        ""\S+[ ](?<url>\S+)[^""]*""[ ]      # method, URL, protocol
        (?<status>\d{3})                    # HTTP status code
        ",
        RegexOptions.CultureInvariant
        | RegexOptions.ExplicitCapture
        | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex LogPattern { get; }

    public LogEntry? ParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        var match = LogPattern.Match(line);
        if (!match.Success) return null;

        return new LogEntry(
            IpAddress:   match.Groups["ip"].Value,
            RequestPath: match.Groups["url"].Value,
            StatusCode:  int.Parse(match.Groups["status"].Value, CultureInfo.InvariantCulture)
        );
    }

    public IEnumerable<LogEntry> ParseFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var entry = ParseLine(line);
            if (entry != null)
                yield return entry;
        }
    }
}
