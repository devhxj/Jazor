import { join, normalize, resolve, sep } from "node:path";
import { runBuild } from "./build.ts";
import { fileExists, prepareWorkspace } from "./lib/pipeline.ts";

type BrowserSmokeOptions = {
  expectedTexts: string[];
  clickButtonText: string | null;
  afterClickExpectedTexts: string[];
};

type CdpMessage = {
  id?: number;
  method?: string;
  params?: Record<string, unknown>;
  result?: unknown;
  error?: { message?: string };
};

const defaultExpectedTexts = [
  "RazorVue Todo Workspace",
  "Library mode emits Vue SFC artifacts during design time.",
  "3 tasks in scope",
  "1 completed",
  "2 still active",
  "Define per-component SFC topology",
  "Wire host requirements into consumer bootstrap",
  "Verify generated .vue imports stay stable",
  "Pinned"
];

const defaultAfterClickExpectedTexts = [
  "Added \"Document RazorVue SFC contract\" to the top of the workspace.",
  "4 tasks in scope",
  "3 still active",
  "Document RazorVue SFC contract"
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
      hasVuetifyApplication: boolean;
      stylesheetCount: number;
      scriptCount: number;
      bodyText: string;
    }>(`
      (function(){
        return {
          title: document.title || "",
          hasApp: !!document.querySelector("#app"),
          hasVuetifyApplication: !!document.querySelector(".v-application"),
          stylesheetCount: document.querySelectorAll('link[rel="stylesheet"]').length,
          scriptCount: document.querySelectorAll('script[type="module"]').length,
          bodyText: document.body ? (document.body.textContent || "") : ""
        };
      })()
    `);

    if (!initialState.hasApp) {
      throw new Error("RazorVue browser smoke did not find the #app mount element.");
    }
    if (!initialState.hasVuetifyApplication) {
      throw new Error("RazorVue browser smoke did not find the Vuetify .v-application root.");
    }
    if (initialState.stylesheetCount < 1) {
      throw new Error("RazorVue browser smoke did not load the generated CSS bundle.");
    }
    if (initialState.scriptCount < 1) {
      throw new Error("RazorVue browser smoke did not load the generated module entry.");
    }

    if (options.clickButtonText !== null) {
      const clicked = await page.evaluateJson<boolean>(`
        (function(){
          const button = Array.from(document.querySelectorAll("button"))
            .find((node) => (node.textContent || "").includes(${JSON.stringify(options.clickButtonText)}));
          if (!button) {
            return false;
          }

          button.click();
          return true;
        })()
      `);

      if (!clicked) {
        throw new Error(`RazorVue browser smoke could not find button text '${options.clickButtonText}'.`);
      }

      await page.waitUntil(
        createBodyContainsAllExpression(options.afterClickExpectedTexts),
        "RazorVue browser interaction state");
    }

    const browserFailures = page.collectFailures();
    if (browserFailures.length > 0) {
      throw new Error(["RazorVue browser smoke observed browser runtime failures.", ...browserFailures].join("\n"));
    }

    console.log("RazorVue browser smoke passed.");
  } finally {
    await browser.dispose();
    await server.dispose();
  }
}

function readBrowserSmokeOptions(): BrowserSmokeOptions {
  return {
    expectedTexts: readJsonTextArray("RAZORVUE_BROWSER_EXPECTED_TEXTS_JSON") ?? defaultExpectedTexts,
    clickButtonText: readOptionalText("RAZORVUE_BROWSER_CLICK_BUTTON_TEXT", "Add task"),
    afterClickExpectedTexts:
      readJsonTextArray("RAZORVUE_BROWSER_AFTER_CLICK_EXPECTED_TEXTS_JSON") ?? defaultAfterClickExpectedTexts
  };
}

function readOptionalText(name: string, defaultValue: string): string | null {
  const value = Deno.env.get(name);
  if (value === undefined) {
    return defaultValue;
  }

  const trimmed = value.trim();
  return trimmed.length === 0 ? null : trimmed;
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

  let exited = false;
  const statusPromise = process.status.then(() => {
    exited = true;
  });

  const deadline = Date.now() + 15000;
  while (Date.now() < deadline) {
    if (exited) {
      throw new Error(`Browser process exited before CDP became ready: ${browserPath}`);
    }

    try {
      const response = await fetch(`http://127.0.0.1:${cdpPort}/json/list`, { cache: "no-store" });
      if (response.ok) {
        return {
          cdpPort,
          dispose: async () => {
            try {
              process.kill("SIGKILL");
            } catch {
            }

            await statusPromise.catch(() => {
            });
            await Deno.remove(userDataRoot, { recursive: true }).catch(() => {
            });
          }
        };
      }
    } catch {
    }

    await delay(150);
  }

  try {
    process.kill("SIGKILL");
  } catch {
  }

  await Deno.remove(userDataRoot, { recursive: true }).catch(() => {
  });
  throw new Error(`Timed out waiting for browser CDP endpoint on port ${cdpPort}.`);
}

async function resolveBrowserExecutable(): Promise<string> {
  const explicit = Deno.env.get("RAZORVUE_BROWSER_EXE")?.trim();
  if (explicit !== undefined && explicit.length > 0) {
    if (!(await fileExists(explicit))) {
      throw new Error(`RAZORVUE_BROWSER_EXE does not exist: ${explicit}`);
    }

    return explicit;
  }

  const candidates = Deno.build.os === "windows"
    ? [
      "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
      "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe",
      "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
      "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
      "msedge.exe",
      "chrome.exe"
    ]
    : Deno.build.os === "darwin"
      ? [
        "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
        "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
        "microsoft-edge",
        "google-chrome",
        "chromium"
      ]
      : [
        "microsoft-edge",
        "microsoft-edge-stable",
        "google-chrome",
        "google-chrome-stable",
        "chromium",
        "chromium-browser"
      ];

  for (const candidate of candidates) {
    const resolved = await tryResolveExecutable(candidate);
    if (resolved !== null) {
      return resolved;
    }
  }

  throw new Error("RazorVue browser smoke requires Microsoft Edge, Chrome, or Chromium. Set RAZORVUE_BROWSER_EXE to the browser executable path.");
}

async function tryResolveExecutable(candidate: string): Promise<string | null> {
  if (candidate.includes("\\") || candidate.includes("/") || candidate.includes(":")) {
    return await fileExists(candidate) ? candidate : null;
  }

  const path = Deno.env.get("PATH") ?? "";
  const extensions = Deno.build.os === "windows"
    ? (Deno.env.get("PATHEXT") ?? ".EXE;.CMD;.BAT").split(";")
    : [""];

  for (const directory of path.split(Deno.build.os === "windows" ? ";" : ":")) {
    if (directory.trim().length === 0) {
      continue;
    }

    for (const extension of extensions) {
      const filePath = join(directory, candidate.endsWith(extension.toLowerCase()) ? candidate : `${candidate}${extension}`);
      if (await fileExists(filePath)) {
        return filePath;
      }
    }
  }

  return null;
}

async function reservePort(): Promise<number> {
  const listener = Deno.listen({ hostname: "127.0.0.1", port: 0 });
  const port = (listener.addr as Deno.NetAddr).port;
  listener.close();
  return port;
}

async function connectToPage(cdpPort: number): Promise<CdpPage> {
  const targets = await fetch(`http://127.0.0.1:${cdpPort}/json/list`, { cache: "no-store" })
    .then((response) => response.json()) as Array<{ type?: string; url?: string; webSocketDebuggerUrl?: string }>;
  const pageTarget =
    targets.find((target) => target.type === "page" && target.url === "about:blank") ??
    targets.find((target) => target.type === "page");

  if (pageTarget?.webSocketDebuggerUrl === undefined) {
    throw new Error("Browser CDP did not expose a page target.");
  }

  const socket = new WebSocket(pageTarget.webSocketDebuggerUrl);
  await new Promise<void>((resolvePromise, reject) => {
    socket.addEventListener("open", () => resolvePromise(), { once: true });
    socket.addEventListener("error", () => reject(new Error("Browser CDP websocket failed to open.")), { once: true });
  });

  return new CdpPage(socket);
}

class CdpPage {
  private nextId = 1;
  private readonly pending = new Map<number, {
    resolve: (value: unknown) => void;
    reject: (error: Error) => void;
  }>();
  private readonly loadResolvers: Array<() => void> = [];
  private readonly consoleFailures: string[] = [];
  private readonly exceptions: string[] = [];
  private readonly requestUrls = new Map<string, string>();
  private readonly networkFailures: string[] = [];

  public constructor(private readonly socket: WebSocket) {
    socket.addEventListener("message", (event) => this.handleMessage(JSON.parse(String(event.data)) as CdpMessage));
  }

  public async enable(): Promise<void> {
    await this.send("Page.enable");
    await this.send("Runtime.enable");
    await this.send("Network.enable");
    await this.send("Network.setCacheDisabled", { cacheDisabled: true });
  }

  public async navigate(url: string): Promise<void> {
    const loaded = this.waitForLoad();
    await this.send("Page.navigate", { url });
    await loaded;
    await delay(500);
  }

  public async waitUntil(expression: string, description: string, timeoutMs = 10000): Promise<void> {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      if (await this.evaluateJson<boolean>(expression)) {
        return;
      }

      await delay(100);
    }

    let bodyText = "";
    try {
      bodyText = await this.evaluateJson<string>("document.body ? (document.body.textContent || '') : ''");
    } catch {
    }

    const failures = this.collectFailures();
    throw new Error(
      [
        `Timed out waiting for ${description}.`,
        bodyText.length === 0 ? "Current body text: <empty>" : `Current body text: ${bodyText.slice(0, 1200)}`,
        ...failures
      ].join("\n"));
  }

  public async evaluateJson<T>(expression: string): Promise<T> {
    const result = await this.send("Runtime.evaluate", {
      expression,
      returnByValue: true,
      awaitPromise: true
    }) as {
      result?: { value?: unknown };
      exceptionDetails?: { text?: string; exception?: { description?: string } };
    };

    if (result.exceptionDetails !== undefined) {
      throw new Error(result.exceptionDetails.exception?.description ?? result.exceptionDetails.text ?? "Runtime.evaluate failed.");
    }

    return result.result?.value as T;
  }

  public collectFailures(): string[] {
    return [
      ...this.consoleFailures.map((entry) => `Console: ${entry}`),
      ...this.exceptions.map((entry) => `Exception: ${entry}`),
      ...this.networkFailures.map((entry) => `Network: ${entry}`)
    ];
  }

  private waitForLoad(timeoutMs = 15000): Promise<void> {
    return new Promise((resolvePromise, reject) => {
      const timer = setTimeout(() => reject(new Error("Timed out waiting for browser page load.")), timeoutMs);
      this.loadResolvers.push(() => {
        clearTimeout(timer);
        resolvePromise();
      });
    });
  }

  private send(method: string, params: Record<string, unknown> = {}): Promise<unknown> {
    const id = this.nextId++;
    const promise = new Promise<unknown>((resolvePromise, reject) => {
      this.pending.set(id, { resolve: resolvePromise, reject });
    });
    this.socket.send(JSON.stringify({ id, method, params }));
    return promise;
  }

  private handleMessage(message: CdpMessage): void {
    if (message.id !== undefined) {
      const pending = this.pending.get(message.id);
      if (pending === undefined) {
        return;
      }

      this.pending.delete(message.id);
      if (message.error !== undefined) {
        pending.reject(new Error(message.error.message ?? JSON.stringify(message.error)));
      } else {
        pending.resolve(message.result);
      }
      return;
    }

    if (message.method === "Page.loadEventFired") {
      const resolvers = this.loadResolvers.splice(0);
      for (const resolvePromise of resolvers) {
        resolvePromise();
      }
      return;
    }

    if (message.method === "Runtime.consoleAPICalled") {
      const params = message.params as { type?: string; args?: Array<Record<string, unknown>> } | undefined;
      if (params?.type === "error" || params?.type === "warning" || params?.type === "assert") {
        this.consoleFailures.push((params.args ?? []).map(formatRemoteArg).join(" "));
      }
      return;
    }

    if (message.method === "Runtime.exceptionThrown") {
      const params = message.params as {
        exceptionDetails?: { text?: string; exception?: { description?: string } };
      } | undefined;
      this.exceptions.push(params?.exceptionDetails?.exception?.description ?? params?.exceptionDetails?.text ?? "Unknown runtime exception");
      return;
    }

    if (message.method === "Network.requestWillBeSent") {
      const params = message.params as { requestId?: string; request?: { url?: string } } | undefined;
      if (params?.requestId !== undefined && params.request?.url !== undefined) {
        this.requestUrls.set(params.requestId, params.request.url);
      }
      return;
    }

    if (message.method === "Network.loadingFailed") {
      const params = message.params as {
        requestId?: string;
        errorText?: string;
        canceled?: boolean;
        type?: string;
      } | undefined;
      if (params?.canceled === true || params?.errorText === "net::ERR_ABORTED") {
        return;
      }

      const url = params?.requestId === undefined ? "" : this.requestUrls.get(params.requestId) ?? "";
      this.networkFailures.push(`${params?.type ?? "unknown"}:${params?.errorText ?? "unknown"}:${url}`);
    }
  }
}

function formatRemoteArg(arg: Record<string, unknown>): string {
  if ("value" in arg) {
    return String(arg.value);
  }
  if (typeof arg.description === "string") {
    return arg.description;
  }

  return typeof arg.type === "string" ? arg.type : "unknown";
}

function delay(ms: number): Promise<void> {
  return new Promise((resolvePromise) => setTimeout(resolvePromise, ms));
}

if (import.meta.main) {
  if (Deno.env.get("RAZORVUE_BROWSER_SKIP_BUILD") !== "1") {
    await runBuild();
  }

  await runBrowserSmoke();
}
