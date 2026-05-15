import { basename, join } from "node:path";
import { collectFiles, emptyDirectory, fileExists, prepareWorkspace, readText } from "./lib/pipeline.ts";

function hasUnresolvedVueImportSpecifier(source: string): boolean {
  const importPattern =
    /\b(?:import|export)\s+(?:[^"'`]*?\s+from\s*)?["'](?<specifier>[^"'`]+)["']|\bimport\s*\(\s*["'](?<dynamic>[^"'`]+)["']\s*\)/g;

  for (const match of source.matchAll(importPattern)) {
    const groups = match.groups as { specifier?: string; dynamic?: string } | undefined;
    const specifier = groups?.specifier ?? groups?.dynamic;
    if (specifier !== undefined && specifier.endsWith(".vue")) {
      return true;
    }
  }

  return false;
}

export async function runBundleApiSmoke(): Promise<void> {
  const workspace = await prepareWorkspace(true);
  const outputDirectory = join(workspace.buildRoot, "bundle-api");
  await emptyDirectory(outputDirectory);

  const result = await Deno.bundle({
    entrypoints: [workspace.clientEntryPath],
    format: "esm",
    outputDir: outputDirectory,
    packages: "bundle",
    platform: "browser",
    sourcemap: "linked",
    write: true
  });

  if (!result.success) {
    throw new Error(
      [
        "Deno.bundle() smoke failed.",
        ...result.errors.map((error) => `- ${error.text}`)
      ].join("\n"));
  }

  const files = await collectFiles(outputDirectory);
  const jsFile = files.find((file) => file.endsWith(".js"));
  const cssFile = files.find((file) => file.endsWith(".css"));
  if (jsFile === undefined || cssFile === undefined) {
    throw new Error("Deno.bundle() smoke expected both JavaScript and CSS outputs.");
  }

  const jsSource = await readText(jsFile);
  if (hasUnresolvedVueImportSpecifier(jsSource)) {
    throw new Error("Deno.bundle() output still contains unresolved .vue specifiers.");
  }

  const jsMapExists = await fileExists(`${jsFile}.map`);
  const cssMapExists = await fileExists(`${cssFile}.map`);
  if (!jsMapExists || !cssMapExists) {
    throw new Error("Deno.bundle() smoke expected linked source maps for both JS and CSS outputs.");
  }

  console.log(`RazorVue Deno.bundle() smoke passed with ${basename(jsFile)} and ${basename(cssFile)}.`);
}

if (import.meta.main) {
  await runBundleApiSmoke();
}
