import type { DiagnosticRecord, SourceMapDescriptor, GetHotUpdatePlanRequest, GetHotUpdatePlanResponse } from "./contracts";
export interface RpcRequestEnvelope {
    id: string;
    method: string;
    payloadJson: string | null;
}
export interface RpcResponseEnvelope {
    id?: string | null;
    success: boolean;
    payloadJson?: string | null;
    error?: {
        code: string;
        message: string;
        details?: string | null;
    } | null;
}
export interface HostCapabilityDescriptor {
    name: string;
    description?: string | null;
}
export interface GetHostInfoResponse {
    hostName: string;
    protocolVersion: string;
    capabilities: HostCapabilityDescriptor[];
}
export interface DocumentSnapshot {
    documentPath: string;
    documentKind: "Jazor" | "Vue" | "JavaScript" | "TypeScript" | "Unknown";
    text: string;
    version: string | null;
}
export interface ArtifactRecord {
    artifactName: string;
    artifactKind: string;
    content: string;
    contentHash: string | null;
}
export interface GetVirtualArtifactRequest {
    documentPath: string;
    artifactKind: string;
    text: string | null;
    version: string | null;
}
export interface GetVirtualArtifactResponse {
    artifact: ArtifactRecord;
    diagnostics: DiagnosticRecord[];
    sourceMaps: SourceMapDescriptor[];
}
export interface VueHostBootstrapOptions {
    command?: string;
    args?: string;
    argsList?: string[];
    rpcMode?: string;
}
export declare function splitCommandLine(commandLine: string): string[];
export declare class PersistentVueHostSession {
    private readonly bootstrap;
    private readonly pending;
    private readonly trackedDocuments;
    private readonly stderrLines;
    private child;
    private childExitPromise;
    private lineReader;
    private nextRequestId;
    private startPromise;
    private disposePromise;
    private activeProcessId;
    constructor(bootstrap: Required<VueHostBootstrapOptions>);
    get processId(): number | null;
    ensureStarted(): Promise<void>;
    dispose(): Promise<void>;
    ping(): Promise<string>;
    getHostInfo(): Promise<GetHostInfoResponse>;
    openDocument(document: DocumentSnapshot): Promise<void>;
    updateDocument(document: DocumentSnapshot): Promise<void>;
    closeDocument(documentPath: string): Promise<void>;
    getOpenDocuments(): Promise<DocumentSnapshot[]>;
    getVirtualArtifact(request: GetVirtualArtifactRequest): Promise<GetVirtualArtifactResponse>;
    getHotUpdatePlan(request: GetHotUpdatePlanRequest): Promise<GetHotUpdatePlanResponse>;
    private startCore;
    private disposeCore;
    private detachChild;
    private handleOutputLine;
    private rejectAllPending;
    private sendRaw;
    private sendRawCore;
    private replayTrackedDocuments;
}
export declare function createPersistentVueHostSession(bootstrap: Required<VueHostBootstrapOptions>): PersistentVueHostSession;
