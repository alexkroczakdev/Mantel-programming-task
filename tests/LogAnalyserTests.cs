namespace MantelLogAnalyser.Tests;

public class LogAnalyserTests
{
    private readonly ILogAnalyser _analyser = new LogAnalyser();

    [Fact]
    public void GetNumberOfUniqueIPs_EmptyList_ReturnsZero()
    {
        // Act
        var result = _analyser.GetNumberOfUniqueIPs([]);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetNumberOfUniqueIPs_AllSameIp_ReturnsOne()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("192.168.1.1", "/", 200),
            new("192.168.1.1", "/about", 200),
            new("192.168.1.1", "/contact", 200),
        };

        // Act
        var result = _analyser.GetNumberOfUniqueIPs(entries);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetNumberOfUniqueIPs_AllDistinctIps_ReturnsCount()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/", 200),
            new("10.0.0.2", "/", 200),
            new("10.0.0.3", "/", 200),
        };

        // Act
        var result = _analyser.GetNumberOfUniqueIPs(entries);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetNumberOfUniqueIPs_MixedIps_ReturnsUniqueCount()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/", 200),
            new("10.0.0.1", "/about", 404),
            new("10.0.0.2", "/", 200),
            new("10.0.0.3", "/", 500),
            new("10.0.0.3", "/docs", 301),
        };

        // Act
        var result = _analyser.GetNumberOfUniqueIPs(entries);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetMostVisitedUrls_ReturnsTopThreeOrderedByVisits()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/a", 200),
            new("10.0.0.1", "/a", 200),
            new("10.0.0.1", "/a", 200),
            new("10.0.0.2", "/b", 200),
            new("10.0.0.2", "/b", 200),
            new("10.0.0.3", "/c", 200),
            new("10.0.0.3", "/c", 200),
            new("10.0.0.4", "/d", 200),
        };

        // Act
        var result = _analyser.GetMostVisitedUrls(entries);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("/a", result[0]);
        Assert.Equal("/b", result[1]);
        Assert.Equal("/c", result[2]);
    }

    [Fact]
    public void GetMostVisitedUrls_FewerThanThreeUrls_ReturnsAll()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/a", 200),
            new("10.0.0.2", "/b", 200),
        };

        // Act
        var result = _analyser.GetMostVisitedUrls(entries);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetMostVisitedUrls_ExcludesNon2xxResponses()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/error", 500),
            new("10.0.0.1", "/error", 500),
            new("10.0.0.1", "/error", 500),
            new("10.0.0.2", "/redirect", 301),
            new("10.0.0.2", "/redirect", 301),
            new("10.0.0.3", "/ok", 200),
        };

        // Act
        var result = _analyser.GetMostVisitedUrls(entries);

        // Assert
        Assert.Equal(["/ok"], result);
    }

    [Fact]
    public void GetMostVisitedUrls_EmptyList_ReturnsEmpty()
    {
        // Act
        var result = _analyser.GetMostVisitedUrls([]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetMostActiveIPs_ReturnsTopThreeOrderedByRequests()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/", 200),
            new("10.0.0.1", "/", 200),
            new("10.0.0.1", "/", 200),
            new("10.0.0.2", "/", 200),
            new("10.0.0.2", "/", 200),
            new("10.0.0.3", "/", 200),
            new("10.0.0.3", "/", 200),
            new("10.0.0.4", "/", 200),
        };

        // Act
        var result = _analyser.GetMostActiveIPs(entries);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("10.0.0.1", result[0]);
        Assert.Equal("10.0.0.2", result[1]);
        Assert.Equal("10.0.0.3", result[2]);
    }

    [Fact]
    public void GetMostActiveIPs_CountsAllStatusCodes()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/", 200),
            new("10.0.0.1", "/", 404),
            new("10.0.0.1", "/", 500),
            new("10.0.0.2", "/", 200),
        };

        // Act
        var result = _analyser.GetMostActiveIPs(entries);

        // Assert
        Assert.Equal("10.0.0.1", result[0]);
        Assert.Equal("10.0.0.2", result[1]);
    }

    [Fact]
    public void GetMostActiveIPs_FewerThanThreeIPs_ReturnsAll()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/", 200),
            new("10.0.0.2", "/", 200),
        };

        // Act
        var result = _analyser.GetMostActiveIPs(entries);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetMostActiveIPs_EmptyList_ReturnsEmpty()
    {
        // Act
        var result = _analyser.GetMostActiveIPs([]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetMostVisitedUrls_TiedCount_ReturnsFirstAppearanceInLog()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/first", 200),
            new("10.0.0.2", "/second", 200),
            new("10.0.0.3", "/third", 200),
            new("10.0.0.4", "/fourth", 200),
        };

        // Act
        var result = _analyser.GetMostVisitedUrls(entries);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("/first", result[0]);
        Assert.Equal("/second", result[1]);
        Assert.Equal("/third", result[2]);
    }

    [Fact]
    public void GetMostActiveIPs_TiedCount_ReturnsFirstAppearanceInLog()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            new("10.0.0.1", "/", 200),
            new("10.0.0.2", "/", 200),
            new("10.0.0.3", "/", 200),
            new("10.0.0.4", "/", 200),
        };

        // Act
        var result = _analyser.GetMostActiveIPs(entries);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("10.0.0.1", result[0]);
        Assert.Equal("10.0.0.2", result[1]);
        Assert.Equal("10.0.0.3", result[2]);
    }
}
