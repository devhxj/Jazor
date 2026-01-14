using Acornima.Ast;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace ECMAScript.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理 using 语句操作
	/// C# 示例：
	/// using (var resource = new DisposableResource()) {
	///     resource.DoWork();
	/// }
	/// 转换结果：不支持，JavaScript 没有内置的资源管理机制
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitUsing(IUsingOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Using statements are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 Stop 操作（编译器内部）
	/// 这是编译器内部使用的操作，不对应具体的 C# 语法
	/// 转换结果：不支持
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitStop(IStopOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Stop operations are compiler-internal and not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 End 操作（编译器内部）
	/// 这是编译器内部使用的操作，不对应具体的 C# 语法
	/// 转换结果：不支持
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitEnd(IEndOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "End operations are compiler-internal and not supported in JavaScript conversion.");

	/// <summary>
	/// 处理事件触发操作
	/// C# 示例：
	/// MyEvent?.Invoke(args);  // 触发事件
	/// 转换结果：不支持，JavaScript 事件模型与 C# 多播事件模型根本不同
	/// 原因：C# 事件支持多播委托、线程安全访问和弱引用，而 JavaScript 事件是简单的回调函数模式
	/// 替代方案：在 JavaScript 中使用自定义事件发射器模式或观察者模式
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitRaiseEvent(IRaiseEventOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Raising events is not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 For-To 循环操作（VB.NET 特有）
	/// VB.NET 示例：
	/// For i = 1 To 10
	///     Console.WriteLine(i)
	/// Next
	/// 转换结果：不支持，请使用标准 for 循环
	/// 原因：VB.NET 的 For-To 循环语法是 VB.NET 特有的，JavaScript 没有对应的语法结构
	/// 替代方案：使用 JavaScript 的 for 循环：for (let i = 1; i <= 10; i++) { console.log(i); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitForToLoop(IForToLoopOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "For-To loops are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 lock 语句操作
	/// C# 示例：
	/// lock (lockObject) {
	///     // 线程安全代码
	/// }
	/// 转换结果：不支持，JavaScript 是单线程语言且没有内置的锁机制
	/// 原因：JavaScript 是单线程事件循环模型，没有多线程竞争条件，因此不需要锁机制
	/// 替代方案：在 JavaScript 中，异步操作使用 Promise/async-await，共享状态使用原子操作或互斥锁库
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitLock(ILockOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Lock statements are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理事件引用操作
	/// C# 示例：
	/// obj.MyEvent += Handler;      // 事件订阅
	/// obj.MyEvent -= Handler;      // 事件取消订阅
	/// 转换结果：不支持，JavaScript 事件模型与 C# 多播事件模型根本不同
	/// 原因：C# 事件支持多播委托、线程安全访问和弱引用，而 JavaScript 事件是简单的回调函数模式
	/// 替代方案：在 JavaScript 中使用 addEventListener/removeEventListener 或自定义事件系统
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitEventReference(IEventReferenceOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Event references are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理事件赋值操作
	/// C# 示例：
	/// event += handler;           // 事件订阅
	/// event -= handler;           // 事件取消订阅
	/// 转换结果：不支持，JavaScript 事件模型与 C# 多播事件模型根本不同
	/// 原因：C# 事件模型（多播、弱引用、线程安全）与 JavaScript 事件模型根本不同，无法保证语义等价
	/// 替代方案：在 JavaScript 中使用事件发射器模式或观察者模式
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitEventAssignment(IEventAssignmentOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Event assignments are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理动态对象创建操作
	/// C# 示例：
	/// dynamic obj = new ExpandoObject();  // 动态对象创建
	/// dynamic result = Activator.CreateInstance(type);  // 动态类型实例化
	/// 转换结果：不支持，JavaScript 的动态性与 C# dynamic 语义不同
	/// 原因：C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约
	/// 替代方案：在 JavaScript 中使用普通对象字面量 {} 或 Map 数据结构
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, Context argument)
		 => HandleTransformationFailure<Node>(operation, "Dynamic object creation is not supported in JavaScript conversion.");

	/// <summary>
	/// 处理动态成员引用操作
	/// C# 示例：
	/// dynamic obj = GetDynamicObject();
	/// var value = obj.SomeMember;         // 动态成员访问
	/// 转换结果：不支持，需要编译时确定成员信息
	/// 原因：C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约
	/// 替代方案：在 JavaScript 中使用 obj.property 或 obj['property'] 访问属性
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Dynamic member references are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理动态方法调用操作
	/// C# 示例：
	/// dynamic obj = GetDynamicObject();
	/// obj.SomeMethod(arg1, arg2);         // 动态方法调用
	/// 转换结果：不支持，需要编译时确定方法签名
	/// 原因：C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约
	/// 替代方案：在 JavaScript 中使用 obj.method(arg1, arg2) 或 obj['method'](arg1, arg2) 调用方法
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDynamicInvocation(IDynamicInvocationOperation operation, Context argument)
	 => HandleTransformationFailure<Node>(operation, "Dynamic method invocations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理动态索引器访问操作
	/// C# 示例：
	/// dynamic obj = GetDynamicObject();
	/// var value = obj["key"];             // 动态索引器访问
	/// obj[0] = newValue;                  // 动态索引器赋值
	/// 转换结果：不支持，需要编译时确定索引器类型
	/// 原因：C# 动态绑定语义（运行时解析、重载决策、动态分派）与 JavaScript 静态分派模型根本不可通约
	/// 替代方案：在 JavaScript 中使用 obj[key] 或 obj[index] 访问属性
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Dynamic indexer access is not supported in JavaScript conversion.");

	/// <summary>
	/// 处理已翻译的 LINQ 查询操作
	/// C# 示例：
	/// var result = from x in collection
	///              where x > 0
	///              select x * 2;              // LINQ 查询表达式
	/// var filtered = list.Where(x => x > 0); // LINQ 方法链
	/// 转换结果：不支持，LINQ 语义复杂且 JavaScript 没有对应构造
	/// 原因：LINQ 提供了延迟执行、表达式树和查询提供程序模式，JavaScript 没有对应的查询构造
	/// 替代方案：在 JavaScript 中使用数组方法（filter、map、reduce）或第三方库（如 lodash）
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitTranslatedQuery(ITranslatedQueryOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Translated LINQ queries are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 typeof 运算符操作
	/// C# 示例：
	/// typeof(int)                         // 获取类型信息
	/// typeof(MyClass)                     // 获取自定义类型信息
	/// 转换结果：不支持，JavaScript typeof 语义与 C# 不同
	/// 原因：C# typeof 获取类型信息（System.Type），而 JavaScript typeof 获取值类型（string、number等）
	/// 替代方案：在 JavaScript 中使用 typeof 操作符获取值类型，或使用 constructor.name 获取构造函数名
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitTypeOf(ITypeOfOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "typeof operator is not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 sizeof 运算符操作
	/// C# 示例：
	/// sizeof(int)                         // 4 字节
	/// sizeof(double)                      // 8 字节
	/// 转换结果：不支持，JavaScript 没有直接的内存大小概念
	/// 原因：JavaScript 是垃圾回收语言，不提供直接的内存大小控制，值的大小由引擎管理
	/// 替代方案：在 JavaScript 中使用 Buffer.byteLength（Node.js）或序列化后的字节长度估算
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitSizeOf(ISizeOfOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "sizeof operator is not supported in JavaScript conversion.");

	/// <summary>
	/// 处理取地址运算符操作
	/// C# 示例：
	/// unsafe {
	///     int x = 42;
	///     int* ptr = &x;                   // 获取变量地址
	/// }
	/// 转换结果：不支持，JavaScript 是安全语言，不支持指针操作
	/// 原因：JavaScript 是安全语言，不支持指针、函数指针或 unsafe 语义，以防止内存安全问题
	/// 替代方案：在 JavaScript 中使用引用传递（对象）或 ArrayBuffer 处理二进制数据
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitAddressOf(IAddressOfOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Address of operator is not supported in JavaScript conversion.");

	/// <summary>
	/// 处理方法体操作（编译器内部）
	/// 这是编译器内部使用的操作，不对应具体的 C# 语法
	/// 转换结果：不支持
	/// 原因：编译器内部操作，不对应具体的 C# 语法，无法转换为 JavaScript
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitMethodBodyOperation(IMethodBodyOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Method body operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理构造函数体操作（编译器内部）
	/// 这是编译器内部使用的操作，不对应具体的 C# 语法
	/// 转换结果：不支持
	/// 原因：编译器内部操作，不对应具体的 C# 语法，无法转换为 JavaScript
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitConstructorBodyOperation(IConstructorBodyOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Constructor body operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理捕获异常操作（编译器内部）
	/// 这是编译器内部使用的操作，表示在 catch 块中捕获的异常
	/// 转换结果：不支持
	/// 原因：编译器内部操作，不对应具体的 C# 语法，无法转换为 JavaScript
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitCaughtException(ICaughtExceptionOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Caught exception operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理静态本地初始化信号量操作（编译器内部）
	/// 这是编译器内部使用的操作，用于线程安全的静态变量初始化
	/// 转换结果：不支持
	/// 原因：编译器内部操作，用于线程安全的静态变量初始化，而 JavaScript 是单线程的
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitStaticLocalInitializationSemaphore(IStaticLocalInitializationSemaphoreOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Static local initialization semaphore operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理流匿名函数操作（编译器内部）
	/// 这是编译器内部使用的操作，用于数据流分析中的匿名函数
	/// 转换结果：不支持
	/// 原因：编译器内部操作，用于数据流分析，不对应具体的 C# 语法，无法转换为 JavaScript
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitFlowAnonymousFunction(IFlowAnonymousFunctionOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Flow anonymous function operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理范围 case 子句操作（VB.NET 特有）
	/// VB.NET 示例：
	/// Select Case value
	///     Case 1 To 10
	///         DoSomething()
	/// End Select
	/// 转换结果：不支持
	/// 原因：VB.NET 特有的范围 case 子句需要复杂的范围检查逻辑，JavaScript 没有对应的语法
	/// 替代方案：在 JavaScript 中使用 if 语句和范围检查：if (value >= 1 && value <= 10) { doSomething(); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitRangeCaseClause(IRangeCaseClauseOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Range case clause operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理流捕获操作（编译器内部）
	/// 这是编译器内部使用的操作，用于数据流分析
	/// 转换结果：不支持
	/// 原因：编译器内部操作，用于数据流分析，不对应具体的 C# 语法，无法转换为 JavaScript
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitFlowCapture(IFlowCaptureOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Flow capture operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理流捕获引用操作（编译器内部）
	/// 这是编译器内部使用的操作，用于数据流分析
	/// 转换结果：不支持
	/// 原因：编译器内部操作，用于数据流分析，不对应具体的 C# 语法，无法转换为 JavaScript
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitFlowCaptureReference(IFlowCaptureReferenceOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Flow capture reference operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理关系 case 子句操作（VB.NET 特有）
	/// VB.NET 示例：
	/// Select Case value
	///     Case Is > 10
	///         DoSomething()
	/// End Select
	/// 转换结果：不支持
	/// 原因：VB.NET 特有的关系 case 子句无法直接转换为 JavaScript
	/// 替代方案：在 JavaScript 中使用 if 语句和条件判断：if (value > 10) { doSomething(); }
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitRelationalCaseClause(IRelationalCaseClauseOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Relational case clause operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理范围操作（Range 操作符）
	/// C# 示例：
	/// Range range = 1..5;                // 创建范围 1 到 5
	/// var slice = array[1..^1];          // 使用范围进行切片
	/// array[start..end]                  // 数组切片操作
	/// list[2..^2]                        // 列表切片，从索引 2 到倒数第 2 个
	/// 转换结果：不支持
	/// 原因：C# Range 操作必须在索引器操作中被消费转换为 slice/splice 调用，单独的 Range 对象在 JavaScript 中无意义
	/// 替代方案：在 JavaScript 中直接使用 slice 方法：array.slice(1, 5) 或 array.slice(start, end+1)
	/// </summary>
	/// <param name="operation">范围操作</param>
	/// <param name="argument">当前operation所属的父operation</param>
	/// <returns>JavaScript范围对象字面量</returns>
	public override Acornima.Ast.Node? VisitRangeOperation(IRangeOperation operation, Context argument)
	{
		// 检查是否在数组元素访问的上下文中
		if (operation.Parent is IArrayElementReferenceOperation arrayRef)
		{
			// 在数组元素访问上下文中，范围操作应该已经在 VisitArrayElementReference 中处理
			// 这里不应该到达，但为了安全起见，返回一个错误
			return HandleTransformationFailure<Node>(operation, "Range operation in array access should be handled by VisitArrayElementReference.");
		}

		// 单独的范围操作（不在数组访问中）在 JavaScript 中没有直接等价物
		// 返回一个错误，因为无法在 JavaScript 中表示独立的范围对象
		return HandleTransformationFailure<Node>(operation, "Standalone range operations are not supported in JavaScript conversion. Use array slicing instead.");
	}

	/// <summary>
	/// 处理 ReDim 操作（VB.NET 特有）
	/// VB.NET 示例：
	/// ReDim array(10)         // 重新设置数组大小
	/// ReDim Preserve array(20) // 保留数据同时重新设置大小
	/// 转换结果：不支持
	/// 原因：这是 VB.NET 特有功能，JavaScript 没有对应的语法
	/// 替代方案：在 JavaScript 中使用数组方法调整大小：array.length = 10 或使用 slice/splice
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitReDim(IReDimOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "ReDim operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 ReDim 子句操作（VB.NET 特有）
	/// VB.NET 示例：
	/// ReDim array1(10), array2(20) 中的每个数组重新定义
	/// 转换结果：不支持
	/// 原因：这是 VB.NET 特有功能，JavaScript 没有对应的语法
	/// 替代方案：在 JavaScript 中使用数组方法调整大小：array1.length = 10; array2.length = 20;
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitReDimClause(IReDimClauseOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "ReDim clause operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 using 声明操作
	/// C# 示例：
	/// using var file = File.OpenRead("data.txt"); // using 声明
	/// using FileStream fs = new FileStream(...);   // 传统 using 声明
	/// 转换结果：不支持
	/// 原因：JavaScript 没有内置的资源管理机制，没有确定性析构
	/// 替代方案：在 JavaScript 中使用 try-finally 块手动管理资源，或使用具有 close/dispose 方法的对象
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitUsingDeclaration(IUsingDeclarationOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Using declaration operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理插值字符串处理器创建操作
	/// C# 示例：
	/// public void WriteInterpolated([InterpolatedStringHandler] ref CustomHandler handler) { ... }
	/// WriteInterpolated($"Hello {name}!"); // 触发处理器创建
	/// 转换结果：不支持
	/// 原因：插值字符串处理器的自定义 AppendLiteral/AppendFormatted 调用链无法在 JavaScript 端重现
	/// 替代方案：在 JavaScript 中使用模板字符串：`Hello ${name}!`
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInterpolatedStringHandlerCreation(IInterpolatedStringHandlerCreationOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Interpolated string handler creation operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理插值字符串追加操作
	/// C# 示例：
	/// 在插值字符串处理器中追加内容的操作
	/// 转换结果：不支持
	/// 原因：插值字符串追加操作属于处理器框架的一部分，其语义依赖于处理器上下文
	/// 替代方案：在 JavaScript 中使用模板字符串或字符串拼接
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInterpolatedStringAppend(IInterpolatedStringAppendOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Interpolated string append operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理插值字符串处理器参数占位符操作
	/// C# 示例：
	/// 插值字符串处理器中的参数占位符（编译器内部）
	/// 转换结果：不支持
	/// 原因：插值字符串处理器参数占位符是编译器内部操作，无法转换
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInterpolatedStringHandlerArgumentPlaceholder(IInterpolatedStringHandlerArgumentPlaceholderOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Interpolated string handler argument placeholder operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理函数指针调用操作
	/// C# 示例：
	/// unsafe {
	///     delegate*<int, int> ptr = &MyMethod;
	///     int result = ptr(42);    // 函数指针调用
	/// }
	/// 转换结果：不支持
	/// 原因：JavaScript 是安全语言，不支持指针、函数指针或 unsafe 语义
	/// 替代方案：在 JavaScript 中使用函数引用或箭头函数
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitFunctionPointerInvocation(IFunctionPointerInvocationOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Function pointer invocation operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理 UTF-8 字符串操作
	/// C# 示例：
	/// ReadOnlySpan<byte> utf8 = "Hello"u8; // UTF-8 字符串字面量
	/// 转换结果：不支持
	/// 原因：UTF-8 字节序列与 JavaScript UTF-16 字符串模型语义不兼容，无法保持字节级等价
	/// 替代方案：在 JavaScript 中使用 TextEncoder/TextDecoder 处理 UTF-8 数据
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitUtf8String(IUtf8StringOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "UTF-8 string operations are not supported in JavaScript conversion.");

	/// <summary>
	/// 处理内联数组访问操作
	/// C# 示例：
	/// Span<int> span = stackalloc int[10]; // 内联数组创建
	/// span[0] = 42;                        // 内联数组访问
	/// 转换结果：不支持
	/// 原因：JavaScript 没有对应的内联数组概念，无法在栈上分配固定大小数组
	/// 替代方案：在 JavaScript 中使用普通数组或 TypedArray 处理高性能场景
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Acornima.Ast.Node? VisitInlineArrayAccess(IInlineArrayAccessOperation operation, Context argument)
		=> HandleTransformationFailure<Node>(operation, "Inline array access operations are not supported in JavaScript conversion.");
}
