namespace MantelLogAnalyser;
public class LogReporter : ILogReporter
{
    public string Report(LogAnalysisResult result) =>
        $"The number of unique IP addresses: {result.UniqueIps}\n" +
        $"The top {(result.TopVisitedUrls.Count < 3 ? result.TopVisitedUrls.Count : 3)} most visited URLs: {string.Join(", ", result.TopVisitedUrls)}\n" +
        $"The top {(result.TopActiveIps.Count < 3 ? result.TopActiveIps.Count : 3)} most active IP addresses: {string.Join(", ", result.TopActiveIps)}";
}
