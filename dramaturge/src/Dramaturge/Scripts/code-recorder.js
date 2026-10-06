// <copyright file="code-recorder.js" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Records the user's actions for code generation, and, in the top frame, shows the toolbar that picks locators and
// adds assertions. Each message goes through a channel as its JSON, then the element it is about and that element's
// nameable ancestors, or null.
(() => {
  const buttons = [
    ['record', 'Record'],
    ['pick', 'Pick locator'],
    ['assertVisible', 'Assert visible'],
    ['assertText', 'Assert text'],
    ['assertValue', 'Assert value'],
    ['assertSnapshot', 'Assert snapshot'],
  ];
  const style = `
    :host { all: initial; position: fixed; top: 8px; left: 50%; transform: translateX(-50%); z-index: 2147483647; width: 600px; }
    .bar, form { display: grid; gap: 4px; padding: 4px; background: #f8f9fa; border: 1px solid #9aa0a6; border-radius: 6px; font: 12px system-ui, sans-serif; }
    .bar { grid-template-columns: repeat(6, 1fr); }
    form { grid-template-columns: 1fr auto auto; margin-top: 4px; }
    button { font: inherit; height: 24px; padding: 0 4px; border: 1px solid #9aa0a6; border-radius: 4px; background: #fff; cursor: pointer; }
    button[aria-pressed=true] { background: #1a73e8; border-color: #1a73e8; color: #fff; }
    input { font: inherit; }`;

  let session = null;

  const facts = ({ element, ...rest }) => rest;

  // A control's value, or null for an element without one; checkboxes and radio buttons are checked or not.
  const valueOf = (element, checked) => {
    if (element instanceof HTMLInputElement && (element.type === 'checkbox' || element.type === 'radio')) {
      return { kind: 'assertChecked', checked };
    }

    return element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement || element instanceof HTMLSelectElement
      ? { kind: 'assertValue', value: element.value }
      : null;
  };

  const createToolbar = (onChoose, onAnswer) => {
    const host = document.createElement('dramaturge-toolbar');
    host.setAttribute('aria-hidden', 'true');
    const root = host.attachShadow({ mode: 'closed' });
    root.innerHTML = `<style>${style}</style><div class="bar"></div>`;
    const bar = root.querySelector('.bar');
    for (const [mode, label] of buttons) {
      const button = document.createElement('button');
      button.type = 'button';
      button.dataset.mode = mode;
      button.textContent = label;
      button.addEventListener('click', () => onChoose(mode));
      bar.appendChild(button);
    }

    document.documentElement.appendChild(host);
    let form = null;
    return {
      host,
      show(mode) {
        for (const button of bar.children) {
          button.setAttribute('aria-pressed', String(button.dataset.mode === mode));
        }
      },
      // Asks for the text to assert, starting from the element's.
      ask(text) {
        form?.remove();
        form = document.createElement('form');
        form.innerHTML = '<input aria-label="Text"><button type="submit">Assert</button><button type="button">Cancel</button>';
        const input = form.querySelector('input');
        input.value = text;
        const close = (answer) => {
          form.remove();
          form = null;
          onAnswer(answer);
        };
        form.addEventListener('submit', (event) => {
          event.preventDefault();
          close(input.value);
        });
        form.querySelector('button[type=button]').addEventListener('click', () => close(null));
        input.addEventListener('keydown', (event) => event.key === 'Escape' && close(null));
        root.appendChild(form);
        input.focus();
        input.select();
      },
      remove() {
        host.remove();
      },
    };
  };

  const start = (send, testIdAttribute, mode) => {
    const describer = new Acquiescence.ElementDescriber();
    const describe = (element, details) => {
      const { target, ancestors } = describer.describe(element, { testIdAttribute });
      send([JSON.stringify({ ...details, target: facts(target), ancestors: ancestors.map(facts) }), target.element, ...ancestors.map((ancestor) => ancestor.element)]);
    };
    let toolbar = null;
    let current = null;
    let asked = null;
    const ignore = (element) => toolbar !== null && element === toolbar.host;

    const recorder = new Acquiescence.ActionRecorder(({ element, ...action }) => describe(element, action), { ignore });
    // A clicked checkbox or radio button is toggled until its cancelled click ends, so its state is read as the mouse
    // button goes down, by a listener that runs before the picker's, which cancels the mouse events.
    let pressed = null;
    const onPointerDown = (event) => {
      const element = event.composedPath()[0];
      pressed = element instanceof HTMLInputElement ? { element, checked: element.checked } : null;
    };
    document.addEventListener('pointerdown', onPointerDown, true);
    const picker = new Acquiescence.ElementPicker((element) => onPick(element), { resolve: (element) => describer.getActionTarget(element), ignore });
    const onPick = (element) => {
      if (current === 'assertText') {
        asked = element;
        picker.stop();
        toolbar.ask(describer.describe(element, { testIdAttribute }).target.text);
        return;
      }

      const details = current === 'pick' ? { kind: 'pick' }
        : current === 'assertVisible' ? { kind: 'assertVisible' }
        : current === 'assertSnapshot' ? { kind: 'assertSnapshot', snapshot: new Acquiescence.AriaSnapshotGenerator().generate(element, { refs: false }).text }
        : valueOf(element, pressed?.element === element ? pressed.checked : element.checked);
      if (details) {
        describe(element, details);
        choose('record');
      }
    };

    // Applies a mode in this document; a mode the user chooses is also sent, so the recording applies it in the others.
    const apply = (mode) => {
      current = mode;
      recorder.stop();
      picker.stop();
      if (mode === 'record') {
        recorder.start(document);
      } else if (mode !== 'none') {
        picker.start(document);
      }

      toolbar?.show(mode);
    };
    const choose = (mode) => {
      const next = mode === current ? (mode === 'record' ? 'none' : 'record') : mode;
      apply(next);
      send([JSON.stringify({ kind: 'mode', mode: next }), null]);
    };

    if (window === window.top) {
      toolbar = createToolbar(choose, (text) => {
        if (text !== null) {
          describe(asked, { kind: 'assertText', text });
        }

        choose('record');
      });
    }

    apply(mode);
    return {
      apply,
      stop: () => {
        recorder.stop();
        picker.stop();
        toolbar?.remove();
        document.removeEventListener('pointerdown', onPointerDown, true);
      },
    };
  };

  return {
    start(send, testIdAttribute, mode) {
      session?.stop();
      session = start(send, testIdAttribute, mode);
    },

    setMode(mode) {
      session?.apply(mode);
    },

    // Each frame element's window, then its description as a message has it.
    describeFrames(testIdAttribute) {
      const describer = new Acquiescence.ElementDescriber();
      return Array.from(document.querySelectorAll('iframe, frame')).filter((element) => element.contentWindow).map((element) => {
        const { target, ancestors } = describer.describe(element, { testIdAttribute });
        return [element.contentWindow, JSON.stringify({ target: facts(target), ancestors: ancestors.map(facts) }), target.element, ...ancestors.map((ancestor) => ancestor.element)];
      });
    },

    stop() {
      session?.stop();
      session = null;
    },
  };
})()
