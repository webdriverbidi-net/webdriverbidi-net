#!/bin/bash
# Parses one or more LCOV .info files and fails if aggregate line, branch, or
# method coverage drops below the configured thresholds.
#
# Usage:
#   check-lcov-thresholds.sh [--min-line N] [--min-branch N] [--min-method N] <lcov-file>...
#
# Defaults:
#   --min-line   100   (line coverage percentage; integer or decimal)
#   --min-branch 100
#   --min-method 100
#
# Exit codes:
#   0 — all thresholds met
#   1 — at least one threshold not met
#   2 — configuration or input error

set -euo pipefail

MIN_LINE=100
MIN_BRANCH=100
MIN_METHOD=100
FILES=()

while [ $# -gt 0 ]; do
  case "$1" in
    --min-line)   MIN_LINE="$2";   shift 2 ;;
    --min-branch) MIN_BRANCH="$2"; shift 2 ;;
    --min-method) MIN_METHOD="$2"; shift 2 ;;
    -h|--help)
      sed -n '2,14p' "$0" | sed 's/^# \{0,1\}//'
      exit 0
      ;;
    --*)
      echo "❌ ERROR: Unknown flag: $1" >&2
      exit 2
      ;;
    *)
      FILES+=("$1")
      shift
      ;;
  esac
done

if [ ${#FILES[@]} -eq 0 ]; then
  echo "❌ ERROR: No LCOV input files supplied." >&2
  echo "Usage: $0 [--min-line N] [--min-branch N] [--min-method N] <lcov-file>..." >&2
  exit 2
fi

for f in "${FILES[@]}"; do
  if [ ! -f "$f" ]; then
    echo "❌ ERROR: LCOV file not found: $f" >&2
    exit 2
  fi
done

# Aggregate LF/LH/BRF/BRH/FNF/FNH across every end_of_record section in every
# file. awk handles the arithmetic in a single pass to avoid bash integer
# quirks and to keep the script fast on large LCOV files.
#
# Output format: a single line of four numbers — total_lf, total_lh, total_brf,
# total_brh, total_fnf, total_fnh — separated by spaces.
read -r LF LH BRF BRH FNF FNH < <(awk '
  /^LF:/  { split($0, a, ":"); lf  += a[2] }
  /^LH:/  { split($0, a, ":"); lh  += a[2] }
  /^BRF:/ { split($0, a, ":"); brf += a[2] }
  /^BRH:/ { split($0, a, ":"); brh += a[2] }
  /^FNF:/ { split($0, a, ":"); fnf += a[2] }
  /^FNH:/ { split($0, a, ":"); fnh += a[2] }
  END     { printf "%d %d %d %d %d %d\n", lf, lh, brf, brh, fnf, fnh }
' "${FILES[@]}")

# Percent helper: awk for decimal arithmetic. Treats 0/0 as "not applicable"
# (returns "-"), which the caller renders as "n/a" and excludes from gating.
calc_percent() {
  local hit="$1" found="$2"
  if [ "$found" -eq 0 ]; then
    echo "-"
  else
    awk -v h="$hit" -v f="$found" 'BEGIN { printf "%.2f\n", (h * 100.0) / f }'
  fi
}

LINE_PCT=$(calc_percent "$LH" "$LF")
BRANCH_PCT=$(calc_percent "$BRH" "$BRF")
METHOD_PCT=$(calc_percent "$FNH" "$FNF")

echo "=== Coverage Summary ==="
printf "Lines:    %s / %s  (%s%%)  — threshold %s%%\n"    "$LH"  "$LF"  "$LINE_PCT"   "$MIN_LINE"
printf "Branches: %s / %s  (%s%%)  — threshold %s%%\n"    "$BRH" "$BRF" "$BRANCH_PCT" "$MIN_BRANCH"
printf "Methods:  %s / %s  (%s%%)  — threshold %s%%\n"    "$FNH" "$FNF" "$METHOD_PCT" "$MIN_METHOD"
echo ""

# Compare a percentage string (or "-") to a threshold. "-" is ignored (nothing
# to gate). Returns 0 if pct >= threshold or pct is "-", 1 otherwise.
meets_threshold() {
  local label="$1" pct="$2" threshold="$3"
  if [ "$pct" = "-" ]; then
    echo "ℹ️  $label: not applicable (no instrumented items), skipping"
    return 0
  fi
  if awk -v p="$pct" -v t="$threshold" 'BEGIN { exit !(p >= t) }'; then
    echo "✅ $label: $pct% >= $threshold%"
    return 0
  else
    echo "❌ $label: $pct% < $threshold%"
    return 1
  fi
}

FAIL=0
meets_threshold "Lines"    "$LINE_PCT"   "$MIN_LINE"   || FAIL=1
meets_threshold "Branches" "$BRANCH_PCT" "$MIN_BRANCH" || FAIL=1
meets_threshold "Methods"  "$METHOD_PCT" "$MIN_METHOD" || FAIL=1

echo ""
if [ "$FAIL" -eq 0 ]; then
  echo "✅ PASS: all coverage thresholds met."
  exit 0
else
  echo "❌ FAIL: one or more coverage thresholds not met."
  exit 1
fi
