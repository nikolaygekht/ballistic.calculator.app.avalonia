using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using BallisticCalculator.Controls.Controllers;
using System;

namespace BallisticCalculator.Controls.Controls;

public partial class WindDirectionControl : UserControl
{
    private readonly WindDirectionController _controller;
    private double _direction = 0;

    // Drawing elements
    private Ellipse? _circle;
    private Line? _arrowLine;
    private Line? _arrowHead1;
    private Line? _arrowHead2;

    #region Styled Properties

    public static readonly StyledProperty<IBrush> StrokeBrushProperty =
        AvaloniaProperty.Register<WindDirectionControl, IBrush>(
            nameof(StrokeBrush), Brushes.Black);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<WindDirectionControl, double>(
            nameof(StrokeThickness), 2.0);

    #endregion

    #region Properties

    /// <summary>
    /// Wind direction in degrees (0-360).
    /// 0° = tailwind (from behind), 90° = from right, 180° = headwind, 270° = from left
    /// </summary>
    public double Direction
    {
        get => _direction;
        set
        {
            _direction = _controller.NormalizeAngle(value);
            UpdateVisual();
        }
    }

    public IBrush StrokeBrush
    {
        get => GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Constructor & Initialization

    public WindDirectionControl()
    {
        _controller = new WindDirectionController();
        InitializeComponent();
        CreateDrawingElements();
        WireEvents();
    }

    private void CreateDrawingElements()
    {
        if (DrawingCanvas == null) return;

        _circle = new Ellipse
        {
            Stroke = StrokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = Brushes.Transparent
        };

        _arrowLine = new Line
        {
            Stroke = StrokeBrush,
            StrokeThickness = StrokeThickness
        };

        _arrowHead1 = new Line
        {
            Stroke = StrokeBrush,
            StrokeThickness = StrokeThickness
        };

        _arrowHead2 = new Line
        {
            Stroke = StrokeBrush,
            StrokeThickness = StrokeThickness
        };

        DrawingCanvas.Children.Add(_circle);
        DrawingCanvas.Children.Add(_arrowLine);
        DrawingCanvas.Children.Add(_arrowHead1);
        DrawingCanvas.Children.Add(_arrowHead2);
    }

    private void WireEvents()
    {
        // Handle size changes
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == BoundsProperty)
            {
                UpdateVisual();
            }
            else if (e.Property == StrokeBrushProperty)
            {
                UpdateStrokeBrush();
            }
            else if (e.Property == StrokeThicknessProperty)
            {
                UpdateStrokeThickness();
            }
        };

        // Handle pointer events for click and drag
        if (DrawingCanvas != null)
        {
            DrawingCanvas.PointerPressed += OnPointerPressed;
            DrawingCanvas.PointerMoved += OnPointerMoved;
        }
    }

    #endregion

    #region Pointer Handling

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DrawingCanvas == null) return;

        var point = e.GetPosition(DrawingCanvas);
        UpdateDirectionFromPoint(point.X, point.Y);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DrawingCanvas == null) return;

        // Only update if left button is pressed (dragging)
        if (!e.GetCurrentPoint(DrawingCanvas).Properties.IsLeftButtonPressed)
            return;

        var point = e.GetPosition(DrawingCanvas);
        UpdateDirectionFromPoint(point.X, point.Y);
        e.Handled = true;
    }

    private void UpdateDirectionFromPoint(double x, double y)
    {
        var newDirection = _controller.DirectionFromClick(Bounds.Width, Bounds.Height, x, y);

        // Round to nearest degree for cleaner values
        newDirection = Math.Round(newDirection);

        if (Math.Abs(newDirection - _direction) > 0.5)
        {
            _direction = _controller.NormalizeAngle(newDirection);
            UpdateVisual();
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Visual Updates

    private void UpdateVisual()
    {
        if (DrawingCanvas == null || _circle == null ||
            _arrowLine == null || _arrowHead1 == null || _arrowHead2 == null)
            return;

        double width = Bounds.Width;
        double height = Bounds.Height;

        if (width <= 0 || height <= 0)
            return;

        var arrow = _controller.CalculateArrow(width, height, _direction);

        // Update circle
        double circleDiameter = arrow.Radius * 2;
        _circle.Width = circleDiameter;
        _circle.Height = circleDiameter;
        Avalonia.Controls.Canvas.SetLeft(_circle, arrow.CenterX - arrow.Radius);
        Avalonia.Controls.Canvas.SetTop(_circle, arrow.CenterY - arrow.Radius);

        // Update arrow line (from edge to center)
        _arrowLine.StartPoint = new Point(arrow.StartX, arrow.StartY);
        _arrowLine.EndPoint = new Point(arrow.EndX, arrow.EndY);

        // Update arrowhead lines (from center)
        _arrowHead1.StartPoint = new Point(arrow.EndX, arrow.EndY);
        _arrowHead1.EndPoint = new Point(arrow.Head1X, arrow.Head1Y);

        _arrowHead2.StartPoint = new Point(arrow.EndX, arrow.EndY);
        _arrowHead2.EndPoint = new Point(arrow.Head2X, arrow.Head2Y);
    }

    private void UpdateStrokeBrush()
    {
        if (_circle != null) _circle.Stroke = StrokeBrush;
        if (_arrowLine != null) _arrowLine.Stroke = StrokeBrush;
        if (_arrowHead1 != null) _arrowHead1.Stroke = StrokeBrush;
        if (_arrowHead2 != null) _arrowHead2.Stroke = StrokeBrush;
    }

    private void UpdateStrokeThickness()
    {
        if (_circle != null) _circle.StrokeThickness = StrokeThickness;
        if (_arrowLine != null) _arrowLine.StrokeThickness = StrokeThickness;
        if (_arrowHead1 != null) _arrowHead1.StrokeThickness = StrokeThickness;
        if (_arrowHead2 != null) _arrowHead2.StrokeThickness = StrokeThickness;
    }

    #endregion
}
