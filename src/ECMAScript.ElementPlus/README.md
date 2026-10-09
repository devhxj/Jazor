# ECMAScript.ElementPlus

> 定位：`element-plus` 的强类型 C# host binding 与 Razor-to-Vue authoring 接口。

该包提供 Element Plus runtime 导入、组件代理和强类型 props/slot 契约。本文描述 `1.0.0-preview.8`；完整质量与发布消费者门禁见[当前状态](../../docs/04-roadmap/current-status.md)，具体交互/服务的验证范围见下文。

本包属于 JS resource library：`manifest.json` 与 `inventory.json` 记录锁定的 Element Plus
npm package、入口、样式和依赖元数据；Emit 生成标准 `jazor/package.json`，Deno 将运行时恢复
到 `jazor/node_modules`。C# 程序集只提供映射和 authoring contract。消费方编写的 RazorVue
组件仍按纯 Jazor 规则生成到消费程序集的 `Jazor.Generated.ModuleCatalog`。

组件使用独立的 `element-plus/<component>/<export>` 逻辑入口；Emit 只复制实际引用组件的
ESM 入口、共享 chunk 和组件样式。`element-plus` 根入口保留给完整插件安装、版本和指令 API，
因此单独使用 `ElButton` 不会携带完整组件库或全量 CSS。

公开的 `RouteLocationRaw` 参数由 `ECMAScript.VueRoute` 提供；NuGet 已声明该依赖并保留其
资源 locator 的传递性，应用无需手工复制 Vue Router 资源。

## 当前支持范围

- 根插件与 runtime host：`ElementPlus`。
- 管理壳常用组件：config provider、container layout、menu、button、card、link、space 与 divider。
- 遵循 Element Plus 命名的公开类型，例如 `ElButtonType`、`ElUploadFile` 与 `ElComponents`；根 host 保留 `ElementPlus` 名称。

## 泛型交互

下列 API 保留作者声明的值、行和 command 类型，仍导入对应的 Element Plus 组件及样式：

| C# 组件 | typed 契约 |
| --- | --- |
| `ElTypedSelect<TValue>` / `ElTypedOption<TValue>` | `ModelValue`、`ModelValueChanged`、`OnChange` 与 option `Value` 保留 `TValue`；可搭配 `Filterable`、`AllowCreate`、`DefaultFirstOption`、`Clearable`。 |
| `ElTypedTable<TRow>` | `Data` 为 `TRow[]`；`OnSelectionChange` 接收 `TRow[]`，`OnRowClick` 接收 `TRow`，`OnSortChange` 接收 `ElTableSort`。 |
| `ElTypedTableColumn<TRow>` | `Selectable`、`SortMethod`、`Formatter`、`FilterMethod` 保留行类型；默认 slot 接收 `ElTableSlotContext<TRow>`。 |
| `ElTypedDropdown<TCommand>` / `ElTypedDropdownItem<TCommand>` | item `Command` 与 dropdown `OnCommand` 使用同一 `TCommand`。 |

上述 typed slice 不改变其他 callback 的既有契约；例如 table 的 `RowKey`、`SummaryMethod`、`SpanMethod`、`Load` 与 `RowExpandable` 仍使用原有类型。

以下 Razor 标记假设组件在 `.razor.cs` 声明了 `string? Id`、`OrderRow[] Rows` 和对应 handler。行模型使用 host record，`Id` / `Allowed` 映射为上游对象的 `id` / `allowed`：

```csharp
[ECMAScript]
public sealed record OrderRow : VueProps
{
    public string Id { get; init; } = "";
    public bool Allowed { get; init; }
}
```

```razor
@using ECMAScript.ElementPlus

<ElTypedSelect TValue="string" @bind-ModelValue="Id"
               Filterable="true" AllowCreate="true"
               DefaultFirstOption="true" Clearable="true" OnChange="@Changed">
    <ElTypedOption TValue="string" Value="@("9007199254740993")" Label="Existing" />
</ElTypedSelect>

<ElTypedTable TRow="OrderRow" Data="Rows" OnRowClick="@RowClicked"
              OnSelectionChange="@SelectionChanged" OnSortChange="@SortChanged">
    <ElTypedTableColumn TRow="OrderRow" Type="selection" Selectable="@CanSelect" />
    <ElTypedTableColumn TRow="OrderRow" Prop="id" Label="ID" Context="cell">
        <span data-index="@cell.Index">@cell.Row.Id</span>
    </ElTypedTableColumn>
</ElTypedTable>

<ElTypedDropdown TCommand="string" OnCommand="@CommandChanged">
    <ChildContent>Export</ChildContent>
    <Dropdown>
        <ElDropdownMenu>
            <ElTypedDropdownItem TCommand="string" Command="@("selected")">Selected</ElTypedDropdownItem>
        </ElDropdownMenu>
    </Dropdown>
</ElTypedDropdown>
```

对应签名为 `Changed(string? value)`、`RowClicked(OrderRow row)`、`SelectionChanged(OrderRow[] rows)`、`SortChanged(ElTableSort sort)`、`CanSelect(OrderRow row, ECMAScript.Number index)` 和 `CommandChanged(string command)`。IDs 可以保留原始十进制字符串，避免超过 JavaScript safe integer 范围时转成 number 丢失精度；BigInt JSON 示例见 [Browser interop](../../docs/03-guides/browser-interop.md)。

## 迁移：TableColumn scoped slot

**`ElTableColumn.ChildContent` 已从无上下文 `RenderFragment` 改为 `RenderFragment<ElTableSlotContext>`。** 这是公共 API 变更，直接赋值或转发该参数的 C# fragment 必须同步调整签名：

```csharp
// 旧签名：RenderFragment CellContent
private RenderFragment<ElTableSlotContext> CellContent => cell => builder =>
    builder.AddContent(0, cell.Index);
```

`ElTableColumn` 使用字典行上下文：`cell.Row` 为 `VueDictionary`。`ElTypedTableColumn<TRow>` 使用 `RenderFragment<ElTableSlotContext<TRow>>`，`cell.Row` 保留具体 `TRow`；两者的 `cell.Column` 均为可空 `ElTableColumnContext`，`cell.Index` 均为 `Number`，映射上游从 0 开始的 `$index`。

纯标记 child content 可保留。需要单元格数据时，按照上面的 typed table 示例声明 `Context="cell"`，或在显式 `<ChildContent Context="cell">` 中读取 `Row`、`Column`、`Index`。使用业务行模型时将 `ElTable` / `ElTableColumn` 换成同一 `TRow` 的 typed 组件，避免把行数据再降为字典。

## 反馈服务

服务入口分别为 `ElMessage.Service`、`ElMessageBox.Service` 和 `ElNotification.Service`，options 与返回值均有明确类型：

```csharp
var toast = ElMessage.Service.Success(new ElMessageOptions
{
    Message = "Saved",
    Grouping = true
});
toast.Close();

var notification = ElNotification.Service.Info(new ElNotificationOptions
{
    Title = "Export",
    Message = "Ready"
});
notification.Close();

ElMessageBox.Service.Confirm("Delete?", "Confirm", new ElMessageBoxOptions
{
    Type = ElFeedbackType.Warning
}).Then(
    action => { Confirmed = action == ElMessageBoxAction.Confirm; },
    () => { Confirmed = false; });

ElMessageBox.Service.Prompt("Name?").Then(
    result => { PromptText = result.Value; },
    () => { PromptText = null; });
```

示例中的 `Confirmed` / `PromptText` 是组件 state。message / notification 的 `Info`、`Success`、`Warning`、`Error` 接受文本或相应 options，返回可 `Close()` 的 handle，并提供 `CloseAll()`。message box 的 `Alert`、`Confirm` 返回 `IPromise<ElMessageBoxAction>`，`Prompt` 返回 `IPromise<ElMessageBoxPromptResult>`，其 `Value` 是输入文本；取消或关闭沿 Promise rejection 传播，需在 `Then` rejection callback 或 `Catch` 中处理。`ElMessageBox.Service.Close()` 关闭当前对话框。

这些服务入口使用 Element Plus 根 ESM named export，并声明各自样式依赖。消费方继续由 Emit 恢复 package/style 依赖，无需复制运行时文件。当前 typed interaction 回归使用真实 Element Plus + Vue + happy-dom 验证创建字符串选项、禁用行选择、row-click、selection-change 与 scoped cell slot；command/sort/service 成功路径使用 stub，服务取消路径与真实 CSS/browser smoke 尚不由该回归证明。

## 作者约束

组件使用完整的 `ComponentBase + IVueComponent` 身份，生成组件声明 `[ECMAScriptModule]`，外部 wrapper 声明 `[ECMAScript]`；缺失身份时 `JAZORVGA027` 给出修复动作。CSS class 优先写 `CssClass="panel"`，或使用精确的小写 `class`。`Class="panel"` 与 `CssClass` 的 runtime `class` 映射冲突时报告 `JAZORVGA028`，位置指向 `.razor` 属性。

外部 wrapper 的源码 `[Parameter]` 默认 initializer 必须是 compile-time constant；常量默认 props 可以从基类继承，并由调用处显式参数或 `@attributes` 覆盖。`GetHeight()` 等动态默认值应移到调用方 state/attribute，或生成组件 lifecycle；非恒定 wrapper initializer 报告 `JAZORVGA021`。完整规则见 [RazorVue 作者指南](../../docs/03-guides/razorvue-authoring.md#external-wrapper-defaults)。

## 边界

Razor Source Generator 集成、render-function lowering 与产物物化分别属于 `Jazor.Vue`、`Jazor.RazorVue` 与 `Jazor.Emit`。本包只定义 host binding 和组件契约。

## 如何阅读 API 注释

生成组件会保留 Element Plus `web-types.json` 中的组件、prop、slot 和 event 原文说明；在 IDE 中悬停
`ElButton.Type`、`ElDialog.Footer` 或 `ElInput.OnChange` 即可查看官方语义。C# 名称按 PascalCase 映射，
`[ECMAScriptName]` 中保留 Vue 运行时名称；`ModelValue`/`ModelValueChanged` 对应 Vue 的
`modelValue`/`update:modelValue`，命名 slot 使用 `RenderFragment` 参数。

## 相关文档

- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
- [RazorVue 作者指南](../../docs/03-guides/razorvue-authoring.md)
- [RazorVue 诊断矩阵](../../docs/03-guides/razorvue-diagnostic-matrix.md)
- [Browser interop](../../docs/03-guides/browser-interop.md)
