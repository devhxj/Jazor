# Jazor.VueHost 文档分层说明

## 目的

`Jazor.VueHost` 现在同时存在两类文档：

- 一类描述 **最终能力蓝图**
- 一类描述 **当前单项目实施约束**

这两类文档都有效，但职责不同，不能互相替代。

---

## 文档职责

### 1. `vuehost-capabilities.md`

文件：
[vuehost-capabilities.md](./vuehost-capabilities.md)

用途：

- 描述 `Jazor.VueHost` 的 **完整目标宿主形态**
- 定义最终能力边界，包括：
  - 全前端 LSP
  - Dev Server
  - 编译管道
  - Source Map
  - 调试支持
  - 多协议接入
- 这是 **长期规划文档**

阅读方式：

- 按 target architecture 阅读
- 这里允许出现当前尚未全部落地的能力
- 不应把它误读成“仓库今天已经全部实现”

### 2. `jazor-vuehost-single-project.md`

文件：
[jazor-vuehost-single-project.md](./jazor-vuehost-single-project.md)

用途：

- 描述 `Jazor.VueHost` 的 **当前主实施路线**
- 明确哪些原则已经锁定，哪些旧路线必须删除
- 约束当前代码推进的实现方向，包括：
  - `.jazor` 是 Razor-first
  - `Jazor.VueHost` 是唯一宿主边界
  - IntelliSense 与 build/materialization 分阶段
  - lane-based host
  - internal virtual artifacts
  - Deno only

阅读方式：

- 按 implementation contract 阅读
- 这里优先约束“现在怎么实现”
- 这里不要求一次性承载 `vuehost-capabilities.md` 中全部终局能力

---

## 二者关系

可以把这两份文档理解为：

- `vuehost-capabilities.md` = **目标全景图**
- `jazor-vuehost-single-project.md` = **当前施工蓝图**

两者不冲突，关系如下：

1. `vuehost-capabilities.md` 负责回答：
   - VueHost 最终要成为什么
   - 最终应该提供哪些能力
   - LSP、Dev Server、编译、Source Map、调试如何统一进入同一宿主

2. `jazor-vuehost-single-project.md` 负责回答：
   - 当前实现必须遵守什么边界
   - 哪些历史路线已经废弃
   - 智能感知阶段应先建立哪些基础设施

3. 当前代码修改应优先服从：
   - `jazor-vuehost-single-project.md`

4. 中长期架构演进不应偏离：
   - `vuehost-capabilities.md`

---

## 当前阶段解释

当前仓库实现更接近下面这个阶段：

- 已进入：
  - 单项目 `VueHost`
  - LSP
  - workspace graph / resolver
  - `.jazor <-> .vue` 设计时关系
  - lane routing
  - internal projections

- 尚未完整进入：
  - 完整 Dev Server 形态
  - 完整编译管道统一入口
  - 完整 Source Map 服务
  - DAP / CDP 调试闭环

这不表示这些能力被否定，只表示它们仍属于 `vuehost-capabilities.md` 所描述的后续宿主建设阶段。

---

## 使用原则

当出现文档阅读或实现分歧时，按下面顺序判断：

1. 是否违反 Razor-first、single-host、Deno-only、stage separation 这些硬约束
2. 若不违反，当前代码推进优先参考 `jazor-vuehost-single-project.md`
3. 若讨论的是 VueHost 最终应承担的能力边界，参考 `vuehost-capabilities.md`
4. 不再引入 RazorVue 路线、split-host 路线、Vite/Bun 路线作为回退方案

---

## 一句话总结

`vuehost-capabilities.md` 不是过度设计文档，而是 `Jazor.VueHost` 的终局能力蓝图。

`jazor-vuehost-single-project.md` 不是替代它，而是把当前代码实现约束在一条与该蓝图一致、但更聚焦于当下阶段的落地路径上。
