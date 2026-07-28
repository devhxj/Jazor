# ECMAScript.Vue3

`src/ECMAScript.Vue3/` 相关的活跃实施计划与执行切片。
这里的计划区当前服务于 Vue host binding surface。RazorVue 的生产输出统一为 render-function `.mjs`，不在本目录维护独立的 SFC artifact 计划。
所有实施计划都基于 `01-目标/ecmascript.vue3/` 下的设计边界与覆盖矩阵展开。

## 文件索引

| 文件 | 说明 |
|------|------|
| `ECMAScript.Vue3.Authoring.ImplementationPlan.md` | `H(...)` authoring、RazorVue 映射与交付验证计划 |
| `ECMAScript.Vue3.RemainingWorkChecklist.md` | 当前主线剩余工作、下一阶段设计项与非目标边界 |
