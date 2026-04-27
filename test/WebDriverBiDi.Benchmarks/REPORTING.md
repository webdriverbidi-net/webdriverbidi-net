# Benchmark Reporting Quick Reference

This document is the practical how-do-I guide for the performance benchmark
suite. For an overview of what the benchmarks measure and how to invoke them,
see [README.md](README.md).

## Continuous integration: the regular flow

Day-to-day you usually do not need to run benchmarks locally. The
[`Benchmarks` workflow](../../.github/workflows/benchmarks.yml) runs on every
pull request that touches:

- `src/WebDriverBiDi/**` — the main library
- `test/WebDriverBiDi.Benchmarks/**` — the benchmark suite itself
- `scripts/compare-benchmarks.sh` — the comparator script
- `.github/workflows/benchmarks.yml` — the workflow

For each run, the workflow:

1. Builds and runs the full benchmark suite on `ubuntu-latest`.
2. Compares the resulting JSON against the committed baselines in
   [`test/WebDriverBiDi.Benchmarks/baselines/`](baselines/) using
   [`scripts/compare-benchmarks.sh`](../../scripts/compare-benchmarks.sh).
3. Posts (or updates) a PR comment with a per-benchmark delta table.
4. Uploads the full `BenchmarkDotNet.Artifacts/` directory as a
   `benchmark-results` workflow artifact (retained for 30 days) for
   deeper investigation.

The workflow is **advisory only** — it is not a required check, so a red
status does not block a merge. The comment classifies every benchmark with
a glyph:

| Glyph | Meaning |
|-------|---------|
| ✅ | No meaningful regression |
| ⚠️ | Mean time +25% or allocations +10% vs. baseline |
| 🔴 | Mean time +50% or allocations +25% vs. baseline |
| `(new)` | Benchmark has no matching entry in the baseline |

GitHub-hosted runners are shared hardware, so expect some run-to-run
variance. Treat sub-20% changes on a single benchmark as noise; treat a
broad shift (e.g., every benchmark moves ~20% in the same direction) as
more likely to be real.

## Baselines

### What is the baseline?

The baseline is a pair of `ci-baseline-*.json` files committed to
[`test/WebDriverBiDi.Benchmarks/baselines/`](baselines/), one per benchmark
class:

```
test/WebDriverBiDi.Benchmarks/baselines/
  ci-baseline-CommandProcessingBenchmarks.json
  ci-baseline-SerializationBenchmarks.json
```

Each file is a `BenchmarkDotNet` JSON export containing mean time and
allocation figures for every benchmark in its class. The comparator script
looks up each benchmark in the current run by `FullName` and computes the
delta against the baseline.

**The baseline must be produced on the same runner image as the workflow
that compares against it (`ubuntu-latest`).** Absolute benchmark numbers are
hardware-dependent; a baseline produced on a developer laptop would make
every subsequent CI run look like a dramatic regression. A dedicated
workflow enforces this.

### Seeding the baseline (first-time setup)

When the benchmark comparator is first introduced to the repository, the
baseline files contain `{"Benchmarks": []}` placeholders. The first CI run
against these placeholders reports every benchmark as `(new)`. To produce
a real baseline:

1. Go to the **Actions** tab → **Benchmark Baseline** → **Run workflow**.
   Pick `main` as the ref unless there is a specific reason to baseline
   against a different branch.
2. Wait for the run to finish (~2 minutes).
3. Download the `benchmark-baseline` artifact from the workflow run. It
   contains two files named exactly:
   - `ci-baseline-CommandProcessingBenchmarks.json`
   - `ci-baseline-SerializationBenchmarks.json`
4. Drop both files into [`test/WebDriverBiDi.Benchmarks/baselines/`](baselines/),
   replacing the placeholders.
5. Open a pull request with just these two file changes and a title like
   "Seed initial benchmark baseline."

The next PR that touches the benchmark paths will show real deltas in its
PR comment.

### Updating the baseline

Update the baseline when:

- You have **intentionally** shifted performance and want the new numbers
  to be the reference point. This is especially useful after a performance
  optimization lands — leaving the old baseline in place would make every
  subsequent run look "slow."
- The CI runner image changes (e.g., GitHub retires `ubuntu-22.04` and
  promotes `ubuntu-24.04`), which can shift absolute numbers across the
  board.
- The benchmark set itself has changed substantially: benchmarks added,
  removed, or renamed. New/renamed benchmarks render as `(new)` until the
  baseline is refreshed.

The mechanics are identical to seeding. Run the **Benchmark Baseline**
workflow, download the artifact, drop the files into `baselines/`, open a
PR. Include a sentence in the PR description noting why the baseline is
being refreshed — e.g., "Baseline refresh following optimization landed in
#123" — so the diff is reviewable.

### Why a maintainer commits, not a bot

The refresh workflow produces an artifact but does not commit the baseline
on its own. Every baseline change is therefore a human-authored PR with a
reviewer. This avoids needing a bot token with write access to `main` and
keeps the performance-of-record decisions explicit.

## Running benchmarks locally

Local runs are useful for experimentation, debugging performance issues,
and pre-validating that a change does what you expect before letting CI
confirm it on the standard hardware.

### Run everything

```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release
```

The default exporters produce console output and a GitHub-flavoured
markdown file per benchmark class under
`BenchmarkDotNet.Artifacts/results/`.

### Run a single class or method

```bash
# Just the serialization benchmarks
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- \
  --filter '*SerializationBenchmarks*'

# Just one method
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- \
  --filter '*DeserializeNetworkEvent*'
```

### Get the same JSON the comparator uses

```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters json
```

Produces `BenchmarkDotNet.Artifacts/results/*-report-full-compressed.json`.
You can feed that directly to the comparator to simulate what CI will do:

```bash
scripts/compare-benchmarks.sh \
  BenchmarkDotNet.Artifacts/results \
  test/WebDriverBiDi.Benchmarks/baselines
```

Note that the absolute numbers will differ from CI because your laptop is
almost certainly not a GitHub-hosted `ubuntu-latest` runner.

### Quick validation runs

For when you just want to confirm a benchmark builds and runs at all (not
for trustworthy numbers):

```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- \
  --filter '*DeserializeNetworkEvent*' --job dry
```

`--job dry` runs each benchmark a single time with no warmup. Useful for
plumbing checks, not for perf decisions.

## Before/after comparison for a local experiment

When iterating on a performance change locally, it is often easier to
compare before/after on your own hardware than to let each iteration
cycle through CI. One convention that works:

```bash
# Before the change, on a clean working tree
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters json
cp -r BenchmarkDotNet.Artifacts/results before-change

# After the change
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- --exporters json

# Use the committed comparator with the "before" directory as the baseline.
# You'll need to rename the JSON files to the form the comparator expects.
for f in before-change/*-report-full-compressed.json; do
  filename=$(basename "$f" -report-full-compressed.json)
  class=${filename#WebDriverBiDi.Benchmarks.}
  cp "$f" "before-change/ci-baseline-${class}.json"
done
scripts/compare-benchmarks.sh BenchmarkDotNet.Artifacts/results before-change
```

This produces the same glyphed delta table the CI PR comment produces, but
with your laptop as the constant hardware.

## Interpreting results

### What's good?

- **Deserialization:** < 50 μs for typical events/results
- **Serialization:** < 20 μs for typical commands
- **Gen0 collections:** ideally < 0.5 per 1000 ops
- **Gen1/Gen2 collections:** zero in hot paths
- **Allocated:** < 2 KB per operation for typical commands

### What's concerning?

- Mean time +20–25% or more vs. baseline on a stable CI run
- Any Gen1 or Gen2 collections appearing where they were previously zero
- Allocation growth > 1 KB per operation that cannot be explained by the
  change under review

### Time units

- `ns` (nanosecond) = 0.000000001 s
- `μs` (microsecond) = 0.000001 s
- `ms` (millisecond) = 0.001 s

## Export formats

| Format | Use case | Command flag |
|--------|----------|--------------|
| Console | Quick checks | (default) |
| Markdown | GitHub comments, docs | `--exporters markdown` (default GitHub-flavour variant is on by default) |
| JSON | Baseline comparison, automation | `--exporters json` |
| HTML | Detailed analysis | `--exporters html` |
| CSV | Spreadsheet analysis | `--exporters csv` |

Combine flags to request multiple at once:

```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- \
  --exporters json html csv
```

## FAQ

**Q: Why do results vary between runs?**
A: System noise, background processes, CPU thermal throttling. 5–10%
variance on a quiet local machine is normal; GitHub-hosted runners can
show more.

**Q: I ran the benchmarks locally and the PR comment looks totally different.**
A: The CI runs on `ubuntu-latest` — a shared x64 VM. Your laptop is
almost certainly not the same hardware, and an Apple Silicon CPU in
particular will produce dramatically different absolute numbers. Compare
relative deltas, not absolute numbers, when cross-referencing local and CI
results.

**Q: A benchmark shows a regression in the PR comment.**
A: First, re-run the workflow to confirm it wasn't a one-off runner noise
event. If the regression reproduces:
- Investigate what in the PR could cause it.
- If the regression is intentional (e.g., you traded perf for correctness),
  note it in the PR description and refresh the baseline once the PR merges.
- If the regression is unintentional, fix before merging.

**Q: A newly-added benchmark shows as `(new)` forever.**
A: That's expected until the baseline is refreshed. New benchmarks only
have a baseline entry after the next baseline-refresh PR merges.

**Q: How do I quickly test a benchmark change without running the full suite?**
A: Use `--filter` and optionally `--job dry` for plumbing checks:

```bash
dotnet run --project test/WebDriverBiDi.Benchmarks -c Release -- \
  --filter '*MyNewBenchmark*' --job dry
```

Results from `--job dry` are not trustworthy for perf decisions — they run
each benchmark once with no warmup. For real numbers, drop the `--job dry`.
