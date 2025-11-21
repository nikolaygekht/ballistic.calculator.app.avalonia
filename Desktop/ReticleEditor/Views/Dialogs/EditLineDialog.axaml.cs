using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;

namespace ReticleEditor.Views.Dialogs;

public partial class EditLineDialog : Window
{
    private readonly ReticleLine _element;

    public EditLineDialog(ReticleLine line)
    {
        _element = line ?? throw new ArgumentNullException(nameof(line));
        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        StartX.UnitType = typeof(AngularUnit);
        StartY.UnitType = typeof(AngularUnit);
        EndX.UnitType = typeof(AngularUnit);
        EndY.UnitType = typeof(AngularUnit);
        LineWidthControl.UnitType = typeof(AngularUnit);

        PopulateFromElement();

        // Restore saved dialog size
        Utilities.WindowStateManager.RestoreDialogSize(this, nameof(EditLineDialog));

        // Save dialog size when closing
        Closing += (s, e) => Utilities.WindowStateManager.SaveDialogSize(this, nameof(EditLineDialog));
    }

    private void PopulateFromElement()
    {
        // Initialize positions if null
        if (_element.Start == null)
            _element.Start = new ReticlePosition(0, 0, AngularUnit.Mil);
        if (_element.End == null)
            _element.End = new ReticlePosition(0, 0, AngularUnit.Mil);

        // Populate start point
        StartX.SetValue(_element.Start.X);
        StartY.SetValue(_element.Start.Y);

        // Populate end point
        EndX.SetValue(_element.End.X);
        EndY.SetValue(_element.End.Y);

        // Populate line width (optional)
        if (_element.LineWidth.HasValue)
            LineWidthControl.SetValue(_element.LineWidth.Value);
        else
            LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));

        // Populate color combo
        ColorCombo.PopulateWithColors();
        ColorCombo.SelectedItem = _element.Color ?? "black";
    }

    public void Save()
    {
        var startX = StartX.GetValue<AngularUnit>();
        var startY = StartY.GetValue<AngularUnit>();
        var endX = EndX.GetValue<AngularUnit>();
        var endY = EndY.GetValue<AngularUnit>();

        if (startX.HasValue) _element.Start.X = startX.Value;
        if (startY.HasValue) _element.Start.Y = startY.Value;
        if (endX.HasValue) _element.End.X = endX.Value;
        if (endY.HasValue) _element.End.Y = endY.Value;

        // Handle optional LineWidth
        var lineWidth = LineWidthControl.GetValue<AngularUnit>();
        if (lineWidth.HasValue && lineWidth.Value.In(lineWidth.Value.Unit) > 0)
            _element.LineWidth = lineWidth.Value;
        else
            _element.LineWidth = null;

        _element.Color = ColorCombo.SelectedItem?.ToString();
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
