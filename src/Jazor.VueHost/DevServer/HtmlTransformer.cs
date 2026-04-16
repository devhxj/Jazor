using System.Text;
using System.Text.RegularExpressions;
using Jazor.VueHost.Build;

namespace Jazor.VueHost.DevServer;

internal sealed class HtmlTransformer
{
    private static readonly Regex ScriptElementPattern = new(
        @"<script\b(?<attrs>[^>]*)>.*?</script>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex ScriptTagPattern = new(
        @"<script\b(?<attrs>[^>]*)>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SourceAttributePattern = new(
        @"\bsrc\s*=\s*(?<quote>[""'])(?<value>[^""']+)\k<quote>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AssetUrlAttributePattern = new(
        @"\b(?<name>src|href)\s*=\s*(?<quote>[""'])(?<value>[^""']+)\k<quote>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SrcSetAttributePattern = new(
        @"\b(?<name>srcset)\s*=\s*(?<quote>[""'])(?<value>[^""']+)\k<quote>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MetaElementPattern = new(
        @"<meta\b(?<attrs>[^>]*)>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HtmlAttributePattern = new(
        @"\b(?<name>[^\s=/>]+)\s*=\s*(?<quote>[""'])(?<value>[^""']*)\k<quote>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TypeAttributePattern = new(
        @"\btype\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly HashSet<string> AssetMetaNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "og:image",
        "og:image:url",
        "og:image:secure_url",
        "twitter:image",
        "twitter:image:src",
        "msapplication-tileimage"
    };
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

    /// <summary>
    /// Inject a production script tag into HTML, placed before </body>.
    /// </summary>
    public static string InjectScript(string html, string scriptPath)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(scriptPath);

        var scriptTag = $"<script type=\"module\" src=\"{scriptPath}\"></script>";
        var bodyIndex = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyIndex >= 0)
        {
            return html.Insert(bodyIndex, scriptTag);
        }

        return html + scriptTag;
    }

    /// <summary>
    /// Inject a CSS link tag into HTML, placed before </head>.
    /// </summary>
    public static string InjectCss(string html, string cssPath)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(cssPath);

        var linkTag = $"<link rel=\"stylesheet\" href=\"{cssPath}\">";
        var headIndex = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headIndex >= 0)
        {
            return html.Insert(headIndex, linkTag);
        }

        var bodyIndex = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
        if (bodyIndex >= 0)
        {
            return html.Insert(bodyIndex, linkTag);
        }

        return linkTag + html;
    }

    /// <summary>
    /// Remove script tags that reference dev-mode /src/ paths.
    /// </summary>
    public static string RemoveDevScriptRefs(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        return ScriptElementPattern.Replace(html, static match =>
        {
            var sourceMatch = SourceAttributePattern.Match(match.Groups["attrs"].Value);
            if (!sourceMatch.Success)
            {
                return match.Value;
            }

            var source = NormalizeScriptPath(sourceMatch.Groups["value"].Value);
            return source.StartsWith("/src/", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : match.Value;
        });
    }

    public static string RemoveScriptReference(string html, string scriptPath)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptPath);

        var normalizedScriptPath = NormalizeScriptPath(scriptPath);
        return ScriptElementPattern.Replace(html, match =>
        {
            var sourceMatch = SourceAttributePattern.Match(match.Groups["attrs"].Value);
            if (!sourceMatch.Success)
            {
                return match.Value;
            }

            return string.Equals(
                NormalizeScriptPath(sourceMatch.Groups["value"].Value),
                normalizedScriptPath,
                StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : match.Value;
        });
    }

    public static string RewriteAssetReferences(string html, IReadOnlyList<AssetInfo> assets)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(assets);

        if (assets.Count == 0)
        {
            return html;
        }

        var assetMap = assets
            .Where(static asset => !string.IsNullOrWhiteSpace(asset.OriginalPath)
                && !string.IsNullOrWhiteSpace(asset.FilePath))
            .Select(static asset => new KeyValuePair<string, string>(
                NormalizeAssetLookupPath(asset.OriginalPath!)!,
                NormalizeOutputAssetPath(asset.FilePath)))
            .ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);
        if (assetMap.Count == 0)
        {
            return html;
        }

        var rewrittenHtml = AssetUrlAttributePattern.Replace(
            html,
            match => RewriteAssetAttribute(match, assetMap));
        rewrittenHtml = SrcSetAttributePattern.Replace(
            rewrittenHtml,
            match => RewriteSrcSetAttribute(match, assetMap));
        return MetaElementPattern.Replace(
            rewrittenHtml,
            match => RewriteMetaElement(match, assetMap));
    }

    private static string RewriteAssetAttribute(Match match, IReadOnlyDictionary<string, string> assetMap)
    {
        var originalValue = match.Groups["value"].Value;
        var normalizedPath = NormalizeAssetLookupPath(originalValue);
        if (normalizedPath is null || !assetMap.TryGetValue(normalizedPath, out var rewrittenPath))
        {
            return match.Value;
        }

        var suffix = ExtractQueryAndHashSuffix(originalValue);
        return $"{match.Groups["name"].Value}={match.Groups["quote"].Value}{rewrittenPath}{suffix}{match.Groups["quote"].Value}";
    }

    private static string RewriteSrcSetAttribute(Match match, IReadOnlyDictionary<string, string> assetMap)
    {
        var originalValue = match.Groups["value"].Value;
        var candidates = originalValue.Split(',');
        var rewritten = false;

        for (var index = 0; index < candidates.Length; index++)
        {
            var rewrittenCandidate = RewriteSrcSetCandidate(candidates[index], assetMap);
            if (!string.Equals(rewrittenCandidate, candidates[index], StringComparison.Ordinal))
            {
                candidates[index] = rewrittenCandidate;
                rewritten = true;
            }
        }

        if (!rewritten)
        {
            return match.Value;
        }

        return $"{match.Groups["name"].Value}={match.Groups["quote"].Value}{string.Join(",", candidates)}{match.Groups["quote"].Value}";
    }

    private static string RewriteSrcSetCandidate(string candidate, IReadOnlyDictionary<string, string> assetMap)
    {
        var urlStart = 0;
        while (urlStart < candidate.Length && char.IsWhiteSpace(candidate[urlStart]))
        {
            urlStart++;
        }

        if (urlStart >= candidate.Length)
        {
            return candidate;
        }

        var urlEnd = urlStart;
        while (urlEnd < candidate.Length && !char.IsWhiteSpace(candidate[urlEnd]))
        {
            urlEnd++;
        }

        var originalUrl = candidate[urlStart..urlEnd];
        var normalizedPath = NormalizeAssetLookupPath(originalUrl);
        if (normalizedPath is null || !assetMap.TryGetValue(normalizedPath, out var rewrittenPath))
        {
            return candidate;
        }

        var suffix = ExtractQueryAndHashSuffix(originalUrl);
        return candidate[..urlStart] + rewrittenPath + suffix + candidate[urlEnd..];
    }

    private static string RewriteMetaElement(Match match, IReadOnlyDictionary<string, string> assetMap)
    {
        var attributes = match.Groups["attrs"].Value;
        Match? contentAttribute = null;
        var isAssetMeta = false;

        foreach (Match attributeMatch in HtmlAttributePattern.Matches(attributes))
        {
            var attributeName = attributeMatch.Groups["name"].Value;
            var attributeValue = attributeMatch.Groups["value"].Value.Trim();

            if (string.Equals(attributeName, "content", StringComparison.OrdinalIgnoreCase))
            {
                contentAttribute = attributeMatch;
                continue;
            }

            if (IsAssetMetaAttribute(attributeName, attributeValue))
            {
                isAssetMeta = true;
            }
        }

        if (!isAssetMeta || contentAttribute is null)
        {
            return match.Value;
        }

        var originalValue = contentAttribute.Groups["value"].Value;
        var normalizedPath = NormalizeAssetLookupPath(originalValue);
        if (normalizedPath is null || !assetMap.TryGetValue(normalizedPath, out var rewrittenPath))
        {
            return match.Value;
        }

        var suffix = ExtractQueryAndHashSuffix(originalValue);
        var valueStart = contentAttribute.Groups["value"].Index;
        var valueEnd = valueStart + contentAttribute.Groups["value"].Length;
        var rewrittenAttributes = attributes[..valueStart]
            + rewrittenPath
            + suffix
            + attributes[valueEnd..];

        return "<meta" + rewrittenAttributes + ">";
    }

    private static bool IsAssetMetaAttribute(string attributeName, string attributeValue)
    {
        if (string.IsNullOrWhiteSpace(attributeValue))
        {
            return false;
        }

        return string.Equals(attributeName, "itemprop", StringComparison.OrdinalIgnoreCase)
            ? string.Equals(attributeValue, "image", StringComparison.OrdinalIgnoreCase)
            : (string.Equals(attributeName, "property", StringComparison.OrdinalIgnoreCase)
                || string.Equals(attributeName, "name", StringComparison.OrdinalIgnoreCase))
                && AssetMetaNames.Contains(attributeValue);
    }

    private static string NormalizeScriptPath(string value)
    {
        var normalized = StripQueryAndHash(value).Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }

        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.StartsWith("/", StringComparison.Ordinal)
            ? normalized
            : "/" + normalized.TrimStart('/');
    }

    private static string? NormalizeAssetLookupPath(string value)
    {
        var normalized = StripQueryAndHash(value).Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || normalized.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith('#'))
        {
            return null;
        }

        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.StartsWith("/", StringComparison.Ordinal)
            ? normalized
            : "/" + normalized.TrimStart('/');
    }

    private static string ExtractQueryAndHashSuffix(string value)
    {
        var index = value.IndexOfAny(['?', '#']);
        return index >= 0 ? value[index..] : string.Empty;
    }

    private static string NormalizeOutputAssetPath(string value)
    {
        var normalized = value.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized;
    }
}
