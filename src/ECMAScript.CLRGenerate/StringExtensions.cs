using System.Globalization;
using System.Text.RegularExpressions;

namespace Jazor.Console;

public static class StringExtensions
{
	public static string ToPascalCase(this string input)
	{
		if (string.IsNullOrEmpty(input))
			return string.Empty;

		// 处理特定保留词
		if (input.Equals("uuid", StringComparison.OrdinalIgnoreCase))
			return "UUID";
		if (input.Equals("uuids", StringComparison.OrdinalIgnoreCase))
			return "UUIDs";

		// 保留全大写带下划线的标识符（SCREAMING_SNAKE_CASE）
		if (Regex.IsMatch(input, @"^[A-Z0-9_]+$") && input.Contains('_'))
			return input;

		// 转换步骤：
		// 1. 将分隔符替换为空格
		// 2. 处理驼峰命名
		// 3. 分割单词并转换
		var v1 = Regex.Replace(input, @"[_\-\s]+", " ");
		var v2 = Regex.Replace(v1,
			"([a-z])([A-Z])|([A-Z])([A-Z][a-z])",
			match => match.Groups[1].Success
				? $"{match.Groups[1].Value} {match.Groups[2].Value}"
				: $"{match.Groups[3].Value} {match.Groups[4].Value}")
			.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
 ;
		var v3 = new List<string>();
		foreach (var word in v2)
		{

			// 保留全大写的缩写（HTML, CSS, JSON等）
			if (word.All(char.IsUpper))
				v3.Add(word);

			else if ((word.Length > 1 && word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
		? word[..^1].All(char.IsUpper)   // 检查复数形式（如IDs）
		: word.All(c => char.IsUpper(c) || !char.IsLetter(c))) // 允许数字

				v3.Add(word);
			else
				// 常规单词：首字母大写 + 其余原样保留
				v3.Add(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLower()));
		}

		var transformed = string.Concat(v3);

		// 处理数字开头的标识符
		if (char.IsDigit(transformed[0]))
			return $"_{transformed}";

		return transformed;
	}
}