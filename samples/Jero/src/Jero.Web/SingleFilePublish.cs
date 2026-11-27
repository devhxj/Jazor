using Furion;
using System.Reflection;
namespace Jero.Web;

/// <summary>
/// 将随宿主打包的应用程序集加入 Furion 的扫描范围，以发现其中的服务和端点。
/// </summary>
public class SingleFilePublish : ISingleFilePublish
{
    /// <summary>
    /// 提供按程序集实例补充扫描范围的入口。Jero 当前使用程序集名称注册。
    /// </summary>
    public Assembly[] IncludeAssemblies()
    {
        return Array.Empty<Assembly>();
    }

    /// <summary>
    /// 按程序集名称登记应用、核心和 Web 宿主，使扫描无需依赖发布目录中的独立 DLL。
    /// </summary>
    public string[] IncludeAssemblyNames()
    {
        return new[]
        {
            "Jero.Application",
            "Jero.Core",
            "Jero.Web",
        };
    }
}
