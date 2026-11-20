# Jolt npm metadata

`NpmVueMetadataProvider` finds the nearest consumer `package.json` from a `.jazor` document path, restores its dependencies with the repository's pnpm API, and uses a tool-owned TypeScript 5.9.3 installation. The consumer declares Vue and the component package in its manifest.

`ListSpecifiersAsync` discovers the package root and explicit public subpaths. Export patterns have an open set of names. Callers can send a concrete matching subpath to `ListExportsAsync` or `ReadAsync`. Export names use the npm name verbatim, including `default`. `NpmNames` translates those names to the Jolt namespace and tag convention.

`ReadAsync` returns props, emitted event signatures, slot signatures, and source definitions from public Vue declaration types. Definition offsets use UTF-16. Generic callable component metadata uses TypeScript's structural inference, so an unresolved type parameter can appear as `unknown`. The provider resolves the runtime entry through the ESM loader and reads export information through declaration files. Component runtime code is evaluated by the render consumer.

`ReadValueAsync` resolves a named or default function or a scalar constant from a public ESM entry and its TypeScript declarations. Metadata preserves the authored module specifier, realized package version, overloads, optional parameters, and definition spans. The typed portion covers scalar values, structured properties, callbacks with one signature, and Vue refs with matching read and write types or a read-only value. For a scalar-or-ref parameter, the scalar branch is available to C# callers. Unsupported shapes such as generic and rest signatures produce an export-specific diagnostic. Lookup reads declarations without evaluating package JavaScript.

`ListValueExportsAsync` lists candidate names. `ReadValuesAsync` projects requested candidates in one checker invocation and returns their supported signatures for completion. Successful responses are cached by consumer project, module specifier, and query. The provider fingerprints the project manifest, lockfile, npm configuration, and files used by the checker and runtime entry resolution.

For a local SFC, Vue Language Tools and TypeScript derive the public props, models, events and slots. The checker reads unsaved editor buffers and can resolve a `.jazor` import against the project's current generated TypeScript declaration. A later edit to that declaration invalidates dependent Vue contracts. Disk fingerprints record the bytes actually read by the checker so a second save during a query invalidates its result on the next lookup.

For saved Vue 3 HTML templates with the standard compiler plugins, the provider can reuse a contract after a literal text edit. It compares Vue parser text spans after the last mapped public definition and checks the other source and dependency inputs. A changed binding, slot expression, script or definition offset triggers a full query. Vite applies the template edit to the running component. [Mixed edit benchmark](../../samples/Jolt.Mixed/EditFeedback-baseline.md)

`dotnet run --file src/Jolt.Npm/verify.cs` covers published Reka UI components, the `es-toolkit/string` subpath, VueUse `useCounter`, default and named exports, overloads and callbacks, and cache invalidation after a dependency upgrade.
