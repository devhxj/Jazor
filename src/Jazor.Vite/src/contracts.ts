export type DocumentKind = "Jazor" | "Vue" | "TypeScript" | "JavaScript" | "Unknown";

export interface DocumentSnapshot {
  documentPath: string;
  documentKind: DocumentKind;
  text: string;
  version: string | null;
}

export interface SemanticContext {
  contextKind: string;
  relatedDocuments: DocumentSnapshot[];
  properties: Record<string, string>;
}

export interface DiagnosticRecord {
  id: string;
  severity: string;
  message: string;
  documentPath: string;
  start: number;
  length: number;
}

export interface ImportDescriptor {
  localName: string;
  source: string;
  importKind: string;
  bindingKind: string;
  importedName: string | null;
  templateVisible: boolean;
}

export interface ArtifactRecord {
  artifactName: string;
  artifactKind: string;
  content: string;
  contentHash: string | null;
}

export interface SourceMapDescriptor {
  sourcePath: string;
  generatedPath: string;
  sourceStart: number;
  sourceLength: number;
  generatedStart: number;
  generatedLength: number;
}

export interface AnalyzeJazorRequest {
  jazorDocument: DocumentSnapshot;
  relatedDocuments: DocumentSnapshot[];
  frontendContext: SemanticContext | null;
}

export interface AnalyzeJazorResponse {
  diagnostics: DiagnosticRecord[];
  imports: ImportDescriptor[];
  artifacts: ArtifactRecord[];
  sourceMaps: SourceMapDescriptor[];
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

export interface GetHotUpdatePlanRequest {
  documentPath: string;
  documentKind: DocumentKind;
  version: string | null;
}

export interface GetHotUpdatePlanResponse {
  requiresFullReload: boolean;
  affectedDocumentPaths: string[];
  reason: string;
}

export interface RpcRequestEnvelope {
  id: string;
  method: string;
  payloadJson: string | null;
}

export interface RpcErrorRecord {
  code: string;
  message: string;
  details: string | null;
}

export interface RpcResponseEnvelope {
  id: string | null;
  success: boolean;
  payloadJson: string | null;
  error: RpcErrorRecord | null;
}
