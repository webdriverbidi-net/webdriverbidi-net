# Chromium BiDi Mapper
This directory contains the Chromium BiDi Wrapper, which allows a Chromium-based browser
like Google Chrome or Microsoft Edge to communicate using the WebDriver BiDi protocol
without requiring a browser driver executable, like chromedriver. It is presented as
a JavaScript file, which is loaded into a tab in the Chromium-based browser, which
facilitates the communication. It is expected that, at some point, the Chromium team
will fold this functionality into the browser itself, and this JavaScript file will
no longer be necessary.

## Updating the mapper tab source
To update the mapper tab source, you will need to have a working, buildable clone
of the [chromium-bidi](https://github.com/GoogleChromeLabs/chromium-bidi) project.
Getting a working clone of this repo is beyond the scope of this document.
Once you have a working, bulidable clone, in the directory of that clone, you can
update the `mapperTab.js` file by executing following commands:

```shell
git pull
npm install
npm run build
```

Once that execution is complete, you will need to copy the `out/Default/lib/iife/mapperTab.js`
file into this directory, and commit the results.

## Getting a working copy of the source repo
You will need to clone both the `chromium-bidi` repo and the `depot_tools` repo.
See above for the former; for the latter, you can follow the instructions at
[this link](https://commondatastorage.googleapis.com/chrome-infra-docs/flat/depot_tools/docs/html/depot_tools_tutorial.html#_setting_up).
There are a couple of things that need to be noted with getting this repo to build:
* The initial execution of `npm install` will fail, as it attempts to call
`npm run build`, which will fail without the prerequisites of the `depot_tools` repo.
* Note carefully that the part about the `depot_tools` repo needing to be on the
`PATH` environment variable. This is not optional to build the project in the repo.
* You **must** have a symlink to `python` as an executable. Many OSes only supply
`python3` without creating a symlink to `python`.
