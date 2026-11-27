# Jolt for JetBrains Rider

[中文](#中文) | [English](#english)

## English

1. Download `jolt-rider-2.7.4.zip` from the [Jazor 2.7.4 release](https://github.com/devhxj/Jazor/releases/tag/v2.7.4).
2. In Rider 2026.3, open **Settings → Plugins**, use the gear menu to select **Install Plugin from Disk**, and choose the ZIP file. Restart Rider if prompted.
3. When a .NET project selects Jolt, the plugin uses the same package version or source apphost as MSBuild. Build a referenced source apphost before opening the editor. If the project instead declares Jolt in a local tool manifest, run `dotnet tool restore` from the manifest’s scope.
4. Open the project and a `.jazor` file. A standalone workspace without a project tool or manifest can use a globally installed Jolt.

The plugin provides Jolt language support for `.jazor`. Ordinary `.cs` files continue to use Rider’s C# tooling. The 2.7.4 CLI ships prebuilt runtimes for Windows and Linux on x64 and Arm64. A prebuilt macOS runtime is unavailable in this release.

## 中文

1. 从 [Jazor 2.7.4 发布页](https://github.com/devhxj/Jazor/releases/tag/v2.7.4) 下载 `jolt-rider-2.7.4.zip`。
2. 在 Rider 2026.3 中打开 **Settings → Plugins**，从齿轮菜单选择 **Install Plugin from Disk**，选中 ZIP。按提示重启 Rider。
3. .NET 项目选定 Jolt 时，插件和 MSBuild 使用同一包版本或源码 apphost。若项目引用源码 apphost，先构建该引用。项目通过本地工具清单声明 Jolt 时，在清单作用域内运行 `dotnet tool restore`。
4. 打开项目和一个 `.jazor` 文件。没有项目工具或工具清单的独立工作区可以使用全局安装的 Jolt。

插件为 `.jazor` 提供 Jolt 语言支持。普通 `.cs` 文件继续由 Rider 的 C# 工具处理。2.7.4 CLI 提供 Windows 和 Linux 的 x64、Arm64 预构建运行时。本次发布未提供 macOS 预构建运行时。
