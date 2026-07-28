# RazorVue G2 Performance Baseline

> Date: 2026-07-27
> Command: `dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- --write-release-report --out .tmp/razorvue-g2-direct-final`
> Commit: `9ead929d26aacca1a2c517c5859fcb6470c26520`

## Summary

The first G2 baseline is recorded for the active RazorVue render-function path.

| Threshold | Status | Detail |
|---|---|---|
| `generated-gzip` | warn | max generated/handwritten fixture gzip ratio = 3.672 (limit 2.000) |
| `node-throughput` | warn | min Node render/update ratio = 0.018 (limit 0.700) |
| `browser-throughput` | observed | min browser render/update ratio = 0.023; browser lane is trend evidence, not a hard throughput threshold |
| `browser-heap` | observed | all browser fixtures reported JS heap deltas |
| `incremental-compile-old-line` | unavailable | `v0.1.26` does not expose the same G2 measurement protocol |
| `node-retained-heap` | observed | Node retained heap delta medians recorded for 3 fixtures |

## Runtime Protocol

| Fixture | Render ratio | Update ratio | Node heap delta median |
|---|---:|---:|---:|
| `plain-text` | 0.024 | 0.018 | 16 |
| `counter` | 0.049 | 0.046 | 2736 |
| `keyed-list-100` | 0.370 | 0.387 | 35376 |

## Generated Artifacts

- Package version: `0.1.27`
- Clean build: 4986 ms
- Incremental p95: 2195 ms

| Fixture | Component gzip | Handwritten gzip | Gzip ratio |
|---|---:|---:|---:|
| `plain-text` | 261 | 83 | 3.145 |
| `counter` | 291 | 168 | 1.732 |
| `keyed-list-100` | 672 | 183 | 3.672 |

## Browser

- Browser: `C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe`
- Samples: 5
- Render/update iterations per sample: 10000
- Mount/unmount cycles: 100

| Fixture | Render ratio | Update ratio | Heap delta median |
|---|---:|---:|---:|
| `plain-text` | 0.036 | 0.063 | 0 |
| `counter` | 0.023 | 0.032 | 176 |
| `keyed-list-100` | 0.326 | 0.340 | 17204 |

## Old-Line Baseline

- Ref: `v0.1.26`
- Commit: `0b42dca0a8df770ddbe2b6f3a1af223fe8ca6bdd`
- Status: `homogeneous-protocol-unavailable`

`v0.1.26` predates `scripts/csharp/benchmark-razorvue-g2.cs`, so numeric old-line threshold comparison is not same-protocol evidence. A fixed old-line worktree can be sampled later only after it exposes the same G2 measurement contract.

## Follow-Up

- First optimize generated module/runtime payload size; `plain-text` and `keyed-list-100` still exceed the current 2.0 gzip target.
- Investigate render-context protocol overhead on small fixtures separately from keyed-list throughput; the small handwritten baselines are near timer-resolution limits.
- Keep browser heap as a trend metric for now. It produced usable `performance.memory.usedJSHeapSize` deltas on this machine.
