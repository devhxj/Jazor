import { type AnalyzeJazorRequest, type AnalyzeJazorResponse } from "./contracts";
export interface VueHostProcessOptions {
    command: string;
    arguments?: string[];
}
export interface VueHostTransport {
    analyzeJazor(request: AnalyzeJazorRequest): Promise<AnalyzeJazorResponse>;
}
export declare class BunVueHostTransport implements VueHostTransport {
    private readonly options;
    constructor(options: VueHostProcessOptions);
    analyzeJazor(request: AnalyzeJazorRequest): Promise<AnalyzeJazorResponse>;
    private invoke;
}
