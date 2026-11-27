namespace Jero.Core;

/// <summary>
/// 系统菜单表种子数据
/// </summary>
public class SysMenuSeedData : ISqlSugarEntitySeedData<SysMenu>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysMenu> HasData()
    {
        return new[]
        {
            new SysMenu{ Id=1300100000101, Pid=0, Title="工作台", Path="/dashboard", Name="dashboard", Component="Layout", Icon="ele-HomeFilled", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=0 },
            new SysMenu{ Id=1300100010101, Pid=1300100000101, Title="工作台", Path="/dashboard/home", Name="home", Component="/home/index", IsAffix=true, Icon="ele-HomeFilled", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300100010201, Pid=1300100000101, Title="站内信", Path="/dashboard/notice", Name="notice", Component="/dashboard/notice/index", Icon="ele-Bell", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=101 },

            // 建议此处Id范围之间放置具体业务应用菜单

            #region 系统管理

            new SysMenu{ Id=1300200000101, Pid=0, Title="系统管理", Path="/system", Name="system", Component="Layout", Icon="ele-Setting", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=10000 },

            // 账号管理
            new SysMenu{ Id=1300200010101, Pid=1300200000101, Title="账号管理", Path="/system/user", Name="sysUser", Component="/system/user/index", Icon="ele-User", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010201, Pid=1300200010101, Title="查询", Permission="sysUser:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010301, Pid=1300200010101, Title="编辑", Permission="sysUser:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010401, Pid=1300200010101, Title="增加", Permission="sysUser:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010501, Pid=1300200010101, Title="删除", Permission="sysUser:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010601, Pid=1300200010101, Title="详情", Permission="sysUser:detail", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010701, Pid=1300200010101, Title="授权角色", Permission="sysUser:grantRole", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010801, Pid=1300200010101, Title="重置密码", Permission="sysUser:resetPwd", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200010901, Pid=1300200010101, Title="设置状态", Permission="sysUser:setStatus", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200011001, Pid=1300200110101, Title="强制下线", Permission="sysOnlineUser:forceOffline", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200011101, Pid=1300200010101, Title="解除锁定", Permission="sysUser:unlockLogin", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 角色管理
            new SysMenu{ Id=1300200020101, Pid=1300200000101, Title="角色管理", Path="/system/role", Name="sysRole", Component="/system/role/index", Icon="ele-Help", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1300200020201, Pid=1300200020101, Title="查询", Permission="sysRole:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200020301, Pid=1300200020101, Title="编辑", Permission="sysRole:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200020401, Pid=1300200020101, Title="增加", Permission="sysRole:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200020501, Pid=1300200020101, Title="删除", Permission="sysRole:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200020601, Pid=1300200020101, Title="授权菜单", Permission="sysRole:grantMenu", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200020701, Pid=1300200020101, Title="授权数据", Permission="sysRole:grantDataScope", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200020801, Pid=1300200020101, Title="设置状态", Permission="sysRole:setStatus", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 机构管理
            new SysMenu{ Id=1300200030101, Pid=1300200000101, Title="机构管理", Path="/system/org", Name="sysOrg", Component="/system/org/index", Icon="ele-OfficeBuilding", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1300200030201, Pid=1300200030101, Title="查询", Permission="sysOrg:list", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200030301, Pid=1300200030101, Title="编辑", Permission="sysOrg:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200030401, Pid=1300200030101, Title="增加", Permission="sysOrg:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200030501, Pid=1300200030101, Title="删除", Permission="sysOrg:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 职位管理
            new SysMenu{ Id=1300200040101, Pid=1300200000101, Title="职位管理", Path="/system/pos", Name="sysPos", Component="/system/pos/index",Icon="ele-Mug", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1300200040201, Pid=1300200040101, Title="查询", Permission="sysPos:list", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200040301, Pid=1300200040101, Title="编辑", Permission="sysPos:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200040401, Pid=1300200040101, Title="增加", Permission="sysPos:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200040501, Pid=1300200040101, Title="删除", Permission="sysPos:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 个人中心
            new SysMenu{ Id=1300200050101, Pid=1300200000101, Title="个人中心", Path="/profile", Name="Profile", Component="/_core/profile/index",Icon="ele-Medal", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=140 },
            new SysMenu{ Id=1300200050201, Pid=1300200050101, Title="修改密码", Permission="sysUser:changePwd", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200050301, Pid=1300200050101, Title="基本信息", Permission="sysUser:baseInfo", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200050401, Pid=1300200050101, Title="电子签名", Permission="sysFile:uploadSignature", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200050501, Pid=1300200050101, Title="上传头像", Permission="sysFile:uploadAvatar", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 通知公告
            new SysMenu{ Id=1300200060101, Pid=1300200000101, Title="通知公告", Path="/system/notice", Name="sysNotice", Component="/system/notice/index",Icon="ele-Bell", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=150 },
            new SysMenu{ Id=1300200060201, Pid=1300200060101, Title="查询", Permission="sysNotice:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200060301, Pid=1300200060101, Title="编辑", Permission="sysNotice:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200060401, Pid=1300200060101, Title="增加", Permission="sysNotice:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200060501, Pid=1300200060101, Title="删除", Permission="sysNotice:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200060601, Pid=1300200060101, Title="发布", Permission="sysNotice:public", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200060701, Pid=1300200060101, Title="撤回", Permission="sysNotice:cancel", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 三方账号
            new SysMenu{ Id=1300200070101, Pid=1300200000101, Title="三方账号", Path="/system/weChatUser", Name="sysWechatUser", Component="/system/weChatUser/index",Icon="ele-ChatDotRound", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=160 },
            new SysMenu{ Id=1300200070201, Pid=1300200070101, Title="查询", Permission="sysWechatUser:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200070301, Pid=1300200070101, Title="编辑", Permission="sysWechatUser:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200070401, Pid=1300200070101, Title="增加", Permission="sysWechatUser:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200070501, Pid=1300200070101, Title="删除", Permission="sysWechatUser:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // AD域配置
            new SysMenu{ Id=1300200080101, Pid=1300200000101, Title="AD域配置", Path="/system/ldap", Name="sysLdap", Component="/system/ldap/index",Icon="ele-Place", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=170 },
            new SysMenu{ Id=1300200080201, Pid=1300200080101, Title="查询", Permission="sysLdap:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200080301, Pid=1300200080101, Title="详情", Permission="sysLdap:detail", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1300200080401, Pid=1300200080101, Title="编辑", Permission="sysLdap:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1300200080501, Pid=1300200080101, Title="增加", Permission="sysLdap:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1300200080601, Pid=1300200080101, Title="删除", Permission="sysLdap:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=140 },
            new SysMenu{ Id=1300200080701, Pid=1300200080101, Title="同步域账户", Permission="sysLdap:syncUser", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=150 },
            new SysMenu{ Id=1300200080801, Pid=1300200080101, Title="同步域组织", Permission="sysLdap:syncOrg", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=160 },

            #endregion 系统管理

            #region 平台管理

            new SysMenu{ Id=1300300000101, Pid=0, Title="平台管理", Path="/platform", Name="platform", Component="Layout", Icon="ele-Operation", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=11000 },

            // 租户管理
            new SysMenu{ Id=1300300010101, Pid=1300300000101, Title="租户管理", Path="/platform/tenant", Name="sysTenant", Component="/system/tenant/index", Icon="ele-School", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010201, Pid=1300300010101, Title="查询", Permission="sysTenant:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010301, Pid=1300300010101, Title="编辑", Permission="sysTenant:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010401, Pid=1300300010101, Title="增加", Permission="sysTenant:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010501, Pid=1300300010101, Title="删除", Permission="sysTenant:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010601, Pid=1300300010101, Title="授权菜单", Permission="sysTenant:grantMenu", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010701, Pid=1300300010101, Title="重置密码", Permission="sysTenant:resetPwd", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010801, Pid=1300300010101, Title="生成库", Permission="sysTenant:createDb", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300010901, Pid=1300300010101, Title="设置状态", Permission="sysTenant:setStatus", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300011001, Pid=1300300010101, Title="同步授权", Permission="sysTenant:syncGrantMenu", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300011101, Pid=1300300010101, Title="切换租户", Permission="sysTenant:changeTenant", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300011201, Pid=1300300010101, Title="进入租管端", Permission="sysTenant:goTenant", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 注册方案
            new SysMenu{ Id=1300300020101, Pid=1300300000101, Title="注册方案", Path="/platform/regWay", Name="sysUserRegWay", Component="/system/userRegWay/index", Icon="ele-Pointer", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=105 },
            new SysMenu{ Id=1300300020201, Pid=1300300020101, Title="查询", Permission="sysUserRegWay:list", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300020301, Pid=1300300020101, Title="编辑", Permission="sysUserRegWay:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300020401, Pid=1300300020101, Title="增加", Permission="sysUserRegWay:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300020501, Pid=1300300020101, Title="删除", Permission="sysUserRegWay:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 菜单管理
            new SysMenu{ Id=1300300030101, Pid=1300300000101, Title="菜单管理", Path="/platform/menu", Name="sysMenu", Component="/system/menu/index", Icon="ele-Menu", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1300300030201, Pid=1300300030101, Title="查询", Permission="sysMenu:list", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300030301, Pid=1300300030101, Title="编辑", Permission="sysMenu:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300030401, Pid=1300300030101, Title="增加", Permission="sysMenu:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300030501, Pid=1300300030101, Title="删除", Permission="sysMenu:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 平台参数
            new SysMenu{ Id=1300300040101, Pid=1300300000101, Title="平台参数", Path="/platform/config", Name="sysConfig", Component="/system/config/index", Icon="ele-DocumentCopy", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1300300040201, Pid=1300300040101, Title="查询", Permission="sysConfig:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300040301, Pid=1300300040101, Title="编辑", Permission="sysConfig:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300040401, Pid=1300300040101, Title="增加", Permission="sysConfig:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300040501, Pid=1300300040101, Title="删除", Permission="sysConfig:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 字典管理
            new SysMenu{ Id=1300300050101, Pid=1300300000101, Title="字典管理", Path="/platform/dict", Name="sysDict", Component="/system/dict/index", Icon="ele-Collection", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1300300050201, Pid=1300300050101, Title="查询", Permission="sysDictType:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300050301, Pid=1300300050101, Title="编辑", Permission="sysDictType:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300050401, Pid=1300300050101, Title="增加", Permission="sysDictType:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300050501, Pid=1300300050101, Title="删除", Permission="sysDictType:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300050601, Pid=1300300050101, Title="增加字典值", Permission="sysDictData:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300050701, Pid=1300300050101, Title="删除字典值", Permission="sysDictData:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300050801, Pid=1300300050101, Title="编辑字典值", Permission="sysDictData:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300050901, Pid=1300300050101, Title="字典迁移", Permission="sysDictType:move", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 模板管理
            new SysMenu{ Id=1300300051101, Pid=1300300000101, Title="模板管理", Path="/platform/template", Name="sysTemplate", Component="/system/template/index", Icon="ele-Document", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=135 },
            new SysMenu{ Id=1300300051201, Pid=1300300051101, Title="查询", Permission="sysTemplate:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300051301, Pid=1300300051101, Title="编辑", Permission="sysTemplate:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300051401, Pid=1300300051101, Title="增加", Permission="sysTemplate:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300051501, Pid=1300300051101, Title="删除", Permission="sysTemplate:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300051601, Pid=1300300051101, Title="预览", Permission="sysTemplate:preview", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 任务调度
            new SysMenu{ Id=1300300060101, Pid=1300300000101, Title="任务调度", Path="/platform/job", Name="sysJob", Component="/system/job/index", Icon="ele-AlarmClock", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=140 },
            new SysMenu{ Id=1300300060201, Pid=1300300060101, Title="查询", Permission="sysJob:pageJobDetail", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300060301, Pid=1300300060101, Title="编辑", Permission="sysJob:updateJobDetail", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300060401, Pid=1300300060101, Title="增加", Permission="sysJob:addJobDetail", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300060501, Pid=1300300060101, Title="删除", Permission="sysJob:deleteJobDetail", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 系统参数
            new SysMenu{ Id=1300200090101, Pid=1300200000101, Title="系统参数", Path="/system/tenantConfig", Name="sysTenantConfig", Component="/system/tenantConfig/index", Icon="ele-DocumentCopy", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=180 },
            new SysMenu{ Id=1300200090201, Pid=1300200090101, Title="查询", Permission="sysTenantConfig:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200090301, Pid=1300200090101, Title="编辑", Permission="sysTenantConfig:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200090401, Pid=1300200090101, Title="增加", Permission="sysTenantConfig:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200090501, Pid=1300200090101, Title="删除", Permission="sysTenantConfig:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 在线用户
            new SysMenu{ Id=1300200110101, Pid=1300200000101, Title="在线用户", Path="/system/onlineUser", Name="sysOnlineUser", Component="/system/onlineUser/index", Icon="ele-User", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=185 },
            new SysMenu{ Id=1300200110201, Pid=1300200110101, Title="查询", Permission="sysOnlineUser:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 语言管理
            new SysMenu{ Id=1300200100101, Pid=1300200000101, Title="语言管理", Path="/system/lang", Name="sysLang", Component="/system/lang/index", Icon="iconfont icon-diqiu", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2025-06-28 00:00:00"), OrderNo=190 },
            new SysMenu{ Id=1300200100201, Pid=1300200100101, Title="查询", Permission="sysLang:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200100301, Pid=1300200100101, Title="编辑", Permission="sysLang:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200100401, Pid=1300200100101, Title="增加", Permission="sysLang:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200100501, Pid=1300200100101, Title="删除", Permission="sysLang:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 翻译管理
            new SysMenu{ Id=1300200200101, Pid=1300200000101, Title="翻译管理", Path="/system/langText", Name="sysLangText", Component="/system/langText/index", Icon="iconfont icon-zhongyingwen", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2025-06-28 00:00:00"), OrderNo=200 },
            new SysMenu{ Id=1300200200201, Pid=1300200200101, Title="查询", Permission="sysLangText:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200200301, Pid=1300200200101, Title="编辑", Permission="sysLangText:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200200401, Pid=1300200200101, Title="增加", Permission="sysLangText:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300200200501, Pid=1300200200101, Title="删除", Permission="sysLangText:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 系统监控
            new SysMenu{ Id=1300300070101, Pid=1300300000101, Title="系统监控", Path="/platform/server", Name="sysServer", Component="/system/server/index", Icon="ele-Monitor", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=150 },

            // 缓存管理
            new SysMenu{ Id=1300300080101, Pid=1300300000101, Title="缓存管理", Path="/platform/cache", Name="sysCache", Component="/system/cache/index", Icon="ele-Loading", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=160 },
            new SysMenu{ Id=1300300080201, Pid=1300300080101, Title="查询", Permission="sysCache:keyList", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300080301, Pid=1300300080101, Title="删除", Permission="sysCache:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300080401, Pid=1300300080101, Title="清空", Permission="sysCache:clear", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 行政区域
            new SysMenu{ Id=1300300090101, Pid=1300300000101, Title="行政区域", Path="/platform/region", Name="sysRegion", Component="/system/region/index", Icon="ele-LocationInformation", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=170 },
            new SysMenu{ Id=1300300090201, Pid=1300300090101, Title="查询", Permission="sysRegion:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300090301, Pid=1300300090101, Title="编辑", Permission="sysRegion:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300090401, Pid=1300300090101, Title="增加", Permission="sysRegion:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300090501, Pid=1300300090101, Title="删除", Permission="sysRegion:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300090601, Pid=1300300090101, Title="同步", Permission="sysRegion:sync", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 文件管理
            new SysMenu{ Id=1300300100101, Pid=1300300000101, Title="文件管理", Path="/platform/file", Name="sysFile", Component="/system/file/index", Icon="ele-Document", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=180 },
            new SysMenu{ Id=1300300100201, Pid=1300300100101, Title="查询", Permission="sysFile:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300100301, Pid=1300300100101, Title="上传", Permission="sysFile:uploadFile", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300100401, Pid=1300300100101, Title="下载", Permission="sysFile:downloadFile", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300100501, Pid=1300300100101, Title="删除", Permission="sysFile:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300100601, Pid=1300300100101, Title="编辑", Permission="sysFile:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2023-10-27 00:00:00"), OrderNo=100 },

            // 打印模板
            new SysMenu{ Id=1300300110101, Pid=1300300000101, Title="打印模板", Path="/platform/print", Name="sysPrint", Component="/system/print/index", Icon="ele-Printer", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=190 },
            new SysMenu{ Id=1300300110201, Pid=1300300110101, Title="查询", Permission="sysPrint:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300110301, Pid=1300300110101, Title="编辑", Permission="sysPrint:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300110401, Pid=1300300110101, Title="增加", Permission="sysPrint:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300110501, Pid=1300300110101, Title="删除", Permission="sysPrint:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 动态插件
            new SysMenu{ Id=1300300120101, Pid=1300300000101, Title="动态插件", Path="/platform/plugin", Name="sysPlugin", Component="/system/plugin/index", Icon="ele-Connection", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=200 },
            new SysMenu{ Id=1300300120201, Pid=1300300120101, Title="查询", Permission="sysPlugin:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300120301, Pid=1300300120101, Title="编辑", Permission="sysPlugin:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300120401, Pid=1300300120101, Title="增加", Permission="sysPlugin:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300120501, Pid=1300300120101, Title="删除", Permission="sysPlugin:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 开放接口
            new SysMenu{ Id=1300300130101, Pid=1300300000101, Title="开放接口", Path="/platform/openAccess", Name="sysOpenAccess", Component="/system/openAccess/index", Icon="ele-Link", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=210 },
            new SysMenu{ Id=1300300130201, Pid=1300300130101, Title="查询", Permission="sysOpenAccess:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300130301, Pid=1300300130101, Title="编辑", Permission="sysOpenAccess:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300130401, Pid=1300300130101, Title="增加", Permission="sysOpenAccess:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300130501, Pid=1300300130101, Title="删除", Permission="sysOpenAccess:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 系统配置
            new SysMenu{ Id=1300300140101, Pid=1300300000101, Title="系统配置", Path="/platform/infoSetting", Name="sysInfoSetting", Component="/system/infoSetting/index", Icon="ele-Setting", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=220 },

            // 微信支付
            new SysMenu{ Id=1300300150101, Pid=1300300000101, Title="微信支付", Path="/platform/wechatpay", Name="sysWechatPay", Component="/system/weChatPay/index", Icon="ele-Coin", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=230 },
            new SysMenu{ Id=1300300150201, Pid=1300300150101, Title="微信支付下单Native", Permission="sysWechatPay:payTransactionNative", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300150301, Pid=1300300150101, Title="查询退款信息", Permission="sysWechatPay:listRefund", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300150401, Pid=1300300150101, Title="获取支付订单详情(本地库)", Permission="sysWechatPay:payInfo", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300150501, Pid=1300300150101, Title="获取支付订单详情(微信接口)", Permission="sysWechatPay:payInfoFromWechat", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300150601, Pid=1300300150101, Title="退款申请", Permission="sysWechatPay:refundDomestic", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 系统更新
            new SysMenu{ Id=1300300160101, Pid=1300300000101, Title="系统更新", Path="/platform/update", Name="sysUpdate", Component="/system/update/index", Icon="ele-Upload", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=240 },
            new SysMenu{ Id=1300300160201, Pid=1300300160101, Title="更新", Permission="sysUpdate:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300160301, Pid=1300300160101, Title="还原", Permission="sysUpdate:restore", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300160401, Pid=1300300160101, Title="备份列表", Permission="sysUpdate:list", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300160501, Pid=1300300160101, Title="日志列表", Permission="sysUpdate:logs", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300160601, Pid=1300300160101, Title="清除日志", Permission="sysUpdate:clear", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300300160701, Pid=1300300160101, Title="获取密钥", Permission="sysUpdate:webHookKey", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

	        #endregion 平台管理

            #region 日志管理

            new SysMenu{ Id=1300400000101, Pid=0, Title="日志管理", Path="/log", Name="log", Component="Layout", Icon="ele-DocumentCopy", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=12000 },

            // 访问日志
            new SysMenu{ Id=1300400010101, Pid=1300400000101, Title="访问日志", Path="/log/vislog", Name="sysVisLog", Component="/system/log/vislog/index", Icon="ele-Document", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300400010201, Pid=1300400010101, Title="查询", Permission="sysVislog:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300400010301, Pid=1300400010101, Title="清空", Permission="sysVislog:clear", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 操作日志
            new SysMenu{ Id=1300400020101, Pid=1300400000101, Title="操作日志", Path="/log/oplog", Name="sysOpLog", Component="/system/log/oplog/index", Icon="ele-Document", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1300400020201, Pid=1300400020101, Title="查询", Permission="sysOplog:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300400020301, Pid=1300400020101, Title="清空", Permission="sysOplog:clear", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300400020401, Pid=1300400020101, Title="导出", Permission="sysOplog:export", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 异常日志
            new SysMenu{ Id=1300400030101, Pid=1300400000101, Title="异常日志", Path="/log/exlog", Name="sysExLog", Component="/system/log/exlog/index", Icon="ele-Document", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1300400030201, Pid=1300400030101, Title="查询", Permission="sysExlog:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300400030301, Pid=1300400030101, Title="清空", Permission="sysExlog:clear", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300400030401, Pid=1300400030101, Title="导出", Permission="sysExlog:export", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            // 差异日志
            new SysMenu{ Id=1300400040101, Pid=1300400000101, Title="差异日志", Path="/log/difflog", Name="sysDiffLog", Component="/system/log/difflog/index", Icon="ele-Document", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1300400040201, Pid=1300400040101, Title="查询", Permission="sysDifflog:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300400040301, Pid=1300400040101, Title="清空", Permission="sysDifflog:clear", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },

            #endregion 日志管理

            #region 开发工具

            // 开发工具
            new SysMenu{ Id=1300500000101, Pid=0, Title="开发工具", Path="/develop", Name="develop", Component="Layout", Icon="ele-Cpu", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=13000 },
            new SysMenu{ Id=1300500010101, Pid=1300500000101, Title="库表管理", Path="/develop/database", Name="sysDatabase", Component="/system/database/index",Icon="ele-Coin", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300500030101, Pid=1300500000101, Title="表单设计", Path="/develop/formDes", Name="sysFormDes", Component="/system/formDes/index", Icon="ele-Edit", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1300500040101, Pid=1300500000101, Title="接口压测", Path="/develop/stressTest", Name="SysStressTest", Component="/system/stressTest/index", Icon="ele-DataLine", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1300500050101, Pid=1300500000101, Title="系统接口", Path="/develop/api", Name="sysApi", Component="layout/routerView/iframe", IsIframe=true, OutLink="/swagger", Icon="ele-Help", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=140 },

            #endregion 开发工具

            #region 帮助文档

            // 帮助文档
            new SysMenu{ Id=1300600000101, Pid=0, Title="帮助文档", Path="/doc", Name="doc", Component="Layout", Icon="ele-Notebook", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=14000 },
            new SysMenu{ Id=1300600010101, Pid=1300600000101, Title="框架教程", Path="/doc/admin", Name="sysAdmin", Component="layout/routerView/link", IsIframe=false, IsKeepAlive=false, OutLink="http://101.43.53.74:5050/", Icon="ele-Sunny", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1300600020101, Pid=1300600000101, Title="后台教程", Path="/doc/furion", Name="sysFurion", Component="layout/routerView/link", IsIframe=false, IsKeepAlive=false, OutLink="https://furion.baiqian.ltd/", Icon="ele-Promotion", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1300600030101, Pid=1300600000101, Title="前端教程", Path="/doc/element", Name="sysElement", Component="layout/routerView/link", IsIframe=false, IsKeepAlive=false, OutLink="https://element-plus.gitee.io/zh-CN/", Icon="ele-Position", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1300600040101, Pid=1300600000101, Title="SqlSugar", Path="/doc/SqlSugar", Name="sysSqlSugar", Component="layout/routerView/link", IsIframe=false, IsKeepAlive=false, OutLink="https://www.donet5.com/Home/Doc", Icon="ele-Coin", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=130 },

	        #endregion 帮助文档

            // 关于项目
            new SysMenu{ Id=1300700000101, Pid=0, Title="关于项目", Path="/about", Name="about", Component="/about/index", Icon="ele-InfoFilled", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2023-03-12 00:00:00"), OrderNo=15000 },
        };
    }
}
