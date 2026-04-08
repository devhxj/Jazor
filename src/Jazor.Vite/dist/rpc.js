import { createPersistentVueHostSession } from "./vue-host-session";
export class BunVueHostTransport {
    session;
    constructor(options, sessionFactory = createPersistentVueHostSession) {
        this.session = sessionFactory({
            command: options.command,
            args: (options.arguments ?? []).join(" "),
            argsList: options.arguments ?? [],
            rpcMode: options.rpcMode ?? "process-stdio"
        });
    }
    async getVirtualArtifact(documentPath, text) {
        await this.upsertDocument(documentPath, text);
        return await this.session.getVirtualArtifact({
            documentPath,
            artifactKind: "vue-sfc",
            text: null,
            version: null
        });
    }
    async upsertDocument(documentPath, text) {
        const version = createVersion(text);
        await this.session.openDocument({
            documentPath,
            documentKind: "Jazor",
            text,
            version
        });
    }
    async closeDocument(documentPath) {
        await this.session.closeDocument(documentPath);
    }
    async dispose() {
        await this.session.dispose();
    }
}
function createVersion(content) {
    let hash = 2166136261;
    for (let index = 0; index < content.length; index += 1) {
        hash ^= content.charCodeAt(index);
        hash = Math.imul(hash, 16777619);
    }
    return `v${hash >>> 0}`;
}
