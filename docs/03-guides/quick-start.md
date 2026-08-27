# 快速开始

> 目标：用最小的两项目结构验证 Jazor 核心的 C# -> ECMAScript 模块路径。

本指南首先验证框架无关的核心能力。Razor-to-Vue 是后续可选集成，不是开始使用 Jazor 的前置条件。

## 1. 创建模块库

创建一个类库项目并安装 `Jazor`：

```bash
dotnet new classlib -n Sample.Modules
dotnet add Sample.Modules package Jazor --version 0.24.0
```

在类库中声明一个 ECMAScript 模块：

```csharp
using ECMAScript;

namespace Sample.Modules;

[ECMAScriptModule("features/greetings.mjs")]
public static class Greetings
{
    public static string Create(string name) => $"Hello, {name}";
}
```

`[ECMAScriptModule]` 表示该类型进入 Jazor 的模块转换域。调用、导入、宿主 API 和不支持语义仍由 Roslyn 绑定与编译器白名单决定。

## 2. 创建宿主项目

创建可执行或 Web 宿主，引用模块库和 `Jazor`：

```bash
dotnet new web -n Sample.Host
dotnet add Sample.Host reference Sample.Modules
dotnet add Sample.Host package Jazor --version 0.24.0
```

在 `Sample.Host.csproj` 配置 debug 输出：

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

## 3. 构建并检查产物

从宿主项目所在目录执行：

```bash
dotnet build Sample.Host
```

构建成功后，`jazor/` 中会包含 `features/greetings.mjs`、对应的 source map 和 `jazor-manifest.json`。Web 宿主使用 `UseJazorHost()` 后，浏览器仍通过 `/jazor/*` 访问；发布时这些文件会复制到 `<publish>/jazor/`。生成模块使用标准 ECMAScript 具名导出；跨模块调用由编译器创建稳定 import。

## 4. 可选：加入 Razor-to-Vue

当宿主是 Razor SDK 项目且需要当前 Vue 组件集成时，额外引用 `Jazor.Vue`：

```xml
<ItemGroup>
  <PackageReference Include="Jazor.Vue" Version="0.24.0" PrivateAssets="all" />
</ItemGroup>
```

此时 Razor-to-Vue 会在官方 Razor SG 的最终 compilation 上工作，并复用上述核心编译路径；它不会改变普通 C# 模块的转换语义。详细边界见 [Razor-to-Vue](../02-architecture/razor-to-vue.md)。

## 常见检查

- 没有产物：确认 `JazorMode` 配置在最终宿主，而不是仅配置在模块类库。
- Razor 组件未参与转换：确认项目显式引用 `Jazor.Vue`，并且是支持 Razor Source Generator 的 Razor SDK 项目。
- 外部成员无法编译：检查该成员是否已有 Jazor 宿主映射；不要通过原始 JavaScript 或 `object` 绕过类型边界。

更多包配置见 [安装与配置](./installation-and-configuration.md)，真实项目组合见 [示例](./examples.md)。
