type Position = {
  line: number;
  character: number;
};

type RequestEnvelope = {
  id: string;
  method: string;
  payload: {
    documentPath: string;
    text: string;
    position?: Position;
    includeDeclaration?: boolean;
    newName?: string;
  };
};

type ResponseEnvelope = {
  id: string;
  success: boolean;
  result?: unknown;
  error?: string;
};

const encoder = new TextEncoder();

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
        const response = handleLine(line);
        await Deno.stdout.write(encoder.encode(JSON.stringify(response) + "\n"));
      }

      newlineIndex = buffered.indexOf("\n");
    }
  }
}

function handleLine(line: string): ResponseEnvelope {
  try {
    const request = JSON.parse(line) as RequestEnvelope;
    const result = dispatch(request.method, request.payload);
    return {
      id: request.id,
      success: true,
      result,
    };
  } catch (error) {
    return {
      id: "",
      success: false,
      error: error instanceof Error ? error.message : String(error),
    };
  }
}

function dispatch(method: string, payload: RequestEnvelope["payload"]): unknown {
  switch (method) {
    case "template/diagnostics":
      return getDiagnostics(payload.documentPath, payload.text);
    case "template/completion":
      assertPosition(method, payload.position);
      return getCompletionItems(payload.documentPath, payload.text, payload.position);
    case "template/hover":
      assertPosition(method, payload.position);
      return getHover(payload.documentPath, payload.text, payload.position);
    case "template/definition":
      assertPosition(method, payload.position);
      return getDefinition(payload.documentPath, payload.text, payload.position);
    case "template/references":
      assertPosition(method, payload.position);
      return getReferences(payload.documentPath, payload.text, payload.position, payload.includeDeclaration !== false);
    case "template/rename":
      assertPosition(method, payload.position);
      return getRename(payload.documentPath, payload.text, payload.position, payload.newName ?? "");
    case "template/documentSymbols":
      return getDocumentSymbols(payload.text);
    case "template/semanticTokens":
      return getSemanticTokens(payload.text);
    default:
      throw new Error(`Unsupported method '${method}'.`);
  }
}

function getDiagnostics(documentPath: string, text: string): unknown[] {
  const diagnostics: unknown[] = [];

  for (const symbol of findTemplateSymbols(text)) {
    if (resolveComponent(documentPath, symbol.name) !== null) {
      continue;
    }

    diagnostics.push({
      range: symbol.range,
      severity: 2,
      code: "JAZORVUEFRONTEND001",
      source: "Jazor.VueHost.Frontend",
      message: `Razor component '${symbol.name}' could not be resolved to a nearby Vue file.`,
    });
  }

  return diagnostics;
}

function getCompletionItems(documentPath: string, text: string, position: Position): unknown[] {
  const tagPrefix = getTagCompletionPrefix(text, position);
  if (tagPrefix === null) {
    return [];
  }

  return enumerateNearbyVueComponents(documentPath)
    .filter((item) => item.componentName.toLowerCase().startsWith(tagPrefix.toLowerCase()))
    .map((item) => ({
      label: item.componentName,
      kind: 7,
      detail: item.importPath,
      documentation: `Vue component discovered on disk at \`${item.importPath}\`.`,
    }));
}

function getTagCompletionPrefix(text: string, position: Position): string | null {
  const offset = toOffset(text, position);
  const prefix = text.slice(0, Math.min(offset, text.length));
  const match = prefix.match(/<\/?([A-Za-z0-9_]*)$/);
  return match === null ? null : match[1];
}

function getHover(documentPath: string, text: string, position: Position): unknown | null {
  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return null;
  }

  const component = resolveComponent(documentPath, symbol.name);
  if (component === null) {
    return null;
  }

  return {
    contents: {
      kind: "markdown",
      value: `\`${symbol.name}\` resolved from Razor markup to \`${component.importPath}\`\n\nkind: \`VueComponent\``,
    },
    range: symbol.range,
  };
}

function getDefinition(documentPath: string, text: string, position: Position): unknown[] {
  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return [];
  }

  const component = resolveComponent(documentPath, symbol.name);
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

function getReferences(documentPath: string, text: string, position: Position, includeDeclaration: boolean): unknown[] {
  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return [];
  }

  const component = resolveComponent(documentPath, symbol.name);
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

function getRename(documentPath: string, text: string, position: Position, newName: string): unknown | null {
  if (newName.trim().length === 0) {
    return null;
  }

  const references = getReferences(documentPath, text, position, true) as Array<{ uri: string; range: { start: Position } }>;
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

function getDocumentSymbols(text: string): unknown[] {
  const componentSymbols = findTemplateSymbols(text)
    .map((symbol) => ({
      name: symbol.name,
      kind: 5,
      range: symbol.range,
      selectionRange: symbol.range,
    }));
  const templateBlock = findTemplateBlock(text);
  if (templateBlock === null) {
    return componentSymbols;
  }

  const templateStart = toOffset(text, templateBlock.range.start);
  const templateEnd = toOffset(text, templateBlock.range.end);
  const children = componentSymbols.filter((symbol) => {
    const start = toOffset(text, symbol.selectionRange.start);
    const end = toOffset(text, symbol.selectionRange.end);
    return start >= templateStart && end <= templateEnd;
  });

  return [{
    name: "Template",
    kind: 2,
    range: templateBlock.range,
    selectionRange: templateBlock.selectionRange,
    children: children.length === 0 ? undefined : children,
  }];
}

function getSemanticTokens(text: string): unknown[] {
  const tokens: unknown[] = [];

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

  return dedupeSemanticTokens(tokens);
}

function resolveComponent(
  documentPath: string,
  componentName: string,
): { absolutePath: string; importPath: string } | null {
  for (const component of enumerateNearbyVueComponents(documentPath)) {
    if (component.componentName === componentName) {
      return {
        absolutePath: component.absolutePath,
        importPath: component.importPath,
      };
    }
  }

  return null;
}

function enumerateNearbyVueComponents(documentPath: string): Array<{ componentName: string; absolutePath: string; importPath: string }> {
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

  const results: Array<{ componentName: string; absolutePath: string; importPath: string }> = [];
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
        });
      }
    } catch {
      continue;
    }
  }

  return results;
}

function findTemplateSymbol(text: string, position: Position): { name: string; range: unknown } | null {
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

function findTemplateSymbols(text: string): Array<{ name: string; range: { start: Position; end: Position } }> {
  const results: Array<{ name: string; range: { start: Position; end: Position } }> = [];
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

await main();
