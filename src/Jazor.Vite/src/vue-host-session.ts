import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { createInterface, type Interface as ReadLineInterface } from "node:readline";
import type {
  DiagnosticRecord,
  SourceMapDescriptor,
  GetHotUpdatePlanRequest,
  GetHotUpdatePlanResponse
} from "./contracts";

export interface RpcRequestEnvelope {
  id: string;
  method: string;
  payloadJson: string | null;
}

export interface RpcResponseEnvelope {
  id?: string | null;
  success: boolean;
  payloadJson?: string | null;
  error?: {
    code: string;
    message: string;
    details?: string | null;
  } | null;
}

export interface HostCapabilityDescriptor {
  name: string;
  description?: string | null;
}

export interface GetHostInfoResponse {
  hostName: string;
  protocolVersion: string;
  capabilities: HostCapabilityDescriptor[];
}

export interface DocumentSnapshot {
  documentPath: string;
  documentKind: "Jazor" | "Vue" | "JavaScript" | "TypeScript" | "Unknown";
  text: string;
  version: string | null;
}

export interface ArtifactRecord {
  artifactName: string;
  artifactKind: string;
  content: string;
  contentHash: string | null;
}

export interface GetVirtualArtifactRequest {
  documentPath: string;
  artifactKind: string;
  text: string | null;
  version: string | null;
}

export interface GetVirtualArtifactResponse {
  artifact: ArtifactRecord;
  diagnostics: DiagnosticRecord[];
  sourceMaps: SourceMapDescriptor[];
}

export interface VueHostBootstrapOptions {
  command?: string;
  args?: string;
  argsList?: string[];
  rpcMode?: string;
}

type PendingRequest = {
  resolve: (payload: string) => void;
  reject: (error: Error) => void;
};

const JSON_LINE_PREFIX = "{";

export function splitCommandLine(commandLine: string): string[] {
  const segments: string[] = [];
  let current = "";
  let inQuotes = false;

  for (let index = 0; index < commandLine.length; index += 1) {
    const character = commandLine[index];

    if (character === "\"") {
      inQuotes = !inQuotes;
      continue;
    }

    if (!inQuotes && /\s/.test(character)) {
      if (current.length > 0) {
        segments.push(current);
        current = "";
      }

      continue;
    }

    current += character;
  }

  if (current.length > 0) {
    segments.push(current);
  }

  return segments;
}

export class PersistentVueHostSession {
  private readonly bootstrap: Required<VueHostBootstrapOptions>;
  private readonly pending = new Map<string, PendingRequest>();
  private readonly trackedDocuments = new Map<string, DocumentSnapshot>();
  private readonly stderrLines: string[] = [];
  private child: ChildProcessWithoutNullStreams | null = null;
  private childExitPromise: Promise<void> | null = null;
  private lineReader: ReadLineInterface | null = null;
  private nextRequestId = 1;
  private startPromise: Promise<void> | null = null;
  private disposePromise: Promise<void> | null = null;
  private activeProcessId: number | null = null;

  public constructor(bootstrap: Required<VueHostBootstrapOptions>) {
    this.bootstrap = {
      command: bootstrap.command,
      args: bootstrap.args,
      argsList: bootstrap.argsList ?? [],
      rpcMode: bootstrap.rpcMode
    };
  }

  public get processId(): number | null {
    return this.child?.pid ?? null;
  }

  public async ensureStarted(): Promise<void> {
    if (this.disposePromise) {
      throw new Error("PersistentVueHostSession has already been disposed.");
    }

    if (this.child && !this.child.killed) {
      return;
    }

    if (!this.startPromise) {
      this.startPromise = this.startCore();
    }

    await this.startPromise;
    const processId = this.processId;
    if (processId !== null && processId !== this.activeProcessId) {
      this.activeProcessId = processId;
      await this.replayTrackedDocuments();
    }
  }

  public async dispose(): Promise<void> {
    if (this.disposePromise) {
      await this.disposePromise;
      return;
    }

    this.disposePromise = this.disposeCore();
    await this.disposePromise;
  }

  public async ping(): Promise<string> {
    const payload = await this.sendRaw("vuehost/ping", null);
    const response = JSON.parse(payload) as { message: string };
    return response.message;
  }

  public async getHostInfo(): Promise<GetHostInfoResponse> {
    const payload = await this.sendRaw("vuehost/getHostInfo", null);
    return JSON.parse(payload) as GetHostInfoResponse;
  }

  public async openDocument(document: DocumentSnapshot): Promise<void> {
    const normalizedDocument = normalizeDocumentSnapshot(document);
    this.trackedDocuments.set(normalizedDocument.documentPath, normalizedDocument);
    await this.sendRaw("vuehost/openDocument", normalizedDocument, allowEmptyPayload);
  }

  public async updateDocument(document: DocumentSnapshot): Promise<void> {
    const normalizedDocument = normalizeDocumentSnapshot(document);
    this.trackedDocuments.set(normalizedDocument.documentPath, normalizedDocument);
    await this.sendRaw("vuehost/updateDocument", normalizedDocument, allowEmptyPayload);
  }

  public async closeDocument(documentPath: string): Promise<void> {
    const normalizedDocumentPath = normalizeDocumentPath(documentPath);
    this.trackedDocuments.delete(normalizedDocumentPath);
    await this.sendRaw("vuehost/closeDocument", normalizedDocumentPath, allowEmptyPayload);
  }

  public async getOpenDocuments(): Promise<DocumentSnapshot[]> {
    const payload = await this.sendRaw("vuehost/getOpenDocuments", null);
    return JSON.parse(payload) as DocumentSnapshot[];
  }

  public async getVirtualArtifact(
    request: GetVirtualArtifactRequest
  ): Promise<GetVirtualArtifactResponse> {
    const payload = await this.sendRaw("vuehost/getVirtualArtifact", {
      ...request,
      documentPath: normalizeDocumentPath(request.documentPath)
    });
    return JSON.parse(payload) as GetVirtualArtifactResponse;
  }

  public async getHotUpdatePlan(
    request: GetHotUpdatePlanRequest
  ): Promise<GetHotUpdatePlanResponse> {
    const payload = await this.sendRaw("vuehost/getHotUpdatePlan", {
      ...request,
      documentPath: normalizeDocumentPath(request.documentPath)
    });
    return JSON.parse(payload) as GetHotUpdatePlanResponse;
  }

  private async startCore(): Promise<void> {
    if (!this.bootstrap.command) {
      throw new Error("Jazor.VueHost command is required.");
    }

    if (this.bootstrap.rpcMode !== "process-stdio") {
      throw new Error(`Unsupported VueHost RPC mode '${this.bootstrap.rpcMode}'.`);
    }

    const args = this.bootstrap.argsList.length > 0
      ? this.bootstrap.argsList
      : splitCommandLine(this.bootstrap.args);
    const child = spawn(
      this.bootstrap.command,
      args,
      {
        windowsHide: true
      }
    );
    this.childExitPromise = new Promise<void>((resolve) => {
      child.once("exit", () => {
        resolve();
      });
    });
    child.stdout.setEncoding("utf8");
    child.stderr.setEncoding("utf8");
    child.stderr.on("data", (chunk: unknown) => {
      const text = typeof chunk === "string" ? chunk : String(chunk);
      for (const line of text.split(/\r?\n/)) {
        const trimmed = line.trim();
        if (!trimmed) {
          continue;
        }

        this.stderrLines.push(trimmed);
        if (this.stderrLines.length > 50) {
          this.stderrLines.shift();
        }
      }
    });
    child.once("error", (error: Error) => {
      this.stderrLines.push(error.message);
      this.rejectAllPending(new Error(`Jazor.VueHost session failed to start. ${error.message}`));
      this.detachChild();
    });

    child.once("exit", () => {
      const error = new Error(
        this.stderrLines.length > 0
          ? `Jazor.VueHost session exited. ${this.stderrLines.join(" | ")}`
          : "Jazor.VueHost session exited unexpectedly."
      );
      this.rejectAllPending(error);
      this.detachChild();
    });

    const lineReader = createInterface({
      input: child.stdout,
      crlfDelay: Number.POSITIVE_INFINITY
    });
    lineReader.on("line", (line: string) => {
      this.handleOutputLine(line);
    });

    this.child = child;
    this.lineReader = lineReader;
  }

  private async disposeCore(): Promise<void> {
    this.rejectAllPending(new Error("Jazor.VueHost session disposed."));
    this.lineReader?.close();

    const child = this.child;
    const childExitPromise = this.childExitPromise;
    this.detachChild();

    if (!child) {
      return;
    }

    try {
      child.stdin.end();
    } catch {
      // Ignore teardown failures on an already-exiting process.
    }

    if (!child.killed) {
      child.kill();
    }

    if (childExitPromise) {
      await Promise.race([childExitPromise, delay(250)]);
    }
  }

  private detachChild(): void {
    this.startPromise = null;
    this.child = null;
    this.childExitPromise = null;
    this.lineReader = null;
    this.activeProcessId = null;
  }

  private handleOutputLine(line: string): void {
    const trimmed = line.trim();
    if (!trimmed.startsWith(JSON_LINE_PREFIX)) {
      return;
    }

    let response: RpcResponseEnvelope;
    try {
      response = JSON.parse(trimmed) as RpcResponseEnvelope;
    } catch {
      return;
    }

    const responseId = response.id ?? "";
    const pending = this.pending.get(responseId);
    if (!pending) {
      return;
    }

    this.pending.delete(responseId);
    if (!response.success) {
      const code = response.error?.code ?? "vuehost_error";
      const message = response.error?.message ?? "Jazor.VueHost RPC call failed.";
      pending.reject(new Error(`${code}: ${message}`));
      return;
    }

    pending.resolve(response.payloadJson ?? "");
  }

  private rejectAllPending(error: Error): void {
    for (const pending of this.pending.values()) {
      pending.reject(error);
    }

    this.pending.clear();
  }

  private async sendRaw<TPayload>(
    method: string,
    payload: TPayload,
    emptyPayloadHandler?: (payload: string) => void
  ): Promise<string> {
    await this.ensureStarted();
    return await this.sendRawCore(method, payload, emptyPayloadHandler);
  }

  private async sendRawCore<TPayload>(
    method: string,
    payload: TPayload,
    emptyPayloadHandler?: (payload: string) => void
  ): Promise<string> {
    const child = this.child;
    if (!child) {
      throw new Error("Jazor.VueHost session is not available.");
    }

    const requestId = `jazor-vite-${this.nextRequestId++}`;
    const requestEnvelope: RpcRequestEnvelope = {
      id: requestId,
      method,
      payloadJson: payload === null ? null : JSON.stringify(payload)
    };
    const requestLine = `${JSON.stringify(requestEnvelope)}\n`;

    const responsePromise = new Promise<string>((resolve, reject) => {
      this.pending.set(requestId, { resolve, reject });
    });

    await writeLine(child, requestLine);
    const responsePayload = await responsePromise;
    if (responsePayload.length === 0 && emptyPayloadHandler) {
      emptyPayloadHandler(responsePayload);
    }

    return responsePayload;
  }

  private async replayTrackedDocuments(): Promise<void> {
    if (this.trackedDocuments.size === 0) {
      return;
    }

    for (const document of this.trackedDocuments.values()) {
      await this.sendRawCore("vuehost/openDocument", document, allowEmptyPayload);
    }
  }
}

function allowEmptyPayload(_: string): void {
  // Some VueHost RPC methods intentionally return no payload.
}

async function writeLine(
  child: ChildProcessWithoutNullStreams,
  requestLine: string
): Promise<void> {
  await new Promise<void>((resolve, reject) => {
    child.stdin.write(requestLine, (error?: Error | null) => {
      if (error) {
        reject(error);
        return;
      }

      resolve();
    });
  });
}

export function createPersistentVueHostSession(
  bootstrap: Required<VueHostBootstrapOptions>
): PersistentVueHostSession {
  return new PersistentVueHostSession(bootstrap);
}

async function delay(timeoutMs: number): Promise<void> {
  await new Promise<void>((resolve) => {
    setTimeout(resolve, timeoutMs);
  });
}

function normalizeDocumentPath(documentPath: string): string {
  return documentPath.replace(/\\/g, "/");
}

function normalizeDocumentSnapshot(document: DocumentSnapshot): DocumentSnapshot {
  return {
    ...document,
    documentPath: normalizeDocumentPath(document.documentPath)
  };
}
