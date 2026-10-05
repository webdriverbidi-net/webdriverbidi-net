// <copyright file="dom-snapshot.js" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// DOM snapshots for traces from Acquiescence, in a shape the protocol returns in one call.
(() => {
  let generator;

  return {
    // The document's snapshot as JSON, each frame's src a placeholder naming its index in frames, the frames'
    // windows, which the protocol returns as their browsing contexts, and, in the top frame, the point at an offset
    // from the target's center.
    snapshot(target, offset) {
      generator ??= new Acquiescence.DomSnapshotGenerator();
      const frames = [];
      const snapshot = generator.generate(document, {
        target: target ?? undefined,
        frameSource: (frame) => `/snapshot/@${frames.push(frame.contentWindow) - 1}`,
      });
      let point = null;
      if (target && offset && window === window.top) {
        const box = target.getBoundingClientRect();
        point = { x: box.left + box.width / 2 + offset.x, y: box.top + box.height / 2 + offset.y };
      }

      return { json: JSON.stringify(snapshot), frames, point };
    },
  };
})()
