// <copyright file="aria-snapshot.js" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Accessibility snapshots from Acquiescence, in a shape the protocol returns in one call.
(() => {
  const generator = new Acquiescence.AriaSnapshotGenerator();
  const frameTags = ['IFRAME', 'FRAME'];

  return {
    // The snapshot of the root, or of the document's body, with its tree as JSON. Its frames' windows come from a
    // snapshot with refs, in document order, so the caller can find each frame even when the text has no refs.
    snapshot(root, includeRefs, refPrefix) {
      const target = root ?? document.body ?? document.documentElement;
      const withRefs = generator.generate(target, { refPrefix });
      const snapshot = includeRefs ? withRefs : generator.generate(target, { refs: false });
      return {
        tree: JSON.stringify(snapshot.root),
        text: snapshot.text,
        refs: snapshot.references.map((reference) => reference.ref),
        elements: snapshot.references.map((reference) => reference.element),
        frames: withRefs.references
          .filter((reference) => frameTags.includes(reference.element.tagName))
          .map((reference) => reference.element.contentWindow),
      };
    },
  };
})()
