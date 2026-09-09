#!/bin/bash
# Prepares the working tree for a release: applies every change that bumping the version number
# requires, then verifies the result. Tagging and pushing the tag stay manual — this script never
# commits, tags, or pushes anything.
#
# Usage:
#   prep-release.sh [--dry-run] [--allow-dirty] [--skip-verification] <version>
#
# The version is the release version without the leading "v" (for example, 0.0.58 or
# 0.0.58-beta.1), matching the tag patterns release.yml triggers on.
#
# What it changes:
#   1. src/WebDriverBiDi.Analyzers/AnalyzerReleases.{Unshipped,Shipped}.md — moves the pending
#      rule entries into a "## Release <version>" section of the shipped file and empties the
#      unshipped file back to its header. The Roslyn release-tracking analyzers treat a rule as
#      shipped only once it appears in the shipped file under a release heading.
#   2. docs/articles/getting-started.md — updates the pinned <PackageReference> version the
#      "Pinning an exact version" section shows to consumers. Skipped for prerelease versions:
#      that page recommends the current stable release, and the docs site is republished from
#      every tag, prerelease tags included.
#
# Nothing else in the tree carries the release version. AssemblyVersion, FileVersion,
# InformationalVersion, and PackageVersion are all derived from the tag by release.yml at build
# time, and the NuGet badge and package links in the READMEs are version-independent.
#
# Verification runs the local subset of the release gates: a Release build with -warnaserror, the
# three unit test projects, and the documentation region validation. The coverage thresholds, the
# analyzer package layout check, and the compatibility, integration, and Windows matrices remain
# CI's job and run again on the tag push.
#
# Exit codes:
#   0 — the working tree is prepared (or, under --dry-run, reported)
#   1 — a preflight check or a verification step failed
#   2 — configuration or input error

set -euo pipefail

DRY_RUN=0
ALLOW_DIRTY=0
SKIP_VERIFICATION=0
VERSION=""

while [ $# -gt 0 ]; do
  case "$1" in
    --dry-run)            DRY_RUN=1; shift ;;
    --allow-dirty)        ALLOW_DIRTY=1; shift ;;
    --skip-verification)  SKIP_VERIFICATION=1; shift ;;
    -h|--help)
      sed -n '2,34p' "$0" | sed 's/^# \{0,1\}//'
      exit 0
      ;;
    --*)
      echo "❌ ERROR: Unknown flag: $1" >&2
      exit 2
      ;;
    *)
      if [ -n "$VERSION" ]; then
        echo "❌ ERROR: More than one version supplied: '$VERSION' and '$1'" >&2
        exit 2
      fi
      VERSION="$1"
      shift
      ;;
  esac
done

if [ -z "$VERSION" ]; then
  echo "❌ ERROR: No version supplied." >&2
  echo "Usage: $0 [--dry-run] [--allow-dirty] [--skip-verification] <version>" >&2
  exit 2
fi

# Accept a leading "v" for the benefit of anyone typing the tag name, but work in terms of the
# bare version everywhere below: the tag is v-prefixed, the package version is not.
VERSION="${VERSION#v}"

# The same shape release.yml's tag filter accepts. Rejecting anything else here keeps a typo from
# producing a prepared tree that no release workflow will ever pick up.
if ! printf '%s' "$VERSION" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$'; then
  echo "❌ ERROR: '$VERSION' is not a valid release version (expected X.Y.Z or X.Y.Z-qualifier)." >&2
  exit 2
fi

TAG="v${VERSION}"
CORE_VERSION="${VERSION%%-*}"
QUALIFIER="${VERSION#"${CORE_VERSION}"}"
QUALIFIER="${QUALIFIER#-}"

repo_root=$(cd "$(dirname "$0")/.." && pwd)
cd "$repo_root"

ANALYZER_UNSHIPPED="src/WebDriverBiDi.Analyzers/AnalyzerReleases.Unshipped.md"
ANALYZER_SHIPPED="src/WebDriverBiDi.Analyzers/AnalyzerReleases.Shipped.md"
GETTING_STARTED="docs/articles/getting-started.md"

# Collected as the run goes and printed at the end, so a reminder raised during the mutation phase
# is not scrolled away by several minutes of build output.
REMINDERS=()

echo "Preparing release $TAG"
echo

# ---------------------------------------------------------------------------------------------
# Preflight
# ---------------------------------------------------------------------------------------------
echo "Preflight"

for tool in git awk sed grep; do
  if ! command -v "$tool" > /dev/null 2>&1; then
    echo "❌ ERROR: required tool not found on PATH: $tool" >&2
    exit 2
  fi
done

if [ "$SKIP_VERIFICATION" -eq 0 ] && ! command -v dotnet > /dev/null 2>&1; then
  echo "❌ ERROR: dotnet not found on PATH; install it or pass --skip-verification." >&2
  exit 2
fi

for file in "$ANALYZER_UNSHIPPED" "$ANALYZER_SHIPPED" "$GETTING_STARTED"; do
  if [ ! -f "$file" ]; then
    echo "❌ ERROR: expected file not found: $file" >&2
    exit 2
  fi
done

if git rev-parse -q --verify "refs/tags/$TAG" > /dev/null; then
  echo "❌ ERROR: tag $TAG already exists locally. Pick a new version, or delete the tag first." >&2
  exit 1
fi

# A tag that exists only on the remote fails at push time, after the tree has already been
# committed as a release prep. Network failures are not fatal: the check is an early warning, not
# a gate the script can guarantee.
if remote_tags=$(git ls-remote --tags origin "refs/tags/$TAG" 2>/dev/null); then
  if [ -n "$remote_tags" ]; then
    echo "❌ ERROR: tag $TAG already exists on origin." >&2
    exit 1
  fi
  echo "  ✔ $TAG is unused locally and on origin"
else
  echo "  ! could not reach origin to check for an existing $TAG tag; verified locally only"
fi

# Compare only the X.Y.Z triple. An equal triple is legitimate (promoting 0.0.58-beta.1 to
# 0.0.58), and comparing qualifiers would mean reimplementing SemVer precedence for no gain; the
# tag-existence checks above already catch an exact repeat.
latest_tag=$(git tag --list 'v[0-9]*' | sed 's/^v//' | awk -F'[.-]' '{ printf "%d %d %d %s\n", $1, $2, $3, $0 }' | sort -k1,1n -k2,2n -k3,3n | tail -1 | awk '{ print $4 }')
if [ -n "$latest_tag" ]; then
  if ! awk -v new="$CORE_VERSION" -v old="${latest_tag%%-*}" '
    BEGIN {
      split(new, n, ".")
      split(old, o, ".")
      for (i = 1; i <= 3; i++) {
        if (n[i] + 0 > o[i] + 0) { exit 0 }
        if (n[i] + 0 < o[i] + 0) { exit 1 }
      }
      exit 0
    }'; then
    echo "❌ ERROR: $VERSION is lower than the most recent release, $latest_tag." >&2
    exit 1
  fi
  echo "  ✔ $VERSION follows the most recent release, $latest_tag"
fi

if [ -n "$(git status --porcelain)" ]; then
  if [ "$ALLOW_DIRTY" -eq 0 ]; then
    echo "❌ ERROR: the working tree has uncommitted changes." >&2
    echo "         Commit or stash them so the release prep changes stand on their own," >&2
    echo "         or re-run with --allow-dirty." >&2
    exit 1
  fi
  echo "  ! the working tree has uncommitted changes (--allow-dirty)"
else
  echo "  ✔ working tree is clean"
fi

current_branch=$(git rev-parse --abbrev-ref HEAD)
if [ "$current_branch" != "main" ]; then
  # Not an error: release prep has historically gone through a pull request. It matters only that
  # the commit reaches main before the tag is cut, which release.yml enforces on its side.
  REMINDERS+=("You are on '$current_branch', not main. release.yml refuses to publish a tag that is not an ancestor of origin/main, so land these changes on main before tagging.")
fi

echo

# ---------------------------------------------------------------------------------------------
# Mutations
# ---------------------------------------------------------------------------------------------
echo "Release version updates"

# Writes a prepared file into place, or shows what it would change under --dry-run.
apply_change() {
  local target="$1" prepared="$2"

  if cmp -s "$target" "$prepared"; then
    rm -f "$prepared"
    return 1
  fi

  if [ "$DRY_RUN" -eq 1 ]; then
    diff -u "$target" "$prepared" | sed 's/^/      /' || true
    rm -f "$prepared"
  else
    cat "$prepared" > "$target"
    rm -f "$prepared"
  fi
  return 0
}

prep_analyzer_release_tracking() {
  local header body shipped_new unshipped_new

  # The file opens with ';' comment lines that stay put; everything after them is the pending
  # release's content, which moves wholesale into the shipped file.
  header=$(awk '/^;/ { print; next } { exit }' "$ANALYZER_UNSHIPPED")
  body=$(awk '
    BEGIN { inheader = 1 }
    inheader && /^;/ { next }
    { inheader = 0; buf[n++] = $0 }
    END {
      start = 0
      while (start < n && buf[start] ~ /^[[:space:]]*$/) { start++ }
      end = n - 1
      while (end >= start && buf[end] ~ /^[[:space:]]*$/) { end-- }
      for (i = start; i <= end; i++) { print buf[i] }
    }' "$ANALYZER_UNSHIPPED")

  if [ -z "$body" ]; then
    echo "  • analyzer release tracking: no unshipped rules to move"
    return
  fi

  if grep -q "^## Release ${VERSION}\$" "$ANALYZER_SHIPPED"; then
    echo "  ! analyzer release tracking: '## Release ${VERSION}' is already in Shipped.md, but"
    echo "    Unshipped.md is not empty. Resolve this by hand; leaving both files alone."
    REMINDERS+=("$ANALYZER_SHIPPED already has a '## Release ${VERSION}' section while $ANALYZER_UNSHIPPED still holds entries. Merge them manually.")
    return
  fi

  shipped_new=$(mktemp)
  unshipped_new=$(mktemp)

  # Normalize away any trailing blank lines before appending, so the new section is always
  # separated from the previous one by exactly one blank line.
  awk '
    { buf[n++] = $0 }
    END {
      last = n - 1
      while (last >= 0 && buf[last] ~ /^[[:space:]]*$/) { last-- }
      for (i = 0; i <= last; i++) { print buf[i] }
    }' "$ANALYZER_SHIPPED" > "$shipped_new"
  {
    printf '\n## Release %s\n\n' "$VERSION"
    printf '%s\n' "$body"
  } >> "$shipped_new"

  printf '%s\n' "$header" > "$unshipped_new"

  local moved_rules
  moved_rules=$(printf '%s\n' "$body" | grep -cE '^BIDI[0-9]+ ' || true)
  echo "  ✔ analyzer release tracking: moved $moved_rules rule entries into '## Release ${VERSION}'"
  apply_change "$ANALYZER_SHIPPED" "$shipped_new" || true
  apply_change "$ANALYZER_UNSHIPPED" "$unshipped_new" || true

  if printf '%s\n' "$body" | grep -q '^### Removed Rules'; then
    REMINDERS+=("This release removes analyzer rules. The versioning section of docs/articles/advanced/api-design.md cites removals by version, and docs/articles/advanced/analyzers.md lists the rules; check both.")
  fi
}

prep_documentation_version_pin() {
  local current pinned_new

  current=$(sed -n 's|.*<PackageReference Include="WebDriverBiDi" Version="\([^"]*\)".*|\1|p' "$GETTING_STARTED" | head -1)
  if [ -z "$current" ]; then
    echo "❌ ERROR: no pinned <PackageReference Include=\"WebDriverBiDi\" ...> found in $GETTING_STARTED." >&2
    echo "         The page changed shape; update this script to match it." >&2
    exit 1
  fi

  if [ -n "$QUALIFIER" ]; then
    echo "  • documentation version pin: left at $current (prerelease versions are not recommended there)"
    return
  fi

  if [ "$current" = "$VERSION" ]; then
    echo "  • documentation version pin: already $VERSION"
    return
  fi

  pinned_new=$(mktemp)
  sed 's|\(<PackageReference Include="WebDriverBiDi" Version="\)[^"]*\(" */>\)|\1'"$VERSION"'\2|g' "$GETTING_STARTED" > "$pinned_new"
  echo "  ✔ documentation version pin: $current → $VERSION"
  apply_change "$GETTING_STARTED" "$pinned_new" || true
}

prep_analyzer_release_tracking
prep_documentation_version_pin

echo

# ---------------------------------------------------------------------------------------------
# Verification
# ---------------------------------------------------------------------------------------------
if [ "$SKIP_VERIFICATION" -eq 1 ]; then
  echo "Verification skipped (--skip-verification)"
elif [ "$DRY_RUN" -eq 1 ]; then
  echo "Verification skipped (--dry-run made no changes to verify)"
else
  echo "Verification"

  run_step() {
    local description="$1"
    shift
    echo "  → $description"
    if ! "$@"; then
      echo "❌ ERROR: $description failed. The release prep changes are still in the working tree;" >&2
      echo "         fix the failure and re-run before tagging." >&2
      exit 1
    fi
  }

  # -warnaserror matches _tests.yml: a new warning fails the release build, so surface it here
  # rather than after the tag is pushed.
  run_step "restore" dotnet restore
  run_step "build (Release, -warnaserror)" dotnet build --configuration Release --no-restore -warnaserror
  run_step "unit tests (library)" dotnet test --project test/WebDriverBiDi.Tests --configuration Release --no-build
  run_step "unit tests (analyzers)" dotnet test --project test/WebDriverBiDi.Analyzers.Tests --configuration Release --no-build
  run_step "unit tests (logging)" dotnet test --project test/WebDriverBiDi.Logging.Tests --configuration Release --no-build
  run_step "documentation region validation" ./docs/tools/validate-doc-regions.sh

  echo "  ✔ all verification steps passed"
fi

echo

# ---------------------------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------------------------
if [ ${#REMINDERS[@]} -gt 0 ]; then
  echo "Before tagging"
  for reminder in "${REMINDERS[@]}"; do
    echo "  ! $reminder"
  done
  echo
fi

if [ "$DRY_RUN" -eq 1 ]; then
  echo "Dry run complete; nothing was written. Re-run without --dry-run to apply."
  exit 0
fi

if [ -z "$(git status --porcelain)" ]; then
  echo "✅ Nothing to change: the tree is already prepared for $TAG."
else
  echo "✅ The working tree is prepared for $TAG. Changed files:"
  git status --porcelain | sed 's/^/     /'
fi

cat <<EOF

Next steps (this script deliberately does none of them):
  1. Review the changes:      git diff
  2. Commit them:             git commit -am "chore: prep $TAG release"
  3. Get the commit onto main (directly or through a pull request).
  4. Tag that commit:         git tag $TAG
  5. Push the tag:            git push origin $TAG
EOF
