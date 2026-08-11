# 工具与集成

> 对应源码：`src/Jazor/`、`src/Jazor.Vue/`、`src/Jazor.Emit/`

## 定位

工具与集成层将编译器能力组织为可安装、可构建和可发布的产品形态。它不重新实现编译语义，而是负责 NuGet 包布局、MSBuild 集成、产物物化和显式工具链选择。

## 包边界

| 包 | 职责 |
| --- | --- |
| `Jazor` | 核心运行时契约、分析器、编译器支持、Emit 工具和 MSBuild 输出支持 |
| `Jazor.Vue` | 显式安装 Razor generator-driver Hook 及 RazorVue analyzer payload |
| `Jazor.Emit` | 读取静态 catalog，并物化模块、源映射、manifest、运行时资源和 bundle |

`Jazor` 默认不安装 Razor-to-Vue Hook。需要处理 Razor 组件时，项目必须显式引用 `Jazor.Vue`；`JazorMode` 只决定是否输出产物以及输出类型。

## 输出模式

| `JazorMode` | 行为 |
| --- | --- |
| `none` | 默认，不输出产物 |
| `debug` | 输出模块、源映射和 manifest |
| `release` | 内部物化后生成生产 bundle 和源映射 |

默认目录为 `$(MSBuildProjectDirectory)\wwwroot\jazor\`。`release` 固定使用 Netpack 打包；DenoHost 只承担显式运行时执行。

## 构建边界

MSBuild 只负责在构建完成后调用 `Jazor.Emit`。编译器负责语义转换，生成器负责将最终 `Compilation` 中的组件语义写入静态 catalog，Emit 负责读取 catalog 并完成文件物化。三者职责保持独立，避免将编译、写文件和工具链协议混为一层。

## 验证入口

```text
dotnet pack src/Jazor/Jazor.csproj
dotnet pack src/Jazor.Vue/Jazor.Vue.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
```
