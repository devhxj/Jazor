# 目标

每个子目录对应仓库中的一个核心模块或一条核心工作线，记录它**为什么存在**、**解决什么问题**、以及**推荐的实现边界**。

## 技术线路

| 线路 | 模式 | 文档目录 | 当前物理落点 |
|------|------|---------|-------------|
| **Razor-to-Vue 架构转型** | 当前唯一主线 | `razorvue/` | 官方 Razor SG generated C# -> Roslyn `IOperation` -> Vue render-function `.mjs` |
| **Jolt** | 历史（已退役） | `jolt/` | 源码见 Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` |
| **Jazor CSX Frontend** | 历史探索 | `csx/` | 当前不作为转型分支执行路线 |

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `compiler/` | `src/Jazor.Compiler/` | 编译器架构、转换管线、主线约束 |
| `compiler/semantic-walker/` | `src/Jazor.Compiler/` | 各操作类型的 lowering 规则 |
| `compiler/sourcemap/` | `src/Jazor.Common/SourceMaps/` + `src/Jazor.Emit/SourceMaps/` | SourceMap 设计与物化约束 |
| `compiler/emit/` | `src/Jazor.Emit/` | 发射、物化、打包与差分输出 |
| `clr/` | `src/Jazor.CLR/` + Generator | CLR 映射声明与白名单生成链路 |
| `analyzer/` | `src/Jazor.Analyzer/` | 静态代码分析、Razor SG hook/bootstrap 与兼容性诊断 |
| `razor/` | historical boundary note | 独立 Razor 基础层已退役；当前通过 official Razor SG generated C# 进入生产链路 |
| `ecmascript/` | `src/ECMAScript/` | ECMAScript host、host/nullish 边界与 WebIDL 相关目标 |
| `ecmascript.vue3/` | `src/ECMAScript.Vue3/` | ECMAScript.Vue3 外部库映射目标、覆盖矩阵与模块规则 |
| `ecmascript.vben/` | `src/ECMAScript.Vben/` | 后台壳层 authoring 目标、UI 适配分层与后台框架语义边界 |
| `ecmascript.pinia/` | `src/ECMAScript.Pinia/` | ECMAScript.Pinia 外部库映射目标、Pinia API 覆盖矩阵与 authoring 边界 |
| `ecmascript.vueroute/` | `src/ECMAScript.VueRoute/` | ECMAScript.VueRoute 外部库映射目标、Vue Router API 覆盖矩阵与 authoring 边界 |
| `razorvue/` | `src/Jazor.RazorVue/` + `src/Jazor.Analyzer/` + `src/Jazor.Compiler/` + `src/Jazor.Emit/` | SG 结果绑定、compiler-owned lowering 与 Vue render-function artifact framing |
| `jolt/` | Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` | 已退役 `.jazor` 宿主的历史目标资料 |
| `csx/` | 无当前源码落点 | 历史 TSX-like `.jazor` 前端探索，不是当前路线 |
| `common/` | `src/ECMAScript.Contract/` + `src/Jazor.Common/` | 最小契约层与共享实现层 |
| `webidl/` | `src/ECMAScript.WebIDL.Generator/` | WebIDL 到 C# 绑定生成 |
| `tools/` | `src/Jazor/` | NuGet 打包与工具侧边界；编辑器集成仅作历史说明 |

## 保留在源码旁的文档

| 位置 | 说明 |
|------|------|
| `src/Jazor.CLR/doc/*.md` | CLR 模块签名与映射参考，和源码紧耦合，保留在模块旁 |
