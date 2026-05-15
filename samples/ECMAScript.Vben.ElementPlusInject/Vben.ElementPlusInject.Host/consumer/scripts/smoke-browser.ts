import { join, normalize, resolve, sep } from "node:path";
import { fileExists, prepareWorkspace } from "./lib/pipeline.ts";

type BrowserSmokeOptions = {
  expectedTexts: string[];
};

type CdpMessage = {
  id?: number;
  method?: string;
  params?: Record<string, unknown>;
  result?: unknown;
  error?: { message?: string };
};

const defaultExpectedTexts = [
  "ECMAScript.Vben",
  "Element Plus injected shell",
  "Operations overview",
  "Compile-time injected Vben shell composed with Element Plus",
  "Build pipeline",
  "Container injection",
  "Consumer contract",
  "Create release",
  "Export report",
  "ops@prod"
];

export async function runBrowserSmoke(): Promise<void> {
  const workspace = await prepareWorkspace(true);
  const indexPath = join(workspace.distRoot, "index.html");
  if (!(await fileExists(indexPath))) {
    throw new Error(`RazorVue browser smoke requires a built Deno dist. Missing '${indexPath}'.`);
  }

  const options = readBrowserSmokeOptions();
  const server = await startStaticServer(workspace.distRoot);
  const browser = await startBrowser();

  try {
    const page = await connectToPage(browser.cdpPort);
    await page.enable();
    await page.navigate(`http://127.0.0.1:${server.port}/`);

    const expectedTextsExpression = createBodyContainsAllExpression(options.expectedTexts);
    await page.waitUntil(expectedTextsExpression, "RazorVue browser mount");

    const initialState = await page.evaluateJson<{
      title: string;
      hasApp: boolean;
      stylesheetCount: number;
      scriptCount: number;
      bodyText: string;
    }>(`
      (function(){
        return {
          title: document.title || "",
          hasApp: !!document.querySelector("#app"),
          stylesheetCount: document.querySelectorAll('link[rel="stylesheet"]').length,
          scriptCount: document.querySelectorAll('script[type="module"]').length,
          bodyText: document.body ? (document.body.textContent || "") : ""
        };
      })()
    `);

    if (!initialState.hasApp) {
      throw new Error("RazorVue browser smoke did not find the #app mount element.");
    }
    if (initialState.stylesheetCount < 1) {
      throw new Error("RazorVue browser smoke did not load the generated CSS bundle.");
    }
    if (initialState.scriptCount < 1) {
      throw new Error("RazorVue browser smoke did not load the generated module entry.");
    }

    const browserFailures = page.collectFailures();
    if (browserFailures.length > 0) {
      throw new Error(["RazorVue browser smoke observed browser runtime failures.", ...browserFailures].join("\n"));
    }

    console.log("ECMAScript.Vben ElementPlusInject browser smoke passed.");
  } finally {
    await browser.dispose();
    await server.dispose();
  }
}

function readBrowserSmokeOptions(): BrowserSmokeOptions {
  return {
    expectedTexts: readJsonTextArray("RAZORVUE_BROWSER_EXPECTED_TEXTS_JSON") ?? defaultExpectedTexts
  };
}

function readJsonTextArray(name: string): string[] | null {
  const value = Deno.env.get(name);
  if (value === undefined || value.trim().length === 0) {
    return null;
  }

  const parsed = JSON.parse(value) as unknown;
  if (!Array.isArray(parsed) || parsed.some((item) => typeof item !== "string" || item.length === 0)) {
    throw new Error(`${name} must be a JSON array of non-empty strings.`);
  }

  return parsed;
}

function createBodyContainsAllExpression(expectedTexts: string[]): string {
  return `
    (function(){
      const text = document.body ? (document.body.textContent || "") : "";
      const expected = ${JSON.stringify(expectedTexts)};
      return expected.every((item) => text.includes(item));
    })()
  `;
}

async function startStaticServer(root: string): Promise<{ port: number; dispose: () => Promise<void> }> {
  const abortController = new AbortController();
  const server = Deno.serve(
    {
      hostname: "127.0.0.1",
      port: 0,
      signal: abortController.signal,
      onListen: () => {
      }
    },
    async (request) => await serveStaticFile(root, request));

  return {
    port: server.addr.port,
    dispose: async () => {
      abortController.abort();
      await server.finished.catch(() => {
      });
    }
  };
}

async function serveStaticFile(root: string, request: Request): Promise<Response> {
  const requestUrl = new URL(request.url);
  const relativePath = requestUrl.pathname === "/"
    ? "index.html"
    : decodeURIComponent(requestUrl.pathname.slice(1));
  const filePath = normalize(resolve(root, relativePath));
  const normalizedRoot = normalize(resolve(root));
  const rootPrefix = normalizedRoot.endsWith(sep) ? normalizedRoot : `${normalizedRoot}${sep}`;

  if (filePath !== normalizedRoot && !filePath.startsWith(rootPrefix)) {
    return new Response("Forbidden", { status: 403 });
  }

  try {
    const file = await Deno.open(filePath, { read: true });
    return new Response(file.readable, {
      headers: {
        "content-type": resolveContentType(filePath),
        "cache-control": "no-store"
      }
    });
  } catch (error) {
    if (error instanceof Deno.errors.NotFound) {
      return new Response("Not Found", { status: 404 });
    }

    throw error;
  }
}

function resolveContentType(path: string): string {
  if (path.endsWith(".html")) {
    return "text/html; charset=utf-8";
  }
  if (path.endsWith(".js") || path.endsWith(".mjs")) {
    return "text/javascript; charset=utf-8";
  }
  if (path.endsWith(".css")) {
    return "text/css; charset=utf-8";
  }
  if (path.endsWith(".json") || path.endsWith(".map")) {
    return "application/json; charset=utf-8";
  }
  if (path.endsWith(".svg")) {
    return "image/svg+xml";
  }

  return "application/octet-stream";
}

async function startBrowser(): Promise<{ cdpPort: number; dispose: () => Promise<void> }> {
  const browserPath = await resolveBrowserExecutable();
  const cdpPort = Number(Deno.env.get("RAZORVUE_BROWSER_CDP_PORT") ?? await reservePort());
  const userDataRoot = await Deno.makeTempDir({ prefix: "razorvue-browser-profile-" });
  const process = new Deno.Command(browserPath, {
    args: [
      "--headless=new",
      "--disable-gpu",
      "--disable-dev-shm-usage",
      "--no-first-run",
      "--no-default-browser-check",
      "--no-sandbox",
      `--remote-debugging-port=${cdpPort}`,
      `--user-data-dir=${userDataRoot}`,
      "about:blank"
    ],
    stdin: "null",
    stdout: "null",
    stderr: "null"
  }).spawn();

  await waitForDebuggerEndpoint(cdpPort);

  return {
    cdpPort,
    dispose: async () => {
      process.kill("SIGTERM");
      await process.status.catch(() => {
      });
      await Deno.remove(userDataRoot, { recursive: true }).catch(() => {
      });
    }
  };
}

async function resolveBrowserExecutable(): Promise<string> {
  const explicitPath = Deno.env.get("RAZORVUE_BROWSER_PATH");
  if (explicitPath !== undefined && explicitPath.trim().length > 0) {
    return explicitPath.trim();
  }

  const candidates = [
    "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
    "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
    "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe",
    "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe"
  ];

  for (const candidate of candidates) {
    try {
      const stat = await Deno.stat(candidate);
      if (stat.isFile) {
        return candidate;
      }
    } catch {
    }
  }

  throw new Error("No Chromium-based browser executable was found. Set RAZORVUE_BROWSER_PATH.");
}

async function reservePort(): Promise<number> {
  const listener = Deno.listen({ hostname: "127.0.0.1", port: 0 });
  try {
    return (listener.addr as Deno.NetAddr).port;
  } finally {
    listener.close();
  }
}

async function waitForDebuggerEndpoint(port: number): Promise<void> {
  const deadline = Date.now() + 15000;
  const endpoint = `http://127.0.0.1:${port}/json/version`;
  while (Date.now() < deadline) {
    try {
      const response = await fetch(endpoint);
      if (response.ok) {
        return;
      }
    } catch {
    }

    await delay(100);
  }

  throw new Error(`Timed out waiting for browser debugger endpoint on port ${port}.`);
}

function delay(milliseconds: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

async function connectToPage(cdpPort: number): Promise<PageSession> {
  const response = await fetch(`http://127.0.0.1:${cdpPort}/json/new?about:blank`, { method: "PUT" });
  if (!response.ok) {
    throw new Error(`CDP target creation failed with HTTP ${response.status}.`);
  }

  const payload = await response.json() as { webSocketDebuggerUrl?: string };
  if (!payload.webSocketDebuggerUrl) {
    throw new Error("CDP target creation did not return a websocket URL.");
  }

  return await PageSession.create(payload.webSocketDebuggerUrl);
}

class PageSession {
  #socket: WebSocket;
  #nextId = 1;
  #pending = new Map<number, { resolve: (value: unknown) => void; reject: (error: unknown) => void }>();
  #consoleFailures: string[] = [];

  private constructor(socket: WebSocket) {
    this.#socket = socket;
    socket.onmessage = (event) => this.#handleMessage(event);
  }

  static async create(webSocketUrl: string): Promise<PageSession> {
    const socket = new WebSocket(webSocketUrl);
    await new Promise<void>((resolve, reject) => {
      socket.onopen = () => resolve();
      socket.onerror = () => reject(new Error("Failed to connect to browser CDP websocket."));
    });
    return new PageSession(socket);
  }

  async enable(): Promise<void> {
    await this.#send("Page.enable");
    await this.#send("Runtime.enable");
    await this.#send("Network.enable");
    await this.#send("Log.enable");
  }

  async navigate(url: string): Promise<void> {
    await this.#send("Page.navigate", { url });
  }

  async waitUntil(expression: string, description: string): Promise<void> {
    const deadline = Date.now() + 15000;
    while (Date.now() < deadline) {
      const result = await this.evaluateJson<boolean>(expression);
      if (result) {
        return;
      }

      await delay(100);
    }

    throw new Error(`Timed out waiting for ${description}.`);
  }

  async evaluateJson<T>(expression: string): Promise<T> {
    const result = await this.#send("Runtime.evaluate", {
      expression,
      returnByValue: true,
      awaitPromise: true
    }) as { result?: { value?: T } };

    return result.result?.value as T;
  }

  collectFailures(): string[] {
    return [...this.#consoleFailures];
  }

  async #send(method: string, params?: Record<string, unknown>): Promise<unknown> {
    const id = this.#nextId++;
    const payload: CdpMessage = { id, method, params };
    const promise = new Promise<unknown>((resolve, reject) => {
      this.#pending.set(id, { resolve, reject });
    });

    this.#socket.send(JSON.stringify(payload));
    return await promise;
  }

  #handleMessage(event: MessageEvent): void {
    const message = JSON.parse(String(event.data)) as CdpMessage;
    if (message.id !== undefined) {
      const pending = this.#pending.get(message.id);
      if (!pending) {
        return;
      }

      this.#pending.delete(message.id);
      if (message.error) {
        pending.reject(new Error(message.error.message ?? "CDP command failed."));
        return;
      }

      pending.resolve(message.result);
      return;
    }

    if (message.method === "Runtime.exceptionThrown") {
      const exception = message.params?.exceptionDetails as {
        text?: string;
        exception?: { description?: string; value?: string };
      } | undefined;
      const detail = exception?.exception?.description ?? exception?.exception?.value ?? exception?.text ?? "Runtime exception thrown.";
      this.#consoleFailures.push(detail);
      return;
    }

    if (message.method === "Runtime.consoleAPICalled") {
      const type = String(message.params?.type ?? "");
      if (type === "error" || type === "warning") {
        this.#consoleFailures.push(formatConsoleMessage(type, message.params));
      }
      return;
    }

    if (message.method === "Log.entryAdded") {
      const level = String((message.params?.entry as { level?: string } | undefined)?.level ?? "");
      if (level === "error" || level === "warning") {
        this.#consoleFailures.push(formatLogEntry(message.params));
      }
    }
  }
}

function formatConsoleMessage(type: string, params?: Record<string, unknown>): string {
  const args = params?.args as Array<{ value?: unknown; description?: string; type?: string }> | undefined;
  const text = args?.map((arg) => arg.description ?? (arg.value === undefined ? arg.type ?? "" : String(arg.value))).filter((part) => part.length > 0).join(" ") ?? "";
  return text.length > 0 ? `Console ${type}: ${text}` : `Console ${type} emitted.`;
}

function formatLogEntry(params?: Record<string, unknown>): string {
  const entry = params?.entry as { level?: string; text?: string } | undefined;
  const level = entry?.level ?? "log";
  const text = entry?.text ?? "";
  return text.length > 0 ? `Log ${level}: ${text}` : `Log ${level} entry emitted.`;
}

if (import.meta.main) {
  await runBrowserSmoke();
}
