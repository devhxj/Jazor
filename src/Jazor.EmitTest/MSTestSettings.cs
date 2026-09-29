// Emit integration tests own isolated workspaces and run child builds with /m:1.
// Method-level scheduling lets package inspection, consumer builds, Deno, and browser
// scenarios overlap without creating an unbounded process fan-out. SdkIntegrationTests
// applies the same four-process limit to child dotnet invocations.
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel, Workers = 4)]
