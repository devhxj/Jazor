import { join } from "node:path";
import { runBuild } from "./build.ts";
import { prepareWorkspace, runDeno } from "./lib/pipeline.ts";

export async function runTests(): Promise<void> {
  const workspace = await prepareWorkspace();
  await runBuild();

  const testFiles = [
    join("src", "pinia.generated.test.js"),
    join("src", "pinia.runtime.test.js"),
    join("src", "pinia.generated.dom.test.js")
  ];

  for (const testFile of testFiles) {
    await runDeno(workspace, [
      "test",
      "-A",
      "--frozen",
      "--import-map",
      workspace.importMapPath,
      testFile
    ]);
  }

  console.log("ECMAScript.Pinia Netpack bundle and Deno runtime test pipeline passed.");
}

if (import.meta.main) {
  await runTests();
}
