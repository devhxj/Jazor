# Razor-to-Vue 设计目标

## 定位

Razor-to-Vue 将官方 Razor Source Generator 生成的组件 C# 语义转换为 Vue render-function 模块。它不是 Razor 模板解析器，也不通过中间 SFC 或自定义模板协议完成转换。

当前生产链路为：

```text
官方 Razor Source Generator
    -> GeneratorDriver 完成后的 Compilation
    -> Jazor.Vue Hook
    -> generated BuildRenderTree 绑定
    -> Jazor.Compiler / SemanticWalker
    -> Vue render-function .mjs
    -> Jazor.Emit
```

## 包边界

| 包 | 职责 |
| --- | --- |
| `Jazor` | 编译器、分析器、运行时契约、Emit 工具和 MSBuild 输出支持 |
| `Jazor.Vue` | 显式安装 Razor Hook 及其 analyzer payload |
| `Jazor.RazorVue` | Razor 生成 C# 绑定、组件选择、Vue 产物构造和运行时资源 |
| `Jazor.Emit` | 模块、源映射、manifest、运行时资源和 bundle 的物化 |

仅引用 `Jazor` 时，不安装 Hook、不扫描 Razor 组件，也不生成 `Jazor.Generated.VueRenderCatalog`。需要 Razor-to-Vue 时，在声明 Razor 组件的项目中显式引用：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.31" />
  <PackageReference Include="Jazor.Vue" Version="0.1.31" PrivateAssets="all" />
</ItemGroup>
```

## 输入与输出边界

生产输入是官方 Razor Source Generator 完成后的最终 Roslyn `Compilation`。实现不依赖：

- `EnableRazorHostOutputs`；
- `RazorCodeDocument` 或 `RazorCSharpDocument`；
- Razor DR/IR 作为生产中间表示；
- 生成 C# 的二次解析；
- Razor-to-SFC 或 wrapper marker 协议作为回退路径。

Razor 组件最终生成 Vue render-function `.mjs` 模块，并由 `Jazor.Emit` 按 `JazorMode` 物化：

| 模式 | 输出 |
| --- | --- |
| `none` | 默认，不输出产物 |
| `debug` | 模块、源映射和 manifest |
| `release` | 生产 bundle 和源映射 |

默认输出目录为 `$(MSBuildProjectDirectory)\wwwroot\jazor\`；`JazorTool` 仅在 `release` 模式下生效。

## 实现落点

| 路径 | 职责 |
| --- | --- |
| `src/Jazor.Vue/` | opt-in 包工程和 analyzer payload 打包 |
| `src/Jazor.RazorVue.Generator/` | generator-driver Hook 与 catalog 生成 |
| `src/Jazor.RazorVue/RazorSdk/` | Razor SG 生成 C# 绑定与组件候选选择 |
| `src/Jazor.RazorVue/Runtime/` | Vue render-context 运行时资源 |
| `src/Jazor.Compiler/` | Roslyn `IOperation` 语义降低 |
| `src/Jazor.Emit/` | 产物物化、manifest、源映射和 bundle |

RazorVue 中的 C# 表达式、成员访问、函数调用、临时变量、导入收集和 RenderTreeBuilder 语义必须通过 `Jazor.Compiler` 的翻译入口完成。RazorVue 仅负责其特有的组件边界和 Vue 产物封装。

## 验证入口

```text
dotnet run --file scripts/csharp/test-dotnet.cs -- --project razor-sg
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit
dotnet run --file scripts/csharp/test-dotnet.cs -- --project render-context
```
