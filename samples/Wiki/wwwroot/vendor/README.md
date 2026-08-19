# Vendor Directory

This directory contains localized browser dependencies to eliminate runtime CDN dependencies.

## Contents

| File | Source | SHA-256 |
|------|--------|---------|
| `vue@3.5.16.mjs` | [vue@3.5.16 ESM browser prod](https://unpkg.com/vue@3.5.16/dist/vue.esm-browser.prod.js) | `3475eec0059f49fb0444c022253ca1de8f414046175f7a386ef404c96c0d756c` |
| `sober@1.1.10.min.js` | [sober@1.1.10 全量 IIFE 构建](https://unpkg.com/sober@1.1.10/dist/sober.min.js)（Material 3 Web Components，零依赖，图标为内联 SVG） | `2ac11927f038b8f438b9c829dfa43c3f9ca9f7d40c02e3c10b2c2ec255a12e41` |

## Upgrade Procedure

1. Download the new artifact from unpkg:
   ```
   curl -o vue@<version>.mjs https://unpkg.com/vue@<version>/dist/vue.esm-browser.prod.js
   curl -o sober@<version>.min.js https://unpkg.com/sober@<version>/dist/sober.min.js
   ```
2. Compute and record the SHA-256 hash:
   ```
   sha256sum <file>
   ```
3. Update `host/index.template.html`: the import map for `vue`, and the `__WIKI_SOBER_URL__` script for sober
4. Update `WikiHostShell.cs` vendor URL constants and `wiki-verify-smoke.cs` vendor markers to match the new filename
5. Update this README with the new version and hash
6. Run `dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build` to verify
7. Remove the old version file
