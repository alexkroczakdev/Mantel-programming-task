namespace MantelLogAnalyser;
public record LogAnalysisResult(
    int UniqueIps,
    IReadOnlyList<string> TopVisitedUrls,
    IReadOnlyList<string> TopActiveIps
);
