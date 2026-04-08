import { type VueHostProcessOptions, type VueHostTransport } from "./rpc";
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
    }): {
        type: "full-reload";
    } | null | Promise<{
        type: "full-reload";
    } | null>;
}
export declare function createJazorVuePlugin(options: JazorVuePluginOptions): VitePluginLike;
