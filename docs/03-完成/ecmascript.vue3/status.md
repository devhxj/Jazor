# ECMAScript.Vue3 状态

> 状态：当前状态快照
> 记录范围：`src/ECMAScript.Vue3/` 的项目结构、Vue 3 类型绑定、authoring contract 与验证入口

## 结论

`ECMAScript.Vue3` 是 Jazor 的 Vue 3 核心绑定层，负责提供可由 C# 编写和由 `Jazor.Compiler` 降低的 Vue API 契约。它不是 Razor-to-Vue Hook，也不负责 Razor 组件扫描、catalog 生成或文件物化。

当前职责已经稳定在以下范围：

- Vue 3 API 与类型绑定；
- `defineComponent`、`h`、响应式引用、生命周期、props、slots 等 authoring surface；
- Vue class/style、slot、model、provide/inject 等通用契约；
- 通过 `ECMAScript.Contract` 和 `Jazor.Compiler` 完成宿主映射与 JavaScript 输出。

## 项目结构

| 路径 | 职责 |
| --- | --- |
| `src/ECMAScript.Vue3/Vue3.cs` | 模块入口和公共导出壳 |
| `src/ECMAScript.Vue3/Api/` | Vue API 绑定 |
| `src/ECMAScript.Vue3/Types/` | Vue 类型和值域契约 |
| `docs/01-目标/ecmascript.vue3/` | 长期设计、模块映射和覆盖矩阵 |
| `docs/02-计划/ecmascript.vue3/` | authoring 实施计划和剩余工作 |
| `src/ECMAScript.Vue3.Test/` | 独立测试工程 |

## 当前契约

1. 公共类型优先表达明确的 C# authoring contract，不以 `object?` 或无约束泛型模拟 JavaScript 任意值。
2. Vue 运行时值域通过命名类型、联合类型或显式重载表达，并保持 Roslyn 绑定与 JavaScript 擦除语义一致。
3. slot、model、attrs、provide/inject 等跨组件约定必须由共享契约承载，不能在单个组件库中重复定义近似类型。
4. 编译器只为已声明且有明确映射的 Vue API 生成 JavaScript；不支持的宿主语义必须在分析或 lowering 阶段明确失败。

## 与 Razor-to-Vue 的关系

Razor-to-Vue 通过独立的 `Jazor.Vue` 包启用。其组件 lowering 可以复用 `ECMAScript.Vue3` 的类型和运行时契约，但以下职责不属于本项目：

- 安装 generator-driver Hook；
- 读取最终 Razor `Compilation`；
- 生成 `Jazor.Generated.VueRenderCatalog`；
- 物化 `.mjs`、manifest 或 production bundle。

## 验证入口

```text
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
```

详细 API 覆盖和映射规则见 [Vue 3 目标设计](../../01-目标/ecmascript.vue3/README.md)。
