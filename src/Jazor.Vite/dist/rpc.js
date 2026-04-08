export class BunVueHostTransport {
    options;
    constructor(options) {
        this.options = options;
    }
    async analyzeJazor(request) {
        return this.invoke("vuehost/analyzeJazor", request);
    }
    async invoke(method, payload) {
        const bunApi = getBun();
        const request = {
            id: `vite-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`,
            method,
            payloadJson: JSON.stringify(payload)
        };
        const command = [this.options.command, ...(this.options.arguments ?? [])];
        const subprocess = bunApi.spawn(command, {
            stdin: "pipe",
            stdout: "pipe",
            stderr: "pipe"
        });
        await writeLine(subprocess.stdin, JSON.stringify(request));
        const responseLine = await readFirstLine(subprocess.stdout);
        const errorText = await new Response(subprocess.stderr).text();
        await subprocess.exited;
        if (!responseLine) {
            throw new Error(errorText.trim() || `VueHost did not return a response for '${method}'.`);
        }
        const response = JSON.parse(responseLine);
        if (!response.success) {
            const code = response.error?.code ?? "rpc_error";
            const message = response.error?.message ?? "Unknown RPC failure.";
            throw new Error(`${code}: ${message}`);
        }
        if (response.payloadJson === null) {
            throw new Error(`VueHost returned an empty payload for '${method}'.`);
        }
        return JSON.parse(response.payloadJson);
    }
}
function getBun() {
    if (typeof Bun === "undefined") {
        throw new Error("Jazor.Vite currently requires Bun runtime for stdio host transport.");
    }
    return Bun;
}
async function writeLine(stream, line) {
    const writer = stream.getWriter();
    try {
        await writer.write(new TextEncoder().encode(`${line}\n`));
        await writer.close();
    }
    finally {
        writer.releaseLock();
    }
}
async function readFirstLine(stream) {
    const text = await new Response(stream).text();
    const firstLine = text.split(/\r?\n/, 1)[0];
    return firstLine.length === 0 ? null : firstLine;
}
