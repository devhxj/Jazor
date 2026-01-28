using Microsoft.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace ECMAScript.Common;

public static class Util
{
	/// <summary>
	/// 不显示global::前缀，保留完整的命名空间路径，不显示泛型参数。
	/// </summary>
	public readonly static SymbolDisplayFormat NameFormat = new(
		globalNamespaceStyle:
			// 不包含 
			SymbolDisplayGlobalNamespaceStyle.Omitted,
		typeQualificationStyle:
			//保留完整的命名空间路径
			SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions:
			// 不显示泛型参数
			SymbolDisplayGenericsOptions.IncludeTypeParameters,
		memberOptions:
			//SymbolDisplayMemberOptions.IncludeType |
			SymbolDisplayMemberOptions.IncludeModifiers |
			//SymbolDisplayMemberOptions.IncludeAccessibility |
			SymbolDisplayMemberOptions.IncludeExplicitInterface |
			SymbolDisplayMemberOptions.IncludeParameters |
			SymbolDisplayMemberOptions.IncludeContainingType |
			SymbolDisplayMemberOptions.IncludeConstantValue |
			SymbolDisplayMemberOptions.IncludeRef,
		delegateStyle:
			SymbolDisplayDelegateStyle.NameAndParameters,
		extensionMethodStyle:
			 SymbolDisplayExtensionMethodStyle.InstanceMethod,
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
			SymbolDisplayMiscellaneousOptions.UseSpecialTypes
	);

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
