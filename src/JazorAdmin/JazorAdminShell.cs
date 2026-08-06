// Provides the host-rendered document shell for the generated RazorVue application.
// 提供 RazorVue 生成应用的宿主文档壳；开发源码不保留静态 HTML、JS 或 CSS 文件。
namespace JazorAdmin;

internal static class JazorAdminShell
{
    private const string Document = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>JazorAdmin</title>
          <script type="importmap">
          {
            "imports": {
              "vue": "https://cdn.jsdelivr.net/npm/vue@3.5.13/dist/vue.runtime.esm-browser.prod.js",
              "npm:vue@3": "https://cdn.jsdelivr.net/npm/vue@3.5.13/dist/vue.runtime.esm-browser.prod.js",
              "npm:vue@3.mjs": "https://cdn.jsdelivr.net/npm/vue@3.5.13/dist/vue.runtime.esm-browser.prod.js",
              "npm:vue-router@4": "https://cdn.jsdelivr.net/npm/vue-router@4.6.4/dist/vue-router.esm-browser.prod.js",
              "npm:vue-router@4.mjs": "https://cdn.jsdelivr.net/npm/vue-router@4.6.4/dist/vue-router.esm-browser.prod.js",
              "tdesign-vue-next": "https://esm.sh/tdesign-vue-next@1.20.5/es2022/tdesign-vue-next.bundle.mjs?external=vue",
              "npm:tdesign-vue-next": "https://esm.sh/tdesign-vue-next@1.20.5/es2022/tdesign-vue-next.bundle.mjs?external=vue",
              "style.mjs": "/jazor/style.mjs",
              "@jazor/vue-runtime/": "/jazor/@jazor/vue-runtime/",
              "components/": "/jazor/components/",
              "System/": "/jazor/System/"
            }
          }
          </script>
        </head>
        <body>
          <div id="app"></div>
          <script type="module" src="/jazor/components/jazor-admin-bootstrap.mjs"></script>
        </body>
        </html>
        """;

    public static Task WriteAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        return HttpMethods.IsHead(context.Request.Method)
            ? Task.CompletedTask
            : context.Response.WriteAsync(Document, cancellationToken);
    }
}
