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
            new SysDictType{ Id=1300000000171, Name="机构类型", Code="org_type", SysFlag=YesNoEnum.Y, OrderNo=201, Remark="机构类型", Status=StatusEnum.Enable, CreateTime=DateTime.Parse("2023-02-10 00:00:00") },
        };
    }
}