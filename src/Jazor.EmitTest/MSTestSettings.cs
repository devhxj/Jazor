// Emit integration tests launch build tools, Deno workers, and browser processes.
// Running methods from one fixture concurrently lets independent tests rebuild the
// same repository outputs and can leave testhost waiting on a locked assembly.
// Keep classes parallel for throughput, but serialize methods within each fixture
// so a package/build fixture owns its process and temporary-output lifetime.
[assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]
