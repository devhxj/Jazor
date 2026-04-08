import {
  type AnalyzeJazorRequest,
  type AnalyzeJazorResponse,
  type RpcRequestEnvelope,
  type RpcResponseEnvelope
} from "./contracts";

export interface VueHostProcessOptions {
  command: string;
  arguments?: string[];
}

export interface VueHostTransport {
  analyzeJazor(request: AnalyzeJazorRequest): Promise<AnalyzeJazorResponse>;
}

export class BunVueHostTransport implements VueHostTransport {
  private readonly options: VueHostProcessOptions;

  public constructor(options: VueHostProcessOptions) {
    this.options = options;
  }

  public async analyzeJazor(request: AnalyzeJazorRequest): Promise<AnalyzeJazorResponse> {
    return this.invoke<AnalyzeJazorResponse>("vuehost/analyzeJazor", request);
  }

  private async invoke<TResponse>(method: string, payload: unknown): Promise<TResponse> {
    const bunApi = getBun();
    const request: RpcRequestEnvelope = {
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

    const response = JSON.parse(responseLine) as RpcResponseEnvelope;
    if (!response.success) {
      const code = response.error?.code ?? "rpc_error";
      const message = response.error?.message ?? "Unknown RPC failure.";
      throw new Error(`${code}: ${message}`);
    }

    if (response.payloadJson === null) {
      throw new Error(`VueHost returned an empty payload for '${method}'.`);
    }

    return JSON.parse(response.payloadJson) as TResponse;
  }
}

function getBun(): NonNullable<typeof Bun> {
  if (typeof Bun === "undefined") {
    throw new Error("Jazor.Vite currently requires Bun runtime for stdio host transport.");
  }

  return Bun;
}

async function writeLine(stream: WritableStream<Uint8Array>, line: string): Promise<void> {
  const writer = stream.getWriter();
  try {
    await writer.write(new TextEncoder().encode(`${line}\n`));
    await writer.close();
  } finally {
    writer.releaseLock();
  }
}

async function readFirstLine(stream: ReadableStream<Uint8Array>): Promise<string | null> {
  const text = await new Response(stream).text();
  const firstLine = text.split(/\r?\n/, 1)[0];
  return firstLine.length === 0 ? null : firstLine;
}
