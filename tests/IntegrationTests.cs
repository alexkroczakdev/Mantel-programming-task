namespace MantelLogAnalyser.Tests;

public class SampleLogFixture
{
    public List<LogEntry> Entries { get; } =
        [.. new LogParser().ParseFile("programming-task-example-data.log")];
}

public class IntegrationTests(SampleLogFixture fixture) : IClassFixture<SampleLogFixture>
{
    private readonly List<LogEntry> _entries = fixture.Entries;

    [Fact]
    public void ParserAndAnalyser_SampleLogFile_ReturnsExpectedUniqueIpCount()
    {
        var result = new LogAnalyser().GetNumberOfUniqueIPs(_entries);

        Assert.Equal(11, result);
    }

    [Fact]
    public void ParserAndAnalyser_SampleLogFile_ReturnsExpectedTopVisitedUrls()
    {
        var result = new LogAnalyser().GetMostVisitedUrls(_entries);

        Assert.Equal(3, result.Count);
        Assert.Equal("/docs/manage-websites/", result[0]);
        Assert.Equal("/intranet-analytics/", result[1]);
        Assert.Equal("http://example.net/faq/", result[2]);
    }

    [Fact]
    public void ParserAndAnalyser_SampleLogFile_ReturnsExpectedTopActiveIps()
    {
        var result = new LogAnalyser().GetMostActiveIPs(_entries);

        Assert.Equal(3, result.Count);
        Assert.Equal("168.41.191.40", result[0]);
        Assert.Equal("177.71.128.21", result[1]);
        Assert.Equal("50.112.00.11", result[2]);
    }

    [Fact]
    public void FullPipeline_SampleLogFile_ReportContainsExpectedOutput()
    {
        var analyser = new LogAnalyser();
        var result = new LogAnalysisResult(
            UniqueIps:      analyser.GetNumberOfUniqueIPs(_entries),
            TopVisitedUrls: analyser.GetMostVisitedUrls(_entries),
            TopActiveIps:   analyser.GetMostActiveIPs(_entries));

        var report = new LogReporter().Report(result);

        Assert.Contains("11", report);
        Assert.Contains("/docs/manage-websites/", report);
        Assert.Contains("168.41.191.40", report);
    }

    [Fact]
    public void ParseFile_MissingFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(
            () => new LogParser().ParseFile("does-not-exist.log").ToList());
    }
}
