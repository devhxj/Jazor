namespace ECMAScript.Style;

/// <summary>
/// Provides the framework-neutral CSS-in-JS authoring facade for ECMAScript modules.
/// The lowercase name intentionally mirrors CSS authoring when imported with <c>using static ECMAScript.Style.css</c>.
/// 提供面向 ECMAScript 模块、框架无关的 CSS-in-JS 作者入口。小写名称刻意在
/// <c>using static ECMAScript.Style.css</c> 后贴近 CSS 写作体验。
/// </summary>
/// <remarks>
/// The facade serializes typed values deterministically, deduplicates equal rules within a context, and owns at most one
/// managed style element per non-detached context. Call context-aware overloads for SSR, Shadow DOM, or isolated tests.
/// 该入口确定性地序列化强类型值，在同一 context 内去重相等规则，并且每个非分离 context 最多拥有一个
/// 受管理 style 元素。SSR、Shadow DOM 和隔离测试应调用带 context 的重载。
/// </remarks>
[ECMAScriptModule("style.mjs")]
public static partial class css
{
    private const string VersionPrefix = "ecmascript-style:v1\0";
    private const string RootSelectorToken = ".__ecmascript_style_root__";
    private const string DefaultStyleId = "ecmascript-style";
    private const string AdditionalKey = "$additional";
    private const string ChildrenKey = "$children";

    private static readonly CssContext DefaultContext = createContextCore(new CssOptions());

    /// <summary>
    /// Registers a component-scoped rule in the default context and returns its deterministic class name.
    /// Re-registering an equivalent rule returns the same class name and does not duplicate CSS output.
    /// 在默认 context 中注册组件作用域规则并返回确定性 class 名。重复注册等价规则会返回相同 class 名，
    /// 且不会重复输出 CSS。
    /// </summary>
    public static string style(CssRule rule)
        => style(DefaultContext, rule);

    /// <summary>
    /// Registers a component-scoped rule in <paramref name="context"/> and returns its deterministic class name.
    /// The rule is serialized relative to the generated class selector, so nested selector children remain scoped.
    /// 在 <paramref name="context"/> 中注册组件作用域规则并返回确定性 class 名。规则会相对于生成的 class
    /// 选择器序列化，因此嵌套选择器子项仍保持作用域隔离。
    /// </summary>
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

    /// <summary>
    /// Registers keyframes in the default context and returns their deterministic animation name.
    /// At least one frame is required; equivalent frame sequences share one emitted <c>@keyframes</c> block.
    /// 在默认 context 中注册关键帧并返回确定性动画名。至少需要一个帧；等价帧序列共享同一个输出的
    /// <c>@keyframes</c> 块。
    /// </summary>
    public static string keyframes([Preserve] params CssFrame[] frames)
        => keyframes(DefaultContext, frames);

    /// <summary>
    /// Registers keyframes in <paramref name="context"/> and returns their deterministic animation name.
    /// Use this overload when the result must be extracted or hydrated independently from the default registry.
    /// 在 <paramref name="context"/> 中注册关键帧并返回确定性动画名。当结果必须独立于默认 registry
    /// 提取或 hydration 时使用此重载。
    /// </summary>
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

    /// <summary>
    /// Registers a global CSS rule in the default context.
    /// The selector must be a selector list, not an at-rule; use <see cref="at_rule(CssAtRule)"/> for top-level at-rules.
    /// 在默认 context 中注册全局 CSS 规则。selector 必须是选择器列表而非 at-rule；顶层 at-rule 请使用
    /// <see cref="at_rule(CssAtRule)"/>。
    /// </summary>
    public static void global(string selector, CssRule rule)
        => global(DefaultContext, selector, rule);

    /// <summary>
    /// Registers a global CSS rule in <paramref name="context"/>.
    /// Equal serialized rules are deduplicated, while selector normalization preserves the authored selector structure.
    /// 在 <paramref name="context"/> 中注册全局 CSS 规则。相同的序列化规则会去重，
    /// selector 规范化则保留作者提供的选择器结构。
    /// </summary>
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

    /// <summary>
    /// Registers a top-level CSS at-rule in the default context.
    /// Use this for constructs whose body is not naturally a nested selector child, such as <c>@font-face</c> or <c>@property</c>.
    /// 在默认 context 中注册顶层 CSS at-rule。适用于不能自然表示为嵌套选择器子项的构造，
    /// 例如 <c>@font-face</c> 或 <c>@property</c>。
    /// </summary>
    [ECMAScriptName("atRule")]
    public static void at_rule(CssAtRule rule)
        => at_rule(DefaultContext, rule);

    /// <summary>
    /// Registers a top-level CSS at-rule in <paramref name="context"/>.
    /// Names are normalized and validated without an <c>@</c> prefix; nested <see cref="CssAtRule.Children"/> retain author order.
    /// 在 <paramref name="context"/> 中注册顶层 CSS at-rule。名称会被规范化并验证，且不得带 <c>@</c> 前缀；
    /// 嵌套 <see cref="CssAtRule.Children"/> 保留作者顺序。
    /// </summary>
    [ECMAScriptName("atRuleIn")]
    public static void at_rule(CssContext context, CssAtRule rule)
    {
        var body = SerializeAtRule(rule);
        var canonical = VersionPrefix + "at-rule\0" + body;
        var id = "ecs-a-" + Hash(canonical);
        Register(context, canonical, id, body);
    }

    /// <summary>
    /// Returns the concatenated CSS currently registered in the default context.
    /// For DOM-backed contexts this first adopts or synchronizes the managed style element.
    /// 返回默认 context 当前已注册的拼接 CSS。对于 DOM 支持的 context，调用前会先接管或同步受管理的 style 元素。
    /// </summary>
    public static string extract()
        => extract(DefaultContext);

    /// <summary>
    /// Returns the concatenated CSS currently registered in <paramref name="context"/>.
    /// This is non-destructive: subsequent registrations append to the same context and retain their deterministic ids.
    /// 返回 <paramref name="context"/> 当前已注册的拼接 CSS。该操作不会清空 context；之后注册的规则会继续追加，
    /// 并保留其确定性 id。
    /// </summary>
    [ECMAScriptName("extractFrom")]
    public static string extract(CssContext context)
    {
        EnsureDomStyle(context);
        return context.EntryBodies.Join("");
    }

    /// <summary>
    /// Captures the default context as a CSS and hydration snapshot for server rendering or client transfer.
    /// The snapshot includes the style id and nonce required to adopt the exact same managed style element.
    /// 将默认 context 捕获为 CSS 与 hydration 快照，供服务端渲染或客户端传输使用。快照包含接管完全相同的
    /// 受管理 style 元素所需的 style id 和 nonce。
    /// </summary>
    public static CssSnapshot snapshot()
        => snapshot(DefaultContext);

    /// <summary>
    /// Captures <paramref name="context"/> as a CSS and hydration snapshot.
    /// Prefer a detached context for pure SSR collection when no browser DOM should be observed.
    /// 将 <paramref name="context"/> 捕获为 CSS 与 hydration 快照。当不应访问浏览器 DOM 的纯 SSR 收集场景时，
    /// 应优先使用分离 context。
    /// </summary>
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

    /// <summary>
    /// Creates an isolated CSS context with its own rule registry, naming collision checks, and DOM ownership state.
    /// A null <paramref name="options"/> uses the standard style id and main document target.
    /// 创建隔离 CSS context，拥有独立的规则 registry、命名碰撞检查和 DOM 所有权状态。null 的
    /// <paramref name="options"/> 使用标准 style id 与主文档目标。
    /// </summary>
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

    /// <summary>
    /// Configures the default CSS context before any registration or hydration has occurred.
    /// This method cannot retarget an active context because doing so could split a deterministic registry across style elements.
    /// 在任何注册或 hydration 发生前配置默认 CSS context。该方法不能重新定向已活动的 context，
    /// 因为那会将确定性 registry 拆分到多个 style 元素。
    /// </summary>
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

        var children = rule.children;
        if (children is null)
            return output.Join("");

        foreach (var child in children)
        {
            var prelude = child.Prelude?.Trim() ?? "";

            if (child.Kind == ChildKind.Selector)
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

    private static string GetGroupAtRuleName(ChildKind kind, string prelude)
    {
        if (kind == ChildKind.StartingStyle)
        {
            if (prelude.Length != 0)
                Fail("@starting-style does not accept a prelude.");

            return "starting-style";
        }

        var allowsEmpty = kind == ChildKind.Layer || kind == ChildKind.Scope;
        ValidateAtRulePrelude(prelude, "CSS child prelude", allowsEmpty);

        if (kind == ChildKind.Media)
            return "media";
        if (kind == ChildKind.Supports)
            return "supports";
        if (kind == ChildKind.Container)
            return "container";
        if (kind == ChildKind.Layer)
            return "layer";
        if (kind == ChildKind.Scope)
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

        var additional = declarations.additional;
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
