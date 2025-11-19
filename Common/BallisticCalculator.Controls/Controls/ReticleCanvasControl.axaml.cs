using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Reticle.Draw;
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

    public ReticleCanvasControl()
    {
        InitializeComponent();

        // Subscribe to property changes
        ReticleProperty.Changed.AddClassHandler<ReticleCanvasControl>(OnReticleChanged);
        BackgroundColorProperty.Changed.AddClassHandler<ReticleCanvasControl>(OnBackgroundColorChanged);
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
            BackgroundColor);

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

        public CustomDrawOp(Rect bounds, ReticleDefinition reticle, Color backgroundColor)
        {
            _bounds = bounds;
            _reticle = reticle;
            _backgroundColor = backgroundColor;
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

            // Create canvas wrapper
            var rect = new SKRect(0, 0, (float)_bounds.Width, (float)_bounds.Height);
            var reticleCanvas = new Canvas.SkiaReticleCanvas(canvas, rect, skBackgroundColor);

            // Clear and draw
            reticleCanvas.Clear();

            // Create draw controller and render reticle
            var drawController = new ReticleDrawController(_reticle, reticleCanvas);
            drawController.DrawReticle();
        }
    }
}
