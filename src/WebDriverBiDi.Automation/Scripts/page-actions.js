// <copyright file="page-actions.js" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Changes to the page that the protocol has no command for. Acquiescence, installed beside this, only queries.
({
  /**
   * Focuses an element and selects its text, so that typing replaces it: the value of an <input> or <textarea>,
   * or the contents of any other element, such as a [contenteditable] one.
   * @param {Element} element The element.
   * @returns {boolean} True if the text was selected; false if the element is not connected.
   */
  selectText(element) {
    if (!element.isConnected) {
      return false;
    }
    if (element.localName === 'input' || element.localName === 'textarea') {
      element.select();
      element.focus();
      return true;
    }
    const range = element.ownerDocument.createRange();
    range.selectNodeContents(element);
    const selection = element.ownerDocument.getSelection();
    if (selection) {
      selection.removeAllRanges();
      selection.addRange(range);
    }
    element.focus();
    return true;
  },

  /**
   * Selects the options of a <select> that match, deselecting the others, then fires the input and change events a
   * user's choice would.
   * @param {Element} element The element.
   * @param {{value?: string, label?: string, index?: number}[]} options Each option, matched by one property.
   * @returns {{status: 'selected', values: string[]} | {status: 'notconnected' | 'notselect' | 'notmultiple'} |
   * {status: 'missing' | 'disabled', index: number}} The values now selected; or why the element cannot take the
   * options; or, by its index, a requested option that does not exist or is disabled.
   */
  selectOptions(element, options) {
    if (!element.isConnected) {
      return { status: 'notconnected' };
    }
    if (element.localName !== 'select') {
      return { status: 'notselect' };
    }
    if (options.length > 1 && !element.multiple) {
      return { status: 'notmultiple' };
    }
    const candidates = [...element.options];
    const chosen = [];
    for (let i = 0; i < options.length; i++) {
      const option = options[i];
      const match = candidates.find((candidate) => option.value !== undefined
        ? candidate.value === option.value
        : option.label !== undefined ? candidate.label === option.label : candidate.index === option.index);
      if (!match) {
        return { status: 'missing', index: i };
      }
      if (match.disabled) {
        return { status: 'disabled', index: i };
      }
      chosen.push(match);
    }
    for (const candidate of candidates) {
      candidate.selected = chosen.includes(candidate);
    }
    element.dispatchEvent(new Event('input', { bubbles: true, composed: true }));
    element.dispatchEvent(new Event('change', { bubbles: true }));
    return { status: 'selected', values: candidates.filter((candidate) => candidate.selected).map((candidate) => candidate.value) };
  },
})
