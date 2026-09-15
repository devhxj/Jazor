# 快速开始

> 目标：用最小的两项目结构验证 Jazor 核心的 C# -> ECMAScript 模块路径。

本指南首先验证框架无关的核心能力。Razor-to-Vue 作为后续可选集成提供组件作者入口。

## 1. 创建模块库

创建一个类库项目并安装 `Jazor`：

```bash
dotnet new classlib -n Sample.Modules
dotnet add Sample.Modules package Jazor --version 1.0.0-preview.1
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

`[ECMAScriptModule]` 表示该类型进入 Jazor 的模块转换域。调用、导入和宿主 API 由 Roslyn 绑定与编译器白名单决定。

## 2. 创建宿主项目

创建可执行或 Web 宿主，引用模块库和 `Jazor`：

```bash
dotnet new web -n Sample.Host
dotnet add Sample.Host reference Sample.Modules
dotnet add Sample.Host package Jazor --version 1.0.0-preview.1
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

构建完成后，MSBuild 在最终 `Exe`/`WinExe` 宿主的 `Build` 后调用 `Jazor.Emit`，一次性物化 `features/greetings.mjs`、对应 source map、`jazor-manifest.json` 和 import map 到 `JazorDir`。类库在 DLL 内携带 `Jazor.Generated.ModuleCatalog`；最终宿主负责输出目录。生成模块使用标准 ECMAScript 具名导出；跨模块调用由编译器创建稳定 import。发布时 SDK 复制已物化目录到发布输出的 `jazor/` 位置。

## 4. 可选：加入 Razor-to-Vue

当宿主是 Razor SDK 项目且需要当前 Vue 组件集成时，额外引用 `Jazor.Vue`：

```xml
<ItemGroup>
  <PackageReference Include="Jazor.Vue" Version="1.0.0-preview.1" PrivateAssets="all" />
</ItemGroup>
```

此时 Razor-to-Vue 在官方 Razor SG 的最终 compilation 上工作，并复用上述核心编译路径；普通 C# 模块沿用相同转换语义。详细边界见 [Razor-to-Vue](../02-architecture/razor-to-vue.md)。

## 常见检查

- 产物位置：确认最终宿主配置了 `JazorMode`；模块类库通过 `ModuleCatalog` 传递生成内容。
- Razor 组件转换：确认项目显式引用 `Jazor.Vue`，并采用支持 Razor Source Generator 的 Razor SDK。
- 外部成员诊断：确认该成员已声明 Jazor 宿主映射，并通过强类型 binding 表达运行时语义。

更多包配置见 [安装与配置](./installation-and-configuration.md)，真实项目组合见 [示例](./examples.md)。
