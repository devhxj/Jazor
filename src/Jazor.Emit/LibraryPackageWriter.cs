using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jazor.Emit;

/// <summary>
/// Writes the package project consumed by package-aware tools. Embedded resources remain local
/// packages; npm and JSR identities are declared in the root project for DenoHost restore.
/// </summary>
internal static class LibraryPackageWriter
{
    private const string PackageProjectName = "@jazor/generated";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void WritePackageProject(string workspaceRoot, LibraryAssets libraries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        ArgumentNullException.ThrowIfNull(libraries);

        var dependencies = new JsonObject();
        var managedPackages = new JsonObject();

        // identity 记录覆盖全部已选绑定（含自有源码 carrier），作为诊断与门禁证据；
        // dependency 只为外部 npm/JSR 包写入。
        foreach (var reference in libraries.PackageReferences.Values
                     .OrderBy(static reference => reference.Name, StringComparer.Ordinal))
        {
            var isExternal = reference.Source is "npm" or "jsr";
            if (isExternal)
            {
                // External packages are resolved by Deno/npm. Never copy a binding snapshot into
                // node_modules: doing so hides upstream exports/sideEffects and defeats tree
                // shaking.
                AddDependency(dependencies, reference.CanonicalName, LibraryPackageIdentity.GetDependencySpecifier(reference));
            }

            // ECMAScript 自有源码 carrier 是项目源码（clr/**、runtime/**、dist/**），按相对
            // import 解析；它不出现在 dependencies 里，但保留 identity 供诊断与门禁比对。
            managedPackages[reference.Name] = new JsonObject
            {
                ["source"] = reference.Source,
                ["version"] = reference.Version,
                ["canonicalName"] = reference.CanonicalName,
                ["specifier"] = reference.Name
            };
            if (!string.IsNullOrWhiteSpace(reference.Integrity))
                managedPackages[reference.Name]! ["integrity"] = reference.Integrity;
        }

        var rootPackage = new JsonObject
        {
            ["name"] = PackageProjectName,
            ["private"] = true,
            ["type"] = "module",
            ["dependencies"] = dependencies,
            // npm/Deno ignore this namespace. It keeps the source and digest that produced an
            // exact dependency available to DenoHost's frozen restore diagnostics.
            ["jazor"] = new JsonObject { ["packages"] = managedPackages }
        };
        WriteJson(Path.Combine(workspaceRoot, "package.json"), rootPackage);
        // package-lock.json 是 npm 的锁图，不是 Deno 图的第二份手写描述。
        // Emit 不再合成它：deno.lock 由 Deno 生成并冻结，完整的 npm lock 作为生态兼容输入
        // 由消费者项目自带。残留的合成 lock 会让 Deno 在解析真实传递图之前就拒绝项目。
        DeleteFile(Path.Combine(workspaceRoot, "package-lock.json"));
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }





    private static void WriteJson(string path, JsonObject value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(
            path,
            value.ToJsonString(JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }


    private static void AddDependency(JsonObject dependencies, string name, string value)
    {
        if (dependencies[name] is { } existing)
        {
            if (!string.Equals(existing.GetValue<string>(), value, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Package '{name}' is declared with conflicting dependency identities '{existing.GetValue<string>()}' and '{value}'.");
            }

            return;
        }

        dependencies[name] = value;
    }


}
