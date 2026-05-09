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
  buildRoot: string;
  browserGeneratedRoot: string;
  ssrGeneratedRoot: string;
  distRoot: string;
  clientEntryPath: string;
  ssrEntryPath: string;
  assetsDirectory: string;
};

const consumerRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const rootComponentId = "Todo.Library.TodoApp";

export async function prepareWorkspace(production: boolean): Promise<PreparedWorkspace> {
  const hostJazorRoot = resolvePathFromEnvironment("RAZORVUE_HOST_JAZOR_ROOT", resolve(consumerRoot, "..", "Todo.Host", "wwwroot", "jazor"));
  const buildRoot = resolvePathFromEnvironment("RAZORVUE_BUILD_ROOT", resolve(consumerRoot, ".deno-build"));
  const browserGeneratedRoot = resolve(buildRoot, "generated-browser");
  const ssrGeneratedRoot = resolve(buildRoot, "generated-ssr");
  const distRoot = resolvePathFromEnvironment("RAZORVUE_DIST_ROOT", resolve(consumerRoot, "dist"));
  const manifestPath = resolve(hostJazorRoot, "jazor-manifest-razorvue.json");
  const hostRequirementsModulePath = resolve(hostJazorRoot, "__jazor", "razorvue-host.mjs");

  if (!(await fileExists(manifestPath))) {
    throw new Error(`RazorVue manifest was not found for the Deno consumer: ${manifestPath}`);
  }

  if (!(await fileExists(hostRequirementsModulePath))) {
    throw new Error(`RazorVue host requirements module was not found for the Deno consumer: ${hostRequirementsModulePath}`);
  }

  const manifest = await readJson<RazorVueManifest>(manifestPath);
  await emptyDirectory(buildRoot);
  await ensureDirectory(browserGeneratedRoot);
  await ensureDirectory(ssrGeneratedRoot);

  let browserRootModuleOutputPath: string | null = null;
  let ssrRootModuleOutputPath: string | null = null;
  for (const module of [...manifest.Modules].sort((left, right) =>
    left.RelativeModulePath.localeCompare(right.RelativeModulePath, "en"))) {
    const sourcePath = resolve(hostJazorRoot, module.RelativeModulePath);
    const browserOutputPath = resolve(browserGeneratedRoot, replaceExtension(module.RelativeModulePath, ".mjs"));
    const ssrOutputPath = resolve(ssrGeneratedRoot, replaceExtension(module.RelativeModulePath, ".mjs"));
    await compileVueModule(sourcePath, browserOutputPath, production, true);
    await compileVueModule(sourcePath, ssrOutputPath, production, false);

    if (module.ComponentId === rootComponentId || module.ComponentName === "TodoApp") {
      browserRootModuleOutputPath = browserOutputPath;
      ssrRootModuleOutputPath = ssrOutputPath;
    }
  }

  if (browserRootModuleOutputPath === null || ssrRootModuleOutputPath === null) {
    throw new Error(`RazorVue manifest did not contain root component '${rootComponentId}'.`);
  }

  const clientEntryPath = resolve(buildRoot, "client-entry.mjs");
  const ssrEntryPath = resolve(buildRoot, "ssr-entry.mjs");
  const assetsDirectory = resolve(distRoot, "assets");
  await writeText(
    clientEntryPath,
    [
      `import TodoApp from ${JSON.stringify(toModuleSpecifier(clientEntryPath, browserRootModuleOutputPath))};`,
      `import { razorVueHostRequirements } from ${JSON.stringify(toModuleSpecifier(clientEntryPath, hostRequirementsModulePath))};`,
      `import { mountTodoApp } from ${JSON.stringify(toModuleSpecifier(clientEntryPath, resolve(consumerRoot, "src", "runtime-client.js")))};`,
      "",
      "mountTodoApp(TodoApp, razorVueHostRequirements);",
      ""
    ].join("\n"));
  await writeText(
    ssrEntryPath,
    [
      `import TodoApp from ${JSON.stringify(toModuleSpecifier(ssrEntryPath, ssrRootModuleOutputPath))};`,
      `import { razorVueHostRequirements } from ${JSON.stringify(toModuleSpecifier(ssrEntryPath, hostRequirementsModulePath))};`,
      `import { runSsrSmoke } from ${JSON.stringify(toModuleSpecifier(ssrEntryPath, resolve(consumerRoot, "src", "runtime-ssr.js")))};`,
      "",
      "export { runSsrSmoke };",
      "export async function executeSsrSmoke() {",
      "  return await runSsrSmoke(TodoApp, razorVueHostRequirements);",
      "}",
      ""
    ].join("\n"));

  return {
    consumerRoot,
    hostJazorRoot,
    buildRoot,
    browserGeneratedRoot,
    ssrGeneratedRoot,
    distRoot,
    clientEntryPath,
    ssrEntryPath,
    assetsDirectory
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

export function createImportUrl(path: string): string {
  return `${pathToFileURL(path).href}?t=${Date.now().toString(36)}`;
}

function resolvePathFromEnvironment(environmentVariableName: string, defaultPath: string): string {
  const configuredPath = Deno.env.get(environmentVariableName)?.trim();
  return configuredPath === undefined || configuredPath.length === 0
    ? defaultPath
    : resolve(consumerRoot, configuredPath);
}

function normalizeLineEndings(value: string): string {
  return value.replace(/\r\n?/g, "\n");
}

function replaceExtension(path: string, extension: string): string {
  return path.slice(0, path.length - extname(path).length) + extension;
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
  includeCssImports: boolean
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
        `Failed to compile RazorVue SFC '${sourcePath}' for the Deno pipeline.`,
        ...diagnostics.map((diagnostic) => `- ${diagnostic}`)
      ].join("\n"));
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

  finalParts.push("export default _sfc_main;");
  const moduleContent = rewriteRelativeVueSpecifiers(finalParts.filter((part) => part.trim().length > 0).join("\n\n"));
  await writeText(outputPath, `${moduleContent}\n`);
}

function rewriteRelativeVueSpecifiers(code: string): string {
  const importPattern =
    /\b(?:import|export)\s+(?:[^"'`]*?\s+from\s*)?["'](?<specifier>[^"'`]+)["']|\bimport\s*\(\s*["'](?<dynamic>[^"'`]+)["']\s*\)/g;

  return code.replace(importPattern, (match, ...args) => {
    const groups = args.at(-1) as { specifier?: string; dynamic?: string } | undefined;
    const specifier = groups?.specifier ?? groups?.dynamic;
    if (specifier === undefined || !specifier.endsWith(".vue")) {
      return match;
    }

    if (!specifier.startsWith("./") && !specifier.startsWith("../")) {
      return match;
    }

    return match.replace(specifier, `${specifier.slice(0, -4)}.mjs`);
  });
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
