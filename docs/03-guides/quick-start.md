# 编译第一个 C# 模块

> 目标：用最小的两项目结构验证 Jazor 核心的 C# -> ECMAScript 模块路径。

本指南用两个 .NET 项目演示如何将类库中的 C# 模块生成 `.mjs` 并写入宿主输出目录。编写 Vue 应用请看 [Jolt 作者指南](./jolt-authoring.md)。

示例使用 `2.5.0`。Jolt 项目通过 npm 依赖使用应用库，普通 .NET 类库可用 `jolt bindings` 生成公开 C# 声明。 [投影用法](./jolt-authoring.md#普通-c-类库的公开投影)

## 1. 创建模块库

创建一个类库项目并安装 `Jazor`：

```bash
dotnet new classlib -n Sample.Modules
dotnet add Sample.Modules package Jazor --version 2.5.0
```

在类库中声明一个 ECMAScript 模块：

```csharp
using ECMAScript;

namespace Sample.Modules;

[ECMAScriptModule("features/greetings.js")]
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
dotnet add Sample.Host package Jazor --version 2.5.0
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

构建后，MSBuild 在最终 `Exe`/`WinExe` 宿主调用 `Jazor.Emit`，在 `JazorDir` 生成标准项目。Emit 通过进程内 pnpm API 恢复 `node_modules` 与 `pnpm-lock.yaml`；Release 通过 Vite API 构建 `dist/`，无需全局 Node/pnpm 或单独运行 JS 工具进程。类库在 DLL 内携带 `Jazor.Generated.ModuleCatalog`，最终宿主拥有项目目录；跨模块调用仍使用稳定的标准 ECMAScript import。

诊断 manifest 仅保存在 `obj`，不参与运行时解析。发布只复制 `dist/**` 和启用后的 `ssr/**`，源码、根 lock 与 `node_modules` 留在构建项目中。source map 默认不发布，需要时设置 `JazorPublishSourceMaps=true`。从 1.0.0 升级的运行时迁移步骤见[安装与配置](./installation-and-configuration.md)。

## 4. 编写组件

编写 `.jazor` 组件、引入 npm 组件或配置编辑器，见 [Jolt 作者指南](./jolt-authoring.md)。

## 常见检查

- 产物位置：确认最终宿主配置了 `JazorMode`；模块类库通过 `ModuleCatalog` 传递生成内容。
- 外部成员诊断：确认该成员已声明 Jazor 宿主映射，并通过强类型 binding 表达运行时语义。

更多包配置见 [安装与配置](./installation-and-configuration.md)，真实项目组合见 [示例](./examples.md)。
