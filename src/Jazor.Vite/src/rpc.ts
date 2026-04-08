import type { GetVirtualArtifactResponse } from "./contracts";
import {
  createPersistentVueHostSession,
  type PersistentVueHostSession,
  type VueHostBootstrapOptions
} from "./vue-host-session";

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

export class BunVueHostTransport implements VueHostTransport {
  private readonly session: PersistentVueHostSession;

  public constructor(
    options: VueHostProcessOptions,
    sessionFactory: (bootstrap: Required<VueHostBootstrapOptions>) => PersistentVueHostSession = createPersistentVueHostSession
  ) {
    this.session = sessionFactory({
      command: options.command,
      args: (options.arguments ?? []).join(" "),
      argsList: options.arguments ?? [],
      rpcMode: options.rpcMode ?? "process-stdio"
    });
  }

  public async getVirtualArtifact(documentPath: string, text: string): Promise<GetVirtualArtifactResponse> {
    await this.upsertDocument(documentPath, text);
    return await this.session.getVirtualArtifact({
      documentPath,
      artifactKind: "vue-sfc",
      text: null,
      version: null
    });
  }

  public async upsertDocument(documentPath: string, text: string): Promise<void> {
    const version = createVersion(text);
    await this.session.openDocument({
      documentPath,
      documentKind: "Jazor",
      text,
      version
    });
  }

  public async closeDocument(documentPath: string): Promise<void> {
    await this.session.closeDocument(documentPath);
  }

  public async dispose(): Promise<void> {
    await this.session.dispose();
  }
}

function createVersion(content: string): string {
  let hash = 2166136261;

  for (let index = 0; index < content.length; index += 1) {
    hash ^= content.charCodeAt(index);
    hash = Math.imul(hash, 16777619);
  }

  return `v${hash >>> 0}`;
}
