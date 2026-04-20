import * as volarLanguageCore from "npm:@volar/language-core@2.4.28";
import * as volarTypeScript from "npm:@volar/typescript@2.4.28";
import ts from "npm:typescript@5.8.3";
import * as vueCompilerSfc from "npm:@vue/compiler-sfc@3.5.13";
import * as volarTypeScriptService from "npm:volar-service-typescript@0.0.70";
import * as vueLanguageCore from "npm:@vue/language-core@3.2.6";
import * as vueLanguageService from "npm:@vue/language-service@3.2.6";
import { URI } from "npm:vscode-uri@3.1.0";

type Position = {
  line: number;
  character: number;
};

type Range = {
  start: Position;
  end: Position;
};

type RequestEnvelope = {
  id: string;
  method: string;
  payload: {
    documentPath: string;
    text: string;
    sfcText?: string;
    filename?: string;
    isProduction?: boolean;
    position?: Position;
    range?: Range;
    includeDeclaration?: boolean;
    newName?: string;
    frontendContext?: FrontendSemanticContext | null;
    frontendArtifacts?: FrontendArtifactRecord[] | null;
  };
};

type ResponseEnvelope = {
  id: string;
  success: boolean;
  result?: unknown;
  error?: string;
};

type FrontendDocumentKind = "jazor" | "vue" | "typescript" | "javascript" | "css" | "html" | "unknown";
type ScriptLanguage = "ts" | "js";
type ScriptSymbolKind = "const" | "let" | "var" | "function" | "class" | "import";
type TemplateSymbol = { name: string; range: { start: Position; end: Position } };
type SourceLineRange = { sourceStartLine: number; sourceLineCount: number };
type SourceMappedPart = {
  content: string;
  sourceRange?: SourceLineRange;
  sourceLineMappings?: ReadonlyArray<number | null>;
  sourceSegments?: ReadonlyArray<SourceMapSegment>;
};
type SourceMapSegment = {
  generatedLine: number;
  generatedColumn: number;
  sourceLine: number;
  sourceColumn: number;
};
type SfcBlockLike = { loc: { start: { line: number }; end: { line: number } } };
type CompiledStyleFragment = {
  cssContent: string;
  sourcePath?: string | null;
  sourceLineStart?: number | null;
  sourceLineCount?: number | null;
};
type FrontendDocumentSnapshot = {
  documentPath: string;
  documentKind: string | number | null | undefined;
  text: string;
  version?: string | null;
};
type FrontendSemanticContext = {
  contextKind: string;
  relatedDocuments: FrontendDocumentSnapshot[];
  properties?: Record<string, string> | null;
};
type FrontendArtifactRecord = {
  artifactName: string;
  artifactKind: string;
  content: string;
  contentHash?: string | null;
};
type FrontendSummaryArtifact = {
  documentPath?: string;
  documentKind?: string;
  lineCount?: number;
  importCount?: number;
  importedSources?: string[];
  exportedSymbols?: string[];
  referencedComponents?: string[];
  hasScriptSetup?: boolean;
};
type FrontendComponent = {
  componentName: string;
  absolutePath: string;
  importPath: string;
  source: "metadata" | "disk";
  summary?: FrontendSummaryArtifact | null;
};

type ScriptSymbol = {
  name: string;
  kind: ScriptSymbolKind;
  range: { start: Position; end: Position };
  scriptRange: { start: Position; end: Position };
  detail: string;
  exportedName?: string;
  importedName?: string;
  isDefaultImport?: boolean;
  isExported?: boolean;
  isDefaultExport?: boolean;
  importPath?: string;
  resolvedImportPath?: string;
};

type ScriptContext = {
  sourceDocumentPath: string;
  sourceText: string;
  scriptText: string;
  contentStartOffset: number;
  scriptRange: { start: Position; end: Position };
  scriptLanguage: ScriptLanguage;
  symbols: ScriptSymbol[];
};

type ImportedScriptTarget = {
  context: ScriptContext;
  symbol: ScriptSymbol;
};

type TypeScriptProject = {
  context: ScriptContext;
  entryFilePath: string;
  languageService: ts.LanguageService;
  virtualContexts: Map<string, ScriptContext>;
};

type VolarServiceContext = {
  documentPath: string;
  serviceDocumentPath: string;
  documentUri: URI;
  documentUriText: string;
  hostDocumentUri: string;
  service: ReturnType<typeof vueLanguageService.createLanguageService>;
  dispose: () => void;
};

const encoder = new TextEncoder();
const volarUnhandled = Symbol("volarUnhandled");

async function main(): Promise<void> {
  const decoder = new TextDecoder();
  let buffered = "";

  for await (const chunk of Deno.stdin.readable) {
    buffered += decoder.decode(chunk, { stream: true });
    let newlineIndex = buffered.indexOf("\n");
    while (newlineIndex >= 0) {
      const line = buffered.slice(0, newlineIndex).trim();
      buffered = buffered.slice(newlineIndex + 1);
      if (line.length > 0) {
        const response = await handleLine(line);
        await Deno.stdout.write(encoder.encode(JSON.stringify(response) + "\n"));
      }

      newlineIndex = buffered.indexOf("\n");
    }
  }
}

async function handleLine(line: string): Promise<ResponseEnvelope> {
  let request: RequestEnvelope | undefined;
  try {
    request = JSON.parse(line) as RequestEnvelope;
    const result = await dispatch(request.method, request.payload);
    return {
      id: request.id,
      success: true,
      result,
    };
  } catch (error) {
    return {
      id: request?.id ?? "",
      success: false,
      error: error instanceof Error ? error.message : String(error),
    };
  }
}

async function dispatch(method: string, payload: RequestEnvelope["payload"]): Promise<unknown> {
  switch (method) {
    case "compile/sfc":
      assertString(method, "documentPath", payload.documentPath);
      assertString(method, "sfcText", payload.sfcText);
      assertString(method, "filename", payload.filename);
      return await compileVueSfc(
        payload.documentPath,
        payload.sfcText,
        payload.filename,
        payload.isProduction === true,
      );
    case "compile/ts":
      assertString(method, "documentPath", payload.documentPath);
      assertString(method, "text", payload.text);
      assertString(method, "filename", payload.filename);
      return compileTypeScript(payload.documentPath, payload.text, payload.filename);
    case "compile/css-module":
      assertString(method, "documentPath", payload.documentPath);
      assertString(method, "text", payload.text);
      assertString(method, "filename", payload.filename);
      return await compileCssModule(
        payload.documentPath,
        payload.text,
        payload.filename,
        payload.isProduction === true,
      );
    case "template/diagnostics":
      return await getDiagnostics(
        payload.documentPath,
        payload.text,
        payload.frontendContext,
        payload.frontendArtifacts,
      );
    case "template/completion":
      assertPosition(method, payload.position);
      return await getCompletionItems(
        payload.documentPath,
        payload.text,
        payload.position,
        payload.frontendContext,
        payload.frontendArtifacts,
      );
    case "template/hover":
      assertPosition(method, payload.position);
      return await getHover(
        payload.documentPath,
        payload.text,
        payload.position,
        payload.frontendContext,
        payload.frontendArtifacts,
      );
    case "template/definition":
      assertPosition(method, payload.position);
      return await getDefinition(
        payload.documentPath,
        payload.text,
        payload.position,
        payload.frontendContext,
        payload.frontendArtifacts,
      );
    case "template/implementation":
      assertPosition(method, payload.position);
      return await getImplementation(
        payload.documentPath,
        payload.text,
        payload.position,
        payload.frontendContext,
        payload.frontendArtifacts,
      );
    case "template/references":
      assertPosition(method, payload.position);
      return await getReferences(
        payload.documentPath,
        payload.text,
        payload.position,
        payload.includeDeclaration !== false,
        payload.frontendContext,
        payload.frontendArtifacts,
      );
    case "template/rename":
      assertPosition(method, payload.position);
      return await getRename(
        payload.documentPath,
        payload.text,
        payload.position,
        payload.newName ?? "",
        payload.frontendContext,
        payload.frontendArtifacts,
      );
    case "template/documentSymbols":
      return await getDocumentSymbols(payload.documentPath, payload.text);
    case "template/semanticTokens":
      return await getSemanticTokens(payload.documentPath, payload.text);
    case "template/documentLinks":
      return await getDocumentLinks(payload.documentPath, payload.text);
    case "template/inlayHints":
      assertRange(method, payload.range);
      return await getInlayHints(payload.documentPath, payload.text, payload.range);
    case "template/foldingRanges":
      return await getFoldingRanges(payload.documentPath, payload.text);
    default:
      throw new Error(`Unsupported method '${method}'.`);
  }
}

async function getDiagnostics(
  documentPath: string,
  text: string,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const diagnostics = await tryGetVueDiagnostics(documentPath, text);
    if (diagnostics !== volarUnhandled) {
      return diagnostics;
    }
  }

  const diagnostics: unknown[] = [];

  const documentKind = getFrontendDocumentKind(documentPath);
  const scriptContext = tryCreateScriptDocumentContext(documentPath, text);
  if (scriptContext !== null) {
    diagnostics.push(...getScriptDiagnostics(scriptContext));
  }

  if (documentKind === "jazor" || documentKind === "vue") {
    for (const symbol of findTemplateSymbols(text)) {
      if (resolveComponent(documentPath, symbol.name, frontendContext, frontendArtifacts) !== null) {
        continue;
      }

      diagnostics.push({
        range: symbol.range,
        severity: 2,
        code: "JAZORVUEFRONTEND001",
        source: "Jolt.Frontend",
        message: `Razor component '${symbol.name}' could not be resolved to a nearby Vue file.`,
      });
    }
  }

  return diagnostics;
}

async function getCompletionItems(
  documentPath: string,
  text: string,
  position: Position,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const items = await tryGetVueCompletionItems(documentPath, text, position);
    if (items !== volarUnhandled) {
      return items;
    }
  }

  const scriptContext = tryCreateScriptContext(documentPath, text, position);
  if (scriptContext !== null) {
    return getScriptCompletionItems(scriptContext, position);
  }

  const tagPrefix = getTagCompletionPrefix(text, position);
  if (tagPrefix === null) {
    return [];
  }

  return enumerateNearbyVueComponents(documentPath, frontendContext, frontendArtifacts)
    .filter((item) => item.componentName.toLowerCase().startsWith(tagPrefix.toLowerCase()))
    .map((item) => ({
      label: item.componentName,
      kind: 7,
      detail: item.importPath,
      documentation: item.source === "metadata"
        ? `Vue component exposed through Jolt frontend metadata at \`${item.importPath}\`.`
        : `Vue component discovered on disk at \`${item.importPath}\`.`,
    }));
}

function getTagCompletionPrefix(text: string, position: Position): string | null {
  const offset = toOffset(text, position);
  const prefix = text.slice(0, Math.min(offset, text.length));
  const match = prefix.match(/<\/?([A-Za-z0-9_]*)$/);
  return match === null ? null : match[1];
}

async function getHover(
  documentPath: string,
  text: string,
  position: Position,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Promise<unknown | null> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const hover = await tryGetVueHover(documentPath, text, position);
    if (hover !== volarUnhandled) {
      return hover;
    }
  }

  const scriptContext = tryCreateScriptContext(documentPath, text, position);
  if (scriptContext !== null) {
    return getScriptHover(scriptContext, position);
  }

  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return null;
  }

  const component = resolveComponent(documentPath, symbol.name, frontendContext, frontendArtifacts);
  if (component === null) {
    return null;
  }

  return {
    contents: {
      kind: "markdown",
      value: `\`${symbol.name}\` resolved from Razor markup to \`${component.importPath}\`\n\nkind: \`VueComponent\`\n\nsource: \`${component.source}\``,
    },
    range: symbol.range,
  };
}

async function getDefinition(
  documentPath: string,
  text: string,
  position: Position,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const locations = await tryGetVueDefinition(documentPath, text, position);
    if (locations !== volarUnhandled) {
      return locations;
    }
  }

  const scriptContext = tryCreateScriptContext(documentPath, text, position);
  if (scriptContext !== null) {
    return getScriptDefinition(scriptContext, position);
  }

  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return [];
  }

  const component = resolveComponent(documentPath, symbol.name, frontendContext, frontendArtifacts);
  if (component === null) {
    return [];
  }

  return [{
    uri: toDocumentUri(component.absolutePath),
    range: {
      start: { line: 0, character: 0 },
      end: { line: 0, character: 0 },
    },
  }];
}

async function getImplementation(
  documentPath: string,
  text: string,
  position: Position,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const locations = await tryGetVueImplementation(documentPath, text, position);
    if (locations !== volarUnhandled) {
      return locations;
    }
  }

  const scriptContext = tryCreateScriptContext(documentPath, text, position);
  if (scriptContext !== null) {
    return getScriptImplementation(scriptContext, position);
  }

  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return [];
  }

  const component = resolveComponent(documentPath, symbol.name, frontendContext, frontendArtifacts);
  if (component === null) {
    return [];
  }

  return [{
    uri: toDocumentUri(component.absolutePath),
    range: {
      start: { line: 0, character: 0 },
      end: { line: 0, character: 0 },
    },
  }];
}

async function getReferences(
  documentPath: string,
  text: string,
  position: Position,
  includeDeclaration: boolean,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const locations = await tryGetVueReferences(documentPath, text, position, includeDeclaration);
    if (locations !== volarUnhandled) {
      return locations;
    }
  }

  const scriptContext = tryCreateScriptContext(documentPath, text, position);
  if (scriptContext !== null) {
    return getScriptReferences(scriptContext, position, includeDeclaration);
  }

  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return [];
  }

  const component = resolveComponent(documentPath, symbol.name, frontendContext, frontendArtifacts);
  if (component === null) {
    return [];
  }

  const results: unknown[] = [];
  if (includeDeclaration) {
    results.push({
      uri: toDocumentUri(component.absolutePath),
      range: {
        start: { line: 0, character: 0 },
        end: { line: 0, character: 0 },
      },
    });
  }

  for (const range of findTemplateSymbolRanges(text, symbol.name)) {
    results.push({
      uri: toDocumentUri(documentPath),
      range,
    });
  }

  return dedupeLocations(results);
}

async function getRename(
  documentPath: string,
  text: string,
  position: Position,
  newName: string,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Promise<unknown | null> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const workspaceEdit = await tryGetVueRename(documentPath, text, position, newName);
    if (workspaceEdit !== volarUnhandled) {
      return workspaceEdit;
    }
  }

  const scriptContext = tryCreateScriptContext(documentPath, text, position);
  if (scriptContext !== null) {
    return getScriptRename(scriptContext, position, newName);
  }

  if (newName.trim().length === 0) {
    return null;
  }

  const references = await getReferences(
    documentPath,
    text,
    position,
    true,
    frontendContext,
    frontendArtifacts,
  ) as Array<{ uri: string; range: { start: Position } }>;
  if (references.length === 0) {
    return null;
  }

  const uri = toDocumentUri(documentPath);
  const edits = references
    .filter((item) => item.uri === uri)
    .map((item) => ({
      range: item.range,
      newText: newName,
    }))
    .sort((left, right) => {
      if (left.range.start.line !== right.range.start.line) {
        return right.range.start.line - left.range.start.line;
      }

      return right.range.start.character - left.range.start.character;
    });

  return {
    changes: {
      [uri]: edits,
    },
  };
}

async function getDocumentSymbols(documentPath: string, text: string): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const symbols = await tryGetVueDocumentSymbols(documentPath, text);
    if (symbols !== volarUnhandled) {
      return symbols;
    }
  }

  const scriptContext = tryCreateScriptDocumentContext(documentPath, text);
  const documentKind = getFrontendDocumentKind(documentPath);
  if (scriptContext !== null && documentKind !== "vue") {
    return getScriptDocumentSymbols(scriptContext);
  }

  const componentSymbols = findTemplateSymbols(text)
    .map((symbol) => ({
      name: symbol.name,
      kind: 5,
      range: symbol.range,
      selectionRange: symbol.range,
    }));
  const templateBlock = findTemplateBlock(text);
  const scriptSymbols = scriptContext === null
    ? []
    : getScriptDocumentSymbols(scriptContext);
  const results: unknown[] = [];

  if (templateBlock !== null) {
    const templateStart = toOffset(text, templateBlock.range.start);
    const templateEnd = toOffset(text, templateBlock.range.end);
    const children = componentSymbols.filter((symbol) => {
      const start = toOffset(text, symbol.selectionRange.start);
      const end = toOffset(text, symbol.selectionRange.end);
      return start >= templateStart && end <= templateEnd;
    });

    results.push({
      name: "Template",
      kind: 2,
      range: templateBlock.range,
      selectionRange: templateBlock.selectionRange,
      children: children.length === 0 ? undefined : children,
    });
  }

  if (scriptContext !== null) {
    results.push({
      name: "Script",
      kind: 2,
      range: scriptContext.scriptRange,
      selectionRange: scriptContext.scriptRange,
      children: scriptSymbols.length === 0 ? undefined : scriptSymbols,
    });
  }

  return results.length === 0 ? componentSymbols : results;
}

async function getSemanticTokens(documentPath: string, text: string): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) === "vue") {
    const tokens = await tryGetVueSemanticTokens(documentPath, text);
    if (tokens !== volarUnhandled) {
      return tokens;
    }
  }

  const tokens: unknown[] = [];
  const documentKind = getFrontendDocumentKind(documentPath);

  if (documentKind === "jazor" || documentKind === "vue") {
    for (const symbol of findTemplateSymbols(text)) {
      tokens.push(createSemanticToken(symbol.range, "class"));
    }

    for (const range of findDirectiveAttributeRanges(text)) {
      tokens.push(createSemanticToken(range, "keyword"));
    }

    const templateBlock = findTemplateBlock(text);
    if (templateBlock !== null) {
      tokens.push(createSemanticToken(templateBlock.selectionRange, "keyword"));
    }
  }

  const scriptContext = tryCreateScriptDocumentContext(documentPath, text);
  if (scriptContext !== null) {
    tokens.push(...getScriptSemanticTokens(scriptContext));
  }

  return dedupeSemanticTokens(tokens);
}

async function getDocumentLinks(documentPath: string, text: string): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) !== "vue") {
    return [];
  }

  const links = await tryGetVueDocumentLinks(documentPath, text);
  return links === volarUnhandled ? [] : links;
}

async function getInlayHints(
  documentPath: string,
  text: string,
  range: Range,
): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) !== "vue") {
    return [];
  }

  const hints = await tryGetVueInlayHints(documentPath, text, range);
  return hints === volarUnhandled ? [] : hints;
}

async function getFoldingRanges(
  documentPath: string,
  text: string,
): Promise<unknown[]> {
  if (getFrontendDocumentKind(documentPath) !== "vue") {
    return [];
  }

  const ranges = await tryGetVueFoldingRanges(documentPath, text);
  return ranges === volarUnhandled ? [] : ranges;
}

async function compileVueSfc(
  documentPath: string,
  sfcText: string,
  filename: string,
  isProduction: boolean,
): Promise<{
  jsContent: string;
  jsSourceMap: string | null;
  cssContent: string | null;
  styleFragments: CompiledStyleFragment[];
  diagnostics: string[];
  supportsHmr: boolean;
}> {
  const diagnostics: string[] = [];
  const { descriptor, errors } = vueCompilerSfc.parse(sfcText, { filename, sourceMap: true });
  for (const error of errors) {
    diagnostics.push(formatSfcCompilerMessage(error));
  }

  const scopeId = createVueScopeId(documentPath);
  const hasScopedStyles = descriptor.styles.some((style) => style.scoped);
  const scriptSourceRange = getCombinedSfcBlockSourceRange(descriptor.script, descriptor.scriptSetup);
  const templateSourceRange = getSfcBlockSourceRange(descriptor.template);
  const scopedStyleSourceRange = getCombinedSfcBlockSourceRange(
    ...descriptor.styles.filter((style) => style.scoped),
  );
  const moduleStyleSourceRange = getCombinedSfcBlockSourceRange(
    ...descriptor.styles.filter((style) => style.module),
  );
  const preferredSourceRange = scriptSourceRange ?? templateSourceRange ?? scopedStyleSourceRange;
  const jsParts: SourceMappedPart[] = [];
  const cssModules = new Map<string, Record<string, string>>();
  let bindingMetadata: ReturnType<typeof vueCompilerSfc.compileScript>["bindings"] | undefined;

  if (descriptor.scriptSetup !== null) {
    try {
      const compiledScript = vueCompilerSfc.compileScript(descriptor, {
        id: scopeId,
        isProd: isProduction,
        genDefaultAs: "_sfc_main",
      });
      bindingMetadata = compiledScript.bindings;
      let scriptContent = compiledScript.content.trim();
      let scriptSourceSegments = extractGeneratedSourceSegments(compiledScript.map);
      let scriptLineMappings = extractGeneratedLineMappings(compiledScript.map);
      if (requiresTypeScriptTranspile(descriptor)) {
        const transpiled = transpileTypeScriptModule(scriptContent, filename, true);
        diagnostics.push(...transpiled.diagnostics);
        scriptContent = transpiled.jsContent;
        scriptSourceSegments = chainGeneratedSourceSegments(transpiled.jsSourceMap, compiledScript.map);
        scriptLineMappings = chainGeneratedLineMappings(
          extractGeneratedLineMappings(transpiled.jsSourceMap),
          scriptLineMappings,
        );
      }

      if (scriptContent.length > 0) {
        jsParts.push({
          content: scriptContent,
          sourceRange: scriptSourceRange,
          sourceLineMappings: scriptLineMappings,
          sourceSegments: scriptSourceSegments,
        });
      }
    } catch (error) {
      diagnostics.push(`Failed to compile <script setup>: ${formatSfcCompilerMessage(error)}`);
      jsParts.push({ content: "const _sfc_main = {};" });
    }
  } else if (descriptor.script !== null) {
    let scriptContent = descriptor.script.content.trim();
    if (scriptContent.length > 0) {
      scriptContent = rewriteDefaultExport(scriptContent, "_sfc_main");
      if (isTypeScriptLanguage(descriptor.script.lang)) {
        const transpiled = transpileTypeScriptModule(scriptContent, filename, false);
        diagnostics.push(...transpiled.diagnostics);
        scriptContent = transpiled.jsContent;
      }

      jsParts.push({
        content: scriptContent,
        sourceRange: getSfcBlockSourceRange(descriptor.script),
      });
    } else {
      jsParts.push({ content: "const _sfc_main = {};" });
    }
  } else {
    jsParts.push({ content: "const _sfc_main = {};" });
  }

  if (descriptor.template !== null) {
    try {
      const templateCompiled = vueCompilerSfc.compileTemplate({
        source: descriptor.template.content,
        filename,
        id: scopeId,
        scoped: hasScopedStyles,
        isProd: isProduction,
        sourceMap: true,
        inMap: descriptor.template.map,
        compilerOptions: bindingMetadata === undefined
          ? undefined
          : {
            bindingMetadata,
          },
      });
      for (const error of templateCompiled.errors) {
        diagnostics.push(formatSfcCompilerMessage(error));
      }

      if (templateCompiled.code.trim().length > 0) {
        const templateSourceSegments = chainGeneratedSourceSegments(
          templateCompiled.map,
          descriptor.template.map,
        );
        jsParts.push({
          content: templateCompiled.code.trim(),
          sourceRange: templateSourceRange,
          sourceLineMappings: chainGeneratedLineMappings(
            extractGeneratedLineMappings(templateCompiled.map),
            extractGeneratedLineMappings(descriptor.template.map),
          ),
          sourceSegments: templateSourceSegments,
        });
      }
    } catch (error) {
      diagnostics.push(`Failed to compile <template>: ${formatSfcCompilerMessage(error)}`);
    }
  }

  const cssParts: string[] = [];
  const styleFragments: CompiledStyleFragment[] = [];
  for (const styleBlock of descriptor.styles) {
    try {
      const styleSourcePath = resolveVueStyleSourcePath(documentPath, styleBlock.src);
      const styleSource = loadVueStyleSource(styleSourcePath, styleBlock.content);
      const styleSourceRange = styleSourcePath === null
        ? getSfcBlockSourceRange(styleBlock)
        : {
          sourceStartLine: 1,
          sourceLineCount: Math.max(1, normalizeLineEndings(styleSource).split("\n").length),
        };
      const cssModuleName = resolveCssModuleName(styleBlock);
      const styleCompiled = await vueCompilerSfc.compileStyleAsync({
        source: styleSource,
        filename: styleSourcePath ?? filename,
        id: `data-v-${scopeId}`,
        scoped: styleBlock.scoped,
        isProd: isProduction,
        modules: cssModuleName !== null,
        modulesOptions: cssModuleName === null
          ? undefined
          : {
            generateScopedName: (name: string, styleFilename: string, css: string) =>
              createCssModuleScopedName(name, styleFilename, scopeId),
          },
      });
      for (const error of styleCompiled.errors) {
        diagnostics.push(formatSfcCompilerMessage(error));
      }

      if (cssModuleName !== null && styleCompiled.modules !== undefined) {
        const existingModules = cssModules.get(cssModuleName) ?? {};
        Object.assign(existingModules, styleCompiled.modules);
        cssModules.set(cssModuleName, existingModules);
      }

      if (styleCompiled.code.trim().length > 0) {
        const compiledCss = styleCompiled.code.trim();
        cssParts.push(compiledCss);
        styleFragments.push({
          cssContent: compiledCss,
          sourcePath: styleSourcePath ?? normalizePath(documentPath),
          sourceLineStart: styleSourceRange?.sourceStartLine ?? null,
          sourceLineCount: styleSourceRange?.sourceLineCount ?? null,
        });
      }
    } catch (error) {
      diagnostics.push(`Failed to compile <style>: ${formatSfcCompilerMessage(error)}`);
    }
  }

  if (hasScopedStyles) {
    jsParts.push({
      content: `_sfc_main.__scopeId = "data-v-${scopeId}";`,
      sourceRange: scopedStyleSourceRange,
    });
  }

  if (cssModules.size > 0) {
    const cssModuleRecord: Record<string, Record<string, string>> = {};
    for (const [moduleName, moduleMappings] of cssModules) {
      cssModuleRecord[moduleName] = moduleMappings;
    }

    jsParts.push({
      content: `_sfc_main.__cssModules = ${JSON.stringify(cssModuleRecord)};`,
      sourceRange: moduleStyleSourceRange ?? preferredSourceRange,
    });
  }

  if (descriptor.template !== null) {
    jsParts.push({
      content: "_sfc_main.render = render;",
      sourceRange: templateSourceRange,
    });
  }

  if (!isProduction) {
    const hmrId = `jazor-vue:${normalizePath(documentPath)}`;
    const templateHash = createStableHash(descriptor.template?.content ?? "");
    const scriptHash = createStableHash([
      descriptor.script?.content ?? "",
      descriptor.scriptSetup?.content ?? "",
      descriptor.styles.map((style) => `${style.scoped}:${style.lang ?? ""}:${style.module ? "module" : "plain"}:${style.src ?? ""}`).join("|"),
    ].join("\n"));
    jsParts.push({ content: `const __jazorHmrId = ${JSON.stringify(hmrId)};` });
    jsParts.push({ content: `const __jazorHmrTemplateHash = ${JSON.stringify(templateHash)};` });
    jsParts.push({ content: `const __jazorHmrScriptHash = ${JSON.stringify(scriptHash)};` });
    jsParts.push({
      content: "_sfc_main.__hmrId = __jazorHmrId;",
      sourceRange: preferredSourceRange,
    });
    jsParts.push({ content: `
const __jazorHotContext = import.meta.hot ?? globalThis.__JAZOR_HMR__?.createHotContext(import.meta.url);
if (__jazorHotContext) {
  import.meta.hot = __jazorHotContext;
}
`.trim() });
    jsParts.push({ content: `
if (import.meta.hot) {
  const __jazorVueHmrRuntime = globalThis.__VUE_HMR_RUNTIME__;
  if (__jazorVueHmrRuntime) {
    __jazorVueHmrRuntime.createRecord(__jazorHmrId, _sfc_main);
  }
  import.meta.hot.accept((updatedModule) => {
    const updatedComponent = updatedModule?.default;
    if (!updatedComponent || !__jazorVueHmrRuntime) {
      import.meta.hot.invalidate?.("Vue SFC HMR runtime was unavailable.");
      return;
    }
    updatedComponent.__hmrId = __jazorHmrId;
    if (updatedModule.__jazorHmrScriptHash === __jazorHmrScriptHash
        && updatedModule.__jazorHmrTemplateHash !== __jazorHmrTemplateHash
        && updatedComponent.render) {
      __jazorVueHmrRuntime.rerender(__jazorHmrId, updatedComponent.render);
      return;
    }
    __jazorVueHmrRuntime.reload(__jazorHmrId, updatedComponent);
  });
}
`.trim() });
    jsParts.push({
      content: "export { __jazorHmrId, __jazorHmrTemplateHash, __jazorHmrScriptHash };",
      sourceRange: preferredSourceRange,
    });
  }

  jsParts.push({
    content: "export default _sfc_main;",
    sourceRange: preferredSourceRange,
  });

  const sourceMappedModule = createSourceMappedModule(jsParts, filename, sfcText);

  return {
    jsContent: sourceMappedModule.content,
    jsSourceMap: sourceMappedModule.sourceMap,
    cssContent: cssParts.length === 0 ? null : cssParts.join("\n\n"),
    styleFragments,
    diagnostics,
    supportsHmr: !isProduction,
  };
}

function compileTypeScript(
  documentPath: string,
  text: string,
  filename: string,
): { jsContent: string; jsSourceMap: string | null; diagnostics: string[] } {
  const sourceFileName = filename.trim().length === 0 ? documentPath : filename;
  return transpileTypeScriptModule(text, sourceFileName, true);
}

async function compileCssModule(
  documentPath: string,
  text: string,
  filename: string,
  isProduction: boolean,
): Promise<{ cssContent: string; modules: Record<string, string>; diagnostics: string[] }> {
  const sourceFileName = filename.trim().length === 0 ? documentPath : filename;
  const normalizedText = normalizeLineEndings(text);
  const scopeId = createVueScopeId(documentPath);
  const diagnostics: string[] = [];

  const compiled = await vueCompilerSfc.compileStyleAsync({
    source: normalizedText,
    filename: sourceFileName,
    id: `data-v-${scopeId}`,
    scoped: false,
    isProd: isProduction,
    modules: true,
    modulesOptions: {
      generateScopedName: (name: string, styleFilename: string, css: string) =>
        createCssModuleScopedName(name, styleFilename, scopeId),
    },
  });

  for (const error of compiled.errors) {
    diagnostics.push(formatSfcCompilerMessage(error));
  }

  return {
    cssContent: compiled.code.trim(),
    modules: compiled.modules ?? {},
    diagnostics,
  };
}

async function tryGetVueDiagnostics(
  documentPath: string,
  text: string,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async ({ service, documentUri }) => {
    const diagnostics = await service.getDiagnostics(documentUri);
    return diagnostics.map((diagnostic) => ({
      range: diagnostic.range,
      severity: diagnostic.severity,
      code: diagnostic.code === undefined ? undefined : String(diagnostic.code),
      source: diagnostic.source,
      message: diagnostic.message,
    }));
  });
}

async function tryGetVueCompletionItems(
  documentPath: string,
  text: string,
  position: Position,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async ({ service, documentUri }) => {
    const completionList = await service.getCompletionItems(documentUri, position, { triggerKind: 1 });
    return completionList.items;
  });
}

async function tryGetVueHover(
  documentPath: string,
  text: string,
  position: Position,
): Promise<unknown | null | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async ({ service, documentUri }) => {
    return await service.getHover(documentUri, position) ?? null;
  });
}

async function tryGetVueDefinition(
  documentPath: string,
  text: string,
  position: Position,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async (context) => {
    const locations = await context.service.getDefinition(context.documentUri, position);
    return mapVolarDefinitionLinksToLocations(locations ?? [], context);
  });
}

async function tryGetVueImplementation(
  documentPath: string,
  text: string,
  position: Position,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async (context) => {
    const dynamicService = context.service as unknown as {
      getImplementations?: (uri: URI, position: Position) => Promise<unknown[] | undefined>;
      getImplementation?: (uri: URI, position: Position) => Promise<unknown[] | undefined>;
    };
    const getImplementations = typeof dynamicService.getImplementations === "function"
      ? dynamicService.getImplementations.bind(dynamicService)
      : typeof dynamicService.getImplementation === "function"
        ? dynamicService.getImplementation.bind(dynamicService)
        : null;
    if (getImplementations === null) {
      return [];
    }

    const locations = await getImplementations(context.documentUri, position);
    return normalizeVolarImplementationLocations(locations ?? [], context);
  });
}

async function tryGetVueReferences(
  documentPath: string,
  text: string,
  position: Position,
  includeDeclaration: boolean,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async (context) => {
    const locations = await context.service.getReferences(
      context.documentUri,
      position,
      { includeDeclaration },
    );
    return normalizeVolarLocations(locations ?? [], context);
  });
}

async function tryGetVueRename(
  documentPath: string,
  text: string,
  position: Position,
  newName: string,
): Promise<unknown | null | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async (context) => {
    const workspaceEdit = await context.service.getRenameEdits(context.documentUri, position, newName);
    return normalizeVolarWorkspaceEdit(workspaceEdit ?? null, context);
  });
}

async function tryGetVueDocumentSymbols(
  documentPath: string,
  text: string,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async ({ service, documentUri }) => {
    return await service.getDocumentSymbols(documentUri) ?? [];
  });
}

async function tryGetVueSemanticTokens(
  documentPath: string,
  text: string,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async ({ service, documentUri }) => {
    const tokens = await service.getSemanticTokens(documentUri, undefined, service.semanticTokenLegend);
    return decodeVolarSemanticTokens(tokens, service.semanticTokenLegend);
  });
}

async function tryGetVueDocumentLinks(
  documentPath: string,
  text: string,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async (context) => {
    const dynamicService = context.service as unknown as {
      getDocumentLinks?: (uri: URI) => Promise<Array<{ range: Range; target?: string; tooltip?: string }> | undefined>;
    };
    if (typeof dynamicService.getDocumentLinks !== "function") {
      return [];
    }

    const links = await dynamicService.getDocumentLinks(context.documentUri);
    return normalizeVolarDocumentLinks(links ?? [], context);
  });
}

async function tryGetVueInlayHints(
  documentPath: string,
  text: string,
  range: Range,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async ({ service, documentUri }) => {
    const dynamicService = service as unknown as {
      getInlayHints?: (uri: URI, range: Range) => Promise<Array<{ position: Position; label: unknown; kind?: number }> | undefined>;
    };
    if (typeof dynamicService.getInlayHints !== "function") {
      return [];
    }

    const hints = await dynamicService.getInlayHints(documentUri, range);
    return normalizeVolarInlayHints(hints ?? []);
  });
}

async function tryGetVueFoldingRanges(
  documentPath: string,
  text: string,
): Promise<unknown[] | typeof volarUnhandled> {
  return await withVueLanguageService(documentPath, text, async ({ service, documentUri }) => {
    const dynamicService = service as unknown as {
      getFoldingRanges?: (uri: URI) => Promise<Array<{ startLine: number; startCharacter?: number; endLine: number; endCharacter?: number; kind?: string }> | undefined>;
    };
    if (typeof dynamicService.getFoldingRanges !== "function") {
      return [];
    }

    const ranges = await dynamicService.getFoldingRanges(documentUri);
    return normalizeVolarFoldingRanges(ranges ?? []);
  });
}

async function withVueLanguageService<TResult>(
  documentPath: string,
  text: string,
  callback: (context: VolarServiceContext) => Promise<TResult>,
): Promise<TResult | typeof volarUnhandled> {
  let context: VolarServiceContext | null = null;
  try {
    context = createVueLanguageServiceContext(documentPath, text);
    return await callback(context);
  } catch {
    return volarUnhandled;
  } finally {
    context?.dispose();
  }
}

function createVueLanguageServiceContext(documentPath: string, text: string): VolarServiceContext {
  const serviceDocumentPath = getVueServiceDocumentPath(documentPath);
  const serviceDocumentPathKey = normalizePathForComparison(serviceDocumentPath);
  const documentUri = URI.file(serviceDocumentPath);
  const workspaceDirectory = getDirectoryName(serviceDocumentPath);
  const hostDocumentUri = toDocumentUri(documentPath);
  const compilerOptions: ts.CompilerOptions = {
    allowJs: true,
    allowNonTsExtensions: true,
    checkJs: true,
    jsx: ts.JsxEmit.Preserve,
    module: ts.ModuleKind.ESNext,
    moduleResolution: ts.ModuleResolutionKind.Bundler,
    noEmit: true,
    skipLibCheck: true,
    target: ts.ScriptTarget.ESNext,
  };
  const vueCompilerOptions = vueLanguageCore.getDefaultCompilerOptions(3.5);
  const scriptRegistry = new Map();
  const uriConverter = {
    asUri: (fileName: string) => URI.file(normalizePath(fileName)),
    asFileName: (uri: URI) => normalizePath(uri.fsPath),
  };
  const fsProvider = createVolarFileSystem();
  const workspaceFolders = [URI.file(workspaceDirectory.length === 0 ? serviceDocumentPath : workspaceDirectory)];
  let language: volarLanguageCore.Language<URI>;
  language = volarLanguageCore.createLanguage([
    vueLanguageCore.createVueLanguagePlugin(
      ts,
      compilerOptions,
      vueCompilerOptions,
      uriConverter.asFileName,
    ),
  ], scriptRegistry, (uri, includeFsFiles) => {
    const fileName = uriConverter.asFileName(uri);
    if (normalizePathForComparison(fileName) === serviceDocumentPathKey) {
      language.scripts.set(uri, ts.ScriptSnapshot.fromString(text), "vue");
      return;
    }

    if (!includeFsFiles) {
      return;
    }

    const diskText = tryReadTextFile(fileName);
    const languageId = getLanguageServiceDocumentLanguageId(fileName);
    if (diskText === null || languageId === null) {
      return;
    }

    language.scripts.set(uri, ts.ScriptSnapshot.fromString(diskText), languageId);
  });

  const environment = {
    workspaceFolders,
    fs: fsProvider,
    console,
  };
  const sys = volarTypeScript.createSys(
    ts.sys,
    environment as Parameters<typeof volarTypeScript.createSys>[1],
    () => workspaceDirectory,
    uriConverter,
  );
  const { languageServiceHost, getExtraServiceScript } = volarTypeScript.createLanguageServiceHost(
    ts,
    sys,
    language,
    (fileName) => uriConverter.asUri(fileName),
    {
      getCurrentDirectory: () => workspaceDirectory,
      getCompilationSettings: () => compilerOptions,
      getScriptFileNames: () => [serviceDocumentPath],
      getProjectVersion: () => "1",
    },
  );
  const service = vueLanguageService.createLanguageService(
    language,
    [
      ...volarTypeScriptService.create(ts),
      ...vueLanguageService.createVueLanguageServicePlugins(ts),
    ],
    environment as Parameters<typeof vueLanguageService.createLanguageService>[2],
    {
      typescript: {
        configFileName: undefined,
        sys,
        languageServiceHost,
        getExtraServiceScript,
        uriConverter,
      },
    },
  );

  return {
    documentPath,
    serviceDocumentPath,
    documentUri,
    documentUriText: documentUri.toString(),
    hostDocumentUri,
    service,
    dispose: () => {
      service.dispose();
      sys.dispose?.();
    },
  };
}

function createVolarFileSystem() {
  return {
    stat(uri: URI) {
      try {
        const stat = Deno.statSync(uri.fsPath);
        return {
          type: stat.isDirectory ? 2 : 1,
          ctime: stat.birthtime?.getTime() ?? 0,
          mtime: stat.mtime?.getTime() ?? 0,
          size: stat.size,
        };
      } catch {
        return undefined;
      }
    },
    readDirectory(uri: URI): [string, number][] {
      try {
        return Array.from(Deno.readDirSync(uri.fsPath), (entry) => [
          entry.name,
          entry.isDirectory ? 2 : entry.isSymlink ? 64 : 1,
        ]);
      } catch {
        return [];
      }
    },
    readFile(uri: URI) {
      return tryReadTextFile(uri.fsPath) ?? undefined;
    },
  };
}

function getVueServiceDocumentPath(documentPath: string): string {
  const normalizedPath = normalizePath(documentPath);
  if (normalizedPath.startsWith("virtual:")) {
    const unwrapped = normalizedPath.slice("virtual:".length);
    if (/^[A-Za-z]:\//.test(unwrapped) || unwrapped.startsWith("/")) {
      return unwrapped;
    }
  }

  return normalizedPath;
}

function getLanguageServiceDocumentLanguageId(documentPath: string): string | null {
  const normalizedPath = normalizePath(documentPath).toLowerCase();
  if (normalizedPath.endsWith(".vue")) {
    return "vue";
  }

  if (normalizedPath.endsWith(".d.ts")
    || normalizedPath.endsWith(".ts")
    || normalizedPath.endsWith(".mts")
    || normalizedPath.endsWith(".cts"))
  {
    return "typescript";
  }

  if (normalizedPath.endsWith(".tsx")) {
    return "typescriptreact";
  }

  if (normalizedPath.endsWith(".js")
    || normalizedPath.endsWith(".mjs")
    || normalizedPath.endsWith(".cjs"))
  {
    return "javascript";
  }

  if (normalizedPath.endsWith(".jsx")) {
    return "javascriptreact";
  }

  if (normalizedPath.endsWith(".json")) {
    return "json";
  }

  if (normalizedPath.endsWith(".html")) {
    return "html";
  }

  if (normalizedPath.endsWith(".css")) {
    return "css";
  }

  return null;
}

function tryReadTextFile(documentPath: string): string | null {
  try {
    return Deno.readTextFileSync(documentPath);
  } catch {
    return null;
  }
}

function mapVolarDefinitionLinksToLocations(
  links: Array<{
    targetUri: string;
    targetRange: { start: Position; end: Position };
    targetSelectionRange?: { start: Position; end: Position };
  }>,
  context: VolarServiceContext,
): unknown[] {
  return links.map((link) => ({
    uri: normalizeVolarResultUri(link.targetUri, context),
    range: link.targetSelectionRange ?? link.targetRange,
  }));
}

function normalizeVolarImplementationLocations(
  locations: unknown[],
  context: VolarServiceContext,
): unknown[] {
  const firstLocation = locations.find((location) => typeof location === "object" && location !== null);
  if (firstLocation !== undefined && "targetUri" in (firstLocation as Record<string, unknown>)) {
    return mapVolarDefinitionLinksToLocations(
      locations as Array<{
        targetUri: string;
        targetRange: { start: Position; end: Position };
        targetSelectionRange?: { start: Position; end: Position };
      }>,
      context,
    );
  }

  return normalizeVolarLocations(
    locations as Array<{ uri: string; range: { start: Position; end: Position } }>,
    context,
  );
}

function normalizeVolarLocations(
  locations: Array<{ uri: string; range: { start: Position; end: Position } }>,
  context: VolarServiceContext,
): unknown[] {
  return locations.map((location) => ({
    uri: normalizeVolarResultUri(location.uri, context),
    range: location.range,
  }));
}

function normalizeVolarDocumentLinks(
  links: Array<{ range: Range; target?: string; tooltip?: string }>,
  context: VolarServiceContext,
): unknown[] {
  return links.map((link) => ({
    range: link.range,
    target: typeof link.target === "string"
      ? normalizeVolarResultUri(link.target, context)
      : undefined,
    tooltip: typeof link.tooltip === "string" ? link.tooltip : undefined,
  }));
}

function normalizeVolarInlayHints(
  hints: Array<{ position: Position; label: unknown; kind?: number }>,
): unknown[] {
  return hints
    .map((hint) => {
      const label = normalizeVolarInlayHintLabel(hint.label);
      if (label === null || label.length === 0) {
        return null;
      }

      return {
        position: hint.position,
        label,
        kind: typeof hint.kind === "number" ? hint.kind : undefined,
      };
    })
    .filter((hint): hint is { position: Position; label: string; kind?: number } => hint !== null);
}

function normalizeVolarInlayHintLabel(label: unknown): string | null {
  if (typeof label === "string") {
    return label;
  }

  if (Array.isArray(label)) {
    const combined = label
      .map((part) => {
        if (typeof part === "string") {
          return part;
        }

        if (typeof part === "object" && part !== null && "value" in part && typeof part.value === "string") {
          return part.value;
        }

        return "";
      })
      .join("");
    return combined.length === 0 ? null : combined;
  }

  return null;
}

function normalizeVolarFoldingRanges(
  ranges: Array<{ startLine: number; startCharacter?: number; endLine: number; endCharacter?: number; kind?: string }>,
): unknown[] {
  return ranges
    .filter((range) => Number.isInteger(range.startLine) && Number.isInteger(range.endLine))
    .map((range) => ({
      startLine: range.startLine,
      startCharacter: typeof range.startCharacter === "number" ? range.startCharacter : undefined,
      endLine: range.endLine,
      endCharacter: typeof range.endCharacter === "number" ? range.endCharacter : undefined,
      kind: typeof range.kind === "string" ? range.kind : undefined,
    }));
}

function normalizeVolarWorkspaceEdit(
  workspaceEdit: {
    changes?: Record<string, Array<{ range: { start: Position; end: Position }; newText: string }>>;
    documentChanges?: Array<
      | { textDocument: { uri: string }; edits: Array<{ range: { start: Position; end: Position }; newText: string }> }
      | unknown
    >;
  } | null,
  context: VolarServiceContext,
): unknown | null {
  if (workspaceEdit === null) {
    return null;
  }

  const changes: Record<string, Array<{ range: { start: Position; end: Position }; newText: string }>> = {};
  for (const [uri, edits] of Object.entries(workspaceEdit.changes ?? {})) {
    changes[normalizeVolarResultUri(uri, context)] = edits;
  }

  for (const change of workspaceEdit.documentChanges ?? []) {
    if (!isTextDocumentEditChange(change)) {
      continue;
    }

    const uri = normalizeVolarResultUri(change.textDocument.uri, context);
    const existing = changes[uri] ?? [];
    changes[uri] = existing.concat(change.edits);
  }

  return Object.keys(changes).length === 0
    ? null
    : { changes };
}

function normalizeVolarResultUri(uri: string, context: VolarServiceContext): string {
  if (uri === context.documentUriText) {
    return context.hostDocumentUri;
  }

  try {
    const parsed = URI.parse(uri);
    if (parsed.scheme === "file") {
      return toDocumentUri(parsed.fsPath);
    }
  } catch {
    // Keep upstream value when URI parsing fails.
  }

  return uri;
}

function isTextDocumentEditChange(
  value: unknown,
): value is {
  textDocument: { uri: string };
  edits: Array<{ range: { start: Position; end: Position }; newText: string }>;
} {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  if (!("textDocument" in value) || !("edits" in value)) {
    return false;
  }

  const change = value as {
    textDocument?: { uri?: unknown };
    edits?: unknown;
  };
  return typeof change.textDocument?.uri === "string" && Array.isArray(change.edits);
}

function decodeVolarSemanticTokens(
  semanticTokens: { data?: number[] } | undefined,
  legend: { tokenTypes: string[]; tokenModifiers: string[] },
): unknown[] {
  if (semanticTokens?.data === undefined) {
    return [];
  }

  const decoded: unknown[] = [];
  let line = 0;
  let character = 0;
  for (let index = 0; index < semanticTokens.data.length; index += 5) {
    const lineDelta = semanticTokens.data[index];
    const characterDelta = semanticTokens.data[index + 1];
    const length = semanticTokens.data[index + 2];
    const tokenTypeIndex = semanticTokens.data[index + 3];
    const modifierBits = semanticTokens.data[index + 4];

    line += lineDelta;
    character = lineDelta === 0
      ? character + characterDelta
      : characterDelta;

    decoded.push({
      line,
      character,
      length,
      tokenType: legend.tokenTypes[tokenTypeIndex] ?? "type",
      tokenModifiers: decodeVolarSemanticTokenModifiers(modifierBits, legend.tokenModifiers),
    });
  }

  return decoded;
}

function decodeVolarSemanticTokenModifiers(bits: number, modifiers: string[]): string[] {
  const decoded: string[] = [];
  for (let index = 0; index < modifiers.length; index++) {
    if ((bits & (1 << index)) !== 0) {
      decoded.push(modifiers[index]);
    }
  }

  return decoded;
}

function getScriptDiagnostics(context: ScriptContext): unknown[] {
  const diagnostics: unknown[] = [];
  const importPattern = /^\s*import\s+.+?\s+from\s+["'](?<path>[^"']+)["']/gm;
  for (const match of context.scriptText.matchAll(importPattern)) {
    const importPath = match.groups?.["path"];
    const index = match.index ?? -1;
    if (importPath === undefined || index < 0 || !isRelativeImportPath(importPath)) {
      continue;
    }

    if (resolveImportPath(context.sourceDocumentPath, importPath) !== null) {
      continue;
    }

    const importPathOffset = match[0].indexOf(importPath);
    diagnostics.push({
      range: mapScriptRangeToSourceRange(context, {
        start: toPosition(context.scriptText, index + importPathOffset),
        end: toPosition(context.scriptText, index + importPathOffset + importPath.length),
      }),
      severity: 1,
      code: "JAZORVUEFRONTENDSCRIPT001",
      source: "Jolt.Frontend",
      message: `Frontend import '${importPath}' could not be resolved.`,
    });
  }

  return diagnostics;
}

function getScriptCompletionItems(context: ScriptContext, position: Position): unknown[] {
  const typeScriptItems = tryGetTypeScriptCompletionItems(context, position);
  if (typeScriptItems !== undefined) {
    return typeScriptItems;
  }

  const scriptPosition = mapSourcePositionToScriptPosition(context, position);
  if (scriptPosition === null) {
    return [];
  }

  const offset = toOffset(context.scriptText, scriptPosition);
  const prefix = getIdentifierPrefix(context.scriptText, offset);
  return context.symbols
    .filter((symbol, index, symbols) =>
      symbols.findIndex((candidate) => candidate.name === symbol.name && candidate.kind === symbol.kind) === index)
    .filter((symbol) => prefix.length === 0 || symbol.name.startsWith(prefix))
    .sort((left, right) => left.name.localeCompare(right.name))
    .map((symbol) => ({
      label: symbol.name,
      kind: mapScriptSymbolCompletionKind(symbol),
      detail: symbol.detail,
      documentation: createScriptSymbolDocumentation(symbol),
    }));
}

function getScriptHover(context: ScriptContext, position: Position): unknown | null {
  const typeScriptHover = tryGetTypeScriptHover(context, position);
  if (typeScriptHover !== undefined) {
    return typeScriptHover;
  }

  const symbol = resolveScriptSymbolAtPosition(context, position);
  if (symbol === null) {
    return null;
  }

  const importedTarget = tryResolveImportedScriptTarget(symbol);
  const targetSymbol = importedTarget?.symbol ?? symbol;
  const source = importedTarget !== null
    ? toImportPath(getDirectoryName(context.sourceDocumentPath), importedTarget.context.sourceDocumentPath)
    : symbol.importPath;

  return {
    contents: {
      kind: "markdown",
      value: `\`\`\`ts\n${targetSymbol.detail}\n\`\`\`\n\nkind: \`${targetSymbol.kind}\`${source === undefined ? "" : `\n\nsource: \`${source}\``}`,
    },
    range: symbol.range,
  };
}

function getScriptImplementation(context: ScriptContext, position: Position): unknown[] {
  const typeScriptImplementations = tryGetTypeScriptImplementation(context, position);
  if (typeScriptImplementations !== undefined) {
    return typeScriptImplementations;
  }

  return getScriptDefinition(context, position);
}

function getScriptDefinition(context: ScriptContext, position: Position): unknown[] {
  const typeScriptDefinitions = tryGetTypeScriptDefinition(context, position);
  if (typeScriptDefinitions !== undefined) {
    const symbol = resolveScriptSymbolAtPosition(context, position);
    if (symbol?.resolvedImportPath !== undefined
      && normalizePath(symbol.resolvedImportPath).endsWith(".vue")) {
      const currentDocumentUri = toDocumentUri(context.sourceDocumentPath).toLowerCase();
      const hasExternalDefinition = typeScriptDefinitions.some((definition) => {
        if (typeof definition !== "object" || definition === null || !("uri" in definition)) {
          return false;
        }

        const uri = (definition as { uri?: string }).uri;
        return typeof uri === "string" && uri.toLowerCase() !== currentDocumentUri;
      });
      if (!hasExternalDefinition) {
        return [createImportDefinitionLocation(symbol)];
      }
    }

    return typeScriptDefinitions;
  }

  const symbol = resolveScriptSymbolAtPosition(context, position);
  if (symbol === null) {
    return [];
  }

  const importedTarget = tryResolveImportedScriptTarget(symbol);
  if (importedTarget !== null) {
    return [{
      uri: toDocumentUri(importedTarget.context.sourceDocumentPath),
      range: importedTarget.symbol.range,
    }];
  }

  if (symbol.resolvedImportPath !== undefined) {
    return [createImportDefinitionLocation(symbol)];
  }

  return [{
    uri: toDocumentUri(context.sourceDocumentPath),
    range: symbol.range,
  }];
}

function getScriptReferences(context: ScriptContext, position: Position, includeDeclaration: boolean): unknown[] {
  const typeScriptReferences = tryGetTypeScriptReferences(context, position, includeDeclaration);
  if (typeScriptReferences !== undefined) {
    return typeScriptReferences;
  }

  const symbol = resolveScriptSymbolAtPosition(context, position);
  if (symbol === null) {
    return [];
  }

  const importedTarget = tryResolveImportedScriptTarget(symbol);
  const results: unknown[] = [];
  if (includeDeclaration) {
    results.push(importedTarget !== null
      ? {
        uri: toDocumentUri(importedTarget.context.sourceDocumentPath),
        range: importedTarget.symbol.range,
      }
      : symbol.resolvedImportPath === undefined
        ? {
          uri: toDocumentUri(context.sourceDocumentPath),
          range: symbol.range,
        }
        : createImportDefinitionLocation(symbol));
  }

  for (const range of findScriptSymbolRanges(context, symbol)) {
    if (!includeDeclaration && areRangesEqual(range, symbol.range)) {
      continue;
    }

    results.push({
      uri: toDocumentUri(context.sourceDocumentPath),
      range,
    });
  }

  return dedupeLocations(results);
}

function getScriptRename(context: ScriptContext, position: Position, newName: string): unknown | null {
  if (!/^[A-Za-z_$][A-Za-z0-9_$]*$/.test(newName.trim())) {
    return null;
  }

  const symbol = resolveScriptSymbolAtPosition(context, position);
  if (symbol === null || !canRenameScriptSymbol(symbol)) {
    return null;
  }

  const uri = toDocumentUri(context.sourceDocumentPath);
  const edits = findScriptSymbolRanges(context, symbol)
    .map((range) => ({
      range,
      newText: newName,
    }))
    .sort((left, right) => {
      if (left.range.start.line !== right.range.start.line) {
        return right.range.start.line - left.range.start.line;
      }

      return right.range.start.character - left.range.start.character;
    });
  if (edits.length === 0) {
    return null;
  }

  return {
    changes: {
      [uri]: edits,
    },
  };
}

function getScriptDocumentSymbols(context: ScriptContext): unknown[] {
  return context.symbols
    .filter((symbol, index, symbols) =>
      symbols.findIndex((candidate) => candidate.name === symbol.name && areRangesEqual(candidate.range, symbol.range)) === index)
    .map((symbol) => ({
      name: symbol.name,
      kind: mapScriptSymbolDocumentKind(symbol),
      range: symbol.range,
      selectionRange: symbol.range,
      detail: symbol.detail,
    }));
}

function getScriptSemanticTokens(context: ScriptContext): unknown[] {
  const tokens: unknown[] = [];
  for (const symbol of context.symbols) {
    const tokenType = mapScriptSymbolSemanticTokenType(symbol);
    for (const range of findScriptSymbolRanges(context, symbol)) {
      tokens.push(createSemanticToken(range, tokenType));
    }
  }

  return dedupeSemanticTokens(tokens);
}

function tryGetTypeScriptCompletionItems(context: ScriptContext, position: Position): unknown[] | undefined {
  return withTypeScriptProject(context, (project) => {
    const offset = getTypeScriptProjectOffset(project.context, position);
    if (offset === null) {
      return [];
    }

    const completions = project.languageService.getCompletionsAtPosition(project.entryFilePath, offset, undefined);
    if (completions === undefined) {
      return [];
    }

    return completions.entries
      .filter((entry, index, entries) =>
        entries.findIndex((candidate) => candidate.name === entry.name && candidate.kind === entry.kind) === index)
      .map((entry) => ({
        label: entry.name,
        kind: mapTypeScriptCompletionKind(entry.kind),
        detail: !entry.kindModifiers || entry.kindModifiers.length === 0
          ? entry.kind
          : `${entry.kind} ${entry.kindModifiers}`.trim(),
      }));
  });
}

function tryGetTypeScriptHover(context: ScriptContext, position: Position): unknown | null | undefined {
  return withTypeScriptProject(context, (project) => {
    const offset = getTypeScriptProjectOffset(project.context, position);
    if (offset === null) {
      return null;
    }

    const quickInfo = project.languageService.getQuickInfoAtPosition(project.entryFilePath, offset);
    if (quickInfo === undefined) {
      return null;
    }

    const location = tryMapTypeScriptTextSpan(project, project.entryFilePath, quickInfo.textSpan);
    if (location === null) {
      return null;
    }

    let displayText = ts.displayPartsToString(quickInfo.displayParts ?? []);
    let documentation = ts.displayPartsToString(quickInfo.documentation ?? []);
    let sourceImportPath: string | undefined;
    const definitions = project.languageService.getDefinitionAtPosition(project.entryFilePath, offset) ?? [];
    const preferredDefinition = definitions.find((definition) => normalizePath(definition.fileName) !== project.entryFilePath)
      ?? definitions[0];
    if (preferredDefinition !== undefined) {
      const definitionQuickInfo = project.languageService.getQuickInfoAtPosition(
        preferredDefinition.fileName,
        preferredDefinition.textSpan.start);
      if (definitionQuickInfo !== undefined) {
        displayText = ts.displayPartsToString(definitionQuickInfo.displayParts ?? []);
        documentation = ts.displayPartsToString(definitionQuickInfo.documentation ?? []);
      }

      const definitionContext = tryGetTypeScriptProjectContext(project, preferredDefinition.fileName);
      if (definitionContext !== null
        && normalizePath(definitionContext.sourceDocumentPath) !== normalizePath(project.context.sourceDocumentPath))
      {
        sourceImportPath = toImportPath(
          getDirectoryName(project.context.sourceDocumentPath),
          definitionContext.sourceDocumentPath);
      }
    }

    return {
      contents: {
        kind: "markdown",
        value: `\`\`\`ts\n${displayText}\n\`\`\`${documentation.length === 0 ? "" : `\n\n${documentation}`}${sourceImportPath === undefined ? "" : `\n\nsource: \`${sourceImportPath}\``}`,
      },
      range: location.range,
    };
  });
}

function tryGetTypeScriptDefinition(context: ScriptContext, position: Position): unknown[] | undefined {
  return withTypeScriptProject(context, (project) => {
    const offset = getTypeScriptProjectOffset(project.context, position);
    if (offset === null) {
      return [];
    }

    const definitions = project.languageService.getDefinitionAtPosition(project.entryFilePath, offset) ?? [];
    return definitions
      .map((definition) => tryMapTypeScriptTextSpan(project, definition.fileName, definition.textSpan))
      .filter((definition): definition is { uri: string; range: { start: Position; end: Position } } => definition !== null);
  });
}

function tryGetTypeScriptImplementation(context: ScriptContext, position: Position): unknown[] | undefined {
  return withTypeScriptProject(context, (project) => {
    const offset = getTypeScriptProjectOffset(project.context, position);
    if (offset === null) {
      return [];
    }

    const implementations = project.languageService.getImplementationAtPosition(project.entryFilePath, offset) ?? [];
    return implementations
      .map((implementation) => tryMapTypeScriptTextSpan(project, implementation.fileName, implementation.textSpan))
      .filter((implementation): implementation is { uri: string; range: { start: Position; end: Position } } => implementation !== null);
  });
}

function tryGetTypeScriptReferences(
  context: ScriptContext,
  position: Position,
  includeDeclaration: boolean,
): unknown[] | undefined {
  return withTypeScriptProject(context, (project) => {
    const offset = getTypeScriptProjectOffset(project.context, position);
    if (offset === null) {
      return [];
    }

    const references = project.languageService.getReferencesAtPosition(project.entryFilePath, offset) ?? [];
    const definitionLocations = (project.languageService.getDefinitionAtPosition(project.entryFilePath, offset) ?? [])
      .map((definition) => tryMapTypeScriptTextSpan(project, definition.fileName, definition.textSpan))
      .filter((definition): definition is { uri: string; range: { start: Position; end: Position } } => definition !== null);
    const declarations = includeDeclaration
      ? definitionLocations
      : [];
    const declarationKeys = new Set(
      definitionLocations.map((definition) =>
        `${definition.uri}:${definition.range.start.line}:${definition.range.start.character}:${definition.range.end.line}:${definition.range.end.character}`),
    );
    return dedupeLocations(
      declarations.concat(
        references
          .map((reference) => tryMapTypeScriptTextSpan(project, reference.fileName, reference.textSpan))
          .filter((reference): reference is { uri: string; range: { start: Position; end: Position } } => reference !== null),
      )
        .filter((reference) =>
          includeDeclaration
            || !declarationKeys.has(`${reference.uri}:${reference.range.start.line}:${reference.range.start.character}:${reference.range.end.line}:${reference.range.end.character}`))
        .filter((reference) => reference !== null)
    );
  });
}

function withTypeScriptProject<TResult>(
  context: ScriptContext,
  callback: (project: TypeScriptProject) => TResult,
): TResult | undefined {
  const project = tryCreateTypeScriptProject(context);
  if (project === null) {
    return undefined;
  }

  try {
    return callback(project);
  } finally {
    project.languageService.dispose();
  }
}

function tryCreateTypeScriptProject(context: ScriptContext): TypeScriptProject | null {
  const entryFilePath = getTypeScriptEntryFilePath(context);
  const compilerOptions: ts.CompilerOptions = {
    allowJs: true,
    allowNonTsExtensions: true,
    checkJs: true,
    jsx: ts.JsxEmit.Preserve,
    module: ts.ModuleKind.ESNext,
    moduleResolution: ts.ModuleResolutionKind.Bundler,
    noEmit: true,
    skipLibCheck: true,
    target: ts.ScriptTarget.ESNext,
  };
  const virtualContexts = new Map<string, ScriptContext>();
  virtualContexts.set(entryFilePath, context);

  const projectContext = {
    context,
    entryFilePath,
    virtualContexts,
  };
  const serviceHost: ts.LanguageServiceHost = {
    directoryExists: (directoryName) => ts.sys.directoryExists?.(directoryName) ?? false,
    fileExists: (fileName) => tryReadTypeScriptProjectText(projectContext, fileName) !== null || ts.sys.fileExists(fileName),
    getCompilationSettings: () => compilerOptions,
    getCurrentDirectory: () => getDirectoryName(entryFilePath),
    getDefaultLibFileName: (options) => ts.getDefaultLibFilePath(options),
    getDirectories: (directoryName) => ts.sys.getDirectories?.(directoryName) ?? [],
    getNewLine: () => "\n",
    getScriptFileNames: () => [entryFilePath],
    getScriptKind: (fileName) => getTypeScriptScriptKind(fileName, context, entryFilePath),
    getScriptSnapshot: (fileName) => {
      const text = tryReadTypeScriptProjectText(projectContext, fileName);
      if (text !== null) {
        return ts.ScriptSnapshot.fromString(text);
      }

      const diskText = ts.sys.readFile(fileName);
      return diskText === undefined ? undefined : ts.ScriptSnapshot.fromString(diskText);
    },
    getScriptVersion: (fileName) => normalizePath(fileName) === entryFilePath ? "1" : "0",
    readDirectory: (rootDir, extensions, excludes, includes, depth) =>
      ts.sys.readDirectory?.(rootDir, extensions, excludes, includes, depth) ?? [],
    readFile: (fileName) => tryReadTypeScriptProjectText(projectContext, fileName) ?? ts.sys.readFile(fileName),
    realpath: (path) => ts.sys.realpath?.(path) ?? path,
    useCaseSensitiveFileNames: () => true,
  };

  try {
    return {
      context,
      entryFilePath,
      languageService: ts.createLanguageService(serviceHost, ts.createDocumentRegistry()),
      virtualContexts,
    };
  } catch {
    return null;
  }
}

function getTypeScriptEntryFilePath(context: ScriptContext): string {
  const normalizedPath = normalizePath(context.sourceDocumentPath);
  return getFrontendDocumentKind(context.sourceDocumentPath) === "vue"
    ? `${normalizedPath}.__jazor_jolt_script__.${context.scriptLanguage}`
    : normalizedPath;
}

function getTypeScriptScriptKind(
  fileName: string,
  context: ScriptContext,
  entryFilePath: string,
): ts.ScriptKind {
  const normalizedFileName = normalizePath(fileName);
  if (normalizedFileName === entryFilePath) {
    return context.scriptLanguage === "ts"
      ? ts.ScriptKind.TS
      : ts.ScriptKind.JS;
  }

  if (normalizedFileName.endsWith(".js") || normalizedFileName.endsWith(".mjs") || normalizedFileName.endsWith(".cjs")) {
    return ts.ScriptKind.JS;
  }

  if (normalizedFileName.endsWith(".jsx")) {
    return ts.ScriptKind.JSX;
  }

  if (normalizedFileName.endsWith(".tsx")) {
    return ts.ScriptKind.TSX;
  }

  return ts.ScriptKind.TS;
}

function tryReadTypeScriptProjectText(
  project: Pick<TypeScriptProject, "context" | "entryFilePath" | "virtualContexts">,
  fileName: string,
): string | null {
  const normalizedFileName = normalizePath(fileName);
  if (normalizedFileName === project.entryFilePath) {
    return project.context.scriptText;
  }

  const virtualContext = project.virtualContexts.get(normalizedFileName);
  if (virtualContext !== undefined) {
    return virtualContext.scriptText;
  }

  const diskContext = tryReadScriptDocumentContext(normalizedFileName);
  return diskContext?.scriptText ?? null;
}

function getTypeScriptProjectOffset(context: ScriptContext, position: Position): number | null {
  const scriptPosition = mapSourcePositionToScriptPosition(context, position);
  return scriptPosition === null
    ? null
    : toOffset(context.scriptText, scriptPosition);
}

function tryMapTypeScriptTextSpan(
  project: TypeScriptProject,
  fileName: string,
  textSpan: ts.TextSpan,
): { uri: string; range: { start: Position; end: Position } } | null {
  const context = tryGetTypeScriptProjectContext(project, fileName);
  if (context === null || context === undefined) {
    const normalizedFileName = normalizePath(fileName);
    if (!normalizedFileName.endsWith(".vue")) {
      return null;
    }

    // Template-only Vue components do not create a script context, but
    // TypeScript still resolves default imports to the source .vue file.
    // Preserve that native target so Jolt can bridge references/rename.
    return {
      uri: toDocumentUri(normalizedFileName),
      range: {
        start: { line: 0, character: 0 },
        end: { line: 0, character: 0 },
      },
    };
  }

  const scriptRange = toRange(context.scriptText, textSpan.start, textSpan.length) as { start: Position; end: Position };
  return {
    uri: toDocumentUri(context.sourceDocumentPath),
    range: mapScriptRangeToSourceRange(context, scriptRange),
  };
}

function tryGetTypeScriptProjectContext(project: TypeScriptProject, fileName: string): ScriptContext | null {
  const normalizedFileName = normalizePath(fileName);
  return project.virtualContexts.get(normalizedFileName)
    ?? tryReadScriptDocumentContext(normalizedFileName);
}

function mapTypeScriptCompletionKind(kind: string): number {
  switch (kind) {
    case ts.ScriptElementKind.classElement:
      return 7;
    case ts.ScriptElementKind.memberFunctionElement:
    case ts.ScriptElementKind.functionElement:
      return 3;
    case ts.ScriptElementKind.memberVariableElement:
    case ts.ScriptElementKind.memberGetAccessorElement:
    case ts.ScriptElementKind.memberSetAccessorElement:
    case ts.ScriptElementKind.variableElement:
    case ts.ScriptElementKind.constElement:
    case ts.ScriptElementKind.letElement:
      return 6;
    case ts.ScriptElementKind.interfaceElement:
    case ts.ScriptElementKind.typeElement:
      return 8;
    default:
      return 9;
  }
}

function canRenameScriptSymbol(symbol: ScriptSymbol): boolean {
  if (symbol.kind !== "import") {
    return true;
  }

  return symbol.isDefaultImport === true
    || (symbol.importedName !== undefined && symbol.importedName !== symbol.name);
}

function tryResolveImportedScriptTarget(symbol: ScriptSymbol): ImportedScriptTarget | null {
  return tryResolveImportedScriptTargetCore(symbol, new Set<string>());
}

function tryResolveImportedScriptTargetCore(symbol: ScriptSymbol, visited: Set<string>): ImportedScriptTarget | null {
  if (symbol.kind !== "import"
    || symbol.resolvedImportPath === undefined
    || symbol.importedName === undefined)
  {
    return null;
  }

  const exportKey = symbol.isDefaultImport === true
    ? "default"
    : symbol.importedName;
  const cacheKey = `${symbol.resolvedImportPath}::${exportKey}`;
  if (visited.has(cacheKey)) {
    return null;
  }

  visited.add(cacheKey);

  const context = tryReadScriptDocumentContext(symbol.resolvedImportPath);
  if (context === null) {
    return null;
  }

  const matches = context.symbols.filter((candidate) => {
    if (!candidate.isExported) {
      return false;
    }

    const exportedName = candidate.isDefaultExport === true
      ? "default"
      : candidate.exportedName ?? candidate.name;

    return symbol.isDefaultImport === true
      ? exportedName === "default"
      : exportedName === symbol.importedName;
  });

  if (matches.length !== 1) {
    const fallbackTarget = symbol.isDefaultImport === true
      ? tryFindExplicitDefaultExportSymbol(context)
      : tryFindExplicitNamedExportSymbol(context, symbol.importedName);
    return fallbackTarget === null
      ? null
      : { context, symbol: fallbackTarget };
  }

  const target = matches[0];
  if (target.kind === "import") {
    return tryResolveImportedScriptTargetCore(target, visited)
      ?? { context, symbol: target };
  }

  return { context, symbol: target };
}

function tryReadScriptDocumentContext(documentPath: string): ScriptContext | null {
  try {
    const text = Deno.readTextFileSync(documentPath);
    return tryCreateScriptDocumentContext(documentPath, text);
  } catch {
    return null;
  }
}

function tryCreateScriptContext(documentPath: string, text: string, position: Position): ScriptContext | null {
  const context = tryCreateScriptDocumentContext(documentPath, text);
  if (context === null || mapSourcePositionToScriptPosition(context, position) === null) {
    return null;
  }

  return context;
}

function tryCreateScriptDocumentContext(documentPath: string, text: string): ScriptContext | null {
  switch (getFrontendDocumentKind(documentPath)) {
    case "typescript":
      return createScriptContext(documentPath, text, text, 0, {
        start: { line: 0, character: 0 },
        end: toPosition(text, text.length),
      }, "ts");
    case "javascript":
      return createScriptContext(documentPath, text, text, 0, {
        start: { line: 0, character: 0 },
        end: toPosition(text, text.length),
      }, "js");
    case "vue": {
      const scriptBlock = findScriptBlock(text);
      if (scriptBlock === null) {
        return null;
      }

      return createScriptContext(
        documentPath,
        text,
        text.slice(scriptBlock.contentStartOffset, scriptBlock.contentEndOffset),
        scriptBlock.contentStartOffset,
        {
          start: toPosition(text, scriptBlock.contentStartOffset),
          end: toPosition(text, scriptBlock.contentEndOffset),
        },
        scriptBlock.scriptLanguage);
    }
    default:
      return null;
  }
}

function createScriptContext(
  documentPath: string,
  sourceText: string,
  scriptText: string,
  contentStartOffset: number,
  scriptRange: { start: Position; end: Position },
  scriptLanguage: ScriptLanguage,
): ScriptContext {
  return {
    sourceDocumentPath: documentPath,
    sourceText,
    scriptText,
    contentStartOffset,
    scriptRange,
    scriptLanguage,
    symbols: collectScriptSymbols(documentPath, sourceText, scriptText, contentStartOffset),
  };
}

function collectScriptSymbols(
  documentPath: string,
  sourceText: string,
  scriptText: string,
  contentStartOffset: number,
): ScriptSymbol[] {
  const symbols: ScriptSymbol[] = [];

  const importPattern = /^\s*import\s+(?<clause>.+?)\s+from\s+["'](?<path>[^"']+)["']/gm;
  for (const match of scriptText.matchAll(importPattern)) {
    const clause = match.groups?.["clause"];
    const importPath = match.groups?.["path"];
    const index = match.index ?? -1;
    if (clause === undefined || importPath === undefined || index < 0) {
      continue;
    }

    const resolvedImportPath = isRelativeImportPath(importPath)
      ? resolveImportPath(documentPath, importPath) ?? undefined
      : undefined;
    const defaultMatch = clause.match(/^(?<name>[A-Za-z_$][A-Za-z0-9_$]*)/);
    if (defaultMatch?.groups?.["name"] !== undefined) {
      const name = defaultMatch.groups["name"];
      const nameIndex = index + match[0].indexOf(name);
      symbols.push(createScriptSymbol(
        sourceText,
        scriptText,
        contentStartOffset,
        nameIndex,
        name.length,
        name,
        "import",
        `import ${name} from "${importPath}"`,
        "default",
        true,
        false,
        false,
        importPath,
        resolvedImportPath));
    }

    const namedClause = clause.match(/\{(?<names>[^}]+)\}/)?.groups?.["names"];
    if (namedClause !== undefined) {
      for (const segment of namedClause.split(",")) {
        const trimmed = segment.trim();
        if (trimmed.length === 0) {
          continue;
        }

        const aliasMatch = trimmed.match(/^(?<imported>[A-Za-z_$][A-Za-z0-9_$]*)(?:\s+as\s+(?<local>[A-Za-z_$][A-Za-z0-9_$]*))?$/);
        const localName = aliasMatch?.groups?.["local"] ?? aliasMatch?.groups?.["imported"];
        if (localName === undefined) {
          continue;
        }

        const localIndexInClause = clause.indexOf(localName);
        if (localIndexInClause < 0) {
          continue;
        }

        symbols.push(createScriptSymbol(
          sourceText,
          scriptText,
          contentStartOffset,
          index + match[0].indexOf(clause) + localIndexInClause,
          localName.length,
          localName,
          "import",
          `import ${trimmed} from "${importPath}"`,
          aliasMatch?.groups?.["imported"],
          false,
          false,
          false,
          importPath,
          resolvedImportPath));
      }
    }
  }

  const variablePattern = /(?:^|\n)\s*(?:(?<export>export)\s+)?(?<kind>const|let|var)\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)/g;
  for (const match of scriptText.matchAll(variablePattern)) {
    const kind = match.groups?.["kind"] as ScriptSymbolKind | undefined;
    const name = match.groups?.["name"];
    const isExported = match.groups?.["export"] !== undefined;
    const index = match.index ?? -1;
    if (kind === undefined || name === undefined || index < 0) {
      continue;
    }

    const declarationIndex = index + match[0].lastIndexOf(name);
    symbols.push(createScriptSymbol(
      sourceText,
      scriptText,
      contentStartOffset,
      declarationIndex,
      name.length,
      name,
      kind,
      `${kind} ${name}`,
      undefined,
      false,
      isExported,
      false));
  }

  const functionPattern = /(?:^|\n)\s*(?:(?<export>export)\s+(?:(?<default>default)\s+)?)?function\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*\((?<params>[^)]*)\)/g;
  for (const match of scriptText.matchAll(functionPattern)) {
    const name = match.groups?.["name"];
    const params = match.groups?.["params"] ?? "";
    const isExported = match.groups?.["export"] !== undefined;
    const isDefaultExport = match.groups?.["default"] !== undefined;
    const index = match.index ?? -1;
    if (name === undefined || index < 0) {
      continue;
    }

    const declarationIndex = index + match[0].indexOf(name);
    symbols.push(createScriptSymbol(
      sourceText,
      scriptText,
      contentStartOffset,
      declarationIndex,
      name.length,
      name,
      "function",
      `function ${name}(${params})`,
      undefined,
      false,
      isExported,
      isDefaultExport));
  }

  const classPattern = /(?:^|\n)\s*(?:(?<export>export)\s+(?:(?<default>default)\s+)?)?class\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)/g;
  for (const match of scriptText.matchAll(classPattern)) {
    const name = match.groups?.["name"];
    const isExported = match.groups?.["export"] !== undefined;
    const isDefaultExport = match.groups?.["default"] !== undefined;
    const index = match.index ?? -1;
    if (name === undefined || index < 0) {
      continue;
    }

    const declarationIndex = index + match[0].lastIndexOf(name);
    symbols.push(createScriptSymbol(
      sourceText,
      scriptText,
      contentStartOffset,
      declarationIndex,
      name.length,
      name,
      "class",
      `class ${name}`,
      undefined,
      false,
      isExported,
      isDefaultExport));
  }

  const exportClausePattern = /(?:^|\n)\s*export\s*\{\s*(?<exports>[^}]+)\s*\}(?!\s*from)\s*;?/g;
  for (const match of scriptText.matchAll(exportClausePattern)) {
    const exportsClause = match.groups?.["exports"];
    if (exportsClause === undefined) {
      continue;
    }

    for (const segment of exportsClause.split(",")) {
      const trimmed = segment.trim();
      if (trimmed.length === 0) {
        continue;
      }

      const exportMatch = trimmed.match(/^(?<local>[A-Za-z_$][A-Za-z0-9_$]*)(?:\s+as\s+(?<exported>[A-Za-z_$][A-Za-z0-9_$]*))?$/);
      const localName = exportMatch?.groups?.["local"];
      const exportedName = exportMatch?.groups?.["exported"] ?? localName;
      if (localName === undefined || exportedName === undefined) {
        continue;
      }

      const localSymbol = findLocalScriptSymbol(symbols, localName);
      if (localSymbol === null) {
        continue;
      }

      symbols.push(createExportedScriptSymbol(localSymbol, exportedName));
    }
  }

  const exportDefaultReferencePattern = /(?:^|\n)\s*export\s+default\s+(?!function\b|class\b)(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*;?/g;
  for (const match of scriptText.matchAll(exportDefaultReferencePattern)) {
    const localName = match.groups?.["name"];
    if (localName === undefined) {
      continue;
    }

    const localSymbol = findLocalScriptSymbol(symbols, localName);
    if (localSymbol === null) {
      continue;
    }

    symbols.push(createExportedScriptSymbol(localSymbol, "default"));
  }

  return symbols;
}

function createScriptSymbol(
  sourceText: string,
  scriptText: string,
  contentStartOffset: number,
  scriptOffset: number,
  length: number,
  name: string,
  kind: ScriptSymbolKind,
  detail: string,
  importedName?: string,
  isDefaultImport?: boolean,
  isExported?: boolean,
  isDefaultExport?: boolean,
  importPath?: string,
  resolvedImportPath?: string,
): ScriptSymbol {
  const range = toRange(sourceText, contentStartOffset + scriptOffset, length) as { start: Position; end: Position };
  const scriptRange = toRange(scriptText, scriptOffset, length) as { start: Position; end: Position };

  return {
    name,
    kind,
    range,
    scriptRange,
    detail,
    importedName,
    isDefaultImport,
    isExported,
    isDefaultExport,
    importPath,
    resolvedImportPath,
  };
}

function mapSourcePositionToScriptPosition(context: ScriptContext, position: Position): Position | null {
  const sourceOffset = toOffset(context.sourceText, position);
  if (sourceOffset < context.contentStartOffset || sourceOffset > context.contentStartOffset + context.scriptText.length) {
    return null;
  }

  return toPosition(context.scriptText, sourceOffset - context.contentStartOffset);
}

function mapScriptRangeToSourceRange(
  context: ScriptContext,
  range: { start: Position; end: Position },
): { start: Position; end: Position } {
  if (context.contentStartOffset === 0 && context.sourceText === context.scriptText) {
    return range;
  }

  const startOffset = context.contentStartOffset + toOffset(context.scriptText, range.start);
  const endOffset = context.contentStartOffset + toOffset(context.scriptText, range.end);
  return {
    start: toPosition(context.sourceText, startOffset),
    end: toPosition(context.sourceText, endOffset),
  };
}

function resolveScriptSymbolAtPosition(context: ScriptContext, position: Position): ScriptSymbol | null {
  const scriptPosition = mapSourcePositionToScriptPosition(context, position);
  if (scriptPosition === null) {
    return null;
  }

  const identifier = getIdentifierAtPosition(context.scriptText, scriptPosition);
  if (identifier === null) {
    return null;
  }

  const exactMatches = context.symbols.filter((symbol) => areRangesEqual(symbol.range, identifier.range));
  if (exactMatches.length === 1) {
    return exactMatches[0];
  }

  // Keep script answers conservative: only resolve when a local declaration is unambiguous.
  const matches = context.symbols.filter((symbol) => symbol.name === identifier.name);
  const uniqueMatches = matches.filter((symbol, index, candidates) =>
    candidates.findIndex((candidate) => areRangesEqual(candidate.range, symbol.range)) === index);
  return uniqueMatches.length === 1 ? uniqueMatches[0] : null;
}

function findScriptSymbolRanges(
  context: ScriptContext,
  symbol: ScriptSymbol,
): Array<{ start: Position; end: Position }> {
  const results: Array<{ start: Position; end: Position }> = [];
  const declarationOffset = toOffset(context.scriptText, symbol.scriptRange.start);
  const pattern = new RegExp(`\\b${escapeRegExp(symbol.name)}\\b`, "g");
  for (const match of context.scriptText.matchAll(pattern)) {
    const index = match.index ?? -1;
    if (index < 0 || !isEligibleScriptReference(context.scriptText, symbol.name, index, declarationOffset)) {
      continue;
    }

    results.push(mapScriptRangeToSourceRange(
      context,
      toRange(context.scriptText, index, symbol.name.length) as { start: Position; end: Position }));
  }

  return results;
}

function isEligibleScriptReference(
  scriptText: string,
  name: string,
  index: number,
  declarationOffset: number,
): boolean {
  const previousCharacter = index > 0 ? scriptText[index - 1] : "";
  if (previousCharacter === ".") {
    return false;
  }

  const nextCharacter = index + name.length < scriptText.length ? scriptText[index + name.length] : "";
  if (nextCharacter === ":" && index !== declarationOffset) {
    return false;
  }

  return true;
}

function getIdentifierAtPosition(text: string, position: Position): { name: string; range: { start: Position; end: Position } } | null {
  const rawOffset = toOffset(text, position);
  let offset = rawOffset;
  if (offset > 0 && !isIdentifierCharacter(text[offset]) && isIdentifierCharacter(text[offset - 1])) {
    offset--;
  }

  if (!isIdentifierCharacter(text[offset])) {
    return null;
  }

  let start = offset;
  let end = offset;
  while (start > 0 && isIdentifierCharacter(text[start - 1])) {
    start--;
  }

  while (end < text.length && isIdentifierCharacter(text[end])) {
    end++;
  }

  return {
    name: text.slice(start, end),
    range: toRange(text, start, end - start) as { start: Position; end: Position },
  };
}

function getIdentifierPrefix(text: string, offset: number): string {
  let start = Math.max(0, Math.min(offset, text.length));
  while (start > 0 && isIdentifierCharacter(text[start - 1])) {
    start--;
  }

  return text.slice(start, Math.max(start, Math.min(offset, text.length)));
}

function isIdentifierCharacter(value: string | undefined): boolean {
  return value !== undefined && /[A-Za-z0-9_$]/.test(value);
}

function createImportDefinitionLocation(symbol: ScriptSymbol): unknown {
  return {
    uri: toDocumentUri(symbol.resolvedImportPath!),
    range: {
      start: { line: 0, character: 0 },
      end: { line: 0, character: 0 },
    },
  };
}

function findLocalScriptSymbol(symbols: ScriptSymbol[], name: string): ScriptSymbol | null {
  const matches = symbols
    .filter((symbol) => symbol.name === name)
    .filter((symbol, index, candidates) =>
      candidates.findIndex((candidate) => areRangesEqual(candidate.range, symbol.range)) === index);
  return matches.length === 1 ? matches[0] : null;
}

function createExportedScriptSymbol(symbol: ScriptSymbol, exportedName: string): ScriptSymbol {
  return {
    ...symbol,
    exportedName,
    isExported: true,
    isDefaultExport: exportedName === "default",
  };
}

function tryFindExplicitDefaultExportSymbol(context: ScriptContext): ScriptSymbol | null {
  const match = context.scriptText.match(/(?:^|\n)\s*export\s+default\s+(?!function\b|class\b)(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*;?/);
  const localName = match?.groups?.["name"];
  return localName === undefined
    ? null
    : findLocalScriptSymbol(context.symbols, localName);
}

function tryFindExplicitNamedExportSymbol(context: ScriptContext, exportedName: string): ScriptSymbol | null {
  const exportClausePattern = /(?:^|\n)\s*export\s*\{\s*(?<exports>[^}]+)\s*\}(?!\s*from)\s*;?/g;
  for (const match of context.scriptText.matchAll(exportClausePattern)) {
    const exportsClause = match.groups?.["exports"];
    if (exportsClause === undefined) {
      continue;
    }

    for (const segment of exportsClause.split(",")) {
      const trimmed = segment.trim();
      if (trimmed.length === 0) {
        continue;
      }

      const exportMatch = trimmed.match(/^(?<local>[A-Za-z_$][A-Za-z0-9_$]*)(?:\s+as\s+(?<exported>[A-Za-z_$][A-Za-z0-9_$]*))?$/);
      const localName = exportMatch?.groups?.["local"];
      const resolvedExportedName = exportMatch?.groups?.["exported"] ?? localName;
      if (localName === undefined || resolvedExportedName !== exportedName) {
        continue;
      }

      return findLocalScriptSymbol(context.symbols, localName);
    }
  }

  return null;
}

function mapScriptSymbolCompletionKind(symbol: ScriptSymbol): number {
  switch (symbol.kind) {
    case "class":
      return 7;
    case "function":
      return 3;
    default:
      return 6;
  }
}

function mapScriptSymbolDocumentKind(symbol: ScriptSymbol): number {
  switch (symbol.kind) {
    case "class":
      return 5;
    case "function":
      return 12;
    default:
      return 13;
  }
}

function mapScriptSymbolSemanticTokenType(symbol: ScriptSymbol): string {
  switch (symbol.kind) {
    case "class":
      return "class";
    case "function":
      return "method";
    default:
      return "variable";
  }
}

function createScriptSymbolDocumentation(symbol: ScriptSymbol): string {
  return symbol.importPath === undefined
    ? `Frontend script symbol: \`${symbol.detail}\`.`
    : `Frontend import from \`${symbol.importPath}\`.`;
}

function getFrontendDocumentKind(documentPath: string): FrontendDocumentKind {
  const normalized = normalizePath(documentPath).toLowerCase();
  if (normalized.endsWith(".jazor")) {
    return "jazor";
  }

  if (normalized.endsWith(".vue")) {
    return "vue";
  }

  if (normalized.endsWith(".ts")) {
    return "typescript";
  }

  if (normalized.endsWith(".js")) {
    return "javascript";
  }

  if (normalized.endsWith(".css")) {
    return "css";
  }

  if (normalized.endsWith(".html")) {
    return "html";
  }

  return "unknown";
}

function isRelativeImportPath(importPath: string): boolean {
  return importPath.startsWith("./") || importPath.startsWith("../");
}

function resolveImportPath(documentPath: string, importPath: string): string | null {
  const documentDirectory = getDirectoryName(documentPath);
  if (documentDirectory.length === 0) {
    return null;
  }

  const basePath = normalizePath(joinPath(documentDirectory, importPath));
  const candidates = hasPathExtension(basePath)
    ? [basePath]
    : [
      `${basePath}.ts`,
      `${basePath}.js`,
      `${basePath}.vue`,
      `${basePath}/index.ts`,
      `${basePath}/index.js`,
      `${basePath}/index.vue`,
    ];
  for (const candidate of candidates) {
    try {
      const stat = Deno.statSync(candidate);
      if (stat.isFile) {
        return normalizePath(candidate);
      }
    } catch {
      continue;
    }
  }

  return null;
}

function getDirectoryName(documentPath: string): string {
  const normalizedDocumentPath = normalizePath(documentPath);
  const lastSlash = normalizedDocumentPath.lastIndexOf("/");
  return lastSlash >= 0
    ? normalizedDocumentPath.slice(0, lastSlash)
    : "";
}

function joinPath(left: string, right: string): string {
  const normalizedLeft = normalizePath(left);
  const normalizedRight = normalizePath(right);
  const combined = /^[A-Za-z]:\//.test(normalizedRight) || normalizedRight.startsWith("/")
    ? normalizedRight
    : `${normalizedLeft}/${normalizedRight}`;
  const driveMatch = combined.match(/^[A-Za-z]:\//);
  const prefix = driveMatch?.[0] ?? (combined.startsWith("/") ? "/" : "");
  const remainder = prefix.length > 0 ? combined.slice(prefix.length) : combined;
  const segments = remainder.split("/");
  const normalizedSegments: string[] = [];
  for (const segment of segments) {
    if (segment.length === 0 || segment === ".") {
      continue;
    }

    if (segment === "..") {
      normalizedSegments.pop();
      continue;
    }

    normalizedSegments.push(segment);
  }

  return `${prefix}${normalizedSegments.join("/")}`.replace(/^\/\/+/, "/");
}

function findScriptBlock(text: string): { contentStartOffset: number; contentEndOffset: number; scriptLanguage: ScriptLanguage } | null {
  const startMatch = /<script\b[^>]*\bsetup\b[^>]*>/i.exec(text)
    ?? /<script\b[^>]*>/i.exec(text);
  if (startMatch === null || startMatch.index === undefined) {
    return null;
  }

  const endPattern = /<\/script>/ig;
  endPattern.lastIndex = startMatch.index + startMatch[0].length;
  const endMatch = endPattern.exec(text);
  if (endMatch === null || endMatch.index === undefined) {
    return null;
  }

  return {
    contentStartOffset: startMatch.index + startMatch[0].length,
    contentEndOffset: endMatch.index,
    scriptLanguage: /\blang\s*=\s*["']ts["']/i.test(startMatch[0]) ? "ts" : "js",
  };
}

function areRangesEqual(
  left: { start: Position; end: Position },
  right: { start: Position; end: Position },
): boolean {
  return left.start.line === right.start.line
    && left.start.character === right.start.character
    && left.end.line === right.end.line
    && left.end.character === right.end.character;
}

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function hasPathExtension(value: string): boolean {
  const lastSegment = value.slice(value.lastIndexOf("/") + 1);
  return /\.[^./]+$/.test(lastSegment);
}

function resolveComponent(
  documentPath: string,
  componentName: string,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): FrontendComponent | null {
  for (const component of enumerateNearbyVueComponents(documentPath, frontendContext, frontendArtifacts)) {
    if (component.componentName === componentName) {
      return component;
    }
  }

  return null;
}

function enumerateNearbyVueComponents(
  documentPath: string,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): FrontendComponent[] {
  const metadataComponents = enumerateFrontendMetadataVueComponents(documentPath, frontendContext, frontendArtifacts);
  if (metadataComponents.length > 0) {
    return metadataComponents;
  }

  const normalizedDocumentPath = normalizePath(documentPath);
  const lastSlash = normalizedDocumentPath.lastIndexOf("/");
  const documentDirectory = lastSlash >= 0 ? normalizedDocumentPath.slice(0, lastSlash) : "";
  if (documentDirectory.length === 0) {
    return [];
  }

  const searchDirectories = new Set<string>();
  searchDirectories.add(documentDirectory);
  searchDirectories.add(`${documentDirectory}/Components`);
  searchDirectories.add(`${documentDirectory}/components`);

  const parentSlash = documentDirectory.lastIndexOf("/");
  if (parentSlash >= 0) {
    const parentDirectory = documentDirectory.slice(0, parentSlash);
    searchDirectories.add(parentDirectory);
    searchDirectories.add(`${parentDirectory}/Components`);
    searchDirectories.add(`${parentDirectory}/components`);
  }

  const results: FrontendComponent[] = [];
  const seen = new Set<string>();
  for (const directory of searchDirectories) {
    try {
      for (const entry of Deno.readDirSync(directory)) {
        if (!entry.isFile || !entry.name.endsWith(".vue")) {
          continue;
        }

        const absolutePath = normalizePath(`${directory}/${entry.name}`);
        if (seen.has(absolutePath)) {
          continue;
        }

        seen.add(absolutePath);
        const componentName = entry.name.slice(0, -".vue".length);
        if (componentName.length === 0 || componentName[0] !== componentName[0].toUpperCase()) {
          continue;
        }

        results.push({
          componentName,
          absolutePath,
          importPath: toImportPath(documentDirectory, absolutePath),
          source: "disk",
        });
      }
    } catch {
      continue;
    }
  }

  return results;
}

function enumerateFrontendMetadataVueComponents(
  documentPath: string,
  frontendContext?: FrontendSemanticContext | null,
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): FrontendComponent[] {
  const relatedDocuments = frontendContext?.relatedDocuments ?? [];
  if (relatedDocuments.length === 0) {
    return [];
  }

  const summaryByPath = createFrontendSummaryArtifactMap(frontendArtifacts);
  const documentDirectory = getDirectoryName(documentPath);
  const results: FrontendComponent[] = [];
  const seenPaths = new Set<string>();
  for (const relatedDocument of relatedDocuments) {
    if (normalizeFrontendDocumentKind(relatedDocument.documentKind) !== "vue") {
      continue;
    }

    const absolutePath = normalizePath(relatedDocument.documentPath);
    if (absolutePath.length === 0 || !seenPaths.add(normalizePathForComparison(absolutePath))) {
      continue;
    }

    const componentName = getComponentNameFromPath(absolutePath);
    if (componentName.length === 0 || componentName[0] !== componentName[0].toUpperCase()) {
      continue;
    }

    results.push({
      componentName,
      absolutePath,
      importPath: toImportPath(documentDirectory, absolutePath),
      source: "metadata",
      summary: summaryByPath.get(normalizePathForComparison(absolutePath)) ?? null,
    });
  }

  return results;
}

function createFrontendSummaryArtifactMap(
  frontendArtifacts?: FrontendArtifactRecord[] | null,
): Map<string, FrontendSummaryArtifact> {
  const results = new Map<string, FrontendSummaryArtifact>();
  for (const artifact of frontendArtifacts ?? []) {
    if (artifact.artifactKind !== "frontend-summary") {
      continue;
    }

    const summary = tryParseFrontendSummaryArtifact(artifact.content);
    if (summary?.documentPath === undefined) {
      continue;
    }

    results.set(normalizePathForComparison(summary.documentPath), summary);
  }

  return results;
}

function tryParseFrontendSummaryArtifact(content: string): FrontendSummaryArtifact | null {
  try {
    return JSON.parse(content) as FrontendSummaryArtifact;
  } catch {
    return null;
  }
}

function normalizeFrontendDocumentKind(value: string | number | null | undefined): FrontendDocumentKind {
  if (typeof value === "number") {
    switch (value) {
      case 0:
        return "jazor";
      case 2:
        return "vue";
      case 3:
        return "javascript";
      case 4:
        return "typescript";
      case 5:
        return "css";
      default:
        return "unknown";
    }
  }

  if (typeof value !== "string") {
    return "unknown";
  }

  switch (value.trim().toLowerCase()) {
    case "jazor":
    case "0":
      return "jazor";
    case "vue":
    case "2":
      return "vue";
    case "typescript":
    case "4":
      return "typescript";
    case "javascript":
    case "3":
      return "javascript";
    case "css":
    case "5":
      return "css";
    case "html":
      return "html";
    default:
      return "unknown";
  }
}

function getComponentNameFromPath(documentPath: string): string {
  const normalizedPath = normalizePath(documentPath);
  const fileName = normalizedPath.slice(normalizedPath.lastIndexOf("/") + 1);
  return fileName.endsWith(".vue")
    ? fileName.slice(0, -".vue".length)
    : "";
}

function findTemplateSymbol(text: string, position: Position): TemplateSymbol | null {
  const offset = toOffset(text, position);
  for (const symbol of findTemplateSymbols(text)) {
    const start = toOffset(text, symbol.range.start);
    const end = toOffset(text, symbol.range.end);
    if (offset >= start && offset <= end) {
      return symbol;
    }
  }

  return null;
}

function findTemplateSymbols(text: string): TemplateSymbol[] {
  const results: TemplateSymbol[] = [];
  const pattern = /<(?<name>[A-Z][A-Za-z0-9_]*)\b/g;
  for (const match of text.matchAll(pattern)) {
    const group = match.groups?.["name"];
    const index = match.index ?? -1;
    if (group === undefined || index < 0) {
      continue;
    }

    const start = index + match[0].indexOf(group);
    results.push({
      name: group,
      range: toRange(text, start, group.length) as { start: Position; end: Position },
    });
  }

  return results;
}

function findTemplateSymbolRanges(text: string, componentName: string): Array<{ start: Position; end: Position }> {
  return findTemplateSymbols(text)
    .filter((symbol) => symbol.name === componentName)
    .map((symbol) => symbol.range);
}

function findDirectiveAttributeRanges(text: string): Array<{ start: Position; end: Position }> {
  const results: Array<{ start: Position; end: Position }> = [];
  const pattern = /(?<name>v-[A-Za-z0-9_-]+|[@:#][A-Za-z0-9_-]+)/g;
  for (const match of text.matchAll(pattern)) {
    const name = match.groups?.["name"];
    const index = match.index ?? -1;
    if (name === undefined || index < 0) {
      continue;
    }

    results.push(toRange(text, index, name.length) as { start: Position; end: Position });
  }

  return results;
}

function findTemplateBlock(text: string): { range: { start: Position; end: Position }; selectionRange: { start: Position; end: Position } } | null {
  const startMatch = /<template\b[^>]*>/i.exec(text);
  if (startMatch === null || startMatch.index === undefined) {
    return null;
  }

  const endPattern = /<\/template>/ig;
  endPattern.lastIndex = startMatch.index + startMatch[0].length;
  const endMatch = endPattern.exec(text);
  if (endMatch === null || endMatch.index === undefined) {
    return null;
  }

  return {
    range: toRange(text, startMatch.index, (endMatch.index + endMatch[0].length) - startMatch.index) as { start: Position; end: Position },
    selectionRange: toRange(text, startMatch.index, startMatch[0].length) as { start: Position; end: Position },
  };
}

function toImportPath(documentDirectory: string, absolutePath: string): string {
  const normalizedDirectory = normalizePath(documentDirectory);
  const normalizedAbsolutePath = normalizePath(absolutePath);
  if (!normalizedAbsolutePath.startsWith(normalizedDirectory)) {
    return normalizedAbsolutePath;
  }

  const relativePath = normalizedAbsolutePath.slice(normalizedDirectory.length).replace(/^\/+/, "");
  return relativePath.startsWith(".") ? relativePath : `./${relativePath}`;
}

function normalizePath(value: string): string {
  return value.replace(/\\/g, "/");
}

function normalizePathForComparison(value: string): string {
  return normalizePath(value).toLowerCase();
}

function toDocumentUri(path: string): string {
  const normalized = normalizePath(path);
  if (/^[A-Za-z]:\//.test(normalized)) {
    return `file:///${normalized}`;
  }

  return `file://${normalized}`;
}

function toOffset(text: string, position: Position): number {
  let currentLine = 0;
  let currentCharacter = 0;
  for (let index = 0; index < text.length; index++) {
    if (currentLine === position.line && currentCharacter === position.character) {
      return index;
    }

    if (text[index] === "\n") {
      currentLine++;
      currentCharacter = 0;
      continue;
    }

    currentCharacter++;
  }

  return text.length;
}

function toRange(text: string, start: number, length: number): unknown {
  return {
    start: toPosition(text, start),
    end: toPosition(text, start + Math.max(length, 0)),
  };
}

function toPosition(text: string, targetOffset: number): Position {
  let line = 0;
  let character = 0;
  const clampedOffset = Math.max(0, Math.min(text.length, targetOffset));
  for (let index = 0; index < clampedOffset; index++) {
    if (text[index] === "\n") {
      line++;
      character = 0;
    } else {
      character++;
    }
  }

  return { line, character };
}

function dedupeLocations(items: unknown[]): unknown[] {
  const seen = new Set<string>();
  const results: unknown[] = [];
  for (const item of items as Array<{ uri: string; range: { start: Position; end: Position } }>) {
    const key = `${item.uri}:${item.range.start.line}:${item.range.start.character}:${item.range.end.line}:${item.range.end.character}`;
    if (seen.has(key)) {
      continue;
    }

    seen.add(key);
    results.push(item);
  }

  return results;
}

function dedupeSemanticTokens(items: unknown[]): unknown[] {
  const seen = new Set<string>();
  const results: unknown[] = [];
  for (const item of items as Array<{ line: number; character: number; length: number; tokenType: string }>) {
    const key = `${item.line}:${item.character}:${item.length}:${item.tokenType}`;
    if (seen.has(key)) {
      continue;
    }

    seen.add(key);
    results.push(item);
  }

  return results.sort((left, right) => {
    const leftToken = left as { line: number; character: number };
    const rightToken = right as { line: number; character: number };
    if (leftToken.line !== rightToken.line) {
      return leftToken.line - rightToken.line;
    }

    return leftToken.character - rightToken.character;
  });
}

function createSemanticToken(
  range: { start: Position; end: Position },
  tokenType: string,
): unknown {
  if (range.start.line !== range.end.line) {
    return {
      line: range.start.line,
      character: range.start.character,
      length: 0,
      tokenType,
      tokenModifiers: [],
    };
  }

  return {
    line: range.start.line,
    character: range.start.character,
    length: range.end.character - range.start.character,
    tokenType,
    tokenModifiers: [],
  };
}

function assertPosition(method: string, position: Position | undefined): asserts position is Position {
  if (position === undefined) {
    throw new Error(`Method '${method}' requires a position.`);
  }
}

function assertRange(method: string, range: Range | undefined): asserts range is Range {
  if (range === undefined) {
    throw new Error(`Method '${method}' requires a range.`);
  }
}

function assertString(method: string, fieldName: string, value: string | undefined): asserts value is string {
  if (typeof value !== "string" || value.trim().length === 0) {
    throw new Error(`Method '${method}' requires '${fieldName}'.`);
  }
}

function requiresTypeScriptTranspile(descriptor: ReturnType<typeof vueCompilerSfc.parse>["descriptor"]): boolean {
  return isTypeScriptLanguage(descriptor.script?.lang)
    || isTypeScriptLanguage(descriptor.scriptSetup?.lang);
}

function isTypeScriptLanguage(value: string | undefined): boolean {
  return value === "ts" || value === "tsx";
}

function rewriteDefaultExport(scriptContent: string, localName: string): string {
  return /\bexport\s+default\b/.test(scriptContent)
    ? scriptContent.replace(/\bexport\s+default\b/, `const ${localName} =`)
    : `${scriptContent}\nconst ${localName} = {};`;
}

function resolveVueStyleSourcePath(documentPath: string, specifier: string | undefined): string | null {
  if (specifier === undefined) {
    return null;
  }

  const normalizedSpecifier = specifier.trim();
  if (normalizedSpecifier.length === 0) {
    return null;
  }

  if (!normalizedSpecifier.startsWith("./") && !normalizedSpecifier.startsWith("../") && !/^[A-Za-z]:\//.test(normalizedSpecifier)) {
    throw new Error(`Unsupported <style src> specifier '${normalizedSpecifier}'. Only relative filesystem paths are supported.`);
  }

  return normalizePath(joinPath(getDirectoryName(documentPath), normalizedSpecifier));
}

function loadVueStyleSource(styleSourcePath: string | null, inlineContent: string): string {
  if (styleSourcePath === null) {
    return inlineContent;
  }

  return normalizeLineEndings(Deno.readTextFileSync(styleSourcePath));
}

function createVueScopeId(documentPath: string): string {
  return createStableHash(normalizePath(documentPath));
}

function resolveCssModuleName(styleBlock: ReturnType<typeof vueCompilerSfc.parse>["descriptor"]["styles"][number]): string | null {
  if (!styleBlock.module) {
    return null;
  }

  if (typeof styleBlock.module === "string" && styleBlock.module.trim().length > 0) {
    return styleBlock.module.trim();
  }

  return "$style";
}

function createCssModuleScopedName(
  localName: string,
  filename: string,
  scopeId: string,
): string {
  const fileStem = sanitizeCssIdentifier(stripFileExtension(getPathBaseName(filename)));
  const localStem = sanitizeCssIdentifier(localName);
  const hash = createStableHash(`${normalizePath(filename)}\n${scopeId}\n${localName}`).slice(0, 8);
  return `jz_${fileStem}_${localStem}_${hash}`;
}

function createStableHash(value: string): string {
  let hash = 2166136261;
  for (const character of value) {
    hash ^= character.charCodeAt(0);
    hash = Math.imul(hash, 16777619);
  }

  return Math.abs(hash >>> 0).toString(16);
}

function getPathBaseName(path: string): string {
  const normalized = normalizePath(path);
  const separatorIndex = Math.max(normalized.lastIndexOf("/"), normalized.lastIndexOf("\\"));
  return separatorIndex >= 0 ? normalized.slice(separatorIndex + 1) : normalized;
}

function stripFileExtension(value: string): string {
  const extensionIndex = value.lastIndexOf(".");
  return extensionIndex > 0 ? value.slice(0, extensionIndex) : value;
}

function sanitizeCssIdentifier(value: string): string {
  const normalized = value.replace(/[^A-Za-z0-9_-]/g, "_");
  if (normalized.length === 0) {
    return "style";
  }

  return /^[A-Za-z_]/.test(normalized)
    ? normalized
    : `_${normalized}`;
}

function formatSfcCompilerMessage(error: unknown): string {
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

function formatTypeScriptDiagnostic(diagnostic: ts.Diagnostic): string {
  const message = ts.flattenDiagnosticMessageText(diagnostic.messageText, "\n");
  if (diagnostic.file === undefined || diagnostic.start === undefined) {
    return message;
  }

  const position = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start);
  return `${diagnostic.file.fileName}(${position.line + 1},${position.character + 1}): ${message}`;
}

function stripTrailingSourceMappingUrl(text: string): string {
  return text.replace(/(?:\r?\n)?\/\/# sourceMappingURL=.*\s*$/u, "");
}

function createSourceMappedModule(
  parts: readonly SourceMappedPart[],
  sourceFilename: string,
  sourceText: string,
): { content: string; sourceMap: string | null } {
  const normalizedParts = parts
    .map((part) => ({
      content: normalizeLineEndings(part.content),
      sourceRange: part.sourceRange,
      sourceLineMappings: part.sourceLineMappings,
      sourceSegments: part.sourceSegments,
    }))
    .filter((part) => part.content.length > 0);
  if (normalizedParts.length === 0) {
    return {
      content: "",
      sourceMap: null,
    };
  }

  const sourceSegments: SourceMapSegment[] = [];
  let content = "";
  let generatedLineOffset = 0;
  for (const part of normalizedParts) {
    if (content.length > 0) {
      content += "\n\n";
      generatedLineOffset += 1;
    }

    content += part.content;
    appendSourceSegments(
      sourceSegments,
      generatedLineOffset,
      part.content,
      part.sourceRange,
      part.sourceLineMappings,
      part.sourceSegments,
    );
    generatedLineOffset += getLineCount(part.content);
  }

  return {
    content,
    sourceMap: createSingleSourceSegmentMap(
      sourceFilename,
      sourceText,
      sourceSegments,
      getLineCount(content),
    ),
  };
}

function createSingleSourceSegmentMap(
  sourceFilename: string,
  sourceText: string,
  sourceSegments: ReadonlyArray<SourceMapSegment>,
  generatedLineCount: number,
): string | null {
  if (sourceSegments.length === 0) {
    return null;
  }

  const sortedSegments = [...sourceSegments].sort((left, right) => {
    if (left.generatedLine !== right.generatedLine) {
      return left.generatedLine - right.generatedLine;
    }

    if (left.generatedColumn !== right.generatedColumn) {
      return left.generatedColumn - right.generatedColumn;
    }

    if (left.sourceLine !== right.sourceLine) {
      return left.sourceLine - right.sourceLine;
    }

    return left.sourceColumn - right.sourceColumn;
  });

  const mappingLines: string[] = [];
  let currentLine = 0;
  let currentLineSegments: string[] = [];
  let previousGeneratedColumn = 0;
  let previousSourceLine = 0;
  let previousSourceColumn = 0;

  for (const segment of sortedSegments) {
    while (currentLine < segment.generatedLine) {
      mappingLines.push(currentLineSegments.join(","));
      currentLine += 1;
      currentLineSegments = [];
      previousGeneratedColumn = 0;
    }

    currentLineSegments.push([
      encodeBase64Vlq(segment.generatedColumn - previousGeneratedColumn),
      encodeBase64Vlq(0),
      encodeBase64Vlq(segment.sourceLine - previousSourceLine),
      encodeBase64Vlq(segment.sourceColumn - previousSourceColumn),
    ].join(""));
    previousGeneratedColumn = segment.generatedColumn;
    previousSourceLine = segment.sourceLine;
    previousSourceColumn = segment.sourceColumn;
  }

  mappingLines.push(currentLineSegments.join(","));
  while (mappingLines.length < generatedLineCount) {
    mappingLines.push("");
  }

  return JSON.stringify({
    version: 3,
    file: sourceFilename,
    sources: [sourceFilename],
    sourcesContent: [sourceText],
    names: [],
    mappings: mappingLines.join(";"),
  });
}

function createSingleSourceLineMap(
  sourceFilename: string,
  sourceText: string,
  lineMappings: ReadonlyArray<number | null>,
): string | null {
  if (lineMappings.length === 0) {
    return null;
  }

  let previousSourceLine = 0;
  const mappingLines = lineMappings.map((sourceLine) => {
    if (sourceLine === null) {
      return "";
    }

    const segment = [
      encodeBase64Vlq(0),
      encodeBase64Vlq(0),
      encodeBase64Vlq(sourceLine - previousSourceLine),
      encodeBase64Vlq(0),
    ].join("");
    previousSourceLine = sourceLine;
    return segment;
  });

  return JSON.stringify({
    version: 3,
    file: sourceFilename,
    sources: [sourceFilename],
    sourcesContent: [sourceText],
    names: [],
    mappings: mappingLines.join(";"),
  });
}

function chainGeneratedLineMappings(
  generatedLineMappings: ReadonlyArray<number | null> | undefined,
  sourceLineMappings: ReadonlyArray<number | null> | undefined,
): ReadonlyArray<number | null> | undefined {
  if (generatedLineMappings === undefined) {
    return sourceLineMappings;
  }

  if (sourceLineMappings === undefined) {
    return generatedLineMappings;
  }

  return generatedLineMappings.map((sourceLine) => {
    if (sourceLine === null || sourceLine < 0 || sourceLine >= sourceLineMappings.length) {
      return null;
    }

    return sourceLineMappings[sourceLine] ?? null;
  });
}

function chainGeneratedSourceSegments(
  generatedSourceMap: unknown,
  sourceSourceMap: unknown,
): ReadonlyArray<SourceMapSegment> | undefined {
  const generatedSourceSegments = extractGeneratedSourceSegments(generatedSourceMap);
  const sourceSegments = extractGeneratedSourceSegments(sourceSourceMap);
  if (generatedSourceSegments === undefined || sourceSegments === undefined) {
    return undefined;
  }

  const sourceSegmentsByGeneratedLine = new Map<number, SourceMapSegment[]>();
  for (const segment of sourceSegments) {
    const lineSegments = sourceSegmentsByGeneratedLine.get(segment.generatedLine);
    if (lineSegments === undefined) {
      sourceSegmentsByGeneratedLine.set(segment.generatedLine, [segment]);
    } else {
      lineSegments.push(segment);
    }
  }
  for (const lineSegments of sourceSegmentsByGeneratedLine.values()) {
    lineSegments.sort((left, right) => left.generatedColumn - right.generatedColumn);
  }

  const chainedSegments: SourceMapSegment[] = [];
  for (const segment of generatedSourceSegments) {
    const sourceSegment = findSourceSegment(
      sourceSegmentsByGeneratedLine,
      segment.sourceLine,
      segment.sourceColumn,
    );
    if (sourceSegment === null) {
      continue;
    }

    chainedSegments.push({
      generatedLine: segment.generatedLine,
      generatedColumn: segment.generatedColumn,
      sourceLine: sourceSegment.sourceLine,
      sourceColumn: mapChainedSourceColumn(segment.sourceColumn, sourceSegment),
    });
  }

  return chainedSegments.length === 0 ? undefined : chainedSegments;
}

function extractGeneratedSourceSegments(sourceMap: unknown): ReadonlyArray<SourceMapSegment> | undefined {
  const sourceMapObject = parseSourceMapObject(sourceMap);
  if (sourceMapObject === null || sourceMapObject.mappings.length === 0) {
    return undefined;
  }

  const segments: SourceMapSegment[] = [];
  const state = { position: 0 };
  const mappings = sourceMapObject.mappings;
  let generatedLine = 0;
  let previousGeneratedColumn = 0;
  let previousSourceIndex = 0;
  let previousSourceLine = 0;
  let previousSourceColumn = 0;

  while (state.position < mappings.length) {
    const current = mappings[state.position];
    if (current === ";") {
      generatedLine += 1;
      previousGeneratedColumn = 0;
      state.position += 1;
      continue;
    }

    if (current === ",") {
      state.position += 1;
      continue;
    }

    const generatedColumnDelta = decodeBase64Vlq(mappings, state);
    if (generatedColumnDelta === null) {
      return undefined;
    }

    previousGeneratedColumn += generatedColumnDelta;
    if (state.position >= mappings.length || mappings[state.position] === "," || mappings[state.position] === ";") {
      continue;
    }

    const sourceIndexDelta = decodeBase64Vlq(mappings, state);
    const sourceLineDelta = decodeBase64Vlq(mappings, state);
    const sourceColumnDelta = decodeBase64Vlq(mappings, state);
    if (sourceIndexDelta === null || sourceLineDelta === null || sourceColumnDelta === null) {
      return undefined;
    }

    previousSourceIndex += sourceIndexDelta;
    previousSourceLine += sourceLineDelta;
    previousSourceColumn += sourceColumnDelta;
    if (previousSourceIndex < 0 || previousSourceLine < 0 || previousSourceColumn < 0) {
      return undefined;
    }

    segments.push({
      generatedLine,
      generatedColumn: previousGeneratedColumn,
      sourceLine: previousSourceLine,
      sourceColumn: previousSourceColumn,
    });

    if (state.position < mappings.length && mappings[state.position] !== "," && mappings[state.position] !== ";") {
      const nameDelta = decodeBase64Vlq(mappings, state);
      if (nameDelta === null) {
        return undefined;
      }
    }
  }

  return segments.length === 0 ? undefined : segments;
}

function extractGeneratedLineMappings(sourceMap: unknown): ReadonlyArray<number | null> | undefined {
  const sourceSegments = extractGeneratedSourceSegments(sourceMap);
  if (sourceSegments === undefined) {
    return undefined;
  }

  const lineMappings: Array<number | null> = [];
  for (const segment of sourceSegments) {
    if (lineMappings[segment.generatedLine] === null || lineMappings[segment.generatedLine] === undefined) {
      lineMappings[segment.generatedLine] = segment.sourceLine;
    }
  }

  for (let index = 0; index < lineMappings.length; index += 1) {
    if (lineMappings[index] === undefined) {
      lineMappings[index] = null;
    }
  }

  return lineMappings;
}

function parseSourceMapObject(sourceMap: unknown): { mappings: string } | null {
  let sourceMapObject = sourceMap;
  if (typeof sourceMapObject === "string") {
    if (sourceMapObject.trim().length === 0) {
      return null;
    }

    try {
      sourceMapObject = JSON.parse(sourceMapObject);
    } catch {
      return null;
    }
  }

  if (typeof sourceMapObject !== "object" || sourceMapObject === null || !("mappings" in sourceMapObject)) {
    return null;
  }

  const mappings = sourceMapObject.mappings;
  if (typeof mappings !== "string") {
    return null;
  }

  return { mappings };
}

function appendLineMappings(
  lineMappings: Array<number | null>,
  content: string,
  sourceRange?: SourceLineRange,
  sourceLineMappings?: ReadonlyArray<number | null>,
): void {
  const lines = normalizeLineEndings(content).split("\n");
  if (sourceLineMappings !== undefined && sourceLineMappings.length > 0) {
    const maxOffset = Math.max(0, (sourceRange?.sourceLineCount ?? 1) - 1);
    for (let index = 0; index < lines.length; index += 1) {
      const mappedSourceLine = index < sourceLineMappings.length
        ? sourceLineMappings[index]
        : null;
      if (mappedSourceLine !== null && mappedSourceLine !== undefined) {
        lineMappings.push(mappedSourceLine);
      } else if (sourceRange === undefined) {
        lineMappings.push(null);
      } else {
        lineMappings.push(sourceRange.sourceStartLine - 1 + Math.min(index, maxOffset));
      }
    }

    return;
  }

  if (sourceRange === undefined) {
    for (let index = 0; index < lines.length; index += 1) {
      lineMappings.push(null);
    }

    return;
  }

  const maxOffset = Math.max(0, sourceRange.sourceLineCount - 1);
  for (let index = 0; index < lines.length; index += 1) {
    lineMappings.push(sourceRange.sourceStartLine - 1 + Math.min(index, maxOffset));
  }
}

function appendSourceSegments(
  sourceSegments: Array<SourceMapSegment>,
  generatedLineOffset: number,
  content: string,
  sourceRange?: SourceLineRange,
  sourceLineMappings?: ReadonlyArray<number | null>,
  partSourceSegments?: ReadonlyArray<SourceMapSegment>,
): void {
  const mappedGeneratedLines = new Set<number>();
  if (partSourceSegments !== undefined && partSourceSegments.length > 0) {
    for (const segment of partSourceSegments) {
      sourceSegments.push({
        generatedLine: generatedLineOffset + segment.generatedLine,
        generatedColumn: segment.generatedColumn,
        sourceLine: segment.sourceLine,
        sourceColumn: segment.sourceColumn,
      });
      mappedGeneratedLines.add(segment.generatedLine);
    }
  }

  const lineMappings: Array<number | null> = [];
  appendLineMappings(lineMappings, content, sourceRange, sourceLineMappings);
  for (let index = 0; index < lineMappings.length; index += 1) {
    if (mappedGeneratedLines.has(index)) {
      continue;
    }

    const sourceLine = lineMappings[index];
    if (sourceLine === null || sourceLine === undefined) {
      continue;
    }

    sourceSegments.push({
      generatedLine: generatedLineOffset + index,
      generatedColumn: 0,
      sourceLine,
      sourceColumn: 0,
    });
  }
}

function getSfcBlockSourceRange(block: SfcBlockLike | null | undefined): SourceLineRange | undefined {
  if (block === null || block === undefined) {
    return undefined;
  }

  return {
    sourceStartLine: block.loc.start.line,
    sourceLineCount: Math.max(1, block.loc.end.line - block.loc.start.line + 1),
  };
}

function getCombinedSfcBlockSourceRange(
  ...blocks: Array<SfcBlockLike | null | undefined>
): SourceLineRange | undefined {
  const ranges = blocks
    .map((block) => getSfcBlockSourceRange(block))
    .filter((range): range is SourceLineRange => range !== undefined);
  if (ranges.length === 0) {
    return undefined;
  }

  const sourceStartLine = Math.min(...ranges.map((range) => range.sourceStartLine));
  const sourceEndLine = Math.max(...ranges.map((range) => range.sourceStartLine + range.sourceLineCount - 1));
  return {
    sourceStartLine,
    sourceLineCount: Math.max(1, sourceEndLine - sourceStartLine + 1),
  };
}

function normalizeLineEndings(text: string): string {
  return text.replace(/\r\n/g, "\n");
}

function getLineCount(text: string): number {
  return normalizeLineEndings(text).split("\n").length;
}

function findSourceSegment(
  sourceSegmentsByGeneratedLine: ReadonlyMap<number, ReadonlyArray<SourceMapSegment>>,
  generatedLine: number,
  generatedColumn: number,
): SourceMapSegment | null {
  const lineSegments = sourceSegmentsByGeneratedLine.get(generatedLine);
  if (lineSegments === undefined || lineSegments.length === 0) {
    return null;
  }

  let candidate: SourceMapSegment | null = null;
  for (const segment of lineSegments) {
    if (segment.generatedColumn > generatedColumn) {
      break;
    }

    candidate = segment;
  }

  return candidate ?? lineSegments[0] ?? null;
}

function mapChainedSourceColumn(
  generatedSourceColumn: number,
  sourceSegment: SourceMapSegment,
): number {
  if (generatedSourceColumn <= sourceSegment.generatedColumn) {
    return sourceSegment.sourceColumn;
  }

  return sourceSegment.sourceColumn + (generatedSourceColumn - sourceSegment.generatedColumn);
}

function decodeBase64Vlq(mappings: string, state: { position: number }): number | null {
  const base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
  let result = 0;
  let shift = 0;

  while (state.position < mappings.length) {
    const current = mappings[state.position];
    const digit = base64Characters.indexOf(current);
    if (digit < 0) {
      return null;
    }

    state.position += 1;
    const hasContinuation = (digit & 32) !== 0;
    result += (digit & 31) << shift;
    shift += 5;
    if (!hasContinuation) {
      const isNegative = (result & 1) === 1;
      result >>= 1;
      return isNegative ? -result : result;
    }
  }

  return null;
}

function encodeBase64Vlq(value: number): string {
  const base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
  let vlq = value < 0
    ? ((-value) << 1) + 1
    : value << 1;
  let encoded = "";

  do {
    let digit = vlq & 31;
    vlq >>>= 5;
    if (vlq > 0) {
      digit |= 32;
    }

    encoded += base64Characters[digit];
  } while (vlq > 0);

  return encoded;
}

function transpileTypeScriptModule(
  text: string,
  filename: string,
  emitSourceMap: boolean,
): { jsContent: string; jsSourceMap: string | null; diagnostics: string[] } {
  const transpiled = ts.transpileModule(text, {
    fileName: filename,
    compilerOptions: {
      module: ts.ModuleKind.ESNext,
      target: ts.ScriptTarget.ES2022,
      sourceMap: emitSourceMap,
      inlineSourceMap: false,
      inlineSources: emitSourceMap,
    },
    reportDiagnostics: true,
  });

  return {
    jsContent: stripTrailingSourceMappingUrl(transpiled.outputText).trim(),
    jsSourceMap: emitSourceMap ? transpiled.sourceMapText ?? null : null,
    diagnostics: (transpiled.diagnostics ?? []).map(formatTypeScriptDiagnostic),
  };
}

await main();
