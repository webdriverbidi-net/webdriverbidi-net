"use strict";
var Acquiescence = (() => {
  var __defProp = Object.defineProperty;
  var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
  var __getOwnPropNames = Object.getOwnPropertyNames;
  var __hasOwnProp = Object.prototype.hasOwnProperty;
  var __export = (target, all) => {
    for (var name in all)
      __defProp(target, name, { get: all[name], enumerable: true });
  };
  var __copyProps = (to, from, except, desc) => {
    if (from && typeof from === "object" || typeof from === "function") {
      for (let key of __getOwnPropNames(from))
        if (!__hasOwnProp.call(to, key) && key !== except)
          __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
    }
    return to;
  };
  var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

  // src/index.ts
  var index_exports = {};
  __export(index_exports, {
    ElementStateInspector: () => elementStateInspector_default,
    RequestAnimationFrameWaiter: () => RequestAnimationFrameWaiter,
    TimeoutWaiter: () => TimeoutWaiter
  });

  // src/domUtilities.ts
  var DOMUtilities = class {
    /**
     * Gets a value indicating whether an element is focusable.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element is focusable; otherwise, false.
     */
    isFocusable(element) {
      return !this.isNativelyDisabled(element) && (this.isNativelyFocusable(element) || this.hasTabIndex(element));
    }
    /**
     * Gets a value indicating whether an element is natively disabled.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element is natively disabled; otherwise, false.
     */
    isNativelyDisabled(element) {
      const isNativeFormControl = ["BUTTON", "INPUT", "SELECT", "TEXTAREA", "OPTION", "OPTGROUP"].includes(this.getNormalizedElementTagName(element));
      return isNativeFormControl && (element.hasAttribute("disabled") || this.isInDisabledOptGroup(element) || this.isInDisabledFieldSet(element));
    }
    /**
     * Gets the normalized (uppercase) element tag name.
     * @param element {Element} The element to check.
     * @returns {string} The normalized element tag name.
     */
    getNormalizedElementTagName(element) {
      const tagName = element.tagName;
      if (typeof tagName === "string") {
        return tagName.toUpperCase();
      }
      if (element instanceof HTMLFormElement) {
        return "FORM";
      }
      return element.tagName.toUpperCase();
    }
    /**
     * Gets the parent element or shadow host of an element.
     * @param element {Element} The element to check.
     * @returns {Element | undefined} The parent element or shadow host of the element, or undefined if the element has no parent.
     */
    getParentElementOrShadowHost(element) {
      if (element.parentElement) {
        return element.parentElement;
      }
      if (!element.parentNode) {
        return;
      }
      if (element.parentNode.nodeType === 11 && element.parentNode.host) {
        return element.parentNode.host;
      }
    }
    /**
     * Gets the enclosing shadow root or document of an element.
     * @param element {Element} The element to check.
     * @returns {Document | ShadowRoot | undefined} The enclosing shadow root or document of the element, or undefined if the element has no enclosing shadow root or document.
     */
    getEnclosingShadowRootOrDocument(element) {
      let node = element;
      while (node.parentNode) {
        node = node.parentNode;
      }
      if (node.nodeType === Node.DOCUMENT_FRAGMENT_NODE || node.nodeType === Node.DOCUMENT_NODE) {
        return node;
      }
    }
    /**
     * Gets the closest cross-shadow element.
     * @param element {Element | undefined} The element to check.
     * @param css {string} The CSS selector to use.
     * @param scope {Document | Element | undefined} The scope to use. If provided, the element must be inside scope's subtree.
     * @returns {Element | undefined} The closest cross-shadow element, or undefined if no closest element is found.
     */
    getClosestCrossShadowElement(element, css, scope) {
      while (element) {
        const closest = element.closest(css);
        if (scope && closest !== scope && closest?.contains(scope)) {
          return;
        }
        if (closest) {
          return closest;
        }
        element = this.getEnclosingShadowHost(element);
      }
    }
    /**
     * Gets the enclosing shadow host of an element.
     * @param element {Element} The element to check.
     * @returns {Element | undefined} The enclosing shadow host of the element, or undefined if the element has no enclosing shadow host.
     */
    getEnclosingShadowHost(element) {
      while (element.parentElement) {
        element = element.parentElement;
      }
      return this.getParentElementOrShadowHost(element);
    }
    /**
     * Gets a value indicating whether an element has a tab index.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element has a tab index; otherwise, false.
     */
    hasTabIndex(element) {
      return !Number.isNaN(Number(String(element.getAttribute("tabindex"))));
    }
    /**
     * Gets a value indicating whether an element is in a disabled opt group.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element is in a disabled opt group; otherwise, false.
     */
    isInDisabledOptGroup(element) {
      return this.getNormalizedElementTagName(element) === "OPTION" && !!element.closest("OPTGROUP[DISABLED]");
    }
    /**
     * Gets a value indicating whether an element is in a disabled field set.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element is in a disabled field set; otherwise, false.
     */
    isInDisabledFieldSet(element) {
      const fieldSetElement = element?.closest("FIELDSET[DISABLED]");
      if (!fieldSetElement) {
        return false;
      }
      const legendElement = fieldSetElement.querySelector(":scope > LEGEND");
      return !legendElement?.contains(element);
    }
    /**
     * Gets a value indicating whether an element is natively focusable.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element is natively focusable; otherwise, false.
     */
    isNativelyFocusable(element) {
      const tagName = this.getNormalizedElementTagName(element);
      if (["BUTTON", "DETAILS", "SELECT", "TEXTAREA"].includes(tagName)) {
        return true;
      }
      if (tagName === "A" || tagName === "AREA") {
        return element.hasAttribute("href");
      }
      if (tagName === "INPUT") {
        return !element.hidden;
      }
      return false;
    }
  };
  var domUtilities_default = DOMUtilities;

  // src/ariaUtilities.ts
  var AriaUtilities = class {
    domUtilities = new domUtilities_default();
    ariaDisabledRoles = [
      "application",
      "button",
      "composite",
      "gridcell",
      "group",
      "input",
      "link",
      "menuitem",
      "scrollbar",
      "separator",
      "tab",
      "checkbox",
      "columnheader",
      "combobox",
      "grid",
      "listbox",
      "menu",
      "menubar",
      "menuitemcheckbox",
      "menuitemradio",
      "option",
      "radio",
      "radiogroup",
      "row",
      "rowheader",
      "searchbox",
      "select",
      "slider",
      "spinbutton",
      "switch",
      "tablist",
      "textbox",
      "toolbar",
      "tree",
      "treegrid",
      "treeitem"
    ];
    validRoles = [
      "alert",
      "alertdialog",
      "application",
      "article",
      "banner",
      "blockquote",
      "button",
      "caption",
      "cell",
      "checkbox",
      "code",
      "columnheader",
      "combobox",
      "complementary",
      "contentinfo",
      "definition",
      "deletion",
      "dialog",
      "directory",
      "document",
      "emphasis",
      "feed",
      "figure",
      "form",
      "generic",
      "grid",
      "gridcell",
      "group",
      "heading",
      "img",
      "insertion",
      "link",
      "list",
      "listbox",
      "listitem",
      "log",
      "main",
      "mark",
      "marquee",
      "math",
      "meter",
      "menu",
      "menubar",
      "menuitem",
      "menuitemcheckbox",
      "menuitemradio",
      "navigation",
      "none",
      "note",
      "option",
      "paragraph",
      "presentation",
      "progressbar",
      "radio",
      "radiogroup",
      "region",
      "row",
      "rowgroup",
      "rowheader",
      "scrollbar",
      "search",
      "searchbox",
      "separator",
      "slider",
      "spinbutton",
      "status",
      "strong",
      "subscript",
      "superscript",
      "switch",
      "tab",
      "table",
      "tablist",
      "tabpanel",
      "term",
      "textbox",
      "time",
      "timer",
      "toolbar",
      "tooltip",
      "tree",
      "treegrid",
      "treeitem"
    ];
    presentationInheritanceParents = {
      "DD": ["DL", "DIV"],
      "DIV": ["DL"],
      "DT": ["DL", "DIV"],
      "LI": ["OL", "UL"],
      "TBODY": ["TABLE"],
      "TD": ["TR"],
      "TFOOT": ["TABLE"],
      "TH": ["TR"],
      "THEAD": ["TABLE"],
      "TR": ["THEAD", "TBODY", "TFOOT", "TABLE"]
    };
    // https://www.w3.org/TR/wai-aria-practices/examples/landmarks/HTML5.html
    ancestorPreventingLandmark = "article:not([role]), aside:not([role]), main:not([role]), nav:not([role]), section:not([role]), [role=article], [role=complementary], [role=main], [role=navigation], [role=region]";
    inputTypeToRole = {
      "button": "button",
      "checkbox": "checkbox",
      "image": "button",
      "number": "spinbutton",
      "radio": "radio",
      "range": "slider",
      "reset": "button",
      "submit": "button"
    };
    // https://w3c.github.io/html-aam/#html-element-role-mappings
    // https://www.w3.org/TR/html-aria/#docconformance
    implicitRoleByTagName = {
      "A": (e) => {
        return e.hasAttribute("href") ? "link" : null;
      },
      "AREA": (e) => {
        return e.hasAttribute("href") ? "link" : null;
      },
      "ARTICLE": () => "article",
      "ASIDE": () => "complementary",
      "BLOCKQUOTE": () => "blockquote",
      "BUTTON": () => "button",
      "CAPTION": () => "caption",
      "CODE": () => "code",
      "DATALIST": () => "listbox",
      "DD": () => "definition",
      "DEL": () => "deletion",
      "DETAILS": () => "group",
      "DFN": () => "term",
      "DIALOG": () => "dialog",
      "DT": () => "term",
      "EM": () => "emphasis",
      "FIELDSET": () => "group",
      "FIGURE": () => "figure",
      "FOOTER": (e) => this.domUtilities.getClosestCrossShadowElement(e, this.ancestorPreventingLandmark) ? null : "contentinfo",
      "FORM": (e) => this.hasExplicitAccessibleName(e) ? "form" : null,
      "H1": () => "heading",
      "H2": () => "heading",
      "H3": () => "heading",
      "H4": () => "heading",
      "H5": () => "heading",
      "H6": () => "heading",
      "HEADER": (e) => this.domUtilities.getClosestCrossShadowElement(e, this.ancestorPreventingLandmark) ? null : "banner",
      "HR": () => "separator",
      "HTML": () => "document",
      "IMG": (e) => e.getAttribute("alt") === "" && !e.getAttribute("title") && !this.hasGlobalAriaAttribute(e) && !this.domUtilities.hasTabIndex(e) ? "presentation" : "img",
      "INPUT": (e) => {
        const type = e.type.toLowerCase();
        if (type === "search") {
          return e.hasAttribute("list") ? "combobox" : "searchbox";
        }
        if (["email", "tel", "text", "url", ""].includes(type)) {
          const list = this.getIdRefs(e, e.getAttribute("list"))[0];
          if (list) {
            const listTagName = this.domUtilities.getNormalizedElementTagName(list);
            if (listTagName === "DATALIST") {
              return "combobox";
            }
          }
          return "textbox";
        }
        if (type === "hidden")
          return null;
        if (type === "file")
          return "button";
        return this.inputTypeToRole[type] || "textbox";
      },
      "INS": () => "insertion",
      "LI": () => "listitem",
      "MAIN": () => "main",
      "MARK": () => "mark",
      "MATH": () => "math",
      "MENU": () => "list",
      "METER": () => "meter",
      "NAV": () => "navigation",
      "OL": () => "list",
      "OPTGROUP": () => "group",
      "OPTION": () => "option",
      "OUTPUT": () => "status",
      "P": () => "paragraph",
      "PROGRESS": () => "progressbar",
      "SEARCH": () => "search",
      "SECTION": (e) => this.hasExplicitAccessibleName(e) ? "region" : null,
      "SELECT": (e) => e.hasAttribute("multiple") || e.size > 1 ? "listbox" : "combobox",
      "STRONG": () => "strong",
      "SUB": () => "subscript",
      "SUP": () => "superscript",
      // For <svg> we default to Chrome behavior:
      // - Chrome reports 'img'.
      // - Firefox reports 'diagram' that is not in official ARIA spec yet.
      // - Safari reports 'no role', but still computes accessible name.
      "SVG": () => "img",
      "TABLE": () => "table",
      "TBODY": () => "rowgroup",
      "TD": (e) => {
        const table = this.domUtilities.getClosestCrossShadowElement(e, "table");
        const role = table ? this.getExplicitAriaRole(table) : "";
        return role === "grid" || role === "treegrid" ? "gridcell" : "cell";
      },
      "TEXTAREA": () => "textbox",
      "TFOOT": () => "rowgroup",
      "TH": (e) => {
        if (e.getAttribute("scope") === "col")
          return "columnheader";
        if (e.getAttribute("scope") === "row")
          return "rowheader";
        const table = this.domUtilities.getClosestCrossShadowElement(e, "table");
        const role = table ? this.getExplicitAriaRole(table) : "";
        return role === "grid" || role === "treegrid" ? "gridcell" : "cell";
      },
      "THEAD": () => "rowgroup",
      "TIME": () => "time",
      "TR": () => "row",
      "UL": () => "list"
    };
    // https://www.w3.org/TR/wai-aria-1.2/#global_states
    globalAriaAttributes = [
      ["aria-atomic", void 0],
      ["aria-busy", void 0],
      ["aria-controls", void 0],
      ["aria-current", void 0],
      ["aria-describedby", void 0],
      ["aria-details", void 0],
      // Global use deprecated in ARIA 1.2
      // ['aria-disabled', undefined],
      ["aria-dropeffect", void 0],
      // Global use deprecated in ARIA 1.2
      // ['aria-errormessage', undefined],
      ["aria-flowto", void 0],
      ["aria-grabbed", void 0],
      // Global use deprecated in ARIA 1.2
      // ['aria-haspopup', undefined],
      ["aria-hidden", void 0],
      // Global use deprecated in ARIA 1.2
      // ['aria-invalid', undefined],
      ["aria-keyshortcuts", void 0],
      ["aria-label", ["caption", "code", "deletion", "emphasis", "generic", "insertion", "paragraph", "presentation", "strong", "subscript", "superscript"]],
      ["aria-labelledby", ["caption", "code", "deletion", "emphasis", "generic", "insertion", "paragraph", "presentation", "strong", "subscript", "superscript"]],
      ["aria-live", void 0],
      ["aria-owns", void 0],
      ["aria-relevant", void 0],
      ["aria-roledescription", ["generic"]]
    ];
    ariaReadonlyRoles = [
      "checkbox",
      "combobox",
      "grid",
      "gridcell",
      "listbox",
      "radiogroup",
      "slider",
      "spinbutton",
      "textbox",
      "columnheader",
      "rowheader",
      "searchbox",
      "switch",
      "treegrid"
    ];
    /**
     * Gets a value indicating whether an element has an explicit ARIA disabled attribute.
     * @param element {Element | undefined} The element to check.
     * @param isAncestor {boolean} Whether to check the element's ancestors. If omitted, defaults to false.
     * @returns {boolean} True if the element has an explicit ARIA disabled attribute; otherwise, false.
     */
    hasExplicitAriaDisabled(element, isAncestor = false) {
      if (!element)
        return false;
      if (isAncestor || this.ariaDisabledRoles.includes(this.getAriaRole(element) ?? "")) {
        const attribute = (element.getAttribute("aria-disabled") ?? "").toLowerCase();
        if (attribute === "true") {
          return true;
        }
        if (attribute === "false") {
          return false;
        }
        return this.hasExplicitAriaDisabled(this.domUtilities.getParentElementOrShadowHost(element), true);
      }
      return false;
    }
    /**
     * Gets a value indicating whether an element has an ARIA read only role.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element has an ARIA read only role; otherwise, false.
     */
    isAriaReadOnlyRole(element) {
      return this.ariaReadonlyRoles.includes(this.getAriaRole(element) ?? "");
    }
    /**
     * Gets the ARIA role of an element, taking into account the element's explicit and implicit roles.
     * @param element {Element} The element to get the ARIA role of.
     * @returns {AriaRole | null} The ARIA role of the element, or null if the element has no ARIA role.
     */
    getAriaRole(element) {
      const explicitRole = this.getExplicitAriaRole(element);
      if (!explicitRole) {
        return this.getImplicitAriaRole(element);
      }
      if (explicitRole === "none" || explicitRole === "presentation") {
        const implicitRole = this.getImplicitAriaRole(element);
        if (this.hasPresentationConflictResolution(element, implicitRole)) {
          return implicitRole;
        }
      }
      return explicitRole;
    }
    /**
     * Gets the explicit ARIA role of an element.
     * @param element {Element} The element to get the explicit ARIA role of.
     * @returns {AriaRole | null} The explicit ARIA role of the element, or null if the element has no explicit ARIA role.
     */
    getExplicitAriaRole(element) {
      const roles = (element.getAttribute("role") ?? "").split(" ").map((role) => role.trim());
      return roles.find((role) => this.validRoles.includes(role)) || null;
    }
    /**
     * Gets the implicit ARIA role of an element.
     * @param element {Element} The element to get the implicit ARIA role of.
     * @returns {AriaRole | null} The implicit ARIA role of the element, or null if the element has no implicit ARIA role.
     */
    getImplicitAriaRole(element) {
      const implicitRole = this.implicitRoleByTagName[this.domUtilities.getNormalizedElementTagName(element)]?.(element) ?? "";
      if (!implicitRole) {
        return null;
      }
      let ancestor = element;
      while (ancestor) {
        const parent = this.domUtilities.getParentElementOrShadowHost(ancestor);
        const parents = this.presentationInheritanceParents[this.domUtilities.getNormalizedElementTagName(ancestor)];
        if (!parents || !parent || !parents.includes(this.domUtilities.getNormalizedElementTagName(parent))) {
          break;
        }
        const parentExplicitRole = this.getExplicitAriaRole(parent);
        if ((parentExplicitRole === "none" || parentExplicitRole === "presentation") && !this.hasPresentationConflictResolution(parent, parentExplicitRole)) {
          return parentExplicitRole;
        }
        ancestor = parent;
      }
      return implicitRole;
    }
    /**
     * Gets a value indicating whether an element has a global ARIA attribute.
     * @param element {Element} The element to check.
     * @param forRole {string | null} The role to check the global ARIA attributes for. If omitted, the global ARIA attributes are checked for all roles.
     * @returns {boolean} True if the element has a global ARIA attribute; otherwise, false.
     */
    hasGlobalAriaAttribute(element, forRole) {
      return this.globalAriaAttributes.some(([attr, prohibited]) => {
        return !prohibited?.includes(forRole ?? "") && element.hasAttribute(attr);
      });
    }
    /**
     * Gets a value indicating whether an element has an explicit accessible name.
     * @param element {Element} The element to check.
     * @returns {boolean} True if the element has an explicit accessible name; otherwise, false.
     */
    hasExplicitAccessibleName(e) {
      return e.hasAttribute("aria-label") || e.hasAttribute("aria-labelledby");
    }
    /**
     * Gets a value indicating whether an element has a presentation conflict resolution.
     * @param element {Element} The element to check.
     * @param role {string | null} The role to check the presentation conflict resolution for. If omitted, the presentation conflict resolution is checked for all roles.
     * @returns {boolean} True if the element has a presentation conflict resolution; otherwise, false.
     */
    hasPresentationConflictResolution(element, role) {
      return this.hasGlobalAriaAttribute(element, role) || this.domUtilities.isFocusable(element);
    }
    /**
     * Gets the elements referenced by an ID.
     * @param element {Element} The element to check.
     * @param ref {string | null} The ID to get the elements referenced by. If omitted, the elements referenced by the ID are returned.
     * @returns {Element[]} The elements referenced by the ID.
     */
    getIdRefs(element, ref) {
      if (!ref) {
        return [];
      }
      const root = this.domUtilities.getEnclosingShadowRootOrDocument(element);
      if (!root) {
        return [];
      }
      try {
        const ids = ref.split(" ").filter((id) => !!id);
        const result = [];
        for (const id of ids) {
          const firstElement = root.querySelector("#" + CSS.escape(id));
          if (firstElement && !result.includes(firstElement)) {
            result.push(firstElement);
          }
        }
        return result;
      } catch {
        return [];
      }
    }
  };
  var ariaUtilities_default = AriaUtilities;

  // src/nodePreviewer.ts
  var NodePreviewer = class {
    autoClosingTags = /* @__PURE__ */ new Set([
      "AREA",
      "BASE",
      "BR",
      "COL",
      "COMMAND",
      "EMBED",
      "HR",
      "IMG",
      "INPUT",
      "KEYGEN",
      "LINK",
      "MENUITEM",
      "META",
      "PARAM",
      "SOURCE",
      "TRACK",
      "WBR"
    ]);
    booleanAttributes = /* @__PURE__ */ new Set(["checked", "selected", "disabled", "readonly", "multiple"]);
    /**
     * Generates a string representation of a node.
     * @param node {Node} The node to preview.
     * @returns {string} A string representation of the node.
     */
    previewNode(node) {
      if (node.nodeType === Node.TEXT_NODE) {
        return this.oneLine(`#text=${node.nodeValue ?? ""}`);
      }
      if (node.nodeType !== Node.ELEMENT_NODE) {
        return this.oneLine(`<${node.nodeName.toLowerCase()} />`);
      }
      const element = node;
      const attrs = [];
      for (let i = 0; i < element.attributes.length; i++) {
        const attr = element.attributes.item(i);
        if (!attr) {
          continue;
        }
        const { name, value } = attr;
        if (name === "style") {
          continue;
        }
        if (!value && this.booleanAttributes.has(name)) {
          attrs.push(` ${name}`);
        } else {
          attrs.push(` ${name}="${value}"`);
        }
      }
      attrs.sort((a, b) => a.length - b.length);
      const attrText = this.trimStringWithEllipsis(attrs.join(""), 500);
      if (this.autoClosingTags.has(element.nodeName)) {
        return this.oneLine(`<${element.nodeName.toLowerCase()}${attrText}/>`);
      }
      const children = element.childNodes;
      let onlyText = false;
      if (children.length <= 5) {
        onlyText = true;
        for (let i = 0; i < children.length; i++) {
          const child = children.item(i);
          if (!child || child.nodeType !== Node.TEXT_NODE) {
            onlyText = false;
            break;
          }
        }
      }
      const text = onlyText ? element.textContent || "" : "\u2026";
      return this.oneLine(`<${element.nodeName.toLowerCase()}${attrText}>${this.trimStringWithEllipsis(text, 50)}</${element.nodeName.toLowerCase()}>`);
    }
    oneLine(s) {
      return s.replaceAll("\n", "\u21B5").replaceAll("	", "\u21C6");
    }
    trimStringWithEllipsis(input, cap) {
      return this.trimString(input, cap, "\u2026");
    }
    trimString(input, cap, suffix = "") {
      if (input.length <= cap) {
        return input;
      }
      const chars = [...input];
      if (chars.length > cap) {
        return chars.slice(0, cap - suffix.length).join("") + suffix;
      }
      return chars.join("");
    }
  };
  var nodePreviewer_default = NodePreviewer;

  // src/waiter.ts
  var TimeoutWaiter = class {
    condition;
    timeout;
    intervals;
    currentIntervalIndex = 0;
    intervalId = null;
    cancelled = false;
    /**
     * Initializes a new instance of the TimeoutWaiter class.
     * @param condition {() => T | Promise<T>} A Function testing the condition to poll for.
     * @param timeoutInMilliseconds {number} The timeout in milliseconds. If omitted, the timeout is zero, implying the check will execute once.
     * @param pollIntervalsInMilliseconds {number[]} An array of the intervals in milliseconds to poll at. If omitted, the default interval of 100ms is used.
     */
    constructor(condition, timeoutInMilliseconds = 0, pollIntervalsInMilliseconds = [100]) {
      this.condition = condition;
      this.timeout = timeoutInMilliseconds;
      this.intervals = pollIntervalsInMilliseconds;
    }
    /**
     * Waits for the condition to be met.
     * @returns {Promise<T>} A Promise that resolves to the result of the condition. The Promise is rejected if the timeout is reached, or if the wait is cancelled..
     */
    waitForCondition() {
      return new Promise((resolve, reject) => {
        this.cancelled = false;
        this.currentIntervalIndex = 0;
        const endTime = performance.now() + this.timeout;
        const checkCondition = async () => {
          if (this.cancelled) {
            reject(new Error("Wait cancelled"));
            return;
          }
          try {
            const result = await this.condition();
            if (result) {
              this.cleanup();
              resolve(result);
              return;
            }
          } catch {
          }
          if (performance.now() >= endTime) {
            this.cleanup();
            reject(new Error(`Timeout after ${this.timeout}ms`));
            return;
          }
          const currentInterval = this.intervals[Math.min(this.currentIntervalIndex++, this.intervals.length - 1)];
          this.intervalId = setTimeout(() => void checkCondition(), currentInterval);
        };
        void checkCondition();
      });
    }
    /**
     * Cancels the wait.
     */
    cancel() {
      this.cancelled = true;
      this.cleanup();
    }
    /**
     * Cleans up the waiter.
     */
    cleanup() {
      if (this.intervalId !== null) {
        clearTimeout(this.intervalId);
        this.intervalId = null;
      }
    }
  };
  var RequestAnimationFrameWaiter = class {
    condition;
    timeout;
    rafId = null;
    cancelled = false;
    /**
     * Initializes a new instance of the RequestAnimationFrameWaiter class.
     * @param condition {() => T | Promise<T>} A Function testing the condition to poll for.
     * @param timeoutInMilliseconds {number} The timeout in milliseconds. If omitted, the timeout is zero, implying the check will execute once.
     */
    constructor(condition, timeoutInMilliseconds = 0) {
      this.condition = condition;
      this.timeout = timeoutInMilliseconds;
    }
    /**
     * Waits for the condition to be met.
     * @returns {Promise<T>} A Promise that resolves to the result of the condition. The Promise is rejected if the timeout is reached, or if the wait is cancelled.
     */
    waitForCondition() {
      return new Promise((resolve, reject) => {
        this.cancelled = false;
        const endTime = performance.now() + this.timeout;
        const checkCondition = async () => {
          if (this.cancelled) {
            reject(new Error("Wait cancelled"));
            return;
          }
          try {
            const result = await this.condition();
            if (result) {
              this.cleanup();
              resolve(result);
              return;
            }
          } catch {
          }
          if (performance.now() >= endTime) {
            this.cleanup();
            reject(new Error(`Timeout after ${this.timeout}ms`));
            return;
          }
          this.rafId = globalThis.requestAnimationFrame(() => void checkCondition());
        };
        void checkCondition();
      });
    }
    /**
     * Cancels the wait.
     */
    cancel() {
      this.cancelled = true;
      this.cleanup();
    }
    /**
     * Cleans up the waiter.
     */
    cleanup() {
      if (this.rafId !== null) {
        globalThis.cancelAnimationFrame(this.rafId);
        this.rafId = null;
      }
    }
  };

  // src/elementStateInspector.ts
  var ElementStateInspector = class {
    ariaUtilities = new ariaUtilities_default();
    domUtilities = new domUtilities_default();
    nodePreviewer = new nodePreviewer_default();
    cacheStyle;
    cacheStyleBefore;
    cacheStyleAfter;
    /**
     * Queries a Node for a list of states.
     * @param node {Node} The node to query, which will be transformed into the nearest element to query the state.
     * @param states {ElementState[]} The states to query.
     * @returns {Promise<{ status: 'success' } | { status: 'failure', missingState: ElementState } | { status: 'error', message: string }>} 
     * A Promise that resolves to an object with the status of the query.
     * - 'success' if all states are present.
     * - 'failure' if at least one state is missing.
     * - 'error' if the node is not connected.
     * - 'missingState' is the state that is missing.
     * - 'message' is the message of the error.
     */
    async queryElementStates(node, states) {
      if (states.includes("stable")) {
        const stableResult = await this.checkElementIsStable(node);
        if (stableResult === false) {
          return { status: "failure", missingState: "stable" };
        }
        if (stableResult === "error:notconnected") {
          return { status: "error", message: "notconnected" };
        }
      }
      for (const state of states) {
        if (state !== "stable") {
          const result = await this.queryElementState(node, state);
          if (result.received === "error:notconnected") {
            return { status: "error", message: "notconnected" };
          }
          if (!result.matches) {
            return { status: "failure", missingState: result.received };
          }
        }
      }
      return { status: "success" };
    }
    /**
     * Queries a Node for a single state.
     * @param node {Node} The node to query, which will be transformed into the nearest element to query the state.
     * @param state {ElementStateWithoutStable} The state to query.
     * @returns {Promise<ElementStateQueryResult>} A Promise that resolves to an object with the status of the query.
     * - 'matches' is true if the state is present.
     * - 'received' is the state that was received, or 'error:notconnected' if the element is not connected.
     * @throws {Error} If an invalid state is provided.
     */
    async queryElementState(node, state) {
      const element = this.findElementFromNode(node, "none");
      if (!element?.isConnected) {
        return { matches: false, received: "error:notconnected" };
      }
      if (state === "visible" || state === "hidden") {
        const visible = this.isElementVisible(element);
        return {
          matches: state === "visible" ? visible : !visible,
          received: visible ? "visible" : "hidden"
        };
      }
      if (state === "disabled" || state === "enabled") {
        const disabled = this.isElementDisabled(element);
        return {
          matches: state === "disabled" ? disabled : !disabled,
          received: disabled ? "disabled" : "enabled"
        };
      }
      if (state === "editable") {
        const disabled = this.isElementDisabled(element);
        const readonly = this.isElementReadOnly(element);
        if (readonly === "error") {
          throw this.createError("Element is not an <input>, <textarea>, <select> or [contenteditable] and does not have a role allowing [aria-readonly]");
        }
        return {
          matches: !disabled && !readonly,
          received: disabled ? "disabled" : readonly ? "readOnly" : "editable"
        };
      }
      if (state === "inview") {
        const inView = await this.isElementInViewPort(element);
        const scrollable = inView ? true : this.isElementScrollable(element);
        return {
          matches: inView && scrollable,
          received: inView ? "inview" : scrollable ? "notinview" : "unviewable"
        };
      }
      throw this.createError(`Unexpected element state "${state}"`);
    }
    /**
     * Checks if an element is ready for an interaction.
     * @param element {Element} The element to check.
     * @param interactionType {ElementInteractionType} The type of interaction to check.
     * @param hitPointOffset {?{x: number, y: number}} The offset of the hit point from the center of the element.
     * @returns {Promise<{ status: ElementInteractionReadyResult, interactionPoint?: { x: number, y: number } }>}
     * A Promise that resolves to an object with the status of the check.
     * - 'status' is the status of the check.
     * - 'interactionPoint' is the hit point of the interaction, if the element is ready for the interaction.
     * - 'needsscroll' if the element is not in the view port, and cannot be scrolled into view due to overflow.
     * - 'notready' if the element is not ready for the interaction.
     * @throws {Error} If the element is
     * - not connected;
     * - not in the view port, and cannot be scrolled into view due to overflow
     * - is obscured by another element
     */
    async isInteractionReady(element, interactionType, hitPointOffset) {
      const states = ["stable", "visible", "inview"];
      if (interactionType === "click" || interactionType === "doubleclick" || interactionType === "hover" || interactionType === "drag") {
        states.push("enabled");
      }
      if (interactionType === "type" || interactionType === "clear") {
        states.push("enabled", "editable");
      }
      const result = await this.queryElementStates(element, states);
      if (result.status === "error") {
        throw new Error("element not connected");
      }
      if (result.status === "failure") {
        if (result.missingState === "unviewable") {
          throw new Error("element is not in view port, and cannot be scrolled into view due to overflow");
        }
        if (result.missingState === "notinview") {
          return { status: "needsscroll" };
        }
        return { status: "notready" };
      }
      const clickPoint = await this.getElementClickPoint(element, hitPointOffset);
      if (clickPoint.status === "error") {
        throw new Error(clickPoint.message);
      }
      return { status: "ready", interactionPoint: clickPoint.hitPoint };
    }
    /**
     * Waits for an element to be ready for an interaction.
     * @param element {Element} The element to wait for.
     * @param interactionType {ElementInteractionType} The type of interaction to wait for.
     * @param timeoutInMilliseconds {number} The timeout in milliseconds.
     * @param hitPointOffset {?{x: number, y: number}} The offset of the hit point from the center of the element.
     * @returns {Promise<{x: number, y: number}>} A Promise that resolves to the hit point of the interaction.
     * - 'x' is the x coordinate of the hit point.
     * - 'y' is the y coordinate of the hit point.
     * @throws {Error} If the element is not ready for the interaction before the timeout is reached.
     */
    async waitForInteractionReady(element, interactionType, timeoutInMilliseconds, hitPointOffset) {
      const pollIntervals = [0, 0, 20, 50, 100, 100, 500];
      const waiter = new TimeoutWaiter(
        async () => {
          const result = await this.isInteractionReady(element, interactionType, hitPointOffset);
          if (result.status === "needsscroll") {
            element.scrollIntoView({ behavior: "instant", block: "center", inline: "center" });
          }
          if (result.status === "ready") {
            return result.interactionPoint ?? { x: 0, y: 0 };
          }
          return null;
        },
        timeoutInMilliseconds,
        pollIntervals
      );
      try {
        const result = await waiter.waitForCondition();
        return result;
      } catch {
        throw new Error("timeout waiting for interaction to be ready");
      }
    }
    /**
     * Gets the bounding rectangle of an element in the view port.
     * @param element {Element}The element to get the bounding rectangle of.
     * @returns {Promise<{ x: number, y: number, width: number, height: number } | undefined>} 
     * A Promise that resolves to the bounding rectangle of the element in the view port, or undefined if the element
     * is not in the view port.
     * - 'x' is the x coordinate of the bounding rectangle.
     * - 'y' is the y coordinate of the bounding rectangle.
     * - 'width' is the width of the bounding rectangle.
     * - 'height' is the height of the bounding rectangle.
     * - 'undefined' if the element is not in the view port.
     */
    async getElementInViewPortRect(element) {
      if (element.matches("option, optgroup")) {
        const nearestSelect = element.closest("select");
        if (!nearestSelect) {
          return void 0;
        }
        return this.getElementInViewPortRect(nearestSelect);
      }
      const entry = await this.checkElementViewPortIntersection(element);
      if (!entry?.isIntersecting) {
        return void 0;
      }
      return entry.intersectionRect;
    }
    /**
     * Checks if an element is in the view port.
     * @param element {Element}The element to check.
     * @returns {Promise<boolean>} A Promise that resolves to a boolean indicating if the element is in the view port.
     */
    async isElementInViewPort(element) {
      if (element.matches("option, optgroup")) {
        const nearestSelect = element.closest("select");
        if (!nearestSelect) {
          return false;
        }
        return this.isElementInViewPort(nearestSelect);
      }
      const entry = await this.checkElementViewPortIntersection(element);
      if (!entry) {
        return false;
      }
      return entry.isIntersecting;
    }
    /**
     * Checks if an element is visible.
     * @param element The element to check.
     * @returns {boolean} A boolean indicating if the element is visible.
     */
    isElementVisible(element) {
      return this.computeBox(element).visible;
    }
    /**
     * Checks if an element is disabled.
     * @param element The element to check.
     * @returns {boolean} A boolean indicating if the element is disabled.
     */
    isElementDisabled(element) {
      return this.domUtilities.isNativelyDisabled(element) || this.ariaUtilities.hasExplicitAriaDisabled(element);
    }
    /**
     * Checks if an element is read only.
     * @param element The element to check.
     * @returns {boolean | 'error'} A boolean indicating if the element is read only, 
     * or 'error' if the element is not an <input>, <textarea>, <select>, or [contenteditable]
     * and does not have a role allowing [aria-readonly].
     */
    isElementReadOnly(element) {
      const tagName = this.domUtilities.getNormalizedElementTagName(element);
      if (["INPUT", "TEXTAREA", "SELECT"].includes(tagName)) {
        return element.hasAttribute("readonly");
      }
      if (this.ariaUtilities.isAriaReadOnlyRole(element)) {
        return element.getAttribute("aria-readonly") === "true";
      }
      if (element.isContentEditable) {
        return false;
      }
      return "error";
    }
    /**
     * Checks if an element is scrollable into view.
     * @param element The element to check.
     * @returns {boolean} A boolean indicating if the element is scrollable into view.
     */
    isElementScrollable(element) {
      const style = this.getElementComputedStyle(element);
      if (!style) {
        return true;
      }
      return !this.isHiddenByOverflow(element, style);
    }
    /**
     * Gets a click point of an element.
     * @param targetElement The element to get the click point of.
     * @param offset The offset of the click point from the center of the element.
     * @returns {Promise<{ status: 'success' | 'error', message?: string, hitPoint?: { x: number, y: number } }>} A Promise that resolves to an object with information about the click point.
     * - 'status' is the status of the check.
     * - 'message' is the message of the error, if the status is 'error'.
     * - 'hitPoint' is the hit point of the click, if the status is 'success'.
     *   - 'x' is the x coordinate of the click point.
     *   - 'y' is the y coordinate of the click point.
     */
    async getElementClickPoint(targetElement, offset) {
      const roots = this.getComponentRootElements(targetElement);
      const rect = await this.getElementInViewPortRect(targetElement);
      if (!rect) {
        return { status: "error", message: "element is not in view port" };
      }
      if (rect.width === 0 || rect.height === 0) {
        return { status: "error", message: `element is not visible (width: ${rect.width}, height: ${rect.height})` };
      }
      const hitPoint = {
        x: rect.x + rect.width / 2 + (offset?.x ?? 0),
        y: rect.y + rect.height / 2 + (offset?.y ?? 0)
      };
      const hitParents = [];
      let hitElement = this.getHitElementFromPoint(roots, hitPoint);
      while (hitElement && hitElement !== targetElement) {
        hitParents.push(hitElement);
        hitElement = hitElement.assignedSlot ?? this.domUtilities.getParentElementOrShadowHost(hitElement);
      }
      if (hitElement === targetElement) {
        return { status: "success", hitPoint };
      }
      return { status: "error", message: this.createElementObscuredErrorMessage(targetElement, hitParents) };
    }
    /**
     * Gets a list of the document or shadow root elements that contain the target element.
     * @param targetElement {Element} The element to get the component root elements of.
     * @returns {Array<Document | ShadowRoot>} An array of component root elements.
     */
    getComponentRootElements(targetElement) {
      const roots = [];
      let parentElement = targetElement;
      while (parentElement) {
        const root = this.domUtilities.getEnclosingShadowRootOrDocument(parentElement);
        if (!root) {
          break;
        }
        roots.push(root);
        if (root.nodeType === Node.DOCUMENT_NODE) {
          break;
        }
        parentElement = root.host;
      }
      return roots;
    }
    /**
     * Gets the element that is hit by a point.
     * @param componentRootElements {Array<Document | ShadowRoot>} The document or shadow root elements to check.
     * @param hitPoint {x: number, y: number} The point to check.
     * @returns {Element | undefined} The element that is hit by the point, or undefined if no element is hit.
     */
    getHitElementFromPoint(componentRootElements, hitPoint) {
      let hitElement;
      for (let index = componentRootElements.length - 1; index >= 0; index--) {
        const root = componentRootElements[index];
        const elements = root.elementsFromPoint(hitPoint.x, hitPoint.y);
        const singleElement = root.elementFromPoint(hitPoint.x, hitPoint.y);
        if (singleElement && elements[0] && this.domUtilities.getParentElementOrShadowHost(singleElement) === elements[0]) {
          const style = globalThis.getComputedStyle(singleElement);
          if (style?.display === "contents") {
            elements.unshift(singleElement);
          }
        }
        if (elements[0]?.shadowRoot === root && elements[1] === singleElement) {
          elements.shift();
        }
        const innerElement = elements[0];
        if (!innerElement) {
          break;
        }
        hitElement = innerElement;
        if (index && innerElement !== componentRootElements[index - 1].host) {
          break;
        }
      }
      return hitElement;
    }
    /**
     * Gets a value indicating whether an element is hidden by overflow of its containing elements.
     * @param element {Element} The element to check.
     * @param style {CSSStyleDeclaration} The computed style of the element.
     * @returns {boolean} True if the element is hidden by overflow; otherwise, false.
     */
    isHiddenByOverflow(element, style) {
      if (!this.checkIsHiddenByOverflow(element, style)) {
        return false;
      }
      const children = Array.from(element.childNodes).filter((child) => this.findElementFromNode(child, "none") !== null).reduce((accumulator, current) => {
        if (!accumulator.includes(current)) {
          accumulator.push(current);
        }
        return accumulator;
      }, []);
      const childrenHiddenByOverflow = children.filter((child) => {
        const childBox = this.computeBox(child);
        const hasPositiveSize = childBox.rect && childBox.visible;
        if (!hasPositiveSize) {
          return true;
        }
        const childStyle = this.getElementComputedStyle(child);
        if (!childStyle) {
          return true;
        }
        return this.isHiddenByOverflow(child, childStyle);
      });
      return childrenHiddenByOverflow.length === children.length;
    }
    /**
     * Gets a value indicating whether an element is hidden by overflow of its containing elements.
     * @param element {Element} The element to check.
     * @param style {CSSStyleDeclaration} The computed style of the element.
     * @returns {boolean} True if the element is hidden by overflow of its containing elements; otherwise, false.
     */
    checkIsHiddenByOverflow(element, style) {
      const htmlElement = element.ownerDocument.documentElement;
      let parentElement = this.getNearestOverflowAncestor(element, style, htmlElement);
      while (parentElement) {
        const parentStyle = this.getElementComputedStyle(parentElement);
        if (!parentStyle) {
          return true;
        }
        const parentOverflowX = parentStyle.getPropertyValue("overflow-x");
        const parentOverflowY = parentStyle.getPropertyValue("overflow-y");
        if (parentOverflowX !== "visible" || parentOverflowY !== "visible") {
          const parentBox = this.computeBox(parentElement);
          if (!parentBox.rect || !parentBox.visible) {
            return true;
          }
          const elementBox = this.computeBox(element);
          if (!elementBox.rect || !elementBox.visible) {
            return true;
          }
          const parentRect = parentBox.rect;
          const elementRect = elementBox.rect;
          const isLeftOf = elementRect.x + elementRect.width < parentRect.x;
          const isAbove = elementRect.y + elementRect.height < parentRect.y;
          if (isLeftOf && parentOverflowX === "hidden" || isAbove && parentOverflowY === "hidden") {
            return true;
          }
          const isRightOf = elementRect.x >= parentRect.x + parentRect.width;
          const isBelow = elementRect.y >= parentRect.y + parentRect.height;
          if (isRightOf && parentOverflowX === "hidden" || isBelow && parentOverflowY === "hidden") {
            return true;
          } else if (isRightOf && parentOverflowX !== "visible" || isBelow && parentOverflowY !== "visible") {
            if (style.getPropertyValue("position") === "fixed") {
              const isParentHtmlElement = parentElement.tagName === "HTML";
              if (isParentHtmlElement && !parentElement.ownerDocument.scrollingElement) {
                return true;
              }
              const scrollPosition = isParentHtmlElement ? {
                x: parentElement.ownerDocument.scrollingElement?.scrollLeft ?? 0,
                y: parentElement.ownerDocument.scrollingElement?.scrollTop ?? 0
              } : (
                /* istanbul ignore next -- @preserve */
                {
                  x: parentElement.scrollLeft,
                  y: parentElement.scrollTop
                }
              );
              if (elementRect.x >= htmlElement.scrollWidth - scrollPosition.x || elementRect.y >= htmlElement.scrollHeight - scrollPosition.y) {
                return true;
              }
            }
          }
        }
        parentElement = this.getNearestOverflowAncestor(parentElement, parentStyle, htmlElement);
      }
      return false;
    }
    /**
     * Gets the nearest overflow ancestor of an element.
     * @param element {Element} The element to check.
     * @param style {CSSStyleDeclaration} The computed style of the element.
     * @param htmlElement {HTMLElement} The HTML element to check.
     * @returns {Element | null} The nearest overflow ancestor of the element, or null if no overflow ancestor is found.
     */
    getNearestOverflowAncestor(element, style, htmlElement) {
      const elementPosition = style.getPropertyValue("position");
      if (elementPosition === "fixed") {
        return element === htmlElement ? null : htmlElement;
      }
      let container = element.parentElement;
      if (!container) {
        return null;
      }
      const containerStyle = this.getElementComputedStyle(container);
      if (!containerStyle) {
        return null;
      }
      while (container && !this.canBeOverflowed(container, containerStyle, htmlElement)) {
        container = container.parentElement;
      }
      return container;
    }
    /**
     * Gets a value indicating whether an element can be overflowed.
     * @param element {Element} The element to check.
     * @param style {CSSStyleDeclaration} The computed style of the element.
     * @param htmlElement {HTMLElement} The root HTML element containing the element.
     * @returns {boolean} True if the element can be overflowed; otherwise, false.
     */
    canBeOverflowed(element, style, htmlElement) {
      if (element === htmlElement) {
        return true;
      }
      const containerStyle = this.getElementComputedStyle(element);
      if (!containerStyle) {
        return true;
      }
      const containerDisplay = containerStyle.getPropertyValue("display");
      if (containerDisplay.startsWith("inline") || containerDisplay === "contents") {
        return false;
      }
      const elementPosition = style.getPropertyValue("position");
      const containerPosition = containerStyle.getPropertyValue("position");
      if (elementPosition === "absolute" && containerPosition === "static") {
        return false;
      }
      return true;
    }
    /**
     * Creates an error message for an element that is obscured by another element, including a description
     * of the element that is obscuring the target element.
     * @param targetElement {Element} The element that is obscured.
     * @param hitParents {Element[]} The elements that are in the chain of the target element.
     * @returns {string} The error message.
     */
    createElementObscuredErrorMessage(targetElement, hitParents) {
      const hitTargetDescription = this.nodePreviewer.previewNode(hitParents[0] || document.documentElement);
      let rootHitTargetDescription;
      let element = targetElement;
      while (element) {
        const index = hitParents.indexOf(element);
        if (index !== -1) {
          if (index > 1) {
            rootHitTargetDescription = this.nodePreviewer.previewNode(hitParents[index - 1]);
          }
          break;
        }
        element = this.domUtilities.getParentElementOrShadowHost(element);
      }
      if (rootHitTargetDescription) {
        return `${hitTargetDescription} from ${rootHitTargetDescription} subtree`;
      }
      return hitTargetDescription;
    }
    /**
     * Checks if an element is in the view port.
     * @param element {Element} The element to check.
     * @returns {Promise<IntersectionObserverEntry | undefined>} 
     * A Promise that resolves to the IntersectionObserverEntry for the element,
     * or undefined if the element is not in the view port.
     * - 'isIntersecting' is true if the element is intersecting with the view port.
     * - 'intersectionRect' is the bounding rectangle of the element in the view port.
     *   - 'x' is the x coordinate of the bounding rectangle.
     *   - 'y' is the y coordinate of the bounding rectangle.
     *   - 'width' is the width of the bounding rectangle.
     *   - 'height' is the height of the bounding rectangle.
     * - 'undefined' if the element's bounding rectangle does not intersect with the view port.
     */
    async checkElementViewPortIntersection(element) {
      const observerEntries = [];
      const viewportObserver = new IntersectionObserver((entries) => {
        for (const entry2 of entries) {
          observerEntries.push(entry2);
        }
      });
      viewportObserver.observe(element);
      const waiter = new RequestAnimationFrameWaiter(
        () => {
          const filtered = observerEntries.filter((entry2) => entry2.target === element);
          if (filtered.length) {
            const { isIntersecting, intersectionRect } = filtered[0];
            const rect = { x: intersectionRect.x, y: intersectionRect.y, width: intersectionRect.width, height: intersectionRect.height };
            return { isIntersecting, intersectionRect: rect };
          }
          return void 0;
        },
        Number.MAX_SAFE_INTEGER
      );
      const entry = await waiter.waitForCondition();
      viewportObserver.unobserve(element);
      viewportObserver.disconnect();
      return entry;
    }
    /**
     * Checks if an element's position is stable, that is, it has not changed positions since the last animation frame..
     * @param node {Node} The node to check, which will be transformed into the nearest element to check the stability.
     * @returns {Promise<'error:notconnected' | boolean>} A Promise that resolves to a boolean indicating if the element is stable.
     * - 'error:notconnected' if the element is not connected.
     * - true if the element is stable; otherwise, false.
     */
    async checkElementIsStable(node) {
      let lastRect;
      let stableRafCounter = 0;
      const waiter = new RequestAnimationFrameWaiter(
        () => {
          const element = this.findElementFromNode(node, "no-follow-label");
          if (!element) {
            return "error:notconnected";
          }
          const clientRect = element.getBoundingClientRect();
          const rect = { x: clientRect.top, y: clientRect.left, width: clientRect.width, height: clientRect.height };
          if (lastRect) {
            const samePosition = rect.x === lastRect.x && rect.y === lastRect.y && rect.width === lastRect.width && rect.height === lastRect.height;
            if (!samePosition) {
              return { stable: false };
            }
            if (++stableRafCounter >= 1) {
              return { stable: true };
            }
          }
          lastRect = rect;
          return void 0;
        },
        Number.MAX_SAFE_INTEGER
        // No timeout - caller handles timeout
      );
      const result = await waiter.waitForCondition();
      if (result === "error:notconnected") {
        return "error:notconnected";
      }
      if (!result) {
        throw new Error("Unexpected undefined result from RequestAnimationFrameWaiter");
      }
      return result.stable;
    }
    /**
     * Finds the nearest element from a node, based on the behavior.
     * @param node {Node} The node to find the element from.
     * @param behavior { 'none' | 'follow-label' | 'no-follow-label' | 'button-link' } The behavior to use.
     * @returns {Element | null} The nearest element from the node, or null if no element is found.
     */
    findElementFromNode(node, behavior) {
      let element = node.nodeType === Node.ELEMENT_NODE ? node : node.parentElement;
      if (!element) {
        return null;
      }
      if (behavior === "none") {
        return element;
      }
      if (!element.matches("input, textarea, select") && !element.isContentEditable) {
        if (behavior === "button-link") {
          element = element.closest("button, [role=button], a, [role=link]") ?? element;
        } else {
          element = element.closest("button, [role=button], [role=checkbox], [role=radio]") ?? element;
        }
      }
      if (behavior === "follow-label") {
        if (!element.matches("a, input, textarea, button, select, [role=link], [role=button], [role=checkbox], [role=radio]") && !element.isContentEditable) {
          const enclosingLabel = element.closest("label");
          if (enclosingLabel?.control) {
            element = enclosingLabel.control;
          }
        }
      }
      return element;
    }
    /**
     * Computes the box of an element, including its position and size..
     * @param element {Element} The element to compute the box of.
     * @returns {Box} The computed box of the element.
     */
    computeBox(element) {
      const style = this.getElementComputedStyle(element);
      if (!style) {
        return { visible: true, inline: false };
      }
      const cursor = style.cursor;
      if (style.display === "contents") {
        for (let child = element.firstChild; child; child = child.nextSibling) {
          if (child.nodeType === 1 && this.isElementVisible(child)) {
            return { visible: true, inline: false, cursor };
          }
          if (child.nodeType === 3 && this.isVisibleTextNode(child)) {
            return { visible: true, inline: true, cursor };
          }
        }
        return { visible: false, inline: false, cursor };
      }
      if (!this.isElementStyleVisibilityVisible(element, style)) {
        return { cursor, visible: false, inline: false };
      }
      const rect = element.getBoundingClientRect();
      return { rect, cursor, visible: rect.width > 0 && rect.height > 0, inline: style.display === "inline" };
    }
    /**
     * Checks if a text node is visible.
     * @param node {Text} The text node to check.
     * @returns {boolean} True if the text node is visible; otherwise, false.
     */
    isVisibleTextNode(node) {
      const range = node.ownerDocument.createRange();
      range.selectNode(node);
      const rect = range.getBoundingClientRect();
      return rect.width > 0 && rect.height > 0;
    }
    /**
     * Gets the computed style of an element.
     * @param element {Element} The element to get the computed style of.
     * @param pseudo {string} The pseudo-element to get the computed style of.
     * @returns {CSSStyleDeclaration | undefined} The computed style of the element, or undefined
     * if the element is not in the document.
     */
    getElementComputedStyle(element, pseudo) {
      const cache = this.getCache(pseudo);
      if (cache.has(element)) {
        return cache.get(element);
      }
      const style = element.ownerDocument?.defaultView ? element.ownerDocument.defaultView.getComputedStyle(element, pseudo) : void 0;
      cache.set(element, style);
      return style;
    }
    /**
     * Gets the cache for a pseudo-element type.
     * @param pseudo {string | undefined} The pseudo-element type to get the cache for. If omitted, the cache for main elements is returned.
     * @returns {Map<Element, CSSStyleDeclaration | undefined>} The cache for the pseudo-element.
     * If no cache has yet been created for the pseudo-element, a new cache is created and returned.
     */
    getCache(pseudo) {
      if (pseudo === "::before") {
        return this.cacheStyleBefore ??= /* @__PURE__ */ new Map();
      } else if (pseudo === "::after") {
        return this.cacheStyleAfter ??= /* @__PURE__ */ new Map();
      } else {
        return this.cacheStyle ??= /* @__PURE__ */ new Map();
      }
    }
    /**
     * Gets a value indicating whether an element is visible as defined in the element's style attributes.
     * @param element {Element} The element to check.
     * @param style {CSSStyleDeclaration | undefined} The computed style of the element. If omitted, the computed style is retrieved from the element.
     * @returns {boolean} True if the element's style visibility is visible; otherwise, false.
     */
    isElementStyleVisibilityVisible(element, style) {
      style = style ?? this.getElementComputedStyle(element);
      if (!style) {
        return true;
      }
      if (!element.checkVisibility()) {
        return false;
      }
      if (style.visibility !== "visible") {
        return false;
      }
      return true;
    }
    /**
     * Creates an error with an empty stack.
     * @param message {string} The message of the error.
     * @returns {Error} The error with an empty stack.
     */
    createError(message) {
      const error = new Error(message);
      error.stack = "";
      delete error.stack;
      return error;
    }
  };
  var elementStateInspector_default = ElementStateInspector;
  return __toCommonJS(index_exports);
})();
/* istanbul ignore next -- @preserve */
/* istanbul ignore else -- @preserve */
/* istanbul ignore if -- @preserve */
//# sourceMappingURL=acquiescence.browser.js.map
