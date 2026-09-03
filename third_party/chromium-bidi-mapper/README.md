# Chromium BiDi Mapper
This directory contains the Chromium BiDi Wrapper, which allows a Chromium-based browser
like Google Chrome or Microsoft Edge to communicate using the WebDriver BiDi protocol
without requiring a browser driver executable, like chromedriver. It is presented as
a JavaScript file, which is loaded into a tab in the Chromium-based browser, which
facilitates the communication. It is expected that, at some point, the Chromium team
will fold this functionality into the browser itself, and this JavaScript file will
no longer be necessary.

## Updating the mapper tab source
To update the mapper tab source, from the root of this project, you can execute the
following command:

```shell
./scripts/update-chromium-bidi.sh
```

This will download the latest published version of the mapper tab tarball from NPM,
extract it, and copy the tab JavaScript file to the proper location in this repo.
You can then commit the changes.
