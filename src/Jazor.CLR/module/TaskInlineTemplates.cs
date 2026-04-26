namespace Jazor.CLR;

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
