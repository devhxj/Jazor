namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机业务菜单表种子数据
/// </summary>
public class SysMenuSeedData : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        return new[]
        {
            new SysMenu{ Id=1310700000101, Pid=0, Title="医线生机", Path="/save-life", Name="saveLife", Component="Layout", Redirect="/save-life/dashboard", Icon="lucide:heart-pulse", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=9000 },

            new SysMenu{ Id=1310700000201, Pid=1310700000101, Title="仪表盘", Path="/save-life/dashboard", Name="saveLifeDashboard", Component="/saveLife/dashboard/index", Icon="lucide:layout-dashboard", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=90 },

            new SysMenu{ Id=1310700010101, Pid=1310700000101, Title="项目管理", Path="/save-life/projects", Name="saveLifeProjects", Component="/saveLife/projects/index", Icon="lucide:clipboard-list", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700010201, Pid=1310700010101, Title="查询", Permission="saveLifeProject:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700010301, Pid=1310700010101, Title="增加", Permission="saveLifeProject:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700010401, Pid=1310700010101, Title="编辑", Permission="saveLifeProject:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310700010501, Pid=1310700010101, Title="删除", Permission="saveLifeProject:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1310700010601, Pid=1310700010101, Title="置顶", Permission="saveLifeProject:pin", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=140 },

            new SysMenu{ Id=1310700020101, Pid=1310700000101, Title="报名记录", Path="/save-life/enrollments", Name="saveLifeEnrollments", Component="/saveLife/enrollments/index", Icon="lucide:user-check", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700020201, Pid=1310700020101, Title="查询", Permission="saveLifeEnrollmentRecord:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700020301, Pid=1310700020101, Title="编辑", Permission="saveLifeEnrollmentRecord:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700020401, Pid=1310700020101, Title="审核", Permission="saveLifeEnrollmentRecord:audit", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310700020501, Pid=1310700020101, Title="删除", Permission="saveLifeEnrollmentRecord:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1310700020601, Pid=1310700020101, Title="联系", Permission="saveLifeEnrollmentRecord:contact", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=140 },
            new SysMenu{ Id=1310700020701, Pid=1310700020101, Title="审核领域配置", Permission="saveLifeEnrollmentRecord:manageAuditScope", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-08-26 00:00:00"), OrderNo=150 },

            new SysMenu{ Id=1310700030101, Pid=1310700000101, Title="疾病知识", Path="/save-life/knowledge", Name="saveLifeKnowledge", Component="/saveLife/knowledge/index", Icon="lucide:book-open", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310700030201, Pid=1310700030101, Title="查询", Permission="saveLifeKnowledge:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700030301, Pid=1310700030101, Title="增加", Permission="saveLifeKnowledge:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700030401, Pid=1310700030101, Title="编辑", Permission="saveLifeKnowledge:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310700030501, Pid=1310700030101, Title="删除", Permission="saveLifeKnowledge:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1310700030601, Pid=1310700030101, Title="发布", Permission="saveLifeKnowledge:publish", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=140 },

            new SysMenu{ Id=1310700040101, Pid=1310700000101, Title="用户管理", Path="/save-life/users", Name="saveLifeUsers", Component="/saveLife/users/index", Icon="lucide:users", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1310700040201, Pid=1310700040101, Title="查询", Permission="saveLifeUser:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700040301, Pid=1310700040101, Title="增加", Permission="saveLifeUser:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700040401, Pid=1310700040101, Title="编辑", Permission="saveLifeUser:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310700040501, Pid=1310700040101, Title="删除", Permission="saveLifeUser:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=130 },

            new SysMenu{ Id=1310700050101, Pid=1310700000101, Title="导入记录", Path="/save-life/imports", Name="saveLifeImports", Component="/saveLife/imports/index", Icon="lucide:file-input", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=140 },
            new SysMenu{ Id=1310700050201, Pid=1310700050101, Title="查询", Permission="saveLifeImport:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700050301, Pid=1310700050101, Title="增加", Permission="saveLifeImport:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700050401, Pid=1310700050101, Title="编辑", Permission="saveLifeImport:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310700050501, Pid=1310700050101, Title="删除", Permission="saveLifeImport:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=130 },

            new SysMenu{ Id=1310700060101, Pid=1310700000101, Title="助手配置", Path="/save-life/agent-config", Name="saveLifeAgentConfig", Component="/saveLife/agent-config/index", Icon="lucide:bot", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=150 },
            new SysMenu{ Id=1310700060201, Pid=1310700060101, Title="查询", Permission="saveLifeAgentConfig:config", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700060301, Pid=1310700060101, Title="保存", Permission="saveLifeAgentConfig:saveConfig", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700060401, Pid=1310700060101, Title="验证", Permission="saveLifeAgentConfig:testConfig", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },

            new SysMenu{ Id=1310700090101, Pid=1310700000101, Title="运营审计", Path="/save-life/operations", Name="saveLifeOperations", Component="/saveLife/operations/index", Icon="lucide:list-checks", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=155 },
            new SysMenu{ Id=1310700090201, Pid=1310700090101, Title="材料查询", Permission="saveLifeEnrollmentMaterial:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700090301, Pid=1310700090101, Title="通知查询", Permission="saveLifeNotification:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700090401, Pid=1310700090101, Title="向量查询", Permission="saveLifeAgentVectorIndex:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310700090501, Pid=1310700090101, Title="向量重试", Permission="saveLifeAgentVectorIndex:retry", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=130 },

            new SysMenu{ Id=1310700070101, Pid=1310700000101, Title="小程序设置", Path="/save-life/settings", Name="saveLifeSettings", Component="/saveLife/settings/index", Icon="lucide:sliders-horizontal", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=160 },
            new SysMenu{ Id=1310700070201, Pid=1310700070101, Title="查询", Permission="saveLifeSetting:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310700070301, Pid=1310700070101, Title="配置查询", Permission="saveLifeSetting:miniAppConfig", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310700070401, Pid=1310700070101, Title="配置保存", Permission="saveLifeSetting:saveMiniAppConfig", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=120 },

            new SysMenu{ Id=1310700080101, Pid=1310700000101, Title="用户反馈", Path="/save-life/feedback", Name="saveLifeFeedback", Component="/saveLife/feedback/index", Icon="lucide:message-square-more", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=170 },
            new SysMenu{ Id=1310700080201, Pid=1310700080101, Title="查询", Permission="saveLifeFeedback:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2026-06-11 00:00:00"), OrderNo=100 },
        };
    }
}
