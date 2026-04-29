using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Jazor.Compiler;

internal sealed class UniqueNameSession
{
    private const string Version = "jz2";

    private readonly OperationIdentityIndex _operationIndex;

    public UniqueNameSession(IOperation root, ScopeSite rootSite)
    {
        if (root is null)
            throw new System.ArgumentNullException(nameof(root));

        OwnerKey = CreateOwnerKey(root);
        _operationIndex = new OperationIdentityIndex(root);
        RootScope = EmissionScopeContext.CreateRoot(this, root, rootSite);
    }

    public string OwnerKey { get; }

    public EmissionScopeContext RootScope { get; }

    public string GetOperationIdentity(IOperation operation)
        => _operationIndex.GetIdentity(operation);

    public string CreateScopeKey(string? parentScopeKey, ScopeSite site)
    {
        var builder = new StringBuilder();
        builder.Append("scope|").Append(Version).Append('|');
        builder.Append(OwnerKey).Append('|');
        builder.Append(parentScopeKey ?? "<root>").Append('|');
        builder.Append(site.Kind);
        return "sc_" + HashHex(builder.ToString(), 24);
    }

    public string CreateName(LoweringSite site, string scopeKey, LoweringNameOwner owner, string salt)
    {
        var builder = new StringBuilder();
        builder.Append("name|").Append(Version).Append('|');
        builder.Append(OwnerKey).Append('|');
        builder.Append(scopeKey).Append('|');
        builder.Append(site.Kind).Append('|');
        builder.Append(site.Slot).Append('|');
        builder.Append(owner.StableKey).Append('|');
        builder.Append(salt);
        return "__" + site.Tag + "$" + HashHex(builder.ToString(), 24);
    }

    public static string HashHex(string text, int hexLength)
    {
        if (hexLength <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(hexLength));

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hashBytes = sha256.ComputeHash(bytes);
        var builder = new StringBuilder(hashBytes.Length * 2);
        foreach (var value in hashBytes)
        {
            builder.Append(value.ToString("x2"));
            if (builder.Length >= hexLength)
                break;
        }

        if (builder.Length > hexLength)
            builder.Length = hexLength;

        return builder.ToString();
    }

    private static string CreateOwnerKey(IOperation root)
    {
        var documentKey = CreateDocumentKey(root);
        var symbol = root.SemanticModel?.GetEnclosingSymbol(root.Syntax.SpanStart);
        if (symbol is not null)
            return documentKey + "|" + symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);

        var builder = new StringBuilder();
        builder.Append(documentKey).Append('|');
        builder.Append(root.Kind).Append('|');
        builder.Append(root.Type?.OriginalDefinition.ToDisplayString(Format.NameFormat) ?? "<null>");
        return builder.ToString();
    }

    private static string CreateDocumentKey(IOperation root)
    {
        var filePath = NormalizePath(root.Syntax.SyntaxTree.FilePath);
        return string.IsNullOrEmpty(filePath)
            ? "<memory>"
            : filePath;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var nonNullPath = path!;
        var normalized = nonNullPath.Replace('\\', '/');
        if (Path.IsPathRooted(nonNullPath))
            normalized = Path.GetFullPath(nonNullPath).Replace('\\', '/');

        normalized = normalized.TrimEnd('/');
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? normalized.ToLowerInvariant()
            : normalized;
    }
}
