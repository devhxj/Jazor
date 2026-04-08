declare module "node:fs/promises" {
  export function readFile(
    path: string,
    encoding: "utf8"
  ): Promise<string>;

  export function writeFile(
    path: string,
    data: string,
    encoding: "utf8"
  ): Promise<void>;
}

declare module "node:path" {
  export function dirname(path: string): string;
  export function resolve(...paths: string[]): string;
}

declare module "node:child_process" {
  type DataEvent = "data";
  type ExitEvent = "exit";
  type ErrorEvent = "error";

  export interface WritableStreamLike {
    write(
      chunk: string,
      callback?: (error?: Error | null) => void
    ): boolean;
    end(): void;
  }

  export interface ReadableStreamLike {
    setEncoding(encoding: "utf8"): void;
    on(event: DataEvent, listener: (chunk: string) => void): this;
  }

  export interface ChildProcessWithoutNullStreams {
    pid: number;
    killed: boolean;
    stdin: WritableStreamLike;
    stdout: ReadableStreamLike;
    stderr: ReadableStreamLike;
    kill(): void;
    once(event: ExitEvent, listener: (code: number | null, signal: string | null) => void): this;
    once(event: ErrorEvent, listener: (error: Error) => void): this;
  }

  export function spawn(
    command: string,
    args?: readonly string[],
    options?: {
      windowsHide?: boolean;
      stdio?: "pipe";
    }
  ): ChildProcessWithoutNullStreams;
}

declare module "node:readline" {
  export interface Interface {
    on(event: "line", listener: (line: string) => void): this;
    close(): void;
  }

  export function createInterface(options: {
    input: unknown;
    crlfDelay?: number;
  }): Interface;
}

declare module "vite" {
  export interface SourceMapLike {
    version: number;
    file: string;
    sources: string[];
    sourcesContent: string[];
    names: string[];
    mappings: string;
  }

  export interface ModuleNode {
    id?: string | null;
  }

  export interface ModuleGraph {
    getModuleById(id: string): ModuleNode | null;
    invalidateModule(module: ModuleNode): void;
  }

  export interface Watcher {
    on(event: "add" | "change" | "unlink", listener: (path: string) => void): this;
    off?(event: "add" | "change" | "unlink", listener: (path: string) => void): this;
  }

  export interface ViteDevServer {
    moduleGraph: ModuleGraph;
    watcher: Watcher;
  }

  export interface HmrContext {
    file: string;
    modules: ModuleNode[];
    server: ViteDevServer;
    read?(): Promise<string>;
  }

  export interface Plugin {
    name: string;
    enforce?: "pre" | "post";
    buildStart?: () => void | Promise<void>;
    buildEnd?: () => void | Promise<void>;
    closeBundle?: () => void | Promise<void>;
    configureServer?: (server: ViteDevServer) => void;
    resolveId?: (
      source: string,
      importer?: string
    ) => string | null | Promise<string | null>;
    load?: (
      id: string
    ) => string | { code: string; map: SourceMapLike | null } | null | Promise<string | { code: string; map: SourceMapLike | null } | null>;
    handleHotUpdate?: (
      context: HmrContext
    ) => ModuleNode[] | void | Promise<ModuleNode[] | void>;
  }
}
