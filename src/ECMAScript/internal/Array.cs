namespace ECMAScript;

[ECMAScript]
/// <summary>
/// Non-generic authoring binding for JavaScript <c>Array</c>.
/// JavaScript Array 的非泛型编写绑定。
/// </summary>
/// <remarks>
/// Arrays are mutable and iterable JavaScript containers whose length is a JavaScript number.
/// This interface exposes only compiler-projectable members; it does not promise CLR array semantics such as fixed length,
/// multiple dimensions, or runtime element-type checks.
/// Array 是可变、可迭代且按 Number length 管理的 JavaScript 容器；接口只公开编译器可投影的成员，
/// 不承诺 CLR Array 的固定长度、多维或运行时元素类型检查语义。
/// </remarks>
public interface IArray : IEnumerable
{
	/// <summary>Gets the JavaScript array length. 获取 JavaScript 数组的长度。</summary>
	[Description("@#length")]
	Number Length { get; }

	/// <summary>Checks whether a runtime value is a JavaScript array. 检查运行时值是否为 JavaScript 数组。</summary>
	[Description("@#isArray")]
	static extern bool IsArray(object? obj);
}

[ECMAScript]
/// <summary>
/// JavaScript <c>Array</c> binding with a compile-time element type annotation.
/// 带编译期元素类型标注的 JavaScript <c>Array</c> 绑定。
/// </summary>
public interface IArray<T> : IArray
{
	/// <summary>
	/// Direct JavaScript index access surface.
	/// This stays non-nullable for compatibility with existing array-like mappings.
	/// Callers that need absence-aware reads should prefer APIs such as <c>At()</c> on concrete hosts.
	/// 直接映射 JavaScript 的索引访问。为兼容既有数组映射保持非空；需要表达越界 <c>undefined</c> 时应使用具体宿主的 <c>At()</c>。
	/// </summary>
	T this[Number index] { get; }
}

/// <summary>
/// JavaScript <c>Array</c> runtime host.
/// Hidden members near the end of this type exist only for CLR bridge scenarios such as collection initializers;
/// they are not intended to redefine the JavaScript runtime shape.
/// JavaScript <c>Array</c> 运行时宿主。类型末尾隐藏成员只服务于集合初始化器等 CLR 桥接场景，
/// 不改变 JavaScript 原生运行时形状。
/// </summary>
/// <typeparam name="T">Compile-time annotation for array elements. 数组元素的编译期类型标注。</typeparam>
[ECMAScript]
[Description("@#Array")]
public partial class Array<T> : object, IArray<T>
{
	/// <summary>Creates an empty JavaScript array. 创建空的 JavaScript 数组。</summary>
	public extern Array();

	/// <summary>Creates a sparse JavaScript array with the specified length. 按指定长度创建稀疏 JavaScript 数组，不填充元素。</summary>
	public extern Array(Number size);

	/// <summary>Creates a one-element JavaScript array. 创建仅包含一个元素的 JavaScript 数组。</summary>
	public extern Array(T item);

	/// <summary>Creates a JavaScript array containing two elements in order. 按顺序创建包含两个元素的 JavaScript 数组。</summary>
	public extern Array(T item1, T item2);

	/// <summary>Creates a JavaScript array from the supplied items. 根据提供的元素创建 JavaScript 数组。</summary>
	public extern Array(T item1, T item2, params T[] items);

	/// <summary>Projects this JavaScript array as a CLR array for compile-time interop. 将此 JavaScript 数组投影为 CLR 数组，仅用于编译期互操作。</summary>
	public extern static implicit operator T[](Array<T> x);

	/// <summary>Projects this JavaScript array as a CLR list for compile-time interop. 将此 JavaScript 数组投影为 CLR List，仅用于编译期互操作。</summary>
	public extern static implicit operator List<T>(Array<T> x);

	/// <summary>Projects a CLR array to its JavaScript array runtime representation. 将 CLR 数组投影为 JavaScript 数组运行时表示。</summary>
	public extern static implicit operator Array<T>(T[] array);

	/// <summary>Projects a CLR list to its JavaScript array runtime representation. 将 CLR List 投影为 JavaScript 数组运行时表示。</summary>
	public extern static implicit operator Array<T>(List<T> array);

	/// <summary>Projects a read-only CLR collection to a JavaScript array runtime representation. 将只读 CLR 集合投影为 JavaScript 数组运行时表示。</summary>
	public extern static implicit operator Array<T>(ReadOnlyCollection<T> array);

	/// <summary>Projects a read-only CLR set to a JavaScript array runtime representation. 将只读 CLR 集合投影为 JavaScript 数组运行时表示。</summary>
	public extern static implicit operator Array<T>(ReadOnlySet<T> array);

	/// <summary>Projects a non-generic CLR array through the JavaScript array boundary. 将非泛型 CLR 数组投影穿过 JavaScript 数组边界。</summary>
	public extern static implicit operator Array<T>(Array array);

	/// <summary>
	/// Direct JavaScript index access surface.
	/// This stays non-nullable to preserve compatibility with CLR collection-style mappings that project to JavaScript arrays.
	/// Use <see cref="At" /> when you need a nullable result for out-of-range access.
	/// 直接映射 JavaScript 索引读写。为兼容 CLR 集合式映射保持非空；需要越界可空结果时使用 <see cref="At"/>。
	/// </summary>
	public extern T this[Number index] { get; set; }

	/// <summary>
	/// Gets or sets the length of the array. This is a number one higher than the highest index in the array.
	/// 获取或设置数组长度；设置该值可能截断数组或创建空槽，遵循 JavaScript <c>length</c> 语义。
	/// </summary>
	[Description("@#length")]
	public extern Number Length { get; }

	/// <summary>
	/// Returns the JavaScript string form of the array.
	/// This is the direct projection of <c>Array.prototype.toString()</c>.
	/// 这是 <c>Array.prototype.toString()</c> 的直接投影，不等同于 CLR 集合格式化。
	/// </summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the array.
	/// This is the direct projection of <c>Array.prototype.toLocaleString()</c>.
	/// 直接投影 <c>Array.prototype.toLocaleString()</c>，格式由元素自身的本地化转换决定。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the array.
	/// JavaScript forwards <paramref name="locales" /> and <paramref name="options" /> to each element's own <c>toLocaleString</c> method.
	/// JavaScript 会将 <paramref name="locales"/> 与 <paramref name="options"/> 转交给每个元素自己的 <c>toLocaleString</c> 方法。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales, object? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// 这是 C# 便利重载，因为 C# 调用不能自然省略位于前面的 <c>locales</c> 参数。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(object? options);

	/// <summary>
	/// Returns a locale-sensitive string representation of the array.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// JavaScript forwards <paramref name="locales" /> and <paramref name="options" /> to each element's own <c>toLocaleString</c> method.
	/// 使用 <see cref="IEnumerable{T}"/> 表达 JavaScript locale 列表，参数会转交给每个元素的 <c>toLocaleString</c>。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string>? locales, object? options = null);

	/// <summary>
	/// Removes the last element from an array and returns it.
	/// If the array is empty, JavaScript returns <c>undefined</c>; this C# projection surfaces that absence as <see langword="null" />
	/// and does not modify the array.
	/// 删除并返回末项。空数组时 JavaScript 返回 <c>undefined</c>，本投影以 <see langword="null"/> 表示且不修改数组。
	/// </summary>
	/// <returns></returns>
	[Description("@#pop")]
	public extern T? Pop();

	/// <summary>
	/// Appends new elements to the end of an array, and returns the new length of the array.
	/// 向数组尾部追加元素并返回新长度；该操作原地修改 JavaScript 数组。
	/// </summary>
	/// <param name="items">New elements to add to the array.</param>
	/// <returns></returns>
	[Description("@#push")]
	public extern Number Push(params T[] items);

	/// <summary>
	/// Combines two or more arrays.
	/// This method returns a new array without modifying any existing arrays.
	/// 合并可迭代值或元素，返回新数组且不修改现有数组。
	/// </summary>
	/// <param name="items">Additional arrays and/or items to add to the end of the array.</param>
	/// <returns></returns>
	[Description("@#concat")]
	public extern Array<T> Concat(params IEnumerable<T>[] items);

	/// <summary>
	/// Combines two or more arrays.
	/// 合并元素为新数组；不会修改原数组。
	/// </summary>
	/// <param name="items">Additional arrays and/or items to add to the end of the array.</param>
	/// <returns>This method returns a new array without modifying any existing arrays.</returns>
	[Description("@#concat")]
	public extern Array<T> Concat(params T[] items);

	/// <summary>
	/// Adds all the elements of an array into a string, separated by the specified separator string.
	/// 使用指定分隔符连接数组元素；未提供分隔符时遵循 JavaScript 默认逗号规则。
	/// </summary>
	/// <param name="separator">A string used to separate one element of the array from the next in the resulting string. If omitted, the array elements are separated with a comma.</param>
	/// <returns></returns>
	[Description("@#join")]
	public extern string Join(string? separator = null);

	/// <summary>
	/// Reverses the elements in an array in place.
	/// 原地反转数组元素，并返回同一个数组实例。
	/// </summary>
	/// <returns>This method mutates the array and returns a reference to the same array.</returns>
	[Description("@#reverse")]
	public extern Array<T> Reverse();

	/// <summary>
	/// Removes the first element from an array and returns it.
	/// 删除并返回首项；空数组时 <c>undefined</c> 在此投影为 <see langword="null"/>。
	/// </summary>
	/// <returns>If the array is empty, JavaScript returns <c>undefined</c>; this C# projection surfaces that absence as <see langword="null" /> and does not modify the array.</returns>
	[Description("@#shift")]
	public extern T? Shift();

	/// <summary>
	/// Returns a copy of a section of an array.
	/// For both start and end, a negative index can be used to indicate an offset from the end of the array.
	/// For example, -2 refers to the second to last element of the array.
	/// 返回数组片段副本，不修改原数组；负索引相对数组末尾计算，例如 <c>-2</c> 表示倒数第二项。
	/// </summary>
	/// <param name="start">The beginning index of the specified portion of the array.</param>
	/// <param name="end">The end index of the specified portion of the array. This is exclusive of the element at the index 'end'.</param>
	/// <returns>If <paramref name="start" /> is omitted, the slice begins at index 0. If <paramref name="end" /> is omitted, the slice extends to the end of the array.</returns>
	[Description("@#slice")]
	public extern Array<T> Slice(Number? start = null, Number? end = null);

	/// <summary>
	/// Sorts an array in place.
	/// This method mutates the array and returns a reference to the same array.
	/// 原地排序并返回同一数组。未提供比较器时按 JavaScript UTF-16 代码单元顺序排序。
	/// </summary>
	/// <param name="compareFn"><para><b>(a: T, b: T) => number</b></para>Function used to determine the order of the elements.It is expected to return</param>
	/// <returns>a negative value if the first argument is less than the second argument, zero if they're equal, and a positive value otherwise.If omitted, the elements are sorted in ascending, UTF-16 code unit order.</returns>
	[Description("@#sort")]
	public extern Array<T> Sort(Func<T, T, Number>? compareFn = null);

	/// <summary>CLR comparison delegate bridge for JavaScript <c>sort</c>. 面向 JavaScript <c>sort</c> 的 CLR 比较委托桥接重载。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#sort")]
	public extern Array<T> Sort(Comparison<T> compareFn);

	/// <summary>
	/// Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
	/// 从指定位置删除元素，并可在其位置插入元素；该操作原地修改数组并返回被删除项。
	/// </summary>
	/// <param name="start">The zero-based location in the array from which to start removing elements.</param>
	/// <param name="deleteCount">The number of elements to remove.</param>
	/// <returns>An array containing the elements that were deleted.</returns>
	[Description("@#splice")]
	public extern Array<T> Splice(Number start, Number? deleteCount = null);

	/// <summary>
	/// Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
	/// 从指定位置删除并插入元素；返回删除项且原地修改数组。
	/// </summary>
	/// <param name="start">The zero-based location in the array from which to start removing elements.</param>
	/// <param name="deleteCount">The number of elements to remove.</param>
	/// <param name="items">Elements to insert into the array in place of the deleted elements.</param>
	/// <returns>An array containing the elements that were deleted.</returns>
	[Description("@#splice")]
	public extern Array<T> Splice(Number start, Number deleteCount, params T[] items);

	/// <summary>
	/// Inserts new elements at the start of an array, and returns the new length of the array.
	/// 在数组开头插入元素，原地修改并返回新长度。
	/// </summary>
	/// <param name="items">Elements to insert at the start of the array.</param>
	/// <returns></returns>
	[Description("@#unshift")]
	public extern Number Unshift(params T[] items);

	/// <summary>
	/// Returns the index of the first occurrence of a value in an array, or -1 if it is not present.
	/// 返回首个匹配值的索引；不存在时返回 <c>-1</c>，比较语义遵循 JavaScript strict equality。
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search.If fromIndex is omitted, the search starts at index 0.</param>
	/// <returns></returns>
	[Description("@#indexOf")]
	public extern Number IndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Projection of JavaScript <c>Array.prototype.includes</c>.
	/// This stays on the array host so user code can follow JavaScript runtime shape directly.
	/// 直接投影 JavaScript <c>Array.prototype.includes</c>，使用 SameValueZero 比较规则，因而与 <c>IndexOf</c> 对 <c>NaN</c> 的处理不同。
	/// </summary>
	[Description("@#includes")]
	public extern bool Includes(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Returns the index of the last occurrence of a specified value in an array, or -1 if it is not present.
	/// 从后向前查找最后一个匹配项；找不到时返回 <c>-1</c>。
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin searching backward.If fromIndex is omitted, the search starts at the last index in the array.</param>
	/// <returns></returns>
	[Description("@#lastIndexOf")]
	public extern Number LastIndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Determines whether all the members of an array satisfy the specified test.
	/// 判断所有元素是否满足回调；回调返回值按 JavaScript truthy/falsy 规则解释，并在首个 falsy 结果时停止。
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments. The every method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#every")]
	public extern bool Every(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	/// <summary>Index-aware overload of <c>every</c>. 带索引回调的 <c>every</c> 重载。</summary>
	[Description("@#every")]
	public extern bool Every(Func<T, Number, object?> predicate, object? thisArg = null);

		/// <summary>Value-only overload of <c>every</c>. 仅接收元素值的 <c>every</c> 重载。</summary>
		[Description("@#every")]
		public extern bool Every(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// 该 CLR 便利重载被隐藏；JavaScript 数组谓词采用 truthy/falsy 语义，上述 object 返回值重载才是主要运行时形状。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#every")]
		public extern bool Every(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Determines whether the specified callback function returns true for any element of an array.
	/// 判断是否存在满足回调的元素；回调结果按 JavaScript truthy/falsy 解释，并在首个 truthy 结果时停止。
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments.The some method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value true, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#some")]
	public extern bool Some(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	/// <summary>Index-aware overload of <c>some</c>. 带索引回调的 <c>some</c> 重载。</summary>
	[Description("@#some")]
	public extern bool Some(Func<T, Number, object?> predicate, object? thisArg = null);

		/// <summary>Value-only overload of <c>some</c>. 仅接收元素值的 <c>some</c> 重载。</summary>
		[Description("@#some")]
		public extern bool Some(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// 该 CLR 便利重载被隐藏；JavaScript 数组谓词采用 truthy/falsy 语义，上述 object 返回值重载才是主要运行时形状。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#some")]
		public extern bool Some(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Performs the specified action for each element in an array.
	/// 对每个元素执行回调。回调执行过程中数组变更的可见性遵循 JavaScript <c>forEach</c> 语义。
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: IEnumerable<T>) => void</b></para>A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, Number, Array<T>> callbackfn, object? thisArg = null);
	/// <summary>Runs an index-aware callback for each array element. 对每个数组元素执行带索引的回调。</summary>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, Number> callbackfn, object? thisArg = null);
	/// <summary>Runs a callback for each array element. 对每个数组元素执行回调。</summary>
	[Description("@#forEach")]
	public extern void ForEach(Action<T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Calls a defined callback function on each element of an array, and returns an array that contains the results.
	/// 为每个元素执行映射回调并返回新数组；不会修改源数组，空槽处理遵循 JavaScript <c>map</c> 语义。
	/// </summary>
	/// <typeparam name="U">Compile-time annotation for mapped elements. 映射后元素的编译期类型标注。</typeparam>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: IEnumerable<T>) => U</b></para>A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#map")]
	public extern Array<U> Map<U>(Func<T, Number, Array<T>, U> callbackfn, object? thisArg = null);

	/// <summary>Index-aware mapping overload. 带索引的映射重载。</summary>
	[Description("@#map")]
	public extern Array<U> Map<U>(Func<T, Number, U> callbackfn, object? thisArg = null);

	/// <summary>Value-only mapping overload. 仅接收元素值的映射重载。</summary>
	[Description("@#map")]
	public extern Array<U> Map<U>(Func<T, U> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the elements of an array that meet the condition specified in a callback function.
	/// 返回回调结果为 truthy 的元素组成的新数组；不修改源数组。
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments.The filter method calls the predicate function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	/// <summary>Index-aware filtering overload. 带索引的筛选重载。</summary>
	[Description("@#filter")]
	public extern Array<T> Filter(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	[Description("@#filter")]
	public extern Array<T> Filter(Func<T, Number, object?> predicate, object? thisArg = null);

		/// <summary>Value-only filtering overload. 仅接收元素值的筛选重载。</summary>
		[Description("@#filter")]
		public extern Array<T> Filter(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// 该 CLR 便利重载被隐藏；JavaScript 谓词采用 truthy/falsy 语义，上述 object 返回值重载才是主要运行时形状。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#filter")]
		public extern Array<T> Filter(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Returns the first element whose value satisfies the provided testing function.
	/// Nullable is used because JavaScript returns <c>undefined</c> when no matching element exists,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// 从后向前查找最后一个满足谓词的元素；未匹配时 JavaScript <c>undefined</c> 在 C# 中投影为 <see langword="null"/>。
	/// 返回第一个满足谓词的元素；没有匹配项时 JavaScript 的 <c>undefined</c> 在 C# 中投影为 <see langword="null"/>。
	/// </summary>
	[Description("@#find")]
	public extern T? Find(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	/// <summary>Index-aware overload of <c>find</c>. 带索引回调的 <c>find</c> 重载。</summary>
	[Description("@#find")]
	public extern T? Find(Func<T, Number, object?> predicate, object? thisArg = null);

		/// <summary>Value-only overload of <c>find</c>. 仅接收元素值的 <c>find</c> 重载。</summary>
		[Description("@#find")]
		public extern T? Find(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// 该 CLR 便利重载被隐藏；JavaScript 谓词采用 truthy/falsy 语义，上述 object 返回值重载才是主要运行时形状。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#find")]
		public extern T? Find(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Returns a new array with nested array elements recursively concatenated up to the specified depth.
	/// The return type is widened to <see cref="Array{T}"/> of <see cref="object"/> because JavaScript flattening changes the element shape in ways C# generics cannot faithfully express here.
	/// 将嵌套数组递归展开到指定深度。返回元素类型放宽为 <see cref="object"/>，因为 JavaScript 展平后的元素形状无法由 C# 泛型精确表示。
	/// </summary>
	[Description("@#flat")]
	public extern Array<object?> Flat(Number? depth = null);

	/// <summary>
	/// Maps each element to a value and then flattens the result by one level.
	/// This overload covers the JavaScript case where the callback returns scalar values.
	/// 将每项映射后展开一层；此组重载覆盖回调返回标量的 JavaScript <c>flatMap</c> 情形。
	/// </summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, Array<T>, U> callbackfn, object? thisArg = null);

	/// <summary>Index-aware scalar <c>flatMap</c> overload. 带索引、回调返回标量的 <c>flatMap</c> 重载。</summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, U> callbackfn, object? thisArg = null);

	/// <summary>Value-only scalar <c>flatMap</c> overload. 仅接收元素值、回调返回标量的 <c>flatMap</c> 重载。</summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, U> callbackfn, object? thisArg = null);

	/// <summary>
	/// Maps each element to an array and then flattens the mapped arrays by one level.
	/// This matches the most common JavaScript <c>flatMap</c> usage while keeping the C# generic result type explicit.
	/// 映射为数组后展开一层；该重载保留最常见的 JavaScript <c>flatMap</c> 用法，并显式保持 C# 泛型结果类型。
	/// </summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, Array<T>, Array<U>> callbackfn, object? thisArg = null);

	/// <summary>Index-aware array-producing <c>flatMap</c> overload. 带索引、回调返回数组的 <c>flatMap</c> 重载。</summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, Array<U>> callbackfn, object? thisArg = null);

	/// <summary>Value-only array-producing <c>flatMap</c> overload. 仅接收元素值、回调返回数组的 <c>flatMap</c> 重载。</summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Array<U>> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the last element whose value satisfies the provided testing function.
	/// Nullable is used because JavaScript returns <c>undefined</c> when no matching element exists,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	/// <summary>Index-aware overload of <c>findLast</c>. 带索引回调的 <c>findLast</c> 重载。</summary>
	[Description("@#findLast")]
	public extern T? FindLast(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	[Description("@#findLast")]
	public extern T? FindLast(Func<T, Number, object?> predicate, object? thisArg = null);

		/// <summary>Value-only overload of <c>findLast</c>. 仅接收元素值的 <c>findLast</c> 重载。</summary>
		[Description("@#findLast")]
		public extern T? FindLast(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// 该 CLR 便利重载被隐藏；JavaScript 谓词采用 truthy/falsy 语义，上述 object 返回值重载才是主要运行时形状。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#findLast")]
		public extern T? FindLast(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// C# host projection of JavaScript <c>Array.prototype.at</c>.
	/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// JavaScript <c>Array.prototype.at</c> 的 C# 投影，支持负索引；越界的 <c>undefined</c> 投影为 <see langword="null"/>。
	/// </summary>
	[Description("@#at")]
	public extern T? At(Number index);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从左向右归约数组。未提供初始值时首个元素作为累加器；空数组会遵循 JavaScript <c>reduce</c> 的运行时错误语义。
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: IEnumerable&lt;T&gt;) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the first array element as the initial accumulator.</param>
	/// <returns>The accumulated result.</returns>
	/// <summary>Value-only overload of <c>reduce</c>. 仅接收累加值和当前元素的 <c>reduce</c> 重载。</summary>
	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, Number, Array<T>, T> callbackfn);

	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从左向右归约数组，并以 <paramref name="initialValue"/> 作为显式累加器初值。
	/// </summary>
	/// <typeparam name="U">Compile-time annotation for the accumulator. 累加器的编译期类型标注。</typeparam>
	/// <param name="callbackfn"><para><b>(previousValue: U, currentValue: T, currentIndex: number, array: IEnumerable<T>) => U</b></para>A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	/// <summary>Value-only overload with an explicit initial accumulator. 带显式初始累加器的仅值 <c>reduce</c> 重载。</summary>
	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, Number, Array<T>, U> callbackfn, U initialValue);

	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从右向左归约数组。未提供初始值时末个元素作为累加器；空数组会遵循 JavaScript <c>reduceRight</c> 的运行时错误语义。
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: IEnumerable&lt;T&gt;) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the last array element as the initial accumulator.</param>
	/// <returns>The accumulated result.</returns>
	/// <summary>Value-only overload of <c>reduceRight</c>. 仅接收累加值和当前元素的 <c>reduceRight</c> 重载。</summary>
	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, Number, Array<T>, T> callbackfn);

	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从右向左归约数组，并以 <paramref name="initialValue"/> 作为显式累加器初值。
	/// </summary>
	/// <typeparam name="U">Compile-time annotation for the accumulator. 累加器的编译期类型标注。</typeparam>
	/// <param name="callbackfn">A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	/// <summary>Value-only overload with an explicit initial accumulator. 带显式初始累加器的仅值 <c>reduceRight</c> 重载。</summary>
	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, Number, Array<T>, U> callbackfn, U initialValue);

	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// Fills an array range with one fixed value and returns the mutated array.
	/// 用一个固定值填充数组中从起始索引（默认为 0）到终止索引（默认为 array.length）内的全部元素，并返回被修改的数组。
	/// </summary>
	/// <param name="value">用来填充数组元素的值。注意所有数组中的元素都将是这个确定的值：如果 value 是个对象，那么数组的每一项都会引用这个元素。</param>
	/// <param name="start">
	/// 基于零的索引，从此开始填充，转换为整数。
	/// 负数索引从数组的末端开始计算，如果 start < 0，则使用 start + array.length。
	/// 如果 start < -array.length 或 start 被省略，则使用 0。
	/// 如果 start >= array.length，没有索引被填充。
	/// </param>
	/// <param name="end">
	/// 基于零的索引，在此结束填充，转换为整数。fill() 填充到但不包含 end 索引。
	/// 负数索引从数组的末端开始计算，如果 end < 0，则使用 end + array.length。
	/// 如果 end < -array.length，则使用 0。
	/// 如果 end >= array.length 或 end 被省略，则使用 array.length，导致所有索引都被填充。
	/// 如果经标准化后，end 的位置在 start 之前或之上，没有索引被填充。
	/// </param>
	/// <returns>经 value 填充修改后的数组。</returns>
	[Description("@#fill")]
	public extern Array<T> Fill(T value, Number? start = null, Number? end = null);

	/// <summary>Finds the first truthy predicate match and returns its index, or <c>-1</c>. 查找首个谓词为 truthy 的元素并返回索引；未找到时返回 <c>-1</c>。</summary>
	[Description("@#findIndex")]
	public extern Number FindIndex(Func<T, Number, Array<T>, object?> callbackfn, object? thisArg = null);

	/// <summary>Index-aware overload of <c>findIndex</c>. 带索引回调的 <c>findIndex</c> 重载。</summary>
	[Description("@#findIndex")]
	public extern Number FindIndex(Func<T, Number, object?> callbackfn, object? thisArg = null);

		/// <summary>Value-only overload of <c>findIndex</c>. 仅接收元素值的 <c>findIndex</c> 重载。</summary>
		[Description("@#findIndex")]
		public extern Number FindIndex(Func<T, object?> callbackfn, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// 该 CLR 便利重载被隐藏；JavaScript 谓词采用 truthy/falsy 语义，上述 object 返回值重载才是主要运行时形状。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#findIndex")]
		public extern Number FindIndex(Predicate<T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the index of the last element whose value satisfies the provided testing function, or <c>-1</c> if no match is found.
	/// 从后向前查找最后一个谓词为 truthy 的元素；没有匹配项时返回 <c>-1</c>。
	/// </summary>
	/// <summary>Index-aware overload of <c>findLastIndex</c>. 带索引回调的 <c>findLastIndex</c> 重载。</summary>
	[Description("@#findLastIndex")]
	public extern Number FindLastIndex(Func<T, Number, Array<T>, object?> callbackfn, object? thisArg = null);

	[Description("@#findLastIndex")]
	public extern Number FindLastIndex(Func<T, Number, object?> callbackfn, object? thisArg = null);

		/// <summary>Value-only overload of <c>findLastIndex</c>. 仅接收元素值的 <c>findLastIndex</c> 重载。</summary>
		[Description("@#findLastIndex")]
		public extern Number FindLastIndex(Func<T, object?> callbackfn, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// 该 CLR 便利重载被隐藏；JavaScript 谓词采用 truthy/falsy 语义，上述 object 返回值重载才是主要运行时形状。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#findLastIndex")]
		public extern Number FindLastIndex(Predicate<T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns a copied array with the elements in reverse order.
	/// This stays distinct from <see cref="Reverse"/> because JavaScript exposes a non-mutating copy-producing variant.
	/// 返回反转后的数组副本，不修改源数组；它不同于会原地修改的 <see cref="Reverse"/>。
	/// </summary>
	[Description("@#toReversed")]
	public extern Array<T> ToReversed();

	/// <summary>
	/// Returns a copied array with its elements sorted.
	/// This stays distinct from <see cref="Sort(Func{T, T, Number}?)"/> because JavaScript exposes a non-mutating copy-producing variant.
	/// 返回排序后的数组副本，不修改源数组；它不同于会原地修改的 <see cref="Sort(Func{T, T, Number}?)"/>。
	/// </summary>
	[Description("@#toSorted")]
	public extern Array<T> ToSorted(Func<T, T, Number>? compareFn = null);

	[EditorBrowsable(EditorBrowsableState.Never)]
	/// <summary>CLR comparison delegate bridge for JavaScript <c>toSorted</c>. 面向 JavaScript <c>toSorted</c> 的 CLR 比较委托桥接重载。</summary>
	[Description("@#toSorted")]
	public extern Array<T> ToSorted(Comparison<T> compareFn);

	/// <summary>
	/// Returns a copied array with items removed and optionally inserted at the given index.
	/// This mirrors JavaScript <c>Array.prototype.toSpliced</c>, which does not mutate the source array.
	/// 返回删除指定元素后的数组副本，不修改源数组；对应 JavaScript <c>Array.prototype.toSpliced</c>。
	/// </summary>
	[Description("@#toSpliced")]
	public extern Array<T> ToSpliced(Number start, Number? deleteCount = null);

	/// <summary>
	/// Returns a copied array with items removed and optionally inserted at the given index.
	/// This mirrors JavaScript <c>Array.prototype.toSpliced</c>, which does not mutate the source array.
	/// 返回删除并插入元素后的数组副本，不修改源数组；对应 JavaScript <c>Array.prototype.toSpliced</c>。
	/// </summary>
	[Description("@#toSpliced")]
	public extern Array<T> ToSpliced(Number start, Number deleteCount, params T[] items);

	/// <summary>
	/// Returns a copied array with the element at the specified index replaced.
	/// Negative indices follow JavaScript <c>Array.prototype.with</c> semantics and count from the end.
	/// 返回替换指定索引元素后的数组副本；负索引遵循 JavaScript <c>Array.prototype.with</c> 并从末尾计数。
	/// </summary>
	[Description("@#with")]
	public extern Array<T> With(Number index, T value);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Array.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Array.prototype.keys()</c> 产生的 JavaScript 迭代器；以 <see cref="IEnumerable{T}"/> 作为 C# 中可枚举值的宿主表示。
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<Number> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Array.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Array.prototype.values()</c> 产生的 JavaScript 迭代器；以 <see cref="IEnumerable{T}"/> 作为 C# 中可枚举值的宿主表示。
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<T> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Array.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[index, value]</c>.
	/// 返回 <c>Array.prototype.entries()</c> 产生的 JavaScript 迭代器；每项是 <c>[index, value]</c> 二元数组。
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	/// <summary>
	/// Creates an array from a JavaScript iterable or array-like value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// 从 JavaScript 可迭代或类数组值创建数组；<see cref="IEnumerable{T}"/> 用于表达数组、列表等可映射为 JavaScript iterable 的 C# 输入。
	/// </summary>
	[Description("@#from")]
	public extern static Array<T> From(IEnumerable<T> arrayLike);

	/// <summary>
	/// Creates an array from a JavaScript iterable or array-like value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// 从可迭代或类数组值创建数组，并对每项执行带索引映射；<see cref="IEnumerable{T}"/> 为 C# 输入侧的通用可迭代表达。
	/// </summary>
	[Description("@#from")]
	public extern static Array<T> From<U>(IEnumerable<U> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript iterable or array-like value.
	/// This overload mirrors JavaScript <c>Array.from</c> when the caller does not need the element index in the mapping callback.
	/// 对应 JavaScript <c>Array.from</c> 的仅值映射回调重载。
	/// </summary>
	[Description("@#from")]
	public extern static Array<T> From<U>(IEnumerable<U> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript async iterable or iterable value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// 从 JavaScript async iterable 或 iterable 创建数组；结果 Promise 在每个输入项被消费后兑现。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync(IEnumerable<T> arrayLike);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and awaits each element before storing it.
	/// <see cref="IPromise{T}"/> is used as the host surface for JavaScript promise-like values.
	/// 从 Promise-like 项创建数组并等待每一项；<see cref="IPromise{T}"/> 是 JavaScript Promise-like 值的 C# 宿主表面。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync(IEnumerable<IPromise<T>> arrayLike);

	/// <summary>
	/// Creates an array from a JavaScript async iterable or iterable value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// 从 async iterable 或 iterable 创建数组并使用带索引映射回调；映射与等待顺序遵循 JavaScript <c>Array.fromAsync</c>。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript iterable and awaits each mapper result before storing it.
	/// This matches the JavaScript case where <c>Array.fromAsync</c> receives an async mapping callback.
	/// 对应 JavaScript <c>Array.fromAsync</c> 接收异步映射回调的情形；每个映射结果会被等待后存入结果数组。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, Number, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks.
	/// JavaScript still sees the usual async mapper behavior; the bridge type only exists on the C# side.
	/// 此桥接重载只服务编译器 lowering；JavaScript 仍看到普通异步映射行为，<c>PromiseResult&lt;T&gt;</c> 不引入新的运行时类型。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, Number, PromiseResult<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies a synchronous mapping callback to the awaited source values.
	/// 从 Promise-like 输入创建数组，并对已等待的源值执行同步映射。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies an async mapping callback to the awaited source values.
	/// 从 Promise-like 输入创建数组，并对已等待的源值执行异步映射。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, Number, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks over promise-like source items.
	/// 面向 Promise-like 源项异步映射的编译器专用桥接重载。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, Number, PromiseResult<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript async iterable or iterable value.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the mapping callback.
	/// 对应无需元素索引的 JavaScript <c>Array.fromAsync</c> 同步映射回调。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript iterable and awaits each mapper result before storing it.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the async mapping callback.
	/// 对应无需元素索引的 JavaScript <c>Array.fromAsync</c> 异步映射回调。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks without the index parameter.
	/// 不带索引参数的编译器专用异步映射桥接重载。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, PromiseResult<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies a synchronous mapping callback to the awaited source values.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the mapping callback.
	/// 对 Promise-like 源项执行无需索引的同步映射，对应 JavaScript <c>Array.fromAsync</c>。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies an async mapping callback to the awaited source values.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the mapping callback.
	/// 对 Promise-like 源项执行无需索引的异步映射，对应 JavaScript <c>Array.fromAsync</c>。
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks over promise-like source items without the index parameter.
	/// 对 Promise-like 源项、不带索引参数的编译器专用异步映射桥接重载。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, PromiseResult<T>> mapFn, object? thisArg = null);

	/// <summary>Checks whether a runtime value is a JavaScript array. 检查运行时值是否为 JavaScript 数组。</summary>
	[Description("@#isArray")]
	public extern static bool IsArray(object? value);

	/// <summary>Creates a JavaScript array from the supplied values without flattening iterables. 从提供的值创建 JavaScript 数组，不展开其中的可迭代值。</summary>
	[Description("@#of")]
	public extern static Array<T> Of(params T[] value);

	/// <summary>
	/// CLR bridge members kept for collection-initializer and collection-like interop.
	/// They do not correspond to distinct JavaScript <c>Array.prototype</c> members.
	/// 这些成员只为集合初始化器与集合式 CLR 互操作保留，不对应独立的 JavaScript <c>Array.prototype</c> 成员。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	/// <summary>Adds an item through the CLR collection-initializer bridge. 通过 CLR 集合初始化器桥接添加元素。</summary>
	public extern void Add(T item);

	[EditorBrowsable(EditorBrowsableState.Never)]
	/// <summary>Clears items through the CLR collection bridge. 通过 CLR 集合桥接清空元素。</summary>
	public extern virtual void Clear();

	[EditorBrowsable(EditorBrowsableState.Never)]
	/// <summary>Checks an item through the CLR collection bridge. 通过 CLR 集合桥接检查是否包含元素。</summary>
	public extern bool Contains(T item);

	[EditorBrowsable(EditorBrowsableState.Never)]
	/// <summary>Copies items through the CLR collection bridge. 通过 CLR 集合桥接复制元素。</summary>
	public extern void CopyTo(T[] array, int arrayIndex);

	[EditorBrowsable(EditorBrowsableState.Never)]
	/// <summary>Removes an item through the CLR collection bridge. 通过 CLR 集合桥接删除元素。</summary>
	public extern bool Remove(T item);

	extern IEnumerator IEnumerable.GetEnumerator();

	//extern IEnumerator<T> IEnumerable<T>.GetEnumerator();
}

public static partial class Global
{
	extension(Array array)
	{
		/// <summary>
		/// Mirrors JavaScript's <c>Array.isArray</c> static check on the global <c>Array</c> constructor host.
		/// 在全局 <c>Array</c> 构造器宿主上镜像 JavaScript 的静态 <c>Array.isArray</c> 检查。
		/// </summary>
		[Description("@#isArray")]
		public extern static bool IsArray(object? obj);
	}
}
