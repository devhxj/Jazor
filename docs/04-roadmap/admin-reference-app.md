# JazorAdmin 生产级参考应用路线图

> 适用范围：`samples/JazorAdmin` 的产品定位、模块边界、界面设计指导与验收路线。`src/Jazor.Admin` 库契约不因本路线变更；应用层 UI 直接通过 `ECMAScript.TDesign` 的强类型组件表达。

## 目标

把 JazorAdmin 从"dogfood 示例 + 上游模板复刻"收敛为**生产级参考应用**：中小型团队可以整体照抄的登录中心门户、资源管理与生产页面范式。验收维度有三个：

1. 单点登录中心门户成立：存在真实下游客户端跑通完整 OIDC 流程（授权码 + PKCE → 回调 → 令牌 → 调受保护 API → 单点登出）。
2. 资源管理完整：组织机构、配置管理、角色权限、账号、调度、通知，加上操作审计日志。
3. 界面为单一设计语言：内容层回归 TDesign 组件体系，页面范式可复制、可截图对比。

## 边界规矩（防撞车）

- 平台 IAM 只管身份（谁是谁）与平台资源（谁能进哪个应用、谁能管平台配置）。下游应用的业务权限归下游应用自己，只消费 OIDC claims；**禁止把下游业务权限注册为 JazorAdmin 的 resource operations**。
- 门户与管理台共用一个导航体系，靠现有 resource-operation 授权区分可见性：普通用户看应用入口，管理员看管理模块。
- `Features/*`（各自 Contracts + Endpoints）是未来拆分的天然缝隙；现在合着用，但不得让模块间直接引用对方的内部类型。

## 界面现状诊断

丑的根因不是缺少设计资源，而是**组件语言断层**：

- 壳层（`TDesignLayout` / `TDesignSidebarMenu` / `RouteTabs` 等）使用真实 TDesign 组件（`TMenu`、`THeadMenu` 等）。
- 所有页面内容层（真实管理模块的 `ja-management__*` 与 M1 已退役复刻页的 `ja-starter__*`）是原生 HTML 加手写 CSS：`Styles.cs` 约 3000 行，`StarterStyles.cs` 在 M1 收缩后仍保留约 500 行壳层/仪表盘/外观抽屉规则；内容层仅零星使用 `TIcon`。
- 两套页面 CSS 风格不一致，且都丢失了 TDesign 组件自带的 hover/focus/过渡/密度控制，形成"壳精致、内容廉价"的断层观感。

内容层所需组件在 `ECMAScript.TDesign` 绑定中全部可用：`TTable`、`TForm`、`TInput`、`TSelect`、`TDialog`、`TPagination`、`TLoading`、`TSkeleton`、`TEmpty`、`TPopconfirm`、`TSteps`、`TDescriptions`、`TMessage`、`TNotification`、`TTag`、`TTabs`、`TSwitch`、`TUpload` 等。`TResult` 未在上游绑定中提供，结果页由图标 + 排版组合。

## UI 技术选型与分工

参考应用的界面栈由三个绑定组成，职责单一、互不替代：

| 绑定 | 职责 | 使用规则 |
| --- | --- | --- |
| `ECMAScript.TDesign` | 结构与交互组件（表格、表单、弹窗、导航、反馈） | 页面内容层唯一的结构组件来源；TDesign 组件内部图标随组件走，不改写 |
| `ECMAScript.VueDataUi` | 图表与数据可视化 | 所有图表一律用 `VueUi*` 强类型 Dataset/Config；禁止手绘 CSS/SVG 假图表；图表必须放在有确定高度的容器内（`Responsive = true` 只解析宽度） |
| `ECMAScript.VuIcons` | 应用自有图标面（导航图标、空态、功能入口、结果页大图标） | 已知图标用静态 `Vu*` 组件；运行时选择用 `VuIcon` + 闭合 `VuIconName` enum；禁止把图标名写成裸 string 传给组件 |

三者均为按需 ESM entry（单图表、单图标），`Jazor.Emit` 只物化实际引用的闭包，不使用聚合入口。图表 authoring 范式参考 `samples/ECMAScript.VueDataUi.Dashboard`。管理页常用图表选型：KPI 数字卡 `VueUiKpi`、趋势 `VueUiXy` / 表内迷你 `VueUiSparkline`、占比 `VueUiDonut`、排名对比 `VueUiHorizontalBar`。

## 里程碑

### M1 定位与信息架构收敛（已完成）

- 重写 `samples/JazorAdmin/README.md` 定位为生产级参考应用；本路线图即规划文档。
- 退役 `StarterCatalog` 的 22 个上游模板复刻页与 result 页品牌资产。实施时确认的边界：壳层（页头搜索/通知/用户区）、外观抽屉（`StarterSettings` + TDesign 控件 + setting 品牌资产）和仪表盘指标卡复用 Starter 复刻期的样式与组件，因此 `StarterStyles.cs` 收缩为壳层 + 仪表盘 + 外观抽屉最小规则集而不是整体删除，命名与类名（`ja-starter-*`）收敛留给 M2。属破坏性变更，走 MINOR 版本并在 CHANGELOG 写迁移说明。
- 导航信息架构重组为分区：工作台（直达）、身份与访问（组织/授权/账号/SSO）、平台运营（配置/调度）。IconBar 承载分区，次级菜单承载模块与页面；“门户工作台”分区语义随 M4 落位；“开发者参考”最终未设独立导航分区，以本路线图“页面范式（开发者参考契约）”承载可复制范式。
- 验收：`verify-smoke` 通过（分区导航断言、IconBar 分区数、英文切换迁移至真实模块页）；`docs/03-guides/examples.md` 与 `docs/01-overview/product-scope.md` 已同步。

### M2 界面设计系统落地（已完成）

- 组件 spike 结论：`TTable`（含 `Cell` 渲染片段、`RowClassName` 选中态）、`TForm`/`TFormItem`、`TLoading`、`TEmpty`、`TAlert`、`TTag`、`TButton` 与 `VueUiKpi` / `VueUiVerticalBar` / `VueUiDonut` / VuIcons 全部可在 RazorVue 页面上下文组合渲染。管理页现在直接使用这些强类型 TDesign 组件；泛型参数由 Razor 标记显式声明，不需要应用专用控件桥接。
- **降级边界发现与修复**（已登记到 [RazorVue 作者面诊断路线图](./razorvue-authoring-diagnostics.md)）：官方 Razor SG 为泛型组件生成的 `TypeInference` 辅助方法，以及手写泛型基类 `BuildRenderTree` 中的开放式 `OpenComponent<T>`，现在都沿用当前 fragment/direct-render builder 作用域，并将构造泛型方法与方法体参数绑定到 `OriginalDefinition`。因此根渲染体、宿主组件子片段和闭式派生组件都保持泛型擦除语义，不会泄漏未定义的 `__builder` 或 `builder.*` 伪调用。`.razor` 标记可以直接使用泛型组件；只有在需要固定领域 API 或模块名时才保留非泛型薄包装。
- 管理模块页（SSO 应用/作用域/授权/令牌、组织机构、角色与资源授权、账号、配置、调度）全部重写为 TDesign 组件：数据表走 `TTable<T>`（列定义在 code-behind，组合单元格走 `Cell`），表单走 `TForm<T>`/`TFormItem` 与 typed `TInput<string>`、`TTextarea`、`TSwitch<bool>`、`TRadioGroup<string>`，加载态 `TLoading`、空态 `TEmpty`、错误态 `TAlert`、状态徽标 `TTag`。账户页已进一步采用 typed draft、字段规则、`@bind-Value` 和 submit/reset；其他页面的 `TJsonObject` 表单迁移和表格操作列 authoring 仍按 M5 后续摩擦项推进。页面骨架统一为 `ja-page`/`ja-page__split`/`ja-panel` 布局工具。
- 仪表盘重写：指标卡 `VueUiKpi`、执行趋势 `VueUiVerticalBar`、资源占比 `VueUiDonut`（数据来自 `Features/Overview` 真实 API），图标面迁至 VuIcons（`VuAppWindow`/`VuUser`/`VuServer`/`VuChartLine`/`VuArrowUpRight`）。新增 `ECMAScript.VueDataUi`、`ECMAScript.VuIcons` 包引用。
- 样式收缩：`Styles.cs` 3048→约 1800 行、`StarterStyles.cs` 953→约 600 行，死规则清零（按"定义-引用"差异删除 `ja-management__*`、`ja-overview__*`、旧仪表盘/旧壳层骨架等约 250 条语句）。剩余规则全部在用；**登录页与壳层手绘面（`ja-access`、`ja-iconbar`、TDesign 组合壳）未重写是 <1000 行目标未达的原因**；M4 完成时登录页仍保留手绘实现，接入 TDesign 表单留作后续门户工作。
- 验收：`verify-smoke` 更新为 TDesign DOM 锚点（`.t-table`/`.t-loading`/`.t-empty`）与 `data-*` 命令锚点，新增空态验收（配置中心删除全部配置后 `TEmpty` 出现，同时覆盖两段式删除确认）；桌面与移动 viewport 断言覆盖重写页面。

### M3 审计日志（已完成）

- `feature.audit.enabled` 由 `Features/Audit` 消费：
  - 记录管理操作（创建/修改/删除/授权变更）与 OpenIddict 令牌签发、撤销事件；
  - 查询页支持时间范围、操作者、对象、操作类型筛选；
  - 写入走统一拦截点，不在各 Endpoint 手工埋点。
- 验收：`samples/JazorAdmin.Test` 覆盖事件写入与查询；`verify-smoke` 覆盖 `AuditPage` 产物、对象/操作筛选和清空后的表格恢复。

### M4 SSO 演示客户端与门户工作台（已完成）

依赖：M1（门户语义确立）；与 M2/M3 并行度低，建议最后做。

- 新增独立最小 RazorVue 示例应用 `samples/JazorAdmin.DemoClient`（独立端口/源），注册为 OpenIddict confidential client，演示完整授权码 + PKCE 流程与单点登出。
- JazorAdmin 门户工作台聚合下游应用入口（`TCard` + VuIcons 图标），并以 `VueUiKpi` / `VueUiDonut` 等展示平台运营指标（登录趋势、活跃应用、令牌签发量，数据来自审计日志与 Overview API）。
- 验收：`samples/JazorAdmin.DemoClient/verify-smoke.cs` 从当前源包隔离重建两个宿主，以临时 SQLite 和 ephemeral secret 跑 CAPTCHA 登录、授权码 + PKCE、令牌交换、Bearer 保护 API、审计令牌事件、门户注册和单点登出。

### M5 Blazor-first 作者面与平台能力回归（由独立路线承接）

M5 不再在 JazorAdmin 页面里堆叠 RazorVue 专属 workaround。M2 实测暴露的泛型组件、开放式 `BuildRenderTree`、`RenderFragment` 可达性、表单 binding 和生命周期边界，统一转入 [RazorVue Blazor-first 兼容与开发者体验路线图](./razorvue-developer-experience.md)；该路线以平台能力、作者源码诊断和真实运行时证明为 owner。

JazorAdmin 在 M5 中只承担真实消费者和回归样本的角色：

- 页面源码继续使用标准 Blazor Razor/C# 作者面，不新增内部 builder、Vue module、手写 JavaScript 或 `object` 逃生参数；
- 已在页面中出现的桥接组件，只有在两个以上独立页面重复且原生 Razor 形状确实不自然时才进入 API review，不以单个页面 workaround 反推公共 API；
- 每个被平台声明为 Direct Support 或 Compatibility Adapter 的能力，都必须在 JazorAdmin 的 Release browser smoke、package consumer 和适用 SSR/hydration 路径中验证；
- 无法在浏览器保持 Blazor 可观察行为的形状，由 authored-source compatibility analyzer 在作者代码位置说明原因和最小替代；JazorAdmin 不通过静默降级掩盖它。

M5 的完成条件、P0/P1/P2 优先级、诊断 ID、适配协议和门禁以独立路线图为准；本文件只记录 JazorAdmin 的消费关系，避免形成第二套兼容矩阵。

## 页面范式（开发者参考契约）

所有数据页共用一个骨架：**页头（标题 + 描述 + 主操作区）→ 筛选/工具条 → 内容 → 反馈**。四类页面的标准组合：

| 范式 | 组件组合 | 要点 |
| --- | --- | --- |
| 列表页 | `TForm`(inline 筛选) + `TTable` + `TPagination` | 批量选择、行操作用 `TPopconfirm`/`TDialog` 确认；筛选即时生效或显式提交，二选一并全站一致 |
| 表单页 | `TForm` + `TSteps`(分步) | 校验错误就地展示；一次性敏感值（密钥）用独立展示态而非表单回填 |
| 详情页 | `TDescriptions` + `TCard` 区块 | 只读字段成组展示，操作区固定在页头或右上 |
| 结果页 | VuIcons 大图标 + 排版组合（绑定无 `TResult`） | 提供明确的下一步动作（返回/重试/查看进度），不留死胡同 |

状态设计（每个数据页必备）：

- 加载：`TLoading` 或 `TSkeleton`，禁止裸文本 "Loading..."。
- 空态：`TEmpty` + 引导动作（如"新建第一个应用"），禁止空白区域。
- 错误：`TAlert` + 重试入口，保留已输入内容。
- 无权限：明确的 403 态与说明，而非静默隐藏入口。
- 写操作反馈：成功/失败用 `TMessage`/`TNotification`；破坏性操作必须确认。

## 设计令牌

- 品牌色单一来源 `--ja-brand-color`，TDesign 桥接（`--td-brand-color` 等）已有，页面不得硬编码色值。
- 语义色（success/warning/danger）映射 TDesign token，不另设色板。
- 间距走 4/8 栅格；圆角、阴影、字号层级全部跟随 TDesign，页面内不自造字号与阴影档位。
- 文本层级沿用现有 `--text` / `--text-muted` 令牌。
- 图标统一从 VuIcons 取用，尺寸收敛为少数档位（16/20/24），颜色默认继承文本令牌或语义色，不逐处硬编码。

## 验收门禁

每个里程碑完成后运行：

```bash
dotnet run --no-launch-profile --file samples/JazorAdmin/verify-smoke.cs -- --configuration Release
dotnet test samples/JazorAdmin.Test/JazorAdmin.Test.csproj
```

M5 触及 RazorVue 作者面或运行时适配时，以上两项只是消费者门禁，必须同时执行独立路线图规定的 Razor SG、Emit、browser、package consumer 以及适用 SSR/hydration 门禁；JazorAdmin smoke 不能替代平台语义证明。

发版：整体走 MINOR；M1 的复刻页退役按 CHANGELOG 规范写迁移说明。

## 非目标

多租户、文件中心、报表大屏、消息队列集成、工作流引擎。参考应用的价值在每一块都做到"能照抄的生产范式"，不在功能清单长度。下游应用的业务权限建模（平台 IAM 只发身份）同样不在范围内。
