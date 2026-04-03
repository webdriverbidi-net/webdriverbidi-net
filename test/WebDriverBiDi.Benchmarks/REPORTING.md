# Benchmark Reporting Quick Reference

## Quick Commands

### For GitHub Issues/PRs
```bash
# Run benchmarks and copy console output
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release
```
Copy the summary table from the end of the output. It looks like this:

```
| Method                           | Mean      | Error    | StdDev   | Allocated |
|--------------------------------- |----------:|---------:|---------:|----------:|
| SerializeCommandParameters       |  1.234 μs | 0.045 μs | 0.012 μs |     512 B |
| DeserializeCommandResult         |  2.567 μs | 0.089 μs | 0.023 μs |    1024 B |
| DeserializeNetworkEvent          |  3.456 μs | 0.123 μs | 0.034 μs |    2048 B |
| DeserializeSimpleRemoteValue     |  0.789 μs | 0.034 μs | 0.009 μs |     256 B |
| DeserializeComplexRemoteValue    |  1.890 μs | 0.067 μs | 0.018 μs |     768 B |
```

**Best for**: Sharing results in GitHub discussions, issues, or PR comments.

---

### For Pre-Release Validation
```bash
# Run and save baseline
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters json markdown

# Save the baseline
VERSION="1.0.0"  # Replace with actual version
cp BenchmarkDotNet.Artifacts/results/*-report-full-compressed.json \
   test/WebDriverBiDi.Benchmarks/baselines/v${VERSION}-baseline.json
```

**Best for**: Creating stable reference points for future comparisons.

---

### For Detailed Analysis
```bash
# Generate HTML report for deep dive
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters html

# Open in browser
open BenchmarkDotNet.Artifacts/results/*-report.html
```

**Best for**: Investigating performance issues or analyzing distributions.

---

## Example Report Format for GitHub

When posting benchmark results to GitHub, use this format:

```markdown
## Performance Benchmarks

Environment:
- OS: macOS 14.5 / Ubuntu 24.04 / Windows 11
- CPU: Apple M4 Max / Intel i9-13900K / AMD Ryzen 9 7950X
- .NET: 10.0.2
- Commit: abc1234

### Results

| Method | Mean | Allocated | Notes |
|--------|------|-----------|-------|
| DeserializeNetworkEvent | 3.456 μs | 2048 B | ✅ Meets goal (<50μs) |
| SerializeCommandParameters | 1.234 μs | 512 B | ✅ Meets goal (<20μs) |

### Changes from Previous Baseline (v1.0.0)
- DeserializeNetworkEvent: **+5%** (was 3.292 μs) - acceptable variance
- SerializeCommandParameters: **-2%** (was 1.259 μs) - slight improvement
```

---

## Interpreting Results

### Time Units
- `ns` (nanosecond) = 0.000000001 second
- `μs` (microsecond) = 0.000001 second
- `ms` (millisecond) = 0.001 second

### What's Good?
✅ **Deserialization** < 50 μs
✅ **Serialization** < 20 μs
✅ **Gen0** collections < 0.5 per 1000 ops
✅ **Gen1/Gen2** collections = 0
✅ **Allocated** < 2KB per operation

### What's Concerning?
⚠️ **>20% slower** than baseline
⚠️ **Gen1** collections > 0
⚠️ **Gen2** collections > 0
⚠️ **Allocated** increased significantly

---

## Comparison Workflow

### Before Major Refactor
```bash
# Save baseline
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release
cp -r BenchmarkDotNet.Artifacts/results BenchmarkDotNet.Artifacts/before-refactor
```

### After Major Refactor
```bash
# Run again
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release

# Compare manually
# Look at: BenchmarkDotNet.Artifacts/before-refactor/*-github.md
# vs: BenchmarkDotNet.Artifacts/results/*-github.md
```

### Quick Visual Comparison
Open both markdown files side-by-side and compare the "Mean" and "Allocated" columns.

---

## Export Formats Summary

| Format | Use Case | Command Flag |
|--------|----------|--------------|
| **Console** | Quick checks, GitHub issues | (default) |
| **Markdown** | GitHub PRs, documentation | `--exporters markdown` |
| **JSON** | Historical tracking, automation | `--exporters json` |
| **HTML** | Detailed analysis, visualizations | `--exporters html` |
| **CSV** | Excel/Sheets analysis | `--exporters csv` |

### All Formats at Once
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- \
  --exporters json markdown html csv
```

---

## Storage Recommendations

### Commit to Git (Optional)
- ✅ **DO commit**: Release baseline JSON files (e.g., `baselines/v1.0.0-baseline.json`)
- ❌ **DON'T commit**: All artifacts from every run (already in .gitignore)

### Keep Locally
- Individual benchmark runs for experimentation
- Before/after comparison artifacts
- HTML reports for investigation

---

## FAQ

**Q: Why do results vary between runs?**
A: System noise, background processes, CPU throttling. Variance of 5-10% is normal.

**Q: Should I run on same hardware every time?**
A: Yes, for accurate comparisons. Different CPUs produce different absolute numbers.

**Q: How many times should I run benchmarks?**
A: BenchmarkDotNet runs multiple iterations automatically. Run the full suite 2-3 times if results seem unusual.

**Q: What if a benchmark shows a regression?**
A: First, run again to confirm. If confirmed >20%, investigate. Check allocations, algorithm changes, or added work.

**Q: Can I run a single benchmark quickly?**
A: Yes, use `--filter "*MethodName*"` and `--job dry` for quick validation (less accurate):
```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- \
  --filter "*DeserializeNetworkEvent*" --job dry
```
