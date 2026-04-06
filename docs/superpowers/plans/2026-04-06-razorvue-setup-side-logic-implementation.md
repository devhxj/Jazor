# RazorVue Setup-side Logic Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 RazorVue 增加 setup-side logic 最小闭环，让保守字段与无参 helper 能进入 `setup()` 并被 render/lifecycle 复用。

**Architecture:** 沿用 `RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVueArtifactFactory` 现有主链路，不引入组件实例 runtime。通过扩展 logic semantic carrier、补一个 setup-side expression lowering lane、在 `setup()` 中输出局部声明与 helper，并对超界形态走结构化诊断。

**Tech Stack:** C# 14 / .NET 10、Roslyn、MSTest、Vue Composition API codegen

---

## File Map

- Modify: `src/Jazor.RazorVue/RazorVue/Descriptor/VueComponentDescriptor.cs`
  - 扩展 `VueLogicDescriptor`，纳入字段与带 symbol/body shape 的方法 carrier。
- Modify: `src/Jazor.RazorVue/RazorVue/RazorVueComponentCandidate.cs`
  - 增加 logic fields carrier。
- Modify: `src/Jazor.RazorVue/RazorVue/Discovery/RazorVueEntryClassifier.cs`
  - 新增发现最小安全字段的方法。
- Modify: `src/Jazor.RazorVue/RazorVue/RazorVueCompilationContext.cs`
  - 在 snapshot 构建时把 logic fields + richer methods 装配进 `VueLogicDescriptor`。
- Modify: `src/Jazor.RazorVue/RazorVue/Descriptor/RazorVueCompilationIssue.cs`
  - 新增 `UnsupportedSetupLogicLowering`。
- Modify: `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
  - 将 `UnsupportedSetupLogicLowering` 映射到新的结构化诊断。
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
  - 新增 setup-side expression emission，对组件字段 / 无参 helper 给出保守支持。
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
  - 在 `setup()` 中输出字段常量与 helper 函数，并把它们计入 logic hash。
- Modify: `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`
  - 新增 logic fields / richer helper carrier 测试。
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
  - 新增 setup-side logic 成功/失败测试。
- Modify: `src/Jazor.CompilerTest/ESGeneratorTests.cs`
  - 新增 generator 级 setup-side logic 诊断投影测试。
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.Design.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`

---

### Task 1: 先写失败测试，锁定 setup-side logic 最小闭环行为

**Files:**
- Modify: `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- Modify: `src/Jazor.CompilerTest/ESGeneratorTests.cs`

- [ ] **Step 1: 在 descriptor 测试中新增 logic field carrier 失败测试**

```csharp
[TestMethod]
public void RazorVue_Snapshot_ContainsSupportedLogicFieldsAndHelpers()
{
    var snapshot = CreateSingleSnapshot(
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
            [ECMAScript.ECMAScriptModule("./components/helper-card")]
            public class HelperCard : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private string TitleText = "Count: " + Value;

                public string FormatTitle()
                    => TitleText;
            }
        }
        """);

    Assert.AreEqual(1, snapshot.Logic.Fields.Length);
    Assert.AreEqual("TitleText", snapshot.Logic.Fields[0].Name);
    Assert.AreEqual("FormatTitle", snapshot.Logic.Methods.Single().Name);
}
```

- [ ] **Step 2: 在 pipeline 测试中新增 setup-side helper 成功测试**

```csharp
[TestMethod]
public void RazorVue_Pipeline_LowersSupportedSetupFieldAndHelperIntoSetupScope()
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

                private string TitleText = "Count: " + Value;

                public string FormatTitle()
                    => TitleText;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, FormatTitle());
                    builder.CloseElement();
                }
            }
        }
        """);

    var artifact = new RazorVuePipeline().Execute(context).Artifacts.Single();

    StringAssert.Contains(artifact.ModuleCode, "const titleText = (\"Count: \" + props.value);");
    StringAssert.Contains(artifact.ModuleCode, "function formatTitle()");
    StringAssert.Contains(artifact.ModuleCode, "return titleText;");
    StringAssert.Contains(artifact.ModuleCode, "return () => h(\"section\", null, formatTitle());");
}
```

- [ ] **Step 3: 在 generator 测试中新增 setup-side 超界诊断测试**

```csharp
[TestMethod]
public void GenerateCatalog_WithUnsupportedSetupLogicLowering_ReportsStructuredDiagnostic()
{
    var compilation = CreateCompilation(
        "RazorVue.SetupLogic.Tests",
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

                public string FormatTitle(int step)
                    => (Value + step).ToString();

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, FormatTitle(1));
                }
            }
        }
        """);

    var (_, runResult) = RunGeneratorWithResult(compilation);
    var diagnostics = runResult.Results
        .SelectMany(static result => result.Diagnostics)
        .Where(static diagnostic => diagnostic.Id == "JAZORVGA006")
        .ToArray();

    Assert.AreEqual(1, diagnostics.Length);
    StringAssert.Contains(diagnostics[0].GetMessage(), "FormatTitle");
}
```

- [ ] **Step 4: 运行三组测试，确认它们先失败**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/.worktrees/razorvue-setup-side-logic-20260406/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVue_Snapshot_ContainsSupportedLogicFieldsAndHelpers|FullyQualifiedName~RazorVue_Pipeline_LowersSupportedSetupFieldAndHelperIntoSetupScope|FullyQualifiedName~GenerateCatalog_WithUnsupportedSetupLogicLowering_ReportsStructuredDiagnostic" /m:1
```

Expected: FAIL，分别报缺少 `Logic.Fields`、未生成 `setup()` 局部声明、没有 `JAZORVGA006`。

---

### Task 2: 扩 semantic carrier 和诊断通道，最小通过 descriptor / generator 测试

**Files:**
- Modify: `src/Jazor.RazorVue/RazorVue/Descriptor/VueComponentDescriptor.cs`
- Modify: `src/Jazor.RazorVue/RazorVue/RazorVueComponentCandidate.cs`
- Modify: `src/Jazor.RazorVue/RazorVue/Discovery/RazorVueEntryClassifier.cs`
- Modify: `src/Jazor.RazorVue/RazorVue/RazorVueCompilationContext.cs`
- Modify: `src/Jazor.RazorVue/RazorVue/Descriptor/RazorVueCompilationIssue.cs`
- Modify: `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`

- [ ] **Step 1: 给 `VueLogicDescriptor` 增加 fields carrier**

```csharp
public sealed record VueLogicFieldDescriptor(
    string Name,
    bool IsReadOnly,
    IFieldSymbol FieldSymbol);

public sealed record VueLogicMethodDescriptor(
    string Name,
    int Arity,
    bool IsAsync,
    IMethodSymbol MethodSymbol);

public sealed record VueLogicDescriptor(
    ImmutableArray<VueLogicFieldDescriptor> Fields,
    ImmutableArray<VueLogicMethodDescriptor> Methods)
{
    public static VueLogicDescriptor Empty { get; } = new(
        ImmutableArray<VueLogicFieldDescriptor>.Empty,
        ImmutableArray<VueLogicMethodDescriptor>.Empty);
}
```

- [ ] **Step 2: candidate / classifier 补字段发现**

```csharp
public static ImmutableArray<IFieldSymbol> FindLogicFields(INamedTypeSymbol symbol)
{
    var builder = ImmutableArray.CreateBuilder<IFieldSymbol>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    for (var current = symbol; current is not null; current = current.BaseType)
    {
        foreach (var field in current.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic || field.AssociatedSymbol is not null)
                continue;
            if (!field.Locations.Any(static location => location.IsInSource))
                continue;
            if (!seen.Add(field.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)))
                continue;
            builder.Add(field);
        }
    }

    return builder.ToImmutable();
}
```

- [ ] **Step 3: 在 `RazorVueCompilationContext` 中装配 richer logic descriptor**

```csharp
var logicFields = candidate.LogicFields
    .Select(static field => new VueLogicFieldDescriptor(field.Name, field.IsReadOnly, field))
    .ToImmutableArray();
var logicMethods = candidate.LogicMethods
    .Select(static method => new VueLogicMethodDescriptor(method.Name, method.Parameters.Length, method.IsAsync, method))
    .ToImmutableArray();
var logic = logicFields.IsDefaultOrEmpty && logicMethods.IsDefaultOrEmpty
    ? VueLogicDescriptor.Empty
    : new VueLogicDescriptor(logicFields, logicMethods);
```

- [ ] **Step 4: 加新 issue code 并映射到 generator**

```csharp
public enum RazorVueIssueCode
{
    ComponentNotFound,
    AmbiguousComponentName,
    ReservedIntrinsicNameCollision,
    UnsupportedLifecycleLowering,
    UnsupportedSetupLogicLowering
}
```

```csharp
private static readonly DiagnosticDescriptor RazorVueUnsupportedSetupLogicLowering = new(
    id: "JAZORVGA006",
    title: "RazorVue setup-side logic lowering is unsupported",
    messageFormat: "{0}",
    category: "Jazor.RazorVue.Analysis",
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);
```

- [ ] **Step 5: 运行 descriptor / generator 相关测试，确认转绿**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/.worktrees/razorvue-setup-side-logic-20260406/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVueDescriptorExtractionTests|FullyQualifiedName~GenerateCatalog_WithUnsupportedSetupLogicLowering_ReportsStructuredDiagnostic" /m:1
```

Expected: 这两类新测试 PASS；pipeline helper 测试仍然 FAIL。

---

### Task 3: 实现 setup-side expression/lowering，让 pipeline helper 测试转绿

**Files:**
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- Modify: `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- Modify: `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

- [ ] **Step 1: 在 expression emitter 中新增 setup-side 发射入口**

```csharp
internal string EmitSetupExpression(IOperation operation)
{
    var current = Unwrap(operation);
    if (current is null)
        return "undefined";

    return current switch
    {
        ILiteralOperation literal => EmitLiteral(literal),
        IPropertyReferenceOperation property => EmitSetupPropertyReference(property),
        IFieldReferenceOperation field => EmitSetupFieldReference(field),
        IInvocationOperation invocation => EmitSetupInvocation(invocation),
        IBinaryOperation binary => "(" + EmitSetupExpression(binary.LeftOperand) + " " + GetBinaryOperator(binary.OperatorKind) + " " + EmitSetupExpression(binary.RightOperand) + ")",
        IUnaryOperation unary => GetUnaryOperator(unary.OperatorKind) + EmitSetupExpression(unary.Operand),
        IConditionalOperation conditional => "(" + EmitSetupExpression(conditional.Condition) + " ? " + EmitSetupExpression(conditional.WhenTrue!) + " : " + EmitSetupExpression(conditional.WhenFalse!) + ")",
        IInterpolatedStringOperation interpolated => EmitSetupInterpolatedString(interpolated),
        _ => throw new NotSupportedException($"RazorVue setup-side logic does not support expression '{current.Kind}' in component '{_snapshot.Descriptor.FullName}'.")
    };
}
```

- [ ] **Step 2: 在 artifact factory 中输出字段常量和 helper 函数**

```csharp
private static void AppendSetupLogicLowering(StringBuilder builder, RazorVueSemanticSnapshot snapshot, RazorVueExpressionEmitter emitter)
{
    foreach (var field in snapshot.Logic.Fields)
        builder.Append("    const ").Append(ToLowerCamelCase(field.Name)).Append(" = ").Append(EmitSupportedFieldInitializer(snapshot, emitter, field)).AppendLine(";");

    foreach (var method in snapshot.Logic.Methods.Where(static method => method.Arity == 0))
    {
        builder.Append("    function ").Append(ToLowerCamelCase(method.Name)).AppendLine("() {");
        builder.Append("      return ").Append(EmitSupportedMethodBody(snapshot, emitter, method)).AppendLine(";");
        builder.AppendLine("    }");
    }
}
```

- [ ] **Step 3: 把 setup-side lowering 接入 `BuildModuleCode`**

```csharp
builder.AppendLine("  setup(props, { emit, slots, expose, attrs }) {");
AppendLifecycleLowering(builder, snapshot);
AppendSetupLogicLowering(builder, snapshot, expressionEmitter);
builder.Append("    return () => ").Append(expressionEmitter.EmitFragment(renderTree)).AppendLine(";");
```

- [ ] **Step 4: 让 logic hash 包含字段 shape 和 helper body shape**

```csharp
foreach (var field in snapshot.Logic.Fields.OrderBy(static item => item.Name, StringComparer.Ordinal))
    logicShape.AppendLine("field:" + field.Name + "|" + field.IsReadOnly);
foreach (var method in snapshot.Logic.Methods.OrderBy(static item => item.Name, StringComparer.Ordinal))
    logicShape.AppendLine("logic:" + method.Name + "|" + method.Arity + "|" + method.IsAsync);
```

- [ ] **Step 5: 跑 pipeline 测试确认转绿**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/.worktrees/razorvue-setup-side-logic-20260406/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVue_Pipeline_LowersSupportedSetupFieldAndHelperIntoSetupScope|FullyQualifiedName~RazorVuePipelineTests" /m:1
```

Expected: 新 helper 测试 PASS，已有 RazorVue pipeline 测试不回归。

---

### Task 4: 同步文档并跑收口验证

**Files:**
- Modify: `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.Design.md`
- Modify: `src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md`

- [ ] **Step 1: 更新 Overview 的当前能力与未完成项**

```md
- minimal setup-side logic lowering for conservative fields and parameter-backed zero-arity helpers
- unsupported setup-side instance semantics continue to fail through structured diagnostics
```

- [ ] **Step 2: 更新 Design 的边界描述**

```md
RazorVue now supports a minimal setup-side logic lane for conservative field initializers and zero-arity helpers.
This is still not a full component-instance runtime bridge.
```

- [ ] **Step 3: 更新 Checklist 的 current progress/open items**

```md
- conservative setup-side logic lowering for fields and zero-arity helpers
- full component-instance semantics remains open
- parameterized methods / stateful instance writes remain out of phase-one scope
```

- [ ] **Step 4: 运行最终针对性测试**

Run:
```bash
dotnet test "D:/repository/own/jazor/Jazor/.worktrees/razorvue-setup-side-logic-20260406/src/Jazor.CompilerTest/Jazor.CompilerTest.csproj" --filter "FullyQualifiedName~RazorVueDescriptorExtractionTests|FullyQualifiedName~RazorVuePipelineTests|FullyQualifiedName~ESGeneratorTests" /m:1
```

Expected: PASS。
