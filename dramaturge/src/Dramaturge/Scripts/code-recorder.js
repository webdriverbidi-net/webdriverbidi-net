// <copyright file="code-recorder.js" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Records the user's actions for code generation, sending each through a channel as its JSON and the element acted on.
(() => {
  let recorder = null;
  const facts = ({ element, ...rest }) => rest;

  return {
    start(send, testIdAttribute) {
      recorder?.stop();
      const describer = new Acquiescence.ElementDescriber();
      recorder = new Acquiescence.ActionRecorder(({ element, ...action }) => {
        const { target, ancestors } = describer.describe(element, { testIdAttribute });
        send([JSON.stringify({ ...action, target: facts(target), ancestors: ancestors.map(facts) }), target.element]);
      });
      recorder.start(document);
    },

    stop() {
      recorder?.stop();
      recorder = null;
    },
  };
})()
