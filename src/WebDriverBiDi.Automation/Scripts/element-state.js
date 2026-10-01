// <copyright file="element-state.js" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Reads of element state beyond what Acquiescence answers, shared by locator reads and expectations.
(() => {
  const htmlNamespace = 'http://www.w3.org/1999/xhtml';
  const valueElements = ['input', 'textarea', 'select'];

  return {
    // The text of each element; with innerText, null for an element that is not HTML.
    readTexts(elements, useInnerText) {
      return elements.map((element) => {
        if (!useInnerText) {
          return element.textContent;
        }

        return element.namespaceURI === htmlNamespace ? element.innerText : null;
      });
    },

    // The value of an input, a text area, or a select; null for any other element.
    readValue(element) {
      return valueElements.includes(element.localName) ? element.value : null;
    },

    readAttribute(element, name) {
      return element.getAttribute(name);
    },

    readCss(element, property) {
      return getComputedStyle(element).getPropertyValue(property);
    },

    // An input or a text area without a value, or another element with no child elements and only white space.
    isEmpty(element) {
      if (element.localName === 'input' || element.localName === 'textarea') {
        return !element.value;
      }

      return element.children.length === 0 && !element.textContent.trim();
    },

    // Whether the element has focus within its document or shadow root.
    isFocused(element) {
      return element.getRootNode().activeElement === element;
    },
  };
})()
