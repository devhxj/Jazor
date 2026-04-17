# Jazor.VueHost 状态（2026-04-17）

> Status: current status snapshot
> Positioning: Repository-level status snapshot for the active `Jazor.VueHost` development-time boundary.
> Scope: `.jazor` authoring host, LSP, Dev Server/HMR, Source Map pipeline, Debug adapter baseline, and production build lane.

## 总结

`Jazor.VueHost` 已经从“单纯架构搭建”进入“能力收口与补齐”阶段。

当前更准确的描述是：

- Phase 1/2/3：主路径已落地，正在持续修边与稳定性补强
- Phase 4：DAP + CDP 已具备断点/调用栈/scopes/variables/evaluate/continue 基础闭环，且已有真实浏览器/CDP + HMR 长链路压测（env-gated）
- Phase 5：生产构建路径已进入实现与回归覆盖阶段
- Phase 6：高级 LSP（P2 + 部分 P3）已进入可用状态并持续增强
- Phase 7：扩展系统已启动最小闭环（diagnostics/codeAction provider 抽象 + LSP 接入 + 基础加载器）

## 当前状态判断

### 1. Host 边界和 Lane 架构已稳定

- `Jazor.VueHost` 作为唯一 active host 边界已经成型
- In-proc Razor/Roslyn + Deno frontend worker 的组合已进入主干
- LSP 路由、桥接聚合和 workspace resolver 形成了稳定主线

### 2. Dev Server / HMR / SourceMap 已形成闭环主路径

- `--dev` 路径、HMR 通道、workspace 变更去重已具备系统级实现
- `.jazor/.vue/.ts` 的编译与 SourceMap 回传/链式处理已在主路径可用
- SourceMap 已被 DAP/CDP 调试链路消费于断点映射与调用栈回填；`.jazor` 多锚点列映射与 `.vue` 原生 script/template 链式列映射已落地，并有真实浏览器/CDP + HMR 长链路压测
- `resolve.alias` 已接入 `jazor.config.json -> DevServerOptions/BuildOptions -> ModuleResolver`，dev/build 共用解析链路

### 3. 高级 LSP 能力已超过 phase-one 最小范围

- `references` / `rename` / `codeAction` / `documentSymbol` / `semanticTokens` 已进入主线能力
- `.jazor` 与 `.vue/.ts/.js` 之间的 bridge supplement 已有实装与回归用例
- Roslyn lane 已覆盖部分未打开磁盘文档场景（有界扫描）

### 4. 测试覆盖规模已具备“回归网”属性

- `src/Jazor.CompilerTest` 中 `JazorVueHost*Tests` 文件已达到 20+，覆盖 DevServer/LSP/Build/Debug/SourceMap 等主域
- 当前本地工作区存在长时间运行的 `Jazor.VueHost --lsp` 进程时，`dotnet test` 可能出现 dll 锁定失败；验证时需先清理占用进程

## 阶段进展矩阵

| 阶段 | 进展判断 | 说明 |
|------|---------|------|
| Phase 1 Dev Server MVP | 已完成并持续增强 | 核心 HTTP 模块服务与编译路径已可用 |
| Phase 2 Source Map | 高完成度（收尾中） | 行级+列级映射与链式映射已落地，DAP/CDP 消费已接入，仍需持续扩展高压调试矩阵 |
| Phase 3 HMR | 已完成并持续增强 | 工作区变更、去重、style/js/full-reload 分类已实现，`resolve.alias` 已补齐 |
| Phase 4 Debug (DAP + CDP) | 收尾中 | DAP + CDP 已覆盖 stackTrace/scopes/variables/evaluate/continue，并有真实浏览器/CDP+HMR 多轮压测；仍需更高压并发与异常栈矩阵 |
| Phase 5 Production Build | 活跃推进中 | build skeleton 已落地并有较多构建回归测试 |
| Phase 6 Advanced LSP | 高完成度 | P2 基本可用，P3 中 semantic tokens 等已部分落地 |
| Phase 7 Extension System | 已启动（最小闭环） | 扩展核心接口、注册表、加载器与 LSP diagnostics/codeAction provider 接入已落地；IDE 集成与生态层仍待推进 |

## 近期推进信号（截至 2026-04-17）

- 最近一周 `vuehost` 相关提交持续高频，且覆盖 feature/refactor/test/docs 四类
- 最新提交聚焦在：
  - LSP semantic tokens 与 phase6 文档服务增强
  - production build 骨架与 dev server 增强
  - Source Map 服务接口与编译链跟踪

## 下一步行动

1. **Phase 4 调试稳态**
   - 扩展真实浏览器/CDP 压测矩阵（异常栈、并发断点、长时间 HMR）
   - 补调试可视化与诊断端点，降低问题定位成本

2. **Phase 5 构建收口**
   - 继续压实构建产物一致性（manifest/css/js/source map）
   - 补足 build lane 的失败路径与边界场景回归

3. **Phase 6 能力收敛**
   - 继续修正跨 lane supplement 的一致性和保守边界
   - 明确哪些能力是“native first”，哪些由 host 负责补桥接

4. **Phase 7 启动门槛**
   - 在不破坏现有主线稳定性的前提下，继续扩展 provider 面（completion/hover/symbol）
   - 补扩展健康监控、超时与隔离策略，再推进 IDE 生态层集成

## 风险与注意项

- 文档与实现迭代速度不一致时，最容易造成阶段误读；应优先维护 repo-level 状态页
- 调试链路若继续停留在 fallback，会影响 SourceMap 价值验证
- 构建与 LSP 并行演进时，需持续防止“测试路径与真实路径分叉”
