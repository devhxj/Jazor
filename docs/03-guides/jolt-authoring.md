# 用 Jolt 编写 Vue 组件

Jolt 是用 C# 编写 Vue 3 单文件组件的工具。一份 `.jazor` 文件对应一个 ES 模块，默认导出 Vue 组件。HTML 和组件标签直接写在顶层。`@code` 的实例成员由每个组件实例独立持有，静态方法和字段进入 Jazor 静态模块类。`public static` 方法和字段以及公开常量按 Jazor 的模块规则生成具名导出。

编译链路和指令的实际产物见[语法审计](../02-architecture/jolt-authoring-comparison.md#现状审计)。后续拟支持的写法与示例见[目标特性](../02-architecture/jolt-authoring-comparison.md#目标特性与写法)。

## 安装并运行

Jolt CLI 使用 .NET 11，发布包覆盖 Windows 和 glibc Linux 的 x64 与 ARM64。在克隆的仓库根目录执行以下命令。

```sh
dotnet tool install --global Jazor.Jolt --version 2.3.0
jolt dev samples/Jolt.Counter --port 15658
```

已有 CLI 时，请用 `dotnet tool update --global Jazor.Jolt --version 2.3.0`。打开终端打印的网址，点击 **Increment**。子组件通过 `Action<int>` 回调通知父组件，父组件更新计数并控制 Reka UI 组件的打开状态。 [父组件](../../samples/Jolt.Counter/App.jazor) · [子组件](../../samples/Jolt.Counter/CounterButton.jazor)

| 命令 | 结果 |
| --- | --- |
| `jolt dev <project>` | 启动 Vite 并监听作者文件。当前源码默认使用 `http://127.0.0.1:15658/`，可用 `--port <端口>` 覆盖。 |
| `jolt build <project>` | 在 `.jolt/` 生成组件模块、源码映射与模块清单。可用 `--output <目录>` 指定输出。 |
| `jolt bundle <project>` | 用项目的 Vite 配置生成生产资源。计数器示例输出到 `dist/`。 |
| `jolt bundle <project> --ssr <server-entry>` | 从同一次组件编译生成 `dist/client` 和 `dist/server`。服务端入口相对于项目根目录解析。 |
| `jolt library <project>` | 生成可安装的 ESM 组件包、TypeScript 声明和样式，默认输出到项目的 `dist/`。支持 `--output <目录>`。 |
| `jolt lsp` | 通过标准输入输出启动编辑器使用的语言服务。 |

从仓库源码运行时，将 `jolt` 替换为 `dotnet run --project src/Jolt.Cli --`。 [示例与浏览器测试](../../samples/Jolt.Counter/README.md)

## 配置编辑器

在发布页下载与 CLI 同一发行版的扩展，将示例的 `samples/Jolt.Counter` 作为文件夹打开，再打开 `App.jazor`。 [2.3.0 下载](https://github.com/devhxj/Jazor/releases/tag/v2.3.0)

| 编辑器 | 安装 |
| --- | --- |
| VS Code | 选择 **Extensions: Install from VSIX**，安装 `jolt-2.3.0.vsix`。 [VS Code 配置](../../editors/jolt/README.md) |
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

命名约定区分包、子路径和导出。例如 `@scope/ui-kit/button` 的显式公开子路径对应 `Npm.Scope.UiKit.Button`。组件的具名导出沿用导出名，默认组件用路径末段的 PascalCase 名称。编译和编辑器读取已还原包的公开声明，运行时使用对应的 ESM 导出。同项目的 `.vue` 文件遵循本地目录命名空间规则。 [npm 元数据与范围](../../src/Jolt.Npm/README.md) · [混编样例](../../samples/Jolt.Mixed/App.vue)

### 直接调用 npm 函数

在项目的 `package.json` 中声明依赖。下例使用 `es-toolkit` 1.53.2 的公开子路径 `es-toolkit/string`。导入 `Exports` 类型并起一个 C# 别名，就可以在标记或 `@code` 中调用函数。多个组件共用的别名可以放在目录的 `_Imports.jazor`。

```razor
@using Text = Npm.EsToolkit.String.Exports

<output>@Text.CamelCase(Title)</output>

@code {
    [Parameter] public string Title { get; set; } = "Order total";
}
```

页面显示 `orderTotal`。Jolt 从锁定版本的声明得到 `string → string`，生成模块从原子路径导入 `camelCase`。写成 `Text.CamelCase(42)` 时，CLI 和编辑器在实参处报告 C# 类型错误。编辑器可从方法跳到已还原包中的声明。普通具名值采用 PascalCase，默认值使用 `Text.Default`，标量常量可作为只读属性读取。 [真实依赖及版本](../../samples/Jolt.Counter/package.json) · [导出类型与限制](../../src/Jolt.Npm/README.md)

### 在每个实例中使用 VueUse

对需要 Vue 实例上下文的组合式函数，在 `@code` 中声明一个带 `[Setup]` 的同步、无参、返回 `void` 的私有实例方法。Vue 建立 props、slots 后调用它一次，随后开始渲染。下面的组件每次挂载都创建自己的计数器。

```razor
@using VueUse = Npm.Vueuse.Core.Exports

<button @onclick="Increment">@counter.Count.Value</button>

@code {
    [Parameter] public double Initial { get; set; }
    private VueUse.UseCounterResult counter = default!;

    [Setup]
    private void Configure() => counter = VueUse.UseCounter(Initial);

    private void Increment() => counter.Inc();
}
```

`UseCounterResult` 来自 `@vueuse/core` 14.4.0 的返回类型。`Count.Value` 可读取且参与响应式追踪，按该版本的声明应通过 `Inc()`、`Dec()` 或 `Set(value)` 更新。同步 setup 中注册的 Vue watcher 归组件实例所有，卸载即停止。 [可运行组件](../../samples/Jolt.Counter/.fixtures/NpmCounter.jazor) · [双实例、卸载与重新挂载验证](../../samples/Jolt.Counter/verify-browser.cs)

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

### 表单、节点身份和事件

从本仓库构建 CLI 后，下面的组件可以让 Reka UI 的开关与原生勾选框共同编辑一个布尔值。

```razor
@using Npm.RekaUi

<section>
    <input @bind="filter" @bind:event="oninput" />
    <label><input type="checkbox" @bind="open" /> 展开</label>
    <CollapsibleRoot @bind-Open="open">
        <CollapsibleTrigger>详情</CollapsibleTrigger>
        <CollapsibleContent>@filter</CollapsibleContent>
    </CollapsibleRoot>
</section>

@code {
    private string filter = "";
    private bool open;
}
```

文本框和 `textarea` 默认在 `change` 时写回字符串。`oninput` 随输入更新，输入法组合期间由 Vue 推迟到提交后写回。checkbox 使用 `bool`。命名模型从 npm 声明配对同名 prop 与单参数 `update:名字` 事件，目前支持同类型的 `boolean`、`string` 或 `number`。绑定目标须是组件可以写入的状态。

循环中的 `<li @key="row.Id">` 和带 `@key` 的组件使用 Vue 节点身份。DOM 事件支持无参处理器，以及接收 WebIDL 事件的同步或 `Task` 方法，例如 `@onkeydown="HandleKey"` 可接收 `ECMAScript.KeyboardEvent`。用 `@onsubmit:preventDefault` 取消默认行为，用 `@onclick:stopPropagation="enabled"` 在事件发生时读取布尔条件。npm 组件的 `@onupdate:open` 仍按包声明的载荷调用。 [可编辑列表示例](../../samples/Jolt.Counter/.fixtures/EditableList.jazor)

| 原生控件 | 绑定类型 | 更新时机 |
| --- | --- | --- |
| 文本输入、`textarea` | `string` | 默认 `change`，可指定 `oninput` |
| checkbox | `bool` | 勾选变化 |
| radio、单选 `select` | `string` | 选择变化。显式选项值也使用字符串 |
| `<select multiple>` | `string[]` | 选择变化。多选由静态 HTML 属性决定 |
| `<input type="number">` | `double?` | 默认 `change`，可指定 `oninput`。清空或无效输入写回 `null` |

希望自己决定是否接受输入时，成对使用 `:get` 与 `:set`。普通绑定完成赋值后要运行逻辑时，使用 `:after`。这两种形式分别支持同步方法或返回 `Task` 的方法，也可用于组件的命名模型，如 `@bind-Value:get` / `@bind-Value:set`。文本和数字控件在同步 setter 返回时重新读取 getter，拒绝输入后立即恢复显示。异步提交的后续更新及选择类控件的回显遵循 Vue 响应式刷新。 [原生表单样例](../../samples/Jolt.Counter/.fixtures/NativeForm.jazor) · [绑定与交互样例](../../samples/Jolt.Counter/.fixtures/TableDemo.jazor)

```razor
<input @bind:get="Name" @bind:set="SetName" @bind:event="oninput" />
<input @bind="Note" @bind:after="SaveNote" />
<output>@LastSaved</output>

@code {
    private string Name = "";
    private string Note = "";
    private string LastSaved = "";
    private void SetName(string value) => Name = value.TrimStart();
    private void SaveNote() => LastSaved = Note;
}
```

### 本地组件、引用和属性透传

在子组件上声明 `[Parameter] public string Value { get; set; }` 和 `[Parameter] public Action<string>? ValueChanged { get; set; }`，并在编辑时调用 `ValueChanged?.Invoke(next)`。父组件可用 `<TableEditor @bind-Value="name" />` 接收更新。生成的组件公开 Vue 的 `value` / `update:value`，已有显式传 `Value` 和 `ValueChanged` 的调用仍走原回调。 [子组件](../../samples/Jolt.Counter/.fixtures/TableEditor.jazor)

用 `<input @ref="input" />` 捕获单个元素，在组件上声明可写的 `ECMAScript.HTMLInputElement?` 字段或属性。给本地组件的公共实例方法加 `[Expose]`，父组件即可用生成的 `TableEditorRef?` 调用 `editor?.Focus()`。挂载完成后引用可用，移除目标时 Vue 清空引用。当前只支持单个 DOM 或本地组件目标，循环中的集合引用和 npm 组件实例引用留待单独实现。 [焦点和卸载样例](../../samples/Jolt.Counter/.fixtures/TableDemo.jazor)

透传一组属性可写 `<button class="base" @attributes="attrs" title="保存">保存</button>`。例如在 `@using static ECMAScript.Vue` 下将 `attrs` 声明为 `VueDictionary<string>`。属性和展开表达式按书写顺序各求值一次，普通重名键以后者为准，class、style 和监听器交给 Vue 的 `mergeProps`。调用具有必填参数的组件时仍需显式给出该参数，便于 Roslyn 在编译时检查。 [属性展开回归](../../src/Jolt.Compiler.Test/AttributesDirectiveTests.cs)

### Vue 内置组件

`<Teleport>` 和 `<Transition>` 可直接用于 `.jazor` 模板。当前作用域内同名的本地组件优先。

```razor
<Teleport To="#overlays" Disabled="@ShowInline">
    <aside>通知</aside>
</Teleport>

<Transition Name="fade" Mode="out-in">
    @if (ShowNotice) {
        <p>通知已开启</p>
    }
</Transition>

@code {
    private bool ShowInline;
    private bool ShowNotice = true;
}
```

`Teleport` 的 `To` 必填，接受 CSS 选择器或能隐式转换为 `Vue.VueTeleportTarget` 的表达式。`Disabled` 接受布尔值，启用时子节点留在原处。SSR 宿主需要在目标容器内放入服务器返回的 Teleport 内容，客户端才能水合该部分。

`Transition` 接受 `Name` 和可选的 `Mode`。`Mode` 支持 `in-out`、`out-in` 或 `Vue.VueTransitionMode` 表达式。直接子节点或 `<template #default>` 构成默认插槽，每次渲染可以提供一个元素或组件，`@if` 的各个分支分别检查。

### 参数与插槽

组件用 `[Parameter]` 声明参数和委托，用 `[EditorRequired]` 标记调用者必须提供的参数。默认插槽使用 `<slot />`，具名插槽使用 `<slot name="header" />`，调用方用 `<template #header>…</template>` 填充内容。`<template>` 在这里是具名插槽写法，文件顶层可以直接书写标记。C# 区域支持 `@name`、`@(expression)`、`@if` 和 `@foreach`。

编辑器用相同的项目加载器、组件声明和 C# 投影提供补全、悬停、签名帮助、定义导航和诊断。键入标签名时，可以选择同项目其他命名空间的本地组件，接受建议后会插入相应的 `@using`，随后可以补全该组件的类型化参数。npm 定义可跳转至声明文件。 [VS Code 扩展](../../editors/jolt/README.md) · [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md)

编辑器提供查找引用、改名、格式化和快捷修复。跨文件改名按 Roslyn 符号和组件绑定更新 C#、参数、委托和具名插槽。支持文件操作的编辑器还可以从标签改名组件及其同目录文件。格式化调整 C# 和标签内的属性缩进，保留渲染文本与换行风格。未知的本地组件可以用快捷修复添加 `@using`，缺少必填参数时可以匹配当前组件的同名同类型字段或可读属性。

### 泛型组件

本地组件支持单个 `@typeparam T`。调用方提供类型明确的数组或 `IReadOnlyList<T>` 参数时，Roslyn 推断 `T`，再用它检查委托和插槽载荷。调用方也可以显式写 `T="string"`。组件运行时使用同一份 Vue 模块。 [本地泛型组件样例](../../samples/Jolt.Counter/.fixtures/List.jazor)

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

### 泛型表格、内联模板与目录导入

本地组件可分别声明多个 `@typeparam` 及其 `where` 约束。调用点可显式给出全部参数，也可让 Roslyn 从输入值与委托推断。下例中 `Items` 决定行类型，`GetStatus` 决定状态类型，`Row Context="row"` 中能直接访问带类型的 `row.value` 和 `row.status`。传递的类型参数在运行时擦除。普通 `T` 上的 `default(T)` 仍受核心编译器的约束，可证明引用类型时才能降为 `null`。 [表格声明](../../samples/Jolt.Counter/.fixtures/TypedTable.jazor) · [多泛型检查](../../src/Jolt.Compiler.Test/GenericComponentTests.cs)

```razor
<TypedTable Items="@Names" RowKey="@Length" GetStatus="@Length">
    <Row Context="row">
        @{ var label = row.value; }
        <td>@label</td><td>@row.status</td>
    </Row>
</TypedTable>

@code {
    private string[] Names = ["One", "Four"];
    private int Length(string name) => name.Length;
}
```

用 `@for`、`@foreach` 和 `@switch` 组织标记，支持模式守卫、`break` 和 `continue`。循环变量、case 变量和 `@{ … }` 中的局部声明遵循投影后的 C# 作用域。组件声明了 `row` 出口，就可以用 `<Row Context="row">` 填充，也可以保留 `<template #row context="row">`。默认出口可写 `<ChildContent>`。同名出口在不同分支需提供相同的字段及类型。 [控制流样例](../../samples/Jolt.Counter/.fixtures/TableDemo.jazor)

内联模板适合在多个位置复用同一段标记。通过 `RenderFragment<T>` 传给子组件的 `[Parameter]` 时，调用点检查类型。无参片段使用 `RenderFragment` 和 `() => @<…>`。每次调用产生新的 Vue 节点，并读取闭包中捕获的变量。 [内联模板回归](../../src/Jolt.Compiler.Test/InlineFragmentTests.cs)

```razor
@{ var suffix = "!"; RenderFragment<string> label = name => @<strong>@(name + suffix)</strong>; }
<p>@label("One")</p>
<p>@label("Four")</p>
```

项目根或子目录的 `_Imports.jazor` 接受 `@using`，包括 `@using Npm.RekaUi`。从根向内层依次导入，最后应用组件自己的指令。共享文件只影响所在目录及后代，不生成组件模块。编辑器使用未保存的导入，错误定位到实际声明文件。 [示例共享导入](../../samples/Jolt.Counter/.fixtures/_Imports.jazor)

### 组件样式

在文件顶层写 `<style scoped>`。Jolt 为组件模块分配稳定的 scope ID，`jolt dev` 和 `jolt bundle` 复用 Vue 的样式转换。已验证同名选择器隔离、父组件对其子组件根节点的样式、`:slotted()` 和样式热更新。当前作者入口支持原生 CSS 样式块。 [浏览器验收样例](../../samples/Jolt.Counter/README.md)

## Vue 项目集成

### 在一个 Vite 项目中混用 .vue 和 .jazor

从仓库根目录运行 `dotnet run --project src/Jolt.Cli -- dev samples/Jolt.Mixed`。样例中的 `App.vue` 持有一个字符串，通过 `v-model:text` 调用 Jolt 组件。Jolt 组件再调用同目录的原生 Vue 输入组件。

```vue
<script setup lang="ts">
import { ref } from 'vue';
import MixedEditor from './editors/MixedEditor.jazor';
const name = ref('First draft');
</script>

<template>
    <MixedEditor :Limit="24" v-model:text="name" />
</template>
```

在 `MixedEditor.jazor` 内，`<NameInput Limit="@Limit" @bind-Text:get="Text" @bind-Text:set="Save" />` 调用 `NameInput.vue`。Jolt 读取 Vue 组件的公开声明，识别 `limit: number` 和 `defineModel<string>('text')`，校验传入值与回写委托。跨目录使用 `@using <组件命名空间>`，规则与本地 Jolt 组件一致。 [Jolt 调用方](../../samples/Jolt.Mixed/editors/MixedEditor.jazor) · [Vue 输入组件](../../samples/Jolt.Mixed/editors/NameInput.vue)

项目声明 `vue`、`vite` 和 `@vitejs/plugin-vue`，用 `jolt dev` 或 `jolt bundle` 运行。标准 Vue 与 TypeScript 编辑工具通过生成的声明识别源码导入。首次运行 `jolt build` 或 `jolt dev` 后，在项目的 `tsconfig.json` 中使用下面两项配置。`jolt dev` 在编辑时更新 `.jolt/` 下的声明和映射，Vue Language Tools 可检查 SFC 和 TSX 调用方。 [完整 tsconfig](../../samples/Jolt.Mixed/tsconfig.json) · [TSX 源码调用](../../samples/Jolt.Mixed/probe.tsx)

```json
{
    "compilerOptions": {
        "moduleResolution": "Bundler",
        "allowArbitraryExtensions": true,
        "rootDirs": [".", "./.jolt"]
    }
}
```

### 通过 Vue Router 使用 `@page` 和 `@layout`

安装 `vue-router` 并在应用的 `package.json` 中设置 `"jolt": { "router": true }`。从构建目录导入 `routes`，将它传给应用已有的 `createRouter`，也可以和手写路由合用。

```ts
import { routes } from './.jolt/_routes.mjs'

const router = createRouter({ history, routes })
```

```razor
@page "/drafts/{id:int}"
@layout DraftLayout
<h1>Draft @Id</h1>

@code {
    [Parameter] public int Id { get; set; }
}
```

`@page` 接受绝对路径、独立的 `{name}` 字符串段和 `{name:int}` 整数段。参数名按大小写不敏感的方式匹配公开的 `[Parameter]` 属性，属性类型分别为 `string` 和 `int`。整数超出 Int32 范围会在向页面传递路由参数时报告错误。路由仍由应用创建并安装，因此客户端和 SSR 可以使用同一份路由记录。

`@layout` 选择一个本地、非泛型 `.jazor` 组件。布局须提供无作用域参数的默认 `<slot />`，且不能声明必填参数或路由指令。可以用完整组件名消除同名歧义。

### 将组件库交付给普通 Vue 或 TSX 项目

让库的 `package.json` 提供包名、版本和 Vue 版本范围，执行 `jolt library <项目目录>`。产物包含 npm 清单、以 Vue 为 peer dependency 的 ESM、公开类型及转换后的 scoped CSS。例如，本仓库的 `Meter` 可从包根具名导入，也可从独立子路径默认导入。不同目录有同名组件时按子路径选择。 [组件源码](../../scripts/csharp/fixtures/jolt-library/library/Meter.jazor) · [示例库清单](../../scripts/csharp/fixtures/jolt-library/library/package.json)

```ts
import { Meter } from '@jazor/library-meter-fixture';
```

在 SFC 中用 `<Meter v-model:value="value">`，用 `<template #reading="{ amount }">` 读取数值，模板引用只公开 `[Expose]` 声明的方法。TSX 可写 `value={value.value}`、`onUpdate:value={next => value.value = next}` 并传入带类型的插槽。包的 `exports` 会将 Vue 与 TypeScript 工具引向同一份组件声明。 [SFC 消费者](../../scripts/csharp/fixtures/jolt-library/consumer/src/App.vue) · [TSX 消费者](../../scripts/csharp/fixtures/jolt-library/consumer/src/JsxMeter.tsx)

### 使用 ASP.NET Core 托管混合应用的 SSR

从仓库根目录构建并启动示例。打开 `http://127.0.0.1:5168/editor?name=Alice`，查看页面源代码即可看到请求名称、计数和已渲染的 Teleport。点击计数按钮后，浏览器接管原来的 DOM，再用 Vue Router 切换页面。 [示例与发布步骤](../../samples/Jolt.Mixed.Ssr/README.md)

```sh
dotnet run --project src/Jolt.Cli -- bundle samples/Jolt.Mixed.Ssr/frontend --ssr entry-server.ts
dotnet run --project samples/Jolt.Mixed.Ssr -- --urls http://127.0.0.1:5168
```

**构建与部署。** `index.html` 提供 Vue 挂载节点、状态和 Teleport 目标的位置。Vite 生成带哈希的脚本和 CSS，两个入口都成功后替换项目的 `dist/`。样例的 ASP.NET Core 项目将整个目录随 `dotnet publish` 交付。部署新产物后重启宿主。如果应用位于子路径，Vite 的 `base`、Vue Router 的 history base 和 ASP.NET Core 的 `PathBase` 应一致。 [文档模板](../../samples/Jolt.Mixed.Ssr/frontend/index.html) · [发布配置](../../samples/Jolt.Mixed.Ssr/Jolt.Mixed.Ssr.csproj)

**每请求渲染。** 在宿主中注册 `AddJoltSsr("frontend/dist")`，通过 `UseJoltSsr` 的请求工厂提供 props 和 provider。服务端入口导出 `render({ url, state })`，为该请求创建 Vue app、memory history 和 Pinia。等待路由和预取完成，将最终 store 快照写回 `state.props`，并返回根 HTML 和 Vue 的 Teleport 片段。宿主把结果和序列化状态填入同一份构建模板。 [ASP.NET Core 入口](../../samples/Jolt.Mixed.Ssr/Program.cs) · [服务端入口](../../samples/Jolt.Mixed.Ssr/frontend/entry-server.ts)

**客户端接管。** 从 `#__jazor_ssr_state` 读取状态，以 web history 调用同一个应用工厂，恢复 Pinia 和 provider。等待 `router.isReady()` 后用 `createSSRApp` 创建的应用挂载。示例中的 `.jazor` 组件从 `[Setup]` 用 `Vue.Inject<string>` 读取请求值。 [共享工厂](../../samples/Jolt.Mixed.Ssr/frontend/workspace.ts) · [客户端入口](../../samples/Jolt.Mixed.Ssr/frontend/entry-client.ts) · [类型化注入](../../samples/Jolt.Mixed.Ssr/frontend/RequestBadge.jazor)

当页面需要从一组已知的本地组件中选择一个，可写 `<component is="@(showDetails ? Details : Summary)" Caption="@caption" />`。每个候选的参数和插槽都要通过检查，选中分支才创建 VNode。候选为当前作用域中可见的 `.jazor` 或 `.vue` 组件。 [动态组件回归](../../src/Jolt.Compiler.Test/DynamicComponentTests.cs)

## 验证与边界

运行 `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt` 检查语法与编译回归。npm 类型、编辑器协议和浏览器分别有独立验证，可直接检查包升级后的行为、未保存编辑和生产资源。 [npm 验证](../../src/Jolt.Npm/verify.cs) · [编辑器协议验证](../../src/Jolt.LanguageServer/verify.cs) · [浏览器验证](../../samples/Jolt.Counter/README.md)

SSR 有独立的并发请求与浏览器验收，覆盖预取失败后的恢复、接管原 DOM、首次点击、返回导航和组件清理。 [SSR 验收脚本](../../scripts/csharp/verify-jolt-ssr.cs) · [已发布能力](../04-roadmap/current-status.md)
