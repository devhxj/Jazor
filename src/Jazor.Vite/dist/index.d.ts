import type { Plugin } from "vite";
import type { SourceMapDescriptor } from "./contracts";
import { type GetVirtualArtifactRequest, type VueHostBootstrapOptions } from "./vue-host-session";
type MinimalPlugin = Pick<Plugin, "name" | "enforce" | "buildStart" | "buildEnd" | "closeBundle" | "configureServer" | "resolveId" | "load" | "handleHotUpdate">;
export interface JazorVitePluginOptions {
    vueHost?: VueHostBootstrapOptions;
}
export interface SourceMapLike {
    version: number;
    file: string;
    sources: string[];
    sourcesContent: string[];
    names: string[];
    mappings: string;
}
export declare function resolveVueHostBootstrap(options?: VueHostBootstrapOptions | undefined): Required<VueHostBootstrapOptions>;
export declare function buildGetVirtualArtifactRequest(documentPath: string): GetVirtualArtifactRequest;
export declare function normalizeImporterPath(importer: string | undefined): string | undefined;
export declare function normalizeWorkspacePath(documentPath: string): string;
export declare function resolveJazorModulePath(source: string, importer: string | undefined): string;
export declare function buildSourceMap(sourcePath: string, sourceText: string, generatedPath: string, generatedText: string, descriptors: SourceMapDescriptor[]): SourceMapLike | null;
export declare function createJazorPlugin(options?: JazorVitePluginOptions): MinimalPlugin;
export {};
