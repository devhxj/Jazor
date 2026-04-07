# Architecture

本目录只承接长期有效的参考入口，不负责当前执行导航。你如果是来继续往前推开发，先去看 `docs/workstream-dashboard.md`，莫在这儿找当天的推进安排。

## 仓库级长期参考
- [Compiler 文档桥接入口](./compiler/README.md)
- [Modules Bridge](./modules/README.md)
- [ECMAScript host alignment](../ECMAScript-host-alignment.md)
- [ECMAScript nullish semantics](../ECMAScript-nullish-semantics.md)

## 说明
- 仓库级 `docs/architecture/` 负责统一导航和收口
- 子系统如果已经有成熟的局部索引，优先通过桥接入口接入
- `src/Jazor.Compiler/doc/` 继续作为 compiler 深度文档的原位入口
- 本目录偏长期有效参考，不承担 active execution 导航；执行入口请看 [docs/workstream-dashboard.md](../workstream-dashboard.md)
