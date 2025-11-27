using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;

namespace ReticleEditor.Views.Dialogs;

public partial class EditCircleDialog : Window
{
    private readonly ReticleCircle _element;

    public EditCircleDialog(ReticleCircle circle)
    {
        _element = circle ?? throw new ArgumentNullException(nameof(circle));
        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        CenterX.UnitType = typeof(AngularUnit);
        CenterY.UnitType = typeof(AngularUnit);
        RadiusControl.UnitType = typeof(AngularUnit);
        LineWidthControl.UnitType = typeof(AngularUnit);

        PopulateFromElement();

        // Restore saved dialog size
        WindowStateManager.RestoreDialogSize(this, nameof(EditCircleDialog));

        // Save dialog size when closing
        Closing += (s, e) => WindowStateManager.SaveDialogSize(this, nameof(EditCircleDialog));
    }

    private void PopulateFromElement()
    {
        // Initialize center position if null
        if (_element.Center == null)
            _element.Center = new ReticlePosition(0, 0, AngularUnit.Mil);

        // Populate center point
        CenterX.SetValue(_element.Center.X);
        CenterY.SetValue(_element.Center.Y);

        // Populate radius
        RadiusControl.SetValue(_element.Radius);

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
        var centerX = CenterX.GetValue<AngularUnit>();
        var centerY = CenterY.GetValue<AngularUnit>();
        var radius = RadiusControl.GetValue<AngularUnit>();

        if (centerX.HasValue) _element.Center.X = centerX.Value;
        if (centerY.HasValue) _element.Center.Y = centerY.Value;
        if (radius.HasValue) _element.Radius = radius.Value;

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
