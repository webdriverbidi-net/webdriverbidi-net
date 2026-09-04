#!/usr/bin/env pwsh
# Parses one or more LCOV .info files and fails if aggregate line, branch, or
# method coverage drops below the configured thresholds.
#
# Usage:
#   check-lcov-thresholds.ps1 [-MinLine N] [-MinBranch N] [-MinMethod N] [-Help] <lcov-file>...
#
# Defaults:
#   -MinLine   100   (line coverage percentage; integer or decimal)
#   -MinBranch 100
#   -MinMethod 100
#
# Exit codes:
#   0 — all thresholds met
#   1 — at least one threshold not met
#   2 — configuration or input error

[CmdletBinding()]
param(
    [double] $MinLine   = 100,
    [double] $MinBranch = 100,
    [double] $MinMethod = 100,
    [switch] $Help,
    [Parameter(ValueFromRemainingArguments)]
    [string[]] $Files = @()
)

$ErrorActionPreference = 'Stop'

# -Help prints the header block above, matching the -h/--help behaviour of
# check-lcov-thresholds.sh.
if ($Help) {
    Get-Content -LiteralPath $PSCommandPath |
        Select-Object -Skip 1 -First 15 |
        ForEach-Object { $_ -replace '^# ?', '' } |
        Write-Host
    exit 0
}

# Anything that still looks like a flag was not bound by param(), so it is an unknown flag. The
# shell script exits 2 for this rather than treating it as a file name, and silently reading it as
# a path would turn a typo into a "file not found" or, worse, into a silent no-op.
$unknownFlags = @($Files | Where-Object { $_ -like '-*' })
if ($unknownFlags.Count -gt 0) {
    Write-Host "ERROR: Unknown flag: $($unknownFlags[0])" -ForegroundColor Red
    exit 2
}

# CI passes a glob (lcov-library.*.info) which the shell expands before the script sees it.
# PowerShell does not expand arguments, so expand here; a pattern that matches nothing is left
# alone so the not-found check below reports it by name.
$expanded = @()
foreach ($pattern in $Files) {
    $matched = @(Get-ChildItem -Path $pattern -File -ErrorAction SilentlyContinue)
    if ($matched.Count -gt 0) {
        $expanded += $matched.FullName
    }
    else {
        $expanded += $pattern
    }
}

$Files = $expanded

if ($Files.Count -eq 0) {
    Write-Host 'ERROR: No LCOV input files supplied.' -ForegroundColor Red
    Write-Host 'Usage: check-lcov-thresholds.ps1 [-MinLine N] [-MinBranch N] [-MinMethod N] <lcov-file>...' -ForegroundColor Red
    exit 2
}

foreach ($file in $Files) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        Write-Host "ERROR: LCOV file not found: $file" -ForegroundColor Red
        exit 2
    }
}

# Aggregate LF/LH/BRF/BRH/FNF/FNH across all input files in a single pass.
#
# In a `switch -Regex` loop over an array, `continue` advances to the next
# array element, skipping any remaining cases for the current line — equivalent
# to the awk single-pass aggregation in check-lcov-thresholds.sh.
[long] $lf = 0; [long] $lh = 0
[long] $brf = 0; [long] $brh = 0
[long] $fnf = 0; [long] $fnh = 0

foreach ($file in $Files) {
    switch -Regex (Get-Content -LiteralPath $file) {
        '^LF:(\d+)'  { $lf  += [long]$Matches[1]; continue }
        '^LH:(\d+)'  { $lh  += [long]$Matches[1]; continue }
        '^BRF:(\d+)' { $brf += [long]$Matches[1]; continue }
        '^BRH:(\d+)' { $brh += [long]$Matches[1]; continue }
        '^FNF:(\d+)' { $fnf += [long]$Matches[1]; continue }
        '^FNH:(\d+)' { $fnh += [long]$Matches[1]; continue }
    }
}

# Returns $null when denominator is zero (no instrumented items); the threshold
# check treats $null as "not applicable" and skips gating, matching the bash
# script's "-" sentinel behaviour.
function Get-Percent {
    param([long]$Hit, [long]$Found)
    if ($Found -eq 0) { return $null }
    return [Math]::Round(($Hit * 100.0) / $Found, 2)
}

function Format-Pct {
    param($Pct)
    if ($null -eq $Pct) { 'n/a' } else { "${Pct}%" }
}

$linePct   = Get-Percent $lh  $lf
$branchPct = Get-Percent $brh $brf
$methodPct = Get-Percent $fnh $fnf

Write-Host '=== Coverage Summary ==='
Write-Host ("Lines:    {0} / {1}  ({2})  — threshold {3}%" -f $lh,  $lf,  (Format-Pct $linePct),   $MinLine)
Write-Host ("Branches: {0} / {1}  ({2})  — threshold {3}%" -f $brh, $brf, (Format-Pct $branchPct), $MinBranch)
Write-Host ("Methods:  {0} / {1}  ({2})  — threshold {3}%" -f $fnh, $fnf, (Format-Pct $methodPct), $MinMethod)
Write-Host ''

# A report with no instrumented lines is not fully covered; it is a broken input. An include filter
# that matched nothing, a project that failed to produce a report, or a truncated file all land here,
# and every one of them used to print PASS and exit 0 — so the gate could be satisfied by measuring
# nothing at all. This is an input error, not a coverage failure, and gets its own exit code so a
# caller can tell the two apart.
if ($lf -eq 0) {
    Write-Host 'x  ERROR: the coverage report contains no instrumented lines.'
    Write-Host '   An include/exclude filter that matched nothing, or an empty or truncated report,'
    Write-Host '   produces this. Nothing was measured, so no threshold can be met.'
    exit 2
}

# Compares exact coverage (Hit/Found) to a threshold using the unrounded ratio,
# not the 2-decimal display percentage, so a value like 94.995% does not pass a
# 95% threshold. Found==0 is "not applicable" and is skipped.
function Test-Threshold {
    param([string]$Label, [long]$Hit, [long]$Found, [double]$Threshold)
    if ($Found -eq 0) {
        Write-Host "i  ${Label}: not applicable (no instrumented items), skipping"
        return $true
    }
    $pct = Format-Pct (Get-Percent $Hit $Found)
    if ((($Hit * 100.0) / $Found) -ge $Threshold) {
        Write-Host "v  ${Label}: ${pct} >= ${Threshold}%"
        return $true
    }
    Write-Host "x  ${Label}: ${pct} < ${Threshold}%"
    return $false
}

$pass = $true
if (-not (Test-Threshold 'Lines'    $lh  $lf  $MinLine))   { $pass = $false }
if (-not (Test-Threshold 'Branches' $brh $brf $MinBranch)) { $pass = $false }
if (-not (Test-Threshold 'Methods'  $fnh $fnf $MinMethod)) { $pass = $false }

Write-Host ''
if ($pass) {
    Write-Host 'PASS: all coverage thresholds met.'
    exit 0
} else {
    Write-Host 'FAIL: one or more coverage thresholds not met.'
    exit 1
}
