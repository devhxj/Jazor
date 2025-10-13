namespace ECMAScript;

/// <summary>
/// MLContextOptions
/// </summary>
[ECMAScript]
[Description("@#MLContextOptions")]
public record MLContextOptions(
    [property: Description("@#powerPreference")]MLPowerPreference PowerPreference = MLPowerPreference.Default);

/// <summary>
/// MLContextLostInfo
/// </summary>
[ECMAScript]
[Description("@#MLContextLostInfo")]
public record MLContextLostInfo(
    [property: Description("@#message")]string? Message = default);

/// <summary>
/// MLOpSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLOpSupportLimits")]
public record MLOpSupportLimits(
    [property: Description("@#preferredInputLayout")]MLInputOperandLayout? PreferredInputLayout = default,
	[property: Description("@#maxTensorByteLength")]ulong MaxTensorByteLength = default,
	[property: Description("@#input")]MLDataTypeLimits? Input = default,
	[property: Description("@#constant")]MLDataTypeLimits? Constant = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default,
	[property: Description("@#argMin")]MLSingleInputSupportLimits? ArgMin = default,
	[property: Description("@#argMax")]MLSingleInputSupportLimits? ArgMax = default,
	[property: Description("@#batchNormalization")]MLBatchNormalizationSupportLimits? BatchNormalization = default,
	[property: Description("@#cast")]MLSingleInputSupportLimits? Cast = default,
	[property: Description("@#clamp")]MLSingleInputSupportLimits? Clamp = default,
	[property: Description("@#concat")]MLConcatSupportLimits? Concat = default,
	[property: Description("@#conv2d")]MLConv2dSupportLimits? Conv2d = default,
	[property: Description("@#convTranspose2d")]MLConv2dSupportLimits? ConvTranspose2d = default,
	[property: Description("@#cumulativeSum")]MLSingleInputSupportLimits? CumulativeSum = default,
	[property: Description("@#add")]MLBinarySupportLimits? Add = default,
	[property: Description("@#sub")]MLBinarySupportLimits? Sub = default,
	[property: Description("@#mul")]MLBinarySupportLimits? Mul = default,
	[property: Description("@#div")]MLBinarySupportLimits? Div = default,
	[property: Description("@#max")]MLBinarySupportLimits? Max = default,
	[property: Description("@#min")]MLBinarySupportLimits? Min = default,
	[property: Description("@#pow")]MLBinarySupportLimits? Pow = default,
	[property: Description("@#equal")]MLBinarySupportLimits? Equal = default,
	[property: Description("@#notEqual")]MLBinarySupportLimits? NotEqual = default,
	[property: Description("@#greater")]MLBinarySupportLimits? Greater = default,
	[property: Description("@#greaterOrEqual")]MLBinarySupportLimits? GreaterOrEqual = default,
	[property: Description("@#lesser")]MLBinarySupportLimits? Lesser = default,
	[property: Description("@#lesserOrEqual")]MLBinarySupportLimits? LesserOrEqual = default,
	[property: Description("@#logicalNot")]MLLogicalNotSupportLimits? LogicalNot = default,
	[property: Description("@#logicalAnd")]MLBinarySupportLimits? LogicalAnd = default,
	[property: Description("@#logicalOr")]MLBinarySupportLimits? LogicalOr = default,
	[property: Description("@#logicalXor")]MLBinarySupportLimits? LogicalXor = default,
	[property: Description("@#abs")]MLSingleInputSupportLimits? Abs = default,
	[property: Description("@#ceil")]MLSingleInputSupportLimits? Ceil = default,
	[property: Description("@#cos")]MLSingleInputSupportLimits? Cos = default,
	[property: Description("@#erf")]MLSingleInputSupportLimits? Erf = default,
	[property: Description("@#exp")]MLSingleInputSupportLimits? Exp = default,
	[property: Description("@#floor")]MLSingleInputSupportLimits? Floor = default,
	[property: Description("@#identity")]MLSingleInputSupportLimits? Identity = default,
	[property: Description("@#log")]MLSingleInputSupportLimits? Log = default,
	[property: Description("@#neg")]MLSingleInputSupportLimits? Neg = default,
	[property: Description("@#reciprocal")]MLSingleInputSupportLimits? Reciprocal = default,
	[property: Description("@#sin")]MLSingleInputSupportLimits? Sin = default,
	[property: Description("@#sign")]MLSingleInputSupportLimits? Sign = default,
	[property: Description("@#sqrt")]MLSingleInputSupportLimits? Sqrt = default,
	[property: Description("@#tan")]MLSingleInputSupportLimits? Tan = default,
	[property: Description("@#dequantizeLinear")]MLQuantizeDequantizeLinearSupportLimits? DequantizeLinear = default,
	[property: Description("@#quantizeLinear")]MLQuantizeDequantizeLinearSupportLimits? QuantizeLinear = default,
	[property: Description("@#elu")]MLSingleInputSupportLimits? Elu = default,
	[property: Description("@#expand")]MLSingleInputSupportLimits? Expand = default,
	[property: Description("@#gather")]MLGatherSupportLimits? Gather = default,
	[property: Description("@#gatherElements")]MLGatherSupportLimits? GatherElements = default,
	[property: Description("@#gatherND")]MLGatherSupportLimits? GatherND = default,
	[property: Description("@#gelu")]MLSingleInputSupportLimits? Gelu = default,
	[property: Description("@#gemm")]MLGemmSupportLimits? Gemm = default,
	[property: Description("@#gru")]MLGruSupportLimits? Gru = default,
	[property: Description("@#gruCell")]MLGruCellSupportLimits? GruCell = default,
	[property: Description("@#hardSigmoid")]MLSingleInputSupportLimits? HardSigmoid = default,
	[property: Description("@#hardSwish")]MLSingleInputSupportLimits? HardSwish = default,
	[property: Description("@#instanceNormalization")]MLNormalizationSupportLimits? InstanceNormalization = default,
	[property: Description("@#layerNormalization")]MLNormalizationSupportLimits? LayerNormalization = default,
	[property: Description("@#leakyRelu")]MLSingleInputSupportLimits? LeakyRelu = default,
	[property: Description("@#linear")]MLSingleInputSupportLimits? Linear = default,
	[property: Description("@#lstm")]MLLstmSupportLimits? Lstm = default,
	[property: Description("@#lstmCell")]MLLstmCellSupportLimits? LstmCell = default,
	[property: Description("@#matmul")]MLBinarySupportLimits? Matmul = default,
	[property: Description("@#pad")]MLSingleInputSupportLimits? Pad = default,
	[property: Description("@#averagePool2d")]MLSingleInputSupportLimits? AveragePool2d = default,
	[property: Description("@#l2Pool2d")]MLSingleInputSupportLimits? L2Pool2d = default,
	[property: Description("@#maxPool2d")]MLSingleInputSupportLimits? MaxPool2d = default,
	[property: Description("@#prelu")]MLPreluSupportLimits? Prelu = default,
	[property: Description("@#reduceL1")]MLSingleInputSupportLimits? ReduceL1 = default,
	[property: Description("@#reduceL2")]MLSingleInputSupportLimits? ReduceL2 = default,
	[property: Description("@#reduceLogSum")]MLSingleInputSupportLimits? ReduceLogSum = default,
	[property: Description("@#reduceLogSumExp")]MLSingleInputSupportLimits? ReduceLogSumExp = default,
	[property: Description("@#reduceMax")]MLSingleInputSupportLimits? ReduceMax = default,
	[property: Description("@#reduceMean")]MLSingleInputSupportLimits? ReduceMean = default,
	[property: Description("@#reduceMin")]MLSingleInputSupportLimits? ReduceMin = default,
	[property: Description("@#reduceProduct")]MLSingleInputSupportLimits? ReduceProduct = default,
	[property: Description("@#reduceSum")]MLSingleInputSupportLimits? ReduceSum = default,
	[property: Description("@#reduceSumSquare")]MLSingleInputSupportLimits? ReduceSumSquare = default,
	[property: Description("@#relu")]MLSingleInputSupportLimits? Relu = default,
	[property: Description("@#resample2d")]MLSingleInputSupportLimits? Resample2d = default,
	[property: Description("@#reshape")]MLSingleInputSupportLimits? Reshape = default,
	[property: Description("@#reverse")]MLSingleInputSupportLimits? Reverse = default,
	[property: Description("@#scatterElements")]MLScatterSupportLimits? ScatterElements = default,
	[property: Description("@#scatterND")]MLScatterSupportLimits? ScatterND = default,
	[property: Description("@#sigmoid")]MLSingleInputSupportLimits? Sigmoid = default,
	[property: Description("@#slice")]MLSingleInputSupportLimits? Slice = default,
	[property: Description("@#softmax")]MLSingleInputSupportLimits? Softmax = default,
	[property: Description("@#softplus")]MLSingleInputSupportLimits? Softplus = default,
	[property: Description("@#softsign")]MLSingleInputSupportLimits? Softsign = default,
	[property: Description("@#split")]MLSplitSupportLimits? Split = default,
	[property: Description("@#tanh")]MLSingleInputSupportLimits? Tanh = default,
	[property: Description("@#tile")]MLSingleInputSupportLimits? Tile = default,
	[property: Description("@#transpose")]MLSingleInputSupportLimits? Transpose = default,
	[property: Description("@#triangular")]MLSingleInputSupportLimits? Triangular = default,
	[property: Description("@#where")]MLWhereSupportLimits? Where = default)
{
    [Category("optional")]
    public extern static MLOpSupportLimits OptionalPreferredInputLayoutMaxTensorByteLengthInput5(
        [Description("@#preferredInputLayout")]MLInputOperandLayout? PreferredInputLayout = default,
        [Description("@#maxTensorByteLength")]ulong MaxTensorByteLength = default,
        [Description("@#input")]MLDataTypeLimits? Input = default,
        [Description("@#constant")]MLDataTypeLimits? Constant = default,
        [Description("@#output")]MLDataTypeLimits? Output = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalArgMinArgMax(
        [Description("@#argMin")]MLSingleInputSupportLimits? ArgMin = default,
        [Description("@#argMax")]MLSingleInputSupportLimits? ArgMax = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalBatchNormalization(
        [Description("@#batchNormalization")]MLBatchNormalizationSupportLimits? BatchNormalization = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalCast(
        [Description("@#cast")]MLSingleInputSupportLimits? Cast = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalClamp(
        [Description("@#clamp")]MLSingleInputSupportLimits? Clamp = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalConcat(
        [Description("@#concat")]MLConcatSupportLimits? Concat = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalConv2d(
        [Description("@#conv2d")]MLConv2dSupportLimits? Conv2d = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalConvTranspose2d(
        [Description("@#convTranspose2d")]MLConv2dSupportLimits? ConvTranspose2d = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalCumulativeSum(
        [Description("@#cumulativeSum")]MLSingleInputSupportLimits? CumulativeSum = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalAddSubMul7(
        [Description("@#add")]MLBinarySupportLimits? Add = default,
        [Description("@#sub")]MLBinarySupportLimits? Sub = default,
        [Description("@#mul")]MLBinarySupportLimits? Mul = default,
        [Description("@#div")]MLBinarySupportLimits? Div = default,
        [Description("@#max")]MLBinarySupportLimits? Max = default,
        [Description("@#min")]MLBinarySupportLimits? Min = default,
        [Description("@#pow")]MLBinarySupportLimits? Pow = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalEqualNotEqualGreater10(
        [Description("@#equal")]MLBinarySupportLimits? Equal = default,
        [Description("@#notEqual")]MLBinarySupportLimits? NotEqual = default,
        [Description("@#greater")]MLBinarySupportLimits? Greater = default,
        [Description("@#greaterOrEqual")]MLBinarySupportLimits? GreaterOrEqual = default,
        [Description("@#lesser")]MLBinarySupportLimits? Lesser = default,
        [Description("@#lesserOrEqual")]MLBinarySupportLimits? LesserOrEqual = default,
        [Description("@#logicalNot")]MLLogicalNotSupportLimits? LogicalNot = default,
        [Description("@#logicalAnd")]MLBinarySupportLimits? LogicalAnd = default,
        [Description("@#logicalOr")]MLBinarySupportLimits? LogicalOr = default,
        [Description("@#logicalXor")]MLBinarySupportLimits? LogicalXor = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalAbsCeilCos14(
        [Description("@#abs")]MLSingleInputSupportLimits? Abs = default,
        [Description("@#ceil")]MLSingleInputSupportLimits? Ceil = default,
        [Description("@#cos")]MLSingleInputSupportLimits? Cos = default,
        [Description("@#erf")]MLSingleInputSupportLimits? Erf = default,
        [Description("@#exp")]MLSingleInputSupportLimits? Exp = default,
        [Description("@#floor")]MLSingleInputSupportLimits? Floor = default,
        [Description("@#identity")]MLSingleInputSupportLimits? Identity = default,
        [Description("@#log")]MLSingleInputSupportLimits? Log = default,
        [Description("@#neg")]MLSingleInputSupportLimits? Neg = default,
        [Description("@#reciprocal")]MLSingleInputSupportLimits? Reciprocal = default,
        [Description("@#sin")]MLSingleInputSupportLimits? Sin = default,
        [Description("@#sign")]MLSingleInputSupportLimits? Sign = default,
        [Description("@#sqrt")]MLSingleInputSupportLimits? Sqrt = default,
        [Description("@#tan")]MLSingleInputSupportLimits? Tan = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalDequantizeLinear(
        [Description("@#dequantizeLinear")]MLQuantizeDequantizeLinearSupportLimits? DequantizeLinear = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalQuantizeLinear(
        [Description("@#quantizeLinear")]MLQuantizeDequantizeLinearSupportLimits? QuantizeLinear = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalElu(
        [Description("@#elu")]MLSingleInputSupportLimits? Elu = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalExpand(
        [Description("@#expand")]MLSingleInputSupportLimits? Expand = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalGather(
        [Description("@#gather")]MLGatherSupportLimits? Gather = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalGatherElements(
        [Description("@#gatherElements")]MLGatherSupportLimits? GatherElements = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalGatherND(
        [Description("@#gatherND")]MLGatherSupportLimits? GatherND = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalGelu(
        [Description("@#gelu")]MLSingleInputSupportLimits? Gelu = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalGemm(
        [Description("@#gemm")]MLGemmSupportLimits? Gemm = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalGru(
        [Description("@#gru")]MLGruSupportLimits? Gru = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalGruCell(
        [Description("@#gruCell")]MLGruCellSupportLimits? GruCell = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalHardSigmoid(
        [Description("@#hardSigmoid")]MLSingleInputSupportLimits? HardSigmoid = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalHardSwish(
        [Description("@#hardSwish")]MLSingleInputSupportLimits? HardSwish = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalInstanceNormalization(
        [Description("@#instanceNormalization")]MLNormalizationSupportLimits? InstanceNormalization = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalLayerNormalization(
        [Description("@#layerNormalization")]MLNormalizationSupportLimits? LayerNormalization = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalLeakyRelu(
        [Description("@#leakyRelu")]MLSingleInputSupportLimits? LeakyRelu = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalLinear(
        [Description("@#linear")]MLSingleInputSupportLimits? Linear = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalLstm(
        [Description("@#lstm")]MLLstmSupportLimits? Lstm = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalLstmCell(
        [Description("@#lstmCell")]MLLstmCellSupportLimits? LstmCell = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalMatmul(
        [Description("@#matmul")]MLBinarySupportLimits? Matmul = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalPad(
        [Description("@#pad")]MLSingleInputSupportLimits? Pad = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalAveragePool2dL2Pool2dMaxPool2d(
        [Description("@#averagePool2d")]MLSingleInputSupportLimits? AveragePool2d = default,
        [Description("@#l2Pool2d")]MLSingleInputSupportLimits? L2Pool2d = default,
        [Description("@#maxPool2d")]MLSingleInputSupportLimits? MaxPool2d = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalPrelu(
        [Description("@#prelu")]MLPreluSupportLimits? Prelu = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalReduceL1ReduceL2ReduceLogSum10(
        [Description("@#reduceL1")]MLSingleInputSupportLimits? ReduceL1 = default,
        [Description("@#reduceL2")]MLSingleInputSupportLimits? ReduceL2 = default,
        [Description("@#reduceLogSum")]MLSingleInputSupportLimits? ReduceLogSum = default,
        [Description("@#reduceLogSumExp")]MLSingleInputSupportLimits? ReduceLogSumExp = default,
        [Description("@#reduceMax")]MLSingleInputSupportLimits? ReduceMax = default,
        [Description("@#reduceMean")]MLSingleInputSupportLimits? ReduceMean = default,
        [Description("@#reduceMin")]MLSingleInputSupportLimits? ReduceMin = default,
        [Description("@#reduceProduct")]MLSingleInputSupportLimits? ReduceProduct = default,
        [Description("@#reduceSum")]MLSingleInputSupportLimits? ReduceSum = default,
        [Description("@#reduceSumSquare")]MLSingleInputSupportLimits? ReduceSumSquare = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalRelu(
        [Description("@#relu")]MLSingleInputSupportLimits? Relu = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalResample2d(
        [Description("@#resample2d")]MLSingleInputSupportLimits? Resample2d = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalReshape(
        [Description("@#reshape")]MLSingleInputSupportLimits? Reshape = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalReverse(
        [Description("@#reverse")]MLSingleInputSupportLimits? Reverse = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalScatterElements(
        [Description("@#scatterElements")]MLScatterSupportLimits? ScatterElements = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalScatterND(
        [Description("@#scatterND")]MLScatterSupportLimits? ScatterND = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalSigmoid(
        [Description("@#sigmoid")]MLSingleInputSupportLimits? Sigmoid = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalSlice(
        [Description("@#slice")]MLSingleInputSupportLimits? Slice = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalSoftmax(
        [Description("@#softmax")]MLSingleInputSupportLimits? Softmax = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalSoftplus(
        [Description("@#softplus")]MLSingleInputSupportLimits? Softplus = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalSoftsign(
        [Description("@#softsign")]MLSingleInputSupportLimits? Softsign = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalSplit(
        [Description("@#split")]MLSplitSupportLimits? Split = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalTanh(
        [Description("@#tanh")]MLSingleInputSupportLimits? Tanh = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalTile(
        [Description("@#tile")]MLSingleInputSupportLimits? Tile = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalTranspose(
        [Description("@#transpose")]MLSingleInputSupportLimits? Transpose = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalTriangular(
        [Description("@#triangular")]MLSingleInputSupportLimits? Triangular = default);

    [Category("optional")]
    public extern static MLOpSupportLimits OptionalWhere(
        [Description("@#where")]MLWhereSupportLimits? Where = default);
}

/// <summary>
/// MLDataTypeLimits
/// </summary>
[ECMAScript]
[Description("@#MLDataTypeLimits")]
public record MLDataTypeLimits(
    [property: Description("@#dataTypes")]MLDataTypeList? DataTypes = default);

/// <summary>
/// MLRankRange
/// </summary>
[ECMAScript]
[Description("@#MLRankRange")]
public record MLRankRange(
    [property: Description("@#min")]uint Min = default,
	[property: Description("@#max")]uint Max = default);

/// <summary>
/// MLTensorLimits
/// </summary>
[ECMAScript]
[Description("@#MLTensorLimits")]
public record MLTensorLimits(
    [property: Description("@#dataTypes")]MLDataTypeList? DataTypes = default,
	[property: Description("@#rankRange")]MLRankRange? RankRange = default);

/// <summary>
/// MLBinarySupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLBinarySupportLimits")]
public record MLBinarySupportLimits(
    [property: Description("@#a")]MLTensorLimits? A = default,
	[property: Description("@#b")]MLTensorLimits? B = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLSingleInputSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLSingleInputSupportLimits")]
public record MLSingleInputSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLOperandDescriptor
/// </summary>
[ECMAScript]
[Description("@#MLOperandDescriptor")]
public record MLOperandDescriptor(
    [property: Description("@#dataType")]MLOperandDataType? DataType = default,
	[property: Description("@#shape")]uint[]? Shape = default);

/// <summary>
/// MLOperatorOptions
/// </summary>
[ECMAScript]
[Description("@#MLOperatorOptions")]
public record MLOperatorOptions(
    [property: Description("@#label")]string? Label = default);

/// <summary>
/// MLTensorDescriptor
/// </summary>
[ECMAScript]
[Description("@#MLTensorDescriptor")]
public record MLTensorDescriptor(
    [property: Description("@#readable")]bool Readable = false,
	[property: Description("@#writable")]bool Writable = false): MLOperandDescriptor;

/// <summary>
/// MLArgMinMaxOptions
/// </summary>
[ECMAScript]
[Description("@#MLArgMinMaxOptions")]
public record MLArgMinMaxOptions(
    [property: Description("@#keepDimensions")]bool KeepDimensions = false,
	[property: Description("@#outputDataType")]MLOperandDataType OutputDataType = MLOperandDataType.Int32): MLOperatorOptions;

/// <summary>
/// MLBatchNormalizationOptions
/// </summary>
[ECMAScript]
[Description("@#MLBatchNormalizationOptions")]
public record MLBatchNormalizationOptions(
    [property: Description("@#scale")]MLOperand? Scale = default,
	[property: Description("@#bias")]MLOperand? Bias = default,
	[property: Description("@#axis")]uint Axis = 1,
	[property: Description("@#epsilon")]double Epsilon = 1e-5d): MLOperatorOptions;

/// <summary>
/// MLBatchNormalizationSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLBatchNormalizationSupportLimits")]
public record MLBatchNormalizationSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#mean")]MLTensorLimits? Mean = default,
	[property: Description("@#variance")]MLTensorLimits? Variance = default,
	[property: Description("@#scale")]MLTensorLimits? Scale = default,
	[property: Description("@#bias")]MLTensorLimits? Bias = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLClampOptions
/// </summary>
[ECMAScript]
[Description("@#MLClampOptions")]
public record MLClampOptions(
    [property: Description("@#minValue")]MLNumber? MinValue = default,
	[property: Description("@#maxValue")]MLNumber? MaxValue = default): MLOperatorOptions;

/// <summary>
/// MLConcatSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLConcatSupportLimits")]
public record MLConcatSupportLimits(
    [property: Description("@#inputs")]MLTensorLimits? Inputs = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLConv2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLConv2dOptions")]
public record MLConv2dOptions(
    [property: Description("@#padding")]uint[]? Padding = default,
	[property: Description("@#strides")]uint[]? Strides = default,
	[property: Description("@#dilations")]uint[]? Dilations = default,
	[property: Description("@#groups")]uint Groups = 1,
	[property: Description("@#inputLayout")]MLInputOperandLayout InputLayout = MLInputOperandLayout.Nchw,
	[property: Description("@#filterLayout")]MLConv2dFilterOperandLayout FilterLayout = MLConv2dFilterOperandLayout.Oihw,
	[property: Description("@#bias")]MLOperand? Bias = default): MLOperatorOptions;

/// <summary>
/// MLConv2dSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLConv2dSupportLimits")]
public record MLConv2dSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#filter")]MLTensorLimits? Filter = default,
	[property: Description("@#bias")]MLTensorLimits? Bias = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLConvTranspose2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLConvTranspose2dOptions")]
public record MLConvTranspose2dOptions(
    [property: Description("@#padding")]uint[]? Padding = default,
	[property: Description("@#strides")]uint[]? Strides = default,
	[property: Description("@#dilations")]uint[]? Dilations = default,
	[property: Description("@#outputPadding")]uint[]? OutputPadding = default,
	[property: Description("@#outputSizes")]uint[]? OutputSizes = default,
	[property: Description("@#groups")]uint Groups = 1,
	[property: Description("@#inputLayout")]MLInputOperandLayout InputLayout = MLInputOperandLayout.Nchw,
	[property: Description("@#filterLayout")]MLConvTranspose2dFilterOperandLayout FilterLayout = MLConvTranspose2dFilterOperandLayout.Iohw,
	[property: Description("@#bias")]MLOperand? Bias = default): MLOperatorOptions;

/// <summary>
/// MLCumulativeSumOptions
/// </summary>
[ECMAScript]
[Description("@#MLCumulativeSumOptions")]
public record MLCumulativeSumOptions(
    [property: Description("@#exclusive")]bool Exclusive = false,
	[property: Description("@#reversed")]bool Reversed = false): MLOperatorOptions;

/// <summary>
/// MLLogicalNotSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLLogicalNotSupportLimits")]
public record MLLogicalNotSupportLimits(
    [property: Description("@#a")]MLTensorLimits? A = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLQuantizeDequantizeLinearSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLQuantizeDequantizeLinearSupportLimits")]
public record MLQuantizeDequantizeLinearSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#scale")]MLTensorLimits? Scale = default,
	[property: Description("@#zeroPoint")]MLTensorLimits? ZeroPoint = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLEluOptions
/// </summary>
[ECMAScript]
[Description("@#MLEluOptions")]
public record MLEluOptions(
    [property: Description("@#alpha")]double Alpha = 1d): MLOperatorOptions;

/// <summary>
/// MLGatherOptions
/// </summary>
[ECMAScript]
[Description("@#MLGatherOptions")]
public record MLGatherOptions(
    [property: Description("@#axis")]uint Axis = 0): MLOperatorOptions;

/// <summary>
/// MLGatherSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLGatherSupportLimits")]
public record MLGatherSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#indices")]MLTensorLimits? Indices = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLGemmOptions
/// </summary>
[ECMAScript]
[Description("@#MLGemmOptions")]
public record MLGemmOptions(
    [property: Description("@#c")]MLOperand? C = default,
	[property: Description("@#alpha")]double Alpha = 1.0d,
	[property: Description("@#beta")]double Beta = 1.0d,
	[property: Description("@#aTranspose")]bool ATranspose = false,
	[property: Description("@#bTranspose")]bool BTranspose = false): MLOperatorOptions;

/// <summary>
/// MLGemmSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLGemmSupportLimits")]
public record MLGemmSupportLimits(
    [property: Description("@#a")]MLTensorLimits? A = default,
	[property: Description("@#b")]MLTensorLimits? B = default,
	[property: Description("@#c")]MLTensorLimits? C = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLGruOptions
/// </summary>
[ECMAScript]
[Description("@#MLGruOptions")]
public record MLGruOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
	[property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
	[property: Description("@#initialHiddenState")]MLOperand? InitialHiddenState = default,
	[property: Description("@#resetAfter")]bool ResetAfter = true,
	[property: Description("@#returnSequence")]bool ReturnSequence = false,
	[property: Description("@#direction")]MLRecurrentNetworkDirection Direction = MLRecurrentNetworkDirection.Forward,
	[property: Description("@#layout")]MLGruWeightLayout Layout = MLGruWeightLayout.Zrn,
	[property: Description("@#activations")]MLRecurrentNetworkActivation[]? Activations = default): MLOperatorOptions;

/// <summary>
/// MLGruSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLGruSupportLimits")]
public record MLGruSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#weight")]MLTensorLimits? Weight = default,
	[property: Description("@#recurrentWeight")]MLTensorLimits? RecurrentWeight = default,
	[property: Description("@#bias")]MLTensorLimits? Bias = default,
	[property: Description("@#recurrentBias")]MLTensorLimits? RecurrentBias = default,
	[property: Description("@#initialHiddenState")]MLTensorLimits? InitialHiddenState = default,
	[property: Description("@#outputs")]MLDataTypeLimits? Outputs = default);

/// <summary>
/// MLGruCellOptions
/// </summary>
[ECMAScript]
[Description("@#MLGruCellOptions")]
public record MLGruCellOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
	[property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
	[property: Description("@#resetAfter")]bool ResetAfter = true,
	[property: Description("@#layout")]MLGruWeightLayout Layout = MLGruWeightLayout.Zrn,
	[property: Description("@#activations")]MLRecurrentNetworkActivation[]? Activations = default): MLOperatorOptions;

/// <summary>
/// MLGruCellSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLGruCellSupportLimits")]
public record MLGruCellSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#weight")]MLTensorLimits? Weight = default,
	[property: Description("@#recurrentWeight")]MLTensorLimits? RecurrentWeight = default,
	[property: Description("@#hiddenState")]MLTensorLimits? HiddenState = default,
	[property: Description("@#bias")]MLTensorLimits? Bias = default,
	[property: Description("@#recurrentBias")]MLTensorLimits? RecurrentBias = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLHardSigmoidOptions
/// </summary>
[ECMAScript]
[Description("@#MLHardSigmoidOptions")]
public record MLHardSigmoidOptions(
    [property: Description("@#alpha")]double Alpha = 0.2d,
	[property: Description("@#beta")]double Beta = 0.5d): MLOperatorOptions;

/// <summary>
/// MLInstanceNormalizationOptions
/// </summary>
[ECMAScript]
[Description("@#MLInstanceNormalizationOptions")]
public record MLInstanceNormalizationOptions(
    [property: Description("@#scale")]MLOperand? Scale = default,
	[property: Description("@#bias")]MLOperand? Bias = default,
	[property: Description("@#epsilon")]double Epsilon = 1e-5d,
	[property: Description("@#layout")]MLInputOperandLayout Layout = MLInputOperandLayout.Nchw): MLOperatorOptions;

/// <summary>
/// MLNormalizationSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLNormalizationSupportLimits")]
public record MLNormalizationSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#scale")]MLTensorLimits? Scale = default,
	[property: Description("@#bias")]MLTensorLimits? Bias = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLLayerNormalizationOptions
/// </summary>
[ECMAScript]
[Description("@#MLLayerNormalizationOptions")]
public record MLLayerNormalizationOptions(
    [property: Description("@#scale")]MLOperand? Scale = default,
	[property: Description("@#bias")]MLOperand? Bias = default,
	[property: Description("@#axes")]uint[]? Axes = default,
	[property: Description("@#epsilon")]double Epsilon = 1e-5d): MLOperatorOptions;

/// <summary>
/// MLLeakyReluOptions
/// </summary>
[ECMAScript]
[Description("@#MLLeakyReluOptions")]
public record MLLeakyReluOptions(
    [property: Description("@#alpha")]double Alpha = 0.01d): MLOperatorOptions;

/// <summary>
/// MLLinearOptions
/// </summary>
[ECMAScript]
[Description("@#MLLinearOptions")]
public record MLLinearOptions(
    [property: Description("@#alpha")]double Alpha = 1d,
	[property: Description("@#beta")]double Beta = 0d): MLOperatorOptions;

/// <summary>
/// MLLstmOptions
/// </summary>
[ECMAScript]
[Description("@#MLLstmOptions")]
public record MLLstmOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
	[property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
	[property: Description("@#peepholeWeight")]MLOperand? PeepholeWeight = default,
	[property: Description("@#initialHiddenState")]MLOperand? InitialHiddenState = default,
	[property: Description("@#initialCellState")]MLOperand? InitialCellState = default,
	[property: Description("@#returnSequence")]bool ReturnSequence = false,
	[property: Description("@#direction")]MLRecurrentNetworkDirection Direction = MLRecurrentNetworkDirection.Forward,
	[property: Description("@#layout")]MLLstmWeightLayout Layout = MLLstmWeightLayout.Iofg,
	[property: Description("@#activations")]MLRecurrentNetworkActivation[]? Activations = default): MLOperatorOptions;

/// <summary>
/// MLLstmSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLLstmSupportLimits")]
public record MLLstmSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#weight")]MLTensorLimits? Weight = default,
	[property: Description("@#recurrentWeight")]MLTensorLimits? RecurrentWeight = default,
	[property: Description("@#bias")]MLTensorLimits? Bias = default,
	[property: Description("@#recurrentBias")]MLTensorLimits? RecurrentBias = default,
	[property: Description("@#peepholeWeight")]MLTensorLimits? PeepholeWeight = default,
	[property: Description("@#initialHiddenState")]MLTensorLimits? InitialHiddenState = default,
	[property: Description("@#initialCellState")]MLTensorLimits? InitialCellState = default,
	[property: Description("@#outputs")]MLDataTypeLimits? Outputs = default);

/// <summary>
/// MLLstmCellOptions
/// </summary>
[ECMAScript]
[Description("@#MLLstmCellOptions")]
public record MLLstmCellOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
	[property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
	[property: Description("@#peepholeWeight")]MLOperand? PeepholeWeight = default,
	[property: Description("@#layout")]MLLstmWeightLayout Layout = MLLstmWeightLayout.Iofg,
	[property: Description("@#activations")]MLRecurrentNetworkActivation[]? Activations = default): MLOperatorOptions;

/// <summary>
/// MLLstmCellSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLLstmCellSupportLimits")]
public record MLLstmCellSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#weight")]MLTensorLimits? Weight = default,
	[property: Description("@#recurrentWeight")]MLTensorLimits? RecurrentWeight = default,
	[property: Description("@#hiddenState")]MLTensorLimits? HiddenState = default,
	[property: Description("@#cellState")]MLTensorLimits? CellState = default,
	[property: Description("@#bias")]MLTensorLimits? Bias = default,
	[property: Description("@#recurrentBias")]MLTensorLimits? RecurrentBias = default,
	[property: Description("@#peepholeWeight")]MLTensorLimits? PeepholeWeight = default,
	[property: Description("@#outputs")]MLDataTypeLimits? Outputs = default);

/// <summary>
/// MLPadOptions
/// </summary>
[ECMAScript]
[Description("@#MLPadOptions")]
public record MLPadOptions(
    [property: Description("@#mode")]MLPaddingMode Mode = MLPaddingMode.Constant,
	[property: Description("@#value")]MLNumber? Value = default): MLOperatorOptions;

/// <summary>
/// MLPool2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLPool2dOptions")]
public record MLPool2dOptions(
    [property: Description("@#windowDimensions")]uint[]? WindowDimensions = default,
	[property: Description("@#padding")]uint[]? Padding = default,
	[property: Description("@#strides")]uint[]? Strides = default,
	[property: Description("@#dilations")]uint[]? Dilations = default,
	[property: Description("@#layout")]MLInputOperandLayout Layout = MLInputOperandLayout.Nchw,
	[property: Description("@#roundingType")]MLRoundingType RoundingType = MLRoundingType.Floor,
	[property: Description("@#outputSizes")]uint[]? OutputSizes = default): MLOperatorOptions;

/// <summary>
/// MLPreluSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLPreluSupportLimits")]
public record MLPreluSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#slope")]MLTensorLimits? Slope = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLReduceOptions
/// </summary>
[ECMAScript]
[Description("@#MLReduceOptions")]
public record MLReduceOptions(
    [property: Description("@#axes")]uint[]? Axes = default,
	[property: Description("@#keepDimensions")]bool KeepDimensions = false): MLOperatorOptions;

/// <summary>
/// MLResample2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLResample2dOptions")]
public record MLResample2dOptions(
    [property: Description("@#mode")]MLInterpolationMode Mode = MLInterpolationMode.NearestNeighbor,
	[property: Description("@#scales")]float[]? Scales = default,
	[property: Description("@#sizes")]uint[]? Sizes = default,
	[property: Description("@#axes")]uint[]? Axes = default): MLOperatorOptions;

/// <summary>
/// MLReverseOptions
/// </summary>
[ECMAScript]
[Description("@#MLReverseOptions")]
public record MLReverseOptions(
    [property: Description("@#axes")]uint[]? Axes = default): MLOperatorOptions;

/// <summary>
/// MLScatterOptions
/// </summary>
[ECMAScript]
[Description("@#MLScatterOptions")]
public record MLScatterOptions(
    [property: Description("@#axis")]uint Axis = 0): MLOperatorOptions;

/// <summary>
/// MLScatterSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLScatterSupportLimits")]
public record MLScatterSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#indices")]MLTensorLimits? Indices = default,
	[property: Description("@#updates")]MLTensorLimits? Updates = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// MLSliceOptions
/// </summary>
[ECMAScript]
[Description("@#MLSliceOptions")]
public record MLSliceOptions(
    [property: Description("@#strides")]uint[]? Strides = default): MLOperatorOptions;

/// <summary>
/// MLSplitOptions
/// </summary>
[ECMAScript]
[Description("@#MLSplitOptions")]
public record MLSplitOptions(
    [property: Description("@#axis")]uint Axis = 0): MLOperatorOptions;

/// <summary>
/// MLSplitSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLSplitSupportLimits")]
public record MLSplitSupportLimits(
    [property: Description("@#input")]MLTensorLimits? Input = default,
	[property: Description("@#outputs")]MLDataTypeLimits? Outputs = default);

/// <summary>
/// MLTransposeOptions
/// </summary>
[ECMAScript]
[Description("@#MLTransposeOptions")]
public record MLTransposeOptions(
    [property: Description("@#permutation")]uint[]? Permutation = default): MLOperatorOptions;

/// <summary>
/// MLTriangularOptions
/// </summary>
[ECMAScript]
[Description("@#MLTriangularOptions")]
public record MLTriangularOptions(
    [property: Description("@#upper")]bool Upper = true,
	[property: Description("@#diagonal")]int Diagonal = 0): MLOperatorOptions;

/// <summary>
/// MLWhereSupportLimits
/// </summary>
[ECMAScript]
[Description("@#MLWhereSupportLimits")]
public record MLWhereSupportLimits(
    [property: Description("@#condition")]MLTensorLimits? Condition = default,
	[property: Description("@#trueValue")]MLTensorLimits? TrueValue = default,
	[property: Description("@#falseValue")]MLTensorLimits? FalseValue = default,
	[property: Description("@#output")]MLDataTypeLimits? Output = default);

/// <summary>
/// ML
/// </summary>
[ECMAScript]
[Description("@#ML")]
public class ML
{
    /// <summary>
    /// createContext
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#createContext")]
    public extern PromiseResult<MLContext> CreateContext(MLContextOptions? options = default);

    /// <summary>
    /// createContext
    /// </summary>
    /// <param name="gpuDevice">gpuDevice</param>
    [Description("@#createContext")]
    public extern PromiseResult<MLContext> CreateContext(GPUDevice gpuDevice);
}

/// <summary>
/// MLContext
/// </summary>
[ECMAScript]
[Description("@#MLContext")]
public class MLContext
{
    /// <summary>
    /// dispatch
    /// </summary>
    /// <param name="graph">graph</param>
    /// <param name="inputs">inputs</param>
    /// <param name="outputs">outputs</param>
    [Description("@#dispatch")]
    public extern void Dispatch(MLGraph graph, MLNamedTensors inputs, MLNamedTensors outputs);

    /// <summary>
    /// createTensor
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createTensor")]
    public extern PromiseResult<MLTensor> CreateTensor(MLTensorDescriptor descriptor);

    /// <summary>
    /// createConstantTensor
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    /// <param name="inputData">inputData</param>
    [Description("@#createConstantTensor")]
    public extern PromiseResult<MLTensor> CreateConstantTensor(MLOperandDescriptor descriptor, IAllowSharedBufferSource inputData);

    /// <summary>
    /// readTensor
    /// </summary>
    /// <param name="tensor">tensor</param>
    [Description("@#readTensor")]
    public extern PromiseResult<ArrayBuffer> ReadTensor(MLTensor tensor);

    /// <summary>
    /// readTensor
    /// </summary>
    /// <param name="tensor">tensor</param>
    /// <param name="outputData">outputData</param>
    [Description("@#readTensor")]
    public extern PromiseResult<object> ReadTensor(MLTensor tensor, IAllowSharedBufferSource outputData);

    /// <summary>
    /// writeTensor
    /// </summary>
    /// <param name="tensor">tensor</param>
    /// <param name="inputData">inputData</param>
    [Description("@#writeTensor")]
    public extern void WriteTensor(MLTensor tensor, IAllowSharedBufferSource inputData);

    /// <summary>
    /// opSupportLimits
    /// </summary>
    [Description("@#opSupportLimits")]
    public extern MLOpSupportLimits OpSupportLimits();

    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// lost
    /// </summary>
    [Description("@#lost")]
    public extern PromiseResult<MLContextLostInfo> Lost { get; }
}

/// <summary>
/// MLGraph
/// </summary>
[ECMAScript]
[Description("@#MLGraph")]
public class MLGraph
{
    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();
}

/// <summary>
/// MLOperand
/// </summary>
[ECMAScript]
[Description("@#MLOperand")]
public class MLOperand
{
    /// <summary>
    /// dataType
    /// </summary>
    [Description("@#dataType")]
    public extern MLOperandDataType DataType { get; }

    /// <summary>
    /// shape
    /// </summary>
    [Description("@#shape")]
    public extern FrozenSet<uint> Shape { get; }
}

/// <summary>
/// MLTensor
/// </summary>
[ECMAScript]
[Description("@#MLTensor")]
public class MLTensor
{
    /// <summary>
    /// dataType
    /// </summary>
    [Description("@#dataType")]
    public extern MLOperandDataType DataType { get; }

    /// <summary>
    /// shape
    /// </summary>
    [Description("@#shape")]
    public extern FrozenSet<uint> Shape { get; }

    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern bool Readable { get; }

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern bool Writable { get; }

    /// <summary>
    /// constant
    /// </summary>
    [Description("@#constant")]
    public extern bool Constant { get; }

    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();
}

/// <summary>
/// MLGraphBuilder
/// </summary>
[ECMAScript]
[Description("@#MLGraphBuilder")]
public partial class MLGraphBuilder
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    public extern MLGraphBuilder(MLContext context);

    /// <summary>
    /// input
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="descriptor">descriptor</param>
    [Description("@#input")]
    public extern MLOperand Input(string name, MLOperandDescriptor descriptor);

    /// <summary>
    /// constant
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    /// <param name="buffer">buffer</param>
    [Description("@#constant")]
    public extern MLOperand Constant(MLOperandDescriptor descriptor, IAllowSharedBufferSource buffer);

    /// <summary>
    /// constant
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="value">value</param>
    [Description("@#constant")]
    public extern MLOperand Constant(MLOperandDataType type, MLNumber value);

    /// <summary>
    /// constant
    /// </summary>
    /// <param name="tensor">tensor</param>
    [Description("@#constant")]
    public extern MLOperand Constant(MLTensor tensor);

    /// <summary>
    /// build
    /// </summary>
    /// <param name="outputs">outputs</param>
    [Description("@#build")]
    public extern PromiseResult<MLGraph> Build(MLNamedOperands outputs);

    /// <summary>
    /// argMin
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="axis">axis</param>
    /// <param name="options">options</param>
    [Description("@#argMin")]
    public extern MLOperand ArgMin(MLOperand input, uint axis, MLArgMinMaxOptions? options = default);

    /// <summary>
    /// argMax
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="axis">axis</param>
    /// <param name="options">options</param>
    [Description("@#argMax")]
    public extern MLOperand ArgMax(MLOperand input, uint axis, MLArgMinMaxOptions? options = default);

    /// <summary>
    /// batchNormalization
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="mean">mean</param>
    /// <param name="variance">variance</param>
    /// <param name="options">options</param>
    [Description("@#batchNormalization")]
    public extern MLOperand BatchNormalization(MLOperand input, MLOperand mean, MLOperand variance, MLBatchNormalizationOptions? options = default);

    /// <summary>
    /// cast
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="type">type</param>
    /// <param name="options">options</param>
    [Description("@#cast")]
    public extern MLOperand Cast(MLOperand input, MLOperandDataType type, MLOperatorOptions? options = default);

    /// <summary>
    /// clamp
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#clamp")]
    public extern MLOperand Clamp(MLOperand input, MLClampOptions? options = default);

    /// <summary>
    /// concat
    /// </summary>
    /// <param name="inputs">inputs</param>
    /// <param name="axis">axis</param>
    /// <param name="options">options</param>
    [Description("@#concat")]
    public extern MLOperand Concat(MLOperand[] inputs, uint axis, MLOperatorOptions? options = default);

    /// <summary>
    /// conv2d
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="filter">filter</param>
    /// <param name="options">options</param>
    [Description("@#conv2d")]
    public extern MLOperand Conv2d(MLOperand input, MLOperand filter, MLConv2dOptions? options = default);

    /// <summary>
    /// convTranspose2d
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="filter">filter</param>
    /// <param name="options">options</param>
    [Description("@#convTranspose2d")]
    public extern MLOperand ConvTranspose2d(MLOperand input, MLOperand filter, MLConvTranspose2dOptions? options = default);

    /// <summary>
    /// cumulativeSum
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="axis">axis</param>
    /// <param name="options">options</param>
    [Description("@#cumulativeSum")]
    public extern MLOperand CumulativeSum(MLOperand input, uint axis, MLCumulativeSumOptions? options = default);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#add")]
    public extern MLOperand Add(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// sub
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#sub")]
    public extern MLOperand Sub(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// mul
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#mul")]
    public extern MLOperand Mul(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// div
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#div")]
    public extern MLOperand Div(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// max
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#max")]
    public extern MLOperand Max(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// min
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#min")]
    public extern MLOperand Min(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// pow
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#pow")]
    public extern MLOperand Pow(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// equal
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#equal")]
    public extern MLOperand Equal(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// notEqual
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#notEqual")]
    public extern MLOperand NotEqual(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// greater
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#greater")]
    public extern MLOperand Greater(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// greaterOrEqual
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#greaterOrEqual")]
    public extern MLOperand GreaterOrEqual(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// lesser
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#lesser")]
    public extern MLOperand Lesser(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// lesserOrEqual
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#lesserOrEqual")]
    public extern MLOperand LesserOrEqual(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// logicalNot
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="options">options</param>
    [Description("@#logicalNot")]
    public extern MLOperand LogicalNot(MLOperand a, MLOperatorOptions? options = default);

    /// <summary>
    /// logicalAnd
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#logicalAnd")]
    public extern MLOperand LogicalAnd(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// logicalOr
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#logicalOr")]
    public extern MLOperand LogicalOr(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// logicalXor
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#logicalXor")]
    public extern MLOperand LogicalXor(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// abs
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#abs")]
    public extern MLOperand Abs(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// ceil
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#ceil")]
    public extern MLOperand Ceil(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// cos
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#cos")]
    public extern MLOperand Cos(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// erf
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#erf")]
    public extern MLOperand Erf(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// exp
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#exp")]
    public extern MLOperand Exp(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// floor
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#floor")]
    public extern MLOperand Floor(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// identity
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#identity")]
    public extern MLOperand Identity(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// log
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#log")]
    public extern MLOperand Log(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// neg
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#neg")]
    public extern MLOperand Neg(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// reciprocal
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reciprocal")]
    public extern MLOperand Reciprocal(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// sin
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#sin")]
    public extern MLOperand Sin(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// sign
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#sign")]
    public extern MLOperand Sign(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// sqrt
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#sqrt")]
    public extern MLOperand Sqrt(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// tan
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#tan")]
    public extern MLOperand Tan(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// dequantizeLinear
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="scale">scale</param>
    /// <param name="zeroPoint">zeroPoint</param>
    /// <param name="options">options</param>
    [Description("@#dequantizeLinear")]
    public extern MLOperand DequantizeLinear(MLOperand input, MLOperand scale, MLOperand zeroPoint, MLOperatorOptions? options = default);

    /// <summary>
    /// quantizeLinear
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="scale">scale</param>
    /// <param name="zeroPoint">zeroPoint</param>
    /// <param name="options">options</param>
    [Description("@#quantizeLinear")]
    public extern MLOperand QuantizeLinear(MLOperand input, MLOperand scale, MLOperand zeroPoint, MLOperatorOptions? options = default);

    /// <summary>
    /// elu
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#elu")]
    public extern MLOperand Elu(MLOperand input, MLEluOptions? options = default);

    /// <summary>
    /// expand
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="newShape">newShape</param>
    /// <param name="options">options</param>
    [Description("@#expand")]
    public extern MLOperand Expand(MLOperand input, uint[] newShape, MLOperatorOptions? options = default);

    /// <summary>
    /// gather
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="indices">indices</param>
    /// <param name="options">options</param>
    [Description("@#gather")]
    public extern MLOperand Gather(MLOperand input, MLOperand indices, MLGatherOptions? options = default);

    /// <summary>
    /// gatherElements
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="indices">indices</param>
    /// <param name="options">options</param>
    [Description("@#gatherElements")]
    public extern MLOperand GatherElements(MLOperand input, MLOperand indices, MLGatherOptions? options = default);

    /// <summary>
    /// gatherND
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="indices">indices</param>
    /// <param name="options">options</param>
    [Description("@#gatherND")]
    public extern MLOperand GatherND(MLOperand input, MLOperand indices, MLOperatorOptions? options = default);

    /// <summary>
    /// gelu
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#gelu")]
    public extern MLOperand Gelu(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// gemm
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#gemm")]
    public extern MLOperand Gemm(MLOperand a, MLOperand b, MLGemmOptions? options = default);

    /// <summary>
    /// gru
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="weight">weight</param>
    /// <param name="recurrentWeight">recurrentWeight</param>
    /// <param name="steps">steps</param>
    /// <param name="hiddenSize">hiddenSize</param>
    /// <param name="options">options</param>
    [Description("@#gru")]
    public extern MLOperand[] Gru(MLOperand input, MLOperand weight, MLOperand recurrentWeight, uint steps, uint hiddenSize, MLGruOptions? options = default);

    /// <summary>
    /// gruCell
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="weight">weight</param>
    /// <param name="recurrentWeight">recurrentWeight</param>
    /// <param name="hiddenState">hiddenState</param>
    /// <param name="hiddenSize">hiddenSize</param>
    /// <param name="options">options</param>
    [Description("@#gruCell")]
    public extern MLOperand GruCell(MLOperand input, MLOperand weight, MLOperand recurrentWeight, MLOperand hiddenState, uint hiddenSize, MLGruCellOptions? options = default);

    /// <summary>
    /// hardSigmoid
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#hardSigmoid")]
    public extern MLOperand HardSigmoid(MLOperand input, MLHardSigmoidOptions? options = default);

    /// <summary>
    /// hardSwish
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#hardSwish")]
    public extern MLOperand HardSwish(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// instanceNormalization
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#instanceNormalization")]
    public extern MLOperand InstanceNormalization(MLOperand input, MLInstanceNormalizationOptions? options = default);

    /// <summary>
    /// layerNormalization
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#layerNormalization")]
    public extern MLOperand LayerNormalization(MLOperand input, MLLayerNormalizationOptions? options = default);

    /// <summary>
    /// leakyRelu
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#leakyRelu")]
    public extern MLOperand LeakyRelu(MLOperand input, MLLeakyReluOptions? options = default);

    /// <summary>
    /// linear
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#linear")]
    public extern MLOperand Linear(MLOperand input, MLLinearOptions? options = default);

    /// <summary>
    /// lstm
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="weight">weight</param>
    /// <param name="recurrentWeight">recurrentWeight</param>
    /// <param name="steps">steps</param>
    /// <param name="hiddenSize">hiddenSize</param>
    /// <param name="options">options</param>
    [Description("@#lstm")]
    public extern MLOperand[] Lstm(MLOperand input, MLOperand weight, MLOperand recurrentWeight, uint steps, uint hiddenSize, MLLstmOptions? options = default);

    /// <summary>
    /// lstmCell
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="weight">weight</param>
    /// <param name="recurrentWeight">recurrentWeight</param>
    /// <param name="hiddenState">hiddenState</param>
    /// <param name="cellState">cellState</param>
    /// <param name="hiddenSize">hiddenSize</param>
    /// <param name="options">options</param>
    [Description("@#lstmCell")]
    public extern MLOperand[] LstmCell(MLOperand input, MLOperand weight, MLOperand recurrentWeight, MLOperand hiddenState, MLOperand cellState, uint hiddenSize, MLLstmCellOptions? options = default);

    /// <summary>
    /// matmul
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="options">options</param>
    [Description("@#matmul")]
    public extern MLOperand Matmul(MLOperand a, MLOperand b, MLOperatorOptions? options = default);

    /// <summary>
    /// pad
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="beginningPadding">beginningPadding</param>
    /// <param name="endingPadding">endingPadding</param>
    /// <param name="options">options</param>
    [Description("@#pad")]
    public extern MLOperand Pad(MLOperand input, uint[] beginningPadding, uint[] endingPadding, MLPadOptions? options = default);

    /// <summary>
    /// averagePool2d
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#averagePool2d")]
    public extern MLOperand AveragePool2d(MLOperand input, MLPool2dOptions? options = default);

    /// <summary>
    /// l2Pool2d
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#l2Pool2d")]
    public extern MLOperand L2Pool2d(MLOperand input, MLPool2dOptions? options = default);

    /// <summary>
    /// maxPool2d
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#maxPool2d")]
    public extern MLOperand MaxPool2d(MLOperand input, MLPool2dOptions? options = default);

    /// <summary>
    /// prelu
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="slope">slope</param>
    /// <param name="options">options</param>
    [Description("@#prelu")]
    public extern MLOperand Prelu(MLOperand input, MLOperand slope, MLOperatorOptions? options = default);

    /// <summary>
    /// reduceL1
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceL1")]
    public extern MLOperand ReduceL1(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceL2
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceL2")]
    public extern MLOperand ReduceL2(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceLogSum
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceLogSum")]
    public extern MLOperand ReduceLogSum(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceLogSumExp
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceLogSumExp")]
    public extern MLOperand ReduceLogSumExp(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceMax
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceMax")]
    public extern MLOperand ReduceMax(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceMean
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceMean")]
    public extern MLOperand ReduceMean(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceMin
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceMin")]
    public extern MLOperand ReduceMin(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceProduct
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceProduct")]
    public extern MLOperand ReduceProduct(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceSum
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceSum")]
    public extern MLOperand ReduceSum(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// reduceSumSquare
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reduceSumSquare")]
    public extern MLOperand ReduceSumSquare(MLOperand input, MLReduceOptions? options = default);

    /// <summary>
    /// relu
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#relu")]
    public extern MLOperand Relu(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// resample2d
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#resample2d")]
    public extern MLOperand Resample2d(MLOperand input, MLResample2dOptions? options = default);

    /// <summary>
    /// reshape
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="newShape">newShape</param>
    /// <param name="options">options</param>
    [Description("@#reshape")]
    public extern MLOperand Reshape(MLOperand input, uint[] newShape, MLOperatorOptions? options = default);

    /// <summary>
    /// reverse
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#reverse")]
    public extern MLOperand Reverse(MLOperand input, MLReverseOptions? options = default);

    /// <summary>
    /// scatterElements
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="indices">indices</param>
    /// <param name="updates">updates</param>
    /// <param name="options">options</param>
    [Description("@#scatterElements")]
    public extern MLOperand ScatterElements(MLOperand input, MLOperand indices, MLOperand updates, MLScatterOptions? options = default);

    /// <summary>
    /// scatterND
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="indices">indices</param>
    /// <param name="updates">updates</param>
    /// <param name="options">options</param>
    [Description("@#scatterND")]
    public extern MLOperand ScatterND(MLOperand input, MLOperand indices, MLOperand updates, MLOperatorOptions? options = default);

    /// <summary>
    /// sigmoid
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#sigmoid")]
    public extern MLOperand Sigmoid(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// slice
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="starts">starts</param>
    /// <param name="sizes">sizes</param>
    /// <param name="options">options</param>
    [Description("@#slice")]
    public extern MLOperand Slice(MLOperand input, uint[] starts, uint[] sizes, MLSliceOptions? options = default);

    /// <summary>
    /// softmax
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="axis">axis</param>
    /// <param name="options">options</param>
    [Description("@#softmax")]
    public extern MLOperand Softmax(MLOperand input, uint axis, MLOperatorOptions? options = default);

    /// <summary>
    /// softplus
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#softplus")]
    public extern MLOperand Softplus(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// softsign
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#softsign")]
    public extern MLOperand Softsign(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// split
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="splits">splits</param>
    /// <param name="options">options</param>
    [Description("@#split")]
    public extern MLOperand[] Split(MLOperand input, Either<uint, uint[]> splits, MLSplitOptions? options = default);

    /// <summary>
    /// tanh
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#tanh")]
    public extern MLOperand Tanh(MLOperand input, MLOperatorOptions? options = default);

    /// <summary>
    /// tile
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="repetitions">repetitions</param>
    /// <param name="options">options</param>
    [Description("@#tile")]
    public extern MLOperand Tile(MLOperand input, uint[] repetitions, MLOperatorOptions? options = default);

    /// <summary>
    /// transpose
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#transpose")]
    public extern MLOperand Transpose(MLOperand input, MLTransposeOptions? options = default);

    /// <summary>
    /// triangular
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#triangular")]
    public extern MLOperand Triangular(MLOperand input, MLTriangularOptions? options = default);

    /// <summary>
    /// where
    /// </summary>
    /// <param name="condition">condition</param>
    /// <param name="trueValue">trueValue</param>
    /// <param name="falseValue">falseValue</param>
    /// <param name="options">options</param>
    [Description("@#where")]
    public extern MLOperand Where(MLOperand condition, MLOperand trueValue, MLOperand falseValue, MLOperatorOptions? options = default);
}