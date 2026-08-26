# ECMAScriptAttribute 统一协议调整计划

> 状态：已实施。统一协议、官方绑定迁移与旧组件 Attribute 删除均已完成。

## 目标

将 `LibraryComponentAttribute`、`VueLibraryComponentAttribute` 与现有
`ECMAScriptAttribute` 收敛为一个外部 ECMAScript 绑定协议。统一后的 Attribute
只描述两件事：外部 ESM `Import` specifier，以及该声明在 Jazor 中的用途
`Transform`。RazorVue 组件入口资格由 `ComponentBase + IVueComponent + 组件导入描述`
共同决定；Attribute 单独不能赋予组件 marker 身份。

`ECMAScriptModuleAttribute` 不在本次合并范围内。它描述 Jazor 生成模块的输出，
与外部 ESM import 方向相反，继续保持独立协议。

## 目标协议

### 声明形状

```csharp
[ECMAScript]
public record DateRange;

[ECMAScript("vue")]
public static partial class Vue;

[ECMAScript("element-plus", Transform.Component, "ElButton")]
public sealed class ElButton : ComponentBase, IVueComponent;
```

当前 `Transform` 只包含三个值，并为后续扩展保留枚举边界：

```csharp
public enum Transform
{
    Allow,
    Import,
    Component
}
```

### 构造函数语义

| C# 写法 | `Import` | `Transform` | 语义 |
| --- | --- | --- | --- |
| `[ECMAScript]` | `null` | `Allow` | 允许作为 ECMAScript 宿主契约，不引入模块 |
| `[ECMAScript("vue")]` | `"vue"` | `Import` | 普通外部 ESM binding |
| `[ECMAScript("element-plus", Transform.Component, "ElButton")]` | `"element-plus"` | `Component` | 外部组件 named ESM binding |
| `[ECMAScript("element-plus", Transform.Component)]` | `"element-plus"` | `Component` | 外部组件 default ESM binding |

目标 Attribute 提供四个作者面构造函数：

```csharp
ECMAScriptAttribute();
ECMAScriptAttribute(string import);
ECMAScriptAttribute(string import, Transform transform);
ECMAScriptAttribute(string import, Transform transform, string? exportName);
```

单字符串重载保留现有 `[ECMAScript("...")]` 行为，并将其明确解释为
`Transform.Import`。双参数重载用于显式指定 `Transform`；三参数重载补充组件
export 名称。三参数重载的 `exportName` 可为空，表示 default import。

约束如下：

- `Allow` 只表示无 import 的声明；
- `Import` 与 `Component` 必须提供非空 `Import`；
- `ExportName` 只对 `Component` 有效；`Component` 未提供时表示 default import；
- `Import` 不接受第三个 `exportName` 参数，普通类型/成员名称继续由
  `ECMAScriptNameAttribute` 或现有名称规则决定；
- `Import` 按 ESM 原文保留：支持 bare/package、相对、root-relative 与 URL specifier；
- 不接受磁盘绝对路径；不补 `.mjs`、不改写 `.js`、不拒绝合法的 `..` 相对 specifier；
- `ECMAScriptNameAttribute` 继续负责普通声明和成员的运行时名称，不与组件
  `ExportName` 参数形成两个事实源。

`Component` 只提供组件导入描述。RazorVue 仍必须验证组件继承
`ComponentBase`、实现 `IVueComponent`（包括派生或泛型 marker），并在最终
RenderTree lowering 时直接生成 Vue 组件 import。

## 程序集与迁移边界

canonical `ECMAScriptAttribute` 与 `Transform` 应下沉到 `ECMAScript.Contract`，
但保持 `ECMAScript` 命名空间。这样 `ECMAScript.VueContract` 等
`netstandard2.0` 消费者不需要依赖 `net11.0` 程序集。

旧的 `LibraryComponentAttribute` 与 `VueLibraryComponentAttribute` 已直接删除，不保留
obsolete 兼容层或双读分支。旧源码必须迁移为统一写法：

```csharp
[VueLibraryComponent("element-plus", "ElButton")]
// 改为
[ECMAScript("element-plus", Transform.Component, "ElButton")]
```

## 分阶段实施

### P0：协议冻结

- 在 `ECMAScript.Contract` 定义 `Transform` 和 canonical `ECMAScriptAttribute`。
- 明确构造函数、枚举值、路径保留规则和非法组合诊断。
- 保留 `ECMAScriptModuleAttribute` 的独立职责。
- 为后续新增 `Transform` 值保留未知值的明确失败路径，不做静默降级。

### P1：共享识别与编译器读取

- 将共享读取器收敛为 `Jazor.Common.ECMAScriptComponentMetadata`，只读取统一 Attribute。
- 更新 `Jazor.Compiler.Util`、`SemanticWalker` 和 `Jazor.Analyzer`：
  - `Allow` 保持当前宿主标记语义；
  - `Import` 保持当前静态 ESM import 语义；
  - `Component` 不得被当作普通 CLR runtime member dispatch；
  - 未知 `Transform` 值在实际使用点明确失败。
- 为外部 ESM 增加独立 specifier 校验/保留 helper，不能复用生成模块路径的
  `.mjs` 补全和输出目录逃逸规则。

### P2：RazorVue 组件 lowering

- `RenderEmitter.ResolveComponentImport` 改为读取
  `[ECMAScript(import, Transform.Component, exportName)]`；缺少 `exportName` 时
  选择 default import。
- `ComponentSelector`、组件绑定诊断和 README 改用统一协议名称。
- 保留 `[ECMAScriptModule]` 优先规则，用于应用/本地生成组件。
- 不提供旧组件 Attribute fallback；包版本不匹配应在 C# 编译期明确失败。

### P3：绑定生成器与官方组件库迁移

- 更新 TDesign、Vuetify、Element Plus、Vue Data UI、Vu Icons 等生成器模板。
- 批量把生成结果从旧组件 Attribute 改为统一 Attribute 的三参数 Component 重载。
- 更新 package consumer、metadata reference 和反射测试，不再以旧 Attribute 作为
  正常成功证据。
- 清理 `LibraryComponentMetadata` 的旧命名和只服务 Vue 的专用发现常量。

### P4：回归门禁与文档收敛

- 增加 Attribute 反射/元数据测试：四种构造函数、`Import`、`Transform`、
  `ExportName` 和非法组合。
- 增加 compiler emission 测试：Allow 无 import、普通 Import、Component import，
  以及 named/default export、import alias 稳定性。
- 增加 specifier 测试：bare、相对、root-relative、URL 和磁盘绝对路径拒绝。
- 增加 Razor SG 测试：`ComponentBase + IVueComponent` 组件、派生 marker、统一协议
  default/named export 和错误诊断。
- 运行 compiler、Razor SG、Emit、CLR/绑定 consumer 全套门禁，并执行
  `git diff --check`。
- 更新架构、作者指南、组件库 README 和发布说明，明确 JS interop 不属于该协议。

### P5：旧协议删除

- 删除两个旧 Attribute、旧 metadata helper、旧生成模板和旧测试夹具。
- consumer 只引用 canonical `ECMAScriptAttribute`；不保留 obsolete 类型或双读路径。
- 将最终协议与已验证边界收敛回 `current-status.md` 和架构文档；历史迁移细节只保留
  在 `docs/05-history/evolution.md`。

## 验收标准

调整完成必须同时满足：

1. 官方绑定、生成器和 consumer 只使用统一协议；旧组件 Attribute 不再由任何程序集导出。
2. `[ECMAScript]` 不产生 import；`[ECMAScript("vue")]` 产生稳定的 `vue` import；
   `Transform.Component` 产生组件 import，且 export 名来自 `ExportName`，为空时
   使用 default import。
3. 合法相对/绝对形式的 ESM specifier 原样保留，磁盘绝对路径明确失败。
4. 组件必须同时满足 `ComponentBase + IVueComponent + 组件导入描述`；第三项只能是
   `[ECMAScriptModule]` 或 `[ECMAScript(import, Transform.Component[, exportName])]`，
   `Allow`/普通 `Import` 不能绕过组件入口约束。
5. 编译器、Analyzer、RazorVue 和 Emit 对未知类别、缺少 import、非法 export 参数
   的诊断位置和行为确定，不产生静默 raw-JS fallback。
6. Blazor JS interop 仍保持现有 Reject 边界，本协议不引入动态 JS runtime dispatcher。

## 当前实施证据

- canonical `ECMAScriptAttribute` 与 `Transform` 已位于 `ECMAScript.Contract`，保留 `ECMAScript` 命名空间；四个构造函数和组件 default/named export 语义已覆盖。
- Element Plus、TDesign、Vuetify、Vue Data UI、Vu Icons 等官方组件声明与生成器已迁移到 `Transform.Component`；旧 Attribute、双读 helper 和兼容测试夹具均已删除。
- `Jazor.Common`、`Jazor.Compiler`、`Jazor.Analyzer` 与 RazorVue 已区分普通 runtime marker、组件 marker 和外部 ESM specifier；外部 specifier 不做 `.mjs` 补全或生成目录规范化，并拒绝盘符/UNC 文件路径。
- Razor SG 组件入口仍强制 `ComponentBase + IVueComponent + 组件导入描述`，`ECMAScriptModule` 优先于外部组件 binding；JS interop 不在该协议内。
- 已验证完整 Compiler、Razor SG、Emit 与受影响绑定 consumer 回归；发布前仍按仓库 tag 门禁执行全量质量检查。

## 风险与处理

| 风险 | 处理 |
| --- | --- |
| canonical Attribute 位于 `net11.0`，VueContract 无法引用 | 下沉到 `ECMAScript.Contract`，保持 `ECMAScript` 命名空间 |
| 外部 import 被错误当作生成模块路径 | 使用独立 ESM specifier helper，不调用 `NormalizeImportSpecifier` |
| Component 被普通 compiler runtime marker 误判 | 在 compiler/analyzer 中显式读取 `Transform`，Component 只走组件 binding 入口 |
| 包版本仍引用旧 Attribute | 不提供 fallback，由 C# 编译期明确失败并要求包 lockstep 升级 |
| 未来新增 Transform 值造成旧编译器误判 | 未知值明确诊断并停止相关 lowering，不按 Allow 处理 |
