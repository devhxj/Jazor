namespace ECMAScript;

/// <summary>
/// SVGFilterElement
/// </summary>
[ECMAScript]
[Description("@#SVGFilterElement")]
public class SVGFilterElement : SVGElement
{
    /// <summary>
    /// filterUnits
    /// </summary>
    [Description("@#filterUnits")]
    public extern SVGAnimatedEnumeration FilterUnits { get; }

    /// <summary>
    /// primitiveUnits
    /// </summary>
    [Description("@#primitiveUnits")]
    public extern SVGAnimatedEnumeration PrimitiveUnits { get; }

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGFEBlendElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEBlendElement")]
public class SVGFEBlendElement : SVGElement
{
    /// <summary>
    /// SVG_FEBLEND_MODE_UNKNOWN
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_UNKNOWN")]
    public const ushort SVG_FEBLEND_MODE_UNKNOWN = 0;

    /// <summary>
    /// SVG_FEBLEND_MODE_NORMAL
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_NORMAL")]
    public const ushort SVG_FEBLEND_MODE_NORMAL = 1;

    /// <summary>
    /// SVG_FEBLEND_MODE_MULTIPLY
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_MULTIPLY")]
    public const ushort SVG_FEBLEND_MODE_MULTIPLY = 2;

    /// <summary>
    /// SVG_FEBLEND_MODE_SCREEN
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_SCREEN")]
    public const ushort SVG_FEBLEND_MODE_SCREEN = 3;

    /// <summary>
    /// SVG_FEBLEND_MODE_DARKEN
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_DARKEN")]
    public const ushort SVG_FEBLEND_MODE_DARKEN = 4;

    /// <summary>
    /// SVG_FEBLEND_MODE_LIGHTEN
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_LIGHTEN")]
    public const ushort SVG_FEBLEND_MODE_LIGHTEN = 5;

    /// <summary>
    /// SVG_FEBLEND_MODE_OVERLAY
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_OVERLAY")]
    public const ushort SVG_FEBLEND_MODE_OVERLAY = 6;

    /// <summary>
    /// SVG_FEBLEND_MODE_COLOR_DODGE
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_COLOR_DODGE")]
    public const ushort SVG_FEBLEND_MODE_COLOR_DODGE = 7;

    /// <summary>
    /// SVG_FEBLEND_MODE_COLOR_BURN
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_COLOR_BURN")]
    public const ushort SVG_FEBLEND_MODE_COLOR_BURN = 8;

    /// <summary>
    /// SVG_FEBLEND_MODE_HARD_LIGHT
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_HARD_LIGHT")]
    public const ushort SVG_FEBLEND_MODE_HARD_LIGHT = 9;

    /// <summary>
    /// SVG_FEBLEND_MODE_SOFT_LIGHT
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_SOFT_LIGHT")]
    public const ushort SVG_FEBLEND_MODE_SOFT_LIGHT = 10;

    /// <summary>
    /// SVG_FEBLEND_MODE_DIFFERENCE
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_DIFFERENCE")]
    public const ushort SVG_FEBLEND_MODE_DIFFERENCE = 11;

    /// <summary>
    /// SVG_FEBLEND_MODE_EXCLUSION
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_EXCLUSION")]
    public const ushort SVG_FEBLEND_MODE_EXCLUSION = 12;

    /// <summary>
    /// SVG_FEBLEND_MODE_HUE
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_HUE")]
    public const ushort SVG_FEBLEND_MODE_HUE = 13;

    /// <summary>
    /// SVG_FEBLEND_MODE_SATURATION
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_SATURATION")]
    public const ushort SVG_FEBLEND_MODE_SATURATION = 14;

    /// <summary>
    /// SVG_FEBLEND_MODE_COLOR
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_COLOR")]
    public const ushort SVG_FEBLEND_MODE_COLOR = 15;

    /// <summary>
    /// SVG_FEBLEND_MODE_LUMINOSITY
    /// </summary>
    [Description("@#SVG_FEBLEND_MODE_LUMINOSITY")]
    public const ushort SVG_FEBLEND_MODE_LUMINOSITY = 16;

    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// in2
    /// </summary>
    [Description("@#in2")]
    public extern SVGAnimatedString In2 { get; }

    /// <summary>
    /// mode
    /// </summary>
    [Description("@#mode")]
    public extern SVGAnimatedEnumeration Mode { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEColorMatrixElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEColorMatrixElement")]
public class SVGFEColorMatrixElement : SVGElement
{
    /// <summary>
    /// SVG_FECOLORMATRIX_TYPE_UNKNOWN
    /// </summary>
    [Description("@#SVG_FECOLORMATRIX_TYPE_UNKNOWN")]
    public const ushort SVG_FECOLORMATRIX_TYPE_UNKNOWN = 0;

    /// <summary>
    /// SVG_FECOLORMATRIX_TYPE_MATRIX
    /// </summary>
    [Description("@#SVG_FECOLORMATRIX_TYPE_MATRIX")]
    public const ushort SVG_FECOLORMATRIX_TYPE_MATRIX = 1;

    /// <summary>
    /// SVG_FECOLORMATRIX_TYPE_SATURATE
    /// </summary>
    [Description("@#SVG_FECOLORMATRIX_TYPE_SATURATE")]
    public const ushort SVG_FECOLORMATRIX_TYPE_SATURATE = 2;

    /// <summary>
    /// SVG_FECOLORMATRIX_TYPE_HUEROTATE
    /// </summary>
    [Description("@#SVG_FECOLORMATRIX_TYPE_HUEROTATE")]
    public const ushort SVG_FECOLORMATRIX_TYPE_HUEROTATE = 3;

    /// <summary>
    /// SVG_FECOLORMATRIX_TYPE_LUMINANCETOALPHA
    /// </summary>
    [Description("@#SVG_FECOLORMATRIX_TYPE_LUMINANCETOALPHA")]
    public const ushort SVG_FECOLORMATRIX_TYPE_LUMINANCETOALPHA = 4;

    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern SVGAnimatedEnumeration Type { get; }

    /// <summary>
    /// values
    /// </summary>
    [Description("@#values")]
    public extern SVGAnimatedNumberList Values { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEComponentTransferElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEComponentTransferElement")]
public class SVGFEComponentTransferElement : SVGElement
{
    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGComponentTransferFunctionElement
/// </summary>
[ECMAScript]
[Description("@#SVGComponentTransferFunctionElement")]
public class SVGComponentTransferFunctionElement : SVGElement
{
    /// <summary>
    /// SVG_FECOMPONENTTRANSFER_TYPE_UNKNOWN
    /// </summary>
    [Description("@#SVG_FECOMPONENTTRANSFER_TYPE_UNKNOWN")]
    public const ushort SVG_FECOMPONENTTRANSFER_TYPE_UNKNOWN = 0;

    /// <summary>
    /// SVG_FECOMPONENTTRANSFER_TYPE_IDENTITY
    /// </summary>
    [Description("@#SVG_FECOMPONENTTRANSFER_TYPE_IDENTITY")]
    public const ushort SVG_FECOMPONENTTRANSFER_TYPE_IDENTITY = 1;

    /// <summary>
    /// SVG_FECOMPONENTTRANSFER_TYPE_TABLE
    /// </summary>
    [Description("@#SVG_FECOMPONENTTRANSFER_TYPE_TABLE")]
    public const ushort SVG_FECOMPONENTTRANSFER_TYPE_TABLE = 2;

    /// <summary>
    /// SVG_FECOMPONENTTRANSFER_TYPE_DISCRETE
    /// </summary>
    [Description("@#SVG_FECOMPONENTTRANSFER_TYPE_DISCRETE")]
    public const ushort SVG_FECOMPONENTTRANSFER_TYPE_DISCRETE = 3;

    /// <summary>
    /// SVG_FECOMPONENTTRANSFER_TYPE_LINEAR
    /// </summary>
    [Description("@#SVG_FECOMPONENTTRANSFER_TYPE_LINEAR")]
    public const ushort SVG_FECOMPONENTTRANSFER_TYPE_LINEAR = 4;

    /// <summary>
    /// SVG_FECOMPONENTTRANSFER_TYPE_GAMMA
    /// </summary>
    [Description("@#SVG_FECOMPONENTTRANSFER_TYPE_GAMMA")]
    public const ushort SVG_FECOMPONENTTRANSFER_TYPE_GAMMA = 5;

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern SVGAnimatedEnumeration Type { get; }

    /// <summary>
    /// tableValues
    /// </summary>
    [Description("@#tableValues")]
    public extern SVGAnimatedNumberList TableValues { get; }

    /// <summary>
    /// slope
    /// </summary>
    [Description("@#slope")]
    public extern SVGAnimatedNumber Slope { get; }

    /// <summary>
    /// intercept
    /// </summary>
    [Description("@#intercept")]
    public extern SVGAnimatedNumber Intercept { get; }

    /// <summary>
    /// amplitude
    /// </summary>
    [Description("@#amplitude")]
    public extern SVGAnimatedNumber Amplitude { get; }

    /// <summary>
    /// exponent
    /// </summary>
    [Description("@#exponent")]
    public extern SVGAnimatedNumber Exponent { get; }

    /// <summary>
    /// offset
    /// </summary>
    [Description("@#offset")]
    public extern SVGAnimatedNumber Offset { get; }
}

/// <summary>
/// SVGFEFuncRElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEFuncRElement")]
public class SVGFEFuncRElement : SVGComponentTransferFunctionElement
{

}

/// <summary>
/// SVGFEFuncGElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEFuncGElement")]
public class SVGFEFuncGElement : SVGComponentTransferFunctionElement
{

}

/// <summary>
/// SVGFEFuncBElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEFuncBElement")]
public class SVGFEFuncBElement : SVGComponentTransferFunctionElement
{

}

/// <summary>
/// SVGFEFuncAElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEFuncAElement")]
public class SVGFEFuncAElement : SVGComponentTransferFunctionElement
{

}

/// <summary>
/// SVGFECompositeElement
/// </summary>
[ECMAScript]
[Description("@#SVGFECompositeElement")]
public class SVGFECompositeElement : SVGElement
{
    /// <summary>
    /// SVG_FECOMPOSITE_OPERATOR_UNKNOWN
    /// </summary>
    [Description("@#SVG_FECOMPOSITE_OPERATOR_UNKNOWN")]
    public const ushort SVG_FECOMPOSITE_OPERATOR_UNKNOWN = 0;

    /// <summary>
    /// SVG_FECOMPOSITE_OPERATOR_OVER
    /// </summary>
    [Description("@#SVG_FECOMPOSITE_OPERATOR_OVER")]
    public const ushort SVG_FECOMPOSITE_OPERATOR_OVER = 1;

    /// <summary>
    /// SVG_FECOMPOSITE_OPERATOR_IN
    /// </summary>
    [Description("@#SVG_FECOMPOSITE_OPERATOR_IN")]
    public const ushort SVG_FECOMPOSITE_OPERATOR_IN = 2;

    /// <summary>
    /// SVG_FECOMPOSITE_OPERATOR_OUT
    /// </summary>
    [Description("@#SVG_FECOMPOSITE_OPERATOR_OUT")]
    public const ushort SVG_FECOMPOSITE_OPERATOR_OUT = 3;

    /// <summary>
    /// SVG_FECOMPOSITE_OPERATOR_ATOP
    /// </summary>
    [Description("@#SVG_FECOMPOSITE_OPERATOR_ATOP")]
    public const ushort SVG_FECOMPOSITE_OPERATOR_ATOP = 4;

    /// <summary>
    /// SVG_FECOMPOSITE_OPERATOR_XOR
    /// </summary>
    [Description("@#SVG_FECOMPOSITE_OPERATOR_XOR")]
    public const ushort SVG_FECOMPOSITE_OPERATOR_XOR = 5;

    /// <summary>
    /// SVG_FECOMPOSITE_OPERATOR_ARITHMETIC
    /// </summary>
    [Description("@#SVG_FECOMPOSITE_OPERATOR_ARITHMETIC")]
    public const ushort SVG_FECOMPOSITE_OPERATOR_ARITHMETIC = 6;

    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// in2
    /// </summary>
    [Description("@#in2")]
    public extern SVGAnimatedString In2 { get; }

    /// <summary>
    /// operator
    /// </summary>
    [Description("@#operator")]
    public extern SVGAnimatedEnumeration Operator { get; }

    /// <summary>
    /// k1
    /// </summary>
    [Description("@#k1")]
    public extern SVGAnimatedNumber K1 { get; }

    /// <summary>
    /// k2
    /// </summary>
    [Description("@#k2")]
    public extern SVGAnimatedNumber K2 { get; }

    /// <summary>
    /// k3
    /// </summary>
    [Description("@#k3")]
    public extern SVGAnimatedNumber K3 { get; }

    /// <summary>
    /// k4
    /// </summary>
    [Description("@#k4")]
    public extern SVGAnimatedNumber K4 { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEConvolveMatrixElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEConvolveMatrixElement")]
public class SVGFEConvolveMatrixElement : SVGElement
{
    /// <summary>
    /// SVG_EDGEMODE_UNKNOWN
    /// </summary>
    [Description("@#SVG_EDGEMODE_UNKNOWN")]
    public const ushort SVG_EDGEMODE_UNKNOWN = 0;

    /// <summary>
    /// SVG_EDGEMODE_DUPLICATE
    /// </summary>
    [Description("@#SVG_EDGEMODE_DUPLICATE")]
    public const ushort SVG_EDGEMODE_DUPLICATE = 1;

    /// <summary>
    /// SVG_EDGEMODE_WRAP
    /// </summary>
    [Description("@#SVG_EDGEMODE_WRAP")]
    public const ushort SVG_EDGEMODE_WRAP = 2;

    /// <summary>
    /// SVG_EDGEMODE_NONE
    /// </summary>
    [Description("@#SVG_EDGEMODE_NONE")]
    public const ushort SVG_EDGEMODE_NONE = 3;

    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// orderX
    /// </summary>
    [Description("@#orderX")]
    public extern SVGAnimatedInteger OrderX { get; }

    /// <summary>
    /// orderY
    /// </summary>
    [Description("@#orderY")]
    public extern SVGAnimatedInteger OrderY { get; }

    /// <summary>
    /// kernelMatrix
    /// </summary>
    [Description("@#kernelMatrix")]
    public extern SVGAnimatedNumberList KernelMatrix { get; }

    /// <summary>
    /// divisor
    /// </summary>
    [Description("@#divisor")]
    public extern SVGAnimatedNumber Divisor { get; }

    /// <summary>
    /// bias
    /// </summary>
    [Description("@#bias")]
    public extern SVGAnimatedNumber Bias { get; }

    /// <summary>
    /// targetX
    /// </summary>
    [Description("@#targetX")]
    public extern SVGAnimatedInteger TargetX { get; }

    /// <summary>
    /// targetY
    /// </summary>
    [Description("@#targetY")]
    public extern SVGAnimatedInteger TargetY { get; }

    /// <summary>
    /// edgeMode
    /// </summary>
    [Description("@#edgeMode")]
    public extern SVGAnimatedEnumeration EdgeMode { get; }

    /// <summary>
    /// kernelUnitLengthX
    /// </summary>
    [Description("@#kernelUnitLengthX")]
    public extern SVGAnimatedNumber KernelUnitLengthX { get; }

    /// <summary>
    /// kernelUnitLengthY
    /// </summary>
    [Description("@#kernelUnitLengthY")]
    public extern SVGAnimatedNumber KernelUnitLengthY { get; }

    /// <summary>
    /// preserveAlpha
    /// </summary>
    [Description("@#preserveAlpha")]
    public extern SVGAnimatedBoolean PreserveAlpha { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEDiffuseLightingElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEDiffuseLightingElement")]
public class SVGFEDiffuseLightingElement : SVGElement
{
    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// surfaceScale
    /// </summary>
    [Description("@#surfaceScale")]
    public extern SVGAnimatedNumber SurfaceScale { get; }

    /// <summary>
    /// diffuseConstant
    /// </summary>
    [Description("@#diffuseConstant")]
    public extern SVGAnimatedNumber DiffuseConstant { get; }

    /// <summary>
    /// kernelUnitLengthX
    /// </summary>
    [Description("@#kernelUnitLengthX")]
    public extern SVGAnimatedNumber KernelUnitLengthX { get; }

    /// <summary>
    /// kernelUnitLengthY
    /// </summary>
    [Description("@#kernelUnitLengthY")]
    public extern SVGAnimatedNumber KernelUnitLengthY { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEDistantLightElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEDistantLightElement")]
public class SVGFEDistantLightElement : SVGElement
{
    /// <summary>
    /// azimuth
    /// </summary>
    [Description("@#azimuth")]
    public extern SVGAnimatedNumber Azimuth { get; }

    /// <summary>
    /// elevation
    /// </summary>
    [Description("@#elevation")]
    public extern SVGAnimatedNumber Elevation { get; }
}

/// <summary>
/// SVGFEPointLightElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEPointLightElement")]
public class SVGFEPointLightElement : SVGElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedNumber X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedNumber Y { get; }

    /// <summary>
    /// z
    /// </summary>
    [Description("@#z")]
    public extern SVGAnimatedNumber Z { get; }
}

/// <summary>
/// SVGFESpotLightElement
/// </summary>
[ECMAScript]
[Description("@#SVGFESpotLightElement")]
public class SVGFESpotLightElement : SVGElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedNumber X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedNumber Y { get; }

    /// <summary>
    /// z
    /// </summary>
    [Description("@#z")]
    public extern SVGAnimatedNumber Z { get; }

    /// <summary>
    /// pointsAtX
    /// </summary>
    [Description("@#pointsAtX")]
    public extern SVGAnimatedNumber PointsAtX { get; }

    /// <summary>
    /// pointsAtY
    /// </summary>
    [Description("@#pointsAtY")]
    public extern SVGAnimatedNumber PointsAtY { get; }

    /// <summary>
    /// pointsAtZ
    /// </summary>
    [Description("@#pointsAtZ")]
    public extern SVGAnimatedNumber PointsAtZ { get; }

    /// <summary>
    /// specularExponent
    /// </summary>
    [Description("@#specularExponent")]
    public extern SVGAnimatedNumber SpecularExponent { get; }

    /// <summary>
    /// limitingConeAngle
    /// </summary>
    [Description("@#limitingConeAngle")]
    public extern SVGAnimatedNumber LimitingConeAngle { get; }
}

/// <summary>
/// SVGFEDisplacementMapElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEDisplacementMapElement")]
public class SVGFEDisplacementMapElement : SVGElement
{
    /// <summary>
    /// SVG_CHANNEL_UNKNOWN
    /// </summary>
    [Description("@#SVG_CHANNEL_UNKNOWN")]
    public const ushort SVG_CHANNEL_UNKNOWN = 0;

    /// <summary>
    /// SVG_CHANNEL_R
    /// </summary>
    [Description("@#SVG_CHANNEL_R")]
    public const ushort SVG_CHANNEL_R = 1;

    /// <summary>
    /// SVG_CHANNEL_G
    /// </summary>
    [Description("@#SVG_CHANNEL_G")]
    public const ushort SVG_CHANNEL_G = 2;

    /// <summary>
    /// SVG_CHANNEL_B
    /// </summary>
    [Description("@#SVG_CHANNEL_B")]
    public const ushort SVG_CHANNEL_B = 3;

    /// <summary>
    /// SVG_CHANNEL_A
    /// </summary>
    [Description("@#SVG_CHANNEL_A")]
    public const ushort SVG_CHANNEL_A = 4;

    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// in2
    /// </summary>
    [Description("@#in2")]
    public extern SVGAnimatedString In2 { get; }

    /// <summary>
    /// scale
    /// </summary>
    [Description("@#scale")]
    public extern SVGAnimatedNumber Scale { get; }

    /// <summary>
    /// xChannelSelector
    /// </summary>
    [Description("@#xChannelSelector")]
    public extern SVGAnimatedEnumeration XChannelSelector { get; }

    /// <summary>
    /// yChannelSelector
    /// </summary>
    [Description("@#yChannelSelector")]
    public extern SVGAnimatedEnumeration YChannelSelector { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEDropShadowElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEDropShadowElement")]
public class SVGFEDropShadowElement : SVGElement
{
    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// dx
    /// </summary>
    [Description("@#dx")]
    public extern SVGAnimatedNumber Dx { get; }

    /// <summary>
    /// dy
    /// </summary>
    [Description("@#dy")]
    public extern SVGAnimatedNumber Dy { get; }

    /// <summary>
    /// stdDeviationX
    /// </summary>
    [Description("@#stdDeviationX")]
    public extern SVGAnimatedNumber StdDeviationX { get; }

    /// <summary>
    /// stdDeviationY
    /// </summary>
    [Description("@#stdDeviationY")]
    public extern SVGAnimatedNumber StdDeviationY { get; }

    /// <summary>
    /// setStdDeviation
    /// </summary>
    /// <param name="stdDeviationX">stdDeviationX</param>
    /// <param name="stdDeviationY">stdDeviationY</param>
    [Description("@#setStdDeviation")]
    public extern void SetStdDeviation(float stdDeviationX, float stdDeviationY);

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEFloodElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEFloodElement")]
public class SVGFEFloodElement : SVGElement
{


    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEGaussianBlurElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEGaussianBlurElement")]
public class SVGFEGaussianBlurElement : SVGElement
{
    /// <summary>
    /// SVG_EDGEMODE_UNKNOWN
    /// </summary>
    [Description("@#SVG_EDGEMODE_UNKNOWN")]
    public const ushort SVG_EDGEMODE_UNKNOWN = 0;

    /// <summary>
    /// SVG_EDGEMODE_DUPLICATE
    /// </summary>
    [Description("@#SVG_EDGEMODE_DUPLICATE")]
    public const ushort SVG_EDGEMODE_DUPLICATE = 1;

    /// <summary>
    /// SVG_EDGEMODE_WRAP
    /// </summary>
    [Description("@#SVG_EDGEMODE_WRAP")]
    public const ushort SVG_EDGEMODE_WRAP = 2;

    /// <summary>
    /// SVG_EDGEMODE_NONE
    /// </summary>
    [Description("@#SVG_EDGEMODE_NONE")]
    public const ushort SVG_EDGEMODE_NONE = 3;

    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// stdDeviationX
    /// </summary>
    [Description("@#stdDeviationX")]
    public extern SVGAnimatedNumber StdDeviationX { get; }

    /// <summary>
    /// stdDeviationY
    /// </summary>
    [Description("@#stdDeviationY")]
    public extern SVGAnimatedNumber StdDeviationY { get; }

    /// <summary>
    /// edgeMode
    /// </summary>
    [Description("@#edgeMode")]
    public extern SVGAnimatedEnumeration EdgeMode { get; }

    /// <summary>
    /// setStdDeviation
    /// </summary>
    /// <param name="stdDeviationX">stdDeviationX</param>
    /// <param name="stdDeviationY">stdDeviationY</param>
    [Description("@#setStdDeviation")]
    public extern void SetStdDeviation(float stdDeviationX, float stdDeviationY);

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEImageElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEImageElement")]
public class SVGFEImageElement : SVGElement
{
    /// <summary>
    /// preserveAspectRatio
    /// </summary>
    [Description("@#preserveAspectRatio")]
    public extern SVGAnimatedPreserveAspectRatio PreserveAspectRatio { get; }

    /// <summary>
    /// crossOrigin
    /// </summary>
    [Description("@#crossOrigin")]
    public extern SVGAnimatedString CrossOrigin { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGFEMergeElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEMergeElement")]
public class SVGFEMergeElement : SVGElement
{


    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEMergeNodeElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEMergeNodeElement")]
public class SVGFEMergeNodeElement : SVGElement
{
    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }
}

/// <summary>
/// SVGFEMorphologyElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEMorphologyElement")]
public class SVGFEMorphologyElement : SVGElement
{
    /// <summary>
    /// SVG_MORPHOLOGY_OPERATOR_UNKNOWN
    /// </summary>
    [Description("@#SVG_MORPHOLOGY_OPERATOR_UNKNOWN")]
    public const ushort SVG_MORPHOLOGY_OPERATOR_UNKNOWN = 0;

    /// <summary>
    /// SVG_MORPHOLOGY_OPERATOR_ERODE
    /// </summary>
    [Description("@#SVG_MORPHOLOGY_OPERATOR_ERODE")]
    public const ushort SVG_MORPHOLOGY_OPERATOR_ERODE = 1;

    /// <summary>
    /// SVG_MORPHOLOGY_OPERATOR_DILATE
    /// </summary>
    [Description("@#SVG_MORPHOLOGY_OPERATOR_DILATE")]
    public const ushort SVG_MORPHOLOGY_OPERATOR_DILATE = 2;

    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// operator
    /// </summary>
    [Description("@#operator")]
    public extern SVGAnimatedEnumeration Operator { get; }

    /// <summary>
    /// radiusX
    /// </summary>
    [Description("@#radiusX")]
    public extern SVGAnimatedNumber RadiusX { get; }

    /// <summary>
    /// radiusY
    /// </summary>
    [Description("@#radiusY")]
    public extern SVGAnimatedNumber RadiusY { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFEOffsetElement
/// </summary>
[ECMAScript]
[Description("@#SVGFEOffsetElement")]
public class SVGFEOffsetElement : SVGElement
{
    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// dx
    /// </summary>
    [Description("@#dx")]
    public extern SVGAnimatedNumber Dx { get; }

    /// <summary>
    /// dy
    /// </summary>
    [Description("@#dy")]
    public extern SVGAnimatedNumber Dy { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFESpecularLightingElement
/// </summary>
[ECMAScript]
[Description("@#SVGFESpecularLightingElement")]
public class SVGFESpecularLightingElement : SVGElement
{
    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    /// <summary>
    /// surfaceScale
    /// </summary>
    [Description("@#surfaceScale")]
    public extern SVGAnimatedNumber SurfaceScale { get; }

    /// <summary>
    /// specularConstant
    /// </summary>
    [Description("@#specularConstant")]
    public extern SVGAnimatedNumber SpecularConstant { get; }

    /// <summary>
    /// specularExponent
    /// </summary>
    [Description("@#specularExponent")]
    public extern SVGAnimatedNumber SpecularExponent { get; }

    /// <summary>
    /// kernelUnitLengthX
    /// </summary>
    [Description("@#kernelUnitLengthX")]
    public extern SVGAnimatedNumber KernelUnitLengthX { get; }

    /// <summary>
    /// kernelUnitLengthY
    /// </summary>
    [Description("@#kernelUnitLengthY")]
    public extern SVGAnimatedNumber KernelUnitLengthY { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFETileElement
/// </summary>
[ECMAScript]
[Description("@#SVGFETileElement")]
public class SVGFETileElement : SVGElement
{
    /// <summary>
    /// in1
    /// </summary>
    [Description("@#in1")]
    public extern SVGAnimatedString In1 { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}

/// <summary>
/// SVGFETurbulenceElement
/// </summary>
[ECMAScript]
[Description("@#SVGFETurbulenceElement")]
public class SVGFETurbulenceElement : SVGElement
{
    /// <summary>
    /// SVG_TURBULENCE_TYPE_UNKNOWN
    /// </summary>
    [Description("@#SVG_TURBULENCE_TYPE_UNKNOWN")]
    public const ushort SVG_TURBULENCE_TYPE_UNKNOWN = 0;

    /// <summary>
    /// SVG_TURBULENCE_TYPE_FRACTALNOISE
    /// </summary>
    [Description("@#SVG_TURBULENCE_TYPE_FRACTALNOISE")]
    public const ushort SVG_TURBULENCE_TYPE_FRACTALNOISE = 1;

    /// <summary>
    /// SVG_TURBULENCE_TYPE_TURBULENCE
    /// </summary>
    [Description("@#SVG_TURBULENCE_TYPE_TURBULENCE")]
    public const ushort SVG_TURBULENCE_TYPE_TURBULENCE = 2;

    /// <summary>
    /// SVG_STITCHTYPE_UNKNOWN
    /// </summary>
    [Description("@#SVG_STITCHTYPE_UNKNOWN")]
    public const ushort SVG_STITCHTYPE_UNKNOWN = 0;

    /// <summary>
    /// SVG_STITCHTYPE_STITCH
    /// </summary>
    [Description("@#SVG_STITCHTYPE_STITCH")]
    public const ushort SVG_STITCHTYPE_STITCH = 1;

    /// <summary>
    /// SVG_STITCHTYPE_NOSTITCH
    /// </summary>
    [Description("@#SVG_STITCHTYPE_NOSTITCH")]
    public const ushort SVG_STITCHTYPE_NOSTITCH = 2;

    /// <summary>
    /// baseFrequencyX
    /// </summary>
    [Description("@#baseFrequencyX")]
    public extern SVGAnimatedNumber BaseFrequencyX { get; }

    /// <summary>
    /// baseFrequencyY
    /// </summary>
    [Description("@#baseFrequencyY")]
    public extern SVGAnimatedNumber BaseFrequencyY { get; }

    /// <summary>
    /// numOctaves
    /// </summary>
    [Description("@#numOctaves")]
    public extern SVGAnimatedInteger NumOctaves { get; }

    /// <summary>
    /// seed
    /// </summary>
    [Description("@#seed")]
    public extern SVGAnimatedNumber Seed { get; }

    /// <summary>
    /// stitchTiles
    /// </summary>
    [Description("@#stitchTiles")]
    public extern SVGAnimatedEnumeration StitchTiles { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern SVGAnimatedEnumeration Type { get; }

    #region mixin SVGFilterPrimitiveStandardAttributes
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern SVGAnimatedString Result { get; }
    #endregion
}