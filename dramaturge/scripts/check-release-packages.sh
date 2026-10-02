#!/usr/bin/env bash
#
# Fails unless the NuGet packages in a directory are exactly the ones this repository releases, so that no
# other package, such as a test project made packable by mistake, can be pushed from here. The release workflow
# runs it on its pack output before pushing, and the CI unit-test job runs it on a pack of the solution, so a
# change that would break it fails a pull request rather than a release.
#
# Usage: check-release-packages.sh <directory>

set -euo pipefail

directory="${1:?usage: check-release-packages.sh <directory>}"

# The packages this repository publishes. A new one is added here deliberately.
allowed=(Dramaturge Dramaturge.Browsers Dramaturge.Tool)

shopt -s nullglob
status=0
for package in "$directory"/*.nupkg "$directory"/*.snupkg; do
  # The ID is read from the package's manifest rather than parsed from the file name, which also carries the version.
  id=$(unzip -p "$package" '*.nuspec' | sed -n 's:.*<id>\(.*\)</id>.*:\1:p' | head -n 1)
  if [[ " ${allowed[*]} " == *" ${id} "* ]]; then
    echo "ok   $(basename "$package") ($id)"
  else
    echo "FAIL $(basename "$package") is the package '$id', which this repository does not release."
    status=1
  fi
done

for id in "${allowed[@]}"; do
  # The next part of a package's file name is its version, which starts with a digit.
  matches=("$directory/$id".[0-9]*.nupkg)
  if (( ${#matches[@]} == 0 )); then
    echo "FAIL no package for $id was produced."
    status=1
  fi
done

exit "$status"
