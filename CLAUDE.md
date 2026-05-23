# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                          # build solution
dotnet test                           # run all tests
dotnet test --filter "FullyQualifiedName~LogParserTests"   # run one test class
dotnet run --project src              # run against bundled sample log
dotnet run --project src -- /path/to/access.log            # run against custom log
```

## Architecture

The pipeline is linear: `LogParser` → `LogAnalyser` → `LogReporter`, wired together in `Program.cs`. Each step is hidden behind an interface (`ILogParser`, `ILogAnalyser`, `ILogReporter`).

**Data flow**

1. `LogParser.ParseFile` streams the file line by line via `yield return`, silently skipping lines that don't match the CLF regex. Returns `IEnumerable<LogEntry>`.
2. `Program.cs` materialises the enumerable into a `List<LogEntry>` (`.ToList()`) before passing it to the analyser — required because the three analyser methods each make a separate pass.
3. `LogAnalyser` runs three independent LINQ passes over the list to produce a `LogAnalysisResult` record.
4. `LogReporter.Report` formats the result into a string; `Program.cs` writes it to stdout.

**Key decisions baked into the code**

- `GetMostVisitedUrls` filters to `2xx` status codes only; `GetMostActiveIPs` counts all status codes.
- Tie-breaking in both ranking methods is by first appearance in the log (a property of `CountBy` + stable `OrderByDescending`).
- The regex uses `[GeneratedRegex]` (compile-time generation) with `RegexOptions.ExplicitCapture` — only the three named groups (`ip`, `url`, `status`) are captured.
- `ParseFile` defers file opening to first iteration; `FileNotFoundException` and other I/O errors propagate to `Program.cs`, which catches them, writes to stderr, and exits.
- `System.Globalization` is explicitly imported in `LogParser.cs` — it is not in the console SDK's implicit usings.
