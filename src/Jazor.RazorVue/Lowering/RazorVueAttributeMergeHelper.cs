using System;
using System.Collections.Generic;
using System.Text;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueAttributeMergeHelper
{
    public const string HelperName = "__jazorVueMergeAttributes";

    public static string BuildInvocation(IEnumerable<string> segments)
        => HelperName + "(" + string.Join(", ", segments) + ")";

    public static bool ContainsInvocation(string text)
        => !string.IsNullOrWhiteSpace(text) &&
           text.Contains(HelperName + "(", StringComparison.Ordinal);

    public static void AppendHelper(StringBuilder builder, string indent)
    {
        builder.Append(indent).AppendLine("function __jazorVueAssignMergedAttribute(target, key, value) {");
        builder.Append(indent).AppendLine("  if (typeof key !== \"string\" || key.length === 0) {");
        builder.Append(indent).AppendLine("    throw new Error(\"RazorVue attribute spread encountered a non-string attribute name.\");");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  target[key] = value;");
        builder.Append(indent).AppendLine("}");
        builder.AppendLine();
        builder.Append(indent).AppendLine("function __jazorVueAssignMergedAttributeEntry(target, entry) {");
        builder.Append(indent).AppendLine("  if (Array.isArray(entry)) {");
        builder.Append(indent).AppendLine("    if (entry.length < 2) {");
        builder.Append(indent).AppendLine("      throw new Error(\"RazorVue attribute spread encountered an entry tuple without both name and value.\");");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    __jazorVueAssignMergedAttribute(target, entry[0], entry[1]);");
        builder.Append(indent).AppendLine("    return;");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  if (entry && typeof entry === \"object\") {");
        builder.Append(indent).AppendLine("    if (\"Key\" in entry && \"Value\" in entry) {");
        builder.Append(indent).AppendLine("      __jazorVueAssignMergedAttribute(target, entry.Key, entry.Value);");
        builder.Append(indent).AppendLine("      return;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (\"key\" in entry && \"value\" in entry) {");
        builder.Append(indent).AppendLine("      __jazorVueAssignMergedAttribute(target, entry.key, entry.value);");
        builder.Append(indent).AppendLine("      return;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  throw new Error(\"RazorVue attribute spread only supports object-like dictionaries, Maps, or key/value entry sequences.\");");
        builder.Append(indent).AppendLine("}");
        builder.AppendLine();
        builder.Append(indent).AppendLine("function __jazorVueMergeAttributes(...sources) {");
        builder.Append(indent).AppendLine("  const result = {};");
        builder.Append(indent).AppendLine("  for (const source of sources) {");
        builder.Append(indent).AppendLine("    if (source === null || source === undefined) {");
        builder.Append(indent).AppendLine("      continue;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (source instanceof Map) {");
        builder.Append(indent).AppendLine("      for (const entry of source) {");
        builder.Append(indent).AppendLine("        __jazorVueAssignMergedAttributeEntry(result, entry);");
        builder.Append(indent).AppendLine("      }");
        builder.Append(indent).AppendLine("      continue;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (typeof source !== \"string\" && typeof source[Symbol.iterator] === \"function\") {");
        builder.Append(indent).AppendLine("      for (const entry of source) {");
        builder.Append(indent).AppendLine("        __jazorVueAssignMergedAttributeEntry(result, entry);");
        builder.Append(indent).AppendLine("      }");
        builder.Append(indent).AppendLine("      continue;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    if (typeof source === \"object\") {");
        builder.Append(indent).AppendLine("      for (const key of Object.keys(source)) {");
        builder.Append(indent).AppendLine("        __jazorVueAssignMergedAttribute(result, key, source[key]);");
        builder.Append(indent).AppendLine("      }");
        builder.Append(indent).AppendLine("      continue;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    throw new Error(\"RazorVue attribute spread only supports object-like dictionaries, Maps, or key/value entry sequences.\");");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  return result;");
        builder.Append(indent).AppendLine("}");
        builder.AppendLine();
    }
}
