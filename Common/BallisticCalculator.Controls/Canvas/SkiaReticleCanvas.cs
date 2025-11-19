using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Reticle.Draw;
using SkiaSharp;

namespace BallisticCalculator.Controls.Canvas;

/// <summary>
/// SkiaSharp implementation of IReticleCanvas
/// </summary>
public sealed class SkiaReticleCanvas : IReticleCanvas
{
    private readonly SKCanvas _canvas;
    private readonly SKRect _drawingArea;
    private readonly SKColor _backgroundColor;

    // Cache for frequently used paints and fonts
    private static readonly Dictionary<string, SKColor> ColorCache = new();
    private static readonly Dictionary<string, SKPaint> PaintCache = new();
    private static readonly Dictionary<string, SKFont> FontCache = new();

    public float Top => _drawingArea.Top;
    public float Left => _drawingArea.Left;
    public float Bottom => _drawingArea.Bottom;
    public float Right => _drawingArea.Right;
    public float Width => _drawingArea.Width;
    public float Height => _drawingArea.Height;

    static SkiaReticleCanvas()
    {
        // Initialize common color names
        InitializeColorCache();
    }

    public SkiaReticleCanvas(SKCanvas canvas, SKRect area, SKColor? backgroundColor = null)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        _drawingArea = area;
        _backgroundColor = backgroundColor ?? SKColors.White;
    }

    public void Clear()
    {
        _canvas.Clear(_backgroundColor);
    }

    public void Circle(float x, float y, float radius, float width, bool fill, string color)
    {
        var paint = GetPaint(color, width, fill);
        _canvas.DrawCircle(x, y, radius, paint);
    }

    public void Line(float x1, float y1, float x2, float y2, float width, string color)
    {
        var paint = GetPaint(color, width, false);
        _canvas.DrawLine(x1, y1, x2, y2, paint);
    }

    public void Rectangle(float x1, float y1, float x2, float y2, float width, bool fill, string color)
    {
        var rect = new SKRect(x1, y1, x2, y2);
        var paint = GetPaint(color, width, fill);
        _canvas.DrawRect(rect, paint);
    }

    public void Text(float x, float y, float height, string text, string color)
        => Text(x, y, height, text, color, TextAnchor.Left);

    public void Text(float x, float y, float height, string text, string color, TextAnchor anchor)
    {
        var font = GetFont(height);
        var paint = GetTextPaint(color);

        // Measure text to handle anchoring
        var bounds = new SKRect();
        paint.MeasureText(text, ref bounds);

        float adjustedX = anchor switch
        {
            TextAnchor.Center => x - bounds.Width / 2,
            TextAnchor.Right => x - bounds.Width,
            _ => x // TextAnchor.Left
        };

        // Adjust Y position - in Skia, text is drawn from baseline
        float adjustedY = y;

        _canvas.DrawText(text, adjustedX, adjustedY, font, paint);
    }

    public IReticleCanvasPath CreatePath()
    {
        return new SkiaReticlePath();
    }

    public void Path(IReticleCanvasPath path, float width, bool fill, string color)
    {
        if (path is not SkiaReticlePath skiaPath)
            throw new ArgumentException($"Path object is not created by {nameof(SkiaReticleCanvas)} class", nameof(path));

        var paint = GetPaint(color, width, fill);
        _canvas.DrawPath(skiaPath.Path, paint);
    }

    #region Color and Paint Management

    private static void InitializeColorCache()
    {
        var colorMap = new Dictionary<string, SKColor>
        {
            { "black", SKColors.Black },
            { "blue", SKColors.Blue },
            { "brown", SKColors.Brown },
            { "cyan", SKColors.Cyan },
            { "darkblue", SKColors.DarkBlue },
            { "darkcyan", SKColors.DarkCyan },
            { "darkgray", SKColors.DarkGray },
            { "darkgreen", SKColors.DarkGreen },
            { "darkmagenta", SKColors.DarkMagenta },
            { "darkorange", SKColors.DarkOrange },
            { "darkred", SKColors.DarkRed },
            { "darkviolet", SKColors.DarkViolet },
            { "gold", SKColors.Gold },
            { "goldenrod", SKColors.Goldenrod },
            { "grey", SKColors.Gray },
            { "gray", SKColors.Gray },
            { "green", SKColors.Green },
            { "greenyellow", SKColors.GreenYellow },
            { "lightgray", SKColors.LightGray },
            { "magenta", SKColors.Magenta },
            { "mediumblue", SKColors.MediumBlue },
            { "mediumpurple", SKColors.MediumPurple },
            { "orange", SKColors.Orange },
            { "pink", SKColors.Pink },
            { "purple", SKColors.Purple },
            { "red", SKColors.Red },
            { "teal", SKColors.Teal },
            { "violet", SKColors.Violet },
            { "white", SKColors.White },
        };

        foreach (var kvp in colorMap)
        {
            ColorCache[kvp.Key] = kvp.Value;
        }
    }

    private static SKColor TranslateColor(string? colorName)
    {
        if (string.IsNullOrEmpty(colorName))
            return SKColors.Black;

        var name = colorName.ToLowerInvariant();

        if (ColorCache.TryGetValue(name, out var color))
            return color;

        // Try to parse as hex color (#RRGGBB or #AARRGGBB)
        if (SKColor.TryParse(colorName, out var parsedColor))
        {
            ColorCache[name] = parsedColor;
            return parsedColor;
        }

        // Default to black
        ColorCache[name] = SKColors.Black;
        return SKColors.Black;
    }

    private static SKPaint GetPaint(string color, float width, bool fill)
    {
        int strokeWidth = width < 0.5f ? 1 : (int)Math.Round(width);
        string cacheKey = $"{color ?? "black"}_{strokeWidth}_{fill}";

        if (!PaintCache.TryGetValue(cacheKey, out var paint))
        {
            paint = new SKPaint
            {
                Color = TranslateColor(color),
                IsAntialias = true,
                Style = fill ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            // Limit cache size
            if (PaintCache.Count > 1000)
                PaintCache.Clear();

            PaintCache[cacheKey] = paint;
        }

        return paint;
    }

    private static SKPaint GetTextPaint(string color)
    {
        string cacheKey = $"text_{color ?? "black"}";

        if (!PaintCache.TryGetValue(cacheKey, out var paint))
        {
            paint = new SKPaint
            {
                Color = TranslateColor(color),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            // Limit cache size
            if (PaintCache.Count > 1000)
                PaintCache.Clear();

            PaintCache[cacheKey] = paint;
        }

        return paint;
    }

    private static SKFont GetFont(float height)
    {
        int size = height < 0.5f ? 1 : (int)Math.Round(height);
        string cacheKey = $"Verdana_{size}";

        if (!FontCache.TryGetValue(cacheKey, out var font))
        {
            var typeface = SKTypeface.FromFamilyName("Verdana", SKFontStyle.Normal);
            font = new SKFont(typeface, size);

            // Limit cache size
            if (FontCache.Count > 100)
                FontCache.Clear();

            FontCache[cacheKey] = font;
        }

        return font;
    }

    #endregion
}
