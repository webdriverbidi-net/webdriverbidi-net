# WebDriverBiDi.NET Performance Benchmarks

This project contains performance benchmarks for the WebDriverBiDi.NET library using BenchmarkDotNet.

**📊 See [REPORTING.md](REPORTING.md) for a quick reference on running and reporting benchmarks.**

## Running Benchmarks

### Run all benchmarks
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release
```

### Run specific benchmark class
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --filter "*SerializationBenchmarks*"
```

### Run specific benchmark method
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --filter "*DeserializeCommandResult*"
```

### Export results
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters json markdown
```

## Benchmark Categories

### SerializationBenchmarks
Measures JSON serialization/deserialization performance for protocol messages:
- **SerializeCommandParameters**: Command parameter serialization overhead
- **DeserializeCommandResult**: Result deserialization (including large payloads like screenshots)
- **DeserializeNetworkEvent**: Network event deserialization
- **DeserializeSimpleRemoteValue**: Simple RemoteValue deserialization
- **DeserializeComplexRemoteValue**: Complex discriminated union deserialization

### CommandProcessingBenchmarks
Measures command object creation and initialization overhead:
- **CreateSimpleCommand**: Minimal command with only required fields
- **CreateComplexCommand**: Command with all optional fields populated
- **CreateNetworkInterceptCommand**: Network intercept setup overhead
- **CreateScriptEvaluateCommand**: Script evaluation command creation
- **CreateScriptCallFunctionCommand**: Function call with arguments

### PendingCommandCollectionBenchmarks
Measures the transport's pending-command bookkeeping overhead:
- **AddRemovePendingCommand**: Full add-then-remove round trip through the
  pending-command collection, covering the semaphore acquire/release and the
  backing ConcurrentDictionary insertion and removal. Every command the
  driver sends traverses this path once.

### EventDispatchBenchmarks
Measures dispatch cost for `ObservableEvent<T>.NotifyObserversAsync` across
multiple observer counts:
- **NotifyObservers (ObserverCount=1, 4, 16)**: Fires one event with the
  specified number of registered no-op observers. The single-observer case
  is the typical production scenario; 4 and 16 reveal the scaling behavior
  of the observer-snapshot lock and per-observer dispatch loop.

### CommandExecutionBenchmarks
Measures the end-to-end cost of `BiDiDriver.ExecuteCommandAsync` against an
in-memory echo connection with zero simulated latency:
- **ExecuteCommandRoundTrip**: Full command round trip — JSON serialization,
  pending-command bookkeeping, send, response synthesis, incoming-message
  queue write/read, response deserialization, and TaskCompletionSource
  completion. The echo connection isolates library overhead from any real
  I/O cost.

## Understanding Results

BenchmarkDotNet provides:
- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation of all measurements
- **Gen0/Gen1/Gen2**: Garbage collections per 1000 operations
- **Allocated**: Allocated memory per operation

## Performance Goals

For a protocol library, key metrics to optimize:
1. **Deserialization**: < 50μs for typical events/results
2. **Serialization**: < 20μs for typical commands
3. **Allocations**: Minimize per-operation allocations
4. **GC pressure**: Low Gen0, zero Gen1/Gen2 in hot paths

## Reporting Results

### For GitHub Issues/PRs (Markdown Table)
Use the default console output or markdown exporter for easy copy-paste:
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release

# Or export as markdown file
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters markdown
# Results in: BenchmarkDotNet.Artifacts/results/*-report-github.md
```

Copy the summary table from console output or the generated markdown file. Example:
```markdown
| Method                        | Mean      | Error    | Allocated |
|------------------------------ |----------:|---------:|----------:|
| SerializeCommandParameters    | 1.234 μs  | 0.045 μs |     512 B |
| DeserializeCommandResult      | 2.567 μs  | 0.089 μs |    1024 B |
```

### For Historical Tracking (JSON)
Export to JSON for programmatic comparison and historical analysis:
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters json

# Results in: BenchmarkDotNet.Artifacts/results/*-report-full-compressed.json
```

Store these files with version tags:
```bash
# After running benchmarks for a release
cp BenchmarkDotNet.Artifacts/results/*-report-full-compressed.json \
   test/WebDriverBiDi.Benchmarks/baselines/v1.0.0-baseline.json
```

### For Detailed Analysis (HTML + CSV)
Generate comprehensive reports for deep-dive analysis:
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters html csv

# HTML report: BenchmarkDotNet.Artifacts/results/*-report.html (open in browser)
# CSV data: BenchmarkDotNet.Artifacts/results/*-report.csv (for Excel/Google Sheets)
```

### Comparing Baseline vs. Current
To compare against a previous run:

1. **Save baseline** (e.g., before a refactor):
   ```bash
   dotnet run --project test/WebDriverBiDi.Benchmarks -c Release
   cp -r BenchmarkDotNet.Artifacts/results BenchmarkDotNet.Artifacts/baseline-results
   ```

2. **Make changes** to code

3. **Run benchmarks again** and compare:
   ```bash
   dotnet run --project test/WebDriverBiDi.Benchmarks -c Release
   ```

4. **Manual comparison**: Compare the Mean/Allocated columns in console output

5. **Automated comparison** (using BenchmarkDotNet.ResultsComparer or custom scripts):
   ```bash
   # Compare JSON files programmatically
   # Or use: dotnet tool install -g BenchmarkDotNet.Tool
   # Then: dotnet-benchmark compare baseline.json current.json
   ```

## What to Look For

When reviewing benchmark results, focus on:

1. **Regression thresholds**:
   - 🟢 <10% change: Normal variance
   - 🟡 10-25% slower: Investigate if critical path
   - 🔴 >25% slower: Likely regression, needs attention

2. **Memory allocations**:
   - 🟢 No increase or reduction: Good
   - 🟡 Small increase (<1KB): Usually acceptable
   - 🔴 Significant increase (>1KB) or Gen1/Gen2 collections: Needs investigation

3. **Critical hot paths** (prioritize these):
   - `DeserializeNetworkEvent` - runs for every network event
   - `DeserializeCommandResult` - runs for every command response
   - `DeserializeComplexRemoteValue` - common in script evaluation

## Pre-Release Checklist

Before tagging a release:

1. Run full benchmark suite:
   ```bash
   dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters json markdown
   ```

2. Compare against last release baseline (if available):
   ```bash
   # Compare markdown summaries or JSON files
   ```

3. Document any significant changes (>20%) in release notes

4. Save baseline for this release:
   ```bash
   mkdir -p test/WebDriverBiDi.Benchmarks/baselines
   cp BenchmarkDotNet.Artifacts/results/*-report-full-compressed.json \
      test/WebDriverBiDi.Benchmarks/baselines/v<VERSION>-baseline.json
   ```

5. Consider adding baseline to git if it represents a stable reference point

## Tips for Accurate Results

- **Run in Release mode**: Always use `-c Release`
- **Close other applications**: Minimize background noise
- **Multiple runs**: Run 2-3 times if results seem unusual
- **Consistent hardware**: Use same machine for comparisons
- **Warm system**: Let first run complete before trusting results (BenchmarkDotNet handles warmup, but cold system state matters)
- **Check for warnings**: BenchmarkDotNet will warn about issues like short iteration times
