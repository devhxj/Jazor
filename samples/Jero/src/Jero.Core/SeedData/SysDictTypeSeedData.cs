namespace Jero.Core;

/// <summary>
/// 系统字典类型表种子数据
/// </summary>
public class SysDictTypeSeedData : ISqlSugarEntitySeedData<SysDictType>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysDictType> HasData()
    {
        return new[]
        {
            new SysDictType{ Id=1300000000111, Name="代码生成控件类型", Code="code_gen_effect_type", SysFlag=YesNoEnum.Y, OrderNo=100, Remark="代码生成控件类型", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysDictType{ Id=1300000000121, Name="代码生成查询类型", Code="code_gen_query_type", SysFlag=YesNoEnum.Y, OrderNo=101, Remark="代码生成查询类型", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysDictType{ Id=1300000000131, Name="代码生成.NET类型", Code="code_gen_net_type", SysFlag=YesNoEnum.Y, OrderNo=102, Remark="代码生成.NET类型", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysDictType{ Id=1300000000141, Name="代码生成方式", Code="code_gen_create_type", SysFlag=YesNoEnum.Y, OrderNo=103, Remark="代码生成方式", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysDictType{ Id=1300000000151, Name="代码生成基类", Code="code_gen_base_class", SysFlag=YesNoEnum.Y, OrderNo=104, Remark="代码生成基类", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
            new SysDictType{ Id=1300000000161, Name="代码生成打印类型", Code="code_gen_print_type", SysFlag=YesNoEnum.Y, OrderNo=105, Remark="代码生成打印类型", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-12-04 00:00:00") },
            new SysDictType{ Id=1300000000171, Name="机构类型", Code="org_type", SysFlag=YesNoEnum.Y, OrderNo=201, Remark="机构类型", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-02-10 00:00:00") },
        };
    }
}