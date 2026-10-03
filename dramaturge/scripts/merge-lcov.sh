#!/usr/bin/env bash
#
# Merges LCOV reports of the same assembly, from different test projects, into one: a line, branch, or method is
# written once, with the hits of every report added, so it is covered if any report covers it.
#
# Only reports of one assembly may be merged. A report names its sources, not its assembly, and the test framework
# packages compile the same shared sources, so merging the reports of two of them would let one package's coverage of
# a shared file hide the other's gaps. check-lcov-thresholds.sh adds up the reports of different assemblies instead.
#
# Usage: merge-lcov.sh <output> <report>...

set -euo pipefail

output="${1:?usage: merge-lcov.sh <output> <report>...}"
shift
if (( $# == 0 )); then
  echo "usage: merge-lcov.sh <output> <report>..." >&2
  exit 2
fi

for report in "$@"; do
  if [ ! -f "$report" ]; then
    echo "❌ ERROR: LCOV file not found: $report" >&2
    exit 2
  fi
done

# A method's name can contain commas, so FN and FNDA split only at the first. A branch never taken is "-", which a
# merged branch keeps only if no report took it.
awk '
  /^SF:/ {
    sf = substr($0, 4)
    if (!(sf in files)) { files[sf] = 1; order[++count] = sf }
    next
  }
  /^FN:/ {
    rest = substr($0, 4); comma = index(rest, ",")
    key = sf SUBSEP substr(rest, comma + 1)
    if (!(key in fn_line)) { fn_line[key] = substr(rest, 1, comma - 1); fn_order[sf, ++fn_count[sf]] = key }
    next
  }
  /^FNDA:/ {
    rest = substr($0, 6); comma = index(rest, ",")
    fn_hits[sf SUBSEP substr(rest, comma + 1)] += substr(rest, 1, comma - 1)
    next
  }
  /^DA:/ {
    split(substr($0, 4), a, ",")
    key = sf SUBSEP a[1]
    if (!(key in da_hits)) da_order[sf, ++da_count[sf]] = a[1]
    da_hits[key] += a[2]
    next
  }
  /^BRDA:/ {
    split(substr($0, 6), a, ",")
    key = sf SUBSEP a[1] "," a[2] "," a[3]
    if (!(key in br_taken)) { br_order[sf, ++br_count[sf]] = a[1] "," a[2] "," a[3]; br_taken[key] = "-" }
    if (a[4] != "-") br_taken[key] = (br_taken[key] == "-" ? 0 : br_taken[key]) + a[4]
    next
  }
  END {
    for (i = 1; i <= count; i++) {
      sf = order[i]
      print "SF:" sf
      found = hit = 0
      for (j = 1; j <= fn_count[sf]; j++) {
        key = fn_order[sf, j]; split(key, parts, SUBSEP)
        print "FN:" fn_line[key] "," parts[2]
      }
      for (j = 1; j <= fn_count[sf]; j++) {
        key = fn_order[sf, j]; split(key, parts, SUBSEP)
        print "FNDA:" (fn_hits[key] + 0) "," parts[2]
        found++; if (fn_hits[key] > 0) hit++
      }
      print "FNF:" found; print "FNH:" hit
      found = hit = 0
      for (j = 1; j <= da_count[sf]; j++) {
        line = da_order[sf, j]
        print "DA:" line "," da_hits[sf, line]
        found++; if (da_hits[sf, line] > 0) hit++
      }
      print "LF:" found; print "LH:" hit
      found = hit = 0
      for (j = 1; j <= br_count[sf]; j++) {
        branch = br_order[sf, j]; taken = br_taken[sf SUBSEP branch]
        print "BRDA:" branch "," taken
        found++; if (taken != "-" && taken > 0) hit++
      }
      print "BRF:" found; print "BRH:" hit
      print "end_of_record"
    }
  }
' "$@" > "$output"
