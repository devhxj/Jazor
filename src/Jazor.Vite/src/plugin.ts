import { dirname, relative, resolve } from "node:path";
import {
  type AnalyzeJazorRequest,
  type ArtifactRecord,
  type DocumentSnapshot
} from "./contracts";
import { type VueHostProcessOptions, type VueHostTransport, BunVueHostTransport } from "./rpc";

const virtualPrefix = "\0jazor:";

export interface JazorVuePluginOptions {
  root?: string;
  host: VueHostProcessOptions;
  transport?: VueHostTransport;
}

export interface VitePluginLike {
  name: string;
  enforce?: "pre" | "post";
  resolveId?(source: string, importer?: string): string | null | Promise<string | null>;
  load?(id: string): string | null | Promise<string | null>;
  handleHotUpdate?(context: { file: string }): { type: "full-reload" } | null | Promise<{ type: "full-reload" } | null>;
}

export function createJazorVuePlugin(options: JazorVuePluginOptions): VitePluginLike {
  const root = resolve(options.root ?? process.cwd());
  const transport = options.transport ?? new BunVueHostTransport(options.host);

  return {
    name: "jazor-vite",
    enforce: "pre",
    resolveId(source, importer) {
      if (!source.endsWith(".jazor")) {
        return null;
      }

      const importerPath = importer?.startsWith(virtualPrefix)
        ? importer.slice(virtualPrefix.length)
        : importer;

      return `${virtualPrefix}${!importer
        ? resolve(root, source)
        : resolve(dirname(importerPath!), source)}`;
    },
    async load(id) {
      if (!id.startsWith(virtualPrefix)) {
        return null;
      }

      const filePath = id.slice(virtualPrefix.length);
      const document = await readJazorDocument(root, filePath);
      const request: AnalyzeJazorRequest = {
        jazorDocument: document,
        relatedDocuments: [],
        frontendContext: null
      };
      const response = await transport.analyzeJazor(request);
      const artifact = findArtifact(response.artifacts, "vue-sfc", document.documentPath);
      return artifact?.content ?? null;
    },
    handleHotUpdate(context) {
      if (!context.file.endsWith(".jazor")) {
        return null;
      }

      return { type: "full-reload" };
    }
  };
}

async function readJazorDocument(root: string, filePath: string): Promise<DocumentSnapshot> {
  if (typeof Bun !== "undefined") {
    const text = await Bun.file(filePath).text();
    return {
      documentPath: normalizeDocumentPath(root, filePath),
      documentKind: "Jazor",
      text,
      version: "vite"
    };
  }

  throw new Error("Jazor.Vite currently requires Bun runtime to load .jazor files.");
}

function findArtifact(
  artifacts: ArtifactRecord[],
  artifactKind: string,
  documentPath: string
): ArtifactRecord | undefined {
  return artifacts.find((artifact) =>
    artifact.artifactKind === artifactKind &&
    artifact.artifactName.includes(documentPath));
}

function normalizeDocumentPath(root: string, filePath: string): string {
  const relativePath = relative(root, filePath);
  return relativePath.length === 0 ? filePath : relativePath.replace(/\\/g, "/");
}
