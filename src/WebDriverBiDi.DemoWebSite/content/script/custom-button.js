class CustomButton extends HTMLElement {
  constructor() {
    super();
    // Defer shadow DOM attachment to connectedCallback to read attributes
    this._shadowRoot = null;
    this._button = null;
  }

  connectedCallback() {
    if (this._shadowRoot === null) {
      this.setAttribute("role", "button");
      // Determine shadow mode from attribute, default to 'open'
      const mode = this.getAttribute('shadow-mode') === 'closed' ? 'closed' : 'open';
      this._shadowRoot = this.attachShadow({ mode: mode });
      
      // Create template
      const template = document.createElement('template');
      template.innerHTML = `
        <style>
          :host {
            display: inline-block;
          }
          button {
            padding: 10px 20px;
            font-size: 16px;
            cursor: pointer;
            border: 2px solid #333;
            border-radius: 4px;
            background-color: #f0f0f0;
            color: #333;
            transition: background-color 0.2s, transform 0.1s;
          }
          button:hover {
            background-color: #e0e0e0;
          }
          button:active {
            transform: scale(0.98);
          }
        </style>
        <button><slot>Click Me</slot></button>
      `;
      
      this._shadowRoot.appendChild(template.content.cloneNode(true));
      this._button = this._shadowRoot.querySelector('button');
      this._button.setAttribute('id', `inner-${mode}-button`);

      // Apply text attribute if set
      const textAttr = this.getAttribute('text');
      if (textAttr !== null) {
        this._button.textContent = textAttr;
        this.ariaLabel = textAttr;
      }

      // Apply disabled attribute if set
      if (this.hasAttribute('disabled')) {
        this._button.disabled = true;
      }
    }
  }

  static get observedAttributes() {
    return ['text', 'disabled'];
  }

  attributeChangedCallback(name, oldValue, newValue) {
    // Only apply changes if button is initialized
    if (this._button) {
      if (name === 'text' && newValue !== null) {
        this._button.textContent = newValue;
      }
      if (name === 'disabled') {
        this._button.disabled = newValue !== null;
      }
    }
  }

  get text() {
    return this._button ? this._button.textContent : this.getAttribute('text');
  }

  set text(value) {
    if (this._button) {
      this._button.textContent = value;
    }
    this.setAttribute('text', value);
  }

  get disabled() {
    return this._button ? this._button.disabled : this.hasAttribute('disabled');
  }

  set disabled(value) {
    if (this._button) {
      this._button.disabled = value;
    }
    if (value) {
      this.setAttribute('disabled', '');
    } else {
      this.removeAttribute('disabled');
    }
  }

  // Getter for the internal shadow root (works for both open and closed modes)
  get internalShadowRoot() {
    return this._shadowRoot;
  }

  // Method to add click event listener to the internal button
  addClickListener(callback) {
    if (this._button) {
      this._button.addEventListener('click', callback);
    }
  }

  // Method to remove click event listener from the internal button
  removeClickListener(callback) {
    if (this._button) {
      this._button.removeEventListener('click', callback);
    }
  }
}

customElements.define('custom-button', CustomButton);
