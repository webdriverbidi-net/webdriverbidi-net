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

# Prints the body of a named #region in a C# file, without the region markers themselves.
extract_region() {
  awk -v want="$2" '
    {
      if ($0 ~ /#region[[:space:]]/) {
        name = $0
        sub(/.*#region[[:space:]]*/, "", name)
        sub(/[[:space:]]*$/, "", name)
        if (name == want) { inregion = 1; next }
      }
      if (inregion && $0 ~ /#endregion/) { inregion = 0; next }
      if (inregion) { print }
    }
  ' "$1"
}

# Reduces code on stdin to a single line of tokens separated by single spaces, so that a comparison
# ignores indentation and line breaks: the same statements are the same code whether a README wrote
# them on one line or the compiled counterpart wrapped them over several.
normalize_code() {
  tr '\n' ' ' | tr -s '[:space:]' ' ' | sed 's/^ //; s/ $//'
}

# Drops the using directives from a block of code. A README sample opens with the usings a reader
# needs; the compiled counterpart carries them at the top of its file, outside the region.
strip_usings() {
  grep -v '^[[:space:]]*using [A-Za-z_][A-Za-z0-9_.]*;[[:space:]]*$' || true
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
  find "$DOCS_DIR/api" -name "*.md" -type f 2>/dev/null || true
  # Written as `if` rather than `[ -f x ] && printf`: the group's status is its last command's, so a
  # short-circuited test there would fail the group, and pipefail would abort the script with no
  # diagnostic. An `if` whose condition is false yields 0, so a missing optional page is not fatal.
  if [ -f "$DOCS_DIR/index.md" ]; then printf '%s\n' "$DOCS_DIR/index.md"; fi
  if [ -f "$DOCS_DIR/README.md" ]; then printf '%s\n' "$DOCS_DIR/README.md"; fi
} | sort -u > "$MARKDOWN_FILES"

# The READMEs packed into the NuGet packages, plus the repository's own. nuget.org renders them as
# plain markdown and knows nothing of DocFX, so their samples cannot be region references; each one
# names the region it mirrors in a marker comment instead, checked further down.
PACKAGE_READMES=$(mktemp)
{
  if [ -f "$REPO_ROOT/README.md" ]; then printf '%s\n' "$REPO_ROOT/README.md"; fi
  find "$REPO_ROOT/src" -maxdepth 2 -name "README.md" -type f
} | sort -u > "$PACKAGE_READMES"

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

# A marker's path is written relative to the repository root, because a packed README is read on
# nuget.org, where a path relative to the file itself would point outside the package.
while IFS= read -r readme; do
  { grep -h '^<!-- readme-csharp:' "$readme" 2>/dev/null || true; } | \
    sed -n 's/^<!-- readme-csharp:[[:space:]]*\([^[:space:]]*\)[[:space:]]*-->.*/\1/p' | \
    while IFS= read -r target; do
      case "$target" in
        *'#'*) ;;
        *) continue ;;
      esac
      target_path="${target%#*}"
      target_region="${target##*#}"
      [ -n "$target_region" ] || continue
      printf '%s\t%s\t%s\n' "$(normalize_path "$REPO_ROOT/$target_path")" "$target_region" "$readme"
    done
done < "$PACKAGE_READMES" | sort -u >> "$REFERENCES_FILE"
sort -u -o "$REFERENCES_FILE" "$REFERENCES_FILE"

if [ ! -s "$REFERENCES_FILE" ]; then
  echo "❌ ERROR: No [!code-csharp[...]] references found under $ARTICLES_DIR — refusing to pass vacuously." >&2
  rm -f "$REGIONS_FILE" "$REFERENCES_FILE" "$MARKDOWN_FILES" "$PACKAGE_READMES"
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

echo ""
echo "=== Checking for unmarked inline C# fences ==="
echo ""

# docs/README.md forbids pasting C# into markdown: it belongs in docs/code as a region, so that it
# compiles and cannot drift from the API. The exception is a fragment that could never compile on its
# own — a list of member names, a signature sketch, a declaration quoted from the library, or code
# written to trip an analyzer. Each of those must say so with a marker on the line directly above it:
#
#     <!-- inline-csharp: why this cannot be a compiled region -->
#
# so that the exception is a deliberate, reviewable choice rather than an oversight. A fence inside a
# blockquote is prose quotation and is not checked.
INLINE_COUNT=0
while IFS= read -r mdfile; do
  while IFS= read -r fence_line; do
    [ -n "$fence_line" ] || continue
    marker_line=$((fence_line - 1))
    if [ "$marker_line" -ge 1 ] && \
       sed -n "${marker_line}p" "$mdfile" | grep -q '^<!-- inline-csharp:'; then
      continue
    fi
    echo "❌ INLINE: $(display_path "$mdfile"):$fence_line opens a csharp fence with no '<!-- inline-csharp: reason -->' marker above it"
    INLINE_COUNT=$((INLINE_COUNT + 1))
  done < <({ grep -n '^```csharp' "$mdfile" 2>/dev/null || true; } | cut -d: -f1)
done < "$MARKDOWN_FILES"

echo ""
echo "=== Checking packed README code blocks ==="
echo ""

# A packed README's samples are read on nuget.org, so they are written out in full rather than
# referenced. Each fence must therefore say which compiled region it mirrors:
#
#     <!-- readme-csharp: docs/code/PackageReadmeSamples.cs#LoggingQuickStart -->
#
# and its body must match that region, so the sample a package's readers copy is one that compiles
# against the current API. A fence that cannot compile at all — an illustration written entirely in
# comments, say — carries the '<!-- inline-csharp: reason -->' marker used elsewhere instead.
README_COUNT=0
while IFS= read -r readme; do
  while IFS= read -r fence_line; do
    [ -n "$fence_line" ] || continue
    marker_line=$((fence_line - 1))
    marker=""
    if [ "$marker_line" -ge 1 ]; then
      marker="$(sed -n "${marker_line}p" "$readme")"
    fi

    case "$marker" in
      '<!-- inline-csharp:'*)
        continue
        ;;
      '<!-- readme-csharp:'*)
        ;;
      *)
        echo "❌ README: $(display_path "$readme"):$fence_line opens a csharp fence with no '<!-- readme-csharp: path#Region -->' marker above it"
        README_COUNT=$((README_COUNT + 1))
        continue
        ;;
    esac

    target="$(printf '%s' "$marker" | sed -n 's/^<!-- readme-csharp:[[:space:]]*\([^[:space:]]*\)[[:space:]]*-->.*/\1/p')"
    region_file="$(normalize_path "$REPO_ROOT/${target%#*}")"
    region_name="${target##*#}"
    if [ ! -f "$region_file" ]; then
      echo "❌ README: $(display_path "$readme"):$fence_line names $(display_path "$region_file"), which does not exist"
      README_COUNT=$((README_COUNT + 1))
      continue
    fi

    # The fence body is everything between its opening line and the next closing fence.
    fence_body=$(mktemp)
    awk -v start="$fence_line" 'NR > start { if ($0 ~ /^```/) { exit } print }' "$readme" > "$fence_body"

    # Every using directive the README shows must be a using directive of the file the region lives
    # in; otherwise the block a reader pastes would not compile even though the region does.
    missing_using=0
    while IFS= read -r using_line; do
      [ -n "$using_line" ] || continue
      if ! grep -qxF -- "$using_line" "$region_file"; then
        echo "❌ README: $(display_path "$readme"):$fence_line shows '$using_line', which $(display_path "$region_file") does not declare"
        missing_using=1
      fi
    done < <({ grep -h '^using [A-Za-z_][A-Za-z0-9_.]*;$' "$fence_body" 2>/dev/null || true; })
    if [ "$missing_using" -ne 0 ]; then
      README_COUNT=$((README_COUNT + 1))
      rm -f "$fence_body"
      continue
    fi

    fence_code="$(strip_usings < "$fence_body" | normalize_code)"
    region_code="$(extract_region "$region_file" "$region_name" | normalize_code)"
    rm -f "$fence_body"

    if [ -z "$region_code" ]; then
      echo "❌ README: $(display_path "$readme"):$fence_line names region '$region_name', which $(display_path "$region_file") does not define"
      README_COUNT=$((README_COUNT + 1))
      continue
    fi

    if [ "$fence_code" != "$region_code" ]; then
      echo "❌ README: $(display_path "$readme"):$fence_line has drifted from $(display_path "$region_file")#$region_name"
      echo "         README: $fence_code"
      echo "         region: $region_code"
      README_COUNT=$((README_COUNT + 1))
    fi
  done < <({ grep -n '^```csharp' "$readme" 2>/dev/null || true; } | cut -d: -f1)
done < "$PACKAGE_READMES"

echo ""
echo "=== Checking quick-reference rows ==="
echo ""

# The cheat sheet states its code in table cells, which DocFX cannot fill from a region, so each row
# is duplicated in docs/code (see QuickReferenceRowSamples.cs). Requiring the row to appear there
# verbatim, whitespace aside, means the compiler sees every line the cheat sheet teaches: a row using
# an API that has changed, or edited without its counterpart, fails here. A row whose code is a
# deliberate sketch rather than a statement opts out with '<!-- not-compiled: reason -->' in the cell.
ROW_COUNT=0
QUICK_REFERENCE="$ARTICLES_DIR/quick-reference.md"
if [ -f "$QUICK_REFERENCE" ]; then
  SNIPPET_TEXT=$(mktemp)
  # One line per snippet file, so that a row cannot be "found" by matching across a file boundary.
  find "$CODE_DIR" -name "*.cs" -type f ! -path "*/obj/*" ! -path "*/bin/*" | \
    while IFS= read -r csfile; do
      normalize_code < "$csfile"
      printf '\n'
    done > "$SNIPPET_TEXT"

  while IFS= read -r row; do
    case "$row" in
      *'<!-- not-compiled:'*) continue ;;
    esac
    # The code is the first backticked span in the row's second cell; a span after it is prose.
    row_code="$(printf '%s' "$row" | sed -n 's/^|[^|]*|[^`]*`\([^`]*\)`.*/\1/p')"
    [ -n "$row_code" ] || continue
    row_code="$(printf '%s' "$row_code" | normalize_code)"
    if ! grep -qF -- "$row_code" "$SNIPPET_TEXT"; then
      echo "❌ ROW: $(display_path "$QUICK_REFERENCE") has a row whose code is in no compiled snippet:"
      echo "         $row_code"
      ROW_COUNT=$((ROW_COUNT + 1))
    fi
  done < <({ grep -h '^| ' "$QUICK_REFERENCE" 2>/dev/null || true; })

  rm -f "$SNIPPET_TEXT"
fi

# Cleanup
rm "$REGIONS_FILE" "$REFERENCES_FILE" "$REFERENCE_KEYS_FILE" "$MARKDOWN_FILES" "$PACKAGE_READMES"

echo ""
echo "=== Summary ==="
echo "Stale references (referenced but no region): $STALE_COUNT"
echo "Unused regions (region but no reference): $UNUSED_COUNT"
echo "Unmarked inline C# fences: $INLINE_COUNT"
echo "Unmarked or drifted packed README blocks: $README_COUNT"
echo "Quick-reference rows with no compiled counterpart: $ROW_COUNT"

if [ $STALE_COUNT -gt 0 ] || [ $INLINE_COUNT -gt 0 ] || [ $README_COUNT -gt 0 ] || [ $ROW_COUNT -gt 0 ]; then
  echo ""
  [ $STALE_COUNT -gt 0 ] && echo "❌ FAIL: Found $STALE_COUNT stale reference(s)"
  [ $INLINE_COUNT -gt 0 ] && echo "❌ FAIL: Found $INLINE_COUNT unmarked inline C# fence(s)"
  [ $README_COUNT -gt 0 ] && echo "❌ FAIL: Found $README_COUNT packed README block(s) unmarked or out of step with their region"
  [ $ROW_COUNT -gt 0 ] && echo "❌ FAIL: Found $ROW_COUNT quick-reference row(s) with no compiled counterpart"
  exit 1
else
  echo ""
  echo "✅ PASS: All markdown references have corresponding region markers, every inline C# fence is marked, every packed README block matches its region, and every quick-reference row is compiled"
  exit 0
fi
