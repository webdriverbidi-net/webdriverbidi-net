#!/bin/bash
# Validates that all [!code-csharp[...]] references in markdown files
# correspond to existing #region markers in the code snippet files.
#
# Matching is file-scoped: a reference is satisfied only by a region defined in
# the very file the reference points at. Comparing bare region names pooled
# across every snippet file would accept a reference whose path is wrong so long
# as some other file happened to define a region of that name, and would report a
# genuinely orphaned region as used because a same-named region elsewhere is
# referenced. Both cases have occurred in this repository.

set -euo pipefail

# The script lives at docs/tools/validate-doc-regions.sh, so the docs directory
# is its parent and the repo root is two levels up. Anchor paths to the script
# location so the script is invariant to the caller's working directory (CI
# invokes it as `./docs/tools/validate-doc-regions.sh` from the repo root;
# local developers may invoke it with relative or absolute paths).
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCS_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
REPO_ROOT="$(cd "$DOCS_DIR/.." && pwd)"
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

# Resolves '.' and '..' segments in an absolute path textually. realpath is not
# available by default on every supported platform, and the target file may be
# checked for existence separately anyway, so a purely lexical resolution is both
# sufficient and portable. Globbing is disabled while splitting so that a path
# containing a glob character cannot expand against the working directory.
normalize_path() {
  normalize_input="$1"
  normalize_result=""
  normalize_saved_ifs="$IFS"
  case "$-" in
    *f*) normalize_had_noglob=1 ;;
    *) normalize_had_noglob=0 ;;
  esac
  set -f
  IFS='/'
  for normalize_part in $normalize_input; do
    case "$normalize_part" in
      '' | '.')
        ;;
      '..')
        normalize_result="${normalize_result%/*}"
        ;;
      *)
        normalize_result="$normalize_result/$normalize_part"
        ;;
    esac
  done
  IFS="$normalize_saved_ifs"
  if [ "$normalize_had_noglob" -eq 0 ]; then
    set +f
  fi
  printf '%s' "$normalize_result"
}

# Renders a path relative to the repository root for display.
display_path() {
  printf '%s' "${1#"$REPO_ROOT"/}"
}

echo "Articles directory: $ARTICLES_DIR"
echo "Code directory: $CODE_DIR"
echo ""

echo "Extracting region markers from snippet files..."
# Each line is "<absolute path to .cs file><TAB><region name>", so that a region
# is only ever matched against a reference that points at its own file.
REGIONS_FILE=$(mktemp)
find "$CODE_DIR" -name "*.cs" -type f ! -path "*/obj/*" ! -path "*/bin/*" | \
  while IFS= read -r csfile; do
    { grep -h "#region" "$csfile" 2>/dev/null || true; } | \
      sed 's/.*#region[[:space:]]*//; s/[[:space:]]*$//' | \
      while IFS= read -r region; do
        [ -n "$region" ] || continue
        printf '%s\t%s\n' "$csfile" "$region"
      done
  done | sort -u > "$REGIONS_FILE"

if [ ! -s "$REGIONS_FILE" ]; then
  echo "❌ ERROR: No #region markers found under $CODE_DIR — refusing to pass vacuously." >&2
  rm -f "$REGIONS_FILE"
  exit 2
fi

echo "Extracting region anchors from markdown references..."
# DocFX reference syntax: [!code-csharp[DISPLAY TITLE](PATH#REGION_NAME)]
# The region name is the fragment after '#' in the path, not the display title.
# Scans docs/articles/ plus the top-level docs pages that also embed code regions
# (docs/index.md, docs/README.md, docs/api/**), while avoiding the example syntax inside
# docs/code/README.md.
# Each line is "<absolute path to .cs file><TAB><region name><TAB><referring markdown file>".
REFERENCES_FILE=$(mktemp)
MARKDOWN_FILES=$(mktemp)
{
  find "$ARTICLES_DIR" -name "*.md" -type f
  find "$DOCS_DIR/api" -name "*.md" -type f 2>/dev/null
  [ -f "$DOCS_DIR/index.md" ] && printf '%s\n' "$DOCS_DIR/index.md"
  [ -f "$DOCS_DIR/README.md" ] && printf '%s\n' "$DOCS_DIR/README.md"
} | sort -u > "$MARKDOWN_FILES"

while IFS= read -r mdfile; do
  mddir="$(dirname "$mdfile")"
  # Match only references that begin their own line (optionally indented). This ignores the syntax
  # examples that appear inline in prose (for example the placeholder in docs/README.md), which are
  # wrapped in backticks rather than written as standalone block references.
  { grep -h "^[[:space:]]*\[!code-csharp\[" "$mdfile" 2>/dev/null || true; } | \
    sed -n 's/.*\[!code-csharp\[[^]]*\](\([^)]*\)).*/\1/p' | \
    while IFS= read -r target; do
      case "$target" in
        *'#'*) ;;
        *) continue ;;
      esac
      # Split on the last '#': everything before it is the path, everything after
      # it is the region name.
      target_path="${target%#*}"
      target_region="${target##*#}"
      [ -n "$target_region" ] || continue
      printf '%s\t%s\t%s\n' "$(normalize_path "$mddir/$target_path")" "$target_region" "$mdfile"
    done
done < "$MARKDOWN_FILES" | sort -u > "$REFERENCES_FILE"

if [ ! -s "$REFERENCES_FILE" ]; then
  echo "❌ ERROR: No [!code-csharp[...]] references found under $ARTICLES_DIR — refusing to pass vacuously." >&2
  rm -f "$REGIONS_FILE" "$REFERENCES_FILE" "$MARKDOWN_FILES"
  exit 2
fi

# The path/region pairs alone, for testing whether a defined region is referenced.
REFERENCE_KEYS_FILE=$(mktemp)
cut -f1,2 "$REFERENCES_FILE" | sort -u > "$REFERENCE_KEYS_FILE"

echo ""
echo "=== Checking for stale references ==="
echo ""

STALE_COUNT=0
while IFS=$'\t' read -r ref_path ref_region ref_source; do
  if grep -qxF -- "$ref_path	$ref_region" "$REGIONS_FILE"; then
    continue
  fi

  if [ ! -f "$ref_path" ]; then
    echo "❌ STALE: $(display_path "$ref_source") references '$ref_region' in $(display_path "$ref_path"), which does not exist"
  else
    echo "❌ STALE: $(display_path "$ref_source") references '$ref_region' in $(display_path "$ref_path"), which defines no such region"
    # The wrong-path case: a region of that name exists, but in another file.
    # Naming those files turns an otherwise puzzling failure into an obvious fix.
    elsewhere=$(awk -F'\t' -v region="$ref_region" '$2 == region { print $1 }' "$REGIONS_FILE")
    if [ -n "$elsewhere" ]; then
      echo "$elsewhere" | while IFS= read -r other; do
        echo "         a region named '$ref_region' is defined in $(display_path "$other") — is the path wrong?"
      done
    fi
  fi

  STALE_COUNT=$((STALE_COUNT + 1))
done < "$REFERENCES_FILE"

echo ""
echo "=== Checking for unused regions ==="
echo ""

UNUSED_COUNT=0
while IFS=$'\t' read -r region_path region_name; do
  if ! grep -qxF -- "$region_path	$region_name" "$REFERENCE_KEYS_FILE"; then
    echo "⚠️  UNUSED: $(display_path "$region_path") defines region '$region_name', which no markdown file references"
    UNUSED_COUNT=$((UNUSED_COUNT + 1))
  fi
done < "$REGIONS_FILE"

# Cleanup
rm "$REGIONS_FILE" "$REFERENCES_FILE" "$REFERENCE_KEYS_FILE" "$MARKDOWN_FILES"

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
