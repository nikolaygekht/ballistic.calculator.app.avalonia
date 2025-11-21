using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Reticle.Draw;
using Gehtsoft.Measurements;
using SkiaSharp;

namespace BallisticCalculator.Controls.Controls;

/// <summary>
/// A control for rendering reticle graphics using SkiaSharp
/// </summary>
public partial class ReticleCanvasControl : UserControl
{
    public static readonly StyledProperty<ReticleDefinition?> ReticleProperty =
        AvaloniaProperty.Register<ReticleCanvasControl, ReticleDefinition?>(nameof(Reticle));

    public static readonly StyledProperty<Color> BackgroundColorProperty =
        AvaloniaProperty.Register<ReticleCanvasControl, Color>(nameof(BackgroundColor), Colors.White);

    /// <summary>
    /// Gets or sets the reticle definition to draw
    /// </summary>
    public ReticleDefinition? Reticle
    {
        get => GetValue(ReticleProperty);
        set => SetValue(ReticleProperty, value);
    }

    /// <summary>
    /// Gets or sets the background color for the canvas
    /// </summary>
    public Color BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the underlay elements collection to be drawn before the reticle (e.g., target)
    /// </summary>
    public ReticleElementsCollection? Underlay { get; set; }

    /// <summary>
    /// Gets or sets the overlay elements collection to be drawn after the reticle (e.g., center marks, BDC, trajectory)
    /// </summary>
    public ReticleElementsCollection? Overlay { get; set; }

    public ReticleCanvasControl()
    {
        InitializeComponent();

        // Subscribe to property changes
        ReticleProperty.Changed.AddClassHandler<ReticleCanvasControl>(OnReticleChanged);
        BackgroundColorProperty.Changed.AddClassHandler<ReticleCanvasControl>(OnBackgroundColorChanged);
    }

    /// <summary>
    /// Converts pixel coordinates to angular coordinates in the reticle's coordinate system.
    /// Returns null if the position is outside the reticle area or if reticle has no size.
    /// The angular coordinates are relative to the zero point (center), with positive Y going UP.
    /// Uses the same coordinate system as BallisticCalculator.Reticle.Draw.CoordinateTranslator.
    /// </summary>
    public ReticlePosition? PixelToAngular(Point pixelPosition)
    {
        if (Reticle?.Size == null ||
            Reticle.Size.X.In(Reticle.Size.X.Unit) == 0 ||
            Reticle.Size.Y.In(Reticle.Size.Y.Unit) == 0)
        {
            return null;
        }

        // Calculate the actual drawing area (same logic as in Render)
        var canvasBounds = Bounds;
        double reticleAspectRatio = Reticle.Size.X.In(Reticle.Size.X.Unit) /
                                     Reticle.Size.Y.In(Reticle.Size.Y.Unit);

        int imageWidth, imageHeight;
        if (canvasBounds.Height * reticleAspectRatio > canvasBounds.Width)
        {
            imageWidth = (int)canvasBounds.Width;
            imageHeight = (int)(canvasBounds.Width / reticleAspectRatio);
        }
        else
        {
            imageHeight = (int)canvasBounds.Height;
            imageWidth = (int)(canvasBounds.Height * reticleAspectRatio);
        }

        // Calculate offset (image is centered)
        double offsetX = (canvasBounds.Width - imageWidth) / 2;
        double offsetY = (canvasBounds.Height - imageHeight) / 2;

        // Adjust for offset
        double adjustedX = pixelPosition.X - offsetX;
        double adjustedY = pixelPosition.Y - offsetY;

        // Check if position is outside the actual reticle area
        if (adjustedX < 0 || adjustedX >= imageWidth || adjustedY < 0 || adjustedY >= imageHeight)
        {
            return null;
        }

        // Get zero offsets (default to 0 if not set)
        double zeroX = Reticle.Zero?.X.In(Reticle.Size.X.Unit) ?? 0;
        double zeroY = Reticle.Zero?.Y.In(Reticle.Size.Y.Unit) ?? 0;

        // Calculate scale factors (pixels per angular unit)
        double scaleX = imageWidth / Reticle.Size.X.In(Reticle.Size.X.Unit);
        double scaleY = imageHeight / Reticle.Size.Y.In(Reticle.Size.Y.Unit);

        // Inverse transform (reverse of CoordinateTranslator.Transform):
        // Forward: x = (sx + zeroX) * scaleX, y = (zeroY - sy) * scaleY
        // Inverse: sx = x/scaleX - zeroX,     sy = zeroY - y/scaleY
        double angularX = adjustedX / scaleX - zeroX;
        double angularY = zeroY - adjustedY / scaleY;

        return new ReticlePosition
        {
            X = new Measurement<AngularUnit>(angularX, Reticle.Size.X.Unit),
            Y = new Measurement<AngularUnit>(angularY, Reticle.Size.Y.Unit)
        };
    }

    /// <summary>
    /// Converts angular coordinates (relative to zero point) to pixel coordinates on the canvas.
    /// Returns null if reticle has no size.
    /// Uses the same coordinate system as BallisticCalculator.Reticle.Draw.CoordinateTranslator.
    /// </summary>
    public Point? AngularToPixel(ReticlePosition angularPosition)
    {
        if (Reticle?.Size == null ||
            Reticle.Size.X.In(Reticle.Size.X.Unit) == 0 ||
            Reticle.Size.Y.In(Reticle.Size.Y.Unit) == 0)
        {
            return null;
        }

        // Calculate the actual drawing area (same logic as in Render)
        var canvasBounds = Bounds;
        double reticleAspectRatio = Reticle.Size.X.In(Reticle.Size.X.Unit) /
                                     Reticle.Size.Y.In(Reticle.Size.Y.Unit);

        int imageWidth, imageHeight;
        if (canvasBounds.Height * reticleAspectRatio > canvasBounds.Width)
        {
            imageWidth = (int)canvasBounds.Width;
            imageHeight = (int)(canvasBounds.Width / reticleAspectRatio);
        }
        else
        {
            imageHeight = (int)canvasBounds.Height;
            imageWidth = (int)(canvasBounds.Height * reticleAspectRatio);
        }

        // Calculate offset (image is centered)
        double offsetX = (canvasBounds.Width - imageWidth) / 2;
        double offsetY = (canvasBounds.Height - imageHeight) / 2;

        // Get zero offsets (default to 0 if not set)
        double zeroX = Reticle.Zero?.X.In(Reticle.Size.X.Unit) ?? 0;
        double zeroY = Reticle.Zero?.Y.In(Reticle.Size.Y.Unit) ?? 0;

        // Calculate scale factors (pixels per angular unit)
        double scaleX = imageWidth / Reticle.Size.X.In(Reticle.Size.X.Unit);
        double scaleY = imageHeight / Reticle.Size.Y.In(Reticle.Size.Y.Unit);

        // Get angular values in reticle units
        double angularX = angularPosition.X.In(Reticle.Size.X.Unit);
        double angularY = angularPosition.Y.In(Reticle.Size.Y.Unit);

        // Transform using same logic as CoordinateTranslator:
        // x = (sx + zeroX) * scaleX, y = (zeroY - sy) * scaleY
        double pixelX = (angularX + zeroX) * scaleX + offsetX;
        double pixelY = (zeroY - angularY) * scaleY + offsetY;

        return new Point(pixelX, pixelY);
    }

    private static void OnReticleChanged(ReticleCanvasControl control, AvaloniaPropertyChangedEventArgs e)
    {
        control.InvalidateVisual();
    }

    private static void OnBackgroundColorChanged(ReticleCanvasControl control, AvaloniaPropertyChangedEventArgs e)
    {
        control.InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Reticle == null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        // Create the custom draw operation
        var drawOp = new CustomDrawOp(
            new Rect(0, 0, Bounds.Width, Bounds.Height),
            Reticle,
            BackgroundColor,
            Underlay,
            Overlay);

        context.Custom(drawOp);
    }

    /// <summary>
    /// Custom drawing operation that renders using SkiaSharp
    /// </summary>
    private class CustomDrawOp : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly ReticleDefinition _reticle;
        private readonly Color _backgroundColor;
        private readonly ReticleElementsCollection? _underlay;
        private readonly ReticleElementsCollection? _overlay;

        public CustomDrawOp(Rect bounds, ReticleDefinition reticle, Color backgroundColor,
            ReticleElementsCollection? underlay, ReticleElementsCollection? overlay)
        {
            _bounds = bounds;
            _reticle = reticle;
            _backgroundColor = backgroundColor;
            _underlay = underlay;
            _overlay = overlay;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public Rect Bounds => _bounds;

        public bool HitTest(Point p) => _bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is CustomDrawOp op && op._reticle == _reticle && op._bounds == _bounds;
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            // Convert Avalonia color to SKColor
            var skBackgroundColor = new SKColor(
                _backgroundColor.R,
                _backgroundColor.G,
                _backgroundColor.B,
                _backgroundColor.A);

            // Calculate proper image size to maintain aspect ratio
            int imageWidth, imageHeight;
            if (_reticle?.Size != null)
            {
                // Calculate image size maintaining reticle proportions
                double reticleAspectRatio = _reticle.Size.X.In(_reticle.Size.X.Unit) /
                                            _reticle.Size.Y.In(_reticle.Size.Y.Unit);

                if (_bounds.Height * reticleAspectRatio > _bounds.Width)
                {
                    // Width constrained
                    imageWidth = (int)_bounds.Width;
                    imageHeight = (int)(_bounds.Width / reticleAspectRatio);
                }
                else
                {
                    // Height constrained
                    imageHeight = (int)_bounds.Height;
                    imageWidth = (int)(_bounds.Height * reticleAspectRatio);
                }
            }
            else
            {
                // No size info, use full bounds
                imageWidth = (int)_bounds.Width;
                imageHeight = (int)_bounds.Height;
            }

            // Center the image in the available space
            float offsetX = (float)((_bounds.Width - imageWidth) / 2);
            float offsetY = (float)((_bounds.Height - imageHeight) / 2);

            // Clear entire background first
            canvas.Clear(skBackgroundColor);

            // Create canvas wrapper with calculated size
            var rect = new SKRect(0, 0, imageWidth, imageHeight);
            var reticleCanvas = new Canvas.SkiaReticleCanvas(canvas, rect, skBackgroundColor);

            // Save canvas state and translate to center position
            canvas.Save();
            canvas.Translate(offsetX, offsetY);

            // Clear and draw
            reticleCanvas.Clear();

            // Create draw controller
            var drawController = new ReticleDrawController(_reticle, reticleCanvas);

            // Draw underlay elements (e.g., target)
            if (_underlay != null)
            {
                foreach (var element in _underlay)
                    drawController.DrawElement(element);
            }

            // Draw reticle
            drawController.DrawReticle();

            // Draw overlay elements (e.g., center marks, BDC, trajectory)
            if (_overlay != null)
            {
                foreach (var element in _overlay)
                    drawController.DrawElement(element);
            }

            // Restore canvas state
            canvas.Restore();
        }
    }
}
