# 用 Jolt 编写 Vue 组件

Jolt 是用 C# 编写 Vue 3 单文件组件的工具。一份 `.jazor` 文件对应一个 ES 模块，默认导出 Vue 组件。HTML 和组件标签直接写在顶层。`@code` 的实例成员由每个组件实例独立持有，静态方法和字段进入 Jazor 静态模块类。`public static` 方法和字段以及公开常量按 Jazor 的模块规则生成具名导出。Jolt 与现有 RazorVue 分别加载自己的项目，两者复用 Jazor 的 C# 编译器。

## 运行示例

预览版通过 NuGet 分发 CLI。VS Code 扩展 `jolt-2.1.0-preview.4.vsix` 通过 GitHub 预发布附件分发，在 VS Code 执行 **Extensions: Install from VSIX** 安装。Visual Studio 2026 使用单独构建的 `Jolt.VisualStudio.vsix`。 [Jolt 预览版](https://github.com/devhxj/Jazor/releases/tag/v2.1.0-preview.4) · [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md)

```sh
dotnet tool install --global Jazor.Jolt --version 2.1.0-preview.4
```

在仓库根目录用 `global.json` 指定的 SDK 执行以下命令，打开终端打印的网址。首次运行会还原 NuGet 和 npm 依赖。

```sh
dotnet run --project src/Jolt.Cli -- dev samples/Jolt.Counter
```

点击按钮后，子组件通过类型为 `Action<int>` 的参数调用父组件方法。父组件的计数变化触发 Vue 更新，同时控制 Reka UI 组件的打开状态。 [父组件](../../samples/Jolt.Counter/App.jazor) · [子组件](../../samples/Jolt.Counter/CounterButton.jazor)

| 命令 | 结果 |
| --- | --- |
| `jolt build <project>` | 在项目的 `.jolt` 目录写入组件模块、作者源码映射和模块清单。可用 `--output <目录>` 指定产物位置。 |
| `jolt dev <project>` | 启动 Vite，监听作者文件并重新编译。可用 `--port <端口>` 选择监听端口。 |
| `jolt bundle <project>` | 用项目的 Vite 配置构建生产资源。 |
| `jolt lsp` | 通过标准输入输出提供语言服务。 |

从源码使用时，将表中的 `jolt` 替换为 `dotnet run --project src/Jolt.Cli --`。如需验证安装后的 CLI，运行 `dotnet run --file scripts/csharp/verify-jolt-package.cs`。该命令本地打包并在仓库外安装、构建和打包示例。[隔离消费者验收](../../scripts/csharp/verify-jolt-package.cs)

## 直接使用 npm 组件

项目用 `package.json` 和 `pnpm-lock.yaml` 声明 Vue 及组件包。以下组件通过 `@using Npm.RekaUi` 让该包导出的名字进入当前文档。构建时从包的公开声明检查 `Open`，运行时从 `reka-ui` 导入 `CollapsibleRoot`。类型检查器由 Jolt 提供，消费者无需安装组件专用 C# 类库。

```razor
@using Npm.RekaUi

<button @onclick="@Toggle">切换</button>
<CollapsibleRoot Open="@open" />

@code {
    private bool open;
    private void Toggle() => open = !open;
}
```

命名约定区分包、子路径和导出。例如 `@scope/ui-kit/button` 的显式公开子路径对应 `Npm.Scope.UiKit.Button`。具名导出沿用导出名，默认导出用路径末段的 PascalCase 名称。编译和编辑器读取已还原包的声明；声明变化后重新解析。此入口消费发布后的 ESM 和 `.d.ts`。直接编译本地 `.vue` 源文件不在 Jolt 范围内。 [npm 元数据与范围](../../src/Jolt.Npm/README.md)

## 组件与编辑器

组件用 `[Parameter]` 声明参数和委托，用 `[EditorRequired]` 标记调用者必须提供的参数。默认插槽使用 `<slot />`，具名插槽使用 `<slot name="header" />`，调用方用 `<template #header>…</template>` 填充内容。`<template>` 在这里是具名插槽写法，文件顶层可以直接书写标记。C# 区域支持 `@name`、`@(expression)`、`@if` 和 `@foreach`。

编辑器用相同的项目加载器、组件声明和 C# 投影提供补全、悬停、签名帮助、定义导航和诊断。键入标签名时，可以选择同项目其他命名空间的本地组件，接受建议后会插入相应的 `@using`，随后可以补全该组件的类型化参数。npm 定义可跳转至声明文件。 [VS Code 扩展](../../editors/jolt/README.md) · [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md)

源码候选增加了查找引用、改名、格式化和快捷修复。跨文件改名按 Roslyn 符号和组件绑定更新 C#、参数、委托和具名插槽。支持文件操作的编辑器还可以从标签改名组件及其同目录文件。格式化调整 C# 和标签内的属性缩进，保留渲染文本与换行风格。未知的本地组件可以用快捷修复添加 `@using`，缺少必填参数时可以匹配当前组件的同名同类型字段或可读属性。 [验收记录](../04-roadmap/jolt-editor-implementation-plan.md#候选验收)

### 泛型组件

本地组件可用 `@typeparam T` 声明一个类型参数。调用方提供类型明确的数组或 `IReadOnlyList<T>` 参数时，Roslyn 推断 `T`，再用它检查委托和插槽载荷。调用方也可以显式写 `T="string"`。组件运行时使用同一份 Vue 模块。 [本地泛型组件样例](../../samples/Jolt.Counter/.fixtures/List.jazor)

```razor
<List Items="@Names" Select="@OnPick">
    <template #item context="row"><p>@row.value.Length</p></template>
</List>

@code {
    private string[] Names = ["One", "Four"];
    private void OnPick(string name) { }
}
```

npm 消费已验证 Reka UI 2.11.0 的 `ListboxVirtualizer`。它从 `Options` 推断元素类型，并将该类型用于 `TextContent` 的参数和默认插槽的 `option`。当前支持公开 Vue 可调用组件的单个泛型参数和必填数组输入，支持范围还取决于声明可映射的原始类型约束。 [npm 消费样例](../../samples/Jolt.Counter/.fixtures/App.jazor)

### 组件样式

在文件顶层写 `<style scoped>`。Jolt 为组件模块分配稳定的 scope ID，`jolt dev` 和 `jolt bundle` 复用 Vue 的样式转换。已验证同名选择器隔离、父组件对其子组件根节点的样式、`:slotted()` 和样式热更新。当前作者入口支持原生 CSS 样式块。 [浏览器验收样例](../../samples/Jolt.Counter/README.md)

## 验证与边界

运行 `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt` 检查语法与编译回归。npm 类型、编辑器协议和浏览器分别有独立验证，可直接检查包升级后的行为、未保存编辑和生产资源。[npm 验证](../../src/Jolt.Npm/verify.cs) · [编辑器协议验证](../../src/Jolt.LanguageServer/verify.cs) · [浏览器验证](../../samples/Jolt.Counter/README.md)

Razor 语言层负责标记与 C# 的切换，Jolt 的前端保留作者位置。当前实现根据 SDK 11.0.100 中三个优化步骤的名称跳过静态标记合并、空白消除和提前编码，升级 Razor 时要重跑位置与错误恢复回归。可投影的 TypeScript 类型和 Vue 组件形状由类型检查和使用点诊断界定。应用级 SSR 和 hydration 继续列在独立验证范围内。
