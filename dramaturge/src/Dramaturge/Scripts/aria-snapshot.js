// <copyright file="aria-snapshot.js" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Accessibility snapshots and their templates from Acquiescence, in a shape the protocol returns in one call.
(() => {
  const generator = new Acquiescence.AriaSnapshotGenerator();
  const matcher = new Acquiescence.AriaSnapshotMatcher();
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

    // The error in a template, or null if it is valid. Matching parses the template before it takes a snapshot, so
    // matching an empty element checks the template alone.
    validate(template) {
      try {
        matcher.match(document.createElement('div'), template);
        return null;
      } catch (error) {
        return error.message;
      }
    },

    // Whether the snapshot of the root matches the template, and the snapshot without refs.
    match(root, template) {
      return matcher.match(root, template);
    },
  };
})()
