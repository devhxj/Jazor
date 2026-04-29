using Microsoft.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Jazor.Common;

public static class Format
{
	private static SymbolDisplayFormat CreateNameFormat(SymbolDisplayExtensionMethodStyle extensionMethodStyle)
		=> new(
			globalNamespaceStyle:
				SymbolDisplayGlobalNamespaceStyle.Omitted,
			typeQualificationStyle:
				SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			genericsOptions:
				SymbolDisplayGenericsOptions.IncludeTypeParameters,
			memberOptions:
				SymbolDisplayMemberOptions.IncludeModifiers |
				SymbolDisplayMemberOptions.IncludeExplicitInterface |
				SymbolDisplayMemberOptions.IncludeParameters |
				SymbolDisplayMemberOptions.IncludeContainingType |
				SymbolDisplayMemberOptions.IncludeConstantValue |
				SymbolDisplayMemberOptions.IncludeRef,
			delegateStyle:
				SymbolDisplayDelegateStyle.NameAndParameters,
			extensionMethodStyle:
				extensionMethodStyle,
			parameterOptions:
				SymbolDisplayParameterOptions.IncludeType |
				SymbolDisplayParameterOptions.IncludeModifiers |
				SymbolDisplayParameterOptions.IncludeParamsRefOut,
			propertyStyle:
				SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
			localOptions:
				SymbolDisplayLocalOptions.IncludeType |
				SymbolDisplayLocalOptions.IncludeModifiers |
				SymbolDisplayLocalOptions.IncludeConstantValue,
			kindOptions:
				SymbolDisplayKindOptions.None,
			miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
				SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

	/// <summary>
	/// 不显示global::前缀，保留完整的命名空间路径，不显示泛型参数。
	/// </summary>
	public readonly static SymbolDisplayFormat NameFormat = CreateNameFormat(SymbolDisplayExtensionMethodStyle.InstanceMethod);

	/// <summary>
	/// 白名单条目常以静态扩展方法签名记录，例如
	/// static System.Linq.Enumerable.Where(...).
	/// 这里提供对应的静态显示格式，供 lookup 回退。
	/// </summary>
	public readonly static SymbolDisplayFormat StaticExtensionNameFormat = CreateNameFormat(SymbolDisplayExtensionMethodStyle.StaticMethod);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static string HashName(string text)
	{
		using var sha256 = SHA256.Create();
		var bytes = Encoding.UTF8.GetBytes(text);
		var hashBytes = sha256.ComputeHash(bytes);
		var sb = new StringBuilder("_");
		for (int i = 0; i < 8; i++)
			sb.Append(hashBytes[i].ToString("x2"));
		return sb.ToString();
	}
}
