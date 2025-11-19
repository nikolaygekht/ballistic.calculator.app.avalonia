using BallisticCalculator.Reticle.Draw;
using SkiaSharp;

namespace BallisticCalculator.Controls.Canvas;

/// <summary>
/// SkiaSharp implementation of IReticleCanvasPath
/// </summary>
internal sealed class SkiaReticlePath : IReticleCanvasPath
{
    private SKPath? _path = new();
    private SKPoint _currentPosition = new(0, 0);

    public SKPath Path => _path ?? throw new ObjectDisposedException(nameof(SkiaReticlePath));

    public void Dispose()
    {
        _path?.Dispose();
        _path = null;
    }

    public void MoveTo(float x, float y)
    {
        if (_path == null)
            throw new ObjectDisposedException(nameof(SkiaReticlePath));

        _currentPosition = new SKPoint(x, y);
        _path.MoveTo(_currentPosition);
    }

    public void LineTo(float x, float y)
    {
        if (_path == null)
            throw new ObjectDisposedException(nameof(SkiaReticlePath));

        var destination = new SKPoint(x, y);
        _path.LineTo(destination);
        _currentPosition = destination;
    }

    public void Arc(float r, float x, float y, bool largeArc, bool clockwiseDirection)
    {
        if (_path == null)
            throw new ObjectDisposedException(nameof(SkiaReticlePath));

        var destination = new SKPoint(x, y);

        // Skip if destination is too close to current position
        if (Math.Abs(destination.X - _currentPosition.X) < 0.5f &&
            Math.Abs(destination.Y - _currentPosition.Y) < 0.5f)
            return;

        // If radius is too small, just draw a line
        if (r < 0.5f)
        {
            _path.LineTo(destination);
        }
        else
        {
            // Calculate arc using SVG arc algorithm
            var calculator = new SvgArcCalculator(
                r,
                _currentPosition.X, _currentPosition.Y,
                x, y,
                largeArc,
                clockwiseDirection);

            // Create arc rectangle
            var rect = new SKRect(
                calculator.CX - calculator.CorrRx,
                calculator.CY - calculator.CorrRy,
                calculator.CX + calculator.CorrRx,
                calculator.CY + calculator.CorrRy);

            // Add arc to path
            _path.ArcTo(rect, calculator.AngleStart, calculator.AngleExtent, false);
        }

        _currentPosition = destination;
    }

    public void Close()
    {
        if (_path == null)
            throw new ObjectDisposedException(nameof(SkiaReticlePath));

        _path.Close();
    }
}
