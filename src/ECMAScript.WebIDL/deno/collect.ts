import { listAll as listAllCss } from "@webref/css";
import { listAll as listAllEvents } from "@webref/events";
import { listAll as listAllIdl } from "@webref/idl";
import * as webidl2 from "webidl2";
import type {
  InterfaceEvent,
  InterfaceEventMap,
  WebIdlDeclaration,
  WebIdlFile,
  WebIdlInventory,
} from "./types.ts";

const parserVersion = "webidl2@24.4.1";
const webrefIdlVersion = "@webref/idl@3.46.1";
const webrefCssVersion = "@webref/css@6.12.7";
const webrefEventsVersion = "@webref/events@1.11.3";

const declarationPriority: Record<string, number> = {
  typedef: 0,
  namespace: 1,
  callback: 2,
  "callback interface": 3,
  enum: 4,
  "interface mixin": 5,
  dictionary: 6,
  interface: 7,
  includes: 8,
};

const cssPropertyName = function (name: string): string {
  const camel = name
    .replace(/^-(\w)/, (_, c) => c)
    .replace(/-(\w)/g, (_, c) => c.toUpperCase());
  return camel === "float" ? "_float" : camel;
};

const generateCssIdl = function (properties: string[]): string {
  return `partial interface CSSStyleDeclaration {${properties
    .map(
      (property) =>
        `\n  [CEReactions] attribute [LegacyNullToEmptyString] CSSOMString ${cssPropertyName(
          property,
        )};`,
    )
    .join("")}\n};`;
};

function parseArgs(args: string[]): string | undefined {
  for (let i = 0; i < args.length; i++) {
    if (args[i] === "--out") {
      return args[i + 1];
    }
  }

  return undefined;
}

function getDirectoryName(path: string): string | undefined {
  const normalized = path.replace(/\\/g, "/");
  const index = normalized.lastIndexOf("/");
  if (index <= 0) {
    return undefined;
  }

  return path.slice(0, index);
}

function normalizeDeclaration(root: webidl2.IDLRootType): WebIdlDeclaration {
  const payload = JSON.parse(JSON.stringify(root));
  const declaration: WebIdlDeclaration = {
    kind: root.type,
    payload,
  };

  if ("name" in root && typeof root.name === "string") {
    declaration.name = root.name;
  }

  if ("partial" in root && typeof root.partial === "boolean") {
    declaration.partial = root.partial;
  }

  if ("inheritance" in root && typeof root.inheritance === "string") {
    declaration.inheritance = root.inheritance;
  }

  if ("target" in root && typeof root.target === "string") {
    declaration.target = root.target;
  }

  if ("includes" in root && typeof root.includes === "string") {
    declaration.includes = root.includes;
  }

  if ("members" in root && Array.isArray(root.members)) {
    declaration.memberCount = root.members.length;
  }

  return declaration;
}

async function collectFiles(): Promise<Map<string, string>> {
  const [idlFiles, cssFiles] = await Promise.all([listAllIdl(), listAllCss()]);
  const files = new Map<string, string>();

  for (const [fileName, file] of Object.entries(idlFiles)) {
    files.set(fileName, await file.text());
  }

  for (const [fileName, data] of Object.entries(cssFiles)) {
    const properties = data.properties.map((property) => property.name);
    if (properties.length === 0) {
      continue;
    }

    const existing = files.get(fileName) ?? "";
    files.set(fileName, `${existing}\n${generateCssIdl(properties)}\n`);
  }

  return files;
}

async function collectInterfaceEvents(): Promise<InterfaceEventMap[]> {
  const allEvents = await listAllEvents();
  const eventMap = new Map<string, Map<string, string>>();

  for (const item of Object.values(allEvents)) {
    for (const target of item.targets) {
      const nested = eventMap.get(target.target) ?? new Map<string, string>();
      nested.set(item.type, item.interface);
      eventMap.set(target.target, nested);

      for (const path of target.bubblingPath ?? []) {
        const bubbling = eventMap.get(path) ?? new Map<string, string>();
        bubbling.set(item.type, item.interface);
        eventMap.set(path, bubbling);
      }
    }
  }

  return [...eventMap.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([interfaceName, events]) => {
      const normalizedEvents: InterfaceEvent[] = [...events.entries()]
        .sort(([left], [right]) => left.localeCompare(right))
        .map(([eventType, eventInterfaceName]) => ({
          eventType,
          interfaceName: eventInterfaceName,
        }));

      return {
        interfaceName,
        events: normalizedEvents,
      };
    });
}

async function collectInventory(): Promise<WebIdlInventory> {
  const [files, interfaceEvents] = await Promise.all([
    collectFiles(),
    collectInterfaceEvents(),
  ]);

  const normalizedFiles: WebIdlFile[] = [];
  const declarationsByKind = new Map<string, number>();
  let declarationCount = 0;

  for (const [fileName, source] of [...files.entries()].sort(([left], [right]) =>
    left.localeCompare(right),
  )) {
    const ast = webidl2.parse(source).sort(
      (left, right) =>
        (declarationPriority[left.type] ?? 999) -
        (declarationPriority[right.type] ?? 999),
    );
    const namespace = ast.find((item) => item.type === "namespace")?.name;
    const declarations = ast.map((root) => {
      declarationsByKind.set(root.type, (declarationsByKind.get(root.type) ?? 0) + 1);
      declarationCount++;
      return normalizeDeclaration(root);
    });

    normalizedFiles.push({
      fileName,
      namespace,
      declarations,
    });
  }

  return {
    schemaVersion: 1,
    generatedAt: new Date().toISOString(),
    source: {
      parser: parserVersion,
      webrefIdl: webrefIdlVersion,
      webrefCss: webrefCssVersion,
      webrefEvents: webrefEventsVersion,
    },
    files: normalizedFiles,
    interfaceEvents,
    stats: {
      fileCount: normalizedFiles.length,
      declarationCount,
      interfaceEventTargetCount: interfaceEvents.length,
      declarationsByKind: Object.fromEntries(
        [...declarationsByKind.entries()].sort(([left], [right]) =>
          left.localeCompare(right),
        ),
      ),
    },
  };
}

const outputPath = parseArgs(Deno.args);
const inventory = await collectInventory();
const json = JSON.stringify(inventory, null, 2);

if (!outputPath) {
  console.log(json);
} else {
  const outputDirectory = getDirectoryName(outputPath);
  if (outputDirectory) {
    await Deno.mkdir(outputDirectory, { recursive: true });
  }
  await Deno.writeTextFile(outputPath, `${json}\n`);
}
