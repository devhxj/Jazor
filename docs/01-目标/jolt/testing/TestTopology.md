# Jolt 测试拓扑设计

> Status: 活跃参考
> Positioning: Jolt 测试分层、实例生命周期和 `.slnx` 边界约定

## 1. 文档定位

本文只描述当前实现已经在用的测试拓扑，不额外发明新的测试框架。目标是把“哪些场景必须启动真实 Jolt，哪些场景只测局部逻辑”固定下来，避免测试互相污染。

## 2. 真实拓扑载体

| 载体 | 位置 | 作用 |
|------|------|------|
| `JoltIntegrationTestTopology` | `src/Jolt.Test/JoltIntegrationTestTopology.cs` | 创建临时根、solution、project，并写入 `.slnx` project entries |
| `JoltIntegrationProjectScope` | `src/Jolt.Test/JoltIntegrationTestTopology.cs` | 包装单项目 topology 生命周期，释放时通过 topology 统一失效 `.slnx` resolver 缓存 |
| `JoltIntegrationRootedProjectDirectory` | `src/Jolt.Test/JoltIntegrationTestTopology.cs` | 兼容仍以 `string tempDirectory` 表达的 rooted 测试，统一跟踪和释放 topology 根目录 |
| `SharedLspTestClient` | `src/Jolt.Test/JoltSharedLspProcessTests.cs` | 用单个 Jolt 进程覆盖多个 workspace roots |
| `CreateTemporaryScopedProject()` | `src/Jolt.Test/JoltTests.cs` | 每用例创建最小 `.slnx + .csproj` 边界 |
| `JoltWorkspaceResolverTests` | `src/Jolt.Test/JoltWorkspaceResolverTests.cs` | 纯 resolver / scoping 规则验证 |
| `JoltLaneRoutingTests` | `src/Jolt.Test/JoltLaneRoutingTests.cs` | 纯路由表验证 |

## 3. 分层约定

| 层级 | 适用测试 | 实例策略 | 说明 |
|------|---------|----------|------|
| 集成 / E2E | `JoltSharedLspProcessTests`、`JoltTests`、需要真实进程的 `JoltLspTests` 用例 | 单场景单实例，多 workspace / solution / project | 一个测试场景只起一份 Jolt 进程或宿主，场景内可以挂多个 workspace roots |
| 生命周期 / 初始化 / 关闭 / 恢复 / 缓存污染 | `JoltTests`、`JoltStdioLspServerTests`、扩展/宿主生命周期相关用例 | 每用例新实例 | 这些用例验证状态边界，复用实例会掩盖泄漏和竞态 |
| Resolver / routing | `JoltWorkspaceResolverTests`、`JoltLaneRoutingTests` | 轻量单元测试 | 只测路径、边界和路由，不为纯规则启动真实 Jolt 进程 |
| Fixture 生成 | `JoltIntegrationTestTopology` | 每场景独立临时目录 | 不做 assembly 级共享根 |

## 4. 设计规则

- 集成 / E2E 优先使用“单测试场景一个 Jolt 实例，多 workspace / solution / project”的拓扑。
- 生命周期、初始化、关闭、恢复、缓存污染等强状态用例必须保持每用例新实例。
- Resolver / routing 继续保持轻量单元测试。
- 禁止 assembly 级全局 Jolt singleton。
- `.slnx` 是唯一 solution 边界，`.sln`、目录邻近和 `*.csproj` 猜测都不算。

## 5. 现有 helper 的真实行为

- `JoltIntegrationTestTopology.Create(...)` 为每个场景创建独立临时根。
- `CreateSolution(...)` 会生成一个 solution root，并写出 `.slnx`。
- `AddProject(...)` 会创建真实的 `*.csproj`，再回写 `.slnx` project entries。
- `JoltIntegrationTestTopology.Dispose()` 会统一失效当前 topology 创建过的全部 `.slnx` resolver 缓存，再删除 topology 根目录。
- `WriteFile(...)` 总是从 project root 落盘，保证测试文件天然处于 owning project 边界内。
- `JoltIntegrationProjectScope.CreateSingleProject(...)` 用于 `using var` 形式的 scoped project 测试，`Dispose()` 通过底层 topology 统一失效 `.slnx` resolver 缓存并删除 topology 根目录。
- `JoltIntegrationRootedProjectDirectory.Create(...)` 用于遗留 `string tempDirectory` 形态的 rooted 测试；清理时必须先调用 `TryDispose(...)`，只有未命中 tracked topology 时才允许退回普通目录删除。
- `SharedLspTestClient.InitializeAsync(params string[] workspaceRoots)` 会把多个 workspace roots 一次性传入同一个 Jolt 进程。

## 6. 可验证约束

- 同一个 Jolt 实例同时服务多个 solution 时，彼此不应串扰。
- 同一个 `.slnx` 下的 sibling project 不能被隐式发现逻辑跨过去。
- 缺少 `.slnx` 时，项目级发现必须返回英文错误，而不是退回磁盘猜测。
- 任何缓存、打开文档和临时目录都应在用例结束后清理，不得依赖 assembly 级共享状态。

## 7. 英文错误示例

```text
No solution .slnx was found for '<documentPath>'. Open the project from a solution directory that contains a .slnx file.
```
