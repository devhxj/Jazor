# Editable list browser baseline

The two standalone pages mount the editable list with the same Vue 3.5.42, Reka UI 2.11.0 and Vite 8.2.0 lockfile and Vite build settings. Both use browser and component default styles. The native `.vue` page uses Vue's `compileScript` with its inline template compiler in the repository's embedded NodeApi/Vite host. [Vue fixture](.fixtures/EditableList.vue) · [Jolt fixture](.fixtures/EditableList.jazor)

Build both production pages from the repository root. Use the candidate Jolt CLI for the second build.

```sh
dotnet run --file samples/Jolt.Counter/build-sfc-baseline.cs -- .tmp/jolt-native-sfc
dotnet run --file samples/Jolt.Counter/build-sfc-baseline.cs -- --prepare-jolt .tmp/jolt-native-pair
jolt bundle .tmp/jolt-native-pair
```

Serve each production directory in a separate terminal.

```sh
dotnet run --no-build --file samples/Jolt.Counter/build-sfc-baseline.cs -- --serve .tmp/jolt-native-sfc/dist 15662
dotnet run --no-build --file samples/Jolt.Counter/build-sfc-baseline.cs -- --serve .tmp/jolt-native-pair/dist 15663
```

Run the common smoke against each origin once as a warm-up, then alternate origins and record only runs that pass all assertions. Replace `chromium` with the browser executable.

```sh
dotnet run --file samples/Jolt.Counter/verify-browser.cs -- chromium http://127.0.0.1:15662/ --editable-only
dotnet run --no-build --file samples/Jolt.Counter/verify-browser.cs -- chromium http://127.0.0.1:15663/ --editable-only
```

The timed sequence starts after the initial controls are ready. It covers text commit on blur, immediate text input and IME composition, checkbox and Reka model updates, parent-driven updates, then two keyed reorders with DOM identity, focus, bubbling and default action checks. The reported time includes CDP calls and assertion polling. Each invocation uses a fresh browser profile. The two-row fixture measures the interaction as a whole. Percentiles use the nearest-rank method.

## November 20, 2026

Headless Chrome 157.0.8103.79 on Windows 11 with an Intel Core i5-12500H. After one warm-up on each page, all six alternating production runs per page passed the shared browser assertions.

| Production page | Samples | p50 (ms) | p95 (ms) | Minified JS (kB) | Gzip JS (kB) |
| --- | ---: | ---: | ---: | ---: | ---: |
| Vue SFC | 6 | 43.617 | 66.968 | 77.94 | 30.23 |
| Jolt | 6 | 41.396 | 63.031 | 79.71 | 30.07 |
| Jolt / SFC | | 0.95× | 0.94× | 1.02× | 0.99× |

Native Vue timings in run order were 29.941, 47.684, 43.617, 48.303, 66.968 and 37.217 ms. Jolt timings were 41.396, 63.031, 39.490, 47.012, 45.113 and 39.364 ms. An earlier six-run SFC-only batch had p50 32.346 ms and p95 52.402 ms. The SFC inter-batch change exceeds the observed differences in the paired batch.
