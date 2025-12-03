# Acqhiescence
This directory contains the JavaScript code for the Acquiescence element state library
which allows an automation library to query for an element's state, find available
click points for interaction, wait for an element to be ready for interaction, and so
on.

To update it, you will need to have a clone of the [Acquiescence](https://github.com/jimevans/acquiescence)
project. In the directory of your clone, you can update the `acquiescence.browser.js` file by executing
following commands:

```shell
git pull
npm install
npm run build:browser
```

Once that execution is complete, you will need to copy the `dist/acquiescence.browser.js` file
into this directory, and commit the results.
