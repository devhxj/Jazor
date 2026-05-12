import { basename, join, relative } from "node:path";
import {
  collectFiles,
  copyDirectory,
  emptyDirectory,
  fileExists,
  prepareWorkspace,
  readText,
  writeText
} from "./lib/pipeline.ts";

export async function runBuild(): Promise<void> {
  const workspace = await prepareWorkspace(true);
  await emptyDirectory(workspace.distRoot);
  await Deno.mkdir(workspace.assetsDirectory, { recursive: true });

  const output = await new Deno.Command(Deno.execPath(), {
    cwd: workspace.consumerRoot,
    args: [
      "bundle",
      "--platform",
      "browser",
      "--format",
      "esm",
      "--packages=bundle",
      "--sourcemap=linked",
      "--outdir",
      workspace.assetsDirectory,
      workspace.clientEntryPath
    ],
    stdin: "null",
    stdout: "piped",
    stderr: "piped"
  }).output();

  if (!output.success) {
    throw new Error(new TextDecoder().decode(output.stderr).trim() || "Playground deno bundle failed.");
  }

  const entryFilePath = join(workspace.assetsDirectory, "client-entry.js");
  if (!(await fileExists(entryFilePath))) {
    throw new Error(`Playground bundle did not produce '${entryFilePath}'.`);
  }

  const cssFiles = (await collectFiles(workspace.assetsDirectory))
    .filter((file) => file.endsWith(".css"))
    .map((file) => `./${relative(workspace.distRoot, file).replaceAll("\\", "/")}`)
    .sort((left, right) => left.localeCompare(right, "en"));

  const template = await readText(join(workspace.consumerRoot, "index.html"));
  const cssMarkup = cssFiles.map((file) => `    <link rel="stylesheet" href="${file}" />`).join("\n");
  const outputHtml = template
    .replace("    <!-- razorvue:styles -->", cssMarkup.length === 0 ? "    <!-- razorvue:styles -->" : cssMarkup)
    .replace(
      "    <!-- razorvue:script -->",
      `    <script type="module" src="./${relative(workspace.distRoot, entryFilePath).replaceAll("\\", "/")}"></script>`
    );

  await writeText(join(workspace.distRoot, "index.html"), outputHtml);
  await copyDirectory(workspace.assetsDirectory, workspace.hostAssetsDirectory);
  console.log(`Playground Deno build emitted ${basename(entryFilePath)} and ${cssFiles.length} CSS asset(s).`);
}

if (import.meta.main) {
  await runBuild();
}
