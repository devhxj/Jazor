# SourceMap 状态（2026-04-06）

> Status: current status snapshot
> Positioning: Repository-level status snapshot for the SourceMap programme.
> Note: this snapshot predates the `Jazor.VueHost` consolidation; read the RazorVue wording as historical lane context, not a separate active boundary. Current active host/runtime wording should be interpreted as `Jazor.VueHost` plus Deno.

## 总结

SourceMap 当前不能再用一句"deferred"概括了。

更准确地说：

- broad SourceMap programme 仍然偏保守
- 但由 RazorVue 演进出来、并由 VueHost / Deno 承接的 bundle chaining 已进入 narrower active lane
- repo-level 文档需要同时表达这两层现实，不能二选一

## 当前状态判断

### 1. Broad programme remains conservative

当前仍不应该把 SourceMap 描述成"全线开工"。

Broad programme 依然强调：

- compiler main path stability first
- module-level map first
- broad rollout should not outrun upstream stability

这意味着 broad SourceMap programme 当前仍然是：
- 以规划和设计为主
- 不急于全面铺开实现
- 优先保证上游 compiler / emit 的稳定性

### 2. Narrower active lane is already real

当前已经存在一个 narrower active lane：

- VueHost / Deno emitted module sourcemap
- writer / manifest evolution
- bundle chaining continuation

所以更准确的状态是：

- broad programme: conservative
- narrow lane: active

这个 narrow lane 是因为 RazorVue 的实际需求而启动的，不是为了 SourceMap 本身的全面推广。

### 3. SourceMap is now a coordination lane, not only a future note

SourceMap 当前已经和以下工作流直接耦合：

- compiler artifact / source-origin shape
- emit writer / manifest / bundler evolution
- RazorVue host-facing materialisation

这意味着 repo-level 文档必须显式桥接它，而不能只把它留在局部专题入口里。

## 下一步行动

### Narrow active lane（优先级最高）

**目标**：完成 RazorVue 需要的 narrow SourceMap slice

**具体行动**：

1. **完成 RazorVue bundle chaining 实现**
   - 让 module sourcemap 在 bundle chain 中正确传递
   - 确保 source-origin 信息不丢失
   - 参考：[2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)

2. **让 writer / manifest 演进就位**
   - 支撑 sourcemap 的写出和持久化
   - 保持和 emit pipeline 的一致性

**验证标准**：
- RazorVue 构建产物有正确的 sourcemap
- Source map chain 能够正确追溯到原始源文件
- 不影响 broad programme 的保守基调

### 维持 broad programme 和 narrow lane 的边界

**目标**：确保 narrow lane 的活跃不掩盖 broad programme 的保守性质

**具体行动**：

1. **持续更新文档**
   - 明确区分 narrow active lane 和 broad programme
   - 避免"narrow lane 活跃 = broad programme 活跃"的误解

2. **保持 scope 控制**
   - Narrow slice 保持比 broad programme 更窄
   - 不因为 narrow lane 成功就提前扩大 scope

**参考文档**：
- [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)
- [SourceMap.ImplementationChecklist.md](../../src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md)

### Broad programme 准备（低优先级）

**目标**：为未来 broad SourceMap rollout 做准备

**具体行动**：

1. **继续完善设计和规划**
   - 但不急于实现
   - 保持对上游依赖（compiler / emit）的观察

2. **等待上游稳定**
   - 在 compiler mainline 和 emit 更稳定之前，不扩大 scope

## 深度文档

- [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)
- [SourceMap.ImplementationChecklist.md](../../src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md)

## 当前缺口

- Repo-level SourceMap 状态入口刚建立，还没形成长期稳定阅读习惯
- Broad programme 和 narrow lane 的边界需要持续维护
- SourceMap 仍然容易被误写成"全 deferred"或"全 active"
