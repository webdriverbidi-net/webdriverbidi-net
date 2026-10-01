#!/usr/bin/env pwsh
# Downloads the latest published chromium-bidi package from the npm registry and refreshes the
# vendored mapper tab source and its license in third_party/chromium-bidi-mapper.
#
# Usage:
#   update-chromium-bidi.ps1 [-Help]
#
# Exit codes:
#   0 — the vendored mapperTab.js and LICENSE were updated
#   1 — the download, extraction, or copy failed
#   2 — a required tool is missing

[CmdletBinding()]
param(
    [switch] $Help
)

$ErrorActionPreference = 'Stop'

# -Help prints the header block above, matching the -h/--help behaviour of update-chromium-bidi.sh.
if ($Help) {
    Get-Content -LiteralPath $PSCommandPath |
        Select-Object -Skip 1 -First 10 |
        ForEach-Object { $_ -replace '^# ?', '' } |
        Write-Host
    exit 0
}

# tar is the one external tool this needs: it ships with Windows 10 1803 and later, and with every
# supported Windows Server. The registry lookup and the download use PowerShell's own web cmdlets,
# so no curl is required, and Invoke-RestMethod parses the JSON response directly, so no jq is
# either. Checking up front reports a missing tar by name rather than as a confusing later failure.
if (-not (Get-Command 'tar' -ErrorAction SilentlyContinue)) {
    Write-Host 'ERROR: required tool not found on PATH: tar' -ForegroundColor Red
    Write-Host '       tar ships with Windows 10 1803 and later. On an older system, install it or' -ForegroundColor Red
    Write-Host '       run scripts/update-chromium-bidi.sh under Git Bash or WSL instead.' -ForegroundColor Red
    exit 2
}

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$destinationDir = Join-Path $repoRoot 'third_party/chromium-bidi-mapper'
$destination = Join-Path $destinationDir 'mapperTab.js'
$licenseDestination = Join-Path $destinationDir 'LICENSE'

# Extract into a temporary directory that is removed however this script exits, rather than into a
# fixed .chromium-bidi directory in the working directory that a failure would leave behind.
$workDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
New-Item -ItemType Directory -Path $workDir | Out-Null

try {
    Write-Host 'Resolving the latest chromium-bidi release...'
    $release = Invoke-RestMethod -Uri 'https://registry.npmjs.org/chromium-bidi/latest'
    $tarballUrl = $release.dist.tarball
    if ([string]::IsNullOrWhiteSpace($tarballUrl)) {
        Write-Host 'ERROR: the registry response did not contain a tarball URL.' -ForegroundColor Red
        exit 1
    }

    Write-Host "Downloading $tarballUrl"
    $tarballPath = Join-Path $workDir 'chromium-bidi.tgz'

    # Invoke-WebRequest writes the body to a file rather than to the pipeline, so the bytes are not
    # reinterpreted as text the way piping a tarball through PowerShell would.
    Invoke-WebRequest -Uri $tarballUrl -OutFile $tarballPath

    tar -xzf $tarballPath -C $workDir
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: tar failed to extract the package (exit code $LASTEXITCODE)." -ForegroundColor Red
        exit 1
    }

    $sourceFile = Join-Path $workDir 'package/out/Default/gen/src/mapperTab.js'
    if (-not (Test-Path -LiteralPath $sourceFile -PathType Leaf)) {
        Write-Host 'ERROR: mapperTab.js was not found in the package at the expected path:' -ForegroundColor Red
        Write-Host '       package/out/Default/gen/src/mapperTab.js' -ForegroundColor Red
        Write-Host '       The package layout may have changed; this script needs updating.' -ForegroundColor Red
        exit 1
    }

    # The mapper is redistributed inside this project's assembly, so its license must travel with it.
    # Both files are checked before either is copied, so a changed package layout never leaves the
    # vendored script updated without the license that covers it.
    $licenseFile = Join-Path $workDir 'package/LICENSE'
    if (-not (Test-Path -LiteralPath $licenseFile -PathType Leaf)) {
        Write-Host 'ERROR: LICENSE was not found in the package at the expected path:' -ForegroundColor Red
        Write-Host '       package/LICENSE' -ForegroundColor Red
        Write-Host '       The package layout may have changed; this script needs updating.' -ForegroundColor Red
        exit 1
    }

    Copy-Item -LiteralPath $sourceFile -Destination $destination -Force
    Write-Host "Updated $destination"
    Copy-Item -LiteralPath $licenseFile -Destination $licenseDestination -Force
    Write-Host "Updated $licenseDestination"
}
catch {
    # $ErrorActionPreference = 'Stop' turns a failed web request or copy into a terminating error;
    # report it and exit 1 rather than letting PowerShell's default non-zero-but-unspecified status
    # stand, so a caller sees the same code the shell script would give.
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    Remove-Item -LiteralPath $workDir -Recurse -Force -ErrorAction SilentlyContinue
}
