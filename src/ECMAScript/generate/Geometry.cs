namespace ECMAScript;

/// <summary>
/// DOMPointInit
/// </summary>
[ECMAScript]
[Description("@#DOMPointInit")]
public record DOMPointInit(
    [property: Description("@#x")]double X = 0d,
	[property: Description("@#y")]double Y = 0d,
	[property: Description("@#z")]double Z = 0d,
	[property: Description("@#w")]double W = 1d);

/// <summary>
/// DOMRectInit
/// </summary>
[ECMAScript]
[Description("@#DOMRectInit")]
public record DOMRectInit(
    [property: Description("@#x")]double X = 0d,
	[property: Description("@#y")]double Y = 0d,
	[property: Description("@#width")]double Width = 0d,
	[property: Description("@#height")]double Height = 0d);

/// <summary>
/// DOMQuadInit
/// </summary>
[ECMAScript]
[Description("@#DOMQuadInit")]
public record DOMQuadInit(
    [property: Description("@#p1")]DOMPointInit? P1 = default,
	[property: Description("@#p2")]DOMPointInit? P2 = default,
	[property: Description("@#p3")]DOMPointInit? P3 = default,
	[property: Description("@#p4")]DOMPointInit? P4 = default);

/// <summary>
/// DOMMatrix2DInit
/// </summary>
[ECMAScript]
[Description("@#DOMMatrix2DInit")]
public record DOMMatrix2DInit(
    [property: Description("@#a")]double A = default,
	[property: Description("@#b")]double B = default,
	[property: Description("@#c")]double C = default,
	[property: Description("@#d")]double D = default,
	[property: Description("@#e")]double E = default,
	[property: Description("@#f")]double F = default,
	[property: Description("@#m11")]double M11 = default,
	[property: Description("@#m12")]double M12 = default,
	[property: Description("@#m21")]double M21 = default,
	[property: Description("@#m22")]double M22 = default,
	[property: Description("@#m41")]double M41 = default,
	[property: Description("@#m42")]double M42 = default);

/// <summary>
/// DOMMatrixInit
/// </summary>
[ECMAScript]
[Description("@#DOMMatrixInit")]
public record DOMMatrixInit(
    [property: Description("@#m13")]double M13 = 0d,
	[property: Description("@#m14")]double M14 = 0d,
	[property: Description("@#m23")]double M23 = 0d,
	[property: Description("@#m24")]double M24 = 0d,
	[property: Description("@#m31")]double M31 = 0d,
	[property: Description("@#m32")]double M32 = 0d,
	[property: Description("@#m33")]double M33 = 1d,
	[property: Description("@#m34")]double M34 = 0d,
	[property: Description("@#m43")]double M43 = 0d,
	[property: Description("@#m44")]double M44 = 1d,
	[property: Description("@#is2D")]bool Is2D = default): DOMMatrix2DInit;

/// <summary>
/// DOMPointReadOnly
/// </summary>
[ECMAScript]
[Description("@#DOMPointReadOnly")]
public class DOMPointReadOnly
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="w">w</param>
    public extern DOMPointReadOnly(double x, double y, double z, double w);

    /// <summary>
    /// fromPoint
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromPoint")]
    public extern static DOMPointReadOnly FromPoint(DOMPointInit? other = default);

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern double X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern double Y { get; }

    /// <summary>
    /// z
    /// </summary>
    [Description("@#z")]
    public extern double Z { get; }

    /// <summary>
    /// w
    /// </summary>
    [Description("@#w")]
    public extern double W { get; }

    /// <summary>
    /// matrixTransform
    /// </summary>
    /// <param name="matrix">matrix</param>
    [Description("@#matrixTransform")]
    public extern DOMPoint MatrixTransform(DOMMatrixInit? matrix = default);

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// DOMPoint
/// </summary>
[ECMAScript]
[Description("@#DOMPoint")]
public class DOMPoint : DOMPointReadOnly
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="w">w</param>
    public extern DOMPoint(double x, double y, double z, double w);

    /// <summary>
    /// fromPoint
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromPoint")]
    public extern static new DOMPoint FromPoint(DOMPointInit? other = default);
}

/// <summary>
/// DOMRectReadOnly
/// </summary>
[ECMAScript]
[Description("@#DOMRectReadOnly")]
public class DOMRectReadOnly
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    public extern DOMRectReadOnly(double x, double y, double width, double height);

    /// <summary>
    /// fromRect
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromRect")]
    public extern static DOMRectReadOnly FromRect(DOMRectInit? other = default);

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern double X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern double Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern double Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern double Height { get; }

    /// <summary>
    /// top
    /// </summary>
    [Description("@#top")]
    public extern double Top { get; }

    /// <summary>
    /// right
    /// </summary>
    [Description("@#right")]
    public extern double Right { get; }

    /// <summary>
    /// bottom
    /// </summary>
    [Description("@#bottom")]
    public extern double Bottom { get; }

    /// <summary>
    /// left
    /// </summary>
    [Description("@#left")]
    public extern double Left { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// DOMRect
/// </summary>
[ECMAScript]
[Description("@#DOMRect")]
public class DOMRect : DOMRectReadOnly
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    public extern DOMRect(double x, double y, double width, double height);

    /// <summary>
    /// fromRect
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromRect")]
    public extern static new DOMRect FromRect(DOMRectInit? other = default);
}

/// <summary>
/// DOMRectList
/// </summary>
[ECMAScript]
[Description("@#DOMRectList")]
public class DOMRectList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern DOMRect? GetItem(uint index);
}

/// <summary>
/// DOMQuad
/// </summary>
[ECMAScript]
[Description("@#DOMQuad")]
public class DOMQuad
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="p1">p1</param>
    /// <param name="p2">p2</param>
    /// <param name="p3">p3</param>
    /// <param name="p4">p4</param>
    public extern DOMQuad(DOMPointInit p1, DOMPointInit p2, DOMPointInit p3, DOMPointInit p4);

    /// <summary>
    /// fromRect
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromRect")]
    public extern static DOMQuad FromRect(DOMRectInit? other = default);

    /// <summary>
    /// fromQuad
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromQuad")]
    public extern static DOMQuad FromQuad(DOMQuadInit? other = default);

    /// <summary>
    /// p1
    /// </summary>
    [Description("@#p1")]
    public extern DOMPoint P1 { get; }

    /// <summary>
    /// p2
    /// </summary>
    [Description("@#p2")]
    public extern DOMPoint P2 { get; }

    /// <summary>
    /// p3
    /// </summary>
    [Description("@#p3")]
    public extern DOMPoint P3 { get; }

    /// <summary>
    /// p4
    /// </summary>
    [Description("@#p4")]
    public extern DOMPoint P4 { get; }

    /// <summary>
    /// getBounds
    /// </summary>
    [Description("@#getBounds")]
    public extern DOMRect GetBounds();

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// DOMMatrixReadOnly
/// </summary>
[ECMAScript]
[Description("@#DOMMatrixReadOnly")]
public class DOMMatrixReadOnly
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern DOMMatrixReadOnly(Either<string, double[]> init);

    /// <summary>
    /// fromMatrix
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromMatrix")]
    public extern static DOMMatrixReadOnly FromMatrix(DOMMatrixInit? other = default);

    /// <summary>
    /// fromFloat32Array
    /// </summary>
    /// <param name="array32">array32</param>
    [Description("@#fromFloat32Array")]
    public extern static DOMMatrixReadOnly FromFloat32Array(Float32Array array32);

    /// <summary>
    /// fromFloat64Array
    /// </summary>
    /// <param name="array64">array64</param>
    [Description("@#fromFloat64Array")]
    public extern static DOMMatrixReadOnly FromFloat64Array(Float64Array array64);

    /// <summary>
    /// a
    /// </summary>
    [Description("@#a")]
    public extern double A { get; }

    /// <summary>
    /// b
    /// </summary>
    [Description("@#b")]
    public extern double B { get; }

    /// <summary>
    /// c
    /// </summary>
    [Description("@#c")]
    public extern double C { get; }

    /// <summary>
    /// d
    /// </summary>
    [Description("@#d")]
    public extern double D { get; }

    /// <summary>
    /// e
    /// </summary>
    [Description("@#e")]
    public extern double E { get; }

    /// <summary>
    /// f
    /// </summary>
    [Description("@#f")]
    public extern double F { get; }

    /// <summary>
    /// m11
    /// </summary>
    [Description("@#m11")]
    public extern double M11 { get; }

    /// <summary>
    /// m12
    /// </summary>
    [Description("@#m12")]
    public extern double M12 { get; }

    /// <summary>
    /// m13
    /// </summary>
    [Description("@#m13")]
    public extern double M13 { get; }

    /// <summary>
    /// m14
    /// </summary>
    [Description("@#m14")]
    public extern double M14 { get; }

    /// <summary>
    /// m21
    /// </summary>
    [Description("@#m21")]
    public extern double M21 { get; }

    /// <summary>
    /// m22
    /// </summary>
    [Description("@#m22")]
    public extern double M22 { get; }

    /// <summary>
    /// m23
    /// </summary>
    [Description("@#m23")]
    public extern double M23 { get; }

    /// <summary>
    /// m24
    /// </summary>
    [Description("@#m24")]
    public extern double M24 { get; }

    /// <summary>
    /// m31
    /// </summary>
    [Description("@#m31")]
    public extern double M31 { get; }

    /// <summary>
    /// m32
    /// </summary>
    [Description("@#m32")]
    public extern double M32 { get; }

    /// <summary>
    /// m33
    /// </summary>
    [Description("@#m33")]
    public extern double M33 { get; }

    /// <summary>
    /// m34
    /// </summary>
    [Description("@#m34")]
    public extern double M34 { get; }

    /// <summary>
    /// m41
    /// </summary>
    [Description("@#m41")]
    public extern double M41 { get; }

    /// <summary>
    /// m42
    /// </summary>
    [Description("@#m42")]
    public extern double M42 { get; }

    /// <summary>
    /// m43
    /// </summary>
    [Description("@#m43")]
    public extern double M43 { get; }

    /// <summary>
    /// m44
    /// </summary>
    [Description("@#m44")]
    public extern double M44 { get; }

    /// <summary>
    /// is2D
    /// </summary>
    [Description("@#is2D")]
    public extern bool Is2D { get; }

    /// <summary>
    /// isIdentity
    /// </summary>
    [Description("@#isIdentity")]
    public extern bool IsIdentity { get; }

    /// <summary>
    /// translate
    /// </summary>
    /// <param name="tx">tx</param>
    /// <param name="ty">ty</param>
    /// <param name="tz">tz</param>
    [Description("@#translate")]
    public extern DOMMatrix Translate(double tx = 0d, double ty = 0d, double tz = 0d);

    /// <summary>
    /// scale
    /// </summary>
    /// <param name="scaleX">scaleX</param>
    /// <param name="scaleY">scaleY</param>
    /// <param name="scaleZ">scaleZ</param>
    /// <param name="originX">originX</param>
    /// <param name="originY">originY</param>
    /// <param name="originZ">originZ</param>
    [Description("@#scale")]
    public extern DOMMatrix Scale(double scaleX = 1d, double? scaleY = default, double scaleZ = 1d, double originX = 0d, double originY = 0d, double originZ = 0d);

    /// <summary>
    /// scaleNonUniform
    /// </summary>
    /// <param name="scaleX">scaleX</param>
    /// <param name="scaleY">scaleY</param>
    [Description("@#scaleNonUniform")]
    public extern DOMMatrix ScaleNonUniform(double scaleX = 1d, double scaleY = 1d);

    /// <summary>
    /// scale3d
    /// </summary>
    /// <param name="scale">scale</param>
    /// <param name="originX">originX</param>
    /// <param name="originY">originY</param>
    /// <param name="originZ">originZ</param>
    [Description("@#scale3d")]
    public extern DOMMatrix Scale3d(double scale = 1d, double originX = 0d, double originY = 0d, double originZ = 0d);

    /// <summary>
    /// rotate
    /// </summary>
    /// <param name="rotX">rotX</param>
    /// <param name="rotY">rotY</param>
    /// <param name="rotZ">rotZ</param>
    [Description("@#rotate")]
    public extern DOMMatrix Rotate(double rotX = 0d, double? rotY = default, double? rotZ = default);

    /// <summary>
    /// rotateFromVector
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#rotateFromVector")]
    public extern DOMMatrix RotateFromVector(double x = 0d, double y = 0d);

    /// <summary>
    /// rotateAxisAngle
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="angle">angle</param>
    [Description("@#rotateAxisAngle")]
    public extern DOMMatrix RotateAxisAngle(double x = 0d, double y = 0d, double z = 0d, double angle = 0d);

    /// <summary>
    /// skewX
    /// </summary>
    /// <param name="sx">sx</param>
    [Description("@#skewX")]
    public extern DOMMatrix SkewX(double sx = 0d);

    /// <summary>
    /// skewY
    /// </summary>
    /// <param name="sy">sy</param>
    [Description("@#skewY")]
    public extern DOMMatrix SkewY(double sy = 0d);

    /// <summary>
    /// multiply
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#multiply")]
    public extern DOMMatrix Multiply(DOMMatrixInit? other = default);

    /// <summary>
    /// flipX
    /// </summary>
    [Description("@#flipX")]
    public extern DOMMatrix FlipX();

    /// <summary>
    /// flipY
    /// </summary>
    [Description("@#flipY")]
    public extern DOMMatrix FlipY();

    /// <summary>
    /// inverse
    /// </summary>
    [Description("@#inverse")]
    public extern DOMMatrix Inverse();

    /// <summary>
    /// transformPoint
    /// </summary>
    /// <param name="point">point</param>
    [Description("@#transformPoint")]
    public extern DOMPoint TransformPoint(DOMPointInit? point = default);

    /// <summary>
    /// toFloat32Array
    /// </summary>
    [Description("@#toFloat32Array")]
    public extern Float32Array ToFloat32Array();

    /// <summary>
    /// toFloat64Array
    /// </summary>
    [Description("@#toFloat64Array")]
    public extern Float64Array ToFloat64Array();

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// DOMMatrix
/// </summary>
[ECMAScript]
[Description("@#DOMMatrix")]
public class DOMMatrix : DOMMatrixReadOnly
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern DOMMatrix(Either<string, double[]> init);

    /// <summary>
    /// fromMatrix
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#fromMatrix")]
    public extern static new DOMMatrix FromMatrix(DOMMatrixInit? other = default);

    /// <summary>
    /// fromFloat32Array
    /// </summary>
    /// <param name="array32">array32</param>
    [Description("@#fromFloat32Array")]
    public extern static new DOMMatrix FromFloat32Array(Float32Array array32);

    /// <summary>
    /// fromFloat64Array
    /// </summary>
    /// <param name="array64">array64</param>
    [Description("@#fromFloat64Array")]
    public extern static new DOMMatrix FromFloat64Array(Float64Array array64);

    /// <summary>
    /// multiplySelf
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#multiplySelf")]
    public extern DOMMatrix MultiplySelf(DOMMatrixInit? other = default);

    /// <summary>
    /// preMultiplySelf
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#preMultiplySelf")]
    public extern DOMMatrix PreMultiplySelf(DOMMatrixInit? other = default);

    /// <summary>
    /// translateSelf
    /// </summary>
    /// <param name="tx">tx</param>
    /// <param name="ty">ty</param>
    /// <param name="tz">tz</param>
    [Description("@#translateSelf")]
    public extern DOMMatrix TranslateSelf(double tx = 0d, double ty = 0d, double tz = 0d);

    /// <summary>
    /// scaleSelf
    /// </summary>
    /// <param name="scaleX">scaleX</param>
    /// <param name="scaleY">scaleY</param>
    /// <param name="scaleZ">scaleZ</param>
    /// <param name="originX">originX</param>
    /// <param name="originY">originY</param>
    /// <param name="originZ">originZ</param>
    [Description("@#scaleSelf")]
    public extern DOMMatrix ScaleSelf(double scaleX = 1d, double? scaleY = default, double scaleZ = 1d, double originX = 0d, double originY = 0d, double originZ = 0d);

    /// <summary>
    /// scale3dSelf
    /// </summary>
    /// <param name="scale">scale</param>
    /// <param name="originX">originX</param>
    /// <param name="originY">originY</param>
    /// <param name="originZ">originZ</param>
    [Description("@#scale3dSelf")]
    public extern DOMMatrix Scale3dSelf(double scale = 1d, double originX = 0d, double originY = 0d, double originZ = 0d);

    /// <summary>
    /// rotateSelf
    /// </summary>
    /// <param name="rotX">rotX</param>
    /// <param name="rotY">rotY</param>
    /// <param name="rotZ">rotZ</param>
    [Description("@#rotateSelf")]
    public extern DOMMatrix RotateSelf(double rotX = 0d, double? rotY = default, double? rotZ = default);

    /// <summary>
    /// rotateFromVectorSelf
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#rotateFromVectorSelf")]
    public extern DOMMatrix RotateFromVectorSelf(double x = 0d, double y = 0d);

    /// <summary>
    /// rotateAxisAngleSelf
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="angle">angle</param>
    [Description("@#rotateAxisAngleSelf")]
    public extern DOMMatrix RotateAxisAngleSelf(double x = 0d, double y = 0d, double z = 0d, double angle = 0d);

    /// <summary>
    /// skewXSelf
    /// </summary>
    /// <param name="sx">sx</param>
    [Description("@#skewXSelf")]
    public extern DOMMatrix SkewXSelf(double sx = 0d);

    /// <summary>
    /// skewYSelf
    /// </summary>
    /// <param name="sy">sy</param>
    [Description("@#skewYSelf")]
    public extern DOMMatrix SkewYSelf(double sy = 0d);

    /// <summary>
    /// invertSelf
    /// </summary>
    [Description("@#invertSelf")]
    public extern DOMMatrix InvertSelf();

    /// <summary>
    /// setMatrixValue
    /// </summary>
    /// <param name="transformList">transformList</param>
    [Description("@#setMatrixValue")]
    public extern DOMMatrix SetMatrixValue(string transformList);
}