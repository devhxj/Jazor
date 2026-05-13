import { dirname, join, relative, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

type RazorVueManifest = {
  AssemblyName: string;
  GeneratedAtUtc: string;
  Modules: RazorVueManifestModule[];
};

type RazorVueManifestModule = {
  ComponentId: string;
  ComponentName: string;
  RelativeModulePath: string;
};

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
  browserRootModuleOutputPath: string;
  ssrRootModuleOutputPath: string;
  browserDetailModuleOutputPath: string;
  ssrDetailModuleOutputPath: string;
};

type RazorVueSfcBridgeResult = {
  Modules: RazorVueSfcBridgeResultModule[];
};

type RazorVueSfcBridgeResultModule = {
  ComponentId: string;
  ComponentName: string;
  ExportName: string;
  RelativeModulePath: string;
  RelativeOutputPath: string;
  OutputPath: string;
  CssOutputPath: string | null;
};

const consumerRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const repositoryRoot = resolve(consumerRoot, "..", "..", "..");
const defaultRootComponentId = "Playground.Pages.PlaygroundCatalogPage";
const defaultRootComponentName = "PlaygroundCatalogPage";
const detailComponentId = "Playground.Pages.PlaygroundDetailPage";
const detailComponentName = "PlaygroundDetailPage";

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
  const rootComponentId = readConfiguredText("RAZORVUE_ROOT_COMPONENT_ID") ?? defaultRootComponentId;
  const rootComponentName = readConfiguredText("RAZORVUE_ROOT_COMPONENT_NAME") ?? defaultRootComponentName;
  const manifestPath = resolve(hostJazorRoot, "jazor-manifest-razorvue.json");
  const hostRequirementsModulePath = resolve(hostJazorRoot, "__jazor", "razorvue-host.mjs");

  if (!(await fileExists(manifestPath))) {
    throw new Error(`RazorVue manifest was not found for the Playground consumer: ${manifestPath}`);
  }

  if (!(await fileExists(hostRequirementsModulePath))) {
    throw new Error(`RazorVue host requirements module was not found for the Playground consumer: ${hostRequirementsModulePath}`);
  }

  await emptyDirectory(buildRoot);
  const browserBridgeResult = await runOfficialRazorVueSfcBridge(
    hostJazorRoot,
    manifestPath,
    browserGeneratedRoot,
    "browser",
    production);
  const ssrBridgeResult = await runOfficialRazorVueSfcBridge(
    hostJazorRoot,
    manifestPath,
    ssrGeneratedRoot,
    "ssr",
    production);
  const manifest = await readJson<RazorVueManifest>(manifestPath);
  const browserModules = mapBridgeModulesByRelativePath(browserBridgeResult);
  const ssrModules = mapBridgeModulesByRelativePath(ssrBridgeResult);

  let browserRootModuleOutputPath: string | null = null;
  let ssrRootModuleOutputPath: string | null = null;
  let rootComponentExportName: string | null = null;
  let browserDetailModuleOutputPath: string | null = null;
  let ssrDetailModuleOutputPath: string | null = null;
  let detailComponentExportName: string | null = null;
  for (const module of [...manifest.Modules].sort((left, right) =>
    left.RelativeModulePath.localeCompare(right.RelativeModulePath, "en"))) {
    const relativeModulePath = normalizeRelativeModulePath(module.RelativeModulePath);
    const browserBridgeModule = browserModules.get(relativeModulePath);
    const ssrBridgeModule = ssrModules.get(relativeModulePath);
    if (browserBridgeModule === undefined) {
      throw new Error(`RazorVue SFC browser bridge did not emit module '${relativeModulePath}'.`);
    }

    if (ssrBridgeModule === undefined) {
      throw new Error(`RazorVue SFC SSR bridge did not emit module '${relativeModulePath}'.`);
    }

    if (module.ComponentId === rootComponentId || module.ComponentName === rootComponentName) {
      browserRootModuleOutputPath = browserBridgeModule.OutputPath;
      ssrRootModuleOutputPath = ssrBridgeModule.OutputPath;
      rootComponentExportName = browserBridgeModule.ExportName;
    }

    if (module.ComponentId === detailComponentId || module.ComponentName === detailComponentName) {
      browserDetailModuleOutputPath = browserBridgeModule.OutputPath;
      ssrDetailModuleOutputPath = ssrBridgeModule.OutputPath;
      detailComponentExportName = browserBridgeModule.ExportName;
    }
  }

  if (browserRootModuleOutputPath === null || ssrRootModuleOutputPath === null || rootComponentExportName === null) {
    throw new Error(
      `RazorVue manifest did not contain root component '${rootComponentId}' or component name '${rootComponentName}'.`
    );
  }

  if (browserDetailModuleOutputPath === null || ssrDetailModuleOutputPath === null || detailComponentExportName === null) {
    throw new Error(
      `RazorVue manifest did not contain detail component '${detailComponentId}' or component name '${detailComponentName}'.`
    );
  }

  const clientEntryPath = resolve(buildRoot, "client-entry.mjs");
  const ssrEntryPath = resolve(buildRoot, "ssr-entry.mjs");
  const vueFeatureFlagsPath = resolve(buildRoot, "vue-feature-flags.mjs");

  await writeText(
    vueFeatureFlagsPath,
    [
      "globalThis.__VUE_OPTIONS_API__ = true;",
      "globalThis.__VUE_PROD_DEVTOOLS__ = false;",
      "globalThis.__VUE_PROD_HYDRATION_MISMATCH_DETAILS__ = false;",
      ""
    ].join("\n")
  );

  await writeText(
    clientEntryPath,
    [
      `import ${JSON.stringify(toModuleSpecifier(clientEntryPath, vueFeatureFlagsPath))};`,
      `import { ${rootComponentExportName} as CatalogPage } from ${JSON.stringify(toModuleSpecifier(clientEntryPath, browserRootModuleOutputPath))};`,
      `import { ${detailComponentExportName} as DetailPage } from ${JSON.stringify(toModuleSpecifier(clientEntryPath, browserDetailModuleOutputPath))};`,
      `import { razorVueHostRequirements } from ${JSON.stringify(toModuleSpecifier(clientEntryPath, hostRequirementsModulePath))};`,
      `import { mountPlaygroundApp } from ${JSON.stringify(toModuleSpecifier(clientEntryPath, resolve(consumerRoot, "src", "runtime-client.js")))};`,
      "",
      "mountPlaygroundApp(CatalogPage, DetailPage, razorVueHostRequirements);",
      ""
    ].join("\n")
  );

  await writeText(
    ssrEntryPath,
    [
      `import { ${rootComponentExportName} as CatalogPage } from ${JSON.stringify(toModuleSpecifier(ssrEntryPath, ssrRootModuleOutputPath))};`,
      `import { ${detailComponentExportName} as DetailPage } from ${JSON.stringify(toModuleSpecifier(ssrEntryPath, ssrDetailModuleOutputPath))};`,
      `import { razorVueHostRequirements } from ${JSON.stringify(toModuleSpecifier(ssrEntryPath, hostRequirementsModulePath))};`,
      `import { runSsrSmoke } from ${JSON.stringify(toModuleSpecifier(ssrEntryPath, resolve(consumerRoot, "src", "runtime-ssr.js")))};`,
      "",
      "export { runSsrSmoke };",
      "export async function executeSsrSmoke() {",
      "  return await runSsrSmoke(CatalogPage, DetailPage, razorVueHostRequirements);",
      "}",
      ""
    ].join("\n")
  );

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
    vueFeatureFlagsPath,
    browserRootModuleOutputPath,
    ssrRootModuleOutputPath,
    browserDetailModuleOutputPath,
    ssrDetailModuleOutputPath
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

function readConfiguredText(environmentVariableName: string): string | null {
  const value = Deno.env.get(environmentVariableName)?.trim();
  return value === undefined || value.length === 0 ? null : value;
}

function normalizeRelativeModulePath(path: string): string {
  return path.replaceAll("\\", "/").replace(/^\.\//u, "");
}

function toModuleSpecifier(fromPath: string, toPath: string): string {
  const relativePath = relative(dirname(fromPath), toPath).replaceAll("\\", "/");
  return relativePath.startsWith(".") ? relativePath : `./${relativePath}`;
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

async function readJson<T>(path: string): Promise<T> {
  return JSON.parse(await readText(path)) as T;
}

async function runOfficialRazorVueSfcBridge(
  hostJazorRoot: string,
  manifestPath: string,
  outputRoot: string,
  mode: "browser" | "ssr",
  production: boolean
): Promise<RazorVueSfcBridgeResult> {
  const resultPath = resolve(outputRoot, `razorvue-sfc-bridge.${mode}.json`);
  const bridgeArgs = [
    "--host-root",
    hostJazorRoot,
    "--manifest",
    manifestPath,
    "--out",
    outputRoot,
    "--mode",
    mode,
    "--production",
    production ? "true" : "false",
    "--clean",
    "true"
  ];
  const emitToolPath = await resolveConfiguredJazorEmitToolPath();
  const args = emitToolPath === null
    ? [
      "run",
      "--project",
      "src/Jazor.Emit/Jazor.Emit.csproj",
      "--",
      "razorvue-sfc-bridge",
      ...bridgeArgs
    ]
    : [
      emitToolPath,
      "razorvue-sfc-bridge",
      ...bridgeArgs
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
        `Official RazorVue SFC bridge failed for Playground ${mode} output.`,
        stdout.length > 0 ? stdout : null,
        stderr.length > 0 ? stderr : null
      ].filter((line) => line !== null).join("\n")
    );
  }

  if (!(await fileExists(resultPath))) {
    throw new Error(`Official RazorVue SFC bridge did not write result metadata: ${resultPath}`);
  }

  return await readJson<RazorVueSfcBridgeResult>(resultPath);
}

async function resolveConfiguredJazorEmitToolPath(): Promise<string | null> {
  const configuredPath =
    Deno.env.get("JAZOR_EMIT_TOOL_PATH")?.trim() ??
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

function mapBridgeModulesByRelativePath(
  result: RazorVueSfcBridgeResult
): ReadonlyMap<string, RazorVueSfcBridgeResultModule> {
  return new Map(result.Modules.map((module) => [
    normalizeRelativeModulePath(module.RelativeModulePath),
    module
  ]));
}

export type { PreparedWorkspace };
