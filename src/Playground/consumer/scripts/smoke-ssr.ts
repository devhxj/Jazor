import { join } from "node:path";
import { createImportUrl, emptyDirectory, fileExists, prepareWorkspace } from "./lib/pipeline.ts";

export async function runSsrSmoke(): Promise<void> {
  const workspace = await prepareWorkspace(true);
  const outputDirectory = join(workspace.buildRoot, "ssr-bundle");
  await emptyDirectory(outputDirectory);

  const output = await new Deno.Command(Deno.execPath(), {
    cwd: workspace.consumerRoot,
    args: [
      "bundle",
      "--platform",
      "deno",
      "--format",
      "esm",
      "--packages=bundle",
      "--sourcemap=linked",
      "--outdir",
      outputDirectory,
      workspace.ssrEntryPath
    ],
    stdin: "null",
    stdout: "piped",
    stderr: "piped"
  }).output();

  if (!output.success) {
    throw new Error(new TextDecoder().decode(output.stderr).trim() || "Playground SSR deno bundle failed.");
  }

  const bundledEntryPath = join(outputDirectory, "ssr-entry.js");
  if (!(await fileExists(bundledEntryPath))) {
    throw new Error(`Playground SSR bundle did not produce '${bundledEntryPath}'.`);
  }

  const entryModule = await import(createImportUrl(bundledEntryPath)) as {
    executeSsrSmoke: () => Promise<string>;
  };
  await entryModule.executeSsrSmoke();
  console.log("Playground Deno SSR smoke passed.");
}

if (import.meta.main) {
  await runSsrSmoke();
}
