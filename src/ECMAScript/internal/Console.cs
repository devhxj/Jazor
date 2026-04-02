namespace ECMAScript;

public static partial class Global
{
	extension(Console)
	{
		/// <summary>
		/// 如果断言为 false，则将一个错误消息写入控制台。如果断言是 true，没有任何反应。
		/// </summary>
		/// <param name="assertion">一个布尔表达式。如果 assertion 为假，消息将会被输出到控制台之中。</param>
		/// <param name="obj">被用来输出的 Javascript 对象列表，最后输出的字符串是各个对象依次拼接的结果。</param>
		[Description("@#assert")]
		public extern static void Assert(bool assertion, params object[] obj);

		/// <summary>
		/// 如果断言为 false，则将一个错误消息写入控制台。如果断言是 true，没有任何反应。
		/// </summary>
		/// <param name="assertion">一个布尔表达式。如果 assertion 为假，消息将会被输出到控制台之中。</param>
		/// <param name="msg">一个包含零个或多个子串的 Javascript 字符串。</param>
		/// <param name="subst">各个消息作为字串的 Javascript 对象。这个参数可以让你能够控制输出的格式。</param>
		[Description("@#assert")]
		public extern static void Assert(bool assertion, string msg, params string[] subst);

		/// <summary>
		/// 清空控制台，但前提是该控制台允许清空。像浏览器运行的图形控制台就允许清空，而像 Node 运行的终端上显示的控制台则不支持它，调用该方法将不会产生任何效果（也不会报错）。
		/// </summary>
		[Description("@#clear")]
		public extern static void Clear();

		/// <summary>
		/// 记录调用 count() 的次数。
		/// </summary>
		/// <param name="label">一个字符串。如果给定，count() 会输出带有该标签的调用次数。如果未提供，调用 count() 的行为就像是带有“default”标签一样。</param>
		[Description("@#count")]
		public extern static void Count(string? label = null);

		/// <summary>
		/// 重置计数器。
		/// </summary>
		/// <param name="label">一个字符串，若传入此参数 countReset() 重置此 label 的 count 为 0。 若忽略此参数 countReset() 重置 count() 默认的 default 字段的 count 为 0</param>
		/// <returns></returns>
		[Description("@#countReset")]
		public extern static void CountReset(string? label = null);

		/// <summary>
		/// 将一条消息输出到 web 控制台，消息的日志级别为“debug”。只有在控制台配置为显示调试输出时，才会向用户显示该消息。在大多数情况下，日志级别在控制台 UI 中进行配置。该日志级别可能对应于 Debug 或 Verbose 日志级别。
		/// </summary>
		/// <param name="obj">要输出的 JavaScript 对象列表。按传参的顺序把对象输出到控制台。</param>
		[Description("@#debug")]
		public extern static void Debug(params object?[] obj);

		/// <summary>
		/// 将一条消息输出到 web 控制台，消息的日志级别为“debug”。只有在控制台配置为显示调试输出时，才会向用户显示该消息。在大多数情况下，日志级别在控制台 UI 中进行配置。该日志级别可能对应于 Debug 或 Verbose 日志级别。
		/// </summary>
		/// <param name="msg">包含零个或多个替换字符串的 JavaScript 字符串，这些替换字符串会按照连续的顺序用 subst1 到 substN 进行替换。</param>
		/// <param name="subst">包含零个或多个替换字符串的 JavaScript 字符串，这些替换字符串会按照连续的顺序用 subst1 到 substN 进行替换。</param>
		[Description("@#debug")]
		public extern static void Debug(string msg, params string?[] subst);

		/// <summary>
		/// 可以显示指定 JavaScript 对象的属性列表，并以交互式的形式展现。输出结果呈现为分层列表，包含展开/折叠的三角形图标，可用于查看子对象的内容。
		/// 换句话说，dir() 是一种在控制台中查看指定 JavaScript 对象的所有属性的方法，开发人员可以通过这种方式轻松获取对象的属性。
		/// </summary>
		/// <param name="obj">应输出其属性的 JavaScript 对象。</param>
		[Description("@#dir")]
		public extern static void Dir(object? obj);

		/// <summary>
		/// 向 Web 控制台输出一条错误消息。
		/// </summary>
		/// <param name="obj">要输出的 JavaScript 对象列表。这些对象的字符串形式按顺序加起来然后输出。</param>
		[Description("@#error")]
		public extern static void Error(params object?[] obj);

		/// <summary>
		/// 向 Web 控制台输出一条错误消息。
		/// </summary>
		/// <param name="msg">一个字符串，它包含零个或多个替代字符串。</param>
		/// <param name="subst">要输出的 JavaScript 对象列表。这些对象的字符串形式按顺序加起来然后输出。</param>
		[Description("@#error")]
		public extern static void Error(string msg, params string?[] subst);

		/// <summary>
		/// 在 Web 控制台上创建一个新的分组。随后输出到控制台上的内容都会被添加一个缩进，表示该内容属于当前分组，直到调用 console.groupEnd() 之后，当前分组结束。
		/// </summary>
		/// <param name="label">分组标签。</param>
		[Description("@#group")]
		public extern static void Group(string? label = null);

		/// <summary>
		/// 在 Web 控制台上创建一个新的分组。与 console.group() 方法的不同点是，新建的分组默认是折叠的。用户必须点击一个按钮才能将折叠的内容打开。
		/// </summary>
		/// <param name="label">分组标签。</param>
		[Description("@#groupCollapsed")]
		public extern static void GroupCollapsed(string? label = null);

		/// <summary>
		/// 在 Web 控制台中退出一格缩进 (结束分组). 请参阅 console 中的Using groups in the console 来获取它的用法和示例。
		/// </summary>
		[Description("@#groupEnd")]
		public extern static void GroupEnd();

		/// <summary>
		/// 向 web 控制台输出一个通知信息。仅在 Firefox，web 控制台的日志中的项目旁边会显示一个小的‘I‘图标
		/// </summary>
		/// <param name="obj">要输出的 JavaScript 对象列表。对象 obj1,obj2,...列出顺序和输出顺序一致。</param>
		[Description("@#info")]
		public extern static void Info(params object?[] obj);

		/// <summary>
		/// 向 web 控制台输出一个通知信息。仅在 Firefox，web 控制台的日志中的项目旁边会显示一个小的‘I‘图标
		/// </summary>
		/// <param name="msg">JavaScript 字符串。可包含零个或多个替换字符串。</param>
		/// <param name="subst">用于替换 msg 内的替换字符串的 JavaScript 对象。可以对输出的格式进行额外的控制。</param>
		[Description("@#info")]
		public extern static void Info(string msg, params string?[] subst);

		/// <summary>
		/// 向 Web 控制台输出一条信息。这条信息可能是单个字符串（包括可选的替代字符串），也可能是一个或多个对象。
		/// </summary>
		/// <param name="obj">一个用于输出的 JavaScript 对象列表。其中每个对象会以字符串的形式按照顺序依次输出到控制台。请注意，如果你在最新版本的 Chrome 和 Firefox 中输出对象，你在控制台中得到的是对该对象的引用，这不一定是你调用 console.log() 时该对象的“值”，但它一定是该对象在你打开控制台时的值。</param>
		[Description("@#log")]
		public extern static void Log(params object?[] obj);

		/// <summary>
		/// 向 Web 控制台输出一条信息。这条信息可能是单个字符串（包括可选的替代字符串），也可能是一个或多个对象。
		/// </summary>
		/// <param name="msg">一个 JavaScript 字符串，其中包含零个或多个替代字符串。</param>
		/// <param name="subst">JavaScript 对象，用来依次替换 msg 中的替代字符串。你可以在替代字符串中指定对象的输出格式。</param>
		[Description("@#log")]
		public extern static void Log(string msg, params string?[] subst);

		/// <summary>
		/// 将数据以表格的形式显示。
		/// 这个方法需要一个必须参数 data，data 必须是一个数组或者是一个对象；还可以使用一个可选参数 columns。
		/// 它会把数据 data 以表格的形式打印出来。数组中的每一个元素（或对象中可枚举的属性）将会以行的形式显示在表格中。
		/// 表格的第一列是 index。如果数据 data 是一个数组，那么这一列的单元格的值就是数组的索引。如果数据是一个对象，那么它们的值就是各对象的属性名称。注意（在 FireFox 中）console.table 被限制为只显示 1000 行（第一行是被标记的索引（原文：labeled index））。
		/// </summary>
		/// <param name="data">要显示的数据。必须是数组或对象。</param>
		/// <param name="columns">一个包含列的名称的数组。</param>
		[Description("@#table")]
		public extern static void Table(object? data, string[]? columns = null);

		/// <summary>
		/// 你可以启动一个计时器来跟踪某一个操作的占用时长。每一个计时器必须拥有唯一的名字，页面中最多能同时运行 10,000 个计时器。当以此计时器名字为参数调用 console.timeEnd() 时，浏览器将以毫秒为单位，输出对应计时器所经过的时间。
		/// </summary>
		/// <param name="label">新计时器的名字。用来标记这个计时器，作为参数调用 console.timeEnd() 可以停止计时并将经过的时间在终端中打印出来。</param>
		[Description("@#time")]
		public extern static void Time(string? label = null);

		/// <summary>
		/// 停止指定的计时器，并将该计时器经过的时间输出到控制台。
		/// </summary>
		/// <param name="label">计时器名称。未提供时使用默认计时器。</param>
		[Description("@#timeEnd")]
		public extern static void TimeEnd(string? label = null);

		/// <summary>
		/// 在控制台输出计时器的值，该计时器必须已经通过 console.time() 启动。
		/// </summary>
		[Description("@#timeLog")]
		public extern static void TimeLog();

		/// <summary>
		/// 在控制台输出计时器的值，该计时器必须已经通过 console.time() 启动。
		/// </summary>
		/// <param name="label">计时器索引。</param>
		/// <param name="val"></param>
		[Description("@#timeLog")]
		public extern static void TimeLog(string label, params object?[] val);

		/// <summary>
		/// 将堆栈追踪信息输出到控制台。
		/// </summary>
		/// <param name="obj">零个或多个要与追踪信息一起输出到控制台的对象。这些对象的组装与格式化方式与传递给 console.log() 方法时相同。</param>
		[Description("@#trace")]
		public extern static void Trace(params object?[] obj);

		/// <summary>
		/// 向 Web 控制台输出一条警告信息。
		/// </summary>
		/// <param name="obj">要输出的 Javascript 对象列表。其中每个对象会以字符串的形式按照顺序依次输出到控制台。</param>
		[Description("@#warn")]
		public extern static void Warn(params object?[] obj);

		/// <summary>
		/// 向 Web 控制台输出一条警告信息。
		/// </summary>
		/// <param name="msg">一个 JavaScript 字符串，其中包含零个或多个替代字符串。</param>
		/// <param name="subst">零个或多个 Javascript 对象 依次替换 msg 中的替代字符串，你可以在替代字符串中指定对象的输出格式。</param>
		[Description("@#warn")]
		public extern static void Warn(string msg, params string?[] subst);
	}
}
