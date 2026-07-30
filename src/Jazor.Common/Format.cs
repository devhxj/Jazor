using Microsoft.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Jazor.Common;

/// <summary>
/// 集中定义 Roslyn symbol 显示格式和稳定名称 hash 规则。
/// </summary>
/// <remarks>
/// NameFormat 生成的文本直接参与白名单 key；HashName 生成的文本参与 Compile 名称和稳定
/// 导出名。修改这些规则会改变生成产物和 lookup 契约，必须同步更新生成器、编译器和测试。
/// </remarks>
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

	/// <summary>根据完整签名生成确定性的短 hash 名称。</summary>
	/// <remarks>输入应是规范化签名，而不是源码遍历序号，否则输出会随文件顺序抖动。</remarks>
	public static string HashName(string text)
	{
		using var sha256 = SHA256.Create();
		var bytes = Encoding.UTF8.GetBytes(text);
		var hashBytes = sha256.ComputeHash(bytes);
		var sb = new StringBuilder("_");
		for (int i = 0; i < 8; i++)
			sb.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
		return sb.ToString();
	}
}
