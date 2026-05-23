# MantelLogAnalyser

A C# solution for parsing Apache/Nginx HTTP access logs and reporting on their contents.

## The Task

Parse a log file containing HTTP requests and report:

- The number of unique IP addresses
- The top 3 most visited URLs
- The top 3 most active IP addresses

**Example log entry:**
```
177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /intranet-analytics/ HTTP/1.1" 200 3574 "-" "Mozilla/5.0 ..."
```

A log file with test data is included with this assignment.

## Project Structure

```
MantelLogAnalyser.slnx
├── src/                        ← console application
│   ├── Program.cs              ← entry point, I/O error handling
│   ├── ILogParser.cs           ← parser interface
│   ├── LogParser.cs            ← log parsing implementation
│   ├── LogEntry.cs             ← immutable record for a parsed line
│   ├── ILogAnalyser.cs         ← analyser interface
│   ├── LogAnalyser.cs          ← analysis logic
│   ├── ILogReporter.cs         ← reporter interface
│   ├── LogReporter.cs          ← result formatting
│   └── LogAnalysisResult.cs    ← immutable record for analysis output
└── tests/                      ← xUnit test project
    ├── LogParserTests.cs
    ├── LogAnalyserTests.cs
    ├── LogReporterTests.cs
    └── IntegrationTests.cs         ← end-to-end tests against the sample log
```

## Getting Started

**Prerequisites:** .NET 10 SDK

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run on the bundled sample log (from solution root)
dotnet run --project src

# Run on a custom log file
dotnet run --project src -- /path/to/access.log
```

## Assumptions

### Reporting

- **Most visited URLs** counts only responses with a `2xx` status code. Redirects (`3xx`) and errors (`4xx`, `5xx`) are excluded as they do not represent a successful visit.
- **Most active IP addresses** counts all requests from an IP regardless of status code, since activity reflects requests sent, not outcomes.

### Log Format

- Input is Apache/Nginx **Combined Log Format (CLF)**. Lines not matching this format are silently skipped.
- Lines with extra trailing fields beyond the CLF spec (e.g. `junk extra`) are accepted — the standard fields are parsed and the extra tokens ignored.
- The request path field accepts both relative paths (`/faq/`) and absolute URLs (`http://example.net/faq/`), as both appear in real CLF logs when a client sends a full URI in the request line. These are treated as distinct URLs — there is no way to determine from the log alone whether `http://example.net/faq/` and `/faq/` resolve to the same server, so normalising them would risk incorrectly merging requests that point to different hosts.
- The user column and other fields not required by the task (ident, timestamp, bytes, referer, user-agent) are intentionally not captured — the regex skips them without allocating named groups.

### File Handling

- The log file is read synchronously line by line using `ReadLine` rather than `ReadLineAsync`. Async reading was considered but excluded — this is a console application with a single blocking operation, so there is no concurrency to benefit from, and the file is expected to be small enough that synchronous reading keeps the code simpler without any practical performance cost.
- I/O errors (file not found, access denied) are not caught inside `LogParser` — they propagate to `Program.cs`, which catches them and writes to `stderr`. This keeps the parser free of output concerns and avoids silent failure (returning an empty result on a missing file would produce misleading output with no error message).
- A `Result<T, Error>` return type was considered for `ParseFile` but excluded. It would make failure explicit in the type system but adds boilerplate and spreads through call chains. For a single-entry-point CLI app where I/O failure is fatal and unrecoverable, exceptions caught at the boundary are the simpler and equally correct choice.

## Design Decisions

- **Interfaces (`ILogParser`, `ILogAnalyser`, `ILogReporter`)** — each component is hidden behind an interface, keeping implementations swappable and making unit testing straightforward without a mocking framework.
- **Interface input parameters use `IEnumerable<T>`** — analyser methods accept `IEnumerable<LogEntry>` rather than `List<LogEntry>`, accepting the broadest type the implementation actually needs. Callers are not forced to materialise a `List<T>` before calling.
- **`[GeneratedRegex]`** — the CLF regex is declared with the source-generator attribute so the implementation is produced at compile time, avoiding any runtime compilation overhead.
- **`LogEntry` and `LogAnalysisResult` records** — immutable by default; positional syntax makes construction concise and equality comparison free.
- **Named capture groups** — each regex group is named after its field, so `ParseLine` reads `match.Groups["ip"]` rather than `match.Groups[1]`, keeping the mapping self-documenting.
- **`LogReporter` returns a string** — the reporter formats the result into a human-readable string and returns it; `Program.cs` is responsible for writing it to the console. This keeps `LogReporter` a pure function with no side effects, making it trivially testable without redirecting `Console.Out`.
- **Tie-breaking** — when more than 3 IPs or URLs share the same hit count at the boundary, the top 3 are selected by first appearance in the log file. This keeps the implementation simple and deterministic. An alternative would be to surface all tied entries, but this was considered out of scope for this task.


## Scope Note

The regex and `LogEntry` are intentionally scoped to the three fields required by the task (`IpAddress`, `RequestPath`, `StatusCode`). Extending to a full CLF model is straightforward — starting points for the record and regex are below.



```csharp 

public record LogEntry(
    string IpAddress,
    string Ident,
    string? User,
    DateTimeOffset Timestamp,
    string HttpMethod,
    string RequestPath,
    string HttpVersion,
    int StatusCode,
    long BytesSent,
    string Referer,
    string UserAgent
);

[GeneratedRegex(@"
        ^(?<ip>\S+)[ ]                      # client IP
        (?<ident>\S+)[ ]                    # ident (usually -)
        (?<user>\S+)[ ]                     # authenticated user (- if anonymous)
        \[(?<timestamp>[^\]]+)\][ ]         # [timestamp]
        ""(?<request>[^""]+)""[ ]           # ""METHOD path version""
        (?<status>\d{3})[ ]                  # HTTP status code
        (?<bytes>\d+)[ ]                    # bytes sent
        ""(?<referer>[^""]*)""[ ]           # ""referer""
        ""(?<useragent>[^""]*)""            # ""user-agent""
        ",
        RegexOptions.CultureInvariant
        | RegexOptions.ExplicitCapture
        | RegexOptions.IgnorePatternWhitespace)]
        
 ```