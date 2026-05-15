import { basename, join, relative } from "node:path";
import { emptyDirectory, fileExists, prepareWorkspace, readText, writeText } from "./lib/pipeline.ts";

export async function runBuild(): Promise<void> {
  const workspace = await prepareWorkspace();
  await emptyDirectory(workspace.distRoot);
  await Deno.mkdir(workspace.assetsDirectory, { recursive: true });

  const entryFilePath = join(workspace.assetsDirectory, "client-entry.js");
  const child = new Deno.Command(Deno.execPath(), {
    cwd: workspace.consumerRoot,
    args: [
      "bundle",
      "--frozen",
      "--import-map",
      workspace.importMapPath,
      "--platform",
      "browser",
      "--format",
      "esm",
      "--packages=bundle",
      "--sourcemap=linked",
      "-o",
      entryFilePath,
      workspace.clientEntryPath
    ],
    env: {
      JAZOR_GENERATED_ROOT: workspace.generatedRoot
    },
    stdin: "null",
    stdout: "inherit",
    stderr: "inherit"
  }).spawn();

  const output = await child.status;
  if (!output.success) {
    throw new Error(`deno bundle failed with exit code ${output.code}.`);
  }

  if (!(await fileExists(entryFilePath))) {
    throw new Error(`deno bundle did not produce '${entryFilePath}'.`);
  }

  const templatePath = join(workspace.consumerRoot, "index.html");
  const template = await readText(templatePath);
  const outputHtml = template
    .replace(
      "</head>",
      ['  <link rel="stylesheet" href="./assets/style.css" />', "</head>"].join("\n")
    )
    .replace(
      '  <script type="module" src="/src/main.js"></script>',
      `  <script type="module" src="./assets/${basename(entryFilePath)}"></script>`
    );

  await Deno.copyFile(join(workspace.consumerRoot, "src", "style.css"), join(workspace.assetsDirectory, "style.css"));
  await writeText(join(workspace.distRoot, "index.html"), outputHtml);

  const relativeEntryFilePath = `./${relative(workspace.distRoot, entryFilePath).replaceAll("\\", "/")}`;
  console.log(`VueRoute Deno build emitted ${relativeEntryFilePath}.`);
}

if (import.meta.main) {
  await runBuild();
}
