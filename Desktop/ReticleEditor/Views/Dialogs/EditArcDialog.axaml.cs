using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;

namespace ReticleEditor.Views.Dialogs;

public partial class EditArcDialog : Window
{
    private readonly ReticlePathElementArc _element;

    public EditArcDialog(ReticlePathElementArc arc)
    {
        _element = arc ?? throw new ArgumentNullException(nameof(arc));
        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        PositionX.UnitType = typeof(AngularUnit);
        PositionY.UnitType = typeof(AngularUnit);
        RadiusControl.UnitType = typeof(AngularUnit);

        PopulateFromElement();

        // Restore saved dialog size
        WindowStateManager.RestoreDialogSize(this, nameof(EditArcDialog));

        // Save dialog size when closing
        Closing += (s, e) => WindowStateManager.SaveDialogSize(this, nameof(EditArcDialog));
    }

    private void PopulateFromElement()
    {
        // Initialize position if null
        if (_element.Position == null)
            _element.Position = new ReticlePosition(0, 0, AngularUnit.Mil);

        // Populate position
        PositionX.SetValue(_element.Position.X);
        PositionY.SetValue(_element.Position.Y);

        // Populate radius
        RadiusControl.SetValue(_element.Radius);

        // Populate checkboxes
        ClockwiseCheckBox.IsChecked = _element.ClockwiseDirection;
        MajorArcCheckBox.IsChecked = _element.MajorArc;
    }

    public void Save()
    {
        var posX = PositionX.GetValue<AngularUnit>();
        var posY = PositionY.GetValue<AngularUnit>();
        var radius = RadiusControl.GetValue<AngularUnit>();

        if (posX.HasValue) _element.Position.X = posX.Value;
        if (posY.HasValue) _element.Position.Y = posY.Value;
        if (radius.HasValue) _element.Radius = radius.Value;

        _element.ClockwiseDirection = ClockwiseCheckBox.IsChecked == true;
        _element.MajorArc = MajorArcCheckBox.IsChecked == true;
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
