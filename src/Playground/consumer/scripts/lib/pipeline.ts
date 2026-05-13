import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

type PreparedWorkspace = {
  consumerRoot: string;
  hostJazorRoot: string;
  hostWwwrootRoot: string;
  buildRoot: string;
  browserGeneratedRoot: string;
  ssrGeneratedRoot: string;
  distRoot: string;
  browserBundleDirectory: string;
  hostBrowserBundleDirectory: string;
  legacyHostAssetsDirectory: string;
  clientEntryPath: string;
  ssrEntryPath: string;
  vueFeatureFlagsPath: string;
};

const consumerRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const repositoryRoot = resolve(consumerRoot, "..", "..", "..");
const defaultCatalogComponentId = "Playground.Pages.PlaygroundCatalogPage";
const defaultCatalogComponentName = "PlaygroundCatalogPage";
const defaultDetailComponentId = "Playground.Pages.PlaygroundDetailPage";
const defaultDetailComponentName = "PlaygroundDetailPage";

export async function prepareWorkspace(production: boolean): Promise<PreparedWorkspace> {
  const hostJazorRoot = resolvePathFromEnvironment(
    "RAZORVUE_HOST_JAZOR_ROOT",
    resolve(consumerRoot, "..", "jazor")
  );
  const hostWwwrootRoot = resolvePathFromEnvironment(
    "RAZORVUE_HOST_WWWROOT_ROOT",
    resolve(consumerRoot, "..", "wwwroot")
  );
  const buildRoot = resolvePathFromEnvironment(
    "RAZORVUE_BUILD_ROOT",
    resolve(consumerRoot, ".deno-build", `pid-${Deno.pid}`)
  );
  const browserGeneratedRoot = resolve(buildRoot, "generated-browser");
  const ssrGeneratedRoot = resolve(buildRoot, "generated-ssr");
  const distRoot = resolvePathFromEnvironment("RAZORVUE_DIST_ROOT", resolve(consumerRoot, "dist"));
  const browserBundleDirectory = resolve(distRoot, "jazor");
  const hostBrowserBundleDirectory = resolve(hostWwwrootRoot, "jazor");
  const legacyHostAssetsDirectory = resolve(hostWwwrootRoot, "assets");
  const manifestPath = resolve(hostJazorRoot, "jazor-manifest.json");
  const hostRequirementsModulePath = resolve(hostJazorRoot, "__jazor", "razorvue-host.mjs");
  const clientEntryPath = resolve(buildRoot, "client-entry.mjs");
  const ssrEntryPath = resolve(buildRoot, "ssr-entry.mjs");
  const vueFeatureFlagsPath = resolve(buildRoot, "vue-feature-flags.mjs");

  await runOfficialRazorVueConsumerEntry({
    hostJazorRoot,
    manifestPath,
    hostRequirementsModulePath,
    buildRoot,
    browserGeneratedRoot,
    ssrGeneratedRoot,
    clientEntryPath,
    ssrEntryPath,
    vueFeatureFlagsPath,
    production,
    catalogComponentSelector: readConfiguredComponentSelector(
      "CATALOG",
      defaultCatalogComponentId,
      defaultCatalogComponentName),
    detailComponentSelector: readConfiguredComponentSelector(
      "DETAIL",
      defaultDetailComponentId,
      defaultDetailComponentName)
  });

  return {
    consumerRoot,
    hostJazorRoot,
    hostWwwrootRoot,
    buildRoot,
    browserGeneratedRoot,
    ssrGeneratedRoot,
    distRoot,
    browserBundleDirectory,
    hostBrowserBundleDirectory,
    legacyHostAssetsDirectory,
    clientEntryPath,
    ssrEntryPath,
    vueFeatureFlagsPath
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

export async function readText(path: string): Promise<string> {
  return await Deno.readTextFile(path);
}

export async function writeText(path: string, content: string): Promise<void> {
  await ensureDirectory(dirname(path));
  await Deno.writeTextFile(path, content);
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

export async function collectFiles(path: string): Promise<string[]> {
  const files: string[] = [];
  await collectFilesCore(path, files);
  return files.sort((left, right) => left.localeCompare(right, "en"));
}

export async function copyDirectory(source: string, destination: string): Promise<void> {
  await emptyDirectory(destination);
  for await (const entry of Deno.readDir(source)) {
    const sourcePath = join(source, entry.name);
    const destinationPath = join(destination, entry.name);
    if (entry.isDirectory) {
      await copyDirectory(sourcePath, destinationPath);
      continue;
    }

    if (entry.isFile) {
      await ensureDirectory(dirname(destinationPath));
      await Deno.copyFile(sourcePath, destinationPath);
    }
  }
}

export async function importModule<T>(path: string): Promise<T> {
  return await import(createImportUrl(path)) as T;
}

export function createImportUrl(path: string): string {
  return `${pathToFileURL(path).href}?t=${Date.now().toString(36)}`;
}

function resolvePathFromEnvironment(environmentVariableName: string, defaultPath: string): string {
  const configuredPath = Deno.env.get(environmentVariableName)?.trim();
  return configuredPath === undefined || configuredPath.length === 0
    ? defaultPath
    : resolve(consumerRoot, configuredPath);
}

function readConfiguredComponentSelector(
  name: "CATALOG" | "DETAIL",
  defaultComponentId: string,
  defaultComponentName: string
): string {
  const selector = readConfiguredText(`RAZORVUE_${name}_COMPONENT_SELECTOR`);
  if (selector !== null) {
    return selector;
  }

  const id = readConfiguredText(`RAZORVUE_${name}_COMPONENT_ID`);
  if (id !== null) {
    return `id:${id}`;
  }

  const nameValue = readConfiguredText(`RAZORVUE_${name}_COMPONENT_NAME`);
  if (nameValue !== null) {
    return `name:${nameValue}`;
  }

  if (name === "CATALOG") {
    const legacyRootId = readConfiguredText("RAZORVUE_ROOT_COMPONENT_ID");
    if (legacyRootId !== null) {
      return `id:${legacyRootId}`;
    }

    const legacyRootName = readConfiguredText("RAZORVUE_ROOT_COMPONENT_NAME");
    if (legacyRootName !== null) {
      return `name:${legacyRootName}`;
    }
  }

  return defaultComponentId.length > 0
    ? `id:${defaultComponentId}`
    : `name:${defaultComponentName}`;
}

function readConfiguredText(environmentVariableName: string): string | null {
  const value = Deno.env.get(environmentVariableName)?.trim();
  return value === undefined || value.length === 0 ? null : value;
}

async function collectFilesCore(path: string, files: string[]): Promise<void> {
  for await (const entry of Deno.readDir(path)) {
    const entryPath = join(path, entry.name);
    if (entry.isDirectory) {
      await collectFilesCore(entryPath, files);
      continue;
    }

    if (entry.isFile) {
      files.push(entryPath);
    }
  }
}

type ConsumerEntryInvocation = {
  hostJazorRoot: string;
  manifestPath: string;
  hostRequirementsModulePath: string;
  buildRoot: string;
  browserGeneratedRoot: string;
  ssrGeneratedRoot: string;
  clientEntryPath: string;
  ssrEntryPath: string;
  vueFeatureFlagsPath: string;
  production: boolean;
  catalogComponentSelector: string;
  detailComponentSelector: string;
};

async function runOfficialRazorVueConsumerEntry(invocation: ConsumerEntryInvocation): Promise<void> {
  const resultPath = resolve(invocation.buildRoot, "razorvue-consumer-entry.json");
  const entryArgs = [
    "--host-root",
    invocation.hostJazorRoot,
    "--manifest",
    invocation.manifestPath,
    "--host-requirements",
    invocation.hostRequirementsModulePath,
    "--out",
    invocation.buildRoot,
    "--browser-generated-root",
    invocation.browserGeneratedRoot,
    "--ssr-generated-root",
    invocation.ssrGeneratedRoot,
    "--client-entry",
    invocation.clientEntryPath,
    "--ssr-entry",
    invocation.ssrEntryPath,
    "--vue-feature-flags",
    invocation.vueFeatureFlagsPath,
    "--client-runtime",
    resolve(consumerRoot, "src", "runtime-client.js"),
    "--ssr-runtime",
    resolve(consumerRoot, "src", "runtime-ssr.js"),
    "--client-runtime-export",
    "mountPlaygroundConsumer",
    "--ssr-runtime-export",
    "runPlaygroundConsumerSsr",
    "--ssr-execute-export",
    "executeSsrSmoke",
    "--component",
    `CatalogPage=${invocation.catalogComponentSelector}`,
    "--component",
    `DetailPage=${invocation.detailComponentSelector}`,
    "--mode",
    "both",
    "--production",
    invocation.production ? "true" : "false",
    "--clean",
    "true",
    "--write-result",
    resultPath
  ];
  const emitToolPath = await resolveConfiguredJazorEmitToolPath();
  const args = emitToolPath === null
    ? [
      "run",
      "--project",
      "src/Jazor.Emit/Jazor.Emit.csproj",
      "--",
      "razorvue-consumer-entry",
      ...entryArgs
    ]
    : [
      emitToolPath,
      "razorvue-consumer-entry",
      ...entryArgs
    ];

  const output = await new Deno.Command("dotnet", {
    cwd: repositoryRoot,
    args,
    stdin: "null",
    stdout: "piped",
    stderr: "piped"
  }).output();

  if (!output.success) {
    const stderr = new TextDecoder().decode(output.stderr).trim();
    const stdout = new TextDecoder().decode(output.stdout).trim();
    throw new Error(
      [
        "Official RazorVue consumer entry generation failed for Playground.",
        stdout.length > 0 ? stdout : null,
        stderr.length > 0 ? stderr : null
      ].filter((line) => line !== null).join("\n")
    );
  }

  if (!(await fileExists(resultPath))) {
    throw new Error(`Official RazorVue consumer entry generation did not write result metadata: ${resultPath}`);
  }
}

async function resolveConfiguredJazorEmitToolPath(): Promise<string | null> {
  const configuredPath =
    Deno.env.get("JAZOR_EMIT_TOOL_PATH")?.trim() ??
    Deno.env.get("RAZORVUE_CONSUMER_ENTRY_TOOL_PATH")?.trim() ??
    Deno.env.get("RAZORVUE_SFC_BRIDGE_TOOL_PATH")?.trim();
  if (configuredPath === undefined || configuredPath.length === 0) {
    return null;
  }

  const resolvedPath = resolve(repositoryRoot, configuredPath);
  if (!(await isFile(resolvedPath))) {
    throw new Error(`Configured Jazor.Emit tool path was not found: ${resolvedPath}`);
  }

  return resolvedPath;
}

async function isFile(path: string): Promise<boolean> {
  try {
    const stat = await Deno.stat(path);
    return stat.isFile;
  } catch (error) {
    if (error instanceof Deno.errors.NotFound) {
      return false;
    }

    throw error;
  }
}

export type { PreparedWorkspace };
