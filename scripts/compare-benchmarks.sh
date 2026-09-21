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
#   0 — comparison ran; the report is advisory, so a benchmark regression does
#       not fail the script. Callers decide policy from the report contents.
#   2 — usage error (wrong number of arguments).
#   Note: `set -euo pipefail` means an unexpected internal error also aborts
#   with a non-zero status; it is not a graceful exit code.

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

  # An absolute delta against a baseline of zero: a percentage is undefined, so report the bytes.
  # Any allocation at all is a regression against a zero-allocation baseline.
  case "$delta" in
    abs:*)
      local absolute="${delta#abs:}"
      if [ "$(awk -v a="$absolute" 'BEGIN { print (a > 0) ? 1 : 0 }')" = "1" ]; then
        echo "+${absolute} B 🔴"
      else
        echo "0 B ✅"
      fi
      return
      ;;
  esac

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
        ($cur.DisplayInfo // $cur.Method // $cur.MethodTitle),
        ($cur.Statistics.Mean | tostring),
        (if $base then (($cur.Statistics.Mean - $base.Statistics.Mean) / $base.Statistics.Mean | tostring) else "new" end),
        ($cur.Memory.BytesAllocatedPerOperation | tostring),
        (if $base | not then "new"
         elif ($base.Memory.BytesAllocatedPerOperation // 0) > 0
         then (($cur.Memory.BytesAllocatedPerOperation - $base.Memory.BytesAllocatedPerOperation) / $base.Memory.BytesAllocatedPerOperation | tostring)
         else
           # A baseline that allocated nothing has no ratio to compute, but it is still a baseline:
           # going from 0 to any allocation is a regression and must be reported as one. Emit the
           # absolute figure for the shell to render, rather than the "new" sentinel, which would
           # claim the benchmark is unknown to the baseline.
           ("abs:" + ($cur.Memory.BytesAllocatedPerOperation | tostring))
         end)
      ]
    | @tsv
  ' "$current_file" | while IFS=$'\t' read -r method mean mean_delta allocated alloc_delta; do
    mean_cell=$(format_delta_cell "$mean_delta" "$mean_warn_threshold" "$mean_bad_threshold")
    alloc_cell=$(format_delta_cell "$alloc_delta" "$alloc_warn_threshold" "$alloc_bad_threshold")
    mean_rounded=$(awk -v m="$mean" 'BEGIN { printf "%.0f", m }')
    echo "| $method | $mean_rounded | $mean_cell | $allocated | $alloc_cell |"
  done

  # Benchmarks the baseline knows about that this run did not produce. Iterating only over the
  # current results made a renamed or deleted benchmark vanish from the comparison entirely, so a
  # rename read as "everything is fine" instead of "this measurement is no longer being taken".
  jq -r --slurpfile current "$current_file" '
    (($current[0].Benchmarks // []) | map(.FullName)) as $current_names
    | .Benchmarks[]
    | select([.FullName] | inside($current_names) | not)
    | [
        (.DisplayInfo // .Method // .MethodTitle),
        (.Statistics.Mean | tostring),
        (.Memory.BytesAllocatedPerOperation | tostring)
      ]
    | @tsv
  ' "$baseline_arg" | while IFS=$'\t' read -r method mean allocated; do
    mean_rounded=$(awk -v m="$mean" 'BEGIN { printf "%.0f", m }')
    echo "| $method | $mean_rounded | (removed) | $allocated | (removed) |"
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

# A baseline with no current results is a class that was deleted or renamed. The loop above iterates the
# current results only, so such a class would otherwise vanish from the comparison without a word.
for baseline_file in "$baselines_dir"/ci-baseline-*.json; do
  [ -f "$baseline_file" ] || continue
  baseline_name=$(basename "$baseline_file" .json)
  class_name=${baseline_name#ci-baseline-}
  if [ ! -f "$results_dir/WebDriverBiDi.Benchmarks.${class_name}-report-full-compressed.json" ]; then
    echo "### $class_name"
    echo ""
    echo "_(class removed) A baseline is on file for this class, but the run produced no results for it; it was renamed or deleted._"
    echo ""
  fi
done

if [ "$found_any" = "false" ]; then
  echo "_No benchmark results were produced in \`$results_dir\`. The benchmark run may have failed._"
fi
