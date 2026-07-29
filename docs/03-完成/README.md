# 状态与验证

本目录记录阶段性完成情况、评审结果、状态快照和验证证据。“完成”表示某项工作在记录时点达到其文档中定义的范围，并不自动表示整个产品已经终局完成。

## 当前状态入口

| 领域 | 入口 | 说明 |
| --- | --- | --- |
| 编译器 | [compiler/status.md](./compiler/status.md) | 编译器支持边界和当前实现状态 |
| Emit | [emit/status.md](./emit/status.md) | 物化、manifest、源映射和 bundle 状态 |
| Razor-to-Vue | [razorvue/completion-analysis.md](./razorvue/completion-analysis.md) | RazorVue 阶段性完成材料，需结合当前源码核对 |
| ECMAScript.Style | [ecmascript.style/status.md](./ecmascript.style/status.md) | 强类型 API、运行时、Emit、RazorVue、浏览器与 NuGet 验证状态 |
| Vue 3 | [ecmascript.vue3/status.md](./ecmascript.vue3/status.md) | Vue 3 绑定状态 |
| Pinia | [ecmascript.pinia/status.md](./ecmascript.pinia/status.md) | Pinia 绑定状态 |
| Vue Router | [ecmascript.vueroute/status.md](./ecmascript.vueroute/status.md) | Vue Router 绑定状态 |
| Wiki | [wiki/status.md](./wiki/status.md) | Wiki sample 状态 |

## 证据优先级

判断当前行为时，按以下顺序使用证据：

1. 当前源码与公共项目契约；
2. 当前自动化测试和构建输出；
3. 当前目标设计与实施计划；
4. 本目录中的状态快照、审计报告和阶段性记录。

历史审计文档中的通过数量、路径、问题列表和项目名称只代表其记录时点。除非经过当前测试重新确认，不得将其直接引用为当前事实。

## 状态文档要求

状态文档应包含记录日期、覆盖范围、验证命令、实际结果、未覆盖范围和后续风险。避免使用没有证据支持的“全部完成”“全部通过”等绝对表述。
