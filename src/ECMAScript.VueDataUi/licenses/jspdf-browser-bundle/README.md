# jsPDF browser bundle notices

`dist/jspdf.browser.mjs` is the browser-closed bundle used by the `jspdf`
import-map entry. The files in this directory retain the license text from
every npm package included by its esbuild metafile, except jsPDF itself, whose
license is `../JSPDF-LICENSE`.

The package/version inventory is deliberately recorded here so a bundle refresh
can update its notices together with its dependency graph:

- `@babel/runtime` 7.29.7
- `canvg` 3.0.11
- `core-js` 3.50.0
- `dompurify` 3.3.1
- `fast-png` 6.4.0
- `fflate` 0.8.3
- `html2canvas` 1.0.0-rc.5
- `iobuffer` 5.4.0
- `pako` 2.2.0
- `performance-now` 2.1.0
- `raf` 3.4.1
- `rgbcolor` 1.0.1
- `stackblur-canvas` 2.7.0
- `svg-pathdata` 6.0.3

`dist/jspdf.browser.mjs.LEGAL.txt` separately preserves legal notices embedded
in the bundled source, including jsPDF's own third-party attributions.
