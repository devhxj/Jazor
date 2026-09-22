import { join, relative } from "node:path";
import { copyDirectoryContents, emptyDirectory, fileExists, prepareWorkspace, readText, writeText } from "./lib/pipeline.ts";

export async function runBuild(): Promise<void> {
  const workspace = await prepareWorkspace();
  await emptyDirectory(workspace.distRoot);

  const entryFilePath = join(workspace.generatedRoot, "host", "app.mjs");
  if (!(await fileExists(entryFilePath))) {
    throw new Error(`Missing generated host entry: ${entryFilePath}`);
  }
  await copyDirectoryContents(workspace.generatedRoot, workspace.assetsDirectory);

  const templatePath = join(workspace.consumerRoot, "index.html");
  const template = await readText(templatePath);
  const outputHtml = template
    .replace(
      "</head>",
      ['  <link rel="stylesheet" href="./assets/style.css" />', "</head>"].join("\n")
    )
    .replace(
      '  <script type="module" src="/src/main.js"></script>',
      '  <script type="module" src="./assets/host/app.mjs"></script>'
    );

  await Deno.copyFile(join(workspace.consumerRoot, "src", "style.css"), join(workspace.assetsDirectory, "style.css"));
  await writeText(join(workspace.distRoot, "index.html"), outputHtml);

  const relativeEntryFilePath = `./${relative(workspace.distRoot, entryFilePath).replaceAll("\\", "/")}`;
  console.log(`Pinia standard JavaScript entry materialized at ${relativeEntryFilePath}.`);
}

if (import.meta.main) {
  await runBuild();
}
