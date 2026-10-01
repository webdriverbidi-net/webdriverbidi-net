# Chromium BiDi Mapper
This directory contains the Chromium BiDi Wrapper, which allows a Chromium-based browser
like Google Chrome or Microsoft Edge to communicate using the WebDriver BiDi protocol
without requiring a browser driver executable, like chromedriver. It is presented as
a JavaScript file, which is loaded into a tab in the Chromium-based browser, which
facilitates the communication. It is expected that, at some point, the Chromium team
will fold this functionality into the browser itself, and this JavaScript file will
no longer be necessary.

## Updating the mapper tab source
To update the mapper tab source, run one of the following from anywhere in the
repository. On Linux and macOS:

```shell
./scripts/update-chromium-bidi.sh
```

On Windows:

```powershell
./scripts/update-chromium-bidi.ps1
```

Either script downloads the latest published version of the mapper tab tarball from
NPM, extracts it into a temporary directory, and copies the tab JavaScript file to the
proper location in this repo. You can then commit the changes.

Both report the same exit codes: `0` on success, `1` if the download, extraction or
copy failed, and `2` if a required tool is missing. Pass `--help` (or `-Help` for the
PowerShell script) for usage. The shell script needs `curl`, `jq` and `tar`; the
PowerShell script needs only `tar`, which ships with Windows 10 1803 and later, because
PowerShell handles the download and JSON parsing itself.
