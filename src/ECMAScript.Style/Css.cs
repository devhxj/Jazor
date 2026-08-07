namespace ECMAScript.Style;

[ECMAScriptModule("style.mjs")]
public static partial class css
{
    private const string VersionPrefix = "ecmascript-style:v1\0";
    private const string RootSelectorToken = ".__ecmascript_style_root__";
    private const string DefaultStyleId = "ecmascript-style";
    private const string AdditionalKey = "$additional";
    private const string ChildrenKey = "$children";

    private static readonly CssContext DefaultContext = createContextCore(new CssOptions());

    public static string style(CssRule rule)
        => style(DefaultContext, rule);

    [ECMAScriptName("styleIn")]
    public static string style(CssContext context, CssRule rule)
    {
        var canonicalBody = SerializeRule(rule, RootSelectorToken);
        var canonical = VersionPrefix + "class\0" + canonicalBody;
        if (context.NamesByCanonical.Has(canonical))
        {
            EnsureDomStyle(context);
            return context.NamesByCanonical.Get(canonical)!;
        }

        var name = "ecs-" + Hash(canonical);
        var body = SerializeRule(rule, "." + name);
        Register(context, canonical, name, body);
        return name;
    }

    public static string keyframes([PreserveParamsArray] params CssFrame[] frames)
        => keyframes(DefaultContext, frames);

    [ECMAScriptName("keyframesIn")]
    public static string keyframes(CssContext context, CssFrame[] frames)
    {
        if (frames.Length == 0)
            Fail("Keyframes requires at least one frame.");

        var canonicalFrames = SerializeFrames(frames);
        var canonical = VersionPrefix + "keyframes\0" + canonicalFrames;
        if (context.NamesByCanonical.Has(canonical))
        {
            EnsureDomStyle(context);
            return context.NamesByCanonical.Get(canonical)!;
        }

        var name = "ecs-k-" + Hash(canonical);
        Register(context, canonical, name, "@keyframes " + name + "{" + canonicalFrames + "}");
        return name;
    }

    public static void global(string selector, CssRule rule)
        => global(DefaultContext, selector, rule);

    [ECMAScriptName("globalIn")]
    public static void global(CssContext context, string selector, CssRule rule)
    {
        var normalizedSelector = NormalizeSelectorList(selector, "Global selector");
        if (normalizedSelector.StartsWith("@"))
            Fail("Global selector cannot be an at-rule.");

        var body = SerializeRule(rule, normalizedSelector);
        var canonical = VersionPrefix + "global\0" + body;
        var id = "ecs-g-" + Hash(canonical);
        Register(context, canonical, id, body);
    }

    public static void atRule(CssAtRule rule)
        => atRule(DefaultContext, rule);

    [ECMAScriptName("atRuleIn")]
    public static void atRule(CssContext context, CssAtRule rule)
    {
        var body = SerializeAtRule(rule);
        var canonical = VersionPrefix + "at-rule\0" + body;
        var id = "ecs-a-" + Hash(canonical);
        Register(context, canonical, id, body);
    }

    public static string extract()
        => extract(DefaultContext);

    [ECMAScriptName("extractFrom")]
    public static string extract(CssContext context)
    {
        EnsureDomStyle(context);
        return context.EntryBodies.Join("");
    }

    public static CssSnapshot snapshot()
        => snapshot(DefaultContext);

    [ECMAScriptName("snapshotFrom")]
    public static CssSnapshot snapshot(CssContext context)
    {
        EnsureDomStyle(context);
        return new CssSnapshot(
            context.StyleId,
            context.Nonce,
            context.EntryBodies.Join(""),
            BuildHydrationText(context));
    }

    public static CssContext context(CssOptions? options = null)
        => createContextCore(options ?? new CssOptions());

    private static CssContext createContextCore(CssOptions options)
    {
        var styleId = NormalizeStyleId(options.StyleId);
        if (options.Detached && options.Target is not null)
            Fail("A detached CSS context cannot have a DOM target.");

        return new CssContext(true)
        {
            NamesByCanonical = new Map<string, string>(),
            CanonicalByName = new Map<string, string>(),
            BodyById = new Map<string, string>(),
            EntryIds = new Array<string>(),
            EntryBodies = new Array<string>(),
            StyleId = styleId,
            Nonce = options.Nonce,
            Target = options.Target,
            Detached = options.Detached
        };
    }

    public static void configure(CssOptions options)
    {
        if (DefaultContext.HasRegistered || DefaultContext.DomHydrated)
            Fail("Configure must be called before the first style registration.");

        if (options.Detached && options.Target is not null)
            Fail("A detached CSS context cannot have a DOM target.");

        DefaultContext.StyleId = NormalizeStyleId(options.StyleId);
        DefaultContext.Nonce = options.Nonce;
        DefaultContext.Target = options.Target;
        DefaultContext.Detached = options.Detached;
    }

    private static string NormalizeStyleId(string? value)
    {
        var styleId = value is null ? DefaultStyleId : value.Trim();
        if (styleId.Length == 0)
            Fail("StyleId cannot be empty.");

        return styleId;
    }

    private static void Register(CssContext context, string canonical, string id, string body)
    {
        if (context.NamesByCanonical.Has(canonical))
        {
            EnsureDomStyle(context);
            return;
        }

        if (context.CanonicalByName.Has(id) && context.CanonicalByName.Get(id) != canonical)
            Fail("An ECMAScript.Style hash collision was detected for '" + id + "'.");

        EnsureDomStyle(context);

        if (context.BodyById.Has(id))
        {
            if (context.BodyById.Get(id) != body)
                Fail("An ECMAScript.Style hash collision was detected for '" + id + "'.");
        }
        else
        {
            context.BodyById.Set(id, body);
            context.EntryIds.Push(id);
            context.EntryBodies.Push(body);
            if (context.DomStyle is null)
                EnsureDomStyle(context);
            else
                AppendDomEntry(context, id, body);
        }

        context.NamesByCanonical.Set(canonical, id);
        context.CanonicalByName.Set(id, canonical);
        context.HasRegistered = true;
    }

    private static string SerializeRule(CssRule rule, string selector)
    {
        var output = new Array<string>();
        var declarations = SerializeDeclarations(rule);
        if (declarations.Length > 0)
            output.Push(selector + "{" + declarations + "}");

        var children = rule.Children;
        if (children is null)
            return output.Join("");

        foreach (var child in children)
        {
            var prelude = child.Prelude?.Trim() ?? "";

            if (child.Kind == CssChildKind.Selector)
            {
                if (prelude.Length == 0)
                    Fail("Selector child prelude cannot be empty.");

                if (prelude.StartsWith("@"))
                    Fail("Selector child cannot be an at-rule.");

                output.Push(SerializeRule(child.Rule, CombineSelectors(selector, prelude)));
                continue;
            }

            var atRuleName = GetGroupAtRuleName(child.Kind, prelude);
            var nested = SerializeRule(child.Rule, selector);
            if (nested.Length == 0)
                continue;

            output.Push("@" + atRuleName + (prelude.Length == 0 ? "" : " " + prelude) + "{" + nested + "}");
        }

        return output.Join("");
    }

    private static string GetGroupAtRuleName(CssChildKind kind, string prelude)
    {
        if (kind == CssChildKind.StartingStyle)
        {
            if (prelude.Length != 0)
                Fail("@starting-style does not accept a prelude.");

            return "starting-style";
        }

        var allowsEmpty = kind == CssChildKind.Layer || kind == CssChildKind.Scope;
        ValidateAtRulePrelude(prelude, "CSS child prelude", allowsEmpty);

        if (kind == CssChildKind.Media)
            return "media";
        if (kind == CssChildKind.Supports)
            return "supports";
        if (kind == CssChildKind.Container)
            return "container";
        if (kind == CssChildKind.Layer)
            return "layer";
        if (kind == CssChildKind.Scope)
            return "scope";

        Fail("Unsupported CSS child kind.");
        return "";
    }

    private static string SerializeDeclarations(CssDeclarations declarations)
    {
        var output = new Array<string>();
        var keys = Object.Keys(declarations);
        keys.Sort();

        for (var index = 0; index < keys.Length; index++)
        {
            var key = keys[index];
            if (key == AdditionalKey || key == ChildrenKey)
                continue;

            ValidateDeclarationName(key);
            // The indexer preserves CssValue's erased union contract. Reflect.Get returns object and would
            // otherwise narrow numeric declarations to string during lowering.
            // 通过索引器保留 CssValue 的擦除 union 合同；Reflect.Get 返回 object 会在 lowering 时把数值错误窄化为 string。
            var value = declarations[key];
            if (value is null)
                continue;

            output.Push(key + ":" + StringFn(value) + ";");
        }

        var additional = declarations.Additional;
        if (additional is null)
            return output.Join("");

        foreach (var declaration in additional)
        {
            var name = declaration.Name.Trim();
            ValidateDeclarationName(name);
            output.Push(name + ":" + declaration.Value +
                (declaration.Priority == CssDeclarationPriority.Important ? "!important;" : ";"));
        }

        return output.Join("");
    }

    private static string SerializeFrames(CssFrame[] frames)
    {
        var output = new Array<string>();
        foreach (var frame in frames)
        {
            var selector = NormalizeFrameSelector(frame.Selector);
            output.Push(selector + "{" + SerializeDeclarations(frame.Declarations) + "}");
        }

        return output.Join("");
    }

    private static string SerializeAtRule(CssAtRule rule)
    {
        var name = NormalizeAtRuleName(rule.Name);
        var prelude = rule.Prelude?.Trim() ?? "";
        ValidateAtRulePrelude(prelude, "At-rule prelude", true);

        var output = new Array<string>();
        output.Push("@" + name + (prelude.Length == 0 ? "" : " " + prelude) + "{");
        output.Push(SerializeDeclarations(rule.Declarations));

        var children = rule.Children;
        if (children is not null)
        {
            foreach (var child in children)
                output.Push(SerializeAtRule(child));
        }

        output.Push("}");
        return output.Join("");
    }

    private static string NormalizeAtRuleName(string value)
    {
        var name = value.Trim();
        if (name.Length == 0)
            Fail("At-rule name cannot be empty.");
        if (name.StartsWith("@"))
            Fail("At-rule name must not include '@'.");

        var hasLetter = false;
        for (var index = 0; index < name.Length; index++)
        {
            var codeUnit = (int)name.CharCodeAt(index);
            var isLetter = codeUnit >= 65 && codeUnit <= 90 || codeUnit >= 97 && codeUnit <= 122;
            var isDigit = codeUnit >= 48 && codeUnit <= 57;
            if (isLetter)
            {
                hasLetter = true;
                continue;
            }

            if (codeUnit == 45)
                continue;

            if (!isDigit || !hasLetter)
                Fail("Invalid at-rule name '" + name + "'.");
        }

        if (!hasLetter)
            Fail("Invalid at-rule name '" + name + "'.");

        return name.ToLowerInvariant();
    }

    private static string Hash(string value)
    {
        var stateA = unchecked((int)0x811c9dc5u);
        var stateB = 0x00001505;

        for (var index = 0; index < value.Length; index++)
        {
            var codeUnit = (int)value.CharCodeAt(index);
            stateA = Math.ImulFn(stateA ^ codeUnit, 0x01000193);
            stateB = Math.ImulFn(stateB, 33) ^ codeUnit;
        }

        return NumberFn(stateA >>> 0).ToString(36) + "-" + NumberFn(stateB >>> 0).ToString(36);
    }

    private static void ValidateDeclarationName(string name)
    {
        if (name.Length == 0)
            Fail("CSS declaration name cannot be empty.");

        for (var index = 0; index < name.Length; index++)
        {
            var character = name.Substring(index, 1);
            var codeUnit = (int)name.CharCodeAt(index);
            if (character == ":" || character == ";" || character == "{" || character == "}" ||
                codeUnit <= 32 || codeUnit == 127)
            {
                Fail("Invalid CSS declaration name '" + name + "'.");
            }
        }
    }

    private static void Fail(string message)
        => throw new Error(message);
}
