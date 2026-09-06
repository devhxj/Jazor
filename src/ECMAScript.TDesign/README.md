# ECMAScript.TDesign

> 定位：TDesign Vue Next 的强类型 C# host binding 与 Razor-to-Vue authoring 接口。

本包属于 JS resource library。发布包携带 `tdesign-vue-next` 1.20.5 的
`manifest.json + dist/**` 浏览器 ESM 和 CSS，许可证等附属文件由 manifest 显式声明；C# 程序
集只提供映射和 authoring contract。应用只需还原 NuGet 包；Jazor 会从本地包资源按 manifest
依赖物化 TDesign 与 Vue，不要求 `node_modules`、CDN 或额外的 Node.js 安装。消费方编写的
RazorVue 组件生成到消费程序集的 `Jazor.Generated.ModuleCatalog`。

## 维护输入

绑定输入固定在 `../ECMAScript.Vue.Generator/upstream/tdesign-vue-next/1.20.5`。`components.json`、`bindings.json` 和 `contracts.json` 分别记录可导出的组件、实际模块/export 与强类型 props 契约。没有当前 runtime export 的文档标签不是 binding 输入。

## 维护命令

以下命令只供包维护者更新锁定上游快照和验证生成结果；应用构建与发布不会执行它们：

```bash
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --report
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
```

`tdesign components` 是全覆盖门禁：只有每个已声明 props 都具备具体 C# 类型时才生成当前 118 个 runtime 组件。不能为了通过生成而使用 `object`、`VueValue` 或占位契约。

## 类型与产物边界

公开 authoring 类型使用 `T*` 命名，例如 `TMenuValue`、`TButtonThemeValue` 与 `TComponents`；根 host 保留 `TDesign`。字符串域使用 `[String]` enum，因此 `TButtonThemeValue.Primary` 会发射为 `"primary"`，不会变成数值序号。

带类型参数的组件（例如 `TInput<T>`、`TForm<T>`、`TTable<T>` 和
`TPrimaryTable<T>`）只公开泛型组件本身。生成器仍保留同名的默认闭式别名供程序集内部元数据使用，
但该别名是 `internal`，不会进入消费方 official Razor Source Generator 的组件发现范围；否则同名泛型
与非泛型组件会触发 Razor SDK 的歧义诊断。Razor 页面直接写显式类型参数（例如
`<TInput T="string" ... />`），C# 代码使用 `TInput<string>`，无需桥接组件或 `object` 转换。

## 已验证的 Razor 写法

以下语法已通过作者源码、official Razor Source Generator、render module、Deno runtime、隔离
Release NuGet consumer 和真实 Edge browser smoke。它证明的是 TDesign typed authoring 这一条
能力，不会扩大 Microsoft/Blazor 内置 UI、`IJSRuntime` 或 server-only service 的明确 Reject 边界。

```razor
<TForm FormData="EditorModel" Data="@FormData" OnSubmit="@Submit">
    <TFormItem LabelValue="Name" Name="name">
        <TInput T="string" @bind-Value="Name" @bind-Value:event="OnChange" />
    </TFormItem>
</TForm>

<TRadioGroup T="string" @bind-Value="Stage" @bind-Value:event="OnChange">
    <TRadioButton T="string" Value="@("draft")">Draft</TRadioButton>
</TRadioGroup>
```

`TForm` 使用上游实际的泛型参数名 `FormData`，而不是统一的 `T`。当泛型参数的值是静态字符串时，
使用带类型上下文的 Razor 表达式，例如 `Value="@("draft")"`；不需要 cast 或 `From(...)`。

同一个 Vue prop 同时有值分支和 slot 分支时，binding 以稳定后缀区分，二者不能在同一组件实例上
同时设置：

```razor
<TDialog ConfirmBtnValue="@("Publish")" OnConfirm="@Confirm">
    <BodyContent>Review release</BodyContent>
</TDialog>

<TDialog ConfirmBtnValue="@ConfirmButton" />

<TDialog>
    <ConfirmBtnContent>Publish</ConfirmBtnContent>
</TDialog>

<TTable T="Row" Data="@Rows" RowKey="Id" LoadingValue="@IsLoading">
    <LoadingContent>Loading</LoadingContent>
    <EmptyContent>No rows</EmptyContent>
</TTable>
```

常见命名为 `XxxValue`（JS prop 的非 fragment 分支）和 `XxxContent`（同一 prop 的 named slot）。
`TTable<T>` 与 `TPrimaryTable<T>` 的 `RowKey` 是 `[EditorRequired]`；省略它会由 official Razor
Source Generator 报告 `RZ2012`，不会等到 RazorVue runtime 才失败。

Razor Source Generator 集成、render-function lowering 和产物物化分别属于 `Jazor.Vue`、`Jazor.RazorVue` 和 `Jazor.Emit`。本包只定义 host binding 与组件契约。

## 相关文档

组件源码中的 XML 注释来自锁定版本的 TDesign `web-types.json`，包含组件、prop、事件和 slot 的中英文
原文。IDE 悬停 `TButton.Theme`、`TForm.OnSubmit` 或 `TTable.LoadingContent` 可直接查看上游说明；
C# 属性名仍按 PascalCase/`XxxValue`/`XxxContent` 规则映射到 Vue 原名。

- [ECMAScript.Vue.Generator](../ECMAScript.Vue.Generator/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
