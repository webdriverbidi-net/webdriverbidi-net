#!/bin/bash
# Validates that all [!code-csharp[...]] references in markdown files
# correspond to existing #region markers in the code snippet files.

set -euo pipefail

# The script lives at docs/tools/validate-doc-regions.sh, so the docs directory
# is its parent and the repo root is two levels up. Anchor paths to the script
# location so the script is invariant to the caller's working directory (CI
# invokes it as `./docs/tools/validate-doc-regions.sh` from the repo root;
# local developers may invoke it with relative or absolute paths).
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCS_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
ARTICLES_DIR="$DOCS_DIR/articles"
CODE_DIR="$DOCS_DIR/code"

if [ ! -d "$ARTICLES_DIR" ]; then
  echo "❌ ERROR: Articles directory not found: $ARTICLES_DIR" >&2
  exit 2
fi
if [ ! -d "$CODE_DIR" ]; then
  echo "❌ ERROR: Code-sample directory not found: $CODE_DIR" >&2
  exit 2
fi

echo "Articles directory: $ARTICLES_DIR"
echo "Code directory: $CODE_DIR"
echo ""

echo "Extracting region markers from snippet files..."
REGIONS_FILE=$(mktemp)
find "$CODE_DIR" -name "*.cs" -type f ! -path "*/obj/*" ! -path "*/bin/*" -exec grep -h "#region" {} \; | \
  sed 's/.*#region \(.*\)/\1/' | \
  sort -u > "$REGIONS_FILE"

if [ ! -s "$REGIONS_FILE" ]; then
  echo "❌ ERROR: No #region markers found under $CODE_DIR — refusing to pass vacuously." >&2
  rm -f "$REGIONS_FILE"
  exit 2
fi

echo "Extracting region anchors from markdown references..."
# DocFX reference syntax: [!code-csharp[DISPLAY TITLE](PATH#REGION_NAME)]
# The region name is the fragment after '#' in the path, not the display title.
# Scans only docs/articles/ to avoid picking up example syntax inside docs/code/README.md.
REFERENCES_FILE=$(mktemp)
find "$ARTICLES_DIR" -name "*.md" -type f -exec grep -h "\[!code-csharp\[" {} \; | \
  sed -n 's/.*\[!code-csharp\[[^]]*\](\([^)]*\)).*/\1/p' | \
  grep '#' | \
  sed 's/.*#\([^)]*\)$/\1/' | \
  sort -u > "$REFERENCES_FILE"

if [ ! -s "$REFERENCES_FILE" ]; then
  echo "❌ ERROR: No [!code-csharp[...]] references found under $ARTICLES_DIR — refusing to pass vacuously." >&2
  rm -f "$REGIONS_FILE" "$REFERENCES_FILE"
  exit 2
fi

echo ""
echo "=== Checking for stale references ==="
echo ""

STALE_COUNT=0
while IFS= read -r reference; do
  if ! grep -qx -- "$reference" "$REGIONS_FILE"; then
    echo "❌ STALE: Referenced in docs but no matching region: '$reference'"
    STALE_COUNT=$((STALE_COUNT + 1))
  fi
done < "$REFERENCES_FILE"

echo ""
echo "=== Checking for unused regions ==="
echo ""

UNUSED_COUNT=0
while IFS= read -r region; do
  if ! grep -qx -- "$region" "$REFERENCES_FILE"; then
    echo "⚠️  UNUSED: Region exists but not referenced: '$region'"
    UNUSED_COUNT=$((UNUSED_COUNT + 1))
  fi
done < "$REGIONS_FILE"

# Cleanup
rm "$REGIONS_FILE" "$REFERENCES_FILE"

echo ""
echo "=== Summary ==="
echo "Stale references (referenced but no region): $STALE_COUNT"
echo "Unused regions (region but no reference): $UNUSED_COUNT"

if [ $STALE_COUNT -gt 0 ]; then
  echo ""
  echo "❌ FAIL: Found $STALE_COUNT stale reference(s)"
  exit 1
else
  echo ""
  echo "✅ PASS: All markdown references have corresponding region markers"
  exit 0
fi
