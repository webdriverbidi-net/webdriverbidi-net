#!/usr/bin/env pwsh
# Parses one or more LCOV .info files and fails if aggregate line, branch, or
# method coverage drops below the configured thresholds.
#
# Usage:
#   check-lcov-thresholds.ps1 [-MinLine N] [-MinBranch N] [-MinMethod N] <lcov-file>...
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
    [Parameter(ValueFromRemainingArguments)]
    [string[]] $Files = @()
)

$ErrorActionPreference = 'Stop'

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

function Test-Threshold {
    param([string]$Label, $Pct, [double]$Threshold)
    if ($null -eq $Pct) {
        Write-Host "i  ${Label}: not applicable (no instrumented items), skipping"
        return $true
    }
    if ($Pct -ge $Threshold) {
        Write-Host "v  ${Label}: ${Pct}% >= ${Threshold}%"
        return $true
    }
    Write-Host "x  ${Label}: ${Pct}% < ${Threshold}%"
    return $false
}

$pass = $true
if (-not (Test-Threshold 'Lines'    $linePct   $MinLine))   { $pass = $false }
if (-not (Test-Threshold 'Branches' $branchPct $MinBranch)) { $pass = $false }
if (-not (Test-Threshold 'Methods'  $methodPct $MinMethod)) { $pass = $false }

Write-Host ''
if ($pass) {
    Write-Host 'PASS: all coverage thresholds met.'
    exit 0
} else {
    Write-Host 'FAIL: one or more coverage thresholds not met.'
    exit 1
}
