# Chromium BiDi Mapper
This directory contains the Chromium BiDi Wrapper, which allows a Chromium-based browser
like Google Chrome or Microsoft Edge to communicate using the WebDriver BiDi protocol
without requiring a browser driver executable, like chromedriver. It is presented as
a JavaScript file, which is loaded into a tab in the Chromium-based browser, which
facilitates the communication. It is expected that, at some point, the Chromium team
will fold this functionality into the browser itself, and this JavaScript file will
no longer be necessary.

To update it, you will need to have a clone of the [chromium-bidi](https://github.com/GoogleChromeLabs/chromium-bidi)
project. In the directory of your clone, you can update the `mapperTab.js` file by executing
following commands:

```shell
git pull
npm install
npm run build
```

Once that execution is complete, you will need to copy the `lib/iife/mapperTab.js` file
into this directory, and commit the results.