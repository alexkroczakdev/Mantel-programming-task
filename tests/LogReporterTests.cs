namespace MantelLogAnalyser.Tests;

public class LogReporterTests
{
    private readonly ILogReporter _reporter = new LogReporter();

    [Fact]
    public void Report_ValidResult_ContainsUniqueIpCount()
    {
        // Arrange
        var result = new LogAnalysisResult(5, ["/a", "/b", "/c"], ["1.1.1.1", "2.2.2.2", "3.3.3.3"]);

        // Act
        var output = _reporter.Report(result);

        // Assert
        Assert.Contains("unique IP addresses: 5", output);
    }

    [Fact]
    public void Report_ValidResult_ContainsTopUrls()
    {
        // Arrange
        var result = new LogAnalysisResult(3, ["/home", "/about", "/contact"], ["1.1.1.1"]);

        // Act
        var output = _reporter.Report(result);

        // Assert
        Assert.Contains("/home", output);
        Assert.Contains("/about", output);
        Assert.Contains("/contact", output);
    }

    [Fact]
    public void Report_ValidResult_ContainsTopIps()
    {
        // Arrange
        var result = new LogAnalysisResult(2, ["/a"], ["10.0.0.1", "10.0.0.2"]);

        // Act
        var output = _reporter.Report(result);

        // Assert
        Assert.Contains("10.0.0.1", output);
        Assert.Contains("10.0.0.2", output);
    }

    [Fact]
    public void Report_EmptyUrlsAndIps_ReturnsWithoutThrowing()
    {
        // Arrange
        var result = new LogAnalysisResult(0, [], []);

        // Act
        var ex = Record.Exception(() => _reporter.Report(result));

        // Assert
        Assert.Null(ex);
    }
}
