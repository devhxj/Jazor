# README Refresh Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refresh the repository root README so it becomes an accurate external landing page aligned with the current 2026-04-06 project status.

**Architecture:** Keep `README.md` as an English repo-level landing page, preserve its broad section flow, and tighten the content around three layers: stable reference areas, active workstreams, and future-facing areas. Replace stale capability matrices and static progress snapshots with concise summaries plus links into the living status and architecture docs.

**Tech Stack:** Markdown, GitHub README conventions, repo status docs under `docs/status/`, bash verification commands

---

## File Map

- **Modify:** `README.md`
  - Refresh the hero summary and project-status framing.
  - Rebuild the documentation map into reader-oriented groups.
  - Trim component and capability sections so they describe boundaries instead of frozen snapshots.
  - Fix the clone URL and build/test guidance.
- **Reference only:** `docs/workstream-dashboard.md`
  - Source of truth for repo-level workstream wording.
- **Reference only:** `docs/status/compiler-mainline-status.md`
  - Source of truth for compiler-mainline maturity wording.
- **Reference only:** `docs/README.md`
  - Top-level documentation hub linked from README.
- **Reference only:** `docs/architecture/README.md`
  - Architecture bridge linked from README.
- **Reference only:** `docs/workstream-dashboard.md`
  - Execution index linked from README.
- **Reference only:** `src/Jazor.Compiler/README.md`
  - Module-level compiler link target.

---

### Task 1: Refresh the README hero and status framing

**Files:**
- Modify: `README.md:5-58`
- Reference: `docs/workstream-dashboard.md`
- Reference: `docs/status/compiler-mainline-status.md`

- [ ] **Step 1: Replace the opening summary and status framing**

Replace the current project summary plus the `Key Features` / `Planned Features` block with the following markdown sections:

```md
# Jazor - C# to JavaScript Compiler with Module-Oriented Tooling

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

> ⚠️ **EXPERIMENTAL DEMO** ⚠️\
> Jazor is still evolving. Public APIs, generated output shapes, and adjacent toolchains may change as the repository continues to stabilize.

Jazor is an experimental Roslyn-based C# to JavaScript compiler project. It focuses on semantic-preserving lowering into JavaScript AST and currently treats the compiler mainline as the repository's most stable reference area, while RazorVue, emit/materialization, and source-map-related work continue as active execution lanes.

## What Jazor focuses on today

- Translating supported C# constructs into JavaScript through AST-based lowering instead of string templating.
- Preserving semantic intent across the compiler pipeline, analyzer checks, and runtime/module surfaces.
- Keeping the compiler mainline usable as a stable reference while adjacent workstreams keep evolving.
- Documenting active execution status explicitly so repository-level docs stay aligned with current work.

## Project status

### Stable reference areas

- **Compiler mainline**: the most mature part of the repository and the primary long-term reference surface.
- **Compiler architecture and deep-dive docs**: the best entry point when you need to understand the existing lowering pipeline.

### Active workstreams

- **RazorVue**: active implementation lane for Vue-oriented Razor lowering and authoring flow.
- **Emit / host materialization**: active dependency lane for shaping emitted assets and host-facing outputs.
- **SourceMap / bundle chaining**: active partial rollout, especially where it already intersects with current RazorVue execution work.

### Evolving / future-facing areas

- Broader authoring ergonomics beyond the currently closed safe subsets.
- Additional host integrations and packaging flows that are still being refined.
- Deeper capability expansion that should follow, not destabilize, the compiler mainline.
```

- [ ] **Step 2: Verify the stale headings are gone**

Run:

```bash
grep -nE "^## Key Features|^## Planned Features" README.md
```

Expected: no output

- [ ] **Step 3: Verify the new status headings exist**

Run:

```bash
grep -nE "^## What Jazor focuses on today|^## Project status|^### Stable reference areas|^### Active workstreams|^### Evolving / future-facing areas" README.md
```

Expected: five matches

---

### Task 2: Rebuild the documentation map for mixed readers

**Files:**
- Modify: `README.md:16-38`
- Reference: `docs/README.md`
- Reference: `docs/workstream-dashboard.md`
- Reference: `docs/workstream-dashboard.md`
- Reference: `docs/architecture/README.md`

- [ ] **Step 1: Replace the flat documentation list with grouped navigation**

Replace the existing `Documentation Map` section with this grouped structure:

```md
## Documentation map

### Start here

- [Repository documentation hub](docs/README.md)
- [Current workstream dashboard](docs/workstream-dashboard.md)
- [Current project stage assessment](docs/workstream-dashboard.md)

### Current status and execution

- [Compiler mainline status](docs/status/compiler-mainline-status.md)
- [Emit and host materialization status](docs/status/emit-host-materialization-status.md)
- [Project execution index](docs/workstream-dashboard.md)
- [Project program roadmap](docs/workstream-dashboard.md)

### Architecture

- [Repository architecture bridge](docs/architecture/README.md)
- [Compiler architecture bridge](docs/architecture/compiler/README.md)
- [Module-level bridge](docs/architecture/modules/README.md)

### Subsystem deep dives

- [Compiler deep-dive index](src/Jazor.Compiler/doc/README.md)
- [Jazor.Compiler module README](src/Jazor.Compiler/README.md)
- [Emit local docs](src/Jazor.Emit/doc/README.md)

### Planning and documentation governance

- [Documentation governance rules](docs/guides/documentation-governance.md)
- [Repository plans index](docs/workstream-dashboard.md)
```

- [ ] **Step 2: Replace the resume-order paragraph with a reader-friendly entry guide**

Replace the current `If you are resuming project work, read in this order:` block with:

```md
If you are new to the repository, read in this order:

1. `docs/README.md`
2. `docs/workstream-dashboard.md`
3. `docs/status/compiler-mainline-status.md`
4. `docs/workstream-dashboard.md`
5. `docs/architecture/README.md`

If you are resuming a specific workstream, start from the current status page for that lane and then drill into the linked subsystem documentation.
```

- [ ] **Step 3: Verify every primary linked file exists**

Run:

```bash
for path in \
  docs/README.md \
  docs/workstream-dashboard.md \
  docs/status/compiler-mainline-status.md \
  docs/workstream-dashboard.md \
  docs/status/emit-host-materialization-status.md \
  docs/workstream-dashboard.md \
  docs/workstream-dashboard.md \
  docs/workstream-dashboard.md \
  docs/architecture/README.md \
  docs/architecture/compiler/README.md \
  docs/architecture/modules/README.md \
  src/Jazor.Compiler/doc/README.md \
  src/Jazor.Compiler/README.md \
  src/Jazor.Emit/doc/README.md \
  docs/guides/documentation-governance.md; do
  test -f "$path" || { echo "missing: $path"; exit 1; }
done
```

Expected: no output

---

### Task 3: Trim component summaries and replace stale capability matrices

**Files:**
- Modify: `README.md:82-299`
- Reference: `src/Jazor.Compiler/README.md`

- [ ] **Step 1: Rewrite the `Core Components` section with concise boundary-focused summaries**

Use the following replacement content for the section body under `## Core Components`:

```md
### 1. Jazor.Compiler

The compiler core lowers Roslyn symbols and operations into JavaScript AST. It is currently the repository's most mature reference surface and the best starting point if you want to understand the project's long-lived architecture.

See [Jazor.Compiler README](src/Jazor.Compiler/README.md) for module-level details and [compiler deep-dive docs](src/Jazor.Compiler/doc/README.md) for the broader pipeline.

### 2. Jazor.Analyzer

The analyzer validates ECMAScript-tagged code against the project's supported surface and whitelist rules. Its role is to keep unsupported shapes visible at compile time instead of leaving them as silent runtime mismatches.

### 3. Jazor.CLR

Jazor.CLR provides runtime-oriented module surfaces for supported .NET types and bridges compiler output to JavaScript-facing behavior. The root README should describe its responsibility, not freeze module completion statistics that change over time.

### 4. ECMAScript.WebIDL

The WebIDL pipeline collects and materializes Web API metadata for future binding generation. It remains an important supporting lane rather than the primary repository entry point.

### 5. Jazor.Emit

Jazor.Emit shapes generated modules into host-facing outputs and bundle-oriented assets. It sits in an active dependency lane shared by multiple current workstreams.
```

- [ ] **Step 2: Replace the large `Supported C# Types and Type Mapping` and `Supported C# Syntax` sections**

Delete the current type-mapping tables and syntax support lists. Insert this shorter section in their place:

```md
## Current capability snapshot

Jazor currently emphasizes the compiler mainline and the repository infrastructure around it rather than claiming a frozen end-user feature surface.

The repository already contains substantial work in these areas:

- Roslyn-driven AST lowering through `AstConverter` and `SemanticWalker`
- Analyzer-backed validation for supported ECMAScript-tagged code
- Runtime/module surfaces for supported .NET types
- Ongoing work around RazorVue, emit/materialization, and source-map-adjacent output flows

The exact supported shape is still evolving. For detailed capability boundaries, prefer the subsystem documentation and current status pages over this top-level README.
```

- [ ] **Step 3: Remove the stale CLR snapshot wording**

Run:

```bash
grep -nE "39 modules total|Complete \(9/10\)|Partial \(7-8/10\)|Needs work" README.md
```

Expected: no output

- [ ] **Step 4: Verify the new capability snapshot exists once**

Run:

```bash
grep -n "^## Current capability snapshot" README.md
```

Expected: one match

---

### Task 4: Correct usage, build, and contact details

**Files:**
- Modify: `README.md:301-365`

- [ ] **Step 1: Fix the clone command and keep contributor commands focused**

In the `Build Steps` block, replace the clone command lines with:

```bash
# Clone repository
git clone https://github.com/devhxj/Jazor.git
cd Jazor
```

- [ ] **Step 2: Tighten the prerequisites wording**

Replace the existing prerequisites list with:

```md
### Prerequisites
- .NET 10 SDK
- PowerShell 7+ for the repository test helper scripts
- Windows, Linux, or macOS
```

- [ ] **Step 3: Keep the build/test section concise and aligned with the repo**

Ensure the `Build Steps` section ends up with exactly these commands in this order:

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run core .NET tests
pwsh ./scripts/test-dotnet.ps1

# Run compiler tests only
pwsh ./scripts/test-dotnet.ps1 -Project compiler

# Run emit/bundle tests only
pwsh ./scripts/test-dotnet.ps1 -Project emit

# Run specific test project directly
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj

# Run single test class
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"

# Run single test method
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"
```

- [ ] **Step 4: Verify the placeholder repository URL is gone and the real URL is present**

Run:

```bash
grep -n "your-repo/Jazor.git" README.md || true
grep -n "https://github.com/devhxj/Jazor.git" README.md
```

Expected:
- first command: no matches
- second command: one match

- [ ] **Step 5: Keep the contact section repo-aligned**

Ensure the contact block remains:

```md
## Contact

- Project homepage: https://github.com/devhxj/Jazor
- Issue tracker: https://github.com/devhxj/Jazor/issues
- Email: developerhan@msn.cn
```

---

### Task 5: Final README verification pass

**Files:**
- Modify: `README.md`
- Reference: `docs/workstream-dashboard.md`
- Reference: `docs/status/compiler-mainline-status.md`

- [ ] **Step 1: Read the final README top-to-bottom and check for internal contradictions**

Use this checklist while reviewing `README.md`:

```md
- The README describes the compiler mainline as the most mature area.
- RazorVue, emit/materialization, and source-map-related work are described as active lanes, not frozen features.
- No section claims a complete or fixed support matrix.
- The documentation map groups links by reader intent.
- The clone URL points to `devhxj/Jazor`.
```

- [ ] **Step 2: Run targeted grep checks for known stale wording**

Run:

```bash
grep -nE "high-performance|complete semantic equivalence|Complete Syntax Support|Advanced Pattern Matching|Async Programming Support|ECMAScript Module System|Static Analysis|Source Map & Debugging" README.md || true
```

Expected: no matches from the removed top-level marketing/status lists

- [ ] **Step 3: Inspect the final diff**

Run:

```bash
git diff -- README.md
```

Expected: the diff only shows the planned README refresh, with no unrelated file edits

- [ ] **Step 4: Mark the implementation ready for user review**

After the checks pass, stop and report:

```md
README refresh complete in `README.md`. Please review the wording before any further doc sync work (such as `README_CN.md`) is started.
```

---

## Self-Review

- **Spec coverage:** The plan covers the hero/status rewrite, documentation map regrouping, component/capability pruning, build/test fixes, and final verification. No spec section is left without an implementation task.
- **Placeholder scan:** No `TBD`, `TODO`, or “similar to previous task” shortcuts remain. Every edit step includes exact markdown or exact commands.
- **Type consistency:** Section names are consistent across tasks: `What Jazor focuses on today`, `Project status`, `Current capability snapshot`, and `Documentation map` are used consistently.
