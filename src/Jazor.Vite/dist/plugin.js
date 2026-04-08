import { dirname, relative, resolve } from "node:path";
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
            return `${virtualPrefix}${!importer
                ? resolve(root, source)
                : resolve(dirname(importerPath), source)}`;
        },
        async load(id) {
            if (!id.startsWith(virtualPrefix)) {
                return null;
            }
            const filePath = id.slice(virtualPrefix.length);
            const document = await readJazorDocument(root, filePath);
            const request = {
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
async function readJazorDocument(root, filePath) {
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
function findArtifact(artifacts, artifactKind, documentPath) {
    return artifacts.find((artifact) => artifact.artifactKind === artifactKind &&
        artifact.artifactName.includes(documentPath));
}
function normalizeDocumentPath(root, filePath) {
    const relativePath = relative(root, filePath);
    return relativePath.length === 0 ? filePath : relativePath.replace(/\\/g, "/");
}
