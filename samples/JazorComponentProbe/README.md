# .jazor 组件验证

[Counter.jazor](./Counter.jazor) 用顶层标记、`@using Npm.RekaUi` 和 `@code` 引用 Reka UI 的 `CollapsibleRoot`。运行时直接从 `reka-ui` 导入组件，类型来自同一份安装包的公开声明。还原依赖即可运行，无需组件绑定类库。

在仓库根目录使用 `global.json` 指定的 SDK，首次运行需访问 NuGet 和 npm 源。

```sh
dotnet run --file samples/JazorComponentProbe/verify.cs
dotnet run --project samples/JazorComponentProbe/Razor
dotnet run --project samples/JazorComponentProbe/Npm -- --verify
```

第一条命令用 Razor 的公开语言 API 读取原文区间，根据包的布尔 prop 构造有返回类型的 C# 表达式，将实例成员与表达式一起交给 Roslyn 和 Jazor.Compiler。产出的 Vue 模块在进程内 LibNode 中经 Vue 服务端渲染，检查 Reka UI 输出的 `data-state="open"`。再将 `Count > 0` 改为 `Count`，独立验证编译失败的状态码、原文第 3 行第 26 列的 `CS0029` 及输出文件缺失。

第二条命令检查嵌套标记、C# 表达式、指令范围和不完整输入恢复。第三条命令验证 npm 的 ESM 导出、props、事件和插槽，并比较 Reka UI 2.10.1 与 2.11.0 的 `NumberFieldRoot.allowInvalid` 声明。依赖还原、TypeScript checker 和模块加载均使用仓库的进程内 Node 宿主。 [Razor API 与位置映射](./Razor/README.md)

单独编译示例、查看生成的模块：

```sh
dotnet run --project samples/JazorComponentProbe -- samples/JazorComponentProbe/Counter.jazor samples/JazorComponentProbe/Npm/current samples/JazorComponentProbe/Npm/current/.jazor/Counter.mjs
```

## 当前范围

联调覆盖一个 npm 命名空间、一个无子节点的组件、一个 C# 表达式属性和一个 `@code` 块。样例按 npm 包名推导命名空间，遇到歧义报告错误。表达式属性目前投影布尔值、字符串或数值。事件和插槽的验证止于读取类型。泛型可调用组件（如 `SwitchRoot`）、浏览器交互、响应式更新和 LSP 仍待验证。SDK 的公开优化 IR 会合并纯静态标记并丢失逐标签范围，后续编辑器工作需要单独验证这一点。
