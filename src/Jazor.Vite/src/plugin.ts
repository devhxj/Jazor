import { readFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import type { ModuleNode } from "vite";
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
  handleHotUpdate?(context: {
    file: string;
    modules: ModuleNode[];
    server: {
      moduleGraph: {
        getModuleById(id: string): ModuleNode | null;
        invalidateModule(module: ModuleNode): void;
      };
    };
  }): ModuleNode[] | null | Promise<ModuleNode[] | null>;
  buildEnd?(): void | Promise<void>;
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

      return `${virtualPrefix}${!importerPath
        ? resolve(root, source)
        : resolve(dirname(importerPath), source)}`;
    },
    async load(id) {
      if (!id.startsWith(virtualPrefix)) {
        return null;
      }

      const filePath = id.slice(virtualPrefix.length);
      const normalizedPath = normalizeDocumentPath(root, filePath);
      const text = await readFile(filePath, "utf8");
      const response = await transport.getVirtualArtifact(normalizedPath, text);
      return response.artifact.content;
    },
    async handleHotUpdate(context) {
      if (!context.file.endsWith(".jazor")) {
        return null;
      }

      const normalizedPath = normalizeDocumentPath(root, context.file);
      const text = await readFile(context.file, "utf8");
      await transport.upsertDocument(normalizedPath, text);

      const moduleId = `${virtualPrefix}${context.file}`;
      const moduleNode = context.server.moduleGraph.getModuleById(moduleId);
      if (moduleNode) {
        context.server.moduleGraph.invalidateModule(moduleNode);
        return [moduleNode];
      }

      return context.modules;
    },
    async buildEnd() {
      await transport.dispose();
    }
  };
}

function normalizeDocumentPath(root: string, filePath: string): string {
  const normalizedRoot = root.replace(/\\/g, "/");
  const normalizedFilePath = filePath.replace(/\\/g, "/");

  if (normalizedFilePath.startsWith(`${normalizedRoot}/`)) {
    return normalizedFilePath.slice(normalizedRoot.length + 1);
  }

  return normalizedFilePath;
}
