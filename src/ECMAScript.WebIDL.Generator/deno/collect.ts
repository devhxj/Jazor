import { readFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { listAll as listAllCss } from "@webref/css";
import { listAll as listAllEvents } from "@webref/events";
import { listAll as listAllIdl } from "@webref/idl";
import "@webref/xref";
import { parseFragment } from "parse5";
import * as webidl2 from "webidl2";
import type {
  InterfaceEvent,
  InterfaceEventMap,
  WebIdlArgumentDocumentation,
  WebIdlDeclaration,
  WebIdlDocumentation,
  WebIdlFile,
  WebIdlInventory,
  WebIdlMemberDocumentation,
  WebIdlSpecificationSource,
} from "./types.ts";

const parserVersion = "webidl2@24.5.0";
const webrefIdlVersion = "@webref/idl@3.82.0";
const webrefCssVersion = "@webref/css@8.7.1";
const webrefEventsVersion = "@webref/events@1.24.2";
const webrefXrefVersion = "@webref/xref@1.2.11";

interface CollectedIdlFile {
  fileName: string;
  source: string;
}

interface XrefSpec {
  shortname: string;
  title?: string;
  shortTitle?: string;
  url?: string;
  nightly?: XrefSpecVersion;
  release?: XrefSpecVersion;
  series?: XrefSeries;
}

interface XrefSpecVersion {
  url?: string;
  alternateUrls?: string[];
  repository?: string;
  sourcePath?: string;
}

interface XrefSeries {
  nightlyUrl?: string;
  releaseUrl?: string;
}

interface XrefHeading {
  id?: string;
  href: string;
  title: string;
  number?: string;
}

interface XrefDefinition {
  id: string;
  href: string;
  linkingText: string[];
  type: string;
  for: string[];
  heading: XrefHeading;
  htmlProse?: string;
  links?: XrefDeveloperLink[];
}

interface XrefDeveloperLink {
  type: string;
  name: string;
  href: string;
}

interface XrefDefinitionsFile {
  spec?: {
    title?: string;
  };
  dfns?: XrefDefinition[];
}

interface DocumentationCandidate {
  types: readonly string[];
  names: readonly string[];
  owners?: readonly string[];
  requiresQualifiedOwner?: boolean;
}

interface WebrefIdlFile {
  text(): Promise<string>;
}

interface WebrefCssProperty {
  name: string;
}

interface WebrefCssFeatures {
  properties: WebrefCssProperty[];
}

interface WebrefEventTarget {
  target: string;
  bubblingPath?: string[];
}

interface WebrefEvent {
  type: string;
  interface: string;
  targets: WebrefEventTarget[];
}

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

function normalizeSpecificationUrl(value: string): string | undefined {
  try {
    const url = new URL(value);
    url.hash = "";
    url.search = "";
    const path = url.pathname.replace(/\/+$/, "") || "/";
    return `${url.protocol}//${url.host}${path}`.toLowerCase();
  } catch {
    return undefined;
  }
}

function readSpecificationSource(source: string): WebIdlSpecificationSource | undefined {
  const match = source.match(/^\/\/ Source:\s*(.+?)\s+\((https?:\/\/[^\s)]+)\)\s*$/m);
  if (!match) {
    return undefined;
  }

  return {
    title: match[1].trim(),
    url: match[2],
  };
}

function getSpecUrls(spec: XrefSpec): string[] {
  return [
    spec.url,
    spec.nightly?.url,
    spec.release?.url,
    spec.series?.nightlyUrl,
    spec.series?.releaseUrl,
    ...(spec.nightly?.alternateUrls ?? []),
    ...(spec.release?.alternateUrls ?? []),
  ].filter((url): url is string => typeof url === "string");
}

class XrefCatalog {
  private readonly _definitionsByShortname = new Map<string, Promise<XrefDefinitionsFile | undefined>>();

  private constructor(
    private readonly _rootDirectory: string,
    private readonly _specs: XrefSpec[],
  ) {}

  public static async create(): Promise<XrefCatalog> {
    const entryPoint = fileURLToPath(import.meta.resolve("@webref/xref"));
    const rootDirectory = dirname(entryPoint);
    const specs = JSON.parse(
      await readFile(join(rootDirectory, "specs.json"), "utf8"),
    ) as XrefSpec[];
    return new XrefCatalog(rootDirectory, specs);
  }

  public async resolve(
    source: WebIdlSpecificationSource | undefined,
    fileName: string,
  ): Promise<{ source?: WebIdlSpecificationSource; definitions?: XrefDefinitionsFile; specification?: XrefSpec }> {
    if (!source) {
      return {};
    }

    const specification = this.findSpecification(source, fileName);
    const resolvedSource = specification ? { ...source, shortname: specification.shortname } : source;
    if (!specification) {
      return { source: resolvedSource };
    }

    return {
      source: resolvedSource,
      definitions: await this.getDefinitions(specification.shortname),
      specification,
    };
  }

  private findSpecification(source: WebIdlSpecificationSource, fileName: string): XrefSpec | undefined {
    const normalizedUrl = normalizeSpecificationUrl(source.url);
    if (normalizedUrl) {
      const exactMatch = this._specs.find((spec) =>
        getSpecUrls(spec).some((url) => normalizeSpecificationUrl(url) === normalizedUrl),
      );
      if (exactMatch) {
        return exactMatch;
      }
    }

    const normalizedTitle = source.title.trim().toLocaleLowerCase();
    const titleMatch = this._specs.find((spec) =>
      [spec.title, spec.shortTitle].some(
        (title) => title?.trim().toLocaleLowerCase() === normalizedTitle,
      ),
    );
    if (titleMatch) {
      return titleMatch;
    }

    const fileShortname = fileName.replace(/\.idl$/i, "");
    return this._specs.find((spec) => spec.shortname === fileShortname);
  }

  private getDefinitions(shortname: string): Promise<XrefDefinitionsFile | undefined> {
    let result = this._definitionsByShortname.get(shortname);
    if (!result) {
      result = this.readDefinitions(shortname);
      this._definitionsByShortname.set(shortname, result);
    }

    return result;
  }

  private async readDefinitions(shortname: string): Promise<XrefDefinitionsFile | undefined> {
    for (const versionDirectory of ["ed", "tr"]) {
      const path = join(this._rootDirectory, versionDirectory, "dfns", `${shortname}.json`);
      try {
        return JSON.parse(await readFile(path, "utf8")) as XrefDefinitionsFile;
      } catch (error) {
        if (typeof error === "object" && error !== null && "code" in error && error.code === "ENOENT") {
          continue;
        }

        throw error;
      }
    }

    return undefined;
  }
}

interface SpecificationSourceBlock {
  start: number;
  end: number;
  tagName: string;
  text: string;
  searchableText: string;
  searchableSource: string;
}

interface SpecificationSourceSection {
  start: number;
  end: number;
  searchableTitle: string;
}

interface SpecificationSourceDocument {
  blocks: SpecificationSourceBlock[];
  sections: SpecificationSourceSection[];
  anchors: ReadonlyMap<string, number>;
}

/**
 * Webref's cross-reference package deliberately stores definitions, not the
 * prose that explains how to use an API. The matching source file recorded in
 * browser-specs is the authoritative fallback for developer-facing comments.
 * This catalog retains only short, local prose blocks in memory; source files
 * themselves are never written into the generated artifact.
 */
class SpecificationSourceCatalog {
  private readonly _documentsByShortname = new Map<string, Promise<SpecificationSourceDocument | undefined>>();
  private readonly _pending: Array<() => void> = [];
  private _activeFetches = 0;

  public async getProse(specification: XrefSpec | undefined, definition: XrefDefinition): Promise<string | undefined> {
    const sourceUrl = specification ? getSpecificationSourceUrl(specification) : undefined;
    if (!specification || !sourceUrl) {
      return undefined;
    }

    let document = this._documentsByShortname.get(specification.shortname);
    if (!document) {
      document = this.schedule(async () => {
        const source = await fetchSpecificationSource(sourceUrl);
        return source ? createSpecificationSourceDocument(source) : undefined;
      });
      this._documentsByShortname.set(specification.shortname, document);
    }

    return findSpecificationProse(await document, definition);
  }

  private schedule<T>(work: () => Promise<T>): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      const run = () => {
        this._activeFetches++;
        work()
          .then(resolve, reject)
          .finally(() => {
            this._activeFetches--;
            this.runPending();
          });
      };

      this._pending.push(run);
      this.runPending();
    });
  }

  private runPending(): void {
    while (this._activeFetches < 6 && this._pending.length > 0) {
      this._pending.shift()!();
    }
  }
}

function getSpecificationSourceUrl(specification: XrefSpec): string | undefined {
  const repository = specification.nightly?.repository;
  const sourcePath = specification.nightly?.sourcePath;
  if (!repository || !sourcePath) {
    return undefined;
  }

  try {
    const repositoryUrl = new URL(repository);
    if (repositoryUrl.hostname !== "github.com") {
      return undefined;
    }

    const [owner, repositoryName] = repositoryUrl.pathname
      .split("/")
      .filter(Boolean)
      .map((part) => part.replace(/\.git$/i, ""));
    if (!owner || !repositoryName) {
      return undefined;
    }

    const encodedPath = sourcePath
      .split("/")
      .filter(Boolean)
      .map((part) => encodeURIComponent(part))
      .join("/");
    return `https://raw.githubusercontent.com/${owner}/${repositoryName}/HEAD/${encodedPath}`;
  } catch {
    return undefined;
  }
}

async function fetchSpecificationSource(sourceUrl: string): Promise<string | undefined> {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 20_000);
  try {
    const response = await fetch(sourceUrl, {
      headers: {
        "User-Agent": "Jazor-WebIDL-Generator/1.0",
      },
      signal: controller.signal,
    });
    if (!response.ok) {
      return undefined;
    }

    return await response.text();
  } catch {
    // Source prose is additive. Exact xref links remain available when a spec
    // repository is temporarily unreachable during collection.
    return undefined;
  } finally {
    clearTimeout(timeout);
  }
}

function createSpecificationSourceDocument(source: string): SpecificationSourceDocument {
  const maskedSource = maskCodeBlocks(source);
  return {
    blocks: extractSpecificationProseBlocks(maskedSource),
    sections: extractSpecificationSections(maskedSource),
    anchors: extractSpecificationAnchorPositions(maskedSource),
  };
}

function maskCodeBlocks(source: string): string {
  return source.replace(/<(?:pre|xmp|script|style)\b[^>]*>[\s\S]*?<\/(?:pre|xmp|script|style)>/gi, (match) =>
    match.replace(/[^\r\n]/g, " "));
}

function extractSpecificationProseBlocks(source: string): SpecificationSourceBlock[] {
  const blocks: SpecificationSourceBlock[] = [];
  const startPattern = /<(p|li|dt|dd)\b[^>]*>/gi;
  let match: RegExpExecArray | null;
  while ((match = startPattern.exec(source)) !== null) {
    const start = match.index;
    const end = findSpecificationBlockEnd(source, startPattern.lastIndex, match[1]);
    if (end <= startPattern.lastIndex) {
      continue;
    }

    const raw = source.slice(start, end);
    const text = normalizeSpecificationProse(toSpecificationPlainText(raw));
    if (!text) {
      continue;
    }

    blocks.push({
      start,
      end,
      tagName: match[1].toLocaleLowerCase(),
      text,
      searchableText: normalizeSourceSearchText(text),
      searchableSource: normalizeSourceSearchText(raw),
    });
    startPattern.lastIndex = end;
  }

  return blocks;
}

function extractSpecificationAnchorPositions(source: string): ReadonlyMap<string, number> {
  const anchors = new Map<string, number>();
  const idPattern = /\bid\s*=\s*(?:"([^"]+)"|'([^']+)'|([^\s>]+))/gi;
  let match: RegExpExecArray | null;
  while ((match = idPattern.exec(source)) !== null) {
    const id = match[1] ?? match[2] ?? match[3];
    if (id && !anchors.has(id)) {
      anchors.set(id, match.index);
    }
  }

  return anchors;
}

function findSpecificationBlockEnd(source: string, contentStart: number, tagName: string): number {
  const closeIndex = source.toLocaleLowerCase().indexOf(`</${tagName.toLocaleLowerCase()}`, contentStart);
  const blankLineIndex = source.indexOf("\n\n", contentStart);
  const boundaryPattern = /<(?:p|li|dt|dd|pre|xmp|h[1-6]|section|div)\b/gi;
  boundaryPattern.lastIndex = contentStart;
  const boundaryIndex = boundaryPattern.exec(source)?.index ?? -1;
  return [closeIndex, blankLineIndex, boundaryIndex]
    .filter((index) => index >= contentStart)
    .sort((left, right) => left - right)[0] ?? source.length;
}

function extractSpecificationSections(source: string): SpecificationSourceSection[] {
  const matches = [...source.matchAll(/^(?:\s*#{1,6}\s+|<h[1-6]\b[^>]*>)(.*)$/gim)];
  return matches.flatMap((match, index) => {
    const title = normalizeSourceSearchText(toSpecificationPlainText(match[1]));
    if (!title) {
      return [];
    }

    return [{
      start: match.index ?? 0,
      end: matches[index + 1]?.index ?? source.length,
      searchableTitle: title,
    }];
  });
}

function toSpecificationPlainText(value: string): string {
  const prepared = value
    .replace(/<!--([\s\S]*?)-->/g, " ")
    .replace(/\{\{([^{}]+)\}\}/g, (_, token: string) => {
      const display = token.split("|").at(-1)?.trim() ?? token;
      return display.includes("/") ? display.slice(display.lastIndexOf("/") + 1) : display;
    })
    .replace(/\[=([^=]+)=\]/g, "$1")
    .replace(/\[\[([^\]]+)\]\]/g, "$1")
    .replace(/\|([^|\n]+)\|/g, "$1");
  return getHtmlProseText(prepared) ?? "";
}

function normalizeSpecificationProse(value: string): string | undefined {
  const text = value
    .replace(/[\u00a0\u2007\u202f]/g, " ")
    .replace(/[\u2018\u2019]/g, "'")
    .replace(/[\u201c\u201d]/g, "\"")
    .replace(/[\u2013\u2014]/g, "-")
    .replace(/\s+/g, " ")
    .trim();
  if (text.length < 12) {
    return undefined;
  }

  const maximumLength = 640;
  if (text.length <= maximumLength) {
    return text;
  }

  const sentenceEnd = Math.max(
    text.lastIndexOf(". ", maximumLength),
    text.lastIndexOf("! ", maximumLength),
    text.lastIndexOf("? ", maximumLength),
  );
  const end = sentenceEnd >= 160
    ? sentenceEnd + 1
    : Math.max(text.lastIndexOf(" ", maximumLength), maximumLength);
  return `${text.slice(0, end).trimEnd()}...`;
}

function normalizeSourceSearchText(value: string): string {
  return value.toLocaleLowerCase().replace(/[^a-z0-9]+/g, "");
}

function findSpecificationProse(
  document: SpecificationSourceDocument | undefined,
  definition: XrefDefinition,
): string | undefined {
  if (!document) {
    return undefined;
  }

  const anchorProse = findAnchorProse(document, definition);
  if (anchorProse) {
    return anchorProse;
  }

  if (isDeclarationDefinition(definition)) {
    const sectionProse = findSectionProse(document, definition);
    if (sectionProse) {
      return sectionProse;
    }
  }

  return findDefinitionProse(document, definition);
}

function findAnchorProse(
  document: SpecificationSourceDocument,
  definition: XrefDefinition,
): string | undefined {
  const anchorIds = [definition.id, definition.heading.id]
    .filter((id): id is string => typeof id === "string" && id.length > 0);
  for (const anchorId of anchorIds) {
    const position = document.anchors.get(anchorId);
    if (position === undefined) {
      continue;
    }

    const containingBlock = document.blocks.find((block) =>
      block.start <= position && position < block.end,
    );
    if (containingBlock && !isLikelyUsageExpression(containingBlock.text)) {
      return containingBlock.text;
    }
  }

  return undefined;
}

function isDeclarationDefinition(definition: XrefDefinition): boolean {
  return ["callback", "callback interface", "dictionary", "enum", "interface", "namespace", "typedef"].includes(definition.type);
}

function findSectionProse(document: SpecificationSourceDocument, definition: XrefDefinition): string | undefined {
  const heading = normalizeSourceSearchText(definition.heading.title);
  const names = definition.linkingText
    .map(normalizeSourceSearchText)
    .filter((name) => name.length >= 3);
  const section = document.sections.find((candidate) =>
    candidate.searchableTitle.includes(heading)
    || names.some((name) => candidate.searchableTitle.includes(name)));
  if (!section) {
    return undefined;
  }

  const candidates = document.blocks
    .filter((block) => block.start > section.start && block.start < section.end)
    .filter((block) => !isLikelyUsageExpression(block.text))
    .map((block) => ({
      block,
      score: getDefinitionBlockScore(block, definition),
    }))
    .filter((candidate) => candidate.score > 0)
    .sort((left, right) =>
      getSpecificationBlockKindScore(right.block) - getSpecificationBlockKindScore(left.block)
      || right.score - left.score
      || left.block.start - right.block.start);
  return candidates[0]?.block.text;
}

function findDefinitionProse(document: SpecificationSourceDocument, definition: XrefDefinition): string | undefined {
  const candidates = document.blocks
    .filter((block) => !isLikelyUsageExpression(block.text))
    .map((block) => ({
      block,
      score: getDefinitionBlockScore(block, definition),
    }))
    .filter((candidate) => candidate.score >= getDefinitionScoreThreshold(definition))
    .sort((left, right) => right.score - left.score || left.block.start - right.block.start);
  return candidates[0]?.block.text;
}

function getSpecificationBlockKindScore(block: SpecificationSourceBlock): number {
  return block.tagName === "p" ? 2 : block.tagName === "dd" ? 1 : 0;
}

function isLikelyUsageExpression(text: string): boolean {
  if (/[.!?;:]$/.test(text)) {
    return false;
  }

  return /^(?:[A-Za-z_$][\w$]*\s*=\s*)?(?:new\s+)?[A-Za-z_$][\w$]*(?:\s*(?:\.|\[|\()|\s+[A-Za-z_$][\w$]*\s*=)/.test(text)
    && (text.includes("=") || text.includes(".") || text.includes("(") || text.includes("["));
}

function getDefinitionBlockScore(block: SpecificationSourceBlock, definition: XrefDefinition): number {
  let nameScore = 0;
  for (const linkingText of definition.linkingText) {
    const normalizedName = normalizeSourceSearchText(linkingText);
    if (normalizedName.length >= 3 && block.searchableText.includes(normalizedName)) {
      nameScore = Math.max(nameScore, 60);
    }

    const normalizedSimpleName = normalizeSourceSearchText(linkingText.replace(/\(.+$/, ""));
    if (normalizedSimpleName.length >= 3 && block.searchableText.includes(normalizedSimpleName)) {
      nameScore = Math.max(nameScore, 30);
    }
  }

  if (nameScore === 0) {
    return 0;
  }

  let ownerScore = 0;
  for (const owner of definition.for) {
    const normalizedOwner = normalizeSourceSearchText(owner);
    if (normalizedOwner && block.searchableSource.includes(normalizedOwner)) {
      ownerScore = Math.max(ownerScore, 120);
    }

    const normalizedOwnerType = normalizeSourceSearchText(owner.split("/")[0]);
    if (normalizedOwnerType.length >= 3 && block.searchableSource.includes(normalizedOwnerType)) {
      ownerScore = Math.max(ownerScore, 35);
    }
  }

  const typeScore = block.searchableSource.includes(`datadfntype${normalizeSourceSearchText(definition.type)}`)
    ? 40
    : block.searchableText.includes(normalizeSourceSearchText(definition.type))
      ? 10
      : 0;
  return nameScore + ownerScore + typeScore;
}

function getDefinitionScoreThreshold(definition: XrefDefinition): number {
  return definition.for.length > 0 ? 90 : 60;
}

function getDeveloperUsage(definition: XrefDefinition): string | undefined {
  const usage = definition.links?.find((link) => link.type === "dev")?.name;
  if (!usage) {
    return undefined;
  }

  return usage
    .replace(/\s*\.\s*/g, ".")
    .replace(/\s+/g, " ")
    .trim();
}

function normalizeReferenceText(value: string): string {
  return value
    .trim()
    .replace(/^(?:"|')|(?:"|')$/g, "")
    .replace(/\s+/g, "")
    .toLocaleLowerCase();
}

function hasMatchingOwner(definition: XrefDefinition, candidate: DocumentationCandidate): boolean {
  if (!candidate.owners || candidate.owners.length === 0) {
    return definition.for.length === 0;
  }

  return definition.for.some((reference) =>
    candidate.owners!.some((owner) => {
      if (reference === owner) {
        return true;
      }

      return !candidate.requiresQualifiedOwner && reference.startsWith(`${owner}/`);
    }),
  );
}

function getNameMatchScore(definition: XrefDefinition, candidate: DocumentationCandidate): number {
  const requestedNames = candidate.names.map(normalizeReferenceText);
  let score = 0;

  for (const linkingText of definition.linkingText) {
    const normalizedLinkingText = normalizeReferenceText(linkingText);
    for (const requestedName of requestedNames) {
      if (normalizedLinkingText === requestedName) {
        score = Math.max(score, 100);
        continue;
      }

      const linkingName = normalizedLinkingText.replace(/\(.+$/, "");
      const requestedNameWithoutArguments = requestedName.replace(/\(.+$/, "");
      if (linkingName === requestedNameWithoutArguments) {
        score = Math.max(score, 50);
      }
    }
  }

  return score;
}

function getHtmlProseText(htmlProse: string | undefined): string | undefined {
  if (!htmlProse) {
    return undefined;
  }

  const fragments: string[] = [];
  const blockElements = new Set(["p", "div", "li", "dt", "dd", "section", "blockquote", "pre"]);
  const visit = (node: unknown): void => {
    if (!node || typeof node !== "object") {
      return;
    }

    const current = node as { nodeName?: string; value?: string; childNodes?: unknown[] };
    if (current.nodeName === "#text" && current.value) {
      fragments.push(current.value);
      return;
    }

    if (current.nodeName === "br") {
      fragments.push(" ");
      return;
    }

    if (blockElements.has(current.nodeName ?? "")) {
      fragments.push(" ");
    }

    for (const child of current.childNodes ?? []) {
      visit(child);
    }

    if (blockElements.has(current.nodeName ?? "")) {
      fragments.push(" ");
    }
  };

  visit(parseFragment(htmlProse));
  const text = fragments.join("").replace(/\s+/g, " ").trim();
  return text || undefined;
}

/**
 * A definition anchor can appear inside an algorithm paragraph rather than an
 * explanatory sentence. Such text is authoritative but makes poor IntelliSense
 * documentation when it is copied to an unrelated member. Keep the exact xref
 * link in that case and emit prose only when the source itself identifies the
 * requested API surface.
 *
 * 定义锚点有时位于算法段落而不是说明句中。该文本虽来自规范，却不适合作为
 * IntelliSense 文档复制到无关成员；此时仍保留精确 xref 链接，只在正文自身能
 * 指向请求的 API 表面时才输出 prose。
 */
export function isUsefulSpecificationProse(
  prose: string,
  candidate: DocumentationCandidate,
): boolean {
  if (prose.length < 12 || /[`*]|\|\'?s\b|\[=|\[\[|\{\{/.test(prose)) {
    return false;
  }

  // Algorithm scaffolding such as "the ... steps are" is frequently adjacent
  // to a dfn, but is not an API description and is often shared by many members.
  if (
    /\b(?:steps?|algorithm)\s+(?:are|is|to|will|that|for|when)\b/i.test(prose)
    || /\binternal\s+(?:observer|slot|queue)\b/i.test(prose)
    || /:\s*$/.test(prose)
  ) {
    return false;
  }

  const hasReference = (value: string): boolean => {
    const name = value.replace(/\(.+$/, "").trim();
    if (name.length < 3) {
      return false;
    }

    return new RegExp(`\\b${name.replace(/[.*+?^${}()|[\]\\\\]/g, "\\$&")}\\b`, "i").test(prose);
  };

  const nameReferences = candidate.names.filter(hasReference);
  if (nameReferences.length === 0) {
    return false;
  }

  if (!candidate.requiresQualifiedOwner) {
    return true;
  }

  // Argument names such as "value" and "options" are common throughout a
  // specification. Require their owning operation too, otherwise an adjacent
  // paragraph from a different method is indistinguishable from real help text.
  const operationReferences = (candidate.owners ?? [])
    .map((owner) => owner.split("/").at(-1)?.replace(/\(.+$/, "") ?? "")
    .filter((operation) => operation.length >= 3)
    .some(hasReference);
  return operationReferences;
}

async function toDocumentation(
  definition: XrefDefinition,
  specificationTitle: string,
  specification: XrefSpec | undefined,
  sourceCatalog: SpecificationSourceCatalog,
  candidate: DocumentationCandidate,
): Promise<WebIdlDocumentation> {
  const heading = definition.heading.number
    ? `${definition.heading.number} ${definition.heading.title}`
    : definition.heading.title;
  const htmlProse = normalizeSpecificationProse(getHtmlProseText(definition.htmlProse) ?? "");
  const sourceProse = htmlProse && isUsefulSpecificationProse(htmlProse, candidate)
    ? undefined
    : await sourceCatalog.getProse(specification, definition);
  const prose = [htmlProse, sourceProse]
    .filter((value): value is string => !!value && isUsefulSpecificationProse(value, candidate))[0];
  const usage = getDeveloperUsage(definition);
  return {
    href: definition.href,
    specificationTitle,
    heading,
    headingHref: definition.heading.href,
    ...(prose ? { prose } : {}),
    ...(usage ? { usage } : {}),
  };
}

async function resolveDocumentation(
  definitionsFile: XrefDefinitionsFile | undefined,
  source: WebIdlSpecificationSource | undefined,
  specification: XrefSpec | undefined,
  sourceCatalog: SpecificationSourceCatalog,
  candidate: DocumentationCandidate,
): Promise<WebIdlDocumentation | undefined> {
  if (!definitionsFile?.dfns || !source) {
    return undefined;
  }

  const typeMatches = definitionsFile.dfns.filter((definition) => candidate.types.includes(definition.type));
  const ownerMatches = typeMatches.filter((definition) => hasMatchingOwner(definition, candidate));
  const candidates = ownerMatches.length > 0 ? ownerMatches : candidate.owners ? [] : typeMatches;
  const match = candidates
    .map((definition) => ({ definition, score: getNameMatchScore(definition, candidate) }))
    .filter((item) => item.score > 0)
    .sort((left, right) => right.score - left.score || left.definition.href.localeCompare(right.definition.href))[0];
  if (!match) {
    return undefined;
  }

  return await toDocumentation(
    match.definition,
    definitionsFile.spec?.title ?? source.title,
    specification,
    sourceCatalog,
    candidate,
  );
}

function getStringValue(value: unknown): string | undefined {
  return typeof value === "string" ? value : undefined;
}

function getArgumentNames(member: Record<string, unknown>): string[] {
  if (!Array.isArray(member.arguments)) {
    return [];
  }

  return member.arguments.map((argument) => getStringValue((argument as Record<string, unknown>).name) ?? "");
}

function getOperationNames(name: string, argumentNames: string[]): string[] {
  return [name, `${name}()`, `${name}(${argumentNames.join(", ")})`];
}

function getMemberCandidate(ownerName: string, member: Record<string, unknown>): DocumentationCandidate | undefined {
  const type = getStringValue(member.type);
  const memberName = getStringValue(member.name);
  const argumentNames = getArgumentNames(member);
  switch (type) {
    case "constructor":
      return {
        types: ["constructor"],
        names: [
          ...getOperationNames(ownerName, argumentNames),
          ...getOperationNames("constructor", argumentNames),
        ],
        owners: [ownerName],
      };
    case "attribute":
      return memberName
        ? { types: ["attribute"], names: [memberName], owners: [ownerName] }
        : undefined;
    case "const":
      return memberName
        ? { types: ["const"], names: [memberName], owners: [ownerName] }
        : undefined;
    case "operation":
      return memberName
        ? { types: ["method"], names: getOperationNames(memberName, argumentNames), owners: [ownerName] }
        : undefined;
    case "field":
      return memberName
        ? { types: ["dict-member"], names: [memberName], owners: [ownerName] }
        : undefined;
    case "enum-value": {
      const value = getStringValue(member.value);
      return value
        ? { types: ["enum-value"], names: [value, JSON.stringify(value)], owners: [ownerName] }
        : undefined;
    }
    default:
      return undefined;
  }
}

function getArgumentCandidate(
  ownerName: string,
  member: Record<string, unknown>,
  argumentName: string,
): DocumentationCandidate | undefined {
  const type = getStringValue(member.type);
  const memberName = type === "constructor" ? ownerName : getStringValue(member.name);
  if (!memberName) {
    return undefined;
  }

  const argumentNames = getArgumentNames(member);
  const operationNames = type === "constructor"
    ? [...getOperationNames(ownerName, argumentNames), ...getOperationNames("constructor", argumentNames)]
    : getOperationNames(memberName, argumentNames);
  return {
    types: ["argument"],
    names: [argumentName],
    owners: operationNames.map((operationName) => `${ownerName}/${operationName}`),
    requiresQualifiedOwner: true,
  };
}

function getDeclarationCandidate(root: webidl2.IDLRootType): DocumentationCandidate | undefined {
  if (!("name" in root) || typeof root.name !== "string") {
    return undefined;
  }

  const types = root.type === "callback interface"
    ? ["callback interface", "interface"]
    : [root.type];
  return {
    types,
    names: [root.name],
  };
}

async function collectMemberDocumentation(
  root: webidl2.IDLRootType,
  declarationName: string | undefined,
  definitionsFile: XrefDefinitionsFile | undefined,
  source: WebIdlSpecificationSource | undefined,
  specification: XrefSpec | undefined,
  sourceCatalog: SpecificationSourceCatalog,
): Promise<WebIdlMemberDocumentation[] | undefined> {
  if (!declarationName) {
    return undefined;
  }

  const values: unknown[] = "values" in root && Array.isArray(root.values)
    ? root.values
    : "members" in root && Array.isArray(root.members)
      ? root.members
      : [];
  const documentation = (await Promise.all(values.map(async (value, memberIndex) => {
    const member = value as unknown as Record<string, unknown>;
    const memberDocumentation = await resolveDocumentation(
      definitionsFile,
      source,
      specification,
      sourceCatalog,
      getMemberCandidate(declarationName, member) ?? { types: [], names: [] },
    );
    const argumentDocumentation: WebIdlArgumentDocumentation[] = Array.isArray(member.arguments)
      ? (await Promise.all(member.arguments.map(async (argument, argumentIndex) => {
          const argumentName = getStringValue((argument as Record<string, unknown>).name);
          const candidate = argumentName
            ? getArgumentCandidate(declarationName, member, argumentName)
            : undefined;
          const resolved = candidate
            ? await resolveDocumentation(
              definitionsFile,
              source,
              specification,
              sourceCatalog,
              candidate,
            )
            : undefined;
          return resolved ? [{ argumentIndex, documentation: resolved }] : [];
        }))).flat()
      : [];
    return memberDocumentation || argumentDocumentation.length > 0
      ? [{
          memberIndex,
          ...(memberDocumentation ? { documentation: memberDocumentation } : {}),
          ...(argumentDocumentation.length > 0 ? { arguments: argumentDocumentation } : {}),
        }]
      : [];
  }))).flat();

  return documentation.length > 0 ? documentation : undefined;
}

async function normalizeDeclaration(
  root: webidl2.IDLRootType,
  definitionsFile: XrefDefinitionsFile | undefined,
  source: WebIdlSpecificationSource | undefined,
  specification: XrefSpec | undefined,
  sourceCatalog: SpecificationSourceCatalog,
): Promise<WebIdlDeclaration> {
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

  const declarationName = "name" in root && typeof root.name === "string" ? root.name : undefined;
  const declarationCandidate = getDeclarationCandidate(root);
  const documentation = declarationCandidate
    ? await resolveDocumentation(
      definitionsFile,
      source,
      specification,
      sourceCatalog,
      declarationCandidate,
    )
    : undefined;
  const memberDocumentation = await collectMemberDocumentation(
    root,
    declarationName,
    definitionsFile,
    source,
    specification,
    sourceCatalog,
  );
  if (documentation) {
    declaration.documentation = documentation;
  }

  if (memberDocumentation) {
    declaration.memberDocumentation = memberDocumentation;
  }

  return declaration;
}

async function collectFiles(): Promise<CollectedIdlFile[]> {
  const [idlFiles, css] = await Promise.all([listAllIdl(), listAllCss()]) as [
    Record<string, WebrefIdlFile>,
    WebrefCssFeatures,
  ];
  const files: CollectedIdlFile[] = [];

  for (const [fileName, file] of Object.entries(idlFiles)) {
    files.push({
      fileName,
      source: await file.text(),
    });
  }

  // @webref/css 8 exposes one consolidated feature set. CSS property metadata is not tied to
  // individual IDL source files, so synthesize one stable partial CSSStyleDeclaration input.
  // @webref/css 8 以单个聚合 feature set 暴露数据。CSS 属性元数据不再绑定到单个 IDL 源文件，
  // 因此这里合成一个稳定的 partial CSSStyleDeclaration 输入。
  if (!Array.isArray(css.properties)) {
    throw new Error("@webref/css did not provide a properties feature array.");
  }

  const properties = css.properties.map((property) => property.name);
  if (properties.length > 0) {
    files.push({
      fileName: "jazor.webref-css-properties.idl",
      source: `${generateCssIdl(properties)}\n`,
    });
  }

  return files;
}

async function collectInterfaceEvents(): Promise<InterfaceEventMap[]> {
  const allEvents = await listAllEvents() as Record<string, WebrefEvent>;
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
  const [files, interfaceEvents, xrefCatalog] = await Promise.all([
    collectFiles(),
    collectInterfaceEvents(),
    XrefCatalog.create(),
  ]);

  const sourceCatalog = new SpecificationSourceCatalog();
  const normalizedFiles = await Promise.all(files
    .sort((left, right) => left.fileName.localeCompare(right.fileName))
    .map(async (file): Promise<WebIdlFile> => {
      const resolvedDocumentation = await xrefCatalog.resolve(
        readSpecificationSource(file.source),
        file.fileName,
      );
      const ast = (webidl2.parse(file.source) as webidl2.IDLRootType[]).sort(
        (left, right) =>
          (declarationPriority[left.type] ?? 999) -
          (declarationPriority[right.type] ?? 999),
      );
      const namespace = ast.find((item) => item.type === "namespace")?.name;
      const declarations = await Promise.all(ast.map((root) =>
        normalizeDeclaration(
          root,
          resolvedDocumentation.definitions,
          resolvedDocumentation.source,
          resolvedDocumentation.specification,
          sourceCatalog,
        )));

      return {
        fileName: file.fileName,
        namespace,
        ...(resolvedDocumentation.source ? { source: resolvedDocumentation.source } : {}),
        declarations,
      };
    }));
  const declarationsByKind = new Map<string, number>();
  let declarationCount = 0;
  for (const file of normalizedFiles) {
    for (const declaration of file.declarations) {
      declarationsByKind.set(declaration.kind, (declarationsByKind.get(declaration.kind) ?? 0) + 1);
      declarationCount++;
    }
  }

  return {
    schemaVersion: 3,
    generatedAt: new Date().toISOString(),
    source: {
      parser: parserVersion,
      webrefIdl: webrefIdlVersion,
      webrefCss: webrefCssVersion,
      webrefEvents: webrefEventsVersion,
      webrefXref: webrefXrefVersion,
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

async function run(): Promise<void> {
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
}

if (import.meta.main) {
  await run();
}
