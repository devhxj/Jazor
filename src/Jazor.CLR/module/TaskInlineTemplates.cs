namespace Jazor.CLR;

/// <summary>
/// 集中保存 Task 模块中需要复用的短 inline 模板。
/// </summary>
/// <remarks>
/// 模板中的参数占位符由 SemanticWalker 实例化；这里不能写入依赖局部 C# 名称的代码。
/// 复杂控制流或需要独立 helper 的 Task 行为应放到 Import runtime method 中。
/// </remarks>
internal static class TaskInlineTemplates
{
	public const string WaitAsyncTimeSpan =
		"Promise.race([Promise.resolve(__arg1), (__arg2.ticks === -10000n ? new Promise(() => {}) : new Promise((_, reject) => setTimeout(() => reject(new Error(\"TimeoutException\")), Number(__arg2.ticks / 10000n))))])";

	public const string WaitTimeSpan =
		"Promise.race([Promise.resolve(__arg1).then(() => true), (__arg2.ticks === -10000n ? new Promise(() => {}) : new Promise((resolve) => setTimeout(() => resolve(false), Number(__arg2.ticks / 10000n))))])";

	public const string WaitAllTimeSpan =
		"Promise.race([Promise.all(__arg1).then(() => true), (__arg2.ticks === -10000n ? new Promise(() => {}) : new Promise((resolve) => setTimeout(() => resolve(false), Number(__arg2.ticks / 10000n))))])";

	public const string WaitAnyTimeSpan =
		"Promise.race([Promise.race(Array.from(__arg1).map((task, index) => Promise.resolve(task).then(() => index, () => index))), (__arg2.ticks === -10000n ? new Promise(() => {}) : new Promise((resolve) => setTimeout(() => resolve(-1), Number(__arg2.ticks / 10000n))))])";

	public const string DelayTimeSpan =
		"(__arg1.ticks === -10000n ? new Promise(() => {}) : new Promise((resolve) => setTimeout(resolve, Number(__arg1.ticks / 10000n))))";
}
