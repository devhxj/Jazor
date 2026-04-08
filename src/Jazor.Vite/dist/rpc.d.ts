import type { GetVirtualArtifactResponse } from "./contracts";
import { type PersistentVueHostSession, type VueHostBootstrapOptions } from "./vue-host-session";
export interface VueHostProcessOptions {
    command: string;
    arguments?: string[];
    rpcMode?: string;
}
export interface VueHostTransport {
    getVirtualArtifact(documentPath: string, text: string): Promise<GetVirtualArtifactResponse>;
    upsertDocument(documentPath: string, text: string): Promise<void>;
    closeDocument(documentPath: string): Promise<void>;
    dispose(): Promise<void>;
}
export declare class BunVueHostTransport implements VueHostTransport {
    private readonly session;
    constructor(options: VueHostProcessOptions, sessionFactory?: (bootstrap: Required<VueHostBootstrapOptions>) => PersistentVueHostSession);
    getVirtualArtifact(documentPath: string, text: string): Promise<GetVirtualArtifactResponse>;
    upsertDocument(documentPath: string, text: string): Promise<void>;
    closeDocument(documentPath: string): Promise<void>;
    dispose(): Promise<void>;
}
