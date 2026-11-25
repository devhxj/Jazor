namespace Jero.Core;

/// <summary>
/// 动态作业编译
/// </summary>
public class DynamicJobCompiler : ISingleton
{
    /// <summary>
    /// 编译代码并返回其中实现 IJob 的类型
    /// </summary>
    /// <param name="script">动态编译的作业代码</param>
    /// <returns></returns>
    public static Type BuildJob(string script)
    {
        if (string.IsNullOrWhiteSpace(script)) throw Oops.Oh("动态作业脚本不能为空");

        // 作业调度器会继续使用返回的类型，编译完成时不能请求卸载。
        var jobAssembly = App.CompileCSharpClassCode(script).Assembly;
        var jobType = jobAssembly.GetTypes().FirstOrDefault(u => typeof(IJob).IsAssignableFrom(u));
        return jobType ?? throw Oops.Oh("动态作业脚本未包含 IJob 实现类型");
    }
}
