# RazorVue Helper Composition Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 RazorVue setup-side logic 增加受限 helper composition 闭环，让 `render -> helperA -> helperB` 在两层固定深度内稳定 lowering 到 `setup()`，超过边界时显式报 `JAZORVGA006`。

**Architecture:** 保持现有 `RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVuePipeline -> RazorVueArtifactFactory -> RazorVueCatalog` 主链路不变。通过在 `RazorVueExpressionEmitter` 中补 helper 依赖登记与深度边界、在 `RazorVueArtifactFactory` 中复用现有“迭代直到稳定”的 helper materialization 结构，完成最小闭环，而不是引入任意深度图算法或组件实例 runtime。

**Tech Stack:** C# 14 / .NET 10、Roslyn `IOperation`、MSTest、Vue `defineComponent + setup + render` codegen

---

## File Map

- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
  - 增加 helper 深度登记状态。
  - 把 render/setup 中的 helper 调用都收口到统一的依赖登记逻辑。
  - 对三层 helper 链抛出更明确的 `UnsupportedSetupLogicLowering`。
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
  - 在生成 helper body 时标记当前 owner helper。
  - 继续使用 while-loop 增量 materialize root helper 与 inner helper。
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
  - 新增两层 helper 成功 lowering、field/props/helper 混合成功、共享 inner helper 不重复 materialize、三层 helper 失败、两层内 async helper 失败。
- Modify: `src/Jazor.CompilerTest/ESGeneratorTests.cs`
  - 新增 generator 级三层 helper 结构化诊断投影测试，确保 `JAZORVGA006` 继续透出。
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
  - 把 setup-side logic 当前能力更新为“受限 helper composition”。
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`
  - 把当前已证明 lowering 子集更新为“两层固定深度 helper composition”。

---

### Task 1: 先用测试锁定两层 helper composition 成功路径

**Files:**
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

- [ ] **Step 1: 新增“两层 helper 链成功 lowering”测试**

```csharp
[TestMethod]
public void RazorVue_Pipeline_LowersTwoLevelHelperCompositionIntoSetupScope()
{
    var context = CreateContext(
        """
        using System;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

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
            [ECMAScript.ECMAScriptModule("./components/helper-card")]
            public class HelperCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private string FormatInner(int value)
                    => (value * 2).ToString();

                private string FormatOuter(int value)
                    => "Value: " + FormatInner(value);

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, FormatOuter(Value));
                    builder.CloseElement();
                }
            }
        }
        """);

    var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

    StringAssert.Contains(artifact.ModuleCode, "function formatOuter(value)");
    StringAssert.Contains(artifact.ModuleCode, "function formatInner(value)");
    StringAssert.Contains(artifact.ModuleCode, "return (\"Value: \" + formatInner(value));");
    StringAssert.Contains(artifact.ModuleCode, "return ((value * 2)).toString();");
    StringAssert.Contains(artifact.ModuleCode, "return () => h(\"section\", null, formatOuter(props.value));");
}
```

- [ ] **Step 2: 新增“field/props/helper 混合读取成功”测试**

```csharp
[TestMethod]
public void RazorVue_Pipeline_LowersTwoLevelHelperCompositionWithFieldAndPropsIntoSetupScope()
{
    var context = CreateContext(
        """
        using System;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

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
            [ECMAScript.ECMAScriptModule("./components/helper-card")]
            public class HelperCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private readonly string Prefix = "Count: ";

                private string FormatLeaf(int value)
                    => Prefix + value;

                private string FormatOuter()
                    => FormatLeaf(Value);

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, FormatOuter());
                    builder.CloseElement();
                }
            }
        }
        """);

    var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

    StringAssert.Contains(artifact.ModuleCode, "const prefix = \"Count: \";");
    StringAssert.Contains(artifact.ModuleCode, "function formatLeaf(value)");
    StringAssert.Contains(artifact.ModuleCode, "function formatOuter()");
    StringAssert.Contains(artifact.ModuleCode, "return (prefix + value);");
    StringAssert.Contains(artifact.ModuleCode, "return formatLeaf(props.value);");
}
```

- [ ] **Step 3: 新增“共享 inner helper 只 materialize 一次”测试**

```csharp
[TestMethod]
public void RazorVue_Pipeline_LowersSharedInnerHelperOnlyOnceIntoSetupScope()
{
    var context = CreateContext(
        """
        using System;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

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
            [ECMAScript.ECMAScriptModule("./components/helper-card")]
            public class HelperCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private string FormatLeaf(int value)
                    => (value + 1).ToString();

                private string FormatA(int value)
                    => "A:" + FormatLeaf(value);

                private string FormatB(int value)
                    => "B:" + FormatLeaf(value);

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, FormatA(Value));
                    builder.AddContent(2, FormatB(Value));
                    builder.CloseElement();
                }
            }
        }
        """);

    var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();
    var leafCount = artifact.ModuleCode.Split("function formatLeaf(value)").Length - 1;

    Assert.AreEqual(1, leafCount, artifact.ModuleCode);
    StringAssert.Contains(artifact.ModuleCode, "formatA(props.value)");
    StringAssert.Contains(artifact.ModuleCode, "formatB(props.value)");
}
```

- [ ] **Step 4: 先只运行这三条测试，确认它们当前是红的**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVue_Pipeline_LowersTwoLevelHelperCompositionIntoSetupScope|FullyQualifiedName~RazorVue_Pipeline_LowersTwoLevelHelperCompositionWithFieldAndPropsIntoSetupScope|FullyQualifiedName~RazorVue_Pipeline_LowersSharedInnerHelperOnlyOnceIntoSetupScope" /m:1
```

Expected: FAIL，并且失败点集中在 `RazorVue setup lowering does not support method ...` 或未生成 inner helper 函数。

- [ ] **Step 5: 提交只包含红测试的改动基线（如果仓库允许红测试中间提交则跳过；默认不提交）**

```bash
git status --short
```

Expected: 只出现 `src/Jazor.CompilerTest/RazorVuePipelineTests.cs` 修改。

---

### Task 2: 扩 emitter 深度登记，让两层 helper composition 变绿

**Files:**
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- Test: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

- [ ] **Step 1: 在 emitter 中新增 helper 深度状态与 owner helper 栈**

把 `RazorVueExpressionEmitter` 的字段区从：

```csharp
private readonly HashSet<IFieldSymbol> _requiredSetupFields;
private readonly HashSet<IMethodSymbol> _requiredSetupMethods;
```

改成：

```csharp
private readonly HashSet<IFieldSymbol> _requiredSetupFields;
private readonly HashSet<IMethodSymbol> _requiredSetupMethods;
private readonly Dictionary<IMethodSymbol, int> _helperDepthBySymbol;
private readonly Stack<IMethodSymbol> _currentSetupMethodStack;
```

并在构造函数里补初始化：

```csharp
_requiredSetupFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
_requiredSetupMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
_helperDepthBySymbol = new Dictionary<IMethodSymbol, int>(SymbolEqualityComparer.Default);
_currentSetupMethodStack = new Stack<IMethodSymbol>();
```

- [ ] **Step 2: 在 emitter 中新增统一 helper 登记方法，限制最大深度为 2**

在 `CreateUnsupportedSetupLogicException` 上方新增：

```csharp
internal void EnterSetupMethod(IMethodSymbol method)
    => _currentSetupMethodStack.Push(method);

internal void ExitSetupMethod(IMethodSymbol method)
{
    var current = _currentSetupMethodStack.Pop();
    if (!SymbolEqualityComparer.Default.Equals(current, method))
        throw new InvalidOperationException("RazorVue helper lowering stack is out of sync.");
}

private void RegisterRequiredSetupMethod(IMethodSymbol method)
{
    var depth = 1;
    if (_currentSetupMethodStack.Count > 0)
    {
        var ownerMethod = _currentSetupMethodStack.Peek();
        var ownerDepth = _helperDepthBySymbol.TryGetValue(ownerMethod, out var currentDepth)
            ? currentDepth
            : 1;
        depth = ownerDepth + 1;
        if (depth > 2)
        {
            throw CreateUnsupportedSetupLogicException(
                method,
                $"RazorVue setup lowering only supports helper composition up to two levels in component '{_snapshot.Descriptor.FullName}'. Helper '{ownerMethod.Name}' reaches helper '{method.Name}' beyond the supported composition depth.");
        }
    }

    if (_helperDepthBySymbol.TryGetValue(method, out var knownDepth))
        _helperDepthBySymbol[method] = Math.Min(knownDepth, depth);
    else
        _helperDepthBySymbol[method] = depth;

    _requiredSetupMethods.Add(method);
}
```

- [ ] **Step 3: 把 render/setup 两条 helper 调用入口都改成走统一登记方法**

把 `EmitInvocation()` 与 `EmitSetupInvocation()` 中这段：

```csharp
_requiredSetupMethods.Add(invocation.TargetMethod);
return ToLowerCamelCase(invocation.TargetMethod.Name) + "(" +
       string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
```

替换成：

```csharp
RegisterRequiredSetupMethod(invocation.TargetMethod);
return ToLowerCamelCase(invocation.TargetMethod.Name) + "(" +
       string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
```

并把 setup 版本同步替换成：

```csharp
RegisterRequiredSetupMethod(invocation.TargetMethod);
return ToLowerCamelCase(invocation.TargetMethod.Name) + "(" +
       string.Join(", ", invocation.Arguments.Select(argument => EmitSetupExpression(argument.Value))) + ")";
```

- [ ] **Step 4: 在 factory 生成 helper body 时 push/pop 当前 owner helper**

把 `BuildSetupMethodLowering()` 中：

```csharp
var expression = expressionEmitter.EmitSetupExpression(operation);
```

改成：

```csharp
expressionEmitter.EnterSetupMethod(method.MethodSymbol);
try
{
    var expression = expressionEmitter.EmitSetupExpression(operation);
    var methodBuilder = new StringBuilder();
    methodBuilder.Append("    function ")
        .Append(ToLowerCamelCase(method.Name))
        .Append('(')
        .Append(string.Join(", ", method.MethodSymbol.Parameters.Select(static parameter => parameter.Name)))
        .AppendLine(") {");
    methodBuilder.Append("      return ")
        .Append(expression)
        .AppendLine(";");
    methodBuilder.AppendLine("    }");
    return methodBuilder.ToString();
}
finally
{
    expressionEmitter.ExitSetupMethod(method.MethodSymbol);
}
```

- [ ] **Step 5: 运行成功路径测试，确认三条都变绿**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVue_Pipeline_LowersTwoLevelHelperCompositionIntoSetupScope|FullyQualifiedName~RazorVue_Pipeline_LowersTwoLevelHelperCompositionWithFieldAndPropsIntoSetupScope|FullyQualifiedName~RazorVue_Pipeline_LowersSharedInnerHelperOnlyOnceIntoSetupScope" /m:1
```

Expected: PASS 3/3。

- [ ] **Step 6: 提交两层 helper composition 成功路径**

```bash
git add src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs src/Jazor.CompilerTest/RazorVuePipelineTests.cs
git commit -m "$(cat <<'EOF'
✨ feat(razorvue): support two-level helper composition
EOF
)"
```

Expected: 生成一条只包含 lowering + pipeline tests 的提交。

---

### Task 3: 锁定失败路径，确保三层 helper 链显式走 JAZORVGA006

**Files:**
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- Modify: `src/Jazor.CompilerTest/ESGeneratorTests.cs`
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`（如需细化错误文案）

- [ ] **Step 1: 新增“三层 helper 链失败” pipeline 测试**

```csharp
[TestMethod]
public void RazorVue_Pipeline_ThrowsCompilationIssueForThreeLevelHelperComposition()
{
    var context = CreateContext(
        """
        using System;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

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
            [ECMAScript.ECMAScriptModule("./components/helper-card")]
            public class HelperCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private string FormatLeaf(int value)
                    => value.ToString();

                private string FormatMiddle(int value)
                    => FormatLeaf(value);

                private string FormatOuter(int value)
                    => FormatMiddle(value);

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, FormatOuter(Value));
                    builder.CloseElement();
                }
            }
        }
        """);

    var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
    Assert.AreEqual(RazorVueIssueCode.UnsupportedSetupLogicLowering, exception.Issue.Code);
    StringAssert.Contains(exception.Message, "two levels");
    StringAssert.Contains(exception.Message, "FormatLeaf");
}
```

- [ ] **Step 2: 新增“两层内但 inner helper 为 async 仍失败” pipeline 测试**

```csharp
[TestMethod]
public void RazorVue_Pipeline_ThrowsCompilationIssueForAsyncInnerHelperWithinTwoLevelComposition()
{
    var context = CreateContext(
        """
        using System;
        using System.Threading.Tasks;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

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
            [ECMAScript.ECMAScriptModule("./components/helper-card")]
            public class HelperCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private async Task<string> FormatLeafAsync(int value)
                    => await Task.FromResult(value.ToString());

                private Task<string> FormatOuterAsync(int value)
                    => FormatLeafAsync(value);

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, FormatOuterAsync(Value));
                    builder.CloseElement();
                }
            }
        }
        """);

    var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => new RazorVuePipeline().Execute(context));
    Assert.AreEqual(RazorVueIssueCode.UnsupportedSetupLogicLowering, exception.Issue.Code);
    StringAssert.Contains(exception.Message, "FormatLeafAsync");
}
```

- [ ] **Step 3: 新增 generator 级三层 helper 诊断投影测试**

```csharp
[TestMethod]
public void GenerateCatalog_WithThreeLevelHelperComposition_ReportsStructuredDiagnostic()
{
    var compilation = CreateCompilation(
        "RazorVue.HelperComposition.Generated",
        """
        using System;
        using Jazor.RazorVue;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

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
            [ECMAScript.ECMAScriptModule("./components/bad-helper")]
            public class BadHelper : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                public string FormatLeaf(int value)
                    => value.ToString();

                public string FormatMiddle(int value)
                    => FormatLeaf(value);

                public string FormatOuter(int value)
                    => FormatMiddle(value);

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, FormatOuter(Value));
                }
            }
        }
        """,
        MetadataReference.CreateFromFile(typeof(JazorComponent).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(VueComponent).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(JazorComponent).BaseType!.Assembly.Location));

    var (_, runResult) = RunAllGeneratorsWithResult(compilation);
    var diagnostics = runResult.Results
        .SelectMany(static result => result.Diagnostics)
        .Where(static diagnostic => diagnostic.Id == "JAZORVGA006")
        .ToArray();

    Assert.AreEqual(1, diagnostics.Length);
    StringAssert.Contains(diagnostics[0].GetMessage(), "FormatLeaf");
    StringAssert.Contains(diagnostics[0].GetMessage(), "two levels");
}
```

- [ ] **Step 4: 运行失败路径测试，确认都按 `JAZORVGA006` 通过**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForThreeLevelHelperComposition|FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForAsyncInnerHelperWithinTwoLevelComposition|FullyQualifiedName~GenerateCatalog_WithThreeLevelHelperComposition_ReportsStructuredDiagnostic" /m:1
```

Expected: PASS 3/3，并且异常/诊断都落在 `UnsupportedSetupLogicLowering` / `JAZORVGA006`。

- [ ] **Step 5: 提交失败路径与诊断文案收口**

```bash
git add src/Jazor.CompilerTest/RazorVuePipelineTests.cs src/Jazor.CompilerTest/ESGeneratorTests.cs src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs
git commit -m "$(cat <<'EOF'
✅ test(razorvue): cover helper composition boundaries
EOF
)"
```

Expected: 生成一条只包含边界测试/诊断文案的提交。

---

### Task 4: 同步文档并跑全量 RazorVue 回归

**Files:**
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`
- Test: `src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`

- [ ] **Step 1: 更新 Overview 的当前状态描述**

把：

```md
- the current logic lane is still a conservative subset, but it now includes lifecycle safe-subset lowering plus a minimal setup-side logic closure for simple fields and helper calls whose arguments can be lowered safely
- minimal setup-side logic lowering for simple instance fields and helper methods whose arguments can be projected safely into `setup()`
```

改成：

```md
- the current logic lane is still a conservative subset, but it now includes lifecycle safe-subset lowering plus a minimal setup-side logic closure for simple fields and helper composition up to two fixed levels
- minimal setup-side logic lowering for simple instance fields and helper methods whose arguments can be projected safely into `setup()`, including helper-to-helper composition up to two fixed levels
```

并在 unsupported 段保持 `JAZORVGA006` 仍是 setup-side logic lowering 的结构化错误面。

- [ ] **Step 2: 更新 ImplementationChecklist 的已证明 lowering 子集**

把：

```md
- minimal setup-side logic lowering for simple instance fields and helper methods whose arguments can be lowered safely
```

改成：

```md
- minimal setup-side logic lowering for simple instance fields and helper methods whose arguments can be lowered safely, including helper composition up to two fixed levels
```

并把 open item 继续保留为更广的 logic extraction，而不是宣称任意深度 helper 图已完成。

- [ ] **Step 3: 运行 RazorVue 全量回归**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "RazorVue" /m:1
```

Expected: PASS，所有 RazorVue 相关测试通过。

- [ ] **Step 4: 检查工作区，只保留本计划涉及的文件**

Run:
```bash
git status --short
```

Expected: 只出现：
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- `src/Jazor.CompilerTest/ESGeneratorTests.cs`
- `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`

- [ ] **Step 5: 提交文档同步与最终回归结果**

```bash
git add src/Jazor.Compiler/doc/RazorVue.Overview.md src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md
git commit -m "$(cat <<'EOF'
📝 docs(razorvue): sync helper composition status
EOF
)"
```

Expected: 生成文档同步提交；若前面两个提交已完成，这一提交只含文档更新。

---

## Self-Review Checklist

- Spec 第 2 节目标已覆盖：Task 1 + Task 2 负责两层 helper composition 成功路径。
- Spec 第 5/6 节边界已覆盖：Task 3 负责三层 helper 链显式失败与诊断文案。
- Spec 第 7 节测试策略已覆盖：Task 1 完成 3 个成功测试，Task 3 完成 2 个失败测试，并补 generator 投影验证。
- Spec 第 8 节文档同步已覆盖：Task 4 更新 `RazorVue.Overview.md` 与 `RazorVue.ImplementationChecklist.md`。
- 无 `TODO` / `TBD` / “similar to” 占位符。
- 所有命令都使用当前仓库实际路径：`D:/repository/own/jazor/Jazor/...`。

---

## Definition of Done

- `render -> helperA -> helperB` 在 `setup()` 中正确 materialize。
- helper body 内混合 field / props / helper 访问保持可用。
- 三层 helper 链显式抛 `RazorVueCompilationIssueException`，`Issue.Code == UnsupportedSetupLogicLowering`。
- generator 继续把该问题投影为 `JAZORVGA006`。
- `dotnet test ... --filter "RazorVue"` 全绿。

---

## Suggested Execution Order

1. Task 1
2. Task 2
3. Task 3
4. Task 4
