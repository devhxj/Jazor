# Jolt Solution / Project Scoping

> 状态：设计约定
> 定位：Jolt 的解决方案边界、项目归属和跨项目限制模型

## 1. 文档定位

本文档定义 Jolt 的 `slnx` 作用域模型。Jolt 可以在一个进程里同时服务多个解决方案；每个解决方案的边界只由 `.slnx` 决定。项目归属从 `.slnx` 的 project entries 反推，不能再用 `.sln`、`*.csproj` 或“离文件最近”的目录猜测来代替。

这个模型的目标是把隐式行为限制在正确的 owning project 内：

- 组件/关联文档的隐式发现只在 owning project 内展开
- HMR 影响面只在 owning project 内传播
- 诊断刷新只在 owning project 内重算和回传
- 当当前位置找不到 `.slnx` 时，项目级发现必须失败，并返回友好的英文错误

用户可见错误必须使用英文；代码注释可以继续使用中文。

## 2. 作用域层级

| 层级 | 约束来源 | 说明 |
|------|----------|------|
| Jolt instance | 进程生命周期 | 一个 Jolt 实例可以承载多个解决方案 |
| Solution | `.slnx` | 解决方案边界只认 `.slnx` |
| Owning project | `.slnx` project entries | 文档归属从解决方案图谱中解析 |
| Document graph | project-local file set | 隐式发现、刷新和 HMR 都只看这个集合 |

## 3. 解析规则

### 3.1 解决方案发现

Jolt 在做项目级发现时，先向上查找 `.slnx`。找到以后，当前目录树才进入解决方案作用域。

如果向上查找后仍然找不到 `.slnx`，项目级发现必须停止，不得继续退回到 `*.csproj`、`*.sln` 或任意磁盘目录推断。

当前实现的用户错误为：

```text
No solution .slnx was found for '<documentPath>'. Open the project from a solution directory that contains a .slnx file.
```

### 3.2 项目归属

Owning project 由 `.slnx` 中的 project entries 决定。

这意味着：

- 文档不属于“最近的文件夹”
- 文档不属于“最近的 project 文件”
- 文档只属于解决方案图中实际声明它的项目

如果一个文件在多个项目中都可见，隐式路径仍然只绑定到当前文档的 owning project。

### 3.3 隐式发现边界

所有隐式发现都必须先拿到 owning project，再只在该项目的 document graph 内展开：

- import / component discovery
- related document discovery
- open document scan
- workspace symbol 的 project-local 解析

跨项目文件可以被显式引用，但不能被隐式发现逻辑自动跨过去。

### 3.4 HMR 和诊断刷新边界

当文件变化时，Jolt 只刷新 owning project 的受影响集合：

- HMR 只向 owning project 的依赖图传播
- 诊断刷新只重算 owning project 的相关文档
- sibling project 的诊断和更新保持不变，除非它们自己的文件也发生变化

这条规则的目的不是限制一个 Jolt 实例的能力，而是避免把局部变更误扩散成工作区级广播。

## 4. 设计约束

- `.slnx` 是唯一的解决方案边界
- owning project 必须来自 `.slnx`，不能靠启发式猜测
- 隐式发现必须保持 project-local
- 用户可见错误必须是英文
- 代码注释可以保持中文，不影响错误文案语言要求

## 5. 建议的验证点

- 同一个 Jolt 实例同时打开多个解决方案时，彼此不应串扰
- 缺少 `.slnx` 时，项目级发现应返回英文错误而不是空结果
- 一个项目内的变更不应触发 sibling project 的隐式发现、HMR 或诊断刷新

## 6. 测试拓扑约定

Jolt 测试应收敛的是集成拓扑，而不是把整个测试框架收敛成 assembly 级全局单例。

推荐分层如下：

- 集成 / E2E 测试优先使用“单测试场景一个 Jolt 实例，多 workspace / solution / project”的拓扑。这个拓扑最接近生产，可以覆盖同一实例内的项目隔离、路由、诊断刷新和 HMR 影响面。
- 生命周期、初始化 / 关闭、恢复、进程清理、缓存污染测试必须保留“每用例新实例”。这些测试关注状态边界，复用实例会掩盖问题或制造 flaky。
- Resolver / routing 逻辑继续保持轻量单元测试，不需要为了验证纯函数或路径规则而启动真实 Jolt 进程。
- 不要引入 assembly 级全局 Jolt 单例。全局单例会放大缓存泄漏、打开文档残留、端口 / pipe 抢占和并发顺序依赖。

测试 helper 应表达真实拓扑：

- `CreateSlnxSolution(...)` / `CreateSolution(...)`：创建 solution root 和 `.slnx`
- `AddProject(...)`：向 `.slnx` 写入 project entry，并返回 project root
- `StartJoltScenarioHost(...)` / LSP client：每个测试场景启动一个 Jolt 实例，再初始化多个 workspace folder

这保证测试覆盖的是“一个实例服务多个清晰边界”，而不是“所有测试共享一个状态容器”。
