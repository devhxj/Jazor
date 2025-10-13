namespace ECMAScript.CSS;

/// <summary>
/// PaintRenderingContext2DSettings
/// </summary>
[ECMAScript]
[Description("@#PaintRenderingContext2DSettings")]
public record PaintRenderingContext2DSettings(
    [property: Description("@#alpha")]bool Alpha = true);

/// <summary>
/// PaintWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#PaintWorkletGlobalScope")]
public class PaintWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// registerPaint
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="paintCtor">paintCtor</param>
    [Description("@#registerPaint")]
    public extern void RegisterPaint(string name, Action paintCtor);

    /// <summary>
    /// devicePixelRatio
    /// </summary>
    [Description("@#devicePixelRatio")]
    public extern double DevicePixelRatio { get; }
}

/// <summary>
/// PaintRenderingContext2D
/// </summary>
[ECMAScript]
[Description("@#PaintRenderingContext2D")]
public class PaintRenderingContext2D
{


    #region mixin CanvasState
    /// <summary>
    /// save
    /// </summary>
    [Description("@#save")]
    public extern void Save();

    /// <summary>
    /// restore
    /// </summary>
    [Description("@#restore")]
    public extern void Restore();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// isContextLost
    /// </summary>
    [Description("@#isContextLost")]
    public extern bool IsContextLost();
    #endregion

    #region mixin CanvasTransform
    /// <summary>
    /// scale
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#scale")]
    public extern void Scale(double x, double y);

    /// <summary>
    /// rotate
    /// </summary>
    /// <param name="angle">angle</param>
    [Description("@#rotate")]
    public extern void Rotate(double angle);

    /// <summary>
    /// translate
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#translate")]
    public extern void Translate(double x, double y);

    /// <summary>
    /// transform
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="c">c</param>
    /// <param name="d">d</param>
    /// <param name="e">e</param>
    /// <param name="f">f</param>
    [Description("@#transform")]
    public extern void Transform(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// getTransform
    /// </summary>
    [Description("@#getTransform")]
    public extern DOMMatrix GetTransform();

    /// <summary>
    /// setTransform
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="c">c</param>
    /// <param name="d">d</param>
    /// <param name="e">e</param>
    /// <param name="f">f</param>
    [Description("@#setTransform")]
    public extern void SetTransform(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// setTransform
    /// </summary>
    /// <param name="transform">transform</param>
    [Description("@#setTransform")]
    public extern void SetTransform(DOMMatrix2DInit? transform = default);

    /// <summary>
    /// resetTransform
    /// </summary>
    [Description("@#resetTransform")]
    public extern void ResetTransform();
    #endregion

    #region mixin CanvasCompositing
    /// <summary>
    /// globalAlpha
    /// </summary>
    [Description("@#globalAlpha")]
    public extern double GlobalAlpha { get; set; }

    /// <summary>
    /// globalCompositeOperation
    /// </summary>
    [Description("@#globalCompositeOperation")]
    public extern string GlobalCompositeOperation { get; set; }
    #endregion

    #region mixin CanvasImageSmoothing
    /// <summary>
    /// imageSmoothingEnabled
    /// </summary>
    [Description("@#imageSmoothingEnabled")]
    public extern bool ImageSmoothingEnabled { get; set; }

    /// <summary>
    /// imageSmoothingQuality
    /// </summary>
    [Description("@#imageSmoothingQuality")]
    public extern ImageSmoothingQuality ImageSmoothingQuality { get; set; }
    #endregion

    #region mixin CanvasFillStrokeStyles
    /// <summary>
    /// strokeStyle
    /// </summary>
    [Description("@#strokeStyle")]
    public extern Either<string, CanvasGradient, CanvasPattern> StrokeStyle { get; set; }

    /// <summary>
    /// fillStyle
    /// </summary>
    [Description("@#fillStyle")]
    public extern Either<string, CanvasGradient, CanvasPattern> FillStyle { get; set; }

    /// <summary>
    /// createLinearGradient
    /// </summary>
    /// <param name="x0">x0</param>
    /// <param name="y0">y0</param>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    [Description("@#createLinearGradient")]
    public extern CanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1);

    /// <summary>
    /// createRadialGradient
    /// </summary>
    /// <param name="x0">x0</param>
    /// <param name="y0">y0</param>
    /// <param name="r0">r0</param>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    /// <param name="r1">r1</param>
    [Description("@#createRadialGradient")]
    public extern CanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1);

    /// <summary>
    /// createConicGradient
    /// </summary>
    /// <param name="startAngle">startAngle</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#createConicGradient")]
    public extern CanvasGradient CreateConicGradient(double startAngle, double x, double y);

    /// <summary>
    /// createPattern
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="repetition">repetition</param>
    [Description("@#createPattern")]
    public extern CanvasPattern? CreatePattern(CanvasImageSource image, string repetition);
    #endregion

    #region mixin CanvasShadowStyles
    /// <summary>
    /// shadowOffsetX
    /// </summary>
    [Description("@#shadowOffsetX")]
    public extern double ShadowOffsetX { get; set; }

    /// <summary>
    /// shadowOffsetY
    /// </summary>
    [Description("@#shadowOffsetY")]
    public extern double ShadowOffsetY { get; set; }

    /// <summary>
    /// shadowBlur
    /// </summary>
    [Description("@#shadowBlur")]
    public extern double ShadowBlur { get; set; }

    /// <summary>
    /// shadowColor
    /// </summary>
    [Description("@#shadowColor")]
    public extern string ShadowColor { get; set; }
    #endregion

    #region mixin CanvasRect
    /// <summary>
    /// clearRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#clearRect")]
    public extern void ClearRect(double x, double y, double w, double h);

    /// <summary>
    /// fillRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#fillRect")]
    public extern void FillRect(double x, double y, double w, double h);

    /// <summary>
    /// strokeRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#strokeRect")]
    public extern void StrokeRect(double x, double y, double w, double h);
    #endregion

    #region mixin CanvasDrawPath
    /// <summary>
    /// beginPath
    /// </summary>
    [Description("@#beginPath")]
    public extern void BeginPath();

    /// <summary>
    /// fill
    /// </summary>
    /// <param name="fillRule">fillRule</param>
    [Description("@#fill")]
    public extern void Fill(CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// fill
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#fill")]
    public extern void Fill(Path2D path, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// stroke
    /// </summary>
    [Description("@#stroke")]
    public extern void Stroke();

    /// <summary>
    /// stroke
    /// </summary>
    /// <param name="path">path</param>
    [Description("@#stroke")]
    public extern void Stroke(Path2D path);

    /// <summary>
    /// clip
    /// </summary>
    /// <param name="fillRule">fillRule</param>
    [Description("@#clip")]
    public extern void Clip(CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// clip
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#clip")]
    public extern void Clip(Path2D path, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInPath
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#isPointInPath")]
    public extern bool IsPointInPath(double x, double y, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInPath
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#isPointInPath")]
    public extern bool IsPointInPath(Path2D path, double x, double y, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInStroke
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#isPointInStroke")]
    public extern bool IsPointInStroke(double x, double y);

    /// <summary>
    /// isPointInStroke
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#isPointInStroke")]
    public extern bool IsPointInStroke(Path2D path, double x, double y);
    #endregion

    #region mixin CanvasDrawImage
    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double dx, double dy);

    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dw">dw</param>
    /// <param name="dh">dh</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double dx, double dy, double dw, double dh);

    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dw">dw</param>
    /// <param name="dh">dh</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh);
    #endregion

    #region mixin CanvasPathDrawingStyles
    /// <summary>
    /// lineWidth
    /// </summary>
    [Description("@#lineWidth")]
    public extern double LineWidth { get; set; }

    /// <summary>
    /// lineCap
    /// </summary>
    [Description("@#lineCap")]
    public extern CanvasLineCap LineCap { get; set; }

    /// <summary>
    /// lineJoin
    /// </summary>
    [Description("@#lineJoin")]
    public extern CanvasLineJoin LineJoin { get; set; }

    /// <summary>
    /// miterLimit
    /// </summary>
    [Description("@#miterLimit")]
    public extern double MiterLimit { get; set; }

    /// <summary>
    /// setLineDash
    /// </summary>
    /// <param name="segments">segments</param>
    [Description("@#setLineDash")]
    public extern void SetLineDash(double[] segments);

    /// <summary>
    /// getLineDash
    /// </summary>
    [Description("@#getLineDash")]
    public extern double[] GetLineDash();

    /// <summary>
    /// lineDashOffset
    /// </summary>
    [Description("@#lineDashOffset")]
    public extern double LineDashOffset { get; set; }
    #endregion

    #region mixin CanvasPath
    /// <summary>
    /// closePath
    /// </summary>
    [Description("@#closePath")]
    public extern void ClosePath();

    /// <summary>
    /// moveTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#moveTo")]
    public extern void MoveTo(double x, double y);

    /// <summary>
    /// lineTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#lineTo")]
    public extern void LineTo(double x, double y);

    /// <summary>
    /// quadraticCurveTo
    /// </summary>
    /// <param name="cpx">cpx</param>
    /// <param name="cpy">cpy</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#quadraticCurveTo")]
    public extern void QuadraticCurveTo(double cpx, double cpy, double x, double y);

    /// <summary>
    /// bezierCurveTo
    /// </summary>
    /// <param name="cp1x">cp1x</param>
    /// <param name="cp1y">cp1y</param>
    /// <param name="cp2x">cp2x</param>
    /// <param name="cp2y">cp2y</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#bezierCurveTo")]
    public extern void BezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);

    /// <summary>
    /// arcTo
    /// </summary>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    /// <param name="x2">x2</param>
    /// <param name="y2">y2</param>
    /// <param name="radius">radius</param>
    [Description("@#arcTo")]
    public extern void ArcTo(double x1, double y1, double x2, double y2, double radius);

    /// <summary>
    /// rect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#rect")]
    public extern void Rect(double x, double y, double w, double h);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit, Either<double, DOMPointInit>[]> radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, double radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, DOMPointInit? radii = default);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit>[] radii);

    /// <summary>
    /// arc
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radius">radius</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#arc")]
    public extern void Arc(double x, double y, double radius, double startAngle, double endAngle, bool counterclockwise = false);

    /// <summary>
    /// ellipse
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radiusX">radiusX</param>
    /// <param name="radiusY">radiusY</param>
    /// <param name="rotation">rotation</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#ellipse")]
    public extern void Ellipse(double x, double y, double radiusX, double radiusY, double rotation, double startAngle, double endAngle, bool counterclockwise = false);
    #endregion
}

/// <summary>
/// PaintSize
/// </summary>
[ECMAScript]
[Description("@#PaintSize")]
public class PaintSize
{
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
}