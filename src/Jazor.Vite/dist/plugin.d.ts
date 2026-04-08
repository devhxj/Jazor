import type { ModuleNode } from "vite";
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
export declare function createJazorVuePlugin(options: JazorVuePluginOptions): VitePluginLike;
