import { createHash } from "node:crypto";
import { basename, dirname, extname, join, resolve } from "node:path";
import { compileScript, compileStyleAsync, compileTemplate, parse } from "npm:@vue/compiler-sfc@3.5.21";
import ts from "npm:typescript@5.9.3";

export type RazorVueSfcBridgeMode = "browser" | "ssr";

export type RazorVueSfcBridgeManifest = {
  AssemblyName?: string;
  GeneratedAtUtc?: string;
  Modules: RazorVueSfcBridgeManifestModule[];
};

export type RazorVueSfcBridgeManifestModule = {
  ComponentId: string;
  ComponentName: string;
  RelativeModulePath: string;
};

export type RazorVueSfcBridgeOptions = {
  hostJazorRoot: string;
  outputRoot: string;
  manifestPath?: string;
  manifest?: RazorVueSfcBridgeManifest;
  mode?: RazorVueSfcBridgeMode;
  production?: boolean;
  clean?: boolean;
  writeResultPath?: string | null;
};

export type RazorVueSfcBridgeResult = {
  manifestPath: string;
  hostJazorRoot: string;
  outputRoot: string;
  mode: RazorVueSfcBridgeMode;
  production: boolean;
  modules: RazorVueSfcBridgeModuleResult[];
};

export type RazorVueSfcBridgeModuleResult = {
  componentId: string;
  componentName: string;
  exportName: string;
  relativeModulePath: string;
  relativeOutputPath: string;
  outputPath: string;
  cssOutputPath: string | null;
};

type CompileModuleOptions = {
  sourcePath: string;
  outputPath: string;
  production: boolean;
  includeCssImports: boolean;
  componentExportName: string;
  relativeModulePath: string;
  componentExportNamesByRelativePath: ReadonlyMap<string, string>;
};

type TextReplacement = {
  start: number;
  end: number;
  text: string;
};

const defaultManifestFileName = "jazor-manifest-razorvue.json";

export async function compileRazorVueSfcBridgeModules(
  options: RazorVueSfcBridgeOptions
): Promise<RazorVueSfcBridgeResult> {
  const normalizedOptions = normalizeOptions(options);
  const manifest = normalizedOptions.manifest ?? await readJson<RazorVueSfcBridgeManifest>(normalizedOptions.manifestPath);
  validateManifest(manifest, normalizedOptions.manifestPath);

  const sortedModules = [...manifest.Modules].sort((left, right) =>
    normalizeRelativeModulePath(left.RelativeModulePath).localeCompare(
      normalizeRelativeModulePath(right.RelativeModulePath),
      "en"
    ));
  const componentExportNamesByRelativePath = new Map<string, string>();
  for (const module of sortedModules) {
    componentExportNamesByRelativePath.set(
      normalizeRelativeModulePath(module.RelativeModulePath),
      resolveRazorVueSfcBridgeComponentExportName(module)
    );
  }

  if (normalizedOptions.clean && isSameOrAncestorDirectory(normalizedOptions.outputRoot, normalizedOptions.hostJazorRoot)) {
    throw new Error(
      `RazorVue SFC bridge output directory '${normalizedOptions.outputRoot}' cannot be the host output root or one of its parent directories when clean is enabled.`
    );
  }

  if (normalizedOptions.clean) {
    await emptyDirectory(normalizedOptions.outputRoot);
  } else {
    await ensureDirectory(normalizedOptions.outputRoot);
  }

  const moduleResults: RazorVueSfcBridgeModuleResult[] = [];
  for (const module of sortedModules) {
    const relativeModulePath = normalizeRelativeModulePath(module.RelativeModulePath);
    const sourcePath = resolve(normalizedOptions.hostJazorRoot, relativeModulePath);
    const relativeOutputPath = replaceExtension(relativeModulePath, ".mjs");
    const outputPath = resolve(normalizedOptions.outputRoot, relativeOutputPath);
    const componentExportName = resolveRazorVueSfcBridgeComponentExportName(module);
    const cssOutputPath = replaceExtension(outputPath, ".css");

    await compileVueModule({
      sourcePath,
      outputPath,
      production: normalizedOptions.production,
      includeCssImports: normalizedOptions.mode === "browser",
      componentExportName,
      relativeModulePath,
      componentExportNamesByRelativePath
    });

    moduleResults.push({
      componentId: module.ComponentId,
      componentName: module.ComponentName,
      exportName: componentExportName,
      relativeModulePath,
      relativeOutputPath,
      outputPath,
      cssOutputPath: await fileExists(cssOutputPath) ? cssOutputPath : null
    });
  }

  const result: RazorVueSfcBridgeResult = {
    manifestPath: normalizedOptions.manifestPath,
    hostJazorRoot: normalizedOptions.hostJazorRoot,
    outputRoot: normalizedOptions.outputRoot,
    mode: normalizedOptions.mode,
    production: normalizedOptions.production,
    modules: moduleResults
  };

  if (normalizedOptions.writeResultPath !== null) {
    await writeText(normalizedOptions.writeResultPath, `${JSON.stringify(toResultDocument(result), null, 2)}\n`);
  }

  return result;
}

export function resolveRazorVueSfcBridgeComponentExportName(module: RazorVueSfcBridgeManifestModule): string {
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

async function compileVueModule(options: CompileModuleOptions): Promise<void> {
  const sourceText = normalizeLineEndings(await readText(options.sourcePath));
  const parsed = parse(sourceText, { filename: options.sourcePath });
  const diagnostics = parsed.errors.map(formatCompilerMessage);
  const { descriptor } = parsed;
  const scopeId = createScopeId(options.sourcePath);
  const hasScopedStyles = descriptor.styles.some((style) => style.scoped);

  let bindingMetadata: ReturnType<typeof compileScript>["bindings"] | undefined;
  let scriptContent = "const _sfc_main = {};";

  if (descriptor.scriptSetup !== null) {
    try {
      const compiledScript = compileScript(descriptor, {
        id: scopeId,
        isProd: options.production,
        genDefaultAs: "_sfc_main"
      });
      bindingMetadata = compiledScript.bindings;
      scriptContent = compiledScript.content.trim();
      if (requiresTypeScriptTranspile(descriptor.script?.lang, descriptor.scriptSetup.lang)) {
        const transpiled = transpileTypeScriptModule(scriptContent, basename(options.sourcePath));
        scriptContent = transpiled.jsContent;
        diagnostics.push(...transpiled.diagnostics);
      }
    } catch (error) {
      diagnostics.push(`Failed to compile <script setup>: ${formatCompilerMessage(error)}`);
    }
  } else if (descriptor.script !== null) {
    scriptContent = rewriteDefaultExport(descriptor.script.content.trim(), "_sfc_main");
    if (isTypeScriptLanguage(descriptor.script.lang)) {
      const transpiled = transpileTypeScriptModule(scriptContent, basename(options.sourcePath));
      scriptContent = transpiled.jsContent;
      diagnostics.push(...transpiled.diagnostics);
    }
  }

  let templateContent = "";
  if (descriptor.template !== null) {
    try {
      const compiledTemplate = compileTemplate({
        source: descriptor.template.content,
        filename: options.sourcePath,
        id: scopeId,
        scoped: hasScopedStyles,
        isProd: options.production,
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
      const styleSourcePath = resolveStyleSourcePath(options.sourcePath, styleBlock.src);
      const styleSourceText = styleSourcePath === null
        ? normalizeLineEndings(styleBlock.content)
        : normalizeLineEndings(await readText(styleSourcePath));
      const moduleName = resolveCssModuleName(styleBlock.module);
      const compiledStyle = await compileStyleAsync({
        source: styleSourceText,
        filename: styleSourcePath ?? options.sourcePath,
        id: `data-v-${scopeId}`,
        scoped: styleBlock.scoped,
        isProd: options.production,
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
        `Failed to compile RazorVue SFC '${options.sourcePath}' for the RazorVue SFC bridge.`,
        ...diagnostics.map((diagnostic) => `- ${diagnostic}`)
      ].join("\n")
    );
  }

  const finalParts: string[] = [];
  const cssOutputPath = replaceExtension(options.outputPath, ".css");
  if (cssParts.length > 0) {
    await writeText(cssOutputPath, `${cssParts.join("\n\n")}\n`);
    if (options.includeCssImports) {
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

  finalParts.push(`export { _sfc_main as ${options.componentExportName} };`);
  const moduleContent = rewriteRelativeVueSpecifiers(
    finalParts.filter((part) => part.trim().length > 0).join("\n\n"),
    options.relativeModulePath,
    options.componentExportNamesByRelativePath
  );
  await writeText(options.outputPath, `${moduleContent}\n`);
}

function rewriteRelativeVueSpecifiers(
  code: string,
  relativeModulePath: string,
  componentExportNamesByRelativePath: ReadonlyMap<string, string>
): string {
  const sourceFile = ts.createSourceFile("razorvue-sfc-bridge.mjs", code, ts.ScriptTarget.Latest, true, ts.ScriptKind.JS);
  const replacements: TextReplacement[] = [];
  const currentDirectory = dirname(normalizeRelativeModulePath(relativeModulePath)).replaceAll("\\", "/");

  const visit = (node: ts.Node): void => {
    if (ts.isImportDeclaration(node) && ts.isStringLiteral(node.moduleSpecifier)) {
      const specifier = node.moduleSpecifier.text;
      if (isRelativeVueSpecifier(specifier)) {
        const rewrittenSpecifier = replaceVueExtension(specifier);
        const targetRelativePath = normalizeRelativeModulePath(join(currentDirectory, specifier));
        const importedExportName = componentExportNamesByRelativePath.get(targetRelativePath);
        if (node.importClause?.name !== undefined && importedExportName !== undefined) {
          replacements.push({
            start: node.getStart(sourceFile),
            end: node.getEnd(),
            text: rewriteDefaultImportDeclaration(sourceFile, node, importedExportName, rewrittenSpecifier)
          });
        } else {
          replacements.push(replaceStringLiteralText(node.moduleSpecifier, rewrittenSpecifier));
        }
      }
    } else if (ts.isExportDeclaration(node) && node.moduleSpecifier !== undefined && ts.isStringLiteral(node.moduleSpecifier)) {
      const specifier = node.moduleSpecifier.text;
      if (isRelativeVueSpecifier(specifier)) {
        const rewrittenSpecifier = replaceVueExtension(specifier);
        const targetRelativePath = normalizeRelativeModulePath(join(currentDirectory, specifier));
        const importedExportName = componentExportNamesByRelativePath.get(targetRelativePath);
        const rewrittenDefaultReExport = importedExportName === undefined
          ? null
          : tryRewriteDefaultReExportDeclaration(sourceFile, node, importedExportName, rewrittenSpecifier);

        replacements.push(rewrittenDefaultReExport ?? replaceStringLiteralText(node.moduleSpecifier, rewrittenSpecifier));
      }
    } else if (isStaticDynamicImport(node)) {
      const [argument] = node.arguments;
      if (ts.isStringLiteral(argument) && isRelativeVueSpecifier(argument.text)) {
        replacements.push(replaceStringLiteralText(argument, replaceVueExtension(argument.text)));
      }
    }

    ts.forEachChild(node, visit);
  };

  visit(sourceFile);
  return applyTextReplacements(code, replacements);
}

function rewriteDefaultImportDeclaration(
  sourceFile: ts.SourceFile,
  node: ts.ImportDeclaration,
  importedExportName: string,
  rewrittenSpecifier: string
): string {
  const importClause = node.importClause;
  const localName = importClause?.name?.text;
  if (localName === undefined) {
    throw new Error("Cannot rewrite a default import declaration without a local default binding.");
  }

  const defaultImport = importedExportName === localName
    ? importedExportName
    : `${importedExportName} as ${localName}`;
  const namedBindings = importClause.namedBindings;
  if (namedBindings === undefined) {
    return `import { ${defaultImport} } from ${JSON.stringify(rewrittenSpecifier)};`;
  }

  if (ts.isNamespaceImport(namedBindings)) {
    return [
      `import { ${defaultImport} } from ${JSON.stringify(rewrittenSpecifier)};`,
      `import * as ${namedBindings.name.text} from ${JSON.stringify(rewrittenSpecifier)};`
    ].join("\n");
  }

  const existingNamedImports = trimNamedImportBraces(namedBindings.getText(sourceFile));
  const combinedNamedImports = existingNamedImports.length === 0
    ? defaultImport
    : `${defaultImport}, ${existingNamedImports}`;
  return `import { ${combinedNamedImports} } from ${JSON.stringify(rewrittenSpecifier)};`;
}

function tryRewriteDefaultReExportDeclaration(
  sourceFile: ts.SourceFile,
  node: ts.ExportDeclaration,
  importedExportName: string,
  rewrittenSpecifier: string
): TextReplacement | null {
  if (node.exportClause === undefined || !ts.isNamedExports(node.exportClause)) {
    return null;
  }

  let changed = false;
  const specifiers = node.exportClause.elements.map((specifier) => {
    if (specifier.propertyName?.text === "default") {
      changed = true;
      return importedExportName === specifier.name.text
        ? importedExportName
        : `${importedExportName} as ${specifier.name.text}`;
    }

    if (specifier.propertyName === undefined && specifier.name.text === "default") {
      changed = true;
      return `${importedExportName} as default`;
    }

    return specifier.getText(sourceFile);
  });

  if (!changed) {
    return null;
  }

  return {
    start: node.getStart(sourceFile),
    end: node.getEnd(),
    text: `export { ${specifiers.join(", ")} } from ${JSON.stringify(rewrittenSpecifier)};`
  };
}

function trimNamedImportBraces(text: string): string {
  const trimmed = text.trim();
  return trimmed.startsWith("{") && trimmed.endsWith("}")
    ? trimmed.slice(1, -1).trim()
    : trimmed;
}

function isStaticDynamicImport(node: ts.Node): node is ts.CallExpression {
  return ts.isCallExpression(node) &&
    node.expression.kind === ts.SyntaxKind.ImportKeyword &&
    node.arguments.length === 1;
}

function replaceStringLiteralText(literal: ts.StringLiteral, text: string): TextReplacement {
  return {
    start: literal.getStart() + 1,
    end: literal.getEnd() - 1,
    text
  };
}

function applyTextReplacements(text: string, replacements: TextReplacement[]): string {
  if (replacements.length === 0) {
    return text;
  }

  const ordered = [...replacements].sort((left, right) => right.start - left.start);
  let result = text;
  let previousStart = text.length + 1;
  for (const replacement of ordered) {
    if (replacement.start < 0 || replacement.end < replacement.start || replacement.end > text.length) {
      throw new Error(`Invalid text replacement range ${replacement.start}..${replacement.end}.`);
    }

    if (replacement.end > previousStart) {
      throw new Error("Overlapping text replacements were produced while rewriting RazorVue SFC bridge imports.");
    }

    result = `${result.slice(0, replacement.start)}${replacement.text}${result.slice(replacement.end)}`;
    previousStart = replacement.start;
  }

  return result;
}

function isRelativeVueSpecifier(specifier: string): boolean {
  return (specifier.startsWith("./") || specifier.startsWith("../")) && specifier.endsWith(".vue");
}

function replaceVueExtension(specifier: string): string {
  return `${specifier.slice(0, -4)}.mjs`;
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

function normalizeOptions(options: RazorVueSfcBridgeOptions): Required<RazorVueSfcBridgeOptions> {
  const hostJazorRoot = resolveRequiredPath(options.hostJazorRoot, "hostJazorRoot");
  const outputRoot = resolveRequiredPath(options.outputRoot, "outputRoot");
  const manifestPath = resolve(options.manifestPath?.trim() ?? join(hostJazorRoot, defaultManifestFileName));
  const mode = options.mode ?? "browser";

  if (mode !== "browser" && mode !== "ssr") {
    throw new Error(`Unsupported RazorVue SFC bridge mode '${mode}'. Expected 'browser' or 'ssr'.`);
  }

  return {
    hostJazorRoot,
    outputRoot,
    manifestPath,
    manifest: options.manifest,
    mode,
    production: options.production ?? true,
    clean: options.clean ?? true,
    writeResultPath: options.writeResultPath === undefined || options.writeResultPath === null
      ? null
      : resolve(options.writeResultPath)
  };
}

function resolveRequiredPath(value: string, name: string): string {
  if (value.trim().length === 0) {
    throw new Error(`RazorVue SFC bridge option '${name}' is required.`);
  }

  return resolve(value);
}

function validateManifest(manifest: RazorVueSfcBridgeManifest, manifestPath: string): void {
  if (!Array.isArray(manifest.Modules)) {
    throw new Error(`RazorVue SFC bridge manifest '${manifestPath}' does not contain a Modules array.`);
  }
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

function isSameOrAncestorDirectory(candidateDirectory: string, targetDirectory: string): boolean {
  const candidate = ensureTrailingSlash(resolve(candidateDirectory).replaceAll("\\", "/"));
  const target = ensureTrailingSlash(resolve(targetDirectory).replaceAll("\\", "/"));
  return target.startsWith(candidate);
}

function ensureTrailingSlash(path: string): string {
  return path.endsWith("/") ? path : `${path}/`;
}

function isJavaScriptIdentifier(value: string): boolean {
  return /^[$_\p{ID_Start}][$\u200C\u200D\p{ID_Continue}]*$/u.test(value);
}

async function emptyDirectory(path: string): Promise<void> {
  await Deno.remove(path, { recursive: true }).catch((error: unknown) => {
    if (!(error instanceof Deno.errors.NotFound)) {
      throw error;
    }
  });
  await Deno.mkdir(path, { recursive: true });
}

async function ensureDirectory(path: string): Promise<void> {
  await Deno.mkdir(path, { recursive: true });
}

async function readText(path: string): Promise<string> {
  try {
    return await Deno.readTextFile(path);
  } catch (error) {
    if (error instanceof Deno.errors.NotFound) {
      throw new Error(`RazorVue SFC bridge input file was not found: ${path}`);
    }

    throw error;
  }
}

async function writeText(path: string, content: string): Promise<void> {
  await ensureDirectory(dirname(path));
  await Deno.writeTextFile(path, content);
}

async function fileExists(path: string): Promise<boolean> {
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

async function readJson<T>(path: string): Promise<T> {
  const text = await readText(path);
  try {
    return JSON.parse(text) as T;
  } catch (error) {
    throw new Error(`RazorVue SFC bridge failed to parse JSON '${path}': ${formatCompilerMessage(error)}`);
  }
}

function toResultDocument(result: RazorVueSfcBridgeResult): unknown {
  return {
    ManifestPath: result.manifestPath,
    HostJazorRoot: result.hostJazorRoot,
    OutputRoot: result.outputRoot,
    Mode: result.mode,
    Production: result.production,
    Modules: result.modules.map((module) => ({
      ComponentId: module.componentId,
      ComponentName: module.componentName,
      ExportName: module.exportName,
      RelativeModulePath: module.relativeModulePath,
      RelativeOutputPath: module.relativeOutputPath,
      OutputPath: module.outputPath,
      CssOutputPath: module.cssOutputPath
    }))
  };
}

async function runCli(args: string[]): Promise<void> {
  const options = parseCliOptions(args);
  const result = await compileRazorVueSfcBridgeModules(options);
  console.log(`razorvue-sfc-bridge modules=${result.modules.length} mode=${result.mode} out=${result.outputRoot}`);
}

function parseCliOptions(args: string[]): RazorVueSfcBridgeOptions {
  const values = new Map<string, string>();
  for (let index = 0; index < args.length; index++) {
    const argument = args[index];
    if (!argument.startsWith("--")) {
      throw new Error(`Unexpected RazorVue SFC bridge argument '${argument}'.`);
    }

    if (index + 1 >= args.length) {
      throw new Error(`Missing value for RazorVue SFC bridge argument '${argument}'.`);
    }

    values.set(argument, args[++index]);
  }

  const hostJazorRoot = readRequiredCliValue(values, "--host-root");
  const outputRoot = readRequiredCliValue(values, "--out");
  const mode = values.get("--mode") as RazorVueSfcBridgeMode | undefined;

  return {
    hostJazorRoot,
    outputRoot,
    manifestPath: values.get("--manifest"),
    mode,
    production: parseOptionalBoolean(values.get("--production"), true, "--production"),
    clean: parseOptionalBoolean(values.get("--clean"), true, "--clean"),
    writeResultPath: values.get("--write-result") ?? null
  };
}

function readRequiredCliValue(values: ReadonlyMap<string, string>, name: string): string {
  const value = values.get(name)?.trim();
  if (value === undefined || value.length === 0) {
    throw new Error(`Missing required RazorVue SFC bridge argument '${name}'.`);
  }

  return value;
}

function parseOptionalBoolean(value: string | undefined, defaultValue: boolean, name: string): boolean {
  if (value === undefined || value.trim().length === 0) {
    return defaultValue;
  }

  const normalized = value.trim().toLowerCase();
  if (normalized === "true") {
    return true;
  }

  if (normalized === "false") {
    return false;
  }

  throw new Error(`RazorVue SFC bridge argument '${name}' must be 'true' or 'false'.`);
}

if (import.meta.main) {
  await runCli(Deno.args);
}
