import { join, relative } from "node:path";
import { copyDirectoryContents, emptyDirectory, fileExists, prepareWorkspace, readText, writeText } from "./lib/pipeline.ts";

export async function runBuild(): Promise<void> {
  const workspace = await prepareWorkspace();
  await emptyDirectory(workspace.distRoot);

  const entryFilePath = join(workspace.bundleRoot, "bundle.js");
  if (!(await fileExists(entryFilePath))) {
    throw new Error(`Missing Netpack browser bundle: ${entryFilePath}`);
  }
  await copyDirectoryContents(workspace.bundleRoot, workspace.assetsDirectory);

  const templatePath = join(workspace.consumerRoot, "index.html");
  const template = await readText(templatePath);
  const outputHtml = template
    .replace(
      "</head>",
      ['  <link rel="stylesheet" href="./assets/style.css" />', "</head>"].join("\n")
    )
    .replace(
      '  <script type="module" src="/src/main.js"></script>',
      '  <script type="module" src="./assets/bundle.js"></script>'
    );

  await Deno.copyFile(join(workspace.consumerRoot, "src", "style.css"), join(workspace.assetsDirectory, "style.css"));
  await writeText(join(workspace.distRoot, "index.html"), outputHtml);

  const relativeEntryFilePath = `./${relative(workspace.distRoot, join(workspace.assetsDirectory, "bundle.js")).replaceAll("\\", "/")}`;
  console.log(`Pinia Netpack bundle materialized at ${relativeEntryFilePath}.`);
}

if (import.meta.main) {
  await runBuild();
}
