# Release Notes

## 2026-07-27

- RazorVue render-context now covers the core generated component semantics for render surface, component props, DOM and component events, slots, bind, lifecycle, references, metadata, and browser DOM behavior.
- Production bundling now supports explicit Deno and Netpack lanes over the same manifest contract.
- Import-backed `.vue` SFC assets now flow from explicit component references into the manifest and production bundles without source-root scanning or a separate frontend asset API.
- External package consumers can build RazorVue output and Netpack bundles from the local `Jazor` NuGet package path without relying on repository-local tool binaries.
