// Drives a real browser against a published SSR TodoList host.
// Usage: node verify-ssr-browser.mjs <baseUrl> <cdpPort> <pathBase>
//
// The page under test is produced by Jazor SSR: the server HTML must already contain the
// rendered task board, and hydration must restore a live Vue app on top of that markup.
// Hydration is proven by interaction recovery, not by a marker: the checkbox bind and the
// add-task handler only respond after the browser module graph mounted the app.
const baseUrl = process.argv[2];
const cdpPort = Number(process.argv[3]);
const configuredPathBase = process.argv[4] || "";

if (!baseUrl || !Number.isFinite(cdpPort)) {
  throw new Error("Usage: node verify-ssr-browser.mjs <baseUrl> <cdpPort> [pathBase]");
}

const pathBase = configuredPathBase && configuredPathBase !== "/"
  ? (configuredPathBase.endsWith("/") && configuredPathBase.length > 1 ? configuredPathBase.slice(0, -1) : configuredPathBase)
  : "";

function externalPath(logicalPath) {
  if (!logicalPath.startsWith("/")) {
    throw new Error(`Logical path must start with '/': ${logicalPath}`);
  }

  return logicalPath === "/" ? (pathBase ? `${pathBase}/` : "/") : `${pathBase}${logicalPath}`;
}

function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function formatRemoteArg(arg) {
  if (Object.prototype.hasOwnProperty.call(arg, "value")) {
    return String(arg.value);
  }

  return arg.description ? String(arg.description) : (arg.type || "unknown");
}

function describeBrowserDiagnostics(consoleErrors, exceptions, networkFailures) {
  const parts = [];
  if (consoleErrors.length > 0) {
    parts.push(" Console errors: " + consoleErrors.join(" | "));
  }

  if (exceptions.length > 0) {
    parts.push(" Exceptions: " + exceptions.join(" | "));
  }

  if (networkFailures.length > 0) {
    parts.push(" Network failures: " + networkFailures.join(" | "));
  }

  return parts.length > 0 ? parts.join(".") + "." : " No browser console errors were recorded.";
}

async function connectToPageTarget() {
  const targets = await fetch(`http://127.0.0.1:${cdpPort}/json/list`).then(response => response.json());
  const pageTarget =
    targets.find(target => target.type === "page" && target.url === "about:blank") ||
    targets.find(target => target.type === "page");

  if (!pageTarget?.webSocketDebuggerUrl) {
    throw new Error("No page target with webSocketDebuggerUrl was found.");
  }

  const ws = new WebSocket(pageTarget.webSocketDebuggerUrl);
  await new Promise((resolve, reject) => {
    ws.addEventListener("open", () => resolve(), { once: true });
    ws.addEventListener("error", event => reject(event.error || new Error("WebSocket open failed.")), { once: true });
  });

  return ws;
}

async function main() {
  const pageUrl = new URL(externalPath("/"), baseUrl).toString();
  const ws = await connectToPageTarget();
  let nextId = 1;
  const pending = new Map();
  const consoleErrors = [];
  const exceptions = [];
  const networkFailures = [];
  let loadResolvers = [];

  ws.addEventListener("message", event => {
    const message = JSON.parse(event.data);

    if (message.id) {
      const entry = pending.get(message.id);
      if (!entry) {
        return;
      }

      pending.delete(message.id);
      if (message.error) {
        entry.reject(new Error(message.error.message || JSON.stringify(message.error)));
      } else {
        entry.resolve(message.result);
      }
      return;
    }

    if (message.method === "Runtime.consoleAPICalled" &&
        message.params && (message.params.type === "error" || message.params.type === "assert")) {
      consoleErrors.push((message.params.args || []).map(formatRemoteArg).join(" "));
    }

    if (message.method === "Runtime.exceptionThrown") {
      const details = message.params?.exceptionDetails;
      // "text" is often just "Uncaught"; the stack lives in exception.description.
      exceptions.push(details?.exception?.description || details?.text || "Unknown runtime exception");
    }

    if (message.method === "Network.loadingFailed" && message.params?.canceled !== true) {
      networkFailures.push(`${message.params?.errorText || ""} ${message.params?.url || ""}`.trim());
    }

    if (message.method === "Page.loadEventFired") {
      const resolvers = loadResolvers;
      loadResolvers = [];
      for (const resolve of resolvers) {
        resolve();
      }
    }
  });

  function send(method, params = {}) {
    const id = nextId++;
    const promise = new Promise((resolve, reject) => pending.set(id, { resolve, reject }));
    ws.send(JSON.stringify({ id, method, params }));
    return promise;
  }

  async function evaluate(expression) {
    const result = await send("Runtime.evaluate", {
      expression,
      returnByValue: true,
      awaitPromise: true
    });

    if (result.exceptionDetails) {
      throw new Error(result.exceptionDetails.text || "Runtime.evaluate failed.");
    }

    return result.result?.value;
  }

  function waitForLoad(timeoutMs = 20000) {
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => reject(new Error("Timed out waiting for Page.loadEventFired.")), timeoutMs);
      loadResolvers.push(() => {
        clearTimeout(timer);
        resolve();
      });
    });
  }

  async function waitUntil(expression, description, timeoutMs = 15000, intervalMs = 100) {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      const value = await evaluate(expression);
      if (value) {
        return value;
      }

      await delay(intervalMs);
    }

    throw new Error(`Timed out waiting for ${description}.`);
  }

  await send("Runtime.enable");
  await send("Page.enable");
  await send("Network.enable");

  const load = waitForLoad();
  await send("Page.navigate", { url: pageUrl });
  await load;

  // The server-rendered task board must arrive in the document before any client module runs.
  await waitUntil(
    `(() => { const board = document.querySelector('main[data-todo-template="todo-template-v1"]'); const total = document.getElementById('todo-total-count'); const parameter = board?.getAttribute('data-todo-parameter'); const status = board?.getAttribute('data-todo-parameter-status'); return board && total && total.textContent === "3" && parameter === "SSR ParameterView title" && status === "ready" ? "ssr-board" : null; })()`,
    "server-rendered TodoApp board");

  // Interaction recovery: retry until the hydrated Vue app responds. Before hydration a click
  // is a no-op, so reaching "1" proves the browser module graph mounted a live application.
  const toggleExpression = `(() => {
    const open = document.getElementById('todo-open-count');
    const done = document.getElementById('todo-done-count');
    const checkbox = document.querySelector('section[aria-label="Tasks"] input[type="checkbox"]');
    if (!checkbox || !open || !done) return "missing";
    if (open.textContent === "2" && done.textContent === "1") { checkbox.click(); return "clicked"; }
    if (open.textContent === "1" && done.textContent === "2") return "toggled";
    return "unexpected:" + open.textContent + "/" + done.textContent;
  })()`;

  var toggleState = "missing";
  for (var attempt = 0; attempt < 150 && (toggleState === "missing" || toggleState === "clicked"); attempt += 1) {
    toggleState = await evaluate(toggleExpression);
    if (toggleState === "clicked" || toggleState === "missing") {
      await delay(100);
    }
  }

  if (toggleState !== "toggled") {
    throw new Error(
      `Hydrated checkbox bind did not recover interaction: ${toggleState}.` +
      describeBrowserDiagnostics(consoleErrors, exceptions, networkFailures));
  }

  // The add-task composer exercises the oninput draft bind plus the click handler in one flow.
  const addTaskExpression = `(() => {
    const draft = document.getElementById('todo-draft');
    const add = document.getElementById('todo-add');
    const total = document.getElementById('todo-total-count');
    if (!draft || !add || !total) return "missing";
    if (total.textContent === "4") return "added";
    draft.value = "ssr gate task";
    draft.dispatchEvent(new Event("input", { bubbles: true }));
    add.click();
    return "submitted";
  })()`;

  var addState = "missing";
  for (var addAttempt = 0; addAttempt < 150 && (addState === "missing" || addState === "submitted"); addAttempt += 1) {
    addState = await evaluate(addTaskExpression);
    if (addState === "submitted" || addState === "missing") {
      await delay(100);
    }
  }

  if (addState !== "added") {
    throw new Error(
      `Hydrated add-task flow did not recover interaction: ${addState}.` +
      describeBrowserDiagnostics(consoleErrors, exceptions, networkFailures));
  }

  // Hydration must keep Vue owning the server-rendered root; a failed hydration that re-rendered
  // from scratch would still show the board, so assert the authored template marker survived.
  const boardState = await evaluate(
    `(() => { const board = document.querySelector('main[data-todo-template="todo-template-v1"]'); const total = document.getElementById('todo-total-count'); const parameter = board?.getAttribute('data-todo-parameter'); const status = board?.getAttribute('data-todo-parameter-status'); return board && total && total.textContent === "4" && parameter === "SSR ParameterView title" && status === "ready" ? "board-live" : "board-state:" + (total ? total.textContent : "none") + "/" + parameter + "/" + status; })()`);
  if (boardState !== "board-live") {
    throw new Error(`Interacted board lost its SSR root or counts: ${boardState}.`);
  }

  if (consoleErrors.length > 0) {
    throw new Error("Browser console errors: " + consoleErrors.join(" | "));
  }

  if (exceptions.length > 0) {
    throw new Error("Browser exceptions: " + exceptions.join(" | "));
  }

  if (networkFailures.length > 0) {
    throw new Error("Browser network failures: " + networkFailures.join(" | "));
  }

  console.log("SSR browser verification passed: server board, hydration interaction recovery, no console errors.");
  ws.close();
}

main().catch(error => {
  console.error(error?.stack || error);
  process.exit(1);
});
