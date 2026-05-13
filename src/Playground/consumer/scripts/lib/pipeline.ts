import { createHash } from "node:crypto";
import { basename, dirname, extname, join, relative, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { compileScript, compileStyleAsync, compileTemplate, parse } from "@vue/compiler-sfc";
import ts from "typescript";

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

const consumerRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
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

  const manifest = await readJson<RazorVueManifest>(manifestPath);
  const componentExportNamesByRelativePath = new Map<string, string>();
  for (const module of manifest.Modules) {
    componentExportNamesByRelativePath.set(
      normalizeRelativeModulePath(module.RelativeModulePath),
      resolveComponentExportName(module)
    );
  }

  await emptyDirectory(buildRoot);
  await ensureDirectory(browserGeneratedRoot);
  await ensureDirectory(ssrGeneratedRoot);

  let browserRootModuleOutputPath: string | null = null;
  let ssrRootModuleOutputPath: string | null = null;
  let rootComponentExportName: string | null = null;
  let browserDetailModuleOutputPath: string | null = null;
  let ssrDetailModuleOutputPath: string | null = null;
  let detailComponentExportName: string | null = null;
  for (const module of [...manifest.Modules].sort((left, right) =>
    left.RelativeModulePath.localeCompare(right.RelativeModulePath, "en"))) {
    const sourcePath = resolve(hostJazorRoot, module.RelativeModulePath);
    const browserOutputPath = resolve(browserGeneratedRoot, replaceExtension(module.RelativeModulePath, ".mjs"));
    const ssrOutputPath = resolve(ssrGeneratedRoot, replaceExtension(module.RelativeModulePath, ".mjs"));
    const componentExportName = resolveComponentExportName(module);
    await compileVueModule(
      sourcePath,
      browserOutputPath,
      production,
      true,
      componentExportName,
      module.RelativeModulePath,
      componentExportNamesByRelativePath);
    await compileVueModule(
      sourcePath,
      ssrOutputPath,
      production,
      false,
      componentExportName,
      module.RelativeModulePath,
      componentExportNamesByRelativePath);

    if (module.ComponentId === rootComponentId || module.ComponentName === rootComponentName) {
      browserRootModuleOutputPath = browserOutputPath;
      ssrRootModuleOutputPath = ssrOutputPath;
      rootComponentExportName = componentExportName;
    }

    if (module.ComponentId === detailComponentId || module.ComponentName === detailComponentName) {
      browserDetailModuleOutputPath = browserOutputPath;
      ssrDetailModuleOutputPath = ssrOutputPath;
      detailComponentExportName = componentExportName;
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

function normalizeLineEndings(value: string): string {
  return value.replace(/\r\n?/g, "\n");
}

function replaceExtension(path: string, extension: string): string {
  return path.slice(0, path.length - extname(path).length) + extension;
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

async function compileVueModule(
  sourcePath: string,
  outputPath: string,
  production: boolean,
  includeCssImports: boolean,
  componentExportName: string,
  relativeModulePath: string,
  componentExportNamesByRelativePath: ReadonlyMap<string, string>
): Promise<void> {
  const sourceText = normalizeLineEndings(await readText(sourcePath));
  const parsed = parse(sourceText, { filename: sourcePath });
  const diagnostics = parsed.errors.map(formatCompilerMessage);
  const { descriptor } = parsed;
  const scopeId = createScopeId(sourcePath);
  const hasScopedStyles = descriptor.styles.some((style) => style.scoped);

  let bindingMetadata: ReturnType<typeof compileScript>["bindings"] | undefined;
  let scriptContent = "const _sfc_main = {};";

  if (descriptor.scriptSetup !== null) {
    try {
      const compiledScript = compileScript(descriptor, {
        id: scopeId,
        isProd: production,
        genDefaultAs: "_sfc_main"
      });
      bindingMetadata = compiledScript.bindings;
      scriptContent = compiledScript.content.trim();
      if (requiresTypeScriptTranspile(descriptor.script?.lang, descriptor.scriptSetup.lang)) {
        const transpiled = transpileTypeScriptModule(scriptContent, basename(sourcePath));
        scriptContent = transpiled.jsContent;
        diagnostics.push(...transpiled.diagnostics);
      }
    } catch (error) {
      diagnostics.push(`Failed to compile <script setup>: ${formatCompilerMessage(error)}`);
    }
  } else if (descriptor.script !== null) {
    scriptContent = rewriteDefaultExport(descriptor.script.content.trim(), "_sfc_main");
    if (isTypeScriptLanguage(descriptor.script.lang)) {
      const transpiled = transpileTypeScriptModule(scriptContent, basename(sourcePath));
      scriptContent = transpiled.jsContent;
      diagnostics.push(...transpiled.diagnostics);
    }
  }

  let templateContent = "";
  if (descriptor.template !== null) {
    try {
      const compiledTemplate = compileTemplate({
        source: descriptor.template.content,
        filename: sourcePath,
        id: scopeId,
        scoped: hasScopedStyles,
        isProd: production,
        compilerOptions: bindingMetadata === undefined ? undefined : { bindingMetadata }
      });
      diagnostics.push(...compiledTemplate.errors.map(formatCompilerMessage));
      templateContent = compiledTemplate.code.trim();
    } catch (error) {
      diagnostics.push(`Failed to compile <template>: ${formatCompilerMessage(error)}`);
    }
  }

  const cssParts: string[] = [];
  const cssModules: Record<string, Record<string, string>> = {};
  for (const styleBlock of descriptor.styles) {
    try {
      const styleSourcePath = resolveStyleSourcePath(sourcePath, styleBlock.src);
      const styleSourceText = styleSourcePath === null
        ? normalizeLineEndings(styleBlock.content)
        : normalizeLineEndings(await readText(styleSourcePath));
      const moduleName = resolveCssModuleName(styleBlock.module);
      const compiledStyle = await compileStyleAsync({
        source: styleSourceText,
        filename: styleSourcePath ?? sourcePath,
        id: `data-v-${scopeId}`,
        scoped: styleBlock.scoped,
        isProd: production,
        modules: moduleName !== null,
        modulesOptions: moduleName === null
          ? undefined
          : {
            generateScopedName: (name: string, filename: string) =>
              createCssModuleScopedName(name, filename, scopeId)
          }
      });
      diagnostics.push(...compiledStyle.errors.map(formatCompilerMessage));

      const cssContent = compiledStyle.code.trim();
      if (cssContent.length > 0) {
        cssParts.push(cssContent);
      }

      if (moduleName !== null && compiledStyle.modules !== undefined) {
        cssModules[moduleName] = compiledStyle.modules;
      }
    } catch (error) {
      diagnostics.push(`Failed to compile <style>: ${formatCompilerMessage(error)}`);
    }
  }

  if (diagnostics.length > 0) {
    throw new Error(
      [
        `Failed to compile RazorVue SFC '${sourcePath}' for the Playground Deno pipeline.`,
        ...diagnostics.map((diagnostic) => `- ${diagnostic}`)
      ].join("\n")
    );
  }

  const finalParts: string[] = [];
  const cssOutputPath = replaceExtension(outputPath, ".css");
  if (cssParts.length > 0) {
    await writeText(cssOutputPath, `${cssParts.join("\n\n")}\n`);
    if (includeCssImports) {
      finalParts.push(`import ${JSON.stringify(`./${basename(cssOutputPath)}`)};`);
    }
  }

  finalParts.push(scriptContent);
  if (templateContent.length > 0) {
    finalParts.push(templateContent);
    finalParts.push("_sfc_main.render = render;");
  }

  if (hasScopedStyles) {
    finalParts.push(`_sfc_main.__scopeId = "data-v-${scopeId}";`);
  }

  if (Object.keys(cssModules).length > 0) {
    finalParts.push(`_sfc_main.__cssModules = ${JSON.stringify(cssModules, null, 2)};`);
  }

  finalParts.push(`export { _sfc_main as ${componentExportName} };`);
  const moduleContent = rewriteRelativeVueSpecifiers(
    finalParts.filter((part) => part.trim().length > 0).join("\n\n"),
    relativeModulePath,
    componentExportNamesByRelativePath);
  await writeText(outputPath, `${moduleContent}\n`);
}

function resolveComponentExportName(module: RazorVueManifestModule): string {
  const componentName = module.ComponentName.trim();
  if (!isJavaScriptIdentifier(componentName)) {
    throw new Error(
      `RazorVue component '${module.ComponentId}' has component name '${module.ComponentName}', which cannot be used as a JavaScript named export.`
    );
  }

  if (componentName === "default") {
    throw new Error(`RazorVue component '${module.ComponentId}' cannot use reserved export name 'default'.`);
  }

  return componentName;
}

function isJavaScriptIdentifier(value: string): boolean {
  return /^[$_\p{ID_Start}][$\u200C\u200D\p{ID_Continue}]*$/u.test(value);
}

function rewriteRelativeVueSpecifiers(
  code: string,
  relativeModulePath: string,
  componentExportNamesByRelativePath: ReadonlyMap<string, string>
): string {
  const defaultImportPattern =
    /\bimport\s+(?<local>[$_\p{ID_Start}][$\u200C\u200D\p{ID_Continue}]*)\s+from\s+["'](?<specifier>[^"'`]+\.vue)["'];?/gu;
  const currentDirectory = dirname(normalizeRelativeModulePath(relativeModulePath)).replaceAll("\\", "/");
  const rewrittenDefaultImports = code.replace(defaultImportPattern, (match, ...args) => {
    const groups = args.at(-1) as { local?: string; specifier?: string } | undefined;
    const local = groups?.local;
    const specifier = groups?.specifier;
    if (local === undefined || specifier === undefined || !isRelativeSpecifier(specifier)) {
      return match;
    }

    const targetRelativePath = normalizeRelativeModulePath(join(currentDirectory, specifier));
    const importedExportName = componentExportNamesByRelativePath.get(targetRelativePath);
    if (importedExportName === undefined) {
      return match.replace(specifier, `${specifier.slice(0, -4)}.mjs`);
    }

    const rewrittenSpecifier = `${specifier.slice(0, -4)}.mjs`;
    return `import { ${importedExportName} as ${local} } from ${JSON.stringify(rewrittenSpecifier)};`;
  });

  const importPattern =
    /\b(?:import|export)\s+(?:[^"'`]*?\s+from\s*)?["'](?<specifier>[^"'`]+)["']|\bimport\s*\(\s*["'](?<dynamic>[^"'`]+)["']\s*\)/g;

  return rewrittenDefaultImports.replace(importPattern, (match, ...args) => {
    const groups = args.at(-1) as { specifier?: string; dynamic?: string } | undefined;
    const specifier = groups?.specifier ?? groups?.dynamic;
    if (specifier === undefined || !specifier.endsWith(".vue")) {
      return match;
    }

    if (!isRelativeSpecifier(specifier)) {
      return match;
    }

    return match.replace(specifier, `${specifier.slice(0, -4)}.mjs`);
  });
}

function isRelativeSpecifier(specifier: string): boolean {
  return specifier.startsWith("./") || specifier.startsWith("../");
}

function resolveStyleSourcePath(documentPath: string, specifier: string | undefined): string | null {
  if (specifier === undefined) {
    return null;
  }

  const normalizedSpecifier = specifier.trim();
  if (normalizedSpecifier.length === 0) {
    return null;
  }

  if (!normalizedSpecifier.startsWith("./") && !normalizedSpecifier.startsWith("../")) {
    throw new Error(`Unsupported <style src> specifier '${normalizedSpecifier}'.`);
  }

  return resolve(dirname(documentPath), normalizedSpecifier);
}

function resolveCssModuleName(value: boolean | string | undefined): string | null {
  if (!value) {
    return null;
  }

  if (typeof value === "string" && value.trim().length > 0) {
    return value.trim();
  }

  return "$style";
}

function createCssModuleScopedName(localName: string, filename: string, scopeId: string): string {
  const fileStem = sanitizeCssIdentifier(basename(filename, extname(filename)));
  const localStem = sanitizeCssIdentifier(localName);
  const hash = createHash("sha256")
    .update(`${filename}\n${scopeId}\n${localName}`, "utf8")
    .digest("hex")
    .slice(0, 8);
  return `jz_${fileStem}_${localStem}_${hash}`;
}

function sanitizeCssIdentifier(value: string): string {
  const normalized = value.replace(/[^A-Za-z0-9_-]/g, "_");
  if (normalized.length === 0) {
    return "style";
  }

  return /^[A-Za-z_]/.test(normalized) ? normalized : `_${normalized}`;
}

function createScopeId(documentPath: string): string {
  return createHash("sha256").update(documentPath, "utf8").digest("hex").slice(0, 16);
}

function rewriteDefaultExport(scriptContent: string, localName: string): string {
  return /\bexport\s+default\b/.test(scriptContent)
    ? scriptContent.replace(/\bexport\s+default\b/, `const ${localName} =`)
    : `${scriptContent}\nconst ${localName} = {};`;
}

function requiresTypeScriptTranspile(scriptLang: string | undefined, scriptSetupLang: string | undefined): boolean {
  return isTypeScriptLanguage(scriptLang) || isTypeScriptLanguage(scriptSetupLang);
}

function isTypeScriptLanguage(value: string | undefined): boolean {
  return value === "ts" || value === "tsx";
}

function transpileTypeScriptModule(text: string, filename: string): {
  jsContent: string;
  diagnostics: string[];
} {
  const transpiled = ts.transpileModule(text, {
    fileName: filename,
    compilerOptions: {
      module: ts.ModuleKind.ESNext,
      target: ts.ScriptTarget.ES2022,
      sourceMap: false,
      inlineSourceMap: false
    },
    reportDiagnostics: true
  });

  return {
    jsContent: stripTrailingSourceMappingUrl(transpiled.outputText).trim(),
    diagnostics: (transpiled.diagnostics ?? []).map(formatTypeScriptDiagnostic)
  };
}

function stripTrailingSourceMappingUrl(text: string): string {
  return text.replace(/(?:\r?\n)?\/\/# sourceMappingURL=.*\s*$/u, "");
}

function formatTypeScriptDiagnostic(diagnostic: ts.Diagnostic): string {
  const message = ts.flattenDiagnosticMessageText(diagnostic.messageText, "\n");
  if (diagnostic.file === undefined || diagnostic.start === undefined) {
    return message;
  }

  const position = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start);
  return `${diagnostic.file.fileName}(${position.line + 1},${position.character + 1}): ${message}`;
}

function formatCompilerMessage(error: unknown): string {
  if (typeof error === "string") {
    return error;
  }

  if (error instanceof Error) {
    return error.message;
  }

  if (typeof error === "object" && error !== null && "message" in error && typeof error.message === "string") {
    return error.message;
  }

  return String(error);
}

export type { PreparedWorkspace };
