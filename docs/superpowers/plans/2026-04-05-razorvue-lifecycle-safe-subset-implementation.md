# RazorVue Lifecycle Safe Subset Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让 RazorVue 的 `OnInitialized*`、`OnParametersSet*`、`OnAfterRender*` 从“存在性元数据/空 hook 壳”升级为“安全子集可执行、超界即明确诊断”的稳定主链路。

**Architecture:** 继续沿用 `RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVueArtifactFactory -> RazorVueGenerator` 现有主链路，不引入新的 class-instance runtime。实现聚焦在 snapshot carrier、lowering 诊断、Vue hook/watch 代码生成和对应测试，所有超出 closure-safe 边界的生命周期方法体都走结构化 issue，而不是静默 fallback。

**Tech Stack:** C# 14 / .NET 10、Roslyn Incremental Generator、MSTest、Vue Composition API codegen (`defineComponent`, `onMounted`, `onUpdated`, `watch`)

---

## File Map

- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueSemanticSnapshot.cs`
  - 补齐 lifecycle lowering 需要的最小语义 carrier，纳入 `OnParametersSet*`。
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationContext.cs`
  - 在创建 snapshot 时把 `OnParametersSetMethod` / `OnParametersSetAsyncMethod` 一并带下去。
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/RazorVueCompilationIssue.cs`
  - 新增 lifecycle lowering 专用 issue code。
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/RazorVueCompilationIssueException.cs`
  - 保持结构化 issue 通道承载 lifecycle lowering 失败。
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs`
  - 本轮核心：安全子集校验、Vue imports、`onMounted`/`onUpdated`/`watch` 代码生成、`firstRender` 桥接、关键边界注释。
- Modify: `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
  - 把新的 lifecycle lowering issue 投影为明确诊断。
- Modify: `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`
  - 补 snapshot carrier 与 lifecycle 发现测试。
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
  - 补 lifecycle lowering 成功/失败测试。
- Modify: `src/Jazor.CompilerTest/ESGeneratorTests.cs`
  - 补 generator 级 lifecycle 诊断与成功产物测试。
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
  - 更新已支持生命周期与边界描述。
- Modify: `src/Jazor.Compiler/doc/RazorVue.Design.md`
  - 更新 lowering 设计与 failure boundary。
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`
  - 勾掉本轮完成项，保留非目标项。

---

### Task 1: 扩展 snapshot carrier 并为 lifecycle lowering 建立专用 issue code

**Files:**
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueSemanticSnapshot.cs`
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationContext.cs`
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/RazorVueCompilationIssue.cs`
- Test: `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`

- [ ] **Step 1: 写失败测试，先锁定 snapshot 必须携带 `OnParametersSet*`**

```csharp
[TestMethod]
public void RazorVue_Snapshot_ContainsParameterSetLifecycleMethods()
{
    var snapshot = CreateSingleSnapshot(
        """
        using System;
        using System.Threading.Tasks;
        using Jazor.RazorVue;

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }

        namespace Demo.Components
        {
            [ECMAScript.ECMAScriptModule("./components/parameter-card")]
            public class ParameterCard : VueComponent
            {
                protected override void OnParametersSet()
                {
                }

                protected override Task OnParametersSetAsync()
                    => Task.CompletedTask;
            }
        }
        """);

    Assert.IsTrue(snapshot.Lifecycle.HasOnParametersSet);
    Assert.IsTrue(snapshot.Lifecycle.HasOnParametersSetAsync);
    Assert.IsNotNull(snapshot.OnParametersSetMethod);
    Assert.IsNotNull(snapshot.OnParametersSetAsyncMethod);
}
```

- [ ] **Step 2: 运行单测，确认它先失败**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVueDescriptorExtractionTests.RazorVue_Snapshot_ContainsParameterSetLifecycleMethods" /m:1
```

Expected: FAIL，报 `RazorVueSemanticSnapshot` 不包含 `OnParametersSetMethod` / `OnParametersSetAsyncMethod`。

- [ ] **Step 3: 最小实现 snapshot carrier 与 issue code**

`src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueSemanticSnapshot.cs`
```csharp
public sealed record RazorVueSemanticSnapshot(
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol? BuildRenderTreeMethod,
    IMethodSymbol? OnInitializedMethod,
    IMethodSymbol? OnInitializedAsyncMethod,
    IMethodSymbol? OnParametersSetMethod,
    IMethodSymbol? OnParametersSetAsyncMethod,
    IMethodSymbol? OnAfterRenderMethod,
    IMethodSymbol? OnAfterRenderAsyncMethod,
    VueLifecycleDescriptor Lifecycle,
    VueLogicDescriptor Logic,
    VueComponentDescriptor Descriptor,
    ImmutableArray<RazorVueSourceOrigin> Origins);
```

`src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationContext.cs`
```csharp
return new RazorVueSemanticSnapshot(
    candidate.ComponentSymbol,
    candidate.BuildRenderTreeMethod,
    candidate.OnInitializedMethod,
    candidate.OnInitializedAsyncMethod,
    candidate.OnParametersSetMethod,
    candidate.OnParametersSetAsyncMethod,
    candidate.OnAfterRenderMethod,
    candidate.OnAfterRenderAsyncMethod,
    lifecycle,
    logic,
    descriptor,
    origins);
```

`src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/RazorVueCompilationIssue.cs`
```csharp
public enum RazorVueIssueCode
{
    ComponentNotFound,
    AmbiguousComponentName,
    ReservedIntrinsicNameCollision,
    UnsupportedLifecycleLowering
}
```

- [ ] **Step 4: 补充现有 descriptor 测试断言**

`src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`
```csharp
Assert.IsTrue(snapshot.Lifecycle.HasOnParametersSet);
Assert.IsTrue(snapshot.Lifecycle.HasOnParametersSetAsync);
Assert.IsNotNull(snapshot.OnParametersSetMethod);
Assert.IsNotNull(snapshot.OnParametersSetAsyncMethod);
```

- [ ] **Step 5: 重新运行 descriptor 测试，确认通过**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVueDescriptorExtractionTests" /m:1
```

Expected: PASS。

- [ ] **Step 6: 提交这一小步**

```bash
git add src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueSemanticSnapshot.cs src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationContext.cs src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/RazorVueCompilationIssue.cs src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs
git commit -m "feat(razorvue): carry parameter lifecycle methods in semantic snapshot"
```

---

### Task 2: 把 lifecycle lowering 超界失败接入结构化诊断通道

**Files:**
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- Modify: `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
- Test: `src/Jazor.CompilerTest/ESGeneratorTests.cs`

- [ ] **Step 1: 先写 generator 失败测试，锁定新的专门诊断**

```csharp
[TestMethod]
public void GenerateCatalog_WithUnsupportedLifecycleBody_ReportsLifecycleDiagnostic()
{
    var compilation = CreateCompilation(
        "RazorVue.Generator.Tests",
        """
        using System;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }

        namespace Demo.Components
        {
            [ECMAScript.ECMAScriptModule("./components/bad-card")]
            public class BadCard : VueComponent
            {
                private int _count;

                protected override void OnInitialized()
                {
                    _count++;
                }
            }
        }
        """);

    var runResult = RunGenerator(compilation, out _);
    var diagnostics = runResult.Results
        .SelectMany(static result => result.Diagnostics)
        .Where(static diagnostic => diagnostic.Id == "JAZORVGA005")
        .ToArray();

    Assert.AreEqual(1, diagnostics.Length);
    StringAssert.Contains(diagnostics[0].GetMessage(), "OnInitialized");
    StringAssert.Contains(diagnostics[0].GetMessage(), "BadCard");
}
```

- [ ] **Step 2: 运行单测，确认先失败**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~ESGeneratorTests.GenerateCatalog_WithUnsupportedLifecycleBody_ReportsLifecycleDiagnostic" /m:1
```

Expected: FAIL，当前只会落到 `JAZORVGA001` 或没有 `JAZORVGA005`。

- [ ] **Step 3: 在 generator 中注册新的 diagnostic descriptor**

`src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
```csharp
private static readonly DiagnosticDescriptor RazorVueUnsupportedLifecycleLowering = new(
    id: "JAZORVGA005",
    title: "RazorVue lifecycle lowering is unsupported",
    messageFormat: "{0}",
    category: "Jazor.RazorVue.Analysis",
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);
```

并更新 descriptor 选择逻辑：
```csharp
private static DiagnosticDescriptor GetCompilationIssueDescriptor(RazorVueIssueCode code)
    => code switch
    {
        RazorVueIssueCode.ComponentNotFound => RazorVueComponentNotFound,
        RazorVueIssueCode.AmbiguousComponentName => RazorVueAmbiguousComponentName,
        RazorVueIssueCode.ReservedIntrinsicNameCollision => RazorVueReservedIntrinsicNameCollision,
        RazorVueIssueCode.UnsupportedLifecycleLowering => RazorVueUnsupportedLifecycleLowering,
        _ => RazorVueGenerationFailed
    };
```

- [ ] **Step 4: 用结构化 issue 替换 `NotSupportedException`**

`src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs`
```csharp
private static RazorVueCompilationIssueException CreateUnsupportedLifecycleLoweringException(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol method,
    string reason)
{
    var issue = new RazorVueCompilationIssue(
        RazorVueIssueCode.UnsupportedLifecycleLowering,
        RazorVueIssueSeverity.Error,
        $"Lifecycle method '{method.Name}' in component '{snapshot.Descriptor.FullName}' is outside the supported RazorVue lifecycle safe subset: {reason}",
        ImmutableArray.Create(snapshot.Descriptor.FullName));

    var origin = method.Locations
        .Where(static location => location.IsInSource)
        .Select(static location => RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Component))
        .FirstOrDefault();

    return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, origin);
}
```

- [ ] **Step 5: 让 safe-subset 校验统一抛结构化 issue**

```csharp
if (methodSyntax.Body is null || !IsSupportedLifecycleBody(snapshot, method, methodSyntax.Body))
    throw CreateUnsupportedLifecycleLoweringException(snapshot, method, "component fields, instance methods, and unsupported statements are not allowed");
```

- [ ] **Step 6: 重新运行 generator 测试，确认报出 `JAZORVGA005`**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~ESGeneratorTests.GenerateCatalog_WithUnsupportedLifecycleBody_ReportsLifecycleDiagnostic" /m:1
```

Expected: PASS。

- [ ] **Step 7: 提交这一小步**

```bash
git add src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs src/Jazor.CompilerTest/ESGeneratorTests.cs
git commit -m "feat(razorvue): report structured diagnostics for unsupported lifecycle lowering"
```

---

### Task 3: 实现 `OnInitialized*` 和 `OnAfterRender*` 的安全子集 lowering

**Files:**
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- Test: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

- [ ] **Step 1: 先写 pipeline 成功测试，锁定 `onMounted` / `onUpdated` / `firstRender` 形状**

```csharp
[TestMethod]
public void RazorVue_Pipeline_LowersAfterRenderHooks_WithFirstRenderBridge()
{
    var context = CreateContext(
        """
        using System;
        using System.Threading.Tasks;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }

        namespace Demo.Components
        {
            [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
            public class LifecycleCard : VueComponent
            {
                protected override void OnInitialized()
                {
                }

                protected override Task OnAfterRenderAsync(bool firstRender)
                    => Task.CompletedTask;
            }
        }
        """);

    var artifact = new RazorVuePipeline().Execute(context).Artifacts[0];
    StringAssert.Contains(artifact.ModuleCode, "import { defineComponent, h, onMounted, onUpdated } from \"vue\";");
    StringAssert.Contains(artifact.ModuleCode, "let __jazorFirstRender = true;");
    StringAssert.Contains(artifact.ModuleCode, "onMounted(() => {");
    StringAssert.Contains(artifact.ModuleCode, "onUpdated(async () => {");
    StringAssert.Contains(artifact.ModuleCode, "const firstRender = __jazorFirstRender;");
    StringAssert.Contains(artifact.ModuleCode, "__jazorFirstRender = false;");
}
```

- [ ] **Step 2: 运行测试，确认先失败**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVuePipelineTests.RazorVue_Pipeline_LowersAfterRenderHooks_WithFirstRenderBridge" /m:1
```

Expected: FAIL，当前 `ModuleCode` 中还没有 `__jazorFirstRender` 与完整 hook 形状。

- [ ] **Step 3: 实现 lifecycle imports、`firstRender` 桥接和关键注释**

`src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs`
```csharp
private static void AppendLifecycleLowering(
    StringBuilder builder,
    RazorVueSemanticSnapshot snapshot)
{
    // RazorVue 当前生成目标是 Vue setup() closure，而不是 class component runtime。
    // 因此这里只 lower 可证明安全的生命周期子集；一旦方法体依赖实例字段/实例方法，
    // 就必须显式失败，避免生成“能跑但语义错误”的代码。
    var needsFirstRenderBridge = snapshot.OnAfterRenderMethod is not null || snapshot.OnAfterRenderAsyncMethod is not null;
    if (needsFirstRenderBridge)
        builder.AppendLine("    let __jazorFirstRender = true;");

    AppendMountedLifecycle(builder, snapshot);
    AppendUpdatedLifecycle(builder, snapshot);
}
```

```csharp
private static void AppendMountedLifecycle(StringBuilder builder, RazorVueSemanticSnapshot snapshot)
{
    AppendLifecycleHook(builder, "onMounted", snapshot, snapshot.OnInitializedMethod, isAsync: false, passFirstRender: false, updateFirstRenderState: false);
    AppendLifecycleHook(builder, "onMounted", snapshot, snapshot.OnInitializedAsyncMethod, isAsync: true, passFirstRender: false, updateFirstRenderState: false);
    AppendLifecycleHook(builder, "onMounted", snapshot, snapshot.OnAfterRenderMethod, isAsync: false, passFirstRender: true, updateFirstRenderState: true);
    AppendLifecycleHook(builder, "onMounted", snapshot, snapshot.OnAfterRenderAsyncMethod, isAsync: true, passFirstRender: true, updateFirstRenderState: true);
}
```

```csharp
private static void AppendUpdatedLifecycle(StringBuilder builder, RazorVueSemanticSnapshot snapshot)
{
    AppendLifecycleHook(builder, "onUpdated", snapshot, snapshot.OnAfterRenderMethod, isAsync: false, passFirstRender: true, updateFirstRenderState: false);
    AppendLifecycleHook(builder, "onUpdated", snapshot, snapshot.OnAfterRenderAsyncMethod, isAsync: true, passFirstRender: true, updateFirstRenderState: false);
}
```

- [ ] **Step 4: 为 `firstRender` 与安全方法体发射最小语句集**

```csharp
private static void AppendLifecycleHook(
    StringBuilder builder,
    string hookName,
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol? method,
    bool isAsync,
    bool passFirstRender,
    bool updateFirstRenderState)
{
    if (method is null)
        return;

    var methodSyntax = GetLifecycleMethodSyntax(snapshot, method);
    builder.Append("    ").Append(hookName).Append("(");
    if (isAsync)
        builder.Append("async ");
    builder.AppendLine("() => {");

    if (passFirstRender)
        builder.AppendLine("      const firstRender = __jazorFirstRender;");

    AppendLifecycleMethodBody(builder, snapshot, method, methodSyntax);

    if (updateFirstRenderState)
        builder.AppendLine("      __jazorFirstRender = false;");

    builder.AppendLine("    });");
}
```

- [ ] **Step 5: 重新运行 pipeline 生命周期测试**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVuePipelineTests" /m:1
```

Expected: PASS，至少覆盖 `onMounted` / `onUpdated` / `firstRender` 代码形状。

- [ ] **Step 6: 提交这一小步**

```bash
git add src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs src/Jazor.CompilerTest/RazorVuePipelineTests.cs
git commit -m "feat(razorvue): lower initialized and after-render lifecycle hooks"
```

---

### Task 4: 实现 `OnParametersSet*` 的 props bridge 与安全子集校验

**Files:**
- Modify: `src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- Test: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

- [ ] **Step 1: 先写 `OnParametersSet*` pipeline 测试**

```csharp
[TestMethod]
public void RazorVue_Pipeline_LowersParameterSetHooks_WithImmediatePropsBridge()
{
    var context = CreateContext(
        """
        using System;
        using System.Threading.Tasks;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }

        namespace Demo.Components
        {
            [ECMAScript.ECMAScriptModule("./components/parameter-card")]
            public class ParameterCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override Task OnParametersSetAsync()
                    => Task.CompletedTask;
            }
        }
        """);

    var artifact = new RazorVuePipeline().Execute(context).Artifacts[0];
    StringAssert.Contains(artifact.ModuleCode, "import { defineComponent, h, watch } from \"vue\";");
    StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.value], async () => {");
    StringAssert.Contains(artifact.ModuleCode, "{ immediate: true }");
}
```

- [ ] **Step 2: 运行测试，确认先失败**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVuePipelineTests.RazorVue_Pipeline_LowersParameterSetHooks_WithImmediatePropsBridge" /m:1
```

Expected: FAIL，当前 imports 里没有 `watch`，也没有 `immediate` props bridge。

- [ ] **Step 3: 在 imports 中按需引入 `watch`**

```csharp
if (snapshot.OnParametersSetMethod is not null || snapshot.OnParametersSetAsyncMethod is not null)
    vueImports.Add("watch");
```

- [ ] **Step 4: 生成稳定 props bridge，并补注释解释为何不能依赖实例状态**

```csharp
private static void AppendParameterLifecycleLowering(StringBuilder builder, RazorVueSemanticSnapshot snapshot)
{
    if (snapshot.OnParametersSetMethod is null && snapshot.OnParametersSetAsyncMethod is null)
        return;

    // OnParametersSet* 在 RazorVue 中不依赖 class instance 累积状态。
    // 这里把它显式桥接为 props 变化监听，并使用 immediate: true 覆盖首轮执行。
    builder.Append("    watch(() => [");
    builder.Append(string.Join(", ", snapshot.Descriptor.Props.Select(static prop => $"props.{prop.Name}")));
    builder.AppendLine("], async () => {");

    if (snapshot.OnParametersSetMethod is not null)
        AppendLifecycleMethodBody(builder, snapshot, snapshot.OnParametersSetMethod, GetLifecycleMethodSyntax(snapshot, snapshot.OnParametersSetMethod));

    if (snapshot.OnParametersSetAsyncMethod is not null)
        AppendLifecycleMethodBody(builder, snapshot, snapshot.OnParametersSetAsyncMethod, GetLifecycleMethodSyntax(snapshot, snapshot.OnParametersSetAsyncMethod));

    builder.AppendLine("    }, { immediate: true });");
}
```

- [ ] **Step 5: 实现安全子集 statement 发射与越界判定**

```csharp
private static void AppendLifecycleMethodBody(
    StringBuilder builder,
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol method,
    MethodDeclarationSyntax methodSyntax)
{
    var statements = methodSyntax.Body?.Statements;
    if (statements is null)
    {
        if (methodSyntax.ExpressionBody is not null)
        {
            AppendLifecycleExpression(builder, snapshot, method, methodSyntax.ExpressionBody.Expression);
            return;
        }

        return;
    }

    foreach (var statement in statements)
    {
        switch (statement)
        {
            case LocalDeclarationStatementSyntax localDeclaration:
                builder.Append("      ").AppendLine(localDeclaration.ToString());
                break;
            case ExpressionStatementSyntax expressionStatement:
                AppendLifecycleExpression(builder, snapshot, method, expressionStatement.Expression);
                break;
            case ReturnStatementSyntax returnStatement when returnStatement.Expression is not null:
                AppendLifecycleExpression(builder, snapshot, method, returnStatement.Expression);
                break;
            default:
                throw CreateUnsupportedLifecycleLoweringException(snapshot, method, $"statement '{statement.Kind()}' is not supported");
        }
    }
}
```

- [ ] **Step 6: 增加超界失败测试，锁定字段/实例方法不可用**

```csharp
[TestMethod]
public void RazorVue_Pipeline_RejectsParameterLifecycleThatTouchesComponentField()
{
    var context = CreateContext(
        """
        using System;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }

        namespace Demo.Components
        {
            [ECMAScript.ECMAScriptModule("./components/bad-parameter-card")]
            public class BadParameterCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private int _count;

                protected override void OnParametersSet()
                {
                    _count = Value;
                }
            }
        }
        """);

    var ex = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
    Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, ex.Issue.Code);
    StringAssert.Contains(ex.Issue.Message, "OnParametersSet");
}
```

- [ ] **Step 7: 运行 pipeline 测试，确认成功/失败路径都通过**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVuePipelineTests" /m:1
```

Expected: PASS。

- [ ] **Step 8: 提交这一小步**

```bash
git add src/Jazor.RazorVue.Analysis/RazorVue/Lowering/RazorVueArtifactFactory.cs src/Jazor.CompilerTest/RazorVuePipelineTests.cs
git commit -m "feat(razorvue): bridge parameter lifecycle hooks through props watch"
```

---

### Task 5: 补齐 generator 成功路径与回归测试

**Files:**
- Modify: `src/Jazor.CompilerTest/ESGeneratorTests.cs`
- Test: `src/Jazor.CompilerTest/ESGeneratorTests.cs`

- [ ] **Step 1: 先写 generator 成功测试，锁定 lifecycle module code 被写入 catalog**

```csharp
[TestMethod]
public void GenerateCatalog_WithLifecycleSafeSubset_EmitsLifecycleModuleCode()
{
    var compilation = CreateCompilation(
        "RazorVue.Generator.Tests",
        """
        using System;
        using System.Threading.Tasks;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;

        namespace ECMAScript
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class ECMAScriptModuleAttribute : Attribute
            {
                public ECMAScriptModuleAttribute() { }
                public ECMAScriptModuleAttribute(string import) { }
            }
        }

        namespace Demo.Components
        {
            [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
            public class LifecycleCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override void OnInitialized()
                {
                }

                protected override Task OnParametersSetAsync()
                    => Task.CompletedTask;

                protected override Task OnAfterRenderAsync(bool firstRender)
                    => Task.CompletedTask;
            }
        }
        """);

    var outputCompilation = RunGenerator(compilation, out var generatedSource);
    var diagnostics = outputCompilation.GetDiagnostics()
        .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
        .ToArray();

    Assert.AreEqual(0, diagnostics.Length, string.Join("\n", diagnostics.Select(static x => x.ToString())));
    StringAssert.Contains(generatedSource, "onMounted");
    StringAssert.Contains(generatedSource, "onUpdated");
    StringAssert.Contains(generatedSource, "watch(() => [props.value]");
}
```

- [ ] **Step 2: 运行 generator 成功测试，确认先失败**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~ESGeneratorTests.GenerateCatalog_WithLifecycleSafeSubset_EmitsLifecycleModuleCode" /m:1
```

Expected: FAIL，当前 catalog source 还没有完整 lifecycle safe-subset codegen。

- [ ] **Step 3: 修正 generator 级断言和 failure path 覆盖**

```csharp
var lifecycleDiagnostics = runResult.Results
    .SelectMany(static result => result.Diagnostics)
    .Where(static diagnostic => diagnostic.Id is "JAZORVGA005")
    .ToArray();

Assert.AreEqual(1, lifecycleDiagnostics.Length);
StringAssert.Contains(lifecycleDiagnostics[0].GetMessage(), "outside the supported RazorVue lifecycle safe subset");
```

- [ ] **Step 4: 跑完整 generator 测试集**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~ESGeneratorTests" /m:1
```

Expected: PASS。

- [ ] **Step 5: 提交这一小步**

```bash
git add src/Jazor.CompilerTest/ESGeneratorTests.cs
git commit -m "test(razorvue): cover lifecycle lowering at generator level"
```

---

### Task 6: 更新文档并做定向回归验证

**Files:**
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.Design.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`
- Test: `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`
- Test: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- Test: `src/Jazor.CompilerTest/ESGeneratorTests.cs`

- [ ] **Step 1: 更新 Overview，明确“已支持安全子集”而不是完整逻辑 lowering**

`src/Jazor.Compiler/doc/RazorVue.Overview.md`
```md
- RazorVue 现已支持 `OnInitialized*`、`OnParametersSet*`、`OnAfterRender*` 的 lifecycle safe subset lowering。
- `OnParametersSet*` 通过显式 props watch bridge 进入 Vue `setup()`。
- `OnAfterRender*` 通过 `onMounted` / `onUpdated` 和显式 `firstRender` bridge 对齐。
- 访问组件字段、实例方法、`this` 的生命周期方法体仍会触发编译诊断。
```

- [ ] **Step 2: 更新 Design，写清楚边界与 failure policy**

`src/Jazor.Compiler/doc/RazorVue.Design.md`
```md
### Lifecycle safe subset lowering

RazorVue 当前不会把 `VueComponent` 还原为完整 class runtime，而是把 closure-safe 生命周期子集 lower 到 Vue Composition API。任何依赖实例字段、实例方法或 `this` 的生命周期逻辑都会被视为超界，并以结构化 diagnostic 失败，而不是生成语义错误的 JavaScript。
```

- [ ] **Step 3: 更新 Checklist，勾掉已完成项**

`src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`
```md
- [x] Lower `OnInitialized*` into Vue lifecycle hooks
- [x] Bridge `OnParametersSet*` through explicit props watch with immediate execution
- [x] Lower `OnAfterRender*` with `firstRender` bridge
- [x] Emit explicit diagnostics for unsupported lifecycle lowering
- [ ] Lower instance fields / instance methods into setup state
- [ ] Support `Dispose*`, `ShouldRender`, `SetParametersAsync`
```

- [ ] **Step 4: 跑定向回归测试，避免再触发 MSBuild OOM**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVueDescriptorExtractionTests|FullyQualifiedName~RazorVuePipelineTests|FullyQualifiedName~ESGeneratorTests" /m:1
```

Expected: PASS。

- [ ] **Step 5: 如果上一步内存仍高，分三次跑并记录结果**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVueDescriptorExtractionTests" /m:1

dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVuePipelineTests" /m:1

dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~ESGeneratorTests" /m:1
```

Expected: 三组都 PASS。

- [ ] **Step 6: 提交文档与最终回归结果**

```bash
git add src/Jazor.Compiler/doc/RazorVue.Overview.md src/Jazor.Compiler/doc/RazorVue.Design.md src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md
git commit -m "docs(razorvue): document lifecycle safe subset lowering"
```

---

## Self-Review Notes

- Spec coverage: 已覆盖 snapshot carrier、专门诊断、`OnInitialized*`、`OnParametersSet*`、`OnAfterRender*`、`firstRender`、测试、文档、注释边界。
- Placeholder scan: 计划中未保留 `TODO` / `TBD` / “later” 之类占位语句。
- Type consistency: 统一使用 `UnsupportedLifecycleLowering` / `JAZORVGA005` / `OnParametersSetMethod` / `OnParametersSetAsyncMethod` 命名。
