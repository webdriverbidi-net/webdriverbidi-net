#!/bin/bash
# Downloads the latest published chromium-bidi package from the npm registry and refreshes the
# vendored mapper tab source in third_party/chromium-bidi-mapper.
#
# Usage:
#   update-chromium-bidi.sh [-h|--help]
#
# Exit codes:
#   0 — the vendored mapperTab.js was updated
#   1 — the download, extraction, or copy failed
#   2 — a required tool is missing

set -euo pipefail

case "${1:-}" in
  -h|--help)
    sed -n '2,12p' "$0" | sed 's/^# \{0,1\}//'
    exit 0
    ;;
  "") ;;
  *)
    echo "ERROR: Unknown argument: $1" >&2
    exit 2
    ;;
esac

# Check every external tool up front, so a missing one is reported by name instead of surfacing as a
# confusing failure part-way through the pipeline.
for tool in curl jq tar; do
  if ! command -v "$tool" > /dev/null 2>&1; then
    echo "ERROR: required tool not found on PATH: $tool" >&2
    exit 2
  fi
done

repo_root=$(cd "$(dirname "$0")/.." && pwd)
destination="$repo_root/third_party/chromium-bidi-mapper/mapperTab.js"

# Extract into a temporary directory that is removed however this script exits, rather than into a
# fixed .chromium-bidi directory in the working directory that a failure would leave behind.
work_dir=$(mktemp -d)
trap 'rm -rf "$work_dir"' EXIT

echo "Resolving the latest chromium-bidi release..."
tarball_url=$(curl -fsSL "https://registry.npmjs.org/chromium-bidi/latest" | jq -r '.dist.tarball')
if [ -z "$tarball_url" ] || [ "$tarball_url" = "null" ]; then
  echo "ERROR: the registry response did not contain a tarball URL." >&2
  exit 1
fi

echo "Downloading $tarball_url"
curl -fsSL "$tarball_url" | tar -xz -C "$work_dir"

source_file="$work_dir/package/out/Default/gen/src/mapperTab.js"
if [ ! -f "$source_file" ]; then
  echo "ERROR: mapperTab.js was not found in the package at the expected path:" >&2
  echo "       package/out/Default/gen/src/mapperTab.js" >&2
  echo "       The package layout may have changed; this script needs updating." >&2
  exit 1
fi

cp "$source_file" "$destination"
echo "Updated $destination"
