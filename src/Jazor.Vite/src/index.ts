import { readFile } from "node:fs/promises";
import type { ModuleNode, Plugin, ViteDevServer, HmrContext } from "vite";
import type { SourceMapDescriptor, GetVirtualArtifactResponse } from "./contracts";
import {
  createPersistentVueHostSession,
  type DocumentSnapshot,
  type GetVirtualArtifactRequest,
  type PersistentVueHostSession,
  type VueHostBootstrapOptions
} from "./vue-host-session";
import { dirname, resolve } from "node:path";

const JAZOR_PREFIX = "\0jazor:";
const VUE_ARTIFACT_KIND = "vue-sfc";

type MinimalPlugin = Pick<
  Plugin,
  | "name"
  | "enforce"
  | "buildStart"
  | "buildEnd"
  | "closeBundle"
  | "configureServer"
  | "resolveId"
  | "load"
  | "handleHotUpdate"
>;

export interface JazorVitePluginOptions {
  vueHost?: VueHostBootstrapOptions;
}

interface TrackedJazorDocumentState {
  kind: DocumentSnapshot["documentKind"];
  text: string;
  version: number;
}

export interface SourceMapLike {
  version: number;
  file: string;
  sources: string[];
  sourcesContent: string[];
  names: string[];
  mappings: string;
}

interface ProcessEnvironmentLike {
  JAZOR_VUEHOST_COMMAND?: string;
  JAZOR_VUEHOST_ARGS?: string;
  JAZOR_VUEHOST_ARGS_JSON?: string;
  JAZOR_VUEHOST_RPC_MODE?: string;
  [key: string]: string | undefined;
}

function parseBootstrapArgsJson(
  env: ProcessEnvironmentLike
): string[] | undefined {
  const value = env.JAZOR_VUEHOST_ARGS_JSON;
  if (!value) {
    return undefined;
  }

  try {
    const parsed = JSON.parse(value) as unknown;
    if (!Array.isArray(parsed) || parsed.some((segment) => typeof segment !== "string")) {
      return undefined;
    }

    return parsed;
  } catch {
    return undefined;
  }
}

export function resolveVueHostBootstrap(
  options: VueHostBootstrapOptions | undefined = undefined
): Required<VueHostBootstrapOptions> {
  const processRef = (globalThis as { process?: { env?: ProcessEnvironmentLike } }).process;
  const env = processRef?.env ?? {};
  const envArgsList = parseBootstrapArgsJson(env);
  const argsList = options?.argsList ?? envArgsList ?? [];

  return {
    command: options?.command ?? env.JAZOR_VUEHOST_COMMAND ?? "",
    args: options?.args ?? env.JAZOR_VUEHOST_ARGS ?? "",
    argsList,
    rpcMode: options?.rpcMode ?? env.JAZOR_VUEHOST_RPC_MODE ?? "process-stdio"
  };
}

export function buildGetVirtualArtifactRequest(
  documentPath: string
): GetVirtualArtifactRequest {
  return {
    documentPath,
    artifactKind: VUE_ARTIFACT_KIND,
    text: null,
    version: null
  };
}

export function normalizeImporterPath(importer: string | undefined): string | undefined {
  if (!importer) {
    return undefined;
  }

  return normalizeWorkspacePath(
    importer.startsWith(JAZOR_PREFIX)
    ? importer.slice(JAZOR_PREFIX.length)
    : importer
  );
}

export function normalizeWorkspacePath(documentPath: string): string {
  return resolve(documentPath).replace(/\\/g, "/");
}

export function resolveJazorModulePath(source: string, importer: string | undefined): string {
  const normalizedImporter = normalizeImporterPath(importer);
  if (!normalizedImporter || !source.startsWith(".")) {
    return normalizeWorkspacePath(source);
  }

  return normalizeWorkspacePath(resolve(dirname(normalizedImporter), source));
}

function getDocumentKind(documentPath: string): DocumentSnapshot["documentKind"] | null {
  if (documentPath.endsWith(".jazor")) {
    return "Jazor";
  }

  if (documentPath.endsWith(".vue")) {
    return "Vue";
  }

  if (documentPath.endsWith(".ts")) {
    return "TypeScript";
  }

  if (documentPath.endsWith(".js")) {
    return "JavaScript";
  }

  return null;
}

function getModuleIdForDocumentPath(documentPath: string): string {
  return JAZOR_PREFIX + normalizeWorkspacePath(documentPath);
}

function getModuleIdCandidates(documentPath: string): string[] {
  const normalized = normalizeWorkspacePath(documentPath);
  const slashNormalized = documentPath.replace(/\\/g, "/");
  const backslashNormalized = documentPath.replace(/\//g, "\\");
  const normalizedBackslash = normalized.replace(/\//g, "\\");
  const candidates = [
    JAZOR_PREFIX + normalized,
    JAZOR_PREFIX + normalizedBackslash,
    JAZOR_PREFIX + slashNormalized,
    JAZOR_PREFIX + backslashNormalized,
    JAZOR_PREFIX + documentPath
  ];

  return [...new Set(candidates)];
}

const BASE64_VLQ_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

function toLineStarts(text: string): number[] {
  const lineStarts = [0];
  for (let index = 0; index < text.length; index += 1) {
    if (text[index] === "\n") {
      lineStarts.push(index + 1);
    }
  }

  return lineStarts;
}

function encodeBase64Vlq(value: number): string {
  let vlq = value < 0
    ? ((-value) << 1) + 1
    : value << 1;
  let encoded = "";

  do {
    let digit = vlq & 31;
    vlq >>>= 5;
    if (vlq > 0) {
      digit |= 32;
    }

    encoded += BASE64_VLQ_CHARS[digit];
  } while (vlq > 0);

  return encoded;
}

function getLineAndColumn(lineStarts: number[], offset: number): { line: number; column: number } {
  let line = 0;
  for (let index = 0; index < lineStarts.length; index += 1) {
    if (lineStarts[index] > offset) {
      break;
    }

    line = index;
  }

  return {
    line,
    column: Math.max(0, offset - lineStarts[line])
  };
}

function buildMappings(
  sourceText: string,
  generatedText: string,
  descriptor: SourceMapDescriptor
): string {
  const sourceLineStarts = toLineStarts(sourceText);
  const generatedLineStarts = toLineStarts(generatedText);
  const segments: string[] = [];
  let previousSourceLine = 0;
  let previousSourceColumn = 0;

  for (let lineIndex = 0; lineIndex < generatedLineStarts.length; lineIndex += 1) {
    const generatedOffset = generatedLineStarts[lineIndex];
    const descriptorGeneratedEnd = descriptor.generatedStart + Math.max(descriptor.generatedLength, 1);
    const descriptorSourceEnd = descriptor.sourceStart + Math.max(descriptor.sourceLength, 1);
    const clampedGeneratedOffset = Math.min(Math.max(generatedOffset, descriptor.generatedStart), descriptorGeneratedEnd);
    const generatedProgress = descriptor.generatedLength <= 0
      ? 0
      : (clampedGeneratedOffset - descriptor.generatedStart) / Math.max(descriptor.generatedLength, 1);
    const sourceOffset = Math.min(
      descriptorSourceEnd,
      descriptor.sourceStart + Math.floor(generatedProgress * Math.max(descriptor.sourceLength, 1))
    );
    const sourcePosition = getLineAndColumn(sourceLineStarts, sourceOffset);
    const segment = [
      encodeBase64Vlq(0),
      encodeBase64Vlq(0),
      encodeBase64Vlq(sourcePosition.line - previousSourceLine),
      encodeBase64Vlq(sourcePosition.column - previousSourceColumn)
    ].join("");

    segments.push(segment);
    previousSourceLine = sourcePosition.line;
    previousSourceColumn = sourcePosition.column;
  }

  return segments.join(";");
}

export function buildSourceMap(
  sourcePath: string,
  sourceText: string,
  generatedPath: string,
  generatedText: string,
  descriptors: SourceMapDescriptor[]
): SourceMapLike | null {
  const descriptor = descriptors.find((candidate) =>
    normalizeWorkspacePath(candidate.generatedPath) === normalizeWorkspacePath(generatedPath)
  ) ?? descriptors[0];
  if (!descriptor) {
    return null;
  }

  return {
    version: 3,
    file: generatedPath,
    sources: [sourcePath],
    sourcesContent: [sourceText],
    names: [],
    mappings: buildMappings(sourceText, generatedText, descriptor)
  };
}

export function createJazorPlugin(
  options: JazorVitePluginOptions = {}
): MinimalPlugin {
  const bootstrap = resolveVueHostBootstrap(options.vueHost);
  const trackedDocuments = new Map<string, TrackedJazorDocumentState>();
  let devServer: ViteDevServer | null = null;
  let sessionProcessId: number | null = null;
  let addHandler: ((path: string) => void) | null = null;
  let changeHandler: ((path: string) => void) | null = null;
  let unlinkHandler: ((path: string) => void) | null = null;
  let session: PersistentVueHostSession | null = null;

  async function getSession(): Promise<PersistentVueHostSession> {
    if (!session) {
      session = createPersistentVueHostSession(bootstrap);
    }

    await session.ensureStarted();
    const nextProcessId = session.processId;
    if (nextProcessId !== sessionProcessId) {
      await replayTrackedDocuments(session);
      sessionProcessId = nextProcessId;
    }

    return session;
  }

  async function disposeSession(): Promise<void> {
    detachWatcherHandlers();

    if (!session) {
      return;
    }

    await session.dispose();
    session = null;
    sessionProcessId = null;
    trackedDocuments.clear();
  }

  async function replayTrackedDocuments(hostSession: PersistentVueHostSession): Promise<void> {
    for (const [documentPath, trackedState] of trackedDocuments) {
      await hostSession.openDocument({
        documentPath,
        documentKind: trackedState.kind,
        text: trackedState.text,
        version: String(trackedState.version)
      });
    }
  }

  async function syncDocument(
    documentPath: string,
    documentKind: DocumentSnapshot["documentKind"],
    sourceText?: string
  ): Promise<DocumentSnapshot> {
    const normalizedPath = normalizeWorkspacePath(documentPath);
    const trackedState = trackedDocuments.get(normalizedPath);
    const text = sourceText
      ?? trackedState?.text
      ?? await readFile(normalizedPath, "utf8");
    const nextVersion = sourceText === undefined && trackedState
      ? trackedState.version
      : (trackedState?.version ?? 0) + 1;
    const version = String(nextVersion);
    const document: DocumentSnapshot = {
      documentPath: normalizedPath,
      documentKind,
      text,
      version
    };
    const hostSession = await getSession();

    if (trackedState) {
      await hostSession.updateDocument(document);
    } else {
      await hostSession.openDocument(document);
    }

    trackedDocuments.set(normalizedPath, {
      kind: documentKind,
      text,
      version: nextVersion
    });
    return document;
  }

  async function syncJazorDocument(
    documentPath: string,
    sourceText?: string
  ): Promise<DocumentSnapshot> {
    return await syncDocument(documentPath, "Jazor", sourceText);
  }

  async function loadJazorModule(sourcePath: string): Promise<GetVirtualArtifactResponse> {
    const normalizedPath = normalizeWorkspacePath(sourcePath);
    await syncJazorDocument(normalizedPath);
    const hostSession = await getSession();
    return await hostSession.getVirtualArtifact(
      buildGetVirtualArtifactRequest(normalizedPath)
    );
  }

  function detachWatcherHandlers(): void {
    if (!devServer) {
      return;
    }

    if (addHandler && typeof devServer.watcher.off === "function") {
      devServer.watcher.off("add", addHandler);
    }

    if (changeHandler && typeof devServer.watcher.off === "function") {
      devServer.watcher.off("change", changeHandler);
    }

    if (unlinkHandler && typeof devServer.watcher.off === "function") {
      devServer.watcher.off("unlink", unlinkHandler);
    }

    addHandler = null;
    changeHandler = null;
    unlinkHandler = null;
    devServer = null;
  }

  function createWatchSyncHandler(): (path: string) => void {
    return (path: string) => {
      const kind = getDocumentKind(path);
      if (!kind) {
        return;
      }

      void syncDocument(path, kind).catch(() => {
        // Best-effort workspace sync for non-.jazor files.
      });
    };
  }

  function collectAffectedModules(
    affectedDocumentPaths: readonly string[],
    contextModules: ModuleNode[],
    server: ViteDevServer
  ): ModuleNode[] {
    const affectedModules: ModuleNode[] = [];
    const seen = new Set<ModuleNode>();

    for (const documentPath of affectedDocumentPaths) {
      if (typeof server.moduleGraph.getModuleById === "function") {
        for (const moduleId of getModuleIdCandidates(documentPath)) {
          const moduleNode = server.moduleGraph.getModuleById(moduleId);
          if (!moduleNode || seen.has(moduleNode)) {
            continue;
          }

          seen.add(moduleNode);
          affectedModules.push(moduleNode);
          break;
        }
      }
    }

    for (const moduleNode of contextModules) {
      if (!seen.has(moduleNode)) {
        seen.add(moduleNode);
        affectedModules.push(moduleNode);
      }
    }

    return affectedModules;
  }

  function collectTrackedJazorModules(server: ViteDevServer): ModuleNode[] {
    const modules: ModuleNode[] = [];
    const seen = new Set<ModuleNode>();

    for (const [documentPath, trackedState] of trackedDocuments) {
      if (trackedState.kind !== "Jazor" || typeof server.moduleGraph.getModuleById !== "function") {
        continue;
      }

      for (const moduleId of getModuleIdCandidates(documentPath)) {
        const moduleNode = server.moduleGraph.getModuleById(moduleId);
        if (!moduleNode || seen.has(moduleNode)) {
          continue;
        }

        seen.add(moduleNode);
        modules.push(moduleNode);
        break;
      }
    }

    return modules;
  }

  return {
    name: "jazor-vite",
    enforce: "pre",
    async buildStart() {
      await getSession();
    },
    async buildEnd() {
      await disposeSession();
    },
    async closeBundle() {
      await disposeSession();
    },
    configureServer(server) {
      detachWatcherHandlers();
      devServer = server;
      addHandler = createWatchSyncHandler();
      changeHandler = createWatchSyncHandler();
      unlinkHandler = (path: string) => {
        const kind = getDocumentKind(path);
        if (!kind) {
          return;
        }

        const normalizedPath = normalizeWorkspacePath(path);
        trackedDocuments.delete(normalizedPath);
        if (!session) {
          return;
        }

        void session.closeDocument(normalizedPath).catch(() => {
          // Best-effort cleanup on file removal.
        });
      };
      server.watcher.on("add", addHandler);
      server.watcher.on("change", changeHandler);
      server.watcher.on("unlink", unlinkHandler);
    },
    resolveId(source: string, importer?: string) {
      if (!source.endsWith(".jazor")) {
        return null;
      }

      return JAZOR_PREFIX + resolveJazorModulePath(source, importer);
    },
    async load(id: string) {
      if (!id.startsWith(JAZOR_PREFIX)) {
        return null;
      }

      const sourcePath = normalizeWorkspacePath(id.slice(JAZOR_PREFIX.length));
      const artifactResponse = await loadJazorModule(sourcePath);
      const trackedDocument = trackedDocuments.get(sourcePath);
      const code = artifactResponse.artifact.content;
      const map = trackedDocument
        ? buildSourceMap(
          sourcePath,
          trackedDocument.text,
          artifactResponse.artifact.artifactName,
          code,
          artifactResponse.sourceMaps
        )
        : null;
      return {
        code,
        map
      };
    },
    async handleHotUpdate(context: HmrContext): Promise<ModuleNode[] | void> {
      const documentKind = getDocumentKind(context.file);
      if (!documentKind) {
        return;
      }

      const sourceText = context.read
        ? await context.read()
        : await readFile(context.file, "utf8");
      const syncedDocument = await syncDocument(context.file, documentKind, sourceText);
      const hostSession = await getSession();
      const hotUpdatePlan = await hostSession.getHotUpdatePlan({
        documentPath: syncedDocument.documentPath,
        documentKind,
        version: syncedDocument.version
      });
      const server = devServer ?? context.server;
      const affectedModules = collectAffectedModules(
        hotUpdatePlan.affectedDocumentPaths,
        documentKind === "Jazor" ? context.modules : [],
        server
      );
      const modulesToInvalidate = affectedModules.length === 0 && documentKind !== "Jazor"
        ? collectTrackedJazorModules(server)
        : affectedModules;
      for (const moduleNode of modulesToInvalidate) {
        server.moduleGraph.invalidateModule(moduleNode);
      }

      return modulesToInvalidate;
    }
  };
}
