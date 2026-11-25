namespace Jero.Core;

/// <summary>
/// 系统配置表种子数据
/// </summary>
public class SysConfigSeedData : ISqlSugarEntitySeedData<SysConfig>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysConfig> HasData()
    {
        return new[]
        {
            new SysConfig{ Id=1300000000101, Name="演示环境", Code=ConfigConst.SysDemoEnv, Value="False", SysFlag=YesNoEnum.Y, Remark="演示环境", OrderNo=10, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000111, Name="默认密码", Code=ConfigConst.SysPassword, Value="Jero++010101", SysFlag=YesNoEnum.Y, Remark="默认密码", OrderNo=20, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000121, Name="密码最大错误次数", Code=ConfigConst.SysPasswordMaxErrorTimes, Value="5", SysFlag=YesNoEnum.Y, Remark="允许密码最大输入错误次数", OrderNo=30, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000131, Name="日志保留天数", Code=ConfigConst.SysLogRetentionDays, Value="180", SysFlag=YesNoEnum.Y, Remark="日志保留天数（天）", OrderNo=40, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000141, Name="记录操作日志", Code=ConfigConst.SysOpLog, Value="True", SysFlag=YesNoEnum.Y, Remark="是否记录操作日志", OrderNo=50, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000151, Name="单设备登录", Code=ConfigConst.SysSingleLogin, Value="False", SysFlag=YesNoEnum.Y, Remark="是否开启单设备登录", OrderNo=60, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000152, Name="登入登出提醒", Code=ConfigConst.SysLoginOutReminder, Value="False", SysFlag=YesNoEnum.Y, Remark="是否开启登入登出提醒", OrderNo=60, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000161, Name="登录二次验证", Code=ConfigConst.SysSecondVer, Value="False", SysFlag=YesNoEnum.Y, Remark="是否开启登录二次验证", OrderNo=70, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000171, Name="图形验证码", Code=ConfigConst.SysCaptcha, Value="False", SysFlag=YesNoEnum.Y, Remark="测试模式默认跳过图形验证码", OrderNo=80, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000172, Name="登录时隐藏租户", Code=ConfigConst.SysHideTenantLogin, Value="True", SysFlag=YesNoEnum.Y, Remark="登录时隐藏租户", OrderNo=90, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000181, Name="Token过期时间", Code=ConfigConst.SysTokenExpire, Value="10080", SysFlag=YesNoEnum.Y, Remark="Token过期时间（分钟）", OrderNo=100, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000191, Name="RefreshToken过期时间", Code=ConfigConst.SysRefreshTokenExpire, Value="20160", SysFlag=YesNoEnum.Y, Remark="刷新Token过期时间（分钟）（一般 refresh_token 的有效时间 > 2 * access_token 的有效时间）", OrderNo=110, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000201, Name="发送异常日志邮件", Code=ConfigConst.SysErrorMail, Value="False", SysFlag=YesNoEnum.Y, Remark="是否发送异常日志邮件", OrderNo=120, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000211, Name="域登录验证", Code=ConfigConst.SysDomainLogin, Value="False", SysFlag=YesNoEnum.Y, Remark="是否开启域登录验证", OrderNo=130, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000221, Name="数据校验日志", Code=ConfigConst.SysValidationLog, Value="True", SysFlag=YesNoEnum.Y, Remark="是否数据校验日志", OrderNo=140, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000231, Name="行政区域同步层级", Code=ConfigConst.SysRegionSyncLevel, Value="3", SysFlag=YesNoEnum.Y, Remark="行政区域同步层级 1-省级,2-市级,3-区县级,4-街道级,5-村级", OrderNo=150, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysConfig{ Id=1300000000241, Name="后台默认首页地址", Code=ConfigConst.SysDefaultHomePath, Value=DefaultHomePathUtil.FallbackHomePath, SysFlag=YesNoEnum.Y, Remark="用户未配置个性化首页时使用的后台默认首页地址", OrderNo=160, GroupCode=ConfigConst.SysDefaultGroup, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
        };
    }
}
