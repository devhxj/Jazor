# Jolt npm Vue metadata

`NpmVueMetadataProvider` finds the nearest consumer `package.json` from a `.jazor` document path, restores its dependencies with the repository's pnpm API, and uses a tool-owned TypeScript 5.9.3 installation. The consumer declares Vue and the component package in its manifest.

`ListSpecifiersAsync` discovers the package root and explicit public subpaths. Export patterns have an open set of names. Callers can send a concrete matching subpath to `ListExportsAsync` or `ReadAsync`. Export names use the npm name verbatim, including `default`. `NpmNames` translates those names to the Jolt namespace and tag convention.

`ReadAsync` returns props, emitted event signatures, slot signatures, and source definitions from public Vue declaration types. Definition offsets use UTF-16. Generic callable component metadata uses TypeScript's structural inference, so an unresolved type parameter can appear as `unknown`. The provider resolves the runtime entry through the ESM loader and reads export information through declaration files. Component runtime code is evaluated by the render consumer.

Successful responses are cached by consumer project, module specifier, and export name. The provider fingerprints the project manifest, lockfile, npm configuration, and files used by the checker and runtime entry resolution.

`dotnet run --file src/Jolt.Npm/verify.cs` verifies the published Reka UI declarations, an in-place dependency upgrade, default and subpath exports, literal-union props, and declaration cache invalidation.
