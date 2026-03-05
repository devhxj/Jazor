# Jazor.CompilerTest 测试任务文档

## 1. 测试覆盖概述

### 1.1 已完成的测试模块

| 模块 | 测试文件 | 测试数量 | 状态 |
|------|---------|---------|------|
| 模式匹配 | `SemanticWalkerPatternTest.cs` | 大量 | ✅ 完成 |
| 循环语句 | `SemanticWalkerLoopTest.cs` | 25+ | ✅ 完成 |
| Switch 语句 | `SemanticWalkerSwitchTest.cs` | 15+ | ✅ 完成 |
| 字符串插值 | `SemanticWalkerStringTest.cs` | 20+ | ✅ 完成 |
| 异常处理 | `SemanticWalkerTryCatchTest.cs` | 15+ | ✅ 完成 |
| 变量声明 | `SemanticWalkerDeclarationTest.cs` | 15+ | ✅ 完成 |
| 普通运算 | `SemanticWalkerOrdinaryTest.cs` | 大量 | ✅ 完成 |
| 引用测试 | `SemanticWalkerReferenceTest.cs` | 30+ | ✅ 完成 |
| 对象创建 | `SemanticWalkerCreationTest.cs` | 大量 | ✅ 完成 |
| 元组解构 | `SemanticWalkerTupleTest.cs` | 25+ | ✅ 完成 |
| 边界条件 | `SemanticWalkerBoundaryTest.cs` | 20+ | ✅ 完成 |
| 无效操作 | `SemanticWalkerInvalidTest.cs` | 0 (搁置) | ⏸️ 搁置 |
| 类转换器 | `AstConverterTests.cs` | 10+ | ✅ 完成 |
| 优化器 | `OptimizerTest.cs` | 25+ | ✅ 完成 |

## 2. 测试场景分类

### 2.1 SemanticWalkerPatternTest - 模式匹配

#### 常量模式测试
- [x] `Visit_IsPattern_Constant` - 整数常量匹配
- [x] `Visit_IsPattern_ConstantString` - 字符串常量匹配
- [x] `Visit_IsPattern_ConstantNull` - null 常量匹配
- [x] `Visit_IsPattern_ConstantBoolean` - 布尔常量匹配

#### 类型模式测试
- [x] `Visit_IsPattern_Type` - 简单类型模式
- [x] `Visit_IsPattern_TypeNullable` - 可空类型模式
- [x] `Visit_IsPattern_TypeWithDeclaration` - 带声明的类型模式

#### 属性模式测试
- [x] `Visit_IsPattern_Property` - 简单属性模式
- [x] `Visit_IsPattern_PropertyMultiple` - 多属性模式
- [x] `Visit_IsPattern_PropertyNested` - 嵌套属性模式

#### 关系模式测试
- [x] `Visit_IsPattern_Relational` - 关系运算符模式
- [x] `Visit_IsPattern_RelationalCombined` - 组合关系模式

#### 列表模式测试
- [x] `Visit_IsPattern_List` - 列表模式
- [x] `Visit_IsPattern_ListWithSlice` - 带切片的列表模式

#### 递归模式测试
- [x] `Visit_IsPattern_Recursive` - 递归模式（构造函数解构）

#### 复合模式测试
- [x] `Visit_IsPattern_Not` - 取反模式
- [x] `Visit_IsPattern_And` - 与模式
- [x] `Visit_IsPattern_Or` - 或模式

### 2.2 SemanticWalkerLoopTest - 循环语句

#### ForEach 循环测试
- [x] `Visit_ForEachLoop` - 基本 foreach 循环
- [x] `Visit_ForEachLoop_List` - 使用 List 集合
- [x] `Visit_ForEachLoop_TypedVariable` - 带类型的循环变量
- [x] `Visit_ForEachLoop_StringArray` - 字符串数组

#### For 循环测试
- [x] `Visit_ForLoop_Simple` - 简单 for 循环
- [x] `Visit_ForLoop_NoInit` - 无初始化
- [x] `Visit_ForLoop_NoCondition` - 无条件
- [x] `Visit_ForLoop_NoUpdate` - 无迭代器
- [x] `Visit_ForLoop_CompoundAssignment` - 复合赋值
- [x] `Visit_ForLoop_Decrement` - 递减循环
- [x] `Visit_ForLoop_StepTwo` - 步长为 2
- [x] `Visit_ForLoop_ComplexUpdate` - 复杂更新表达式
- [x] `Visit_ForLoop_Empty` - 完全空的 for 循环

#### While 循环测试
- [x] `Visit_WhileLoop` - 基本 while 循环
- [x] `Visit_WhileLoop_ComplexCondition` - 复杂条件
- [x] `Visit_WhileLoop_VariableCondition` - 变量条件
- [x] `Visit_WhileLoop_OrCondition` - 逻辑或条件

#### Do-While 循环测试
- [x] `Visit_DoWhileLoop_Simple` - 简单 do-while
- [x] `Visit_DoWhileLoop_ComplexCondition` - 复杂条件
- [x] `Visit_DoWhileLoop_Nested` - 嵌套 do-while

#### 循环控制语句测试
- [x] `Visit_Loop_WithBreak` - break 语句
- [x] `Visit_Loop_WithContinue` - continue 语句
- [x] `Visit_Loop_WithReturn` - return 语句

#### 嵌套循环测试
- [x] `Visit_NestedLoops` - 嵌套 for 循环
- [x] `Visit_NestedLoops_WithBreak` - 嵌套循环中的 break
- [x] `Visit_NestedLoops_WithContinue` - 嵌套循环中的 continue
- [x] `Visit_NestedForEachAndFor` - foreach 和 for 的嵌套

### 2.3 SemanticWalkerSwitchTest - Switch 语句

#### 传统 Switch 测试
- [x] `VisitSwitch_SingleCase` - 单个 case
- [x] `VisitSwitch_MultipleCases` - 多个 case
- [x] `VisitSwitch_WithDefault` - 带 default
- [x] `VisitSwitch_Fallthrough` - fallthrough 行为
- [x] `VisitSwitch_StringLiterals` - 字符串字面量
- [x] `VisitSwitch_BooleanLiterals` - 布尔字面量
- [x] `VisitSwitch_WithStatements` - 带执行语句
- [x] `VisitSwitch_NestedBlock` - 嵌套语句块
- [x] `VisitSwitch_OnlyDefault` - 仅 default

#### 模式匹配 Switch 测试
- [x] `VisitSwitch_PatternMatching_TypePattern` - 类型模式
- [x] `VisitSwitch_PatternMatching_RelationalPattern` - 关系模式
- [x] `VisitSwitch_PatternMatching_Mixed` - 混合模式

#### Case 子句直接测试
- [x] `VisitDefaultCaseClause_ReturnsNull` - default case 子句
- [x] `VisitSingleValueCaseClause_Integer` - 整数 case 子句
- [x] `VisitSingleValueCaseClause_String` - 字符串 case 子句
- [x] `VisitSingleValueCaseClause_Boolean` - 布尔 case 子句

### 2.4 SemanticWalkerStringTest - 字符串插值

#### 简单插值测试
- [x] `Visit_InterpolatedString_Simple` - 简单插值
- [x] `Visit_InterpolatedString_TextOnly` - 仅文本
- [x] `Visit_InterpolatedString_StartsWithExpression` - 以表达式开头
- [x] `Visit_InterpolatedString_EndsWithExpression` - 以表达式结尾

#### 多表达式测试
- [x] `Visit_InterpolatedString_MultipleExpressions` - 多个插值表达式
- [x] `Visit_InterpolatedString_ConsecutiveExpressions` - 连续表达式

#### 复杂表达式测试
- [x] `Visit_InterpolatedString_WithMethodCall` - 方法调用
- [x] `Visit_InterpolatedString_WithArithmetic` - 算术运算
- [x] `Visit_InterpolatedString_WithTernary` - 三元运算符
- [x] `Visit_InterpolatedString_WithPropertyAccess` - 属性访问

#### 转义字符测试
- [x] `Visit_InterpolatedString_WithEscapes` - 转义字符
- [x] `Visit_InterpolatedString_WithTab` - 制表符
- [x] `Visit_InterpolatedString_WithBackslash` - 反斜杠
- [x] `Visit_InterpolatedString_WithNewline` - 换行符
- [x] `Visit_InterpolatedString_WithCarriageReturn` - 回车符

#### 复杂场景测试
- [x] `Visit_InterpolatedString_Complex` - 复杂混合场景
- [x] `Visit_InterpolatedString_Nested` - 嵌套插值
- [x] `Visit_InterpolatedString_WithConcatenation` - 字符串拼接
- [x] `Visit_InterpolatedString_EmptyText` - 空文本
- [x] `Visit_InterpolatedString_InExpression` - 表达式中使用
- [x] `Visit_InterpolatedString_Multiple` - 多个插值字符串组合

### 2.5 SemanticWalkerTryCatchTest - 异常处理

#### Try-Catch 基础测试
- [x] `VisitTry_SingleCatch` - 单个 catch
- [x] `VisitTry_WithFinally` - 带 finally
- [x] `VisitTry_OnlyFinally` - 仅 finally

#### 多 Catch 子句测试
- [x] `VisitTry_MultipleCatches` - 多个 catch
- [x] `VisitTry_MultipleCatchesWithFinally` - 多个 catch 带 finally

#### Throw 语句测试
- [x] `VisitThrow_WithException` - throw 异常
- [x] `VisitThrow_StringLiteral` - 字符串字面量
- [x] `VisitTry_WithThrowInBody` - try 块中的 throw
- [x] `VisitTry_WithThrowInCatch` - catch 块中的 throw

#### 嵌套 Try-Catch 测试
- [x] `VisitTry_NestedTryCatch` - 嵌套 try-catch

#### CatchClause 单独测试
- [x] `VisitCatchClause_Single` - 单个 catch 子句

#### 边界情况测试
- [x] `VisitTry_EmptyBody` - 空 try 块
- [x] `VisitTry_EmptyCatch` - 空 catch 块
- [x] `VisitTry_EmptyFinally` - 空 finally 块
- [x] `VisitTry_UseExceptionVariable` - 使用异常变量

#### Catch When 测试
- [x] `VisitCatchClause_WithWhenClause` - catch when 子句
- [x] `VisitCatchClause_WithWhenClause_SimpleCondition` - 简单条件
- [x] `VisitCatchClause_WithWhenClause_LogicalAndCondition` - 逻辑与条件
- [x] `VisitCatchClause_WithWhenClause_NoExceptionVariable` - 无异常变量

### 2.6 SemanticWalkerDeclarationTest - 变量声明

#### 基本声明测试
- [x] `Visit_ArrayInitializer` - 数组初始化
- [x] `Visit_VariableInitializer` - 变量初始化
- [x] `Visit_VariableDeclarator` - 变量声明符
- [x] `Visit_VariableDeclaration` - 变量声明
- [x] `Visit_VariableDeclarationGroup` - 变量声明组

#### 特殊声明测试
- [x] `Visit_DeclarationExpression_OutVar` - out var 声明
- [x] `Visit_FieldInitializer` - 字段初始化
- [x] `Visit_MixedDeclarationTypes` - 混合声明类型

#### 直接方法测试
- [x] `DirectVisit_ArrayInitializer`
- [x] `DirectVisit_VariableInitializer`
- [x] `DirectVisit_VariableDeclarator`
- [x] `DirectVisit_VariableDeclaration`
- [x] `DirectVisit_VariableDeclarationGroup`
- [x] `DirectVisit_DeclarationExpression_OutVar`
- [x] `DirectVisit_MethodReference`

### 2.7 SemanticWalkerOrdinaryTest - 普通运算

#### 二元运算测试
- [x] 算术运算 (+, -, *, /, %)
- [x] 比较运算 (==, !=, <, >, <=, >=)
- [x] 逻辑运算 (&&, ||)
- [x] 位运算 (&, |, ^, <<, >>, >>>)

#### 一元运算测试
- [x] `Visit_Unary_Negation` - 负号
- [x] `Visit_Unary_LogicalNot` - 逻辑非
- [x] `Visit_Unary_BitwiseNot` - 位非
- [x] `Visit_Unary_Increment` - 自增
- [x] `Visit_Unary_Decrement` - 自减

#### 条件表达式测试
- [x] `Visit_Conditional_Simple` - 简单条件
- [x] `Visit_Conditional_Nested` - 嵌套条件

#### 赋值表达式测试
- [x] `Visit_SimpleAssignment` - 简单赋值
- [x] `Visit_CompoundAssignment` - 复合赋值

#### Null 合并运算测试
- [x] `Visit_NullCoalescing` - ?? 运算符

### 2.8 SemanticWalkerReferenceTest - 引用测试

#### 局部变量引用测试
- [x] `Visit_LocalReference_Simple`
- [x] `Visit_LocalReference_Multiple`

#### 参数引用测试
- [x] `Visit_ParameterReference_Simple`
- [x] `Visit_ParameterReference_Multiple`

#### 字段引用测试
- [x] `Visit_FieldReference_StaticPositiveInfinity`
- [x] `Visit_FieldReference_StaticNegativeInfinity`
- [x] `Visit_FieldReference_StaticNaN`
- [x] `Visit_FieldReference_StaticEpsilon`
- [x] `Visit_FieldReference_StaticMaxValue`
- [x] `Visit_FieldReference_StaticMinValue`
- [x] `Visit_FieldReference_LongMaxValue`
- [x] `Visit_FieldReference_LongMinValue`
- [x] `Visit_FieldReference_InstanceField`

#### 属性引用测试
- [x] `Visit_PropertyReference_InstanceProperty`
- [x] `Visit_PropertyReference_StaticProperty`
- [x] `Visit_PropertyReference_Chained`

#### 方法引用测试
- [x] `Visit_MethodReference_StaticMethod`
- [x] `Visit_MethodReference_InstanceMethod`

#### 实例引用测试
- [x] `Visit_InstanceReference_This`
- [x] `Visit_InstanceReference_ThisMethodCall`

#### 数组元素访问测试
- [x] `Visit_ArrayElementReference_SimpleIndex`
- [x] `Visit_ArrayElementReference_VariableIndex`
- [x] `Visit_ArrayElementReference_FromEnd`
- [x] `Visit_ArrayElementReference_FromEndVariable`
- [x] `Visit_ArrayElementReference_RangeComplete`
- [x] `Visit_ArrayElementReference_RangeFromStart`
- [x] `Visit_ArrayElementReference_RangeToEnd`
- [x] `Visit_ArrayElementReference_RangeAll`
- [x] `Visit_ArrayElementReference_RangeFromEnd`
- [x] `Visit_ArrayElementReference_ExpressionIndex`

### 2.9 SemanticWalkerTupleTest - 元组测试

#### 元组创建测试
- [x] `Visit_TupleBlockCode`
- [x] `VisitTuple_MultipleNamedElements`
- [x] `VisitTuple_MixedNamedAndUnnamed`
- [x] `VisitTuple_NestedTuples`
- [x] `VisitTuple_ComplexTypes`
- [x] `VisitTuple_ExpressionElements`
- [x] `VisitTuple_MethodCallElements`
- [x] `VisitTuple_LongTupleMoreThanSevenElements`

#### 解构赋值测试
- [x] `VisitDeconstructionAssignment_WithTupleRefrence`
- [x] `VisitDeconstructionAssignment_WithExistingVariables`
- [x] `VisitDeconstructionAssignment_MixedDeclaration`
- [x] `VisitDeconstructionAssignment_NestedTuple`
- [x] `VisitDeconstructionAssignment_MethodCall`
- [x] `VisitDeconstructionAssignment_WithDiscard`
- [x] `VisitDeconstructionAssignment_DeconstructMethod`
- [x] `VisitDeconstructionAssignment_DeconstructMethodNestedTuple`
- [x] `VisitDeconstructionAssignment_ConversionOperand`

#### 元组二元运算测试
- [x] `VisitTupleBinaryOperator_Equals`
- [x] `VisitTupleBinaryOperator_NotEquals`
- [x] `VisitTupleBinaryOperator_NamedElements`
- [x] `VisitTupleBinaryOperator_SimpleAssignmentEquals`
- [x] `VisitTupleBinaryOperator_SimpleAssignmentNotEquals`
- [x] `VisitTupleBinaryOperator_SimpleAssignmentNestedEquals`
- [x] `VisitTupleBinaryOperator_SimpleAssignmentNestedNotEquals`
- [x] `VisitTupleBinaryOperator_ThreeElements`
- [x] `VisitTupleBinaryOperator_NestedElements`
- [x] `VisitTupleBinaryOperator_WithInvocationOperand`
- [x] `VisitTupleBinaryOperator_InvocationBothSides`
- [x] `VisitTupleBinaryOperator_Conversion`

#### 丢弃模式测试
- [x] `VisitDiscardOperation_InDeconstruction`
- [x] `VisitDiscardOperation_SimpleAssignment`

### 2.10 SemanticWalkerBoundaryTest - 边界条件测试

#### 位运算符边界测试
- [x] `BitwiseOp_WithZero`
- [x] `BitwiseOp_WithAllOnes`
- [x] `BitwiseOp_WithNegativeNumbers`
- [x] `ShiftOp_ByZero`
- [x] `ShiftOp_MaxBits`

#### 数值边界测试
- [x] `NumericBoundary_IntMaxValue`
- [x] `NumericBoundary_IntMinValue`
- [x] `NumericBoundary_DoubleMaxValue`
- [x] `NumericBoundary_NaN`
- [x] `NumericBoundary_PositiveInfinity`
- [x] `NumericBoundary_NegativeInfinity`
- [x] `NumericBoundary_DivideByZero_Double`

#### 深度嵌套测试
- [x] `NestedObjectCreation_DeepNesting`
- [x] `NestedPropertyAccess_DeepChain`
- [x] `NestedTernary_DeepNesting`
- [x] `NestedArray_DeepNesting`

#### 空值和默认值测试
- [x] `NullCoalescing_LongChain`
- [x] `DefaultValue_ComplexType`
- [x] `EmptyArray_Creation`
- [x] `EmptyString_Variants`

#### 复杂表达式边界测试
- [x] `ComplexLogicalExpression_MixedOperators`
- [x] `ComplexArithmeticExpression_MixedOperators`
- [x] `NestedLambda_DeepNesting`

#### 循环边界测试
- [x] `Loop_EmptyBody`
- [x] `Loop_DeepNesting`
- [x] `Foreach_EmptyCollection`
- [x] `Loop_LargeIteration`

### 2.11 AstConverterTests - 类转换器测试

- [x] `Convert_SimplePublicClass_ReturnsModule`
- [x] `Convert_NonPublicClass_ThrowsNotSupportedException`
- [x] `Convert_ClassWithStaticField_GeneratesVariableDeclaration`
- [x] `Convert_ClassWithConstField_GeneratesConstDeclaration`
- [x] `Convert_ClassWithPrivateField_DoesNotExport`
- [x] `Convert_ClassWithMethod_GeneratesFunctionDeclaration`
- [x] `Convert_ClassWithProperty_GeneratesPropertyMethods`
- [x] `Convert_EmptyClass_ReturnsNull`
- [x] `Convert_ClassWithEnum_GeneratesEnumObject`
- [x] `Convert_ClassWithNestedClass_GeneratesClassDeclaration`

### 2.12 OptimizerTest - 优化器测试

#### 基本去重测试
- [x] `OptimizeLogical_SimpleAndDuplicate_ReturnsSingleOperand`
- [x] `OptimizeLogical_SimpleOrDuplicate_ReturnsSingleOperand`
- [x] `OptimizeLogical_NoDuplicate_ReturnsSameStructure`

#### 多操作数去重测试
- [x] `OptimizeLogical_ThreeOperandsWithDuplicate_RemovesDuplicate`
- [x] `OptimizeLogical_MultipleDuplicates_ReturnsSingleOperand`
- [x] `OptimizeLogical_FourOperandsWithDuplicate_RemovesDuplicate`

#### 不同运算符混合测试
- [x] `OptimizeLogical_IdenticalSubExprWithDifferentOps_Deduplicated`
- [x] `OptimizeLogical_DifferentSubExprWithDifferentOps_PreservesOrStructure`
- [x] `OptimizeLogical_NestedDifferentOperators_PreservesStructure`

#### 嵌套表达式测试
- [x] `OptimizeLogical_ComplexNestedExpression_OptimizesCorrectly`

#### 边界条件测试
- [x] `OptimizeLogical_NonLogicalExpression_ReturnsSame`
- [x] `OptimizeLiteral_BooleanDuplicate_ReturnsSingleLiteral`
- [x] `OptimizeLogical_StringLiteralDuplicate_ReturnsSingleLiteral`

#### 递归深度测试
- [x] `OptimizeLogical_DeepNestedExpression_NoStackOverflow`

#### Nullish Coalescing 测试
- [x] `OptimizeLogical_NullishCoalescingDuplicate_ReturnsSingleOperand`

#### 副作用检测测试
- [x] `OptimizeLogical_FunctionCallDuplicate_PreservesBothCalls`
- [x] `OptimizeLogical_NewExpressionDuplicate_PreservesBoth`
- [x] `OptimizeLogical_AssignmentDuplicate_PreservesBoth`
- [x] `OptimizeLogical_UpdateExpressionDuplicate_PreservesBoth`
- [x] `OptimizeLogical_MixedSideEffect_PreservesStructure`
- [x] `OptimizeLogical_PureExpression_DeduplicatesCorrectly`
- [x] `OptimizeLogical_MemberAccessWithSideEffect_PreservesStructure`

#### 顺序保持测试
- [x] `OptimizeLogical_PreservesOperandOrder_Basic`
- [x] `OptimizeLogical_PreservesOperandOrder_ComplexPatternMatch`
- [x] `OptimizeLogical_PreservesOperandOrder_DuplicateInMiddle`
- [x] `OptimizeLogical_PreservesOperandOrder_DuplicateAtEnd`
- [x] `OptimizeLogical_PreservesOperandOrder_MultipleDuplicates`
- [x] `OptimizeLogical_PreservesOperandOrder_OrOperator`
- [x] `OptimizeLogical_MixedOrAndAnd_DeduplicatesAndSide`

## 3. 待办任务

### 3.1 测试增强任务

| 优先级 | 任务描述 | 状态 |
|--------|---------|------|
| 高 | 增加 async/await 转换测试 | 待添加 |
| 高 | 增加匿名函数/lambda 转换测试 | 待添加 |
| 中 | 增加 default 表达式测试 | 待添加 |
| 中 | 增加 nameof 表达式测试 | 待添加 |
| 中 | 增加 sizeof 表达式测试（如支持） | 待评估 |
| 低 | 增加 checked/unchecked 测试 | 待评估 |

### 3.2 测试质量改进任务

| 优先级 | 任务描述 | 状态 |
|--------|---------|------|
| 高 | 添加更多边界条件测试 | 进行中 |
| 中 | 统一测试命名规范 | 已完成 |
| 中 | 添加测试用例分类标签 | 待添加 |
| 低 | 添加性能基准测试 | 待评估 |

### 3.3 文档任务

| 优先级 | 任务描述 | 状态 |
|--------|---------|------|
| 高 | 更新测试规则文档 | 已完成 |
| 中 | 添加测试覆盖率报告 | 待添加 |
| 低 | 编写测试最佳实践指南 | 待添加 |

## 4. 测试执行命令

```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test src/Jazor.CompilerTest

# 运行单个测试类
dotnet test --filter "SemanticWalkerPatternTest"

# 运行单个测试方法
dotnet test --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"

# 生成测试覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

---

**文档版本**: v1.0
**最后更新**: 2026-03-04
