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
    position: Position;
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

type ImportRecord = {
  localName: string;
  source: string;
  declarationStart: number;
  declarationLength: number;
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
    case "template/completion":
      return getCompletionItems(payload.text, payload.position);
    case "template/hover":
      return getHover(payload.documentPath, payload.text, payload.position);
    case "template/definition":
      return getDefinition(payload.documentPath, payload.text, payload.position);
    case "template/references":
      return getReferences(payload.documentPath, payload.text, payload.position, payload.includeDeclaration !== false);
    case "template/rename":
      return getRename(payload.documentPath, payload.text, payload.position, payload.newName ?? "");
    default:
      throw new Error(`Unsupported method '${method}'.`);
  }
}

function getCompletionItems(text: string, position: Position): unknown[] {
  const offset = toOffset(text, position);
  const prefix = text.slice(0, Math.min(offset, text.length));
  if (!prefix.endsWith("<") && !prefix.endsWith("</")) {
    return [];
  }

  return parseImports(text).map((item) => ({
    label: item.localName,
    kind: 7,
    detail: item.source,
    documentation: `Vue component imported from \`${item.source}\`.`,
  }));
}

function getHover(documentPath: string, text: string, position: Position): unknown | null {
  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return null;
  }

  const importRecord = parseImports(text).find((item) => item.localName === symbol.name);
  if (importRecord === undefined) {
    return null;
  }

  return {
    contents: {
      kind: "markdown",
      value: `\`${importRecord.localName}\` from \`${importRecord.source}\`\n\nkind: \`VueImport\``,
    },
    range: symbol.range,
  };
}

function getDefinition(documentPath: string, text: string, position: Position): unknown[] {
  const symbol = findTemplateSymbol(text, position);
  if (symbol === null) {
    return [];
  }

  const importRecord = parseImports(text).find((item) => item.localName === symbol.name);
  if (importRecord === undefined) {
    return [];
  }

  return [{
    uri: toDocumentUri(resolveImportPath(documentPath, importRecord.source)),
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

  const importRecord = parseImports(text).find((item) => item.localName === symbol.name);
  if (importRecord === undefined) {
    return [];
  }

  const declarationRange = toRange(text, importRecord.declarationStart, importRecord.declarationLength);
  const results: unknown[] = [];
  if (includeDeclaration) {
    results.push({
      uri: toDocumentUri(documentPath),
      range: declarationRange,
    });
  }

  const pattern = new RegExp(`\\b${escapeRegex(symbol.name)}\\b`, "g");
  for (const match of text.matchAll(pattern)) {
    const start = match.index ?? -1;
    if (start < 0) {
      continue;
    }

    const range = toRange(text, start, symbol.name.length);
    if (!includeDeclaration && sameRange(range, declarationRange)) {
      continue;
    }

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

function parseImports(text: string): ImportRecord[] {
  const results: ImportRecord[] = [];
  const pattern = /^\s*@vueimport\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s+from\s+["'](?<source>[^"']+)["']\s*$/gm;
  for (const match of text.matchAll(pattern)) {
    const groups = match.groups;
    if (groups === undefined) {
      continue;
    }

    const localName = groups["name"];
    const source = groups["source"];
    const fullText = match[0];
    const fullIndex = match.index ?? -1;
    const localNameOffset = fullText.indexOf(localName);
    if (fullIndex < 0 || localNameOffset < 0) {
      continue;
    }

    results.push({
      localName,
      source,
      declarationStart: fullIndex + localNameOffset,
      declarationLength: localName.length,
    });
  }

  return results;
}

function findTemplateSymbol(text: string, position: Position): { name: string; range: unknown } | null {
  const offset = toOffset(text, position);
  const pattern = /<(?<name>[A-Z][A-Za-z0-9_]*)\b/g;
  for (const match of text.matchAll(pattern)) {
    const group = match.groups?.["name"];
    const index = match.index ?? -1;
    if (group === undefined || index < 0) {
      continue;
    }

    const start = index + match[0].indexOf(group);
    const end = start + group.length;
    if (offset >= start && offset <= end) {
      return {
        name: group,
        range: toRange(text, start, group.length),
      };
    }
  }

  return null;
}

function resolveImportPath(documentPath: string, source: string): string {
  if (/^[A-Za-z]:[\\/]/.test(source) || source.startsWith("/")) {
    return normalizePath(source);
  }

  const lastSlash = Math.max(documentPath.lastIndexOf("/"), documentPath.lastIndexOf("\\"));
  const baseDirectory = lastSlash >= 0 ? documentPath.slice(0, lastSlash) : "";
  const separator = baseDirectory.includes("\\") ? "\\" : "/";
  return normalizePath(baseDirectory.length === 0 ? source : `${baseDirectory}${separator}${source}`);
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

function sameRange(left: { start: Position; end: Position }, right: { start: Position; end: Position }): boolean {
  return left.start.line === right.start.line
    && left.start.character === right.start.character
    && left.end.line === right.end.line
    && left.end.character === right.end.character;
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

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

await main();
