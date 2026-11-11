# ECMAScript.WebIDL.Generator

> 定位：WebIDL 生成管线的 .NET 宿主，负责采集 WebRef inventory 并生成预览 C# binding。

## 职责

- 使用 `Microsoft.JavaScript.NodeApi` 与 `libnode` 在 C# 进程中恢复依赖并执行 collection worker，全程不启动 Node、pnpm、tsc 或 shell 子进程。
- 从 `webref`、`webidl2` 和 `@webref/xref` 收集稳定 JSON inventory，写入 `src/ECMAScript/webidl`。
- 为 `typedef`、`enum`、`callback`、`dictionary`、`interface` 和 `namespace` 生成预览 C# binding。
- 将 W3C/WHATWG 规范定义的精确锚点、章节、作者写作的正文和可用的规范内用法写入生成 XML 文档，便于 IDE 中直接理解 API 语义并定位规范。

## 运行

本工具是 NodeApi 迁移试点，目前在 Windows x64 / .NET 11 验证。固定组合为 `Microsoft.JavaScript.NodeApi 0.9.36`、`Microsoft.JavaScript.LibNode 20.1800.215`、`@pnpm/napi 12.5.1` 和 `TypeScript 5.9.3`；不依赖本机全局 Node 或 pnpm。该组合不代表整个 Jazor 已迁移，也不代表已获得生产运行时或其他平台的支持证据。

首次采集时，C# 从 npm registry 引导下载固定版本的 pnpm loader 与当前平台原生包，并校验 registry 提供的 SHA-512。引擎缓存位于 `node/obj/pnpm-engine/`；随后在内嵌 Node 中直接调用 pnpm `install` API，从 `node/package.json` 恢复应用依赖。`node/pnpm-lock.yaml` 是权威锁文件，有 lock 时必须 frozen，失败不自动重新解析。共享 store 位于仓库 `.tmp/webidl-pnpm-store/`。

WebRef 与 TypeScript 均为本工具的固定纯 JS 依赖，恢复显式禁用 lifecycle scripts。collector 由 TypeScript `transpileModule` 在进程内转译为标准 ESM，写入 `node/obj/collector/`，随后通过动态 import 调用；没有 `tsc` 或 Node CLI 执行步骤。生成器的既有 `--from-inventory` 路径仍不加载 NodeApi、不恢复 npm 依赖。

采集并生成：

```bash
dotnet run --project src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj -- --out src/ECMAScript/webidl
```

只根据已提交 inventory 再生，不访问 WebRef：

```bash
dotnet run --project src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj -- --out src/ECMAScript/webidl --from-inventory src/ECMAScript/webidl/webidl.inventory.json
```

规范源码正文默认抓取，因为它是生成 XML 文档的重要来源；单个源码请求失败不会阻塞 inventory 和绑定生成。离线或诊断运行可传入 `--skip-spec-prose`，或设置 `JAZOR_SKIP_SPEC_PROSE=1`。

通过内嵌环境执行 collector 文档匹配与 frozen lock 测试：

```text
dotnet run --project src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj -- --test-collector
```

完整试点验证在同一个 C# 进程中进行采集、生成、collector 测试、PID 比较、frozen lock 不变检查、异步 JS 错误传播和多次 environment 创建/释放：

```text
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
```

如需对照旧运行时已保存的固定 inventory，将其路径作为脚本的唯一参数。脚本忽略 `generatedAt` 比较 typed inventory，并用两份 inventory 分别生成 C#，逐文件比较字节；报告写入 `artifacts/webidl-nodeapi/`。正式采集不使用该基线，也不保留 Deno 执行 fallback。

`--worker <collect.ts>` 可选择 collector；`--node-project <package.json>` 可指定其项目配置，同目录需有 `host.cjs`。旧 `--deno-config` 已移除并给出迁移诊断。

## 已验证组合的限制

- Libnode 20 的 V8 栈切换与 Windows .NET 9+ 默认 CET 设置不兼容。该工具项目及验证脚本显式设置 `CETCompat=false`，不修改仓库其他应用或机器设置。其他调用宿主也必须使用兼容的启动配置。
- 该 libnode 对 pnpm Rust 引擎生成的 Windows junction 解析失败，试点采用 pnpm 官方 `hoisted` 布局。不能据此宣称默认 `isolated` 已通过验证。
- `@pnpm/napi 12.19.0` 可以加载，但它的用户级 store operation lock 目录在当前受限执行环境中无法写入。试点固定使用已完成实际安装的 `12.5.1`，不会在失败后自动切换版本。
- NodeApi 的 JS 线程亲和性和 callback scope 生命周期分别处理：宿主通过 `RunAsync` 调度，并以 `JSReference` 跨 `await` 保持模块引用。
- pnpm 安装 API 没有取消参数。取消 token 在 bootstrap 网络阶段生效；进入安装/采集后等待实际工作完成，再观察取消，不把取消等待当作终止原生操作。本工具暂未承诺即时中断正在运行的安装。
- 上游提供的 LibNode 仍为 Node 20 系列；获得持续维护的 Node/libnode 组合与其他平台证据，是迁移产品主线前必须完成的工作。

## 边界

`src/ECMAScript.WebIDL` 是归档的 legacy TypeScript generator，不参与当前构建。该项目不以旧 TypeScript 输出作为当前 WebIDL contract 的唯一依据。

生成器不会根据 WebIDL 类型形状编造诸如“Represents”或“Invokes”的摘要。`@webref/xref` 提供定义锚点，缺少 xref prose 时，worker 仅从该规范在 W3C/WHATWG GitHub 仓库中记录的源码中提取同一锚点或章节附近的作者正文；网络暂时不可用时不会伪造说明。若没有可验证的规范定义，输出将省略 XML 文档；已匹配项始终保留其规范链接。

## 相关文档

- [ECMAScript](../ECMAScript/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
