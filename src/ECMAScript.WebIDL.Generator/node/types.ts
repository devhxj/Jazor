export interface WebIdlInventory {
  schemaVersion: 3;
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
  webrefXref?: string;
}

export interface WebIdlFile {
  fileName: string;
  namespace?: string;
  source?: WebIdlSpecificationSource;
  declarations: WebIdlDeclaration[];
}

/**
 * The specification that contributed an IDL source file. This preserves the
 * provenance needed to make generated binding documentation traceable.
 */
export interface WebIdlSpecificationSource {
  title: string;
  url: string;
  shortname?: string;
}

/**
 * Documentation copied from the W3C/WHATWG specification source that defines an
 * API. `prose` is deliberately limited to a nearby author-written explanatory
 * paragraph; it is never synthesized from the WebIDL declaration shape.
 */
export interface WebIdlDocumentation {
  href: string;
  specificationTitle: string;
  heading?: string;
  headingHref?: string;
  prose?: string;
  usage?: string;
}

export interface WebIdlArgumentDocumentation {
  argumentIndex: number;
  documentation: WebIdlDocumentation;
}

export interface WebIdlMemberDocumentation {
  memberIndex: number;
  documentation?: WebIdlDocumentation;
  arguments?: WebIdlArgumentDocumentation[];
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
  documentation?: WebIdlDocumentation;
  memberDocumentation?: WebIdlMemberDocumentation[];
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
