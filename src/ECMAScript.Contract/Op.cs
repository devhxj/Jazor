namespace ECMAScript.Contract;

/// <summary>
/// Jazor 白名单 producer 侧声明的成员处理方式。
///
/// 这里描述的是“声明端应该把成员标成什么”，不是 consumer 侧的最终分发顺序。
/// 当前总体选择原则是：
/// 1. JS 原生已支持且默认输出就正确：<see cref="Allowed"/>
/// 2. JS 原生已支持但名字不同：<see cref="Alias"/>
/// 3. 能稳定落成单个表达式模板：<see cref="Inline"/>
/// 4. 需要完整运行时实现、循环、校验、异常协议或模块 helper：<see cref="Import"/>
/// 5. 只有既不适合 Inline，也不适合 Import，并且必须由编译器内部直接接管时：<see cref="Compile"/>
///
/// 注意：
/// - consumer 侧在 <c>SemanticWalker</c> 中会优先尝试 <see cref="Compile"/>，再回落到 Alias / Inline / Import。
/// - 这不表示 producer 侧应优先选择 Compile；Compile 仍然是保留给编译器内部特例的最窄选项。
/// </summary>
internal enum Op
{
	/// <summary>
	/// 明确不支持该成员。
	/// 这类成员会进入白名单事实表，但不会生成可消费的正常映射。
	/// 适用于 JS 无对等概念、或当前编译器明确不打算承接的成员。
	/// </summary>
	Discard,

	/// <summary>
	/// 允许直接使用默认 lowering。
	/// 适用于 JS 原生语义、名称和结构都已足够接近的成员。
	/// 这类成员不需要模板、不需要导入，也不需要编译器特判。
	/// </summary>
	Allowed,

	/// <summary>
	/// 允许使用，但输出名称需要替换。
	/// 常见于 C# 成员名和 JS 原生方法/属性名不同的情况，例如 Count -> size。
	/// 这类成员本质上仍走普通宿主访问，只是名字改写。
	/// </summary>
	Alias,

	/// <summary>
	/// 需要模块级运行时实现。
	/// 这类成员必须提供真实方法体，并会被编译到 CLR module。
	/// 适用于：
	/// - 需要循环、多步逻辑或 helper 复用
	/// - 需要完整异常/边界检查协议
	/// - 需要解析、格式化、out/ref 返回包等运行时语义
	/// 不要把 Import 当成“Inline 写起来麻烦”的兜底选项。
	/// </summary>
	Import,

	/// <summary>
	/// 使用表达式模板直接内联。
	/// 这类成员不会生成模块实现，而是由编译器把模板实例化为 AST。
	/// 当前模板占位符使用 __argN 形式：
	/// - 实例成员：__arg1 是实例，后续才是真实参数
	/// - 静态成员：__arg1 开始就是第一个真实参数
	///
	/// 只适用于“稳定单表达式”场景。
	/// 如果需要临时变量、throw 分支、import 收集或 tuple 运行时形状，不应继续塞到 Inline。
	/// </summary>
	Inline,

	/// <summary>
	/// 编译器内部特殊钩子。
	/// 这类成员不依赖模板，也不依赖运行时模块，而是由 SemanticWalker 中的 Compile_* 直接产 AST。
	///
	/// 当前 contract 仍应保持克制：
	/// - 会拿到原始 symbol、当前 SenseArgument、实例 handler、显式参数以及 origin operation
	/// - 因而可以在必要时绑定 import、保留 usage-site 诊断锚点，或构造更精细的宿主 AST
	///
	/// 但 Compile 依然是“编译器拥有语义”的最窄入口。
	/// 不要把本可用 Alias / Inline / Import 建模的成员机械升级成 Compile。
	/// </summary>
	Compile,
}
