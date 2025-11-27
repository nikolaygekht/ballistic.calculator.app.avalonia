using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;

namespace ReticleEditor.Views.Dialogs;

public partial class EditRectangleDialog : Window
{
    private readonly ReticleRectangle _element;

    public EditRectangleDialog(ReticleRectangle rectangle)
    {
        _element = rectangle ?? throw new ArgumentNullException(nameof(rectangle));
        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        TopLeftX.UnitType = typeof(AngularUnit);
        TopLeftY.UnitType = typeof(AngularUnit);
        WidthControl.UnitType = typeof(AngularUnit);
        HeightControl.UnitType = typeof(AngularUnit);
        LineWidthControl.UnitType = typeof(AngularUnit);

        PopulateFromElement();

        // Restore saved dialog size
        WindowStateManager.RestoreDialogSize(this, nameof(EditRectangleDialog));

        // Save dialog size when closing
        Closing += (s, e) => WindowStateManager.SaveDialogSize(this, nameof(EditRectangleDialog));
    }

    private void PopulateFromElement()
    {
        // Initialize positions if null
        if (_element.TopLeft == null)
            _element.TopLeft = new ReticlePosition(0, 0, AngularUnit.Mil);
        if (_element.Size == null)
            _element.Size = new ReticlePosition(0, 0, AngularUnit.Mil);

        // Populate top-left corner
        TopLeftX.SetValue(_element.TopLeft.X);
        TopLeftY.SetValue(_element.TopLeft.Y);

        // Populate size
        WidthControl.SetValue(_element.Size.X);
        HeightControl.SetValue(_element.Size.Y);

        // Populate line width (optional)
        if (_element.LineWidth.HasValue)
            LineWidthControl.SetValue(_element.LineWidth.Value);
        else
            LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));

        // Populate color combo
        ColorCombo.PopulateWithColors();
        ColorCombo.SelectedItem = _element.Color ?? "black";

        // Populate fill checkbox
        FillCheckBox.IsChecked = _element.Fill == true;
    }

    public void Save()
    {
        var topLeftX = TopLeftX.GetValue<AngularUnit>();
        var topLeftY = TopLeftY.GetValue<AngularUnit>();
        var width = WidthControl.GetValue<AngularUnit>();
        var height = HeightControl.GetValue<AngularUnit>();

        if (topLeftX.HasValue) _element.TopLeft.X = topLeftX.Value;
        if (topLeftY.HasValue) _element.TopLeft.Y = topLeftY.Value;
        if (width.HasValue) _element.Size.X = width.Value;
        if (height.HasValue) _element.Size.Y = height.Value;

        // Handle optional LineWidth
        var lineWidth = LineWidthControl.GetValue<AngularUnit>();
        if (lineWidth.HasValue && lineWidth.Value.In(lineWidth.Value.Unit) > 0)
            _element.LineWidth = lineWidth.Value;
        else
            _element.LineWidth = null;

        _element.Color = ColorCombo.SelectedItem?.ToString();
        _element.Fill = FillCheckBox.IsChecked == true;
    }

    private void OnOK(object? sender, RoutedEventArgs e)
    {
        Save();
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
