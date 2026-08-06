// AppModule.cs - Vue 应用入口模块 / Vue application entry module
// 创建 Vue 3 应用实例并挂载到 DOM / Creates a Vue 3 app instance and mounts to DOM

using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

// 标记为 ECMAScript 模块入口 / Marked as ECMAScript module entry point
[ECMAScriptModule("main.mjs")]
public static class AppModule
{
    // 模块加载时自动初始化 / Auto-initialize on module load
    private static readonly bool Initialized = Initialize();

    private static bool Initialize()
    {
        Boot();
        return true;
    }

    // 创建应用并挂载到 #app / Create app and mount to #app
    public static void Boot()
    {
        var app = CreateApp(WikiHomeModule.Component);
        app.Mount("#app");
    }
}
