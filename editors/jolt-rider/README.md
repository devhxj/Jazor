# Jolt for JetBrains Rider

[中文](#中文) | [English](#english)

## English

1. Download `jolt-rider-2.7.0.zip` from the [Jazor 2.7.0 release](https://github.com/devhxj/Jazor/releases/tag/v2.7.0).
2. In Rider 2026.3, open **Settings → Plugins**, use the gear menu to select **Install Plugin from Disk**, and choose the ZIP file. Restart Rider if prompted.
3. In a project with a local Jolt tool manifest, run `dotnet tool restore` from the manifest’s scope. Otherwise install the matching CLI with `dotnet tool install --global Jazor.Jolt --version 2.7.0`.
4. Open the project and a `.jazor` file. The plugin starts Jolt’s language server from the nearest npm workspace. It uses the local tool when the manifest declares `jolt`, or the global tool otherwise.

The plugin provides Jolt language support for `.jazor`. Ordinary `.cs` files continue to use Rider’s C# tooling. The 2.7.0 CLI ships prebuilt runtimes for Windows and Linux on x64 and Arm64. A prebuilt macOS runtime is unavailable in this release.

## 中文

1. 从 [Jazor 2.7.0 发布页](https://github.com/devhxj/Jazor/releases/tag/v2.7.0) 下载 `jolt-rider-2.7.0.zip`。
2. 在 Rider 2026.3 中打开 **Settings → Plugins**，从齿轮菜单选择 **Install Plugin from Disk**，选中 ZIP。按提示重启 Rider。
3. 如果项目的本地工具清单声明了 Jolt，在清单作用域内运行 `dotnet tool restore`。也可以运行 `dotnet tool install --global Jazor.Jolt --version 2.7.0` 安装同版本全局 CLI。
4. 打开项目和一个 `.jazor` 文件。插件从最近的 npm 工作区启动 Jolt 语言服务。工具清单声明 `jolt` 时使用本地工具，其余情况使用全局工具。

插件为 `.jazor` 提供 Jolt 语言支持。普通 `.cs` 文件继续由 Rider 的 C# 工具处理。2.7.0 CLI 提供 Windows 和 Linux 的 x64、Arm64 预构建运行时。本次发布未提供 macOS 预构建运行时。
