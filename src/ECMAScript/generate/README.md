# Historical Generate Directory

`src/ECMAScript/generate/` is a historical repository artifact.

- It is excluded by [`src/ECMAScript/ECMAScript.csproj`](../ECMAScript.csproj).
- It does not participate in the current `ECMAScript` compile pipeline.
- The active .NET WebIDL generator output lives under `src/ECMAScript/webidl/generate/`.

Do not treat files in this directory as the current source of truth for:

- supported public host surface
- current union strategy
- active generated binding shape

If a binding or union behavior needs to change, update the active generator and the active `webidl/generate` output instead of patching this directory.
