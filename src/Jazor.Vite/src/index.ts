import type { Plugin } from "vite";

const JAZOR_PREFIX = "\0jazor:";

export interface VueHostBootstrapOptions {
  command?: string;
  args?: string;
  rpcMode?: string;
}

export interface JazorVitePluginOptions {
  vueHost?: VueHostBootstrapOptions;
}

export function resolveVueHostBootstrap(
  options: VueHostBootstrapOptions | undefined = undefined
): Required<VueHostBootstrapOptions> {
  const env = typeof process !== "undefined" ? process.env : {};

  return {
    command: options?.command ?? env.JAZOR_VUEHOST_COMMAND ?? "",
    args: options?.args ?? env.JAZOR_VUEHOST_ARGS ?? "",
    rpcMode: options?.rpcMode ?? env.JAZOR_VUEHOST_RPC_MODE ?? "process-stdio"
  };
}

export function createJazorPlugin(
  options: JazorVitePluginOptions = {}
): Plugin {
  const bootstrap = resolveVueHostBootstrap(options.vueHost);

  return {
    name: "jazor-vite",
    enforce: "pre",
    resolveId(source) {
      if (!source.endsWith(".jazor")) {
        return null;
      }

      return JAZOR_PREFIX + source;
    },
    async load(id) {
      if (!id.startsWith(JAZOR_PREFIX)) {
        return null;
      }

      const sourcePath = id.slice(JAZOR_PREFIX.length);
      throw new Error(
        [
          `Jazor.VueHost RPC load is not implemented yet for '${sourcePath}'.`,
          `Expected VueHost command: '${bootstrap.command || "<missing>"}'.`,
          "Wire this hook to Jazor.VueHost getVueArtifact / getSourceMap RPC next."
        ].join(" ")
      );
    }
  };
}
