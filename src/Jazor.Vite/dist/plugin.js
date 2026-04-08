import { readFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { BunVueHostTransport } from "./rpc";
const virtualPrefix = "\0jazor:";
export function createJazorVuePlugin(options) {
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
function normalizeDocumentPath(root, filePath) {
    const normalizedRoot = root.replace(/\\/g, "/");
    const normalizedFilePath = filePath.replace(/\\/g, "/");
    if (normalizedFilePath.startsWith(`${normalizedRoot}/`)) {
        return normalizedFilePath.slice(normalizedRoot.length + 1);
    }
    return normalizedFilePath;
}
