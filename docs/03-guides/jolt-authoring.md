# 用 Jolt 编写 Vue 组件

Jolt 是用 C# 编写 Vue 3 单文件组件的工具。一份 `.jazor` 文件对应一个 ES 模块，默认导出 Vue 组件。HTML 和组件标签直接写在顶层。`@code` 的实例成员由每个组件实例独立持有，静态方法和字段进入 Jazor 静态模块类。`public static` 方法和字段以及公开常量按 Jazor 的模块规则生成具名导出。

## 安装并运行

Jolt CLI 使用 .NET 11，发布包覆盖 Windows 和 glibc Linux 的 x64 与 ARM64。在克隆的仓库根目录执行以下命令。

```sh
dotnet tool install --global Jazor.Jolt --version 2.2.0
jolt dev samples/Jolt.Counter
```

已有 CLI 时，请用 `dotnet tool update --global Jazor.Jolt --version 2.2.0`。打开终端打印的网址，点击 **Increment**。子组件通过 `Action<int>` 回调通知父组件，父组件更新计数并控制 Reka UI 组件的打开状态。 [父组件](../../samples/Jolt.Counter/App.jazor) · [子组件](../../samples/Jolt.Counter/CounterButton.jazor)

| 命令 | 结果 |
| --- | --- |
| `jolt dev <project>` | 启动 Vite 并监听作者文件。可用 `--port <端口>` 指定端口。 |
| `jolt build <project>` | 在 `.jolt/` 生成组件模块、源码映射与模块清单。可用 `--output <目录>` 指定输出。 |
| `jolt bundle <project>` | 用项目的 Vite 配置生成生产资源。计数器示例输出到 `dist/`。 |
| `jolt lsp` | 通过标准输入输出启动编辑器使用的语言服务。 |

从仓库源码运行时，将 `jolt` 替换为 `dotnet run --project src/Jolt.Cli --`。 [示例与浏览器测试](../../samples/Jolt.Counter/README.md)

## 配置编辑器

在发布页下载与 CLI 同一发行版的扩展，将示例的 `samples/Jolt.Counter` 作为文件夹打开，再打开 `App.jazor`。 [2.2.0 下载](https://github.com/devhxj/Jazor/releases/tag/v2.2.0)

| 编辑器 | 安装 |
| --- | --- |
| VS Code | 选择 **Extensions: Install from VSIX**，安装 `jolt-2.2.0.vsix`。 [VS Code 配置](../../editors/jolt/README.md) |
| Visual Studio 2026 18.12 及以上（Windows x64） | 关闭 Visual Studio，再用 VSIX Installer 打开 `Jolt.VisualStudio.vsix`。 [Visual Studio 配置](../../editors/jolt-visual-studio/README.md) |

在 `@code` 内输入成员名可检查 C# 补全，在 `CollapsibleRoot` 上执行转到定义可检查 npm 声明导航。首次还原 npm 依赖时，本地 C# 与本地组件仍可补全和导航，npm 类型就绪后编辑器会刷新相关诊断和语义高亮。

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

## 从 JavaScript 导入组件和静态成员

将以下代码保存为项目根目录的 `Counter.jazor`。点击按钮会更新当前组件实例的 `Count`。

```razor
<button @onclick="@Increment">@Count</button>

@code {
    public const int Initial = 3;
    public static int Step = 2;

    private int Count = Initial;

    public static int Advance(int value) => value + Step;
    private void Increment() => Count = Advance(Count);
}
```

执行 `jolt build .` 后，项目根目录的 JavaScript 入口可同时导入组件和静态成员。

```javascript
import Counter, { Advance, Initial, Step } from './.jolt/Counter.mjs';
```

`Count` 随组件实例创建。`Step` 是模块级状态，`Advance` 使用该模块的当前值。

## 组件参数、插槽和重构

组件用 `[Parameter]` 声明参数和委托，用 `[EditorRequired]` 标记调用者必须提供的参数。默认插槽使用 `<slot />`，具名插槽使用 `<slot name="header" />`，调用方用 `<template #header>…</template>` 填充内容。`<template>` 在这里是具名插槽写法，文件顶层可以直接书写标记。C# 区域支持 `@name`、`@(expression)`、`@if` 和 `@foreach`。

编辑器用相同的项目加载器、组件声明和 C# 投影提供补全、悬停、签名帮助、定义导航和诊断。键入标签名时，可以选择同项目其他命名空间的本地组件，接受建议后会插入相应的 `@using`，随后可以补全该组件的类型化参数。npm 定义可跳转至声明文件。 [VS Code 扩展](../../editors/jolt/README.md) · [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md)

编辑器提供查找引用、改名、格式化和快捷修复。跨文件改名按 Roslyn 符号和组件绑定更新 C#、参数、委托和具名插槽。支持文件操作的编辑器还可以从标签改名组件及其同目录文件。格式化调整 C# 和标签内的属性缩进，保留渲染文本与换行风格。未知的本地组件可以用快捷修复添加 `@using`，缺少必填参数时可以匹配当前组件的同名同类型字段或可读属性。

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

运行 `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt` 检查语法与编译回归。npm 类型、编辑器协议和浏览器分别有独立验证，可直接检查包升级后的行为、未保存编辑和生产资源。 [npm 验证](../../src/Jolt.Npm/verify.cs) · [编辑器协议验证](../../src/Jolt.LanguageServer/verify.cs) · [浏览器验证](../../samples/Jolt.Counter/README.md)

应用级 SSR 和 hydration 仍需单独验收。 [当前状态](../04-roadmap/current-status.md)
