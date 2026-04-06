# RazorVue Layering Refactor Implementation Plan

> Status: active plan
> Positioning: Execution-level implementation plan for the RazorVue layering closure lane.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 把 RazorVue 核心语义从 `Jazor.RazorVue.Analysis` 迁回 `Jazor.RazorVue`，让 `Analysis` 收敛为薄 generator/analyzer 入口层。

**Architecture:** `Jazor.RazorVue` 成为 RazorVue 核心装配点，承载 descriptor、render-tree、lowering、artifact、pipeline 等核心语义；`Jazor.RazorVue.Analysis` 只保留 Roslyn generator 入口与诊断投影。先做物理代码归属调整，再做引用/命名空间修正，最后以现有 RazorVue pipeline/generator 测试作为回归护栏。

**Tech Stack:** C# 14, .NET 10, Roslyn incremental generators, MSTest, RazorVue pipeline tests

---

## File Structure

### Core destination
- Modify: `src/Jazor.RazorVue/Jazor.RazorVue.csproj`
- Modify/Create: `src/Jazor.RazorVue/VueComponent.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/RazorVueCompilationSymbols.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/RazorVueCompilationContext.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/RazorVueComponentCandidate.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/RazorVueEntryKind.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/RazorVuePipeline.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/Artifacts/*.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/Descriptor/*.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/Discovery/*.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/Extensibility/*.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/Lowering/*.cs`
- Create/Move: `src/Jazor.RazorVue/RazorVue/RenderTree/*.cs`

### Thin analysis entry
- Modify: `src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj`
- Modify: `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
- Delete/Remove from project: moved core files under `src/Jazor.RazorVue.Analysis/RazorVue/**`

### Tests and docs
- Modify: `src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- Modify: `src/Jazor.CompilerTest/ESGeneratorTests.cs`
- Modify: `src/Jazor.Compiler/doc/RazorVue.ProjectResponsibilities.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.Design.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`

---

### Task 1: Retarget project boundaries

**Files:**
- Modify: `src/Jazor.RazorVue/Jazor.RazorVue.csproj`
- Modify: `src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj`
- Test: `src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`

- [ ] **Step 1: Write the failing boundary check**

Document the intended post-refactor dependency direction in code comments at the top of both project files.

```xml
<!-- Jazor.RazorVue owns RazorVue core semantics: descriptor, render tree, lowering, artifacts, and pipeline. -->
<!-- Jazor.RazorVue.Analysis stays thin and only hosts Roslyn generator/analyzer entry glue. -->
```

- [ ] **Step 2: Run targeted build to capture the current boundary failure**

Run:
```bash
dotnet build "D:/repository/own/jazor/Jazor/src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj" -m:1
```

Expected: PASS before changes, establishing the baseline.

- [ ] **Step 3: Update `Jazor.RazorVue.csproj` to own the core dependencies**

Replace the file body with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.3.0" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="5.3.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Jazor.Razor\Jazor.Razor.csproj" />
    <ProjectReference Include="..\Jazor.Compiler\Jazor.Compiler.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Update `Jazor.RazorVue.Analysis.csproj` to become a thin host**

Replace the file body with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.3.0" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="5.3.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Jazor.Compiler\Jazor.Compiler.csproj" />
    <ProjectReference Include="..\Jazor.RazorVue\Jazor.RazorVue.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 5: Run project builds to verify the new dependency direction**

Run:
```bash
dotnet build "D:/repository/own/jazor/Jazor/src/Jazor.RazorVue/Jazor.RazorVue.csproj" -m:1 && dotnet build "D:/repository/own/jazor/Jazor/src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj" -m:1
```

Expected: PASS, or FAIL only on missing moved types/usings that Task 2 will address.

- [ ] **Step 6: Commit**

```bash
git add src/Jazor.RazorVue/Jazor.RazorVue.csproj src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj
git commit -m "refactor: retarget RazorVue layering boundary"
```

### Task 2: Move RazorVue core types into `Jazor.RazorVue`

**Files:**
- Create/Move: `src/Jazor.RazorVue/RazorVue/**`
- Modify/Delete: `src/Jazor.RazorVue.Analysis/RazorVue/**`
- Test: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

- [ ] **Step 1: Write the failing core-ownership test expectation**

Add or update a comment in `src/Jazor.CompilerTest/RazorVuePipelineTests.cs` near the helper imports:

```csharp
// Layering rule: RazorVue pipeline core must come from Jazor.RazorVue, not from Jazor.RazorVue.Analysis.
```

- [ ] **Step 2: Move core directories from Analysis into RazorVue**

Move these exact directories/files:

```text
src/Jazor.RazorVue.Analysis/RazorVue/Artifacts -> src/Jazor.RazorVue/RazorVue/Artifacts
src/Jazor.RazorVue.Analysis/RazorVue/Descriptor -> src/Jazor.RazorVue/RazorVue/Descriptor
src/Jazor.RazorVue.Analysis/RazorVue/Discovery -> src/Jazor.RazorVue/RazorVue/Discovery
src/Jazor.RazorVue.Analysis/RazorVue/Extensibility -> src/Jazor.RazorVue/RazorVue/Extensibility
src/Jazor.RazorVue.Analysis/RazorVue/Lowering -> src/Jazor.RazorVue/RazorVue/Lowering
src/Jazor.RazorVue.Analysis/RazorVue/RenderTree -> src/Jazor.RazorVue/RazorVue/RenderTree
src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationSymbols.cs -> src/Jazor.RazorVue/RazorVue/RazorVueCompilationSymbols.cs
src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationContext.cs -> src/Jazor.RazorVue/RazorVue/RazorVueCompilationContext.cs
src/Jazor.RazorVue.Analysis/RazorVue/RazorVueComponentCandidate.cs -> src/Jazor.RazorVue/RazorVue/RazorVueComponentCandidate.cs
src/Jazor.RazorVue.Analysis/RazorVue/RazorVueEntryKind.cs -> src/Jazor.RazorVue/RazorVue/RazorVueEntryKind.cs
src/Jazor.RazorVue.Analysis/RazorVue/RazorVuePipeline.cs -> src/Jazor.RazorVue/RazorVue/RazorVuePipeline.cs
```

- [ ] **Step 3: Rewrite namespaces from `.Analysis` to core `Jazor.RazorVue` namespaces**

Apply these exact namespace shapes:

```csharp
namespace Jazor.RazorVue;
namespace Jazor.RazorVue.Artifacts;
namespace Jazor.RazorVue.Descriptor;
namespace Jazor.RazorVue.Discovery;
namespace Jazor.RazorVue.Extensibility;
namespace Jazor.RazorVue.Lowering;
namespace Jazor.RazorVue.RenderTree;
```

- [ ] **Step 4: Keep seam comments where ownership changes**

At the top of `src/Jazor.RazorVue/RazorVue/RazorVuePipeline.cs`, replace the old ownership implication with a comment like:

```csharp
// RazorVuePipeline now lives in Jazor.RazorVue because it is RazorVue core orchestration,
// not Roslyn generator host glue. Jazor.RazorVue.Analysis should only call into this type.
```

At the top of `src/Jazor.RazorVue/VueComponent.cs`, replace the current summary body with:

```csharp
/// <summary>
/// RazorVue 的基础组件类型，同时所在程序集也是 RazorVue 核心语义的归属层。
/// 为什么这样分层：Vue authoring surface 与 RazorVue descriptor/lowering/pipeline 属于同一个产品核心，
/// 而 Roslyn generator 入口只是在 Analysis 层做薄接线，不再承载核心实现。
/// </summary>
```

- [ ] **Step 5: Run the pipeline tests to verify the moved core still works**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVuePipelineTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Jazor.RazorVue src/Jazor.RazorVue.Analysis src/Jazor.CompilerTest/RazorVuePipelineTests.cs
git commit -m "refactor: move RazorVue core into Jazor.RazorVue"
```

### Task 3: Thin `Jazor.RazorVue.Analysis` down to generator host glue

**Files:**
- Modify: `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
- Modify/Delete: moved files no longer referenced in `src/Jazor.RazorVue.Analysis/RazorVue/**`
- Test: `src/Jazor.CompilerTest/ESGeneratorTests.cs`

- [ ] **Step 1: Write the failing generator-host expectation**

Add this comment above `RazorVueGenerator`:

```csharp
// Thin host rule: this generator owns Roslyn wiring and diagnostics only; RazorVue semantics live in Jazor.RazorVue.
```

- [ ] **Step 2: Update generator usings to point at core namespaces**

Change the top using block to:

```csharp
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
```

- [ ] **Step 3: Keep generator logic thin**

Inside `EmitRazorVueCatalog`, preserve this shape:

```csharp
var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
if (razorVueContext is null)
    return;

var catalog = new RazorVuePipeline().Execute(compilation);
```

Do not reintroduce lowering/descriptor logic into `RazorVueGenerator`.

- [ ] **Step 4: Update `ESGeneratorTests.cs` imports if needed**

Ensure tests reference core namespaces where types moved. The top using block should continue to compile with a shape like:

```csharp
using Jazor.RazorVue;
using Jazor.RazorVue.Analysis;
```

Add additional `using Jazor.RazorVue.Artifacts;` / `using Jazor.RazorVue.Descriptor;` only if the compiler requires them.

- [ ] **Step 5: Run generator tests to verify the thin host still emits catalog and diagnostics**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~ESGeneratorTests"
```

Expected: PASS, including `JAZORVGA001`~`JAZORVGA005` regression coverage.

- [ ] **Step 6: Commit**

```bash
git add src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs src/Jazor.CompilerTest/ESGeneratorTests.cs
git commit -m "refactor: thin RazorVue analysis entry"
```

### Task 4: Sync docs and final verification

**Files:**
- Modify: `src/Jazor.Compiler/doc/RazorVue.ProjectResponsibilities.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.Design.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`
- Test: `src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`

- [ ] **Step 1: Update project responsibility wording**

Rewrite the split in `RazorVue.ProjectResponsibilities.md` so it says:

```md
- `Jazor.RazorVue`
  owns the Vue-facing authoring substrate (`VueComponent`) and RazorVue core semantics, including descriptor shaping, render-tree recovery, lowering, artifacts, and pipeline orchestration
- `Jazor.RazorVue.Analysis`
  owns the thin RazorVue generator/analyzer-facing entry that adapts Roslyn inputs and projects diagnostics
```

- [ ] **Step 2: Update hard rules**

Replace the old hard rules with wording that includes:

```md
3. `Jazor.RazorVue` is the RazorVue core layer and must own RazorVue-specific descriptor/render-tree/lowering/pipeline logic.
4. `Jazor.RazorVue.Analysis` must stay thin and must not absorb new RazorVue core semantics.
```

- [ ] **Step 3: Update overview/design/checklist docs to match the new ownership**

Make sure they consistently say:

```md
`Jazor.RazorVue` is the RazorVue core layer.
`Jazor.RazorVue.Analysis` is the thin Roslyn entry layer.
```

- [ ] **Step 4: Run final regression set**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVuePipelineTests|FullyQualifiedName~ESGeneratorTests"
```

Expected: PASS.

- [ ] **Step 5: Run focused build verification**

Run:
```bash
dotnet build "D:/repository/own/jazor/Jazor/src/Jazor.RazorVue/Jazor.RazorVue.csproj" -m:1 && dotnet build "D:/repository/own/jazor/Jazor/src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj" -m:1
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Jazor.Compiler/doc/RazorVue.ProjectResponsibilities.md src/Jazor.Compiler/doc/RazorVue.Design.md src/Jazor.Compiler/doc/RazorVue.Overview.md src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md
git commit -m "docs: align RazorVue layering responsibilities"
```

---

## Self-Review

### Spec coverage
- Covers project retargeting and dependency direction
- Covers core file movement from `Analysis` to `RazorVue`
- Covers generator thinning
- Covers test updates and docs sync
- Covers final build/test verification

### Placeholder scan
- No TBD/TODO placeholders
- All tasks include exact file paths
- All tasks include exact commands
- Code-changing steps include exact code snippets or exact namespace/file moves

### Type consistency
- Core namespaces consistently use `Jazor.RazorVue.*`
- Generator host remains `Jazor.RazorVue.Analysis`
- Tests continue to target `RazorVuePipelineTests` and `ESGeneratorTests`
