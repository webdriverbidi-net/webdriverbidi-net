#!/bin/bash
# Compares BenchmarkDotNet JSON results against committed baselines and
# produces a markdown report on stdout suitable for a PR comment.
#
# Usage:
#   compare-benchmarks.sh <results-dir> <baselines-dir>
#
# Inputs:
#   <results-dir>    Directory produced by `--exporters json`. Expected to
#                    contain files named
#                      WebDriverBiDi.Benchmarks.<ClassName>-report-full-compressed.json
#   <baselines-dir>  Directory of committed baselines. Looked up per class as
#                      ci-baseline-<ClassName>.json
#
# Thresholds (fractional, so 0.25 = 25%):
#   mean:  +25% = ⚠️   +50% = 🔴
#   alloc: +10% = ⚠️   +25% = 🔴
#   improvements and small regressions render as ✅
#   missing-from-baseline benchmarks render as (new)
#
# Exit codes:
#   0 — always. The report is advisory; callers decide policy.

set -euo pipefail

if [ $# -ne 2 ]; then
  echo "Usage: $0 <results-dir> <baselines-dir>" >&2
  exit 2
fi

results_dir="$1"
baselines_dir="$2"

mean_warn_threshold=0.25
mean_bad_threshold=0.50
alloc_warn_threshold=0.10
alloc_bad_threshold=0.25

# Render one delta cell from a fractional delta ("new" or a signed float) and
# two threshold values. Uses awk because bash cannot compare floats natively.
format_delta_cell() {
  local delta="$1"
  local warn="$2"
  local bad="$3"

  if [ "$delta" = "new" ]; then
    echo "(new)"
    return
  fi

  # Compare with a small tolerance so a delta that renders as "+25.0%" also
  # crosses the 25% threshold, avoiding cosmetic boundary flips from
  # float-serialization noise (e.g. 0.24999999 showing as "+25.0% ✅").
  awk -v d="$delta" -v w="$warn" -v b="$bad" 'BEGIN {
    eps = 0.0005
    pct = d * 100
    sign = (d >= 0) ? "+" : ""
    if (d + eps >= b) glyph = " 🔴"
    else if (d + eps >= w) glyph = " ⚠️"
    else glyph = " ✅"
    printf "%s%.1f%%%s", sign, pct, glyph
  }'
}

# Emit a markdown table section for a single benchmark class, comparing the
# current results against the matching baseline. If the baseline file is
# missing or has an empty Benchmarks array, every benchmark is reported as
# "(new)" and the table carries an explanatory note.
render_class_table() {
  local class_name="$1"
  local current_file="$2"
  local baseline_file="$3"

  echo "### $class_name"
  echo ""

  local baseline_exists="false"
  local baseline_arg="$baseline_file"
  if [ -f "$baseline_file" ]; then
    local count
    count=$(jq '.Benchmarks | length' "$baseline_file" 2>/dev/null || echo 0)
    if [ "$count" -gt 0 ]; then
      baseline_exists="true"
    fi
  fi

  if [ "$baseline_exists" = "false" ]; then
    echo "_No baseline on file for this class. All benchmarks reported as new._"
    echo ""
    # jq --slurpfile errors if the file is missing. Point it at a stub with
    # an empty Benchmarks array so the per-row loop still runs and produces
    # "(new)" cells uniformly. This keeps the two "no baseline" cases
    # (missing file / empty file) on the same code path.
    baseline_arg=$(mktemp)
    echo '{"Benchmarks":[]}' > "$baseline_arg"
  fi

  echo "| Method | Mean (ns) | Δ | Allocated (B) | Δ |"
  echo "|---|---:|---:|---:|---:|"

  # jq program: for each current benchmark, look up the matching baseline by
  # FullName and emit one tab-separated row:
  #   method \t mean \t mean_delta \t allocated \t alloc_delta
  # A missing baseline entry produces "new" sentinels that the shell detects.
  jq -r --slurpfile baseline "$baseline_arg" '
    ($baseline[0].Benchmarks // []) as $b
    | .Benchmarks[]
    | . as $cur
    | ($b | map(select(.FullName == $cur.FullName)) | first) as $base
    | [
        ($cur.Method // $cur.MethodTitle),
        ($cur.Statistics.Mean | tostring),
        (if $base then (($cur.Statistics.Mean - $base.Statistics.Mean) / $base.Statistics.Mean | tostring) else "new" end),
        ($cur.Memory.BytesAllocatedPerOperation | tostring),
        (if $base and ($base.Memory.BytesAllocatedPerOperation // 0) > 0 then (($cur.Memory.BytesAllocatedPerOperation - $base.Memory.BytesAllocatedPerOperation) / $base.Memory.BytesAllocatedPerOperation | tostring) else "new" end)
      ]
    | @tsv
  ' "$current_file" | while IFS=$'\t' read -r method mean mean_delta allocated alloc_delta; do
    mean_cell=$(format_delta_cell "$mean_delta" "$mean_warn_threshold" "$mean_bad_threshold")
    alloc_cell=$(format_delta_cell "$alloc_delta" "$alloc_warn_threshold" "$alloc_bad_threshold")
    mean_rounded=$(awk -v m="$mean" 'BEGIN { printf "%.0f", m }')
    echo "| $method | $mean_rounded | $mean_cell | $allocated | $alloc_cell |"
  done

  if [ "$baseline_arg" != "$baseline_file" ]; then
    rm -f "$baseline_arg"
  fi

  echo ""
}

# Main loop: one section per benchmark class file under results_dir.
# Assumes filenames of the form
#   WebDriverBiDi.Benchmarks.<ClassName>-report-full-compressed.json
found_any="false"
for current_file in "$results_dir"/*-report-full-compressed.json; do
  [ -f "$current_file" ] || continue
  found_any="true"
  filename=$(basename "$current_file" -report-full-compressed.json)
  class_name=${filename#WebDriverBiDi.Benchmarks.}
  baseline_file="$baselines_dir/ci-baseline-${class_name}.json"
  render_class_table "$class_name" "$current_file" "$baseline_file"
done

if [ "$found_any" = "false" ]; then
  echo "_No benchmark results were produced in \`$results_dir\`. The benchmark run may have failed._"
fi
