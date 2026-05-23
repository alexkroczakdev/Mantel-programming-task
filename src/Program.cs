using MantelLogAnalyser;

Console.WriteLine("MantelLogAnalyser");

var filePath = args.Length > 0
    ? args[0]
    : "programming-task-example-data.log";

ILogParser parser = new LogParser();
ILogAnalyser analyser = new LogAnalyser();
ILogReporter reporter = new LogReporter();

try
{
    var entries = parser.ParseFile(filePath).ToList();
    Console.WriteLine(reporter.Report(new LogAnalysisResult(
        UniqueIps: analyser.GetNumberOfUniqueIPs(entries),
        TopVisitedUrls: analyser.GetMostVisitedUrls(entries),
        TopActiveIps: analyser.GetMostActiveIPs(entries)
    )));
}
catch (FileNotFoundException) { Console.Error.WriteLine($"File not found: {filePath}"); }
catch (UnauthorizedAccessException ex) { Console.Error.WriteLine($"Access denied: {ex.Message}"); }
catch (IOException ex) { Console.Error.WriteLine($"Error reading file: {ex.Message}"); }