import { JSDOM } from "jsdom";

const globalKeys = [
  "window",
  "self",
  "document",
  "navigator",
  "location",
  "Element",
  "HTMLElement",
  "SVGElement",
  "ShadowRoot",
  "Node",
  "NodeList",
  "Text",
  "Comment",
  "DocumentFragment",
  "DOMParser",
  "Event",
  "CustomEvent",
  "MouseEvent",
  "MutationObserver",
  "getComputedStyle",
  "requestAnimationFrame",
  "cancelAnimationFrame"
];

let globalBindingsInitialized = false;

export function installDomEnvironment() {
  const dom = new JSDOM("<!doctype html><html><body></body></html>", {
    pretendToBeVisual: true,
    url: "http://localhost/"
  });
  const previousValues = new Map();

  for (const key of globalKeys) {
    previousValues.set(key, Object.prototype.hasOwnProperty.call(globalThis, key) ? globalThis[key] : undefined);
  }

  installDomGlobals(dom.window);

  return () => {
    document.body.innerHTML = "";
    restoreDomGlobals(previousValues);
    dom.window.close();
  };
}

export async function withDomEnvironment(callback) {
  const cleanup = installDomEnvironment();

  try {
    return await callback(globalThis.window);
  } finally {
    cleanup();
  }
}

function installDomGlobals(window) {
  ensureGlobalVariableBindings(globalKeys);

  const assignments = {
    window,
    self: window,
    document: window.document,
    navigator: window.navigator,
    location: window.location,
    Element: window.Element,
    HTMLElement: window.HTMLElement,
    SVGElement: window.SVGElement,
    ShadowRoot: window.ShadowRoot,
    Node: window.Node,
    NodeList: window.NodeList,
    Text: window.Text,
    Comment: window.Comment,
    DocumentFragment: window.DocumentFragment,
    DOMParser: window.DOMParser,
    Event: window.Event,
    CustomEvent: window.CustomEvent,
    MouseEvent: window.MouseEvent,
    MutationObserver: window.MutationObserver,
    getComputedStyle: window.getComputedStyle.bind(window),
    requestAnimationFrame: window.requestAnimationFrame.bind(window),
    cancelAnimationFrame: window.cancelAnimationFrame.bind(window)
  };

  for (const [key, value] of Object.entries(assignments)) {
    setGlobalValue(key, value);
  }
}

function restoreDomGlobals(previousValues) {
  for (const [key, value] of previousValues.entries()) {
    setGlobalValue(key, value);
  }
}

function ensureGlobalVariableBindings(keys) {
  if (globalBindingsInitialized) {
    return;
  }

  try {
    (0, eval)(`var ${keys.join(", ")};`);
  } catch {
    // Best effort only. Some runtimes may refuse dynamic global bindings.
  }

  globalBindingsInitialized = true;
}

function setGlobalValue(key, value) {
  try {
    globalThis[key] = value;
    return;
  } catch {
    // Fall through and force a writable own property when assignment is rejected.
  }

  Object.defineProperty(globalThis, key, {
    configurable: true,
    writable: true,
    value
  });
}
