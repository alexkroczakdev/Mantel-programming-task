namespace MantelLogAnalyser.Tests;

public class LogParserTests
{
    private readonly ILogParser _parser = new LogParser();

    [Fact]
    public void ParseLine_ValidLine_ReturnsLogEntry()
    {
        // Arrange
        const string logLine ="50.112.00.11 - admin [11/Jul/2018:17:31:05 +0200] \"GET /hosting/ HTTP/1.1\" 200 3574 \"-\" \"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.6 (KHTML, like Gecko) Chrome/20.0.1092.0 Safari/536.6\"";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("50.112.00.11", result.IpAddress);
        Assert.Equal("/hosting/", result.RequestPath);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void ParseLine_AbsoluteUrlInPath_ParsesCorrectly()
    {
        // Arrange
        const string logLine ="168.41.191.40 - - [09/Jul/2018:10:11:30 +0200] \"GET http://example.net/faq/ HTTP/1.1\" 200 3574 \"-\" \"Mozilla/5.0 (Linux; U; Android 2.3.5; en-us; HTC Vision Build/GRI40) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("168.41.191.40", result.IpAddress);
        Assert.Equal("http://example.net/faq/", result.RequestPath);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void ParseLine_Returns404_WhenPageNotFound()
    {
        // Arrange
        const string logLine ="168.41.191.41 - - [11/Jul/2018:17:41:30 +0200] \"GET /this/page/does/not/exist/ HTTP/1.1\" 404 3574 \"-\" \"Mozilla/5.0 (Linux; U; Android 2.3.5; en-us; HTC Vision Build/GRI40) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("168.41.191.41", result.IpAddress);
        Assert.Equal("/this/page/does/not/exist/", result.RequestPath);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void ParseLine_Returns500_WhenServerError()
    {
        // Arrange
        const string logLine ="72.44.32.11 - - [11/Jul/2018:17:42:07 +0200] \"GET /to-an-error HTTP/1.1\" 500 3574 \"-\" \"Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0\"";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("72.44.32.11", result.IpAddress);
        Assert.Equal("/to-an-error", result.RequestPath);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public void ParseLine_Returns301_WhenMovedPermanently()
    {
        // Arrange
        const string logLine ="168.41.191.43 - - [11/Jul/2018:17:43:40 +0200] \"GET /moved-permanently HTTP/1.1\" 301 3574 \"-\" \"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_6_7) AppleWebKit/534.24 (KHTML, like Gecko) RockMelt/0.9.58.494 Chrome/11.0.696.71 Safari/534.24\"";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("168.41.191.43", result.IpAddress);
        Assert.Equal("/moved-permanently", result.RequestPath);
        Assert.Equal(301, result.StatusCode);
    }

    [Fact]
    public void ParseLine_Returns307_WhenTemporaryRedirect()
    {
        // Arrange
        const string logLine ="168.41.191.43 - - [11/Jul/2018:17:44:40 +0200] \"GET /temp-redirect HTTP/1.1\" 307 3574 \"-\" \"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_6_7) AppleWebKit/534.24 (KHTML, like Gecko) RockMelt/0.9.58.494 Chrome/11.0.696.71 Safari/534.24\"";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("168.41.191.43", result.IpAddress);
        Assert.Equal("/temp-redirect", result.RequestPath);
        Assert.Equal(307, result.StatusCode);
    }

    [Fact]
    public void ParseLine_LineWithExtraTrailingFields_ReturnsValidEntry()
    {
        // Arrange
        const string logLine ="168.41.191.9 - - [09/Jul/2018:22:56:45 +0200] \"GET /docs/ HTTP/1.1\" 200 3574 \"-\" \"Mozilla/5.0 (X11; Linux i686; rv:6.0) Gecko/20100101 Firefox/6.0\" 456 789";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("168.41.191.9", result.IpAddress);
        Assert.Equal("/docs/", result.RequestPath);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void ParseLine_LineWithJunkTrailingFields_ReturnsValidEntry()
    {
        // Arrange
        const string logLine ="72.44.32.10 - - [09/Jul/2018:15:48:07 +0200] \"GET / HTTP/1.1\" 200 3574 \"-\" \"Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0\" junk extra";

        // Act
        var result = _parser.ParseLine(logLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("72.44.32.10", result.IpAddress);
        Assert.Equal("/", result.RequestPath);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void ParseLine_EmptyString_ReturnsNull()
    {
        // Act
        var result = _parser.ParseLine("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseLine_WhitespaceOnly_ReturnsNull()
    {
        // Act
        var result = _parser.ParseLine("   ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseLine_GarbledLine_ReturnsNull()
    {
        // Act
        var result = _parser.ParseLine("this is not a valid log line");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseFile_FileNotFound_ThrowsFileNotFoundException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _parser.ParseFile("this_file_does_not_exist_xyz.log").ToList());
    }

    [Fact]
    public void ParseFile_EmptyFile_ReturnsEmptyList()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Act
            var result = _parser.ParseFile(tempFile);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_MixedValidAndInvalidLines_YieldsOnlyValidEntries()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(tempFile,
            [
                "168.41.191.40 - - [09/Jul/2018:10:11:30 +0200] \"GET /blog/ HTTP/1.1\" 200 3574 \"-\" \"Mozilla/5.0\"",
                "",
                "this is not a valid log line",
                "177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] \"GET /intranet-analytics/ HTTP/1.1\" 200 3574 \"-\" \"Mozilla/5.0\""
            ]);

            // Act
            var results = _parser.ParseFile(tempFile).ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("168.41.191.40", results[0].IpAddress);
            Assert.Equal("177.71.128.21", results[1].IpAddress);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
