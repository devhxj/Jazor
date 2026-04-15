using System.Text;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.DevServer;

internal sealed class HtmlTransformer
{
    private static readonly Regex ScriptTagPattern = new(
        @"<script\b(?<attrs>[^>]*)>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SourceAttributePattern = new(
        @"\bsrc\s*=\s*(?<quote>[""'])(?<value>[^""']+)\k<quote>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TypeAttributePattern = new(
        @"\btype\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private const string DevClientPath = "/@jazor/client";
    private const string VueImportMap = """
        <script type="importmap">
        {
          "imports": {
            "vue": "https://esm.sh/vue@3?dev"
          }
        }
        </script>
        """;

    private readonly DevServerOptions _options;

    public HtmlTransformer(DevServerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string Transform(string html)
        => Transform(html, htmlPath: null);

    public string Transform(string html, string? htmlPath)
    {
        ArgumentNullException.ThrowIfNull(html);

        var transformedHtml = RewriteEntryScripts(html);
        var builder = new StringBuilder(transformedHtml.Length + 256);
        builder.Append(VueImportMap)
            .AppendLine();

        if (_options.HmrEnabled)
        {
            builder.Append("<script type=\"module\" src=\"")
                .Append(DevClientPath)
                .AppendLine("\"></script>");
        }

        var injection = builder.ToString();
        var headIndex = transformedHtml.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headIndex >= 0)
        {
            return transformedHtml.Insert(headIndex, injection);
        }

        var bodyIndex = transformedHtml.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyIndex >= 0)
        {
            return transformedHtml.Insert(bodyIndex, injection);
        }

        return injection + transformedHtml;
    }

    public static string GetDevClientScript()
        => """
        const socketProtocol = location.protocol === "https:" ? "wss" : "ws";
        const socketUrl = `${socketProtocol}://${location.host}/@jazor/hmr`;
        const hotModules = new Map();
        function normalizeModulePath(value) {
          if (typeof value !== "string" || value.length === 0) {
            return "";
          }
          try {
            return new URL(value, location.href).pathname;
          } catch {
            return value.split(/[?#]/, 1)[0];
          }
        }
        function getHotRecord(ownerPath) {
          let record = hotModules.get(ownerPath);
          if (!record) {
            record = { data: {}, acceptCallbacks: [], disposeCallbacks: [] };
            hotModules.set(ownerPath, record);
          }
          return record;
        }
        function appendTimestamp(path, timestamp) {
          const url = new URL(path, location.href);
          url.searchParams.set("t", String(timestamp ?? Date.now()));
          return url.pathname + url.search + url.hash;
        }
        globalThis.__JAZOR_HMR__ ??= {
          createHotContext(ownerUrl) {
            const ownerPath = normalizeModulePath(ownerUrl);
            const record = getHotRecord(ownerPath);
            record.acceptCallbacks = [];
            record.disposeCallbacks = [];
            return {
              get data() {
                return record.data;
              },
              accept(depsOrCallback, callback) {
                if (Array.isArray(depsOrCallback)) {
                  const dependencyCallback = typeof callback === "function" ? callback : () => {};
                  record.acceptCallbacks.push((module) => dependencyCallback([module]));
                  return;
                }
                record.acceptCallbacks.push(typeof depsOrCallback === "function" ? depsOrCallback : () => {});
              },
              dispose(callback) {
                if (typeof callback === "function") {
                  record.disposeCallbacks.push(callback);
                }
              },
              prune(callback) {
                if (typeof callback === "function") {
                  record.disposeCallbacks.push(callback);
                }
              },
              invalidate(message) {
                if (message) {
                  console.warn("[jazor] HMR invalidated:", message);
                }
                location.reload();
              },
              send() {},
              on() {},
              off() {}
            };
          }
        };
        async function applyJavaScriptUpdate(update, timestamp) {
          const acceptedPath = normalizeModulePath(update?.acceptedPath ?? update?.path);
          const updatePath = update?.path ?? acceptedPath;
          const record = hotModules.get(acceptedPath);
          if (!acceptedPath || !updatePath || !record || record.acceptCallbacks.length === 0) {
            location.reload();
            return;
          }
          const acceptCallbacks = [...record.acceptCallbacks];
          const disposeCallbacks = [...record.disposeCallbacks];
          for (const dispose of disposeCallbacks) {
            await dispose(record.data);
          }
          const updatedModule = await import(appendTimestamp(updatePath, update?.timestamp ?? timestamp));
          for (const accept of acceptCallbacks) {
            await accept(updatedModule);
          }
        }
        async function applyUpdates(updates, timestamp) {
          if (!Array.isArray(updates) || updates.length === 0) {
            return;
          }
          for (const update of updates) {
            if (update?.type === "js-update") {
              await applyJavaScriptUpdate(update, timestamp);
            }
          }
        }
        function refreshStyleSheets(paths, timestamp) {
          if (!Array.isArray(paths) || paths.length === 0) {
            return;
          }
          const pathSet = new Set(paths);
          for (const link of document.querySelectorAll('link[rel="stylesheet"][href]')) {
            const currentUrl = new URL(link.getAttribute("href"), location.href);
            if (!pathSet.has(currentUrl.pathname)) {
              continue;
            }
            currentUrl.searchParams.set("t", String(timestamp ?? Date.now()));
            link.href = currentUrl.pathname + currentUrl.search + currentUrl.hash;
          }
        }
        function refreshInlineStyles(styles) {
          if (!Array.isArray(styles) || styles.length === 0) {
            return;
          }
          for (const styleUpdate of styles) {
            if (!styleUpdate?.path) {
              continue;
            }
            let style = document.querySelector(`style[data-jazor-vuehost="${styleUpdate.path}"]`);
            if (!style) {
              style = document.createElement("style");
              style.setAttribute("data-jazor-vuehost", styleUpdate.path);
              document.head.appendChild(style);
            }
            style.textContent = styleUpdate.content ?? "";
          }
        }
        function clearErrorOverlay() {
          document.getElementById("__jazor-error-overlay")?.remove();
        }
        function showErrorOverlay(message) {
          let overlay = document.getElementById("__jazor-error-overlay");
          if (!overlay) {
            overlay = document.createElement("div");
            overlay.id = "__jazor-error-overlay";
            overlay.style.cssText = "position:fixed;top:0;left:0;right:0;z-index:99999;padding:12px 16px;background:#c62828;color:#fff;font:14px/1.5 monospace;white-space:pre-wrap;box-shadow:0 2px 8px rgba(0,0,0,.3);";
            document.body.appendChild(overlay);
          }
          overlay.textContent = `[jazor] ${message ?? "Hot update failed."}`;
        }
        let socket;
        let reconnectTimer;
        let heartbeatTimer;
        function sendMessage(payload) {
          if (!socket || socket.readyState !== WebSocket.OPEN) {
            return;
          }
          socket.send(JSON.stringify(payload));
        }
        function stopHeartbeat() {
          if (!heartbeatTimer) {
            return;
          }
          clearInterval(heartbeatTimer);
          heartbeatTimer = undefined;
        }
        function startHeartbeat() {
          stopHeartbeat();
          heartbeatTimer = setInterval(() => {
            sendMessage({ type: "heartbeat" });
          }, 15000);
        }
        function scheduleReconnect() {
          if (reconnectTimer) {
            return;
          }
          stopHeartbeat();
          reconnectTimer = setTimeout(() => {
            reconnectTimer = undefined;
            connect();
          }, 2000);
        }
        function handleSocketMessage(event) {
          let payload;
          try {
            payload = JSON.parse(event.data);
          } catch {
            payload = { type: event.data };
          }
          if (payload?.type === "reload" || payload?.type === "full-reload") {
            location.reload();
            return;
          }
          if (payload?.type === "connected") {
            clearErrorOverlay();
            return;
          }
          if (payload?.type === "error") {
            console.error("[jazor] HMR error:", payload.message);
            showErrorOverlay(payload.message);
            return;
          }
          if (payload?.type === "update") {
            clearErrorOverlay();
            applyUpdates(payload.updates, payload.timestamp).catch((error) => {
              console.error("[jazor] HMR update failed:", error);
              location.reload();
            });
            return;
          }
          if (payload?.type === "style-update") {
            clearErrorOverlay();
            refreshStyleSheets(payload.paths, payload.timestamp);
            refreshInlineStyles(payload.inlineStyles);
          }
        }
        function connect() {
          try {
            socket = new WebSocket(socketUrl);
            socket.addEventListener("open", () => {
              if (reconnectTimer) {
                clearTimeout(reconnectTimer);
                reconnectTimer = undefined;
              }
              sendMessage({ type: "ready" });
              startHeartbeat();
            });
            socket.addEventListener("message", handleSocketMessage);
            socket.addEventListener("close", () => {
              stopHeartbeat();
              scheduleReconnect();
            });
            socket.addEventListener("error", () => {
              stopHeartbeat();
              socket?.close();
            });
          } catch {
            scheduleReconnect();
          }
        }
        connect();
        export {};
        """;

    private static string RewriteEntryScripts(string html)
        => ScriptTagPattern.Replace(html, static match =>
        {
            var tag = match.Value;
            var attrs = match.Groups["attrs"].Value;
            if (TypeAttributePattern.IsMatch(attrs))
            {
                return tag;
            }

            var sourceMatch = SourceAttributePattern.Match(attrs);
            if (!sourceMatch.Success)
            {
                return tag;
            }

            var source = sourceMatch.Groups["value"].Value;
            if (!IsLocalModuleEntry(source))
            {
                return tag;
            }

            return tag.Insert("<script".Length, " type=\"module\"");
        });

    private static bool IsLocalModuleEntry(string source)
    {
        if (string.IsNullOrWhiteSpace(source)
            || source.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("//", StringComparison.Ordinal)
            || source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var path = StripQueryAndHash(source);
        return path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".vue", StringComparison.OrdinalIgnoreCase);
    }

    private static string StripQueryAndHash(string value)
    {
        var index = value.IndexOfAny(['?', '#']);
        return index >= 0 ? value[..index] : value;
    }
}
