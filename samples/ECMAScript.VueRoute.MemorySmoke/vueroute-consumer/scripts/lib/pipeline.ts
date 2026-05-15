import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

export type PreparedWorkspace = {
  consumerRoot: string;
  generatedRoot: string;
  buildRoot: string;
  distRoot: string;
  assetsDirectory: string;
  importMapPath: string;
  clientEntryPath: string;
};

type PrepareWorkspaceOptions = {
  cleanBuildRoot?: boolean;
};

const consumerRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const sampleRoot = resolve(consumerRoot, "..");
const defaultGeneratedRoot = resolve(sampleRoot, "VueRoute.MemorySmoke.Host", "wwwroot", "jazor");
const defaultBuildRoot = resolve(consumerRoot, ".deno-build");
const defaultDistRoot = resolve(consumerRoot, "dist");

export async function prepareWorkspace(options: PrepareWorkspaceOptions = {}): Promise<PreparedWorkspace> {
  const generatedRoot = resolvePathFromEnvironment("JAZOR_GENERATED_ROOT", defaultGeneratedRoot);
  const buildRoot = resolvePathFromEnvironment("VUEROUTE_DENO_BUILD_ROOT", defaultBuildRoot);
  const distRoot = resolvePathFromEnvironment("VUEROUTE_DENO_DIST_ROOT", defaultDistRoot);
  const assetsDirectory = join(distRoot, "assets");
  const importMapPath = join(buildRoot, "import-map.generated.json");
  const clientEntryPath = join(buildRoot, "client-entry.mjs");

  await assertPathExists(join(generatedRoot, "host", "app.mjs"), "generated host app module");
  await assertPathExists(join(generatedRoot, "router", "memory-router.mjs"), "generated router module");
  await assertPathExists(join(generatedRoot, "tests", "router-testing.mjs"), "generated testing module");
  await assertPathExists(join(generatedRoot, "jazor-manifest.json"), "generated manifest");

  if (options.cleanBuildRoot ?? true) {
    await emptyDirectory(buildRoot);
  } else {
    await ensureDirectory(buildRoot);
  }

  await writeImportMap(importMapPath, generatedRoot);
  await writeText(
    clientEntryPath,
    [
      'import "./../src/style.css";',
      'import { boot } from "host/app.mjs";',
      "",
      'boot("#app");',
      ""
    ].join("\n")
  );

  return {
    consumerRoot,
    generatedRoot,
    buildRoot,
    distRoot,
    assetsDirectory,
    importMapPath,
    clientEntryPath
  };
}

export async function emptyDirectory(path: string): Promise<void> {
  await Deno.remove(path, { recursive: true }).catch((error: unknown) => {
    if (!(error instanceof Deno.errors.NotFound)) {
      throw error;
    }
  });

  await Deno.mkdir(path, { recursive: true });
}

export async function ensureDirectory(path: string): Promise<void> {
  await Deno.mkdir(path, { recursive: true });
}

export async function fileExists(path: string): Promise<boolean> {
  try {
    await Deno.stat(path);
    return true;
  } catch (error) {
    if (error instanceof Deno.errors.NotFound) {
      return false;
    }

    throw error;
  }
}

export async function readText(path: string): Promise<string> {
  return await Deno.readTextFile(path);
}

export async function writeText(path: string, content: string): Promise<void> {
  await ensureDirectory(dirname(path));
  await Deno.writeTextFile(path, content);
}

export async function runDeno(
  workspace: PreparedWorkspace,
  args: string[],
  extraEnvironment: Record<string, string> = {}
): Promise<void> {
  const child = new Deno.Command(Deno.execPath(), {
    cwd: workspace.consumerRoot,
    args,
    env: {
      JAZOR_GENERATED_ROOT: workspace.generatedRoot,
      ...extraEnvironment
    },
    stdin: "null",
    stdout: "inherit",
    stderr: "inherit"
  }).spawn();

  const output = await child.status;
  if (!output.success) {
    throw new Error(`deno ${args.join(" ")} failed with exit code ${output.code}.`);
  }
}

export function resolvePathFromEnvironment(environmentVariableName: string, defaultPath: string): string {
  const configuredPath = Deno.env.get(environmentVariableName)?.trim();
  if (configuredPath === undefined || configuredPath.length === 0) {
    return defaultPath;
  }

  return resolve(consumerRoot, configuredPath);
}

async function assertPathExists(path: string, description: string): Promise<void> {
  if (!(await fileExists(path))) {
    throw new Error(`Missing ${description}: ${path}`);
  }
}

async function writeImportMap(importMapPath: string, generatedRoot: string): Promise<void> {
  const generatedRootUrl = toDirectoryFileUrl(generatedRoot);
  const importMap = {
    imports: {
      vue: "npm:vue@3.5.13",
      "vue-router": "npm:vue-router@4.5.1",
      "jsdom": "npm:jsdom@29.1.1",
      "npm:vue@3": "npm:vue@3.5.13",
      "npm:vue-router@4": "npm:vue-router@4.5.1",
      "components/": `${generatedRootUrl}components/`,
      "host/": `${generatedRootUrl}host/`,
      "router/": `${generatedRootUrl}router/`,
      "tests/": `${generatedRootUrl}tests/`,
      "System/": `${generatedRootUrl}System/`
    }
  };

  await writeText(importMapPath, `${JSON.stringify(importMap, null, 2)}\n`);
}

function toDirectoryFileUrl(path: string): string {
  const resolvedPath = resolve(path);
  const withTrailingSeparator = resolvedPath.endsWith("\\") || resolvedPath.endsWith("/")
    ? resolvedPath
    : `${resolvedPath}\\`;
  return pathToFileURL(withTrailingSeparator).href;
}
