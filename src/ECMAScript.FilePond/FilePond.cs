using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// FilePond 入口；提供实例创建、查找、全局选项与插件注册。
/// FilePond entry; provides instance creation, lookup, global options, and plugin registration.
/// </summary>
/// <remarks>
/// 资源闭包同时交付 <c>filepond</c> 核心与其官方 Vue 适配器 <c>vue-filepond</c>。
/// 本入口绑定核心的命令式 API；Vue 适配器是默认导出的工厂（按插件参数动态生成 props），
/// 其 Razor 代理见 <see cref="VueFilePond"/>。上传依赖真实 DOM 与文件 API，必须在浏览器生命周期内使用。
/// The closure ships both the <c>filepond</c> core and its official Vue adapter
/// <c>vue-filepond</c>. This entry binds the core imperative API; the Vue adapter is a
/// default-export factory whose props are generated per plugin, so its Razor proxy is
/// <see cref="VueFilePond"/>. Uploads need a real DOM and the File API, so they must run within
/// the browser lifecycle.
/// </remarks>
[ECMAScript("filepond")]
[Description("@#")]
public static partial class FilePond
{
}
