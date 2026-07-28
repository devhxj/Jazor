namespace Jazor.Css;

[ECMAScriptModule("Jazor.Css/runtime.mjs")]
public static partial class Css
{
    private const string VersionPrefix = "jazor-css:v1\0";
    private const string RootSelectorToken = ".__jazor_css_root__";
    private const string DefaultStyleId = "jazor-css";
    private const string AdditionalKey = "$additional";
    private const string ChildrenKey = "$children";

    private static readonly Map<string, string> NamesByCanonical = new();
    private static readonly Map<string, string> CanonicalByName = new();
    private static readonly Map<string, string> BodyById = new();
    private static Array<string> EntryIds = new();
    private static Array<string> EntryBodies = new();

    private static string StyleId = DefaultStyleId;
    private static string? Nonce;
    private static bool HasRegistered;

    [Description("@#css")]
    public static string Class(CssRule rule)
    {
        var canonicalBody = SerializeRule(rule, RootSelectorToken);
        var canonical = VersionPrefix + "class\0" + canonicalBody;
        if (NamesByCanonical.Has(canonical))
        {
            EnsureDomStyle();
            return NamesByCanonical.Get(canonical)!;
        }

        var name = "jz-" + Hash(canonical);
        var body = SerializeRule(rule, "." + name);
        Register(canonical, name, body);
        return name;
    }

    public static string Keyframes(params CssFrame[] frames)
    {
        if (frames.Length == 0)
            Fail("Keyframes requires at least one frame.");

        var canonicalFrames = SerializeFrames(frames);
        var canonical = VersionPrefix + "keyframes\0" + canonicalFrames;
        if (NamesByCanonical.Has(canonical))
        {
            EnsureDomStyle();
            return NamesByCanonical.Get(canonical)!;
        }

        var name = "jz-k-" + Hash(canonical);
        Register(canonical, name, "@keyframes " + name + "{" + canonicalFrames + "}");
        return name;
    }

    public static void Global(string selector, CssRule rule)
    {
        var normalizedSelector = NormalizeSelectorList(selector, "Global selector");
        if (normalizedSelector.StartsWith("@"))
            Fail("Global selector cannot be an at-rule.");

        var body = SerializeRule(rule, normalizedSelector);
        var canonical = VersionPrefix + "global\0" + body;
        var id = "jz-g-" + Hash(canonical);
        Register(canonical, id, body);
    }

    public static string Extract()
    {
        EnsureDomStyle();
        return EntryBodies.Join("");
    }

    public static void Configure(CssOptions options)
    {
        if (HasRegistered || DomHydrated)
            Fail("Configure must be called before the first style registration.");

        var styleId = options.StyleId is null ? DefaultStyleId : options.StyleId.Trim();
        if (styleId.Length == 0)
            Fail("StyleId cannot be empty.");

        StyleId = styleId;
        Nonce = options.Nonce;
    }

    private static void Register(string canonical, string id, string body)
    {
        if (NamesByCanonical.Has(canonical))
        {
            EnsureDomStyle();
            return;
        }

        if (CanonicalByName.Has(id) && CanonicalByName.Get(id) != canonical)
            Fail("A Jazor.Css hash collision was detected for '" + id + "'.");

        EnsureDomStyle();

        if (BodyById.Has(id))
        {
            if (BodyById.Get(id) != body)
                Fail("A Jazor.Css hash collision was detected for '" + id + "'.");
        }
        else
        {
            BodyById.Set(id, body);
            EntryIds.Push(id);
            EntryBodies.Push(body);
            if (DomStyle is null)
                EnsureDomStyle();
            else
                AppendDomEntry(id, body);
        }

        NamesByCanonical.Set(canonical, id);
        CanonicalByName.Set(id, canonical);
        HasRegistered = true;
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
            var prelude = child.Prelude.Trim();
            if (prelude.Length == 0)
                Fail("CSS child prelude cannot be empty.");

            if (child.Kind == CssChildKind.Selector)
            {
                if (prelude.StartsWith("@"))
                    Fail("Selector child cannot be an at-rule.");

                output.Push(SerializeRule(child.Rule, CombineSelectors(selector, prelude)));
                continue;
            }

            ValidateConditionPrelude(prelude);
            var nested = SerializeRule(child.Rule, selector);
            if (nested.Length == 0)
                continue;

            if (child.Kind == CssChildKind.Media)
            {
                output.Push("@media " + prelude + "{" + nested + "}");
                continue;
            }

            if (child.Kind == CssChildKind.Supports)
            {
                output.Push("@supports " + prelude + "{" + nested + "}");
                continue;
            }

            Fail("Unsupported CSS child kind.");
        }

        return output.Join("");
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
            var value = Reflect.Get(declarations, key) as string;
            if (value is null)
                continue;

            output.Push(key + ":" + value + ";");
        }

        var additional = declarations.Additional;
        if (additional is null)
            return output.Join("");

        foreach (var declaration in additional)
        {
            var name = declaration.Name.Trim();
            ValidateDeclarationName(name);
            output.Push(name + ":" + declaration.Value + (declaration.Important ? "!important;" : ";"));
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
