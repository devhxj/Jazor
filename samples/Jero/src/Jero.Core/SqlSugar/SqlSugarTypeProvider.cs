


using System.Linq.Dynamic.Core.CustomTypeProviders;
namespace Jero.Core;

/// <summary>
/// 扩展支持 SqlFunc，不支持 Subqueryable
/// </summary>
public class SqlSugarTypeProvider : DefaultDynamicLinqCustomTypeProvider
{
    public SqlSugarTypeProvider(bool cacheCustomTypes = true) : base(ParsingConfig.Default, cacheCustomTypes)
    {
    }

    public override HashSet<Type> GetCustomTypes()
    {
        var customTypes = base.GetCustomTypes();
        customTypes.Add(typeof(SqlFunc));
        return customTypes;
    }
}