namespace Jero.Core;

/// <summary>
/// 系统字典值表种子数据
/// </summary>
public class SysDictDataSeedData : ISqlSugarEntitySeedData<SysDictData>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysDictData> HasData()
    {
        var orgType = new SysDictTypeSeedData().HasData().Single(type => type.Code == "org_type");
        return new[]
        {
            new SysDictData{ Id=1300000000701, DictTypeId=orgType.Id, Label="集团", Value="101", OrderNo=100, Remark="集团", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-02-10 00:00:00") },
            new SysDictData{ Id=1300000000702, DictTypeId=orgType.Id, Label="公司", Value="201", OrderNo=101, Remark="公司", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-02-10 00:00:00") },
            new SysDictData{ Id=1300000000703, DictTypeId=orgType.Id, Label="部门", Value="301", OrderNo=102, Remark="部门", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-02-10 00:00:00") },
            new SysDictData{ Id=1300000000704, DictTypeId=orgType.Id, Label="区域", Value="401", OrderNo=103, Remark="区域", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-02-10 00:00:00") },
            new SysDictData{ Id=1300000000705, DictTypeId=orgType.Id, Label="组", Value="501", OrderNo=104, Remark="组", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-02-10 00:00:00") },
        };
    }
}