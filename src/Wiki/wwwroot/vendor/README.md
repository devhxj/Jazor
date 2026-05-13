# Vendor Directory

This directory contains localized browser dependencies to eliminate runtime CDN dependencies.

## Contents

| File | Source | SHA-256 |
|------|--------|---------|
| `vue@3.5.16.mjs` | [vue@3.5.16 ESM browser prod](https://unpkg.com/vue@3.5.16/dist/vue.esm-browser.prod.js) | `3475eec0059f49fb0444c022253ca1de8f414046175f7a386ef404c96c0d756c` |

## Upgrade Procedure

1. Download the new Vue ESM browser production build from unpkg:
   ```
   curl -o vue@<version>.mjs https://unpkg.com/vue@<version>/dist/vue.esm-browser.prod.js
   ```
2. Compute and record the SHA-256 hash:
   ```
   sha256sum vue@<version>.mjs
   ```
3. Update `host/index.template.html` import map: change `vue` and `npm:vue@3` to `/vendor/vue@<version>.mjs`
4. Update `wiki-verify-smoke.cs` vendor markers to match the new filename
5. Update this README with the new version and hash
6. Run `dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build` to verify
7. Remove the old version file
