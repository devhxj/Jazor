#!/usr/bin/env dotnet run


using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

const string bundlePath = "src/Jero.Web/Pages/FormDesign/runtime/designer.umd.js";
const string baseHash = "aa4cfb7a50db71885b8902ac3d35e2cbb33d892521923fb930268c3faa378da4";
const string patchedHash = "41638c71e37a8e649dd3a8a64b60f22fc9383d1219b9a06be50eb65a2b0d5ff0";
if (args is ["--check"])
{
    if (Hash(await File.ReadAllTextAsync(bundlePath)) != patchedHash)
        throw new InvalidOperationException("The vendored VForm bundle differs from the verified patch.");
    Console.WriteLine("Verified the vendored VForm Radio patch.");
    return;
}
if (args is not [var sourcePath])
    throw new ArgumentException("Usage: patch-vform-radio.cs <base-designer.umd.js> | --check");
var source = await File.ReadAllTextAsync(sourcePath);
if (Hash(source) != baseHash)
    throw new InvalidOperationException("The pinned VForm patch input does not match its recorded SHA256.");

var renderChanges = 0;
var renderFunctions = 0;
source = Regex.Replace(source, @"function _sfc_render\$\w+\([^\n]*?}(?=var )", render =>
{
    var aliases = Regex.Matches(render.Value,
        "(?<alias>\\w+)=require\\$\\$0\\$1\\.resolveComponent\\(\"el-radio(?:-button)?\"\\)");
    if (aliases.Count == 0)
        return render.Value;
    ++renderFunctions;
    var patched = render.Value;
    foreach (Match alias in aliases)
    {
        // Only radio VNode props change. Form-item labels and displayed option text stay intact.
        var pattern = @"(?<prefix>(?:createVNode|createBlock)\(" + alias.Groups["alias"].Value + @",\{(?:key:[^,{}]+,)?)(?:label):";
        patched = Regex.Replace(patched, pattern, match =>
        {
            ++renderChanges;
            return match.Groups["prefix"].Value + "value:";
        });
    }
    // Vue's dynamic-prop list must follow the renamed prop on the radio widget.
    return patched.Replace("[\"label\",\"disabled\",\"border\",\"style\"]", "[\"value\",\"disabled\",\"border\",\"style\"]", StringComparison.Ordinal);
});
if (renderFunctions != 6 || renderChanges != 16)
    throw new InvalidOperationException($"VForm render patch scope changed: {renderFunctions} functions / {renderChanges} radio props.");

source = ReplaceOnce(source,
    "resolveComponent(\"el-radio-button\"),{label:g.value}",
    "resolveComponent(\"el-radio-button\"),{value:g.value}");
var radioBuilderStart = source.IndexOf("function buildRadioChildren(", StringComparison.Ordinal);
var radioBuilderEnd = source.IndexOf("function buildCheckboxChildren(", radioBuilderStart, StringComparison.Ordinal);
var radioBuilder = source[radioBuilderStart..radioBuilderEnd];
source = source[..radioBuilderStart] + ReplaceOnce(radioBuilder, ":label=\"item.value\"", ":value=\"item.value\"") + source[radioBuilderEnd..];

var hash = Hash(source);
if (hash != patchedHash)
    throw new InvalidOperationException("Unexpected VForm Radio patch result.");
await File.WriteAllTextAsync(bundlePath, source, new UTF8Encoding(false));
Console.WriteLine($"VForm Radio patch written. SHA256={hash}");

static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

static string ReplaceOnce(string value, string before, string after)
{
    if (Regex.Matches(value, Regex.Escape(before)).Count != 1)
        throw new InvalidOperationException("The pinned VForm patch fragment changed: " + before);
    return value.Replace(before, after, StringComparison.Ordinal);
}
