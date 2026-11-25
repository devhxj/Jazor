# VForm 3.0.10 local patches

Jero vendors the UMD-only VForm 3.0.10 build. Its license is distributed at
assets/third-party/vform3-LICENSE.txt.

The form design route imports designer.mjs on mount. The lazy module restores
the temporary Vue global on success or failure, and the route ignores late
completion after unmounting.

The base vendored file at Git commit `94d263b` includes the earlier RegExp validation
patch. The Radio patch replays from that exact input; it preserves the previous patch,
upstream version, option labels, and option-value types.

## Radio values

Element Plus now expects an explicit `value` on `el-radio` and `el-radio-button`.
The patch changes the 16 Radio props in six render functions, the property-editor
helper, and the generated Radio SFC template. Vue's dynamic-prop list follows the
renamed prop. Form-item labels and the displayed option text keep their existing
behavior. No global component wrapper or warning filter is installed.

Run from `samples/Jero`. Check the vendored file with `dotnet run --file scripts/csharp/patch-vform-radio.cs -- --check`. To regenerate it, pass the path to the base bundle from commit `94d263b` of the original Jero repository in place of `--check`. The script verifies both input and output hashes.

- Input SHA256: `aa4cfb7a50db71885b8902ac3d35e2cbb33d892521923fb930268c3faa378da4`
- Output SHA256: `41638c71e37a8e649dd3a8a64b60f22fc9383d1219b9a06be50eb65a2b0d5ff0`

When upgrading the vendor build, replace this version-specific patch with maintained
upstream behavior or update the pinned fragments against the new source. Radio
selection, JSON roundtrip, preview values (including numeric/boolean values), property
editing, SFC export, and designer remount belong to the acceptance checks.
