using static ECMAScript.Global;

namespace Jero.Web.Pages.Plugin;

/// <summary>Form normalization and deterministic starter-source generation.</summary>
[ECMAScriptModule("components/plugin-shared.js")]
public static class PluginShared
{
    public static string? Optional(string value)
    {
        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    public static string CreateDefaultCode(string name)
    {
        var displayName = name.Trim().Length == 0 ? "动态插件示例" : name.Trim();
        var identifier = BuildIdentifier(displayName);
        var routeName = identifier.Substring(0, 1).ToLowerInvariant() + identifier.Substring(1);
        var escapedName = EscapeCSharpString(displayName);

        return "using System;\n" +
            "using Furion;\n" +
            "using Furion.DynamicApiController;\n" +
            "using Microsoft.AspNetCore.Mvc;\n\n" +
            "namespace Jero.DynamicPlugins;\n\n" +
            "[ApiDescriptionSettings(\"动态插件\", Name = \"" + routeName + "\", Order = 1000, Description = \"" + escapedName + "\")]\n" +
            "public class " + identifier + "Service : IDynamicApiController\n" +
            "{\n" +
            "    [HttpGet(\"ping\")]\n" +
            "    public string Ping()\n" +
            "    {\n" +
            "        return \"" + escapedName + " 已成功加载\";\n" +
            "    }\n" +
            "}\n";
    }

    private static string BuildIdentifier(string value)
    {
        var parts = value.Replace(new RegExp("[^A-Za-z0-9]+", "g"), " ").Trim().Split(" ");
        var identifier = string.Empty;
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            if (part.Length == 0)
                continue;
            identifier += part.Substring(0, 1).ToUpperInvariant() + part.Substring(1).ToLowerInvariant();
        }

        if (identifier.Length == 0)
            return "DynamicPlugin";
        if (new RegExp("^[0-9]").Test(identifier))
            return "Plugin" + identifier;
        return identifier;
    }

    private static string EscapeCSharpString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
