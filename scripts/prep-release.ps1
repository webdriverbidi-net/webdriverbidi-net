#!/usr/bin/env pwsh
# Prepares the working tree for a release: applies every change that bumping the version number
# requires, then verifies the result. Tagging and pushing the tag stay manual — this script never
# commits, tags, or pushes anything.
#
# Usage:
#   prep-release.ps1 [-DryRun] [-AllowDirty] [-SkipVerification] [-Help] <version>
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
# This is the PowerShell counterpart of prep-release.sh; the two make identical changes and use
# the same exit codes. The documentation region validation is a bash script with no PowerShell
# equivalent, so that one step is skipped when bash is not on PATH (Git for Windows provides it).
#
# Exit codes:
#   0 — the working tree is prepared (or, under -DryRun, reported)
#   1 — a preflight check or a verification step failed
#   2 — configuration or input error

[CmdletBinding()]
param(
    [switch] $DryRun,
    [switch] $AllowDirty,
    [switch] $SkipVerification,
    [switch] $Help,
    [Parameter(ValueFromRemainingArguments)]
    [string[]] $Arguments = @()
)

$ErrorActionPreference = 'Stop'

# PowerShell 7.4 and later turn a non-zero exit from a native command into a terminating error when
# $ErrorActionPreference is 'Stop'. Several git commands here exit non-zero as a normal answer
# rather than as a failure — `git rev-parse --verify` on a tag that does not exist is the whole
# point of the check — so opt out and test $LASTEXITCODE explicitly instead. On Windows PowerShell,
# where this preference does not exist, the assignment is inert.
$PSNativeCommandUseErrorActionPreference = $false

# -Help prints the header block above, matching the -h/--help behaviour of prep-release.sh.
if ($Help) {
    Get-Content -LiteralPath $PSCommandPath |
        Select-Object -Skip 1 -First 37 |
        ForEach-Object { $_ -replace '^# ?', '' } |
        Write-Host
    exit 0
}

# Anything that still looks like a flag was not bound by param(), so it is an unknown flag. Reading
# it as the version instead would turn a typo into a confusing "not a valid release version".
$unknownFlags = @($Arguments | Where-Object { $_ -like '-*' })
if ($unknownFlags.Count -gt 0) {
    Write-Host "ERROR: Unknown flag: $($unknownFlags[0])" -ForegroundColor Red
    exit 2
}

if ($Arguments.Count -gt 1) {
    Write-Host "ERROR: More than one version supplied: '$($Arguments[0])' and '$($Arguments[1])'" -ForegroundColor Red
    exit 2
}

if ($Arguments.Count -eq 0) {
    Write-Host 'ERROR: No version supplied.' -ForegroundColor Red
    Write-Host 'Usage: prep-release.ps1 [-DryRun] [-AllowDirty] [-SkipVerification] <version>' -ForegroundColor Red
    exit 2
}

# Accept a leading "v" for the benefit of anyone typing the tag name, but work in terms of the
# bare version everywhere below: the tag is v-prefixed, the package version is not.
$version = $Arguments[0] -replace '^v', ''

# The same shape release.yml's tag filter accepts. Rejecting anything else here keeps a typo from
# producing a prepared tree that no release workflow will ever pick up.
if ($version -notmatch '^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$') {
    Write-Host "ERROR: '$version' is not a valid release version (expected X.Y.Z or X.Y.Z-qualifier)." -ForegroundColor Red
    exit 2
}

$tag = "v$version"
$coreVersion = ($version -split '-', 2)[0]
$qualifier = ''
if ($version.Contains('-')) {
    $qualifier = ($version -split '-', 2)[1]
}

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSCommandPath)

$analyzerUnshipped = Join-Path $repoRoot 'src/WebDriverBiDi.Analyzers/AnalyzerReleases.Unshipped.md'
$analyzerShipped = Join-Path $repoRoot 'src/WebDriverBiDi.Analyzers/AnalyzerReleases.Shipped.md'
$gettingStarted = Join-Path $repoRoot 'docs/articles/getting-started.md'

# Paths as they read in output and in git's own reporting, rather than as absolute paths.
$analyzerUnshippedName = 'src/WebDriverBiDi.Analyzers/AnalyzerReleases.Unshipped.md'
$analyzerShippedName = 'src/WebDriverBiDi.Analyzers/AnalyzerReleases.Shipped.md'
$gettingStartedName = 'docs/articles/getting-started.md'

# Collected as the run goes and printed at the end, so a reminder raised during the mutation phase
# is not scrolled away by several minutes of build output.
$reminders = New-Object System.Collections.Generic.List[string]

# .gitattributes normalizes every text file in this repository to LF, and the files these functions
# rewrite are LF on disk on every platform. Set-Content would write CRLF on Windows and turn a
# three-line change into a whole-file diff, so write the bytes explicitly instead: LF joins, and
# UTF-8 with no BOM, which is what the files already are.
function Get-FileLines {
    param([string] $Path)

    # Explicit UTF-8 rather than Get-Content: Windows PowerShell 5.1 decodes a file with no BOM
    # using the system ANSI codepage, which would turn the em dashes in getting-started.md into
    # mojibake and then write that back out as UTF-8.
    $text = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    if ($text.Length -eq 0) {
        return @()
    }

    # Split on LF after folding CRLF away, so a checkout that somehow has CRLF endings is read
    # correctly and rewritten as the LF that .gitattributes calls for.
    $lines = ($text -replace "`r`n", "`n") -split "`n"

    # A trailing newline leaves a final empty element that the join in ConvertTo-FileText would
    # otherwise turn into a second one.
    if ($lines[$lines.Count - 1] -eq '') {
        $lines = $lines[0..($lines.Count - 2)]
    }

    return @($lines)
}

function ConvertTo-FileText {
    param([string[]] $Lines)

    if ($Lines.Count -eq 0) {
        return ''
    }

    return (($Lines -join "`n") + "`n")
}

function Write-FileText {
    param([string] $Path, [string] $Text)

    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}

# Writes prepared content into place, or shows what it would change under -DryRun. Returns $true
# when the content differs from what is already on disk.
function Set-PreparedContent {
    param([string] $Path, [string] $Text)

    $current = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    if ($current -eq $Text) {
        return $false
    }

    if ($DryRun) {
        # git is already a hard requirement, and `git diff --no-index` renders the same unified
        # diff the shell script gets from diff(1) without depending on a diff binary being present
        # on Windows. It exits 1 when the files differ, which is the expected outcome here.
        $temporaryFile = [System.IO.Path]::GetTempFileName()
        try {
            Write-FileText $temporaryFile $Text
            $diff = & git --no-pager diff --no-index -- $Path $temporaryFile 2>$null
            $diff | ForEach-Object { Write-Host "      $_" }
        }
        finally {
            Remove-Item -LiteralPath $temporaryFile -Force -ErrorAction SilentlyContinue
        }
    }
    else {
        Write-FileText $Path $Text
    }

    return $true
}

Write-Host "Preparing release $tag"
Write-Host ''

# ---------------------------------------------------------------------------------------------
# Preflight
# ---------------------------------------------------------------------------------------------
Write-Host 'Preflight'

if (-not (Get-Command 'git' -ErrorAction SilentlyContinue)) {
    Write-Host 'ERROR: required tool not found on PATH: git' -ForegroundColor Red
    exit 2
}

if (-not $SkipVerification -and -not (Get-Command 'dotnet' -ErrorAction SilentlyContinue)) {
    Write-Host 'ERROR: dotnet not found on PATH; install it or pass -SkipVerification.' -ForegroundColor Red
    exit 2
}

foreach ($file in @($analyzerUnshipped, $analyzerShipped, $gettingStarted)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        Write-Host "ERROR: expected file not found: $file" -ForegroundColor Red
        exit 2
    }
}

& git -C $repoRoot rev-parse -q --verify "refs/tags/$tag" 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "ERROR: tag $tag already exists locally. Pick a new version, or delete the tag first." -ForegroundColor Red
    exit 1
}

# A tag that exists only on the remote fails at push time, after the tree has already been
# committed as a release prep. Network failures are not fatal: the check is an early warning, not
# a gate the script can guarantee.
$remoteTags = & git -C $repoRoot ls-remote --tags origin "refs/tags/$tag" 2>$null
if ($LASTEXITCODE -eq 0) {
    if (@($remoteTags | Where-Object { $_ }).Count -gt 0) {
        Write-Host "ERROR: tag $tag already exists on origin." -ForegroundColor Red
        exit 1
    }
    Write-Host "  v $tag is unused locally and on origin"
}
else {
    Write-Host "  ! could not reach origin to check for an existing $tag tag; verified locally only"
}

# Compare only the X.Y.Z triple. An equal triple is legitimate (promoting 0.0.58-beta.1 to
# 0.0.58), and comparing qualifiers would mean reimplementing SemVer precedence for no gain; the
# tag-existence checks above already catch an exact repeat.
$releasedVersions = @(
    & git -C $repoRoot tag --list 'v[0-9]*' |
        ForEach-Object {
            $candidate = ($_ -replace '^v', '')
            $parts = (($candidate -split '-', 2)[0]) -split '\.'
            if ($parts.Count -eq 3 -and @($parts -notmatch '^\d+$').Count -eq 0) {
                [PSCustomObject]@{
                    Tag   = $candidate
                    Major = [int] $parts[0]
                    Minor = [int] $parts[1]
                    Patch = [int] $parts[2]
                }
            }
        }
)

if ($releasedVersions.Count -gt 0) {
    $latest = $releasedVersions | Sort-Object Major, Minor, Patch | Select-Object -Last 1
    $newParts = $coreVersion -split '\.'
    $newTriple = @([int] $newParts[0], [int] $newParts[1], [int] $newParts[2])
    $latestTriple = @($latest.Major, $latest.Minor, $latest.Patch)

    for ($i = 0; $i -lt 3; $i++) {
        if ($newTriple[$i] -gt $latestTriple[$i]) {
            break
        }

        if ($newTriple[$i] -lt $latestTriple[$i]) {
            Write-Host "ERROR: $version is lower than the most recent release, $($latest.Tag)." -ForegroundColor Red
            exit 1
        }
    }

    Write-Host "  v $version follows the most recent release, $($latest.Tag)"
}

$status = @(& git -C $repoRoot status --porcelain | Where-Object { $_ })
if ($status.Count -gt 0) {
    if (-not $AllowDirty) {
        Write-Host 'ERROR: the working tree has uncommitted changes.' -ForegroundColor Red
        Write-Host '       Commit or stash them so the release prep changes stand on their own,' -ForegroundColor Red
        Write-Host '       or re-run with -AllowDirty.' -ForegroundColor Red
        exit 1
    }
    Write-Host '  ! the working tree has uncommitted changes (-AllowDirty)'
}
else {
    Write-Host '  v working tree is clean'
}

$currentBranch = (@(& git -C $repoRoot rev-parse --abbrev-ref HEAD) -join '').Trim()
if ($currentBranch -ne 'main') {
    # Not an error: release prep has historically gone through a pull request. It matters only that
    # the commit reaches main before the tag is cut, which release.yml enforces on its side.
    $reminders.Add("You are on '$currentBranch', not main. release.yml refuses to publish a tag that is not an ancestor of origin/main, so land these changes on main before tagging.")
}

Write-Host ''

# ---------------------------------------------------------------------------------------------
# Mutations
# ---------------------------------------------------------------------------------------------
Write-Host 'Release version updates'

function Update-AnalyzerReleaseTracking {
    $unshippedLines = Get-FileLines $analyzerUnshipped

    # The file opens with ';' comment lines that stay put; everything after them is the pending
    # release's content, which moves wholesale into the shipped file.
    $header = New-Object System.Collections.Generic.List[string]
    $body = New-Object System.Collections.Generic.List[string]
    $inHeader = $true
    foreach ($line in $unshippedLines) {
        if ($inHeader -and $line -match '^;') {
            $header.Add($line)
            continue
        }

        $inHeader = $false
        $body.Add($line)
    }

    # Trim the blank lines that surround the content, so the section this produces is spaced
    # exactly like the ones already in the shipped file.
    while ($body.Count -gt 0 -and $body[0] -match '^\s*$') { $body.RemoveAt(0) }
    while ($body.Count -gt 0 -and $body[$body.Count - 1] -match '^\s*$') { $body.RemoveAt($body.Count - 1) }

    if ($body.Count -eq 0) {
        Write-Host '  - analyzer release tracking: no unshipped rules to move'
        return
    }

    $shippedLines = Get-FileLines $analyzerShipped
    if (@($shippedLines | Where-Object { $_ -eq "## Release $version" }).Count -gt 0) {
        Write-Host "  ! analyzer release tracking: '## Release $version' is already in Shipped.md, but"
        Write-Host '    Unshipped.md is not empty. Resolve this by hand; leaving both files alone.'
        $reminders.Add("$analyzerShippedName already has a '## Release $version' section while $analyzerUnshippedName still holds entries. Merge them manually.")
        return
    }

    # Normalize away any trailing blank lines before appending, so the new section is always
    # separated from the previous one by exactly one blank line.
    $shipped = New-Object System.Collections.Generic.List[string]
    $shipped.AddRange([string[]] $shippedLines)
    while ($shipped.Count -gt 0 -and $shipped[$shipped.Count - 1] -match '^\s*$') { $shipped.RemoveAt($shipped.Count - 1) }
    $shipped.Add('')
    $shipped.Add("## Release $version")
    $shipped.Add('')
    $shipped.AddRange($body)

    $movedRules = @($body | Where-Object { $_ -match '^BIDI\d+ ' }).Count
    Write-Host "  v analyzer release tracking: moved $movedRules rule entries into '## Release $version'"

    Set-PreparedContent $analyzerShipped (ConvertTo-FileText $shipped.ToArray()) | Out-Null
    Set-PreparedContent $analyzerUnshipped (ConvertTo-FileText $header.ToArray()) | Out-Null

    if (@($body | Where-Object { $_ -match '^### Removed Rules' }).Count -gt 0) {
        $reminders.Add('This release removes analyzer rules. The versioning section of docs/articles/advanced/api-design.md cites removals by version, and docs/articles/advanced/analyzers.md lists the rules; check both.')
    }
}

function Update-DocumentationVersionPin {
    $pattern = '(<PackageReference Include="WebDriverBiDi" Version=")([^"]*)(" */>)'
    $lines = Get-FileLines $gettingStarted

    $current = $null
    foreach ($line in $lines) {
        if ($line -match $pattern) {
            $current = $Matches[2]
            break
        }
    }

    if ($null -eq $current) {
        Write-Host "ERROR: no pinned <PackageReference Include=`"WebDriverBiDi`" ...> found in $gettingStartedName." -ForegroundColor Red
        Write-Host '       The page changed shape; update this script to match it.' -ForegroundColor Red
        exit 1
    }

    if ($qualifier) {
        Write-Host "  - documentation version pin: left at $current (prerelease versions are not recommended there)"
        return
    }

    if ($current -eq $version) {
        Write-Host "  - documentation version pin: already $version"
        return
    }

    $updated = $lines | ForEach-Object { $_ -replace $pattern, "`${1}$version`${3}" }

    # Report the pin only once it is written. The version is known to differ from $current by here, so
    # Set-PreparedContent reporting no change means the replace matched nothing -- the page's
    # PackageReference has changed shape -- which must fail rather than print a checkmark for an edit
    # that never happened.
    if (Set-PreparedContent $gettingStarted (ConvertTo-FileText $updated)) {
        Write-Host "  v documentation version pin: $current -> $version"
    }
    else {
        Write-Host "ERROR: the version pin in $gettingStartedName did not change." -ForegroundColor Red
        Write-Host "       Its PackageReference no longer matches this script's pattern; update the script." -ForegroundColor Red
        exit 1
    }
}

Update-AnalyzerReleaseTracking
Update-DocumentationVersionPin

Write-Host ''

# ---------------------------------------------------------------------------------------------
# Verification
# ---------------------------------------------------------------------------------------------
function Invoke-VerificationStep {
    param([string] $Description, [string[]] $CommandArguments)

    Write-Host "  -> $Description"
    $executable = $CommandArguments[0]
    # Splatting requires a plain variable: `& $exe @(...)` is the array subexpression operator and
    # would hand the command a single argument holding every element.
    $commandTail = @()
    if ($CommandArguments.Count -gt 1) {
        $commandTail = @($CommandArguments[1..($CommandArguments.Count - 1)])
    }

    & $executable @commandTail
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: $Description failed. The release prep changes are still in the working tree;" -ForegroundColor Red
        Write-Host '       fix the failure and re-run before tagging.' -ForegroundColor Red
        exit 1
    }
}

if ($SkipVerification) {
    Write-Host 'Verification skipped (-SkipVerification)'
}
elseif ($DryRun) {
    Write-Host 'Verification skipped (-DryRun made no changes to verify)'
}
else {
    Write-Host 'Verification'

    Push-Location $repoRoot
    try {
        # -warnaserror matches _tests.yml: a new warning fails the release build, so surface it here
        # rather than after the tag is pushed.
        Invoke-VerificationStep 'restore' @('dotnet', 'restore')
        Invoke-VerificationStep 'build (Release, -warnaserror)' @('dotnet', 'build', '--configuration', 'Release', '--no-restore', '-warnaserror')
        Invoke-VerificationStep 'unit tests (library)' @('dotnet', 'test', '--project', 'test/WebDriverBiDi.Tests', '--configuration', 'Release', '--no-build')
        Invoke-VerificationStep 'unit tests (analyzers)' @('dotnet', 'test', '--project', 'test/WebDriverBiDi.Analyzers.Tests', '--configuration', 'Release', '--no-build')
        Invoke-VerificationStep 'unit tests (logging)' @('dotnet', 'test', '--project', 'test/WebDriverBiDi.Logging.Tests', '--configuration', 'Release', '--no-build')

        # The region validation exists only as a bash script. Git for Windows puts bash on PATH, so
        # this usually runs; where it does not, say so rather than reporting a pass that never
        # happened. CI runs it again on the tag push either way.
        if (Get-Command 'bash' -ErrorAction SilentlyContinue) {
            Invoke-VerificationStep 'documentation region validation' @('bash', './docs/tools/validate-doc-regions.sh')
        }
        else {
            Write-Host '  ! documentation region validation skipped: bash is not on PATH.'
            $reminders.Add('The documentation region validation did not run locally (no bash on PATH). release.yml runs it on the tag push, where a failure blocks the docs publish and the NuGet push.')
        }
    }
    finally {
        Pop-Location
    }

    Write-Host '  v all verification steps passed'
}

Write-Host ''

# ---------------------------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------------------------
if ($reminders.Count -gt 0) {
    Write-Host 'Before tagging'
    foreach ($reminder in $reminders) {
        Write-Host "  ! $reminder"
    }
    Write-Host ''
}

if ($DryRun) {
    Write-Host 'Dry run complete; nothing was written. Re-run without -DryRun to apply.'
    exit 0
}

$status = @(& git -C $repoRoot status --porcelain | Where-Object { $_ })
if ($status.Count -eq 0) {
    Write-Host "Nothing to change: the tree is already prepared for $tag."
}
else {
    Write-Host "The working tree is prepared for $tag. Changed files:"
    $status | ForEach-Object { Write-Host "     $_" }
}

Write-Host ''
Write-Host 'Next steps (this script deliberately does none of them):'
Write-Host '  1. Review the changes:      git diff'
Write-Host "  2. Commit them:             git commit -am ""chore: prep $tag release"""
Write-Host '  3. Get the commit onto main (directly or through a pull request).'
Write-Host "  4. Tag that commit:         git tag $tag"
Write-Host "  5. Push the tag:            git push origin $tag"
