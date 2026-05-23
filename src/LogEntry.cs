namespace MantelLogAnalyser;
public record LogEntry(
    string IpAddress,
    string RequestPath,
    int StatusCode
);
