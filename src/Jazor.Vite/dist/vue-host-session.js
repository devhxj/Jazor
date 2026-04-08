import { spawn } from "node:child_process";
import { createInterface } from "node:readline";
const JSON_LINE_PREFIX = "{";
export function splitCommandLine(commandLine) {
    const segments = [];
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
    bootstrap;
    pending = new Map();
    trackedDocuments = new Map();
    stderrLines = [];
    child = null;
    childExitPromise = null;
    lineReader = null;
    nextRequestId = 1;
    startPromise = null;
    disposePromise = null;
    activeProcessId = null;
    constructor(bootstrap) {
        this.bootstrap = {
            command: bootstrap.command,
            args: bootstrap.args,
            argsList: bootstrap.argsList ?? [],
            rpcMode: bootstrap.rpcMode
        };
    }
    get processId() {
        return this.child?.pid ?? null;
    }
    async ensureStarted() {
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
    async dispose() {
        if (this.disposePromise) {
            await this.disposePromise;
            return;
        }
        this.disposePromise = this.disposeCore();
        await this.disposePromise;
    }
    async ping() {
        const payload = await this.sendRaw("vuehost/ping", null);
        const response = JSON.parse(payload);
        return response.message;
    }
    async getHostInfo() {
        const payload = await this.sendRaw("vuehost/getHostInfo", null);
        return JSON.parse(payload);
    }
    async openDocument(document) {
        const normalizedDocument = normalizeDocumentSnapshot(document);
        this.trackedDocuments.set(normalizedDocument.documentPath, normalizedDocument);
        await this.sendRaw("vuehost/openDocument", normalizedDocument, allowEmptyPayload);
    }
    async updateDocument(document) {
        const normalizedDocument = normalizeDocumentSnapshot(document);
        this.trackedDocuments.set(normalizedDocument.documentPath, normalizedDocument);
        await this.sendRaw("vuehost/updateDocument", normalizedDocument, allowEmptyPayload);
    }
    async closeDocument(documentPath) {
        const normalizedDocumentPath = normalizeDocumentPath(documentPath);
        this.trackedDocuments.delete(normalizedDocumentPath);
        await this.sendRaw("vuehost/closeDocument", normalizedDocumentPath, allowEmptyPayload);
    }
    async getOpenDocuments() {
        const payload = await this.sendRaw("vuehost/getOpenDocuments", null);
        return JSON.parse(payload);
    }
    async getVirtualArtifact(request) {
        const payload = await this.sendRaw("vuehost/getVirtualArtifact", {
            ...request,
            documentPath: normalizeDocumentPath(request.documentPath)
        });
        return JSON.parse(payload);
    }
    async getHotUpdatePlan(request) {
        const payload = await this.sendRaw("vuehost/getHotUpdatePlan", {
            ...request,
            documentPath: normalizeDocumentPath(request.documentPath)
        });
        return JSON.parse(payload);
    }
    async startCore() {
        if (!this.bootstrap.command) {
            throw new Error("Jazor.VueHost command is required.");
        }
        if (this.bootstrap.rpcMode !== "process-stdio") {
            throw new Error(`Unsupported VueHost RPC mode '${this.bootstrap.rpcMode}'.`);
        }
        const args = this.bootstrap.argsList.length > 0
            ? this.bootstrap.argsList
            : splitCommandLine(this.bootstrap.args);
        const child = spawn(this.bootstrap.command, args, {
            windowsHide: true
        });
        this.childExitPromise = new Promise((resolve) => {
            child.once("exit", () => {
                resolve();
            });
        });
        child.stdout.setEncoding("utf8");
        child.stderr.setEncoding("utf8");
        child.stderr.on("data", (chunk) => {
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
        child.once("error", (error) => {
            this.stderrLines.push(error.message);
            this.rejectAllPending(new Error(`Jazor.VueHost session failed to start. ${error.message}`));
            this.detachChild();
        });
        child.once("exit", () => {
            const error = new Error(this.stderrLines.length > 0
                ? `Jazor.VueHost session exited. ${this.stderrLines.join(" | ")}`
                : "Jazor.VueHost session exited unexpectedly.");
            this.rejectAllPending(error);
            this.detachChild();
        });
        const lineReader = createInterface({
            input: child.stdout,
            crlfDelay: Number.POSITIVE_INFINITY
        });
        lineReader.on("line", (line) => {
            this.handleOutputLine(line);
        });
        this.child = child;
        this.lineReader = lineReader;
    }
    async disposeCore() {
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
        }
        catch {
            // Ignore teardown failures on an already-exiting process.
        }
        if (!child.killed) {
            child.kill();
        }
        if (childExitPromise) {
            await Promise.race([childExitPromise, delay(250)]);
        }
    }
    detachChild() {
        this.startPromise = null;
        this.child = null;
        this.childExitPromise = null;
        this.lineReader = null;
        this.activeProcessId = null;
    }
    handleOutputLine(line) {
        const trimmed = line.trim();
        if (!trimmed.startsWith(JSON_LINE_PREFIX)) {
            return;
        }
        let response;
        try {
            response = JSON.parse(trimmed);
        }
        catch {
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
    rejectAllPending(error) {
        for (const pending of this.pending.values()) {
            pending.reject(error);
        }
        this.pending.clear();
    }
    async sendRaw(method, payload, emptyPayloadHandler) {
        await this.ensureStarted();
        return await this.sendRawCore(method, payload, emptyPayloadHandler);
    }
    async sendRawCore(method, payload, emptyPayloadHandler) {
        const child = this.child;
        if (!child) {
            throw new Error("Jazor.VueHost session is not available.");
        }
        const requestId = `jazor-vite-${this.nextRequestId++}`;
        const requestEnvelope = {
            id: requestId,
            method,
            payloadJson: payload === null ? null : JSON.stringify(payload)
        };
        const requestLine = `${JSON.stringify(requestEnvelope)}\n`;
        const responsePromise = new Promise((resolve, reject) => {
            this.pending.set(requestId, { resolve, reject });
        });
        await writeLine(child, requestLine);
        const responsePayload = await responsePromise;
        if (responsePayload.length === 0 && emptyPayloadHandler) {
            emptyPayloadHandler(responsePayload);
        }
        return responsePayload;
    }
    async replayTrackedDocuments() {
        if (this.trackedDocuments.size === 0) {
            return;
        }
        for (const document of this.trackedDocuments.values()) {
            await this.sendRawCore("vuehost/openDocument", document, allowEmptyPayload);
        }
    }
}
function allowEmptyPayload(_) {
    // Some VueHost RPC methods intentionally return no payload.
}
async function writeLine(child, requestLine) {
    await new Promise((resolve, reject) => {
        child.stdin.write(requestLine, (error) => {
            if (error) {
                reject(error);
                return;
            }
            resolve();
        });
    });
}
export function createPersistentVueHostSession(bootstrap) {
    return new PersistentVueHostSession(bootstrap);
}
async function delay(timeoutMs) {
    await new Promise((resolve) => {
        setTimeout(resolve, timeoutMs);
    });
}
function normalizeDocumentPath(documentPath) {
    return documentPath.replace(/\\/g, "/");
}
function normalizeDocumentSnapshot(document) {
    return {
        ...document,
        documentPath: normalizeDocumentPath(document.documentPath)
    };
}
