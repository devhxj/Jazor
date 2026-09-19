# ECMAScript.TDesign

> 定位：TDesign Vue Next 的强类型 C# host binding 与 Razor-to-Vue authoring 接口。

本包属于 JS resource library。发布包的 `manifest.json` 与 `inventory.json` 记录锁定的
`tdesign-vue-next` 1.20.7 npm package、入口、样式和依赖元数据；Emit 生成标准
`jazor/package.json`，Deno 将 TDesign 与 Vue 恢复到 `jazor/node_modules`。C# 程序集只提供
映射和 authoring contract。消费方编写的 RazorVue 组件生成到消费程序集的
`Jazor.Generated.ModuleCatalog`。

## 维护输入

绑定输入固定在 `../ECMAScript.Vue.Generator/upstream/tdesign-vue-next/1.20.7`。`components.json`、`bindings.json` 和 `contracts.json` 分别记录可导出的组件、实际模块/export 与强类型 props 契约。没有当前 runtime export 的文档标签不是 binding 输入。

`documentation.json` 保存相同上游版本源码中的中文 JSDoc，来源 commit 为
`018b90352b184fb93f57dca62db83d80d486a14f`（MIT 许可证）。注释按源文件、类型和成员匹配，
补充默认值、配置对象成员和回调说明；枚举逐项释义由生成器的中文词表维护。

## 维护命令

以下命令只供包维护者更新锁定上游快照和验证生成结果；应用构建与发布不会执行它们：

```bash
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign documentation .tmp/tdesign-docs.tar.gz
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --report
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
```

重新生成文档输入前，将[锁定的上游源码归档](https://codeload.github.com/Tencent/tdesign-vue-next/tar.gz/018b90352b184fb93f57dca62db83d80d486a14f)
保存为 `.tmp/tdesign-docs.tar.gz`。`tdesign snapshot` 会重建快照目录，因此完整刷新时应按上述顺序重建文档；
日常只修改枚举释义时，直接运行 `tdesign components` 即可，不需要下载上游源码。

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
Release NuGet consumer 和真实 Chrome browser smoke。它证明的是 TDesign typed authoring 这一条
能力，不会扩大 Microsoft/Blazor 内置 UI、`IJSRuntime` 或 server-only service 的明确 Reject 边界。

复杂表格列继续使用 C# 侧的强类型 `TPrimaryTableColCell<T>` render fragment；fragment 参数是
`TPrimaryTableCellParams<T>`，因此单元格中的行数据保持 C# 成员访问和 RazorVue 的正常 lowering。
行交互使用 `EventCallback<TRowEventContext<T>>`，事件处理器可以直接读取 `context.Row`，无需
应用侧类型转换或通用 JavaScript 桥接。

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

NuGet 包随程序集提供 `ECMAScript.TDesign.xml`，安装包后 IDE 可显示组件、属性、事件、插槽、
配置对象、委托和公共方法的说明。组件介绍来自锁定的 `web-types.json`，成员优先使用同版本源码的中文 JSDoc。
上游没有说明的字段补充绑定侧说明；枚举成员同时注明中文含义和实际 JavaScript 字符串。

例如悬停 `TButtonThemeValue.Primary` 可见“品牌色主题”，`TButtonShapeValue.Round` 可见“圆角长方形按钮”，
`TScroll.BufferSize` 可见预渲染行数的用途和上游默认值。C# 属性名仍按 PascalCase/`XxxValue`/`XxxContent`
规则映射到 Vue 原名，例如按钮主题属性是 `TButton.Theme`，包含插槽分支的标签属性是 `TFormItem.LabelValue`。

- [ECMAScript.Vue.Generator](../ECMAScript.Vue.Generator/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
