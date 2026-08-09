# JazorAdmin Starter 复刻审计与纠偏

> Status: 当前实施基线
> Updated: 2026-08-07

## 结论

`samples/JazorAdmin/` 是 Jazor 的正式参考管理产品。它验证 RazorVue、ASP.NET Core、Identity、OpenIddict、Quartz 和包内前端资源在一个可运行后台中的组合；它不是临时 UI spike。

界面基线采用 [TDesign Vue Next Starter](https://tdesign.tencent.com/starter/vue-next/dashboard/base)。目标不是借用颜色、间距或组件名称，而是复用 Starter 的布局结构、页面模板、信息密度、响应式行为和操作位置。唯一的产品扩展是在最左侧增加一个 64px 的一级 IconBar；IconBar 之后保留 Starter 的 232px 二级菜单。

## 审计发现

| 问题 | 原因 | 纠偏 |
|---|---|---|
| 壳层只近似 Starter | IconBar 与二级菜单挤在 240px 宽度内，Header 和页面间距另起一套规则 | 固定为 `64px + 232px`，Header、内容区和页脚按 Starter 结构组织 |
| IconBar 是 CSS 伪图形 | 图标形状与主题状态不可复用，和 TDesign 图标系统脱节 | 使用本地 `tdesign-vue-next` 的 `TIcon`，不引入 CDN 或运行时 node_modules |
| 页面看似有功能但首屏空 | 开发数据库仅创建平台管理员，未创建可查看的组织、角色、配置 | 仅在 Development 且存储为空时创建最小演示工作区；生产和 Testing 不写入样例数据 |
| 业务页是自定义面板拼接 | 未采用 Starter 的 dashboard/list/tree/list/filter/form/detail 模板结构 | 路由按下表映射，表格、筛选、编辑和详情使用相应的 Starter 版式 |
| 本地化覆盖范围过小 | 仅导航和登录使用翻译，业务文本硬编码 | 页面文案统一从当前语言上下文读取；操作、状态、空态、加载与错误均可切换 |
| `Jazor.Admin` 与 sample 责任表达不清 | 历史文档将参考产品误判为错误分层 | `Jazor.Admin` 保持 UI 库中立的导航、壳层和页面容器契约；TDesign 组合仅存在于 JazorAdmin sample |

## 页面映射

| JazorAdmin 路由 | Starter 模板 | 产品内容 |
|---|---|---|
| `/` | `dashboard/base` | 管理工作台、会话和已接入中心概览 |
| `/organizations/structure` | `list/tree` | 组织树与当前节点详情 |
| `/organizations/members` | `list/filter` | 成员筛选、表格和角色编辑 |
| `/authorization/*`、`/accounts` | `list/base` / `list/filter` | 授权、账号和资源操作列表 |
| `/sso/applications`、`/sso/scopes` | `form/base` | OpenIddict 应用与 Scope 编辑 |
| `/sso/authorizations`、`/sso/tokens` | `list/filter` | 授权与令牌检索、撤销 |
| `/settings`、`/schedules` | `list/base` + `detail/base` | 配置、调度与执行历史 |
| 错误和操作反馈 | Starter result 页面 | 可恢复的状态展示 |

## 所有权

- `Jazor.Admin`：稳定且 UI 库中立的后台语义、导航模型、容器与 slot contract。
- `samples/JazorAdmin`：TDesign Starter 的实际组合、Jazor 专属 IconBar、产品页面、身份中心与运维中心。
- `ECMAScript.TDesign`：TDesign Vue Next 的类型绑定与包内静态资源，不包含产品布局或业务语义。

不再保留“JazorAdmin 应下线”或“第三方 UI 组合不能作为正式参考产品”的旧结论。第三方组件不反向进入 `Jazor.Admin` 的 public API，参考产品使用它们是应用层组合，二者并不冲突。

## 验收

1. 无 CDN、项目 node_modules 或外部前端安装步骤。
2. 桌面端是 `64px IconBar + 232px Starter side nav + 64px header`；移动端导航可收起且内容不溢出。
3. 每个路由都有实质工作区、加载/空/错误状态和中英文本。
4. 开发环境首次启动后，登录账户可看到组织、角色、配置和任务中心的非空工作区；生产不注入这些数据。
5. API、RazorVue 构建、smoke 和 Playwright 页面截图均通过。
