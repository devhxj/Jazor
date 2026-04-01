export interface WebIdlInventory {
  schemaVersion: 1;
  generatedAt: string;
  source: WebIdlSourceInfo;
  files: WebIdlFile[];
  interfaceEvents: InterfaceEventMap[];
  stats: WebIdlStats;
}

export interface WebIdlSourceInfo {
  parser: string;
  webrefIdl: string;
  webrefCss: string;
  webrefEvents: string;
}

export interface WebIdlFile {
  fileName: string;
  namespace?: string;
  declarations: WebIdlDeclaration[];
}

export interface WebIdlDeclaration {
  kind: string;
  name?: string;
  partial?: boolean;
  inheritance?: string;
  target?: string;
  includes?: string;
  memberCount?: number;
  payload: unknown;
}

export interface InterfaceEventMap {
  interfaceName: string;
  events: InterfaceEvent[];
}

export interface InterfaceEvent {
  eventType: string;
  interfaceName: string;
}

export interface WebIdlStats {
  fileCount: number;
  declarationCount: number;
  interfaceEventTargetCount: number;
  declarationsByKind: Record<string, number>;
}
