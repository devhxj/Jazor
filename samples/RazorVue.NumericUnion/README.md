# Element Plus 数值与 union 最小示例

将 [NumericUnionAuthoring.razor](./NumericUnionAuthoring.razor) 和 [code-behind](./NumericUnionAuthoring.razor.cs)
放入已配置 `Jazor`、`Jazor.Vue`、`ECMAScript.ElementPlus` 的 RazorVue 项目，调整 namespace 后在页面使用
`<NumericUnionAuthoring />`。包版本须与宿主选定的候选/发行版本一致。

示例保留 Avatar 的 number/string 分支、Pagination 的 `Number` 到业务 `int` 边界、Select 的
nullable/nested union 和数组分支，以及 `ElTypedSelect<string>` 的原始字符串 ID 与显式空串清空策略。
普通整数直接写 `Size="@(32)"`，嵌套值直接读取 `Selected?.AsNumber` / `Selected?.AsString`；
原有分支类型和 `AsSingle` 路径继续保留。
完整作者矩阵见 [作者指南](../../docs/03-guides/razorvue-authoring.md#numeric-union-authoring)。

[BindingMatrix.razor](./BindingMatrix.razor) / [code-behind](./BindingMatrix.razor.cs) 增加
Vuetify `Rounded` 和 TDesign `MaxWidth` 的 Number union、整数变量与 float；工程需同时直接引用
同版本 `ECMAScript.Vuetify` / `ECMAScript.TDesign`，并在 `_Imports.razor` 设置与 code-behind
一致的 `@namespace Demo.Pages`。这些作者改善随 preview.9 交付，所有引用包须使用同一版本。

在仓库中运行以下聚焦回归，直接读取这两份源码，由官方 Razor SG 编译，经 RazorVue 生成 Vue 模块，
再使用真实 Element Plus 2.14.5、Vue 3.5.42 与 happy-dom 验证交互：

```sh
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter RazorSgOfficialElementPlusNumericUnionTests
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter RazorSgOfficialNumericUnionMatrixTests
```

回归检查宿主值、事件、class 与数值 inline style；CSS 外观由真实浏览器消费者门禁验证。
跨库矩阵的 runtime 组件是测试替身，检查官方 SG 与最终 JS 参数值；真实 Element Plus 交互由第一条命令验证。
