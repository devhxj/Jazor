namespace Jazor.Style;

public static partial class Css
{
    private static string CombineSelectors(string parentSelector, string childSelector)
    {
        if (childSelector.IndexOf(RootSelectorToken) >= 0)
            Fail("Child selector contains the reserved Jazor.Style root selector token.");

        var parents = SplitSelectorList(parentSelector, "Parent selector");
        var children = SplitSelectorList(childSelector, "Child selector");
        var combined = new Array<string>();

        for (var parentIndex = 0; parentIndex < parents.Length; parentIndex++)
        {
            var parent = parents[parentIndex];
            for (var childIndex = 0; childIndex < children.Length; childIndex++)
            {
                var child = children[childIndex];
                var replaced = ReplaceNestingTokens(child, parent, out var hadNestingToken);
                combined.Push(hadNestingToken ? replaced : parent + " " + child);
            }
        }

        return combined.Join(",");
    }

    private static string NormalizeSelectorList(string selector, string label)
    {
        var normalized = selector.Trim();
        if (normalized.IndexOf(RootSelectorToken) >= 0)
            Fail(label + " contains the reserved Jazor.Style root selector token.");

        return SplitSelectorList(normalized, label).Join(",");
    }

    private static Array<string> SplitSelectorList(string selector, string label)
    {
        if (selector.Length == 0)
            Fail(label + " cannot be empty.");

        var result = new Array<string>();
        var start = 0;
        var parentheses = 0;
        var brackets = 0;
        var quote = "";
        var escaped = false;

        for (var index = 0; index < selector.Length; index++)
        {
            var character = selector.Substring(index, 1);
            var codeUnit = (int)selector.CharCodeAt(index);
            if (codeUnit < 32 || codeUnit == 127 || character == "{" || character == "}")
                Fail(label + " contains an invalid character.");

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == "\\")
            {
                escaped = true;
                continue;
            }

            if (quote.Length > 0)
            {
                if (character == quote)
                    quote = "";

                continue;
            }

            if (character == "\"" || character == "'")
            {
                quote = character;
                continue;
            }

            if (character == "(")
            {
                parentheses++;
                continue;
            }

            if (character == ")")
            {
                if (parentheses == 0)
                    Fail(label + " contains an unmatched closing parenthesis.");

                parentheses--;
                continue;
            }

            if (character == "[")
            {
                brackets++;
                continue;
            }

            if (character == "]")
            {
                if (brackets == 0)
                    Fail(label + " contains an unmatched closing bracket.");

                brackets--;
                continue;
            }

            if (character == "," && parentheses == 0 && brackets == 0)
            {
                AddSelectorPart(result, selector.Substring(start, index - start), label);
                start = index + 1;
            }
        }

        if (escaped || quote.Length > 0 || parentheses != 0 || brackets != 0)
            Fail(label + " contains an unclosed escape, quote, parenthesis, or bracket.");

        AddSelectorPart(result, selector.Substring(start), label);
        return result;
    }

    private static void AddSelectorPart(Array<string> result, string value, string label)
    {
        var part = value.Trim();
        if (part.Length == 0)
            Fail(label + " contains an empty selector.");

        result.Push(part);
    }

    private static string ReplaceNestingTokens(string selector, string parent, out bool hadNestingToken)
    {
        var output = new Array<string>();
        var segmentStart = 0;
        var brackets = 0;
        var quote = "";
        var escaped = false;
        hadNestingToken = false;

        for (var index = 0; index < selector.Length; index++)
        {
            var character = selector.Substring(index, 1);
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == "\\")
            {
                escaped = true;
                continue;
            }

            if (quote.Length > 0)
            {
                if (character == quote)
                    quote = "";

                continue;
            }

            if (character == "\"" || character == "'")
            {
                quote = character;
                continue;
            }

            if (character == "[")
            {
                brackets++;
                continue;
            }

            if (character == "]")
            {
                brackets--;
                continue;
            }

            if (character != "&" || brackets != 0)
                continue;

            output.Push(selector.Substring(segmentStart, index - segmentStart));
            output.Push(parent);
            segmentStart = index + 1;
            hadNestingToken = true;
        }

        if (!hadNestingToken)
            return selector;

        output.Push(selector.Substring(segmentStart));
        return output.Join("");
    }

    private static void ValidateAtRulePrelude(string prelude, string label, bool allowsEmpty)
    {
        if (prelude.Length == 0)
        {
            if (!allowsEmpty)
                Fail(label + " cannot be empty.");

            return;
        }

        if (prelude.StartsWith("@"))
            Fail(label + " cannot start with an at-rule.");

        if (prelude.IndexOf(";") >= 0 || prelude.IndexOf("{") >= 0 || prelude.IndexOf("}") >= 0)
            Fail(label + " contains an invalid structural delimiter.");

        SplitSelectorList(prelude, label);
    }

    private static string NormalizeFrameSelector(string selector)
    {
        var parts = SplitSelectorList(selector.Trim(), "Keyframe selector");
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            if (part == "from" || part == "to")
                continue;

            if (!IsPercentage(part))
                Fail("Keyframe selector must be 'from', 'to', or a percentage.");
        }

        return parts.Join(",");
    }

    private static bool IsPercentage(string value)
    {
        if (!value.EndsWith("%") || value.Length == 1)
            return false;

        var numberText = value.Substring(0, value.Length - 1);
        var hasDigit = false;
        var hasDot = false;
        for (var index = 0; index < numberText.Length; index++)
        {
            var codeUnit = (int)numberText.CharCodeAt(index);
            if (codeUnit >= 48 && codeUnit <= 57)
            {
                hasDigit = true;
                continue;
            }

            if (codeUnit == 46 && !hasDot)
            {
                hasDot = true;
                continue;
            }

            return false;
        }

        if (!hasDigit)
            return false;

        var number = NumberFn(numberText);
        return number >= 0 && number <= 100;
    }
}
