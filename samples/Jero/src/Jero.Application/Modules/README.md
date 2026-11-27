# 内置业务模块

ApprovalFlow、DingTalk、GoView、K3Cloud、ReZero 与 WorkWeixin 随 `Jero.Application` 编译。模块的配置文件在运行及发布目录中仍位于 `Configuration/`。

- ApprovalFlow 保留审批中间件和菜单种子。GoView 保留原 API 路由和响应格式。DingTalk 保留现有配置、API 客户端和调度标识，部署仍需要原有钉钉配置。
- K3Cloud 和 WorkWeixin 分别由 `K3Cloud:Enabled` 和 `WorkWeixin:Enabled` 启用远程客户端。启用前请填写各自的应用配置，部署凭据通过现有配置提供程序传入。
- `ReZero:Enabled` 启用超级 API 注册、管理中间件及菜单种子，默认关闭。包仍向发布目录提供其静态管理页面。

部署升级时，DingTalk 内置作业按现有作业 ID 更新程序集和类型信息。枚举字典保留迁移前的 ID，API 中的 CLR 全名反映新命名空间。
